namespace WindowsToolkit.Core.Models
{
    /// <summary>
    /// File categories for organization
    /// </summary>
    public enum FileCategory
    {
        All,
        Installers,      // .exe, .msi
        Images,          // .jpg, .jpeg, .png, .gif, .bmp, .svg, .webp
        Documents,       // .pdf, .doc, .docx, .txt, .rtf
        Spreadsheets,    // .xls, .xlsx, .csv
        Videos,          // .mp4, .avi, .mkv, .mov, .wmv, .flv
        Audio,           // .mp3, .wav, .flac, .aac, .m4a
        Archives         // .zip, .rar, .7z, .tar, .gz
    }
}
