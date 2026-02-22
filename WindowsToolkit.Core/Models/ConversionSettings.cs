namespace WindowsToolkit.Core.Models
{
    public enum VideoCodec
    {
        H265,
        H264,
        AV1
    }

    public enum OutputFormat
    {
        MKV,
        MP4
    }

    public class ConversionSettings
    {
        public VideoCodec Codec { get; set; } = VideoCodec.H265;
        public OutputFormat Format { get; set; } = OutputFormat.MKV;
        public int Crf { get; set; } = 23;
        public bool PreferGpu { get; set; } = true;
        public bool ValidateDuration { get; set; } = true;
    }
}
