using System.Diagnostics;
using System.Text.RegularExpressions;
using WindowsToolkit.Core.Interfaces;
using WindowsToolkit.Core.Models;

namespace WindowsToolkit.Core.Services.VideoEditor
{
    /// <summary>
    /// Service for cutting/trimming video files using FFmpeg
    /// </summary>
    public class VideoCutterService : IVideoCutterService
    {
        private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm", ".m4v"
        };

        private string? _ffmpegPath;

        public VideoCutterService()
        {
            _ffmpegPath = FindFFmpegPath();
        }

        /// <summary>
        /// Gets the default NVIDIA video folder path
        /// </summary>
        public string GetDefaultVideoFolderPath()
        {
            // NVIDIA ShadowPlay default location
            var videosFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            var nvidiaPath = Path.Combine(videosFolder, "NVIDIA");
            
            if (Directory.Exists(nvidiaPath))
                return nvidiaPath;

            // Alternative: Desktop Captures folder
            var desktopCaptures = Path.Combine(nvidiaPath, "Desktop");
            if (Directory.Exists(desktopCaptures))
                return desktopCaptures;

            // Fallback to Videos folder
            return videosFolder;
        }

        /// <summary>
        /// Scans a folder for video files
        /// </summary>
        public async Task<IEnumerable<VideoFile>> ScanVideoFolderAsync(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                return Enumerable.Empty<VideoFile>();

            return await Task.Run(async () =>
            {
                var videoFiles = new List<VideoFile>();
                
                try
                {
                    var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                        .Where(f => SupportedExtensions.Contains(Path.GetExtension(f)));

                    foreach (var filePath in files)
                    {
                        try
                        {
                            var fileInfo = new FileInfo(filePath);
                            var videoFile = new VideoFile
                            {
                                FilePath = filePath,
                                FileName = Path.GetFileName(filePath),
                                Extension = Path.GetExtension(filePath),
                                FileSize = fileInfo.Length,
                                CreatedDate = fileInfo.CreationTime,
                                ModifiedDate = fileInfo.LastWriteTime
                            };

                            // Try to get duration
                            var info = await GetVideoInfoAsync(filePath);
                            if (info != null)
                            {
                                videoFile.Duration = info.Duration;
                            }

                            videoFiles.Add(videoFile);
                        }
                        catch
                        {
                            // Skip files that can't be read
                        }
                    }
                }
                catch
                {
                    // Return empty list on error
                }

                return videoFiles.OrderByDescending(v => v.ModifiedDate).ToList();
            });
        }

        /// <summary>
        /// Gets video information using FFprobe
        /// </summary>
        public async Task<VideoInfo?> GetVideoInfoAsync(string videoPath)
        {
            if (!File.Exists(videoPath))
                return null;

            var ffprobePath = GetFFprobePath();
            if (ffprobePath == null)
                return null;

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = ffprobePath,
                    Arguments = $"-v quiet -print_format json -show_format -show_streams \"{videoPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = startInfo };
                process.Start();

                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                return ParseVideoInfo(videoPath, output);
            }
            catch
            {
                // Fallback: try to get basic info from file
                return GetBasicVideoInfo(videoPath);
            }
        }

        /// <summary>
        /// Cuts a video from start time to end time
        /// </summary>
        public async Task<VideoCutResult> CutVideoAsync(
            string inputPath,
            string outputPath,
            TimeSpan startTime,
            TimeSpan endTime,
            IProgress<int>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            if (_ffmpegPath == null)
                return VideoCutResult.Failed("FFmpeg not found. Please install FFmpeg and add it to PATH.");

            if (!File.Exists(inputPath))
                return VideoCutResult.Failed($"Input file not found: {inputPath}");

            if (startTime >= endTime)
                return VideoCutResult.Failed("Start time must be before end time.");

            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            try
            {
                var duration = endTime - startTime;
                var startTimeStr = FormatTimeSpan(startTime);
                var durationStr = FormatTimeSpan(duration);

                // FFmpeg command with stream copy for fast cutting
                // -ss before -i for fast seeking, -t for duration
                var arguments = $"-y -ss {startTimeStr} -i \"{inputPath}\" -t {durationStr} -c copy -avoid_negative_ts make_zero \"{outputPath}\"";

                var startInfo = new ProcessStartInfo
                {
                    FileName = _ffmpegPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = startInfo };
                
                process.Start();

                // Read stderr for progress (FFmpeg outputs progress to stderr)
                var progressTask = ReadFFmpegProgressAsync(process, duration, progress, cancellationToken);

                await process.WaitForExitAsync(cancellationToken);
                await progressTask;

                stopwatch.Stop();

                if (process.ExitCode != 0)
                {
                    var error = await process.StandardError.ReadToEndAsync();
                    return VideoCutResult.Failed($"FFmpeg error: {error}");
                }

                if (!File.Exists(outputPath))
                    return VideoCutResult.Failed("Output file was not created.");

                var outputFileInfo = new FileInfo(outputPath);
                progress?.Report(100);

                return VideoCutResult.Succeeded(
                    outputPath,
                    duration,
                    outputFileInfo.Length,
                    stopwatch.Elapsed);
            }
            catch (OperationCanceledException)
            {
                // Clean up partial file
                if (File.Exists(outputPath))
                    File.Delete(outputPath);
                return VideoCutResult.Failed("Operation cancelled.");
            }
            catch (Exception ex)
            {
                return VideoCutResult.Failed($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if FFmpeg is available
        /// </summary>
        public async Task<bool> IsFFmpegAvailableAsync()
        {
            if (_ffmpegPath == null)
            {
                _ffmpegPath = FindFFmpegPath();
            }

            if (_ffmpegPath == null)
                return false;

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = _ffmpegPath,
                    Arguments = "-version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = startInfo };
                process.Start();
                await process.WaitForExitAsync();
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the FFmpeg path
        /// </summary>
        public string? GetFFmpegPath() => _ffmpegPath;

        private string? GetFFprobePath()
        {
            if (_ffmpegPath == null)
                return null;

            var ffmpegDir = Path.GetDirectoryName(_ffmpegPath);
            if (ffmpegDir == null)
                return "ffprobe"; // Try PATH

            var ffprobePath = Path.Combine(ffmpegDir, "ffprobe.exe");
            return File.Exists(ffprobePath) ? ffprobePath : "ffprobe";
        }

        private static string? FindFFmpegPath()
        {
            // Check common locations
            var commonPaths = new[]
            {
                @"C:\ffmpeg\bin\ffmpeg.exe",
                @"C:\Program Files\ffmpeg\bin\ffmpeg.exe",
                @"C:\Program Files (x86)\ffmpeg\bin\ffmpeg.exe",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"ffmpeg\bin\ffmpeg.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"ffmpeg\bin\ffmpeg.exe"),
            };

            foreach (var path in commonPaths)
            {
                if (File.Exists(path))
                    return path;
            }

            // Check PATH environment variable
            var pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (pathEnv != null)
            {
                var paths = pathEnv.Split(';');
                foreach (var path in paths)
                {
                    var ffmpegPath = Path.Combine(path.Trim(), "ffmpeg.exe");
                    if (File.Exists(ffmpegPath))
                        return ffmpegPath;
                }
            }

            // Check if ffmpeg is directly accessible
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = "-version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = startInfo };
                process.Start();
                process.WaitForExit(5000);
                if (process.ExitCode == 0)
                    return "ffmpeg";
            }
            catch
            {
                // FFmpeg not found
            }

            return null;
        }

        private static string FormatTimeSpan(TimeSpan ts)
        {
            return ts.ToString(@"hh\:mm\:ss\.fff");
        }

        private static async Task ReadFFmpegProgressAsync(
            Process process,
            TimeSpan totalDuration,
            IProgress<int>? progress,
            CancellationToken cancellationToken)
        {
            if (progress == null)
                return;

            var timeRegex = new Regex(@"time=(\d{2}):(\d{2}):(\d{2})\.(\d{2})", RegexOptions.Compiled);

            try
            {
                using var reader = process.StandardError;
                var buffer = new char[4096];
                var lineBuilder = new System.Text.StringBuilder();

                while (!process.HasExited && !cancellationToken.IsCancellationRequested)
                {
                    var read = await reader.ReadAsync(buffer, 0, buffer.Length);
                    if (read == 0)
                        break;

                    lineBuilder.Append(buffer, 0, read);
                    var text = lineBuilder.ToString();

                    var match = timeRegex.Match(text);
                    if (match.Success)
                    {
                        var hours = int.Parse(match.Groups[1].Value);
                        var minutes = int.Parse(match.Groups[2].Value);
                        var seconds = int.Parse(match.Groups[3].Value);
                        var currentTime = new TimeSpan(hours, minutes, seconds);

                        var progressPercent = (int)((currentTime.TotalSeconds / totalDuration.TotalSeconds) * 100);
                        progressPercent = Math.Min(99, Math.Max(0, progressPercent));
                        progress.Report(progressPercent);
                    }

                    // Keep last part of buffer for partial lines
                    if (text.Length > 1000)
                    {
                        lineBuilder.Clear();
                        lineBuilder.Append(text.Substring(text.Length - 500));
                    }
                }
            }
            catch
            {
                // Ignore progress reading errors
            }
        }

        private static VideoInfo? ParseVideoInfo(string filePath, string jsonOutput)
        {
            try
            {
                // Simple JSON parsing without external dependencies
                var info = new VideoInfo { FilePath = filePath };
                
                // Parse duration
                var durationMatch = Regex.Match(jsonOutput, @"""duration""\s*:\s*""?([\d.]+)""?");
                if (durationMatch.Success && double.TryParse(durationMatch.Groups[1].Value, 
                    System.Globalization.NumberStyles.Any, 
                    System.Globalization.CultureInfo.InvariantCulture, 
                    out var durationSeconds))
                {
                    info.Duration = TimeSpan.FromSeconds(durationSeconds);
                }

                // Parse width
                var widthMatch = Regex.Match(jsonOutput, @"""width""\s*:\s*(\d+)");
                if (widthMatch.Success && int.TryParse(widthMatch.Groups[1].Value, out var width))
                {
                    info.Width = width;
                }

                // Parse height
                var heightMatch = Regex.Match(jsonOutput, @"""height""\s*:\s*(\d+)");
                if (heightMatch.Success && int.TryParse(heightMatch.Groups[1].Value, out var height))
                {
                    info.Height = height;
                }

                // Parse frame rate
                var fpsMatch = Regex.Match(jsonOutput, @"""r_frame_rate""\s*:\s*""(\d+)/(\d+)""");
                if (fpsMatch.Success && 
                    int.TryParse(fpsMatch.Groups[1].Value, out var fpsNum) &&
                    int.TryParse(fpsMatch.Groups[2].Value, out var fpsDen) &&
                    fpsDen > 0)
                {
                    info.FrameRate = (double)fpsNum / fpsDen;
                }

                // Parse bitrate
                var bitrateMatch = Regex.Match(jsonOutput, @"""bit_rate""\s*:\s*""?(\d+)""?");
                if (bitrateMatch.Success && long.TryParse(bitrateMatch.Groups[1].Value, out var bitrate))
                {
                    info.Bitrate = bitrate / 1000; // Convert to kbps
                }

                // Parse video codec
                var codecMatch = Regex.Match(jsonOutput, @"""codec_name""\s*:\s*""([^""]+)""");
                if (codecMatch.Success)
                {
                    info.VideoCodec = codecMatch.Groups[1].Value;
                }

                // Get file size
                if (File.Exists(filePath))
                {
                    info.FileSize = new FileInfo(filePath).Length;
                }

                return info;
            }
            catch
            {
                return null;
            }
        }

        private static VideoInfo GetBasicVideoInfo(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            return new VideoInfo
            {
                FilePath = filePath,
                FileSize = fileInfo.Length
            };
        }
    }
}
