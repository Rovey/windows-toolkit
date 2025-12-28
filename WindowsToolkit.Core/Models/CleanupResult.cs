namespace WindowsToolkit.Core.Models
{
    /// <summary>
    /// Result of a cleanup operation
    /// </summary>
    public class CleanupResult
    {
        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Number of files successfully processed
        /// </summary>
        public int FilesProcessed { get; set; }

        /// <summary>
        /// Number of files that failed
        /// </summary>
        public int FilesFailed { get; set; }

        /// <summary>
        /// Total size of processed files in bytes
        /// </summary>
        public long TotalBytesProcessed { get; set; }

        /// <summary>
        /// Error messages for failed files
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// Overall error message if operation failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Operation mode that was executed
        /// </summary>
        public CleanupMode Mode { get; set; }
    }
}
