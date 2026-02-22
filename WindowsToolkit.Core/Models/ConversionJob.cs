namespace WindowsToolkit.Core.Models
{
    public enum ConversionStatus
    {
        Pending,
        Running,
        Done,
        Failed,
        Skipped
    }

    public class ConversionJob
    {
        public string InputPath { get; set; } = string.Empty;
        public string OutputPath { get; set; } = string.Empty;
        public ConversionStatus Status { get; set; } = ConversionStatus.Pending;
        public string StatusMessage { get; set; } = string.Empty;
        public int Progress { get; set; }
        public double? InputDurationSec { get; set; }
        public double? OutputDurationSec { get; set; }
    }
}
