namespace WindowsToolkit.Core.Models
{
    /// <summary>
    /// Contains detailed information about a video file
    /// </summary>
    public class VideoInfo
    {
        /// <summary>
        /// Full path to the video file
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Video duration
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Video width in pixels
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Video height in pixels
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Resolution as string (e.g., "1920x1080")
        /// </summary>
        public string Resolution => $"{Width}x{Height}";

        /// <summary>
        /// Frame rate (FPS)
        /// </summary>
        public double FrameRate { get; set; }

        /// <summary>
        /// Video codec (e.g., H.264, HEVC)
        /// </summary>
        public string VideoCodec { get; set; } = string.Empty;

        /// <summary>
        /// Audio codec (e.g., AAC, MP3)
        /// </summary>
        public string AudioCodec { get; set; } = string.Empty;

        /// <summary>
        /// Bitrate in kbps
        /// </summary>
        public long Bitrate { get; set; }

        /// <summary>
        /// File size in bytes
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// File size formatted as human-readable string
        /// </summary>
        public string FileSizeFormatted => FormatFileSize(FileSize);

        /// <summary>
        /// Duration formatted as string
        /// </summary>
        public string DurationFormatted => Duration.ToString(@"hh\:mm\:ss\.fff");

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            return $"{size:0.##} {sizes[order]}";
        }
    }
}
