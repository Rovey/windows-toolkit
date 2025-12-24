namespace WindowsToolkit.Core.Models
{
    /// <summary>
    /// Represents a software package
    /// </summary>
    public class Package
    {
        /// <summary>
        /// Unique package identifier (e.g., winget ID)
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Display name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Package description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Package version
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Publisher/Author
        /// </summary>
        public string Publisher { get; set; } = string.Empty;

        /// <summary>
        /// Package category
        /// </summary>
        public string Category { get; set; } = "General";

        /// <summary>
        /// Whether the package is currently installed
        /// </summary>
        public bool IsInstalled { get; set; }

        /// <summary>
        /// Source (e.g., "winget", "chocolatey")
        /// </summary>
        public string Source { get; set; } = "winget";
    }
}
