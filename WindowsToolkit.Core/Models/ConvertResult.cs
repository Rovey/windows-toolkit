namespace WindowsToolkit.Core.Models
{
    public class ConvertResult
    {
        public bool Success { get; set; }
        public string InputPath { get; set; } = string.Empty;
        public string OutputPath { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public double InputDurationSec { get; set; }
        public double OutputDurationSec { get; set; }
        public bool DurationMismatch { get; set; }
        public TimeSpan ProcessingTime { get; set; }
    }
}
