using WindowsToolkit.Core.Interfaces;
using WindowsToolkit.Core.Models;
using WindowsToolkit.Infrastructure.PowerShell;

namespace WindowsToolkit.Core.Services.PackageManager
{
    /// <summary>
    /// Service for managing software packages via Winget
    /// </summary>
    public class PackageManagerService : IPackageManagerService
    {
        private readonly WingetExecutor _wingetExecutor;
        private List<Package> _availablePackages;

        public PackageManagerService()
        {
            _wingetExecutor = new WingetExecutor();
            _availablePackages = new List<Package>();
            InitializeDefaultPackages();
        }

        /// <summary>
        /// Initialize a default list of commonly used packages
        /// </summary>
        private void InitializeDefaultPackages()
        {
            _availablePackages = new List<Package>
            {
                // Browsers
                new Package { Id = "Google.Chrome", Name = "Google Chrome", Description = "Fast, secure web browser from Google", Category = "Browsers", Source = "winget" },
                new Package { Id = "Mozilla.Firefox", Name = "Mozilla Firefox", Description = "Free and open-source web browser", Category = "Browsers", Source = "winget" },
                new Package { Id = "Microsoft.Edge", Name = "Microsoft Edge", Description = "Microsoft's Chromium-based browser", Category = "Browsers", Source = "winget" },
                new Package { Id = "BraveSoftware.BraveBrowser", Name = "Brave Browser", Description = "Privacy-focused browser", Category = "Browsers", Source = "winget" },

                // Development Tools
                new Package { Id = "Microsoft.VisualStudioCode", Name = "Visual Studio Code", Description = "Lightweight but powerful source code editor", Category = "Development", Source = "winget" },
                new Package { Id = "Git.Git", Name = "Git", Description = "Distributed version control system", Category = "Development", Source = "winget" },
                new Package { Id = "Microsoft.VisualStudio.2022.Community", Name = "Visual Studio 2022", Description = "Integrated Development Environment", Category = "Development", Source = "winget" },
                new Package { Id = "Microsoft.PowerShell", Name = "PowerShell", Description = "Cross-platform task automation solution", Category = "Development", Source = "winget" },
                new Package { Id = "Microsoft.WindowsTerminal", Name = "Windows Terminal", Description = "Modern terminal application", Category = "Development", Source = "winget" },
                new Package { Id = "Docker.DockerDesktop", Name = "Docker Desktop", Description = "Containerization platform", Category = "Development", Source = "winget" },
                new Package { Id = "Postman.Postman", Name = "Postman", Description = "API development platform", Category = "Development", Source = "winget" },

                // Media
                new Package { Id = "VideoLAN.VLC", Name = "VLC Media Player", Description = "Free and open source cross-platform multimedia player", Category = "Media", Source = "winget" },
                new Package { Id = "Spotify.Spotify", Name = "Spotify", Description = "Digital music streaming service", Category = "Media", Source = "winget" },
                new Package { Id = "OBSProject.OBSStudio", Name = "OBS Studio", Description = "Video recording and live streaming software", Category = "Media", Source = "winget" },
                new Package { Id = "GIMP.GIMP", Name = "GIMP", Description = "GNU Image Manipulation Program", Category = "Media", Source = "winget" },
                new Package { Id = "Audacity.Audacity", Name = "Audacity", Description = "Audio editing software", Category = "Media", Source = "winget" },

                // Communication
                new Package { Id = "Discord.Discord", Name = "Discord", Description = "Voice, video and text communication platform", Category = "Communication", Source = "winget" },
                new Package { Id = "SlackTechnologies.Slack", Name = "Slack", Description = "Team collaboration tool", Category = "Communication", Source = "winget" },
                new Package { Id = "Zoom.Zoom", Name = "Zoom", Description = "Video conferencing software", Category = "Communication", Source = "winget" },

                // Utilities
                new Package { Id = "7zip.7zip", Name = "7-Zip", Description = "File archiver with a high compression ratio", Category = "Utilities", Source = "winget" },
                new Package { Id = "Notepad++.Notepad++", Name = "Notepad++", Description = "Free source code editor and Notepad replacement", Category = "Utilities", Source = "winget" },
                new Package { Id = "WinDirStat.WinDirStat", Name = "WinDirStat", Description = "Disk usage statistics viewer", Category = "Utilities", Source = "winget" },
                new Package { Id = "Microsoft.PowerToys", Name = "PowerToys", Description = "Windows system utilities to maximize productivity", Category = "Utilities", Source = "winget" },
                new Package { Id = "TeamViewer.TeamViewer", Name = "TeamViewer", Description = "Remote access and support software", Category = "Utilities", Source = "winget" },
            };
        }

        public async Task<IEnumerable<Package>> GetAvailablePackagesAsync()
        {
            // Check which packages are installed
            var installedIds = await GetInstalledPackageIdsAsync();

            foreach (var package in _availablePackages)
            {
                package.IsInstalled = installedIds.Contains(package.Id);
            }

            return _availablePackages;
        }

        public async Task<IEnumerable<Package>> GetInstalledPackagesAsync()
        {
            var installedIds = await GetInstalledPackageIdsAsync();
            return _availablePackages.Where(p => installedIds.Contains(p.Id));
        }

        private async Task<HashSet<string>> GetInstalledPackageIdsAsync()
        {
            try
            {
                var output = await _wingetExecutor.ListInstalledPackagesAsync();
                var installedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // Parse winget list output
                // This is a simplified parser - winget output format may vary
                var lines = output.Split('\n');
                foreach (var line in lines)
                {
                    foreach (var package in _availablePackages)
                    {
                        if (line.Contains(package.Id, StringComparison.OrdinalIgnoreCase) ||
                            line.Contains(package.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            installedIds.Add(package.Id);
                        }
                    }
                }

                return installedIds;
            }
            catch
            {
                return new HashSet<string>();
            }
        }

        public async Task<InstallationResult> InstallPackageAsync(string packageId, IProgress<string>? progress = null)
        {
            try
            {
                var (exitCode, output) = await _wingetExecutor.InstallPackageAsync(packageId, progress);

                var success = exitCode == 0;

                return new InstallationResult
                {
                    PackageId = packageId,
                    Success = success,
                    Output = output,
                    ExitCode = exitCode,
                    ErrorMessage = success ? null : $"Installation failed with exit code {exitCode}"
                };
            }
            catch (Exception ex)
            {
                return new InstallationResult
                {
                    PackageId = packageId,
                    Success = false,
                    ErrorMessage = ex.Message,
                    ExitCode = -1
                };
            }
        }

        public async Task<IEnumerable<InstallationResult>> InstallPackagesAsync(
            IEnumerable<string> packageIds,
            IProgress<string>? progress = null)
        {
            var results = new List<InstallationResult>();

            foreach (var packageId in packageIds)
            {
                progress?.Report($"Installing {packageId}...");
                var result = await InstallPackageAsync(packageId, progress);
                results.Add(result);

                if (result.Success)
                {
                    progress?.Report($"Successfully installed {packageId}");
                }
                else
                {
                    progress?.Report($"Failed to install {packageId}: {result.ErrorMessage}");
                }
            }

            return results;
        }

        public async Task<InstallationResult> UninstallPackageAsync(string packageId, IProgress<string>? progress = null)
        {
            try
            {
                var (exitCode, output) = await _wingetExecutor.UninstallPackageAsync(packageId, progress);

                var success = exitCode == 0;

                return new InstallationResult
                {
                    PackageId = packageId,
                    Success = success,
                    Output = output,
                    ExitCode = exitCode,
                    ErrorMessage = success ? null : $"Uninstallation failed with exit code {exitCode}"
                };
            }
            catch (Exception ex)
            {
                return new InstallationResult
                {
                    PackageId = packageId,
                    Success = false,
                    ErrorMessage = ex.Message,
                    ExitCode = -1
                };
            }
        }

        public async Task<bool> IsPackageInstalledAsync(string packageId)
        {
            var installedIds = await GetInstalledPackageIdsAsync();
            return installedIds.Contains(packageId);
        }

        public async Task<IEnumerable<Package>> SearchPackagesAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return await GetAvailablePackagesAsync();
            }

            var allPackages = await GetAvailablePackagesAsync();

            return allPackages.Where(p =>
                p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.Id.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.Category.Contains(query, StringComparison.OrdinalIgnoreCase));
        }
    }
}
