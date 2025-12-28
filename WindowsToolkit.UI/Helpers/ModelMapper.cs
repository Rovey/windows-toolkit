using System;
using WindowsToolkit.Core.Models;
using WindowsToolkit.UI.Models;

namespace WindowsToolkit.UI.Helpers
{
    /// <summary>
    /// Maps between domain models and UI models
    /// </summary>
    public static class ModelMapper
    {
        /// <summary>
        /// Converts a Core Package to a UI PackageItem
        /// </summary>
        public static PackageItem ToPackageItem(Package package)
        {
            return new PackageItem
            {
                Id = package.Id,
                Name = package.Name,
                Description = package.Description,
                Category = package.Category,
                Version = package.Version,
                Publisher = package.Publisher,
                IsInstalled = package.IsInstalled,
                IsSelected = !package.IsInstalled // Auto-select uninstalled packages
            };
        }

        /// <summary>
        /// Converts a UI PackageItem to a Core Package
        /// </summary>
        public static Package ToPackage(PackageItem packageItem)
        {
            return new Package
            {
                Id = packageItem.Id,
                Name = packageItem.Name,
                Description = packageItem.Description,
                Category = packageItem.Category,
                Version = packageItem.Version,
                Publisher = packageItem.Publisher,
                IsInstalled = packageItem.IsInstalled
            };
        }

        /// <summary>
        /// Converts a Core DownloadFile to a UI DownloadFileItem
        /// </summary>
        public static DownloadFileItem ToDownloadFileItem(DownloadFile file)
        {
            return new DownloadFileItem
            {
                FilePath = file.FilePath,
                FileName = file.FileName,
                Extension = file.Extension,
                SizeInBytes = file.SizeInBytes,
                ModifiedDate = file.ModifiedDate,
                Category = file.Category.ToString(),
                DaysSinceModified = file.DaysSinceModified,
                IsSelected = true // Default selected
            };
        }

        /// <summary>
        /// Converts a UI DownloadFileItem to a Core DownloadFile
        /// </summary>
        public static DownloadFile ToDownloadFile(DownloadFileItem item)
        {
            return new DownloadFile
            {
                FilePath = item.FilePath,
                FileName = item.FileName,
                Extension = item.Extension,
                SizeInBytes = item.SizeInBytes,
                ModifiedDate = item.ModifiedDate,
                Category = Enum.TryParse<FileCategory>(item.Category, out var category)
                    ? category
                    : FileCategory.All
            };
        }
    }
}
