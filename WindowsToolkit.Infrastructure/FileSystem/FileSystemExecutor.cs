using Microsoft.VisualBasic.FileIO;

namespace WindowsToolkit.Infrastructure.FileSystem
{
    /// <summary>
    /// Handles file system operations including moving and deleting files
    /// </summary>
    public class FileSystemExecutor
    {
        /// <summary>
        /// Moves file to Recycle Bin (soft delete)
        /// </summary>
        public async Task<(bool success, string error)> MoveToRecycleBinAsync(string filePath)
        {
            try
            {
                await Task.Run(() =>
                {
                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(filePath,
                        UIOption.OnlyErrorDialogs,
                        RecycleOption.SendToRecycleBin);
                });
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Moves file to target directory
        /// </summary>
        public async Task<(bool success, string error)> MoveFileAsync(
            string sourceFile,
            string targetDirectory)
        {
            try
            {
                await Task.Run(() =>
                {
                    // Create target directory if it doesn't exist
                    if (!Directory.Exists(targetDirectory))
                        Directory.CreateDirectory(targetDirectory);

                    var fileName = Path.GetFileName(sourceFile);
                    var targetPath = Path.Combine(targetDirectory, fileName);

                    // Handle duplicate file names
                    targetPath = GetUniqueFilePath(targetPath);

                    File.Move(sourceFile, targetPath);
                });
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Gets a unique file path by appending (1), (2), etc. if file already exists
        /// </summary>
        private string GetUniqueFilePath(string filePath)
        {
            if (!File.Exists(filePath))
                return filePath;

            var directory = Path.GetDirectoryName(filePath)!;
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
            var extension = Path.GetExtension(filePath);
            var counter = 1;

            string newPath;
            do
            {
                newPath = Path.Combine(directory,
                    $"{fileNameWithoutExt} ({counter}){extension}");
                counter++;
            } while (File.Exists(newPath));

            return newPath;
        }
    }
}
