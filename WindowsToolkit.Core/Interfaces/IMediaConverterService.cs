using WindowsToolkit.Core.Models;

namespace WindowsToolkit.Core.Interfaces
{
    public interface IMediaConverterService
    {
        Task<bool> IsFFmpegAvailableAsync();
        Task<IEnumerable<string>> DetectGpuEncodersAsync(VideoCodec codec);
        Task<double?> GetDurationAsync(string filePath);
        Task<ConvertResult> ConvertAsync(
            string inputPath,
            string outputPath,
            ConversionSettings settings,
            IProgress<int>? progress = null,
            CancellationToken ct = default);
        string? GetFFmpegPath();
    }
}
