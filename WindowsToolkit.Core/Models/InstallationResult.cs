namespace WindowsToolkit.Core.Models
{
    /// <summary>
    /// Result of a package installation operation
    /// </summary>
    public class InstallationResult
    {
        /// <summary>
        /// Package ID that was installed
        /// </summary>
        public string PackageId { get; set; } = string.Empty;

        /// <summary>
        /// Whether the installation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error message if installation failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Standard output from the installation process
        /// </summary>
        public string? Output { get; set; }

        /// <summary>
        /// Exit code from the installation process
        /// </summary>
        public int ExitCode { get; set; }
    }
}
