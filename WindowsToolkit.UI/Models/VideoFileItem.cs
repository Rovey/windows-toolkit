using System;

namespace WindowsToolkit.UI.Models
{
    /// <summary>
    /// UI model for displaying video files in the list
    /// </summary>
    public class VideoFileItem
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
        /// File extension
        /// </summary>
        public string Extension { get; set; } = string.Empty;

        /// <summary>
        /// File size formatted as human-readable string
        /// </summary>
        public string FileSizeFormatted { get; set; } = string.Empty;

        /// <summary>
        /// Video duration formatted (HH:MM:SS)
        /// </summary>
        public string DurationFormatted { get; set; } = "Unknown";

        /// <summary>
        /// Date the file was last modified
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        /// Modified date formatted as string
        /// </summary>
        public string ModifiedDateFormatted => ModifiedDate.ToString("dd-MM-yyyy HH:mm");

        /// <summary>
        /// Video duration as TimeSpan (for cutting)
        /// </summary>
        public TimeSpan? Duration { get; set; }

        /// <summary>
        /// Whether this item is currently selected
        /// </summary>
        public bool IsSelected { get; set; }
    }
}
