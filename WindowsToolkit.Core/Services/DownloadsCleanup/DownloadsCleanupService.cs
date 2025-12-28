using WindowsToolkit.Core.Interfaces;
using WindowsToolkit.Core.Models;
using WindowsToolkit.Infrastructure.FileSystem;

namespace WindowsToolkit.Core.Services.DownloadsCleanup
{
    /// <summary>
    /// Service for cleaning up and organizing the Downloads folder
    /// </summary>
    public class DownloadsCleanupService : IDownloadsCleanupService
    {
        private readonly FileSystemExecutor _fileSystemExecutor;

        /// <summary>
        /// Extension to category mapping
        /// </summary>
        private static readonly Dictionary<FileCategory, HashSet<string>> CategoryExtensions = new()
        {
            { FileCategory.Installers, new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { ".exe", ".msi" } },
            { FileCategory.Images, new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".svg", ".webp" } },
            { FileCategory.Documents, new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { ".pdf", ".doc", ".docx", ".txt", ".rtf" } },
            { FileCategory.Spreadsheets, new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { ".xls", ".xlsx", ".csv" } },
            { FileCategory.Videos, new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv" } },
            { FileCategory.Audio, new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { ".mp3", ".wav", ".flac", ".aac", ".m4a" } },
            { FileCategory.Archives, new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { ".zip", ".rar", ".7z", ".tar", ".gz" } }
        };

        /// <summary>
        /// Category to folder name mapping (Dutch names)
        /// </summary>
        private static readonly Dictionary<FileCategory, string> CategoryFolderNames = new()
        {
            { FileCategory.Installers, "Installers" },
            { FileCategory.Images, "Afbeeldingen" },
            { FileCategory.Documents, "Documenten" },
            { FileCategory.Spreadsheets, "Spreadsheets" },
            { FileCategory.Videos, "Video's" },
            { FileCategory.Audio, "Audio" },
            { FileCategory.Archives, "Archieven" }
        };

        public DownloadsCleanupService()
        {
            _fileSystemExecutor = new FileSystemExecutor();
        }

        /// <summary>
        /// Gets the Downloads folder path for the current user
        /// </summary>
        public string GetDownloadsFolderPath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads"
            );
        }

        /// <summary>
        /// Scans the Downloads folder and returns matching files based on filter
        /// </summary>
        public async Task<IEnumerable<DownloadFile>> ScanDownloadsFolderAsync(CleanupFilter filter)
        {
            try
            {
                var downloadsPath = GetDownloadsFolderPath();

                if (!Directory.Exists(downloadsPath))
                {
                    return Enumerable.Empty<DownloadFile>();
                }

                var files = Directory.GetFiles(downloadsPath);

                var downloadFiles = await Task.Run(() =>
                {
                    return files
                        .Select(path => CreateDownloadFile(path))
                        .Where(file => MatchesFilter(file, filter))
                        .OrderByDescending(f => f.ModifiedDate)
                        .ToList();
                });

                return downloadFiles;
            }
            catch (Exception)
            {
                return Enumerable.Empty<DownloadFile>();
            }
        }

        /// <summary>
        /// Previews the cleanup operation without executing it
        /// </summary>
        public async Task<CleanupOperation> PreviewCleanupAsync(CleanupFilter filter)
        {
            var files = await ScanDownloadsFolderAsync(filter);

            return new CleanupOperation
            {
                Files = files.ToList(),
                Filter = filter
            };
        }

        /// <summary>
        /// Executes the cleanup operation (DELETE or ORGANIZE)
        /// </summary>
        public async Task<CleanupResult> ExecuteCleanupAsync(
            CleanupOperation operation,
            IProgress<string>? progress = null)
        {
            var result = new CleanupResult
            {
                Mode = operation.Mode,
                Success = false
            };

            try
            {
                var downloadsPath = GetDownloadsFolderPath();
                var totalFiles = operation.Files.Count;
                var processedCount = 0;

                foreach (var file in operation.Files)
                {
                    processedCount++;
                    progress?.Report($"Processing {processedCount}/{totalFiles}: {file.FileName}");

                    bool success;
                    string error;

                    if (operation.Mode == CleanupMode.Delete)
                    {
                        (success, error) = await _fileSystemExecutor.MoveToRecycleBinAsync(file.FilePath);
                    }
                    else // CleanupMode.Organize
                    {
                        var categoryFolderName = GetCategoryFolderName(file.Category);
                        var targetDirectory = Path.Combine(downloadsPath, categoryFolderName);
                        (success, error) = await _fileSystemExecutor.MoveFileAsync(file.FilePath, targetDirectory);
                    }

                    if (success)
                    {
                        result.FilesProcessed++;
                        result.TotalBytesProcessed += file.SizeInBytes;
                    }
                    else
                    {
                        result.FilesFailed++;
                        result.Errors.Add($"{file.FileName}: {error}");
                    }
                }

                result.Success = result.FilesFailed == 0;
                if (!result.Success && result.FilesFailed == totalFiles)
                {
                    result.ErrorMessage = "All files failed to process";
                }
                else if (result.FilesFailed > 0)
                {
                    result.ErrorMessage = $"{result.FilesFailed} file(s) failed to process";
                }

                progress?.Report($"Completed: {result.FilesProcessed} processed, {result.FilesFailed} failed");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Creates a DownloadFile object from a file path
        /// </summary>
        private DownloadFile CreateDownloadFile(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var category = GetFileCategory(fileInfo.Extension);

            return new DownloadFile
            {
                FilePath = filePath,
                FileName = fileInfo.Name,
                Extension = fileInfo.Extension,
                SizeInBytes = fileInfo.Length,
                CreatedDate = fileInfo.CreationTime,
                ModifiedDate = fileInfo.LastWriteTime,
                Category = category
            };
        }

        /// <summary>
        /// Determines if a file matches the filter criteria
        /// </summary>
        private bool MatchesFilter(DownloadFile file, CleanupFilter filter)
        {
            // If no categories selected, show all files
            // Otherwise, only show files in selected categories
            if (filter.SelectedCategories.Any() &&
                !filter.SelectedCategories.Contains(file.Category))
            {
                return false;
            }

            // Age filter
            if (filter.UseAgeFilter &&
                file.DaysSinceModified < filter.MinimumAgeDays!.Value)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the file category based on extension
        /// </summary>
        private FileCategory GetFileCategory(string extension)
        {
            foreach (var (category, extensions) in CategoryExtensions)
            {
                if (extensions.Contains(extension))
                    return category;
            }
            return FileCategory.All;
        }

        /// <summary>
        /// Gets the folder name for a category
        /// </summary>
        private string GetCategoryFolderName(FileCategory category)
        {
            return CategoryFolderNames.TryGetValue(category, out var folderName)
                ? folderName
                : "Other";
        }
    }
}
