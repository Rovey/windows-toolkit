using WindowsToolkit.UI.ViewModels;

namespace WindowsToolkit.UI.Models
{
    /// <summary>
    /// Represents a software package that can be installed
    /// </summary>
    public class PackageItem : ViewModelBase
    {
        private bool _isSelected;
        private bool _isInstalled;
        private string _installationStatus = "Not Installed";

        /// <summary>
        /// Unique package identifier (e.g., winget package ID)
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Display name of the package
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Package description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Package category
        /// </summary>
        public string Category { get; set; } = "General";

        /// <summary>
        /// Package version
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Publisher/Author of the package
        /// </summary>
        public string Publisher { get; set; } = string.Empty;

        /// <summary>
        /// Whether this package is selected for installation
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        /// <summary>
        /// Whether this package is already installed
        /// </summary>
        public bool IsInstalled
        {
            get => _isInstalled;
            set
            {
                if (SetProperty(ref _isInstalled, value))
                {
                    InstallationStatus = value ? "Installed" : "Not Installed";
                }
            }
        }

        /// <summary>
        /// Installation status text
        /// </summary>
        public string InstallationStatus
        {
            get => _installationStatus;
            set => SetProperty(ref _installationStatus, value);
        }
    }
}
