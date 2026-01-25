namespace WindowsToolkit.Core.Models
{
    /// <summary>
    /// Result of a video cut operation
    /// </summary>
    public class VideoCutResult
    {
        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Path to the output file (if successful)
        /// </summary>
        public string? OutputPath { get; set; }

        /// <summary>
        /// Error message (if failed)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Duration of the output video
        /// </summary>
        public TimeSpan? OutputDuration { get; set; }

        /// <summary>
        /// Size of the output file in bytes
        /// </summary>
        public long? OutputFileSize { get; set; }

        /// <summary>
        /// Time taken to process the video
        /// </summary>
        public TimeSpan ProcessingTime { get; set; }

        /// <summary>
        /// Creates a successful result
        /// </summary>
        public static VideoCutResult Succeeded(string outputPath, TimeSpan outputDuration, long outputFileSize, TimeSpan processingTime)
        {
            return new VideoCutResult
            {
                Success = true,
                OutputPath = outputPath,
                OutputDuration = outputDuration,
                OutputFileSize = outputFileSize,
                ProcessingTime = processingTime
            };
        }

        /// <summary>
        /// Creates a failed result
        /// </summary>
        public static VideoCutResult Failed(string errorMessage)
        {
            return new VideoCutResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
