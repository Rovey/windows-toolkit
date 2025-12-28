using System;
using WindowsToolkit.UI.ViewModels;

namespace WindowsToolkit.UI.Models
{
    /// <summary>
    /// UI representation of a download file
    /// </summary>
    public class DownloadFileItem : ViewModelBase
    {
        private bool _isSelected = true;  // Default selected

        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public long SizeInBytes { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Category { get; set; } = string.Empty;
        public int DaysSinceModified { get; set; }

        /// <summary>
        /// Whether this file is selected for cleanup
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        /// <summary>
        /// Formatted file size (e.g., "2.5 MB")
        /// </summary>
        public string FormattedSize => FormatBytes(SizeInBytes);

        /// <summary>
        /// Formatted age (e.g., "5 days ago")
        /// </summary>
        public string FormattedAge =>
            DaysSinceModified == 0 ? "Today" :
            DaysSinceModified == 1 ? "Yesterday" :
            $"{DaysSinceModified} days ago";

        /// <summary>
        /// Formats bytes to human-readable size
        /// </summary>
        public static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
