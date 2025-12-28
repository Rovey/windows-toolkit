namespace WindowsToolkit.Core.Models
{
    /// <summary>
    /// Represents a file in the Downloads folder
    /// </summary>
    public class DownloadFile
    {
        /// <summary>
        /// Full file path
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// File name with extension
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// File extension (e.g., ".pdf")
        /// </summary>
        public string Extension { get; set; } = string.Empty;

        /// <summary>
        /// File size in bytes
        /// </summary>
        public long SizeInBytes { get; set; }

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Last modified date
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        /// File category based on extension
        /// </summary>
        public FileCategory Category { get; set; }

        /// <summary>
        /// Number of days since last modified
        /// </summary>
        public int DaysSinceModified => (DateTime.Now - ModifiedDate).Days;
    }
}
