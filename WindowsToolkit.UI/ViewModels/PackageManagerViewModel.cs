using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using WindowsToolkit.UI.Helpers;
using WindowsToolkit.UI.Models;
using WindowsToolkit.Core.Interfaces;
using WindowsToolkit.UI.Services;

namespace WindowsToolkit.UI.ViewModels
{
    /// <summary>
    /// ViewModel for the Package Manager feature
    /// </summary>
    public class PackageManagerViewModel : ViewModelBase
    {
        private readonly IPackageManagerService _packageManagerService;
        private string _searchText = string.Empty;
        private bool _isInstalling = false;
        private string _statusMessage = "Ready";
        private int _installProgress = 0;
        private List<PackageItem> _allPackages = new();

        public PackageManagerViewModel()
        {
            // Get service from locator
            _packageManagerService = ServiceLocator.Instance.PackageManagerService;

            // Initialize commands
            InstallSelectedCommand = new RelayCommand(async () => await InstallSelectedAsync(), CanInstallSelected);
            InstallAllCommand = new RelayCommand(async () => await InstallAllAsync(), CanInstallAll);
            RefreshPackagesCommand = new RelayCommand(async () => await RefreshPackagesAsync());
            SearchCommand = new RelayCommand(Search);

            // Load packages - fire and forget pattern for constructor
            _ = LoadPackagesAsync();
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

        private async Task LoadPackagesAsync()
        {
            try
            {
                StatusMessage = "Loading packages...";

                var packages = await _packageManagerService.GetAvailablePackagesAsync();
                _allPackages = packages.Select(ModelMapper.ToPackageItem).ToList();

                // Apply current search filter
                ApplyFilter();

                StatusMessage = $"Loaded {_allPackages.Count} packages";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading packages: {ex.Message}";
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Failed to load packages: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private async Task InstallSelectedAsync()
        {
            var selectedPackages = Packages.Where(p => p.IsSelected && !p.IsInstalled).ToList();

            if (!selectedPackages.Any())
            {
                StatusMessage = "No packages selected";
                return;
            }

            try
            {
                IsInstalling = true;
                InstallProgress = 0;

                var packageIds = selectedPackages.Select(p => p.Id).ToList();
                var totalPackages = packageIds.Count;
                var completedPackages = 0;

                // Create progress reporter
                var progress = new Progress<string>(message =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        StatusMessage = message;
                    });
                });

                var results = await _packageManagerService.InstallPackagesAsync(packageIds, progress);

                // Process results
                foreach (var result in results)
                {
                    completedPackages++;
                    InstallProgress = (int)((double)completedPackages / totalPackages * 100);

                    var packageItem = selectedPackages.FirstOrDefault(p => p.Id == result.PackageId);
                    if (packageItem != null)
                    {
                        if (result.Success)
                        {
                            packageItem.IsInstalled = true;
                            packageItem.IsSelected = false;
                        }
                        else
                        {
                            packageItem.InstallationStatus = "Failed";
                        }
                    }
                }

                var successCount = results.Count(r => r.Success);
                var failCount = results.Count(r => !r.Success);

                if (failCount > 0)
                {
                    StatusMessage = $"Installation complete: {successCount} succeeded, {failCount} failed";
                    var failedPackages = string.Join("\n",
                        results.Where(r => !r.Success)
                               .Select(r => $"- {r.PackageId}: {r.ErrorMessage}"));

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"Some installations failed:\n\n{failedPackages}",
                            "Installation Results", MessageBoxButton.OK, MessageBoxImage.Warning);
                    });
                }
                else
                {
                    StatusMessage = $"Successfully installed {successCount} package(s)";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Installation error: {ex.Message}";
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Installation failed: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                IsInstalling = false;
                InstallProgress = 0;
            }
        }

        private bool CanInstallSelected()
        {
            return !IsInstalling && Packages.Any(p => p.IsSelected && !p.IsInstalled);
        }

        private async Task InstallAllAsync()
        {
            foreach (var package in Packages.Where(p => !p.IsInstalled))
            {
                package.IsSelected = true;
            }
            await InstallSelectedAsync();
        }

        private bool CanInstallAll()
        {
            return !IsInstalling && Packages.Any(p => !p.IsInstalled);
        }

        private async Task RefreshPackagesAsync()
        {
            await LoadPackagesAsync();
        }

        private void Search()
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            Packages.Clear();

            var filtered = string.IsNullOrWhiteSpace(_searchText)
                ? _allPackages
                : _allPackages.Where(p =>
                    p.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                    p.Category.Contains(_searchText, StringComparison.OrdinalIgnoreCase));

            foreach (var package in filtered)
            {
                Packages.Add(package);
            }
        }
    }
}
