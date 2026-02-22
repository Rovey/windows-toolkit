using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using WindowsToolkit.Core.Interfaces;
using WindowsToolkit.Core.Models;

namespace WindowsToolkit.Core.Services.MediaConverter
{
    public class MediaConverterService : IMediaConverterService
    {
        private string? _ffmpegPath;
        private string? _ffprobePath;

        public MediaConverterService()
        {
            _ffmpegPath = FindFFmpegPath();
            _ffprobePath = FindFFprobePath(_ffmpegPath);
        }

        public string? GetFFmpegPath() => _ffmpegPath;

        public async Task<bool> IsFFmpegAvailableAsync()
        {
            if (_ffmpegPath == null)
                _ffmpegPath = FindFFmpegPath();

            if (_ffmpegPath == null)
                return false;

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = _ffmpegPath,
                    Arguments = "-version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                using var process = new Process { StartInfo = psi };
                process.Start();
                await process.WaitForExitAsync();
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<string>> DetectGpuEncodersAsync(VideoCodec codec)
        {
            if (_ffmpegPath == null)
                return Enumerable.Empty<string>();

            var candidates = codec switch
            {
                VideoCodec.H265 => new[] { "hevc_nvenc", "hevc_qsv", "hevc_amf" },
                VideoCodec.H264 => new[] { "h264_nvenc", "h264_qsv", "h264_amf" },
                VideoCodec.AV1  => new[] { "av1_nvenc", "av1_qsv", "av1_amf" },
                _ => Array.Empty<string>()
            };

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = _ffmpegPath,
                    Arguments = "-hide_banner -encoders",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                using var process = new Process { StartInfo = psi };
                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                return candidates.Where(c => output.Contains(c)).ToList();
            }
            catch
            {
                return Enumerable.Empty<string>();
            }
        }

        public async Task<double?> GetDurationAsync(string filePath)
        {
            if (!File.Exists(filePath) || _ffprobePath == null)
                return null;

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = _ffprobePath,
                    Arguments = $"-v quiet -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{filePath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                using var process = new Process { StartInfo = psi };
                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (double.TryParse(output.Trim(),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var duration))
                    return duration;
            }
            catch { }

            return null;
        }

        public async Task<ConvertResult> ConvertAsync(
            string inputPath,
            string outputPath,
            ConversionSettings settings,
            IProgress<int>? progress = null,
            CancellationToken ct = default)
        {
            var stopwatch = Stopwatch.StartNew();

            if (_ffmpegPath == null)
                return Fail(inputPath, outputPath, "FFmpeg niet gevonden. Installeer FFmpeg en voeg het toe aan PATH.");

            if (!File.Exists(inputPath))
                return Fail(inputPath, outputPath, $"Invoerbestand niet gevonden: {inputPath}");

            var inputDuration = await GetDurationAsync(inputPath);

            // Detect GPU encoders if preferred
            IEnumerable<string> gpuEncoders = Enumerable.Empty<string>();
            if (settings.PreferGpu)
                gpuEncoders = await DetectGpuEncodersAsync(settings.Codec);

            var gpuEncoder = gpuEncoders.FirstOrDefault();

            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            ConvertResult result;

            if (gpuEncoder != null)
            {
                result = await RunConversionAsync(inputPath, outputPath, settings, gpuEncoder, inputDuration, progress, ct);

                // Duration mismatch or GPU failure → CPU fallback
                if (!result.Success || (settings.ValidateDuration && result.DurationMismatch))
                {
                    if (File.Exists(outputPath))
                        File.Delete(outputPath);
                    result = await RunConversionAsync(inputPath, outputPath, settings, null, inputDuration, progress, ct);
                }
            }
            else
            {
                result = await RunConversionAsync(inputPath, outputPath, settings, null, inputDuration, progress, ct);
            }

            stopwatch.Stop();
            result.ProcessingTime = stopwatch.Elapsed;
            result.InputPath = inputPath;
            result.OutputPath = outputPath;
            return result;
        }

        private async Task<ConvertResult> RunConversionAsync(
            string inputPath,
            string outputPath,
            ConversionSettings settings,
            string? gpuEncoder,
            double? inputDuration,
            IProgress<int>? progress,
            CancellationToken ct)
        {
            var args = BuildFFmpegArgs(inputPath, outputPath, settings, gpuEncoder);

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = _ffmpegPath!,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = psi };
                process.Start();

                var totalDuration = inputDuration.HasValue
                    ? TimeSpan.FromSeconds(inputDuration.Value)
                    : TimeSpan.Zero;

                var progressTask = ReadFFmpegProgressAsync(process, totalDuration, progress, ct);
                await process.WaitForExitAsync(ct);
                await progressTask;

                if (process.ExitCode != 0)
                {
                    var err = await process.StandardError.ReadToEndAsync();
                    var snippet = err.Length > 300 ? err.Substring(err.Length - 300) : err;
                    return new ConvertResult { Success = false, ErrorMessage = $"FFmpeg fout (exit {process.ExitCode}): {snippet}" };
                }

                if (!File.Exists(outputPath))
                    return new ConvertResult { Success = false, ErrorMessage = "Uitvoerbestand niet aangemaakt." };

                progress?.Report(100);

                // Validate duration
                double outputDuration = 0;
                bool durationMismatch = false;
                if (settings.ValidateDuration && inputDuration.HasValue)
                {
                    var outDur = await GetDurationAsync(outputPath);
                    outputDuration = outDur ?? 0;
                    durationMismatch = Math.Abs(outputDuration - inputDuration.Value) > 1.0;
                }

                return new ConvertResult
                {
                    Success = true,
                    InputDurationSec = inputDuration ?? 0,
                    OutputDurationSec = outputDuration,
                    DurationMismatch = durationMismatch
                };
            }
            catch (OperationCanceledException)
            {
                if (File.Exists(outputPath)) File.Delete(outputPath);
                return new ConvertResult { Success = false, ErrorMessage = "Conversie geannuleerd." };
            }
            catch (Exception ex)
            {
                return new ConvertResult { Success = false, ErrorMessage = $"Fout: {ex.Message}" };
            }
        }

        private static string BuildFFmpegArgs(string inputPath, string outputPath, ConversionSettings settings, string? gpuEncoder)
        {
            var baseArgs = $"-y -i \"{inputPath}\" -map 0 -map_metadata 0 -map_chapters 0 -c:a copy -c:s copy";
            var videoArgs = gpuEncoder != null
                ? BuildGpuArgs(gpuEncoder, settings)
                : BuildCpuArgs(settings);
            return $"{baseArgs} {videoArgs} \"{outputPath}\"";
        }

        private static string BuildGpuArgs(string encoder, ConversionSettings settings)
        {
            return encoder switch
            {
                "hevc_nvenc" => $"-c:v hevc_nvenc -preset p6 -rc vbr -cq {settings.Crf} -b:v 0",
                "hevc_qsv"   => $"-c:v hevc_qsv -global_quality {settings.Crf}",
                "hevc_amf"   => $"-c:v hevc_amf -rc cqp -qp_i {settings.Crf} -qp_p {settings.Crf} -qp_b {settings.Crf}",
                "h264_nvenc" => $"-c:v h264_nvenc -preset p6 -rc vbr -cq {settings.Crf} -b:v 0",
                "h264_qsv"   => $"-c:v h264_qsv -global_quality {settings.Crf}",
                "h264_amf"   => $"-c:v h264_amf -rc cqp -qp_i {settings.Crf} -qp_p {settings.Crf} -qp_b {settings.Crf}",
                "av1_nvenc"  => $"-c:v av1_nvenc -preset p6 -rc vbr -cq {settings.Crf} -b:v 0",
                "av1_qsv"    => $"-c:v av1_qsv -global_quality {settings.Crf}",
                "av1_amf"    => $"-c:v av1_amf -rc cqp -qp_i {settings.Crf} -qp_p {settings.Crf} -qp_b {settings.Crf}",
                _ => BuildCpuArgs(new ConversionSettings { Codec = settings.Codec, Crf = settings.Crf })
            };
        }

        private static string BuildCpuArgs(ConversionSettings settings)
        {
            return settings.Codec switch
            {
                VideoCodec.H265 => $"-c:v libx265 -preset veryfast -crf {settings.Crf}",
                VideoCodec.H264 => $"-c:v libx264 -preset veryfast -crf {settings.Crf}",
                VideoCodec.AV1  => $"-c:v libsvtav1 -preset 8 -crf {settings.Crf}",
                _ => $"-c:v libx265 -preset veryfast -crf {settings.Crf}"
            };
        }

        private static async Task ReadFFmpegProgressAsync(
            Process process,
            TimeSpan totalDuration,
            IProgress<int>? progress,
            CancellationToken ct)
        {
            if (progress == null) return;

            var timeRegex = new Regex(@"time=(\d{2}):(\d{2}):(\d{2})\.(\d{2})", RegexOptions.Compiled);

            try
            {
                using var reader = process.StandardError;
                var buffer = new char[4096];
                var sb = new StringBuilder();

                while (!process.HasExited && !ct.IsCancellationRequested)
                {
                    var read = await reader.ReadAsync(buffer, 0, buffer.Length);
                    if (read == 0) break;

                    sb.Append(buffer, 0, read);
                    var text = sb.ToString();
                    var match = timeRegex.Match(text);
                    if (match.Success)
                    {
                        var h = int.Parse(match.Groups[1].Value);
                        var m = int.Parse(match.Groups[2].Value);
                        var s = int.Parse(match.Groups[3].Value);
                        var current = new TimeSpan(h, m, s);
                        var pct = totalDuration.TotalSeconds > 0
                            ? (int)(current.TotalSeconds / totalDuration.TotalSeconds * 100)
                            : 0;
                        progress.Report(Math.Min(99, Math.Max(0, pct)));
                    }

                    if (text.Length > 1000)
                    {
                        sb.Clear();
                        sb.Append(text.Substring(text.Length - 500));
                    }
                }
            }
            catch { }
        }

        private static string? FindFFmpegPath()
        {
            var commonPaths = new[]
            {
                @"C:\ffmpeg\bin\ffmpeg.exe",
                @"C:\Program Files\ffmpeg\bin\ffmpeg.exe",
                @"C:\Program Files (x86)\ffmpeg\bin\ffmpeg.exe",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"ffmpeg\bin\ffmpeg.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"ffmpeg\bin\ffmpeg.exe"),
            };

            foreach (var p in commonPaths)
                if (File.Exists(p)) return p;

            var pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (pathEnv != null)
            {
                foreach (var dir in pathEnv.Split(';'))
                {
                    var candidate = Path.Combine(dir.Trim(), "ffmpeg.exe");
                    if (File.Exists(candidate)) return candidate;
                }
            }

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = "-version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                using var p = new Process { StartInfo = psi };
                p.Start();
                p.WaitForExit(5000);
                if (p.ExitCode == 0) return "ffmpeg";
            }
            catch { }

            return null;
        }

        private static string? FindFFprobePath(string? ffmpegPath)
        {
            if (ffmpegPath == null) return null;

            var dir = Path.GetDirectoryName(ffmpegPath);
            if (dir != null)
            {
                var candidate = Path.Combine(dir, "ffprobe.exe");
                if (File.Exists(candidate)) return candidate;
            }
            return "ffprobe";
        }

        private static ConvertResult Fail(string input, string output, string message) =>
            new ConvertResult { Success = false, InputPath = input, OutputPath = output, ErrorMessage = message };
    }
}
