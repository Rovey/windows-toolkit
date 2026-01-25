using WindowsToolkit.Core.Models;

namespace WindowsToolkit.Core.Interfaces
{
    /// <summary>
    /// Service for cutting/trimming video files
    /// </summary>
    public interface IVideoCutterService
    {
        /// <summary>
        /// Gets the default NVIDIA video folder path
        /// </summary>
        string GetDefaultVideoFolderPath();

        /// <summary>
        /// Scans a folder for video files
        /// </summary>
        Task<IEnumerable<VideoFile>> ScanVideoFolderAsync(string folderPath);

        /// <summary>
        /// Gets video information (duration, resolution, etc.)
        /// </summary>
        Task<VideoInfo?> GetVideoInfoAsync(string videoPath);

        /// <summary>
        /// Cuts a video from start time to end time
        /// </summary>
        Task<VideoCutResult> CutVideoAsync(
            string inputPath,
            string outputPath,
            TimeSpan startTime,
            TimeSpan endTime,
            IProgress<int>? progress = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if FFmpeg is available
        /// </summary>
        Task<bool> IsFFmpegAvailableAsync();

        /// <summary>
        /// Gets the FFmpeg path
        /// </summary>
        string? GetFFmpegPath();
    }
}
