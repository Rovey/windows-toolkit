namespace WindowsToolkit.Core.Models
{
    /// <summary>
    /// Cleanup operation mode
    /// </summary>
    public enum CleanupMode
    {
        Delete,      // Move to Recycle Bin
        Organize     // Move to category subfolders
    }

    /// <summary>
    /// Represents a planned cleanup operation
    /// </summary>
    public class CleanupOperation
    {
        /// <summary>
        /// Operation mode
        /// </summary>
        public CleanupMode Mode { get; set; }

        /// <summary>
        /// Files to be affected
        /// </summary>
        public List<DownloadFile> Files { get; set; } = new();

        /// <summary>
        /// Total number of files
        /// </summary>
        public int TotalFiles => Files.Count;

        /// <summary>
        /// Total size in bytes
        /// </summary>
        public long TotalSizeBytes => Files.Sum(f => f.SizeInBytes);

        /// <summary>
        /// Applied filter
        /// </summary>
        public CleanupFilter Filter { get; set; } = new();
    }
}
