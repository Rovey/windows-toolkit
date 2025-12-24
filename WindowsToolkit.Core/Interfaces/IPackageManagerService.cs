using WindowsToolkit.Core.Models;

namespace WindowsToolkit.Core.Interfaces
{
    /// <summary>
    /// Service for managing software package installation
    /// </summary>
    public interface IPackageManagerService
    {
        /// <summary>
        /// Gets a list of all available packages
        /// </summary>
        Task<IEnumerable<Package>> GetAvailablePackagesAsync();

        /// <summary>
        /// Gets a list of installed packages
        /// </summary>
        Task<IEnumerable<Package>> GetInstalledPackagesAsync();

        /// <summary>
        /// Installs a package by ID
        /// </summary>
        Task<InstallationResult> InstallPackageAsync(string packageId, IProgress<string>? progress = null);

        /// <summary>
        /// Installs multiple packages
        /// </summary>
        Task<IEnumerable<InstallationResult>> InstallPackagesAsync(IEnumerable<string> packageIds, IProgress<string>? progress = null);

        /// <summary>
        /// Uninstalls a package by ID
        /// </summary>
        Task<InstallationResult> UninstallPackageAsync(string packageId, IProgress<string>? progress = null);

        /// <summary>
        /// Checks if a package is installed
        /// </summary>
        Task<bool> IsPackageInstalledAsync(string packageId);

        /// <summary>
        /// Searches for packages by name or ID
        /// </summary>
        Task<IEnumerable<Package>> SearchPackagesAsync(string query);
    }
}
