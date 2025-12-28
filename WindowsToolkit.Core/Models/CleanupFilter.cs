namespace WindowsToolkit.Core.Models
{
    /// <summary>
    /// Filter criteria for cleanup operations
    /// </summary>
    public class CleanupFilter
    {
        /// <summary>
        /// Selected file categories to include
        /// </summary>
        public HashSet<FileCategory> SelectedCategories { get; set; } = new();

        /// <summary>
        /// Minimum age in days (files older than this)
        /// </summary>
        public int? MinimumAgeDays { get; set; }

        /// <summary>
        /// Whether age filter is enabled
        /// </summary>
        public bool UseAgeFilter => MinimumAgeDays.HasValue && MinimumAgeDays.Value > 0;
    }
}
