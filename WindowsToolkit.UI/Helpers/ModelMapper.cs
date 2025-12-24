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
    }
}
