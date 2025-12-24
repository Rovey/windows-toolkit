using System.Diagnostics;
using System.Text;

namespace WindowsToolkit.Infrastructure.PowerShell
{
    /// <summary>
    /// Executes winget commands
    /// </summary>
    public class WingetExecutor
    {
        /// <summary>
        /// Executes a winget command and returns the output
        /// </summary>
        public async Task<(int exitCode, string output, string error)> ExecuteWingetCommandAsync(
            string arguments,
            IProgress<string>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "winget",
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using var process = new Process { StartInfo = processStartInfo };

            // Handle output data
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    outputBuilder.AppendLine(e.Data);
                    progress?.Report(e.Data);
                }
            };

            // Handle error data
            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    errorBuilder.AppendLine(e.Data);
                }
            };

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync(cancellationToken);

                return (process.ExitCode, outputBuilder.ToString(), errorBuilder.ToString());
            }
            catch (Exception ex)
            {
                return (-1, string.Empty, $"Failed to execute winget: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if winget is available on the system
        /// </summary>
        public async Task<bool> IsWingetAvailableAsync()
        {
            try
            {
                var result = await ExecuteWingetCommandAsync("--version");
                return result.exitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Installs a package using winget
        /// </summary>
        public async Task<(int exitCode, string output)> InstallPackageAsync(
            string packageId,
            IProgress<string>? progress = null,
            CancellationToken cancellationToken = default)
        {
            progress?.Report($"Installing {packageId}...");

            // --accept-source-agreements and --accept-package-agreements to avoid prompts
            var arguments = $"install --id {packageId} --exact --silent --accept-source-agreements --accept-package-agreements";
            var result = await ExecuteWingetCommandAsync(arguments, progress, cancellationToken);

            return (result.exitCode, result.output);
        }

        /// <summary>
        /// Uninstalls a package using winget
        /// </summary>
        public async Task<(int exitCode, string output)> UninstallPackageAsync(
            string packageId,
            IProgress<string>? progress = null,
            CancellationToken cancellationToken = default)
        {
            progress?.Report($"Uninstalling {packageId}...");

            var arguments = $"uninstall --id {packageId} --exact --silent";
            var result = await ExecuteWingetCommandAsync(arguments, progress, cancellationToken);

            return (result.exitCode, result.output);
        }

        /// <summary>
        /// Lists installed packages
        /// </summary>
        public async Task<string> ListInstalledPackagesAsync()
        {
            var result = await ExecuteWingetCommandAsync("list");
            return result.output;
        }

        /// <summary>
        /// Searches for packages
        /// </summary>
        public async Task<string> SearchPackagesAsync(string query)
        {
            var arguments = $"search \"{query}\"";
            var result = await ExecuteWingetCommandAsync(arguments);
            return result.output;
        }
    }
}
