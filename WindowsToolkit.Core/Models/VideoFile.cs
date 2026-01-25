namespace WindowsToolkit.Core.Models
{
    /// <summary>
    /// Represents a video file in the file system
    /// </summary>
    public class VideoFile
    {
        /// <summary>
        /// Full path to the video file
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// File name without path
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// File extension (e.g., .mp4, .mkv)
        /// </summary>
        public string Extension { get; set; } = string.Empty;

        /// <summary>
        /// File size in bytes
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// File size formatted as human-readable string
        /// </summary>
        public string FileSizeFormatted => FormatFileSize(FileSize);

        /// <summary>
        /// Date the file was created
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Date the file was last modified
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        /// Video duration (if available)
        /// </summary>
        public TimeSpan? Duration { get; set; }

        /// <summary>
        /// Duration formatted as string (HH:MM:SS)
        /// </summary>
        public string DurationFormatted => Duration.HasValue 
            ? Duration.Value.ToString(@"hh\:mm\:ss") 
            : "Unknown";

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
