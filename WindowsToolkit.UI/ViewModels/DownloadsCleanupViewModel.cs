using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WindowsToolkit.Core.Interfaces;
using WindowsToolkit.Core.Models;
using WindowsToolkit.UI.Helpers;
using WindowsToolkit.UI.Models;
using WindowsToolkit.UI.Services;

namespace WindowsToolkit.UI.ViewModels
{
    public class DownloadsCleanupViewModel : ViewModelBase
    {
        private readonly IDownloadsCleanupService _cleanupService;

        // Observable Collections
        public ObservableCollection<DownloadFileItem> Files { get; }

        // Filter Properties
        private bool _filterInstallers;
        private bool _filterImages;
        private bool _filterDocuments;
        private bool _filterSpreadsheets;
        private bool _filterVideos;
        private bool _filterAudio;
        private bool _filterArchives;
        private bool _useAgeFilter;
        private int _minimumAgeDays = 30;

        // Operation Properties
        private bool _isDeleteMode = true;
        private bool _isOrganizeMode;
        private bool _isScanning;
        private bool _isExecuting;
        private string _statusMessage = "Ready";

        // Preview Properties
        private int _previewFileCount;
        private string _previewTotalSize = "0 B";

        // Commands
        public ICommand ScanFilesCommand { get; }
        public ICommand PreviewCleanupCommand { get; }
        public ICommand ExecuteCleanupCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand DeselectAllCommand { get; }

        public DownloadsCleanupViewModel()
        {
            _cleanupService = ServiceLocator.Instance.DownloadsCleanupService;
            Files = new ObservableCollection<DownloadFileItem>();

            // Initialize commands
            ScanFilesCommand = new RelayCommand(async () => await ScanFilesAsync(), () => !IsScanning && !IsExecuting);
            PreviewCleanupCommand = new RelayCommand(async () => await PreviewCleanupAsync(), () => Files.Any() && !IsExecuting);
            ExecuteCleanupCommand = new RelayCommand(async () => await ExecuteCleanupAsync(), () => Files.Any(f => f.IsSelected) && !IsExecuting);
            SelectAllCommand = new RelayCommand(SelectAll, () => Files.Any());
            DeselectAllCommand = new RelayCommand(DeselectAll, () => Files.Any());

            // Auto-scan on initialization
            _ = ScanFilesAsync();
        }

        #region Filter Properties

        public bool FilterInstallers
        {
            get => _filterInstallers;
            set => SetProperty(ref _filterInstallers, value);
        }

        public bool FilterImages
        {
            get => _filterImages;
            set => SetProperty(ref _filterImages, value);
        }

        public bool FilterDocuments
        {
            get => _filterDocuments;
            set => SetProperty(ref _filterDocuments, value);
        }

        public bool FilterSpreadsheets
        {
            get => _filterSpreadsheets;
            set => SetProperty(ref _filterSpreadsheets, value);
        }

        public bool FilterVideos
        {
            get => _filterVideos;
            set => SetProperty(ref _filterVideos, value);
        }

        public bool FilterAudio
        {
            get => _filterAudio;
            set => SetProperty(ref _filterAudio, value);
        }

        public bool FilterArchives
        {
            get => _filterArchives;
            set => SetProperty(ref _filterArchives, value);
        }

        public bool UseAgeFilter
        {
            get => _useAgeFilter;
            set => SetProperty(ref _useAgeFilter, value);
        }

        public int MinimumAgeDays
        {
            get => _minimumAgeDays;
            set => SetProperty(ref _minimumAgeDays, value);
        }

        #endregion

        #region Operation Properties

        public bool IsDeleteMode
        {
            get => _isDeleteMode;
            set
            {
                if (SetProperty(ref _isDeleteMode, value) && value)
                {
                    IsOrganizeMode = false;
                }
            }
        }

        public bool IsOrganizeMode
        {
            get => _isOrganizeMode;
            set
            {
                if (SetProperty(ref _isOrganizeMode, value) && value)
                {
                    IsDeleteMode = false;
                }
            }
        }

        public bool IsScanning
        {
            get => _isScanning;
            set => SetProperty(ref _isScanning, value);
        }

        public bool IsExecuting
        {
            get => _isExecuting;
            set => SetProperty(ref _isExecuting, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        #endregion

        #region Preview Properties

        public int PreviewFileCount
        {
            get => _previewFileCount;
            set => SetProperty(ref _previewFileCount, value);
        }

        public string PreviewTotalSize
        {
            get => _previewTotalSize;
            set => SetProperty(ref _previewTotalSize, value);
        }

        #endregion

        #region Command Methods

        private async Task ScanFilesAsync()
        {
            try
            {
                IsScanning = true;
                StatusMessage = "Scanning Downloads folder...";
                Files.Clear();

                var filter = CreateFilterFromUI();
                var files = await _cleanupService.ScanDownloadsFolderAsync(filter);

                foreach (var file in files)
                {
                    Files.Add(ModelMapper.ToDownloadFileItem(file));
                }

                StatusMessage = $"Found {Files.Count} file(s)";
                UpdatePreview();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Scan error: {ex.Message}";
                MessageBox.Show($"Failed to scan: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsScanning = false;
            }
        }

        private async Task PreviewCleanupAsync()
        {
            var selectedFiles = Files.Where(f => f.IsSelected).ToList();
            if (!selectedFiles.Any())
            {
                MessageBox.Show("No files selected", "Preview",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var totalSize = selectedFiles.Sum(f => f.SizeInBytes);
            var formattedSize = DownloadFileItem.FormatBytes(totalSize);
            var mode = IsDeleteMode ? "deleted (moved to Recycle Bin)" : "organized into category folders";

            var message = $"Ready to process:\n\n" +
                          $"Files: {selectedFiles.Count}\n" +
                          $"Total size: {formattedSize}\n\n" +
                          $"Action: {(IsDeleteMode ?
                              "Move to Recycle Bin" :
                              "Organize into category folders")}";

            MessageBox.Show(message, "Preview",
                MessageBoxButton.OK, MessageBoxImage.Information);

            await Task.CompletedTask;
        }

        private async Task ExecuteCleanupAsync()
        {
            var selectedFiles = Files.Where(f => f.IsSelected).ToList();
            if (!selectedFiles.Any())
            {
                MessageBox.Show("No files selected", "Cleanup",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Confirmation dialog
            var mode = IsDeleteMode ?
                "moved to Recycle Bin" :
                "organized into category folders";
            var result = MessageBox.Show(
                $"Are you sure you want to process {selectedFiles.Count} file(s)?\n\n" +
                $"Files will be {mode}.",
                "Confirm Cleanup",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                IsExecuting = true;

                var operation = new CleanupOperation
                {
                    Mode = IsDeleteMode ? CleanupMode.Delete : CleanupMode.Organize,
                    Files = selectedFiles.Select(ModelMapper.ToDownloadFile).ToList()
                };

                var progress = new Progress<string>(message =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        StatusMessage = message;
                    });
                });

                var cleanupResult = await _cleanupService.ExecuteCleanupAsync(operation, progress);

                if (cleanupResult.Success || cleanupResult.FilesProcessed > 0)
                {
                    StatusMessage = $"Successfully processed {cleanupResult.FilesProcessed} file(s)";
                    MessageBox.Show(
                        $"Cleanup complete!\n\n" +
                        $"Files processed: {cleanupResult.FilesProcessed}\n" +
                        $"Failed: {cleanupResult.FilesFailed}",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Refresh the file list
                    await ScanFilesAsync();
                }
                else
                {
                    StatusMessage = $"Cleanup failed: {cleanupResult.ErrorMessage}";
                    MessageBox.Show(
                        $"Cleanup failed!\n\n{cleanupResult.ErrorMessage}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                MessageBox.Show($"Cleanup failed: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsExecuting = false;
            }
        }

        private void SelectAll()
        {
            foreach (var file in Files)
            {
                file.IsSelected = true;
            }
            UpdatePreview();
        }

        private void DeselectAll()
        {
            foreach (var file in Files)
            {
                file.IsSelected = false;
            }
            UpdatePreview();
        }

        #endregion

        #region Helper Methods

        private CleanupFilter CreateFilterFromUI()
        {
            var filter = new CleanupFilter();

            if (FilterInstallers) filter.SelectedCategories.Add(FileCategory.Installers);
            if (FilterImages) filter.SelectedCategories.Add(FileCategory.Images);
            if (FilterDocuments) filter.SelectedCategories.Add(FileCategory.Documents);
            if (FilterSpreadsheets) filter.SelectedCategories.Add(FileCategory.Spreadsheets);
            if (FilterVideos) filter.SelectedCategories.Add(FileCategory.Videos);
            if (FilterAudio) filter.SelectedCategories.Add(FileCategory.Audio);
            if (FilterArchives) filter.SelectedCategories.Add(FileCategory.Archives);

            if (UseAgeFilter)
                filter.MinimumAgeDays = MinimumAgeDays;

            return filter;
        }

        private void UpdatePreview()
        {
            var selectedFiles = Files.Where(f => f.IsSelected).ToList();
            PreviewFileCount = selectedFiles.Count;
            PreviewTotalSize = DownloadFileItem.FormatBytes(selectedFiles.Sum(f => f.SizeInBytes));
        }

        #endregion
    }
}
