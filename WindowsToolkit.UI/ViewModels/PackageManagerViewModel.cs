using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using WindowsToolkit.UI.Helpers;
using WindowsToolkit.UI.Models;

namespace WindowsToolkit.UI.ViewModels
{
    /// <summary>
    /// ViewModel for the Package Manager feature
    /// </summary>
    public class PackageManagerViewModel : ViewModelBase
    {
        private string _searchText = string.Empty;
        private bool _isInstalling = false;
        private string _statusMessage = "Ready";
        private int _installProgress = 0;

        public PackageManagerViewModel()
        {
            // Initialize commands
            InstallSelectedCommand = new RelayCommand(InstallSelected, CanInstallSelected);
            InstallAllCommand = new RelayCommand(InstallAll, CanInstallAll);
            RefreshPackagesCommand = new RelayCommand(RefreshPackages);
            SearchCommand = new RelayCommand(Search);

            // Load packages
            LoadPackages();
        }

        /// <summary>
        /// Collection of available packages
        /// </summary>
        public ObservableCollection<PackageItem> Packages { get; } = new();

        /// <summary>
        /// Search text for filtering packages
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    Search();
                }
            }
        }

        /// <summary>
        /// Indicates if an installation is in progress
        /// </summary>
        public bool IsInstalling
        {
            get => _isInstalling;
            set => SetProperty(ref _isInstalling, value);
        }

        /// <summary>
        /// Current status message
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>
        /// Installation progress percentage
        /// </summary>
        public int InstallProgress
        {
            get => _installProgress;
            set => SetProperty(ref _installProgress, value);
        }

        // Commands
        public ICommand InstallSelectedCommand { get; }
        public ICommand InstallAllCommand { get; }
        public ICommand RefreshPackagesCommand { get; }
        public ICommand SearchCommand { get; }

        private void LoadPackages()
        {
            // TODO: Load packages from service
            // For now, add some demo packages
            Packages.Clear();

            Packages.Add(new PackageItem
            {
                Id = "Google.Chrome",
                Name = "Google Chrome",
                Description = "Fast, secure web browser from Google",
                IsInstalled = false,
                IsSelected = true,
                Category = "Browsers"
            });

            Packages.Add(new PackageItem
            {
                Id = "Mozilla.Firefox",
                Name = "Mozilla Firefox",
                Description = "Free and open-source web browser",
                IsInstalled = false,
                IsSelected = true,
                Category = "Browsers"
            });

            Packages.Add(new PackageItem
            {
                Id = "Microsoft.VisualStudioCode",
                Name = "Visual Studio Code",
                Description = "Lightweight but powerful source code editor",
                IsInstalled = false,
                IsSelected = true,
                Category = "Development"
            });

            Packages.Add(new PackageItem
            {
                Id = "Git.Git",
                Name = "Git",
                Description = "Distributed version control system",
                IsInstalled = false,
                IsSelected = true,
                Category = "Development"
            });

            Packages.Add(new PackageItem
            {
                Id = "VideoLAN.VLC",
                Name = "VLC Media Player",
                Description = "Free and open source cross-platform multimedia player",
                IsInstalled = false,
                IsSelected = true,
                Category = "Media"
            });

            Packages.Add(new PackageItem
            {
                Id = "7zip.7zip",
                Name = "7-Zip",
                Description = "File archiver with a high compression ratio",
                IsInstalled = false,
                IsSelected = false,
                Category = "Utilities"
            });

            Packages.Add(new PackageItem
            {
                Id = "Notepad++.Notepad++",
                Name = "Notepad++",
                Description = "Free source code editor and Notepad replacement",
                IsInstalled = false,
                IsSelected = false,
                Category = "Utilities"
            });
        }

        private void InstallSelected()
        {
            // TODO: Implement installation logic with PackageManagerService
            StatusMessage = "Installing selected packages...";
            IsInstalling = true;

            // Placeholder - will be replaced with actual service call
            StatusMessage = "Installation complete!";
            IsInstalling = false;
        }

        private bool CanInstallSelected()
        {
            return !IsInstalling && Packages.Any(p => p.IsSelected && !p.IsInstalled);
        }

        private void InstallAll()
        {
            // Select all uninstalled packages
            foreach (var package in Packages.Where(p => !p.IsInstalled))
            {
                package.IsSelected = true;
            }
            InstallSelected();
        }

        private bool CanInstallAll()
        {
            return !IsInstalling && Packages.Any(p => !p.IsInstalled);
        }

        private void RefreshPackages()
        {
            LoadPackages();
            StatusMessage = "Package list refreshed";
        }

        private void Search()
        {
            // TODO: Implement search/filter logic
            // For now, this is just a placeholder
        }
    }
}
