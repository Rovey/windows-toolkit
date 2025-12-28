using WindowsToolkit.Core.Models;

namespace WindowsToolkit.Core.Interfaces
{
    /// <summary>
    /// Service for cleaning up and organizing the Downloads folder
    /// </summary>
    public interface IDownloadsCleanupService
    {
        /// <summary>
        /// Scans the Downloads folder and returns matching files based on filter
        /// </summary>
        Task<IEnumerable<DownloadFile>> ScanDownloadsFolderAsync(CleanupFilter filter);

        /// <summary>
        /// Previews the cleanup operation without executing it
        /// </summary>
        Task<CleanupOperation> PreviewCleanupAsync(CleanupFilter filter);

        /// <summary>
        /// Executes the cleanup operation (DELETE or ORGANIZE)
        /// </summary>
        Task<CleanupResult> ExecuteCleanupAsync(
            CleanupOperation operation,
            IProgress<string>? progress = null);

        /// <summary>
        /// Gets the Downloads folder path
        /// </summary>
        string GetDownloadsFolderPath();
    }
}
