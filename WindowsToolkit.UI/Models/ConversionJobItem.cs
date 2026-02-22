using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using WindowsToolkit.Core.Models;

namespace WindowsToolkit.UI.Models
{
    public class ConversionJobItem : INotifyPropertyChanged
    {
        private ConversionStatus _status = ConversionStatus.Pending;
        private string _statusMessage = string.Empty;
        private int _progress;

        public string InputPath { get; set; } = string.Empty;
        public string OutputPath { get; set; } = string.Empty;

        public string FileName => Path.GetFileName(InputPath);

        public double? InputDurationSec { get; set; }
        public double? OutputDurationSec { get; set; }

        public ConversionStatus Status
        {
            get => _status;
            set
            {
                if (_status == value) return;
                _status = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusIcon));
                OnPropertyChanged(nameof(StatusColor));
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(IsRunning));
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public int Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProgressText));
            }
        }

        public string StatusIcon => Status switch
        {
            ConversionStatus.Pending => "⏳",
            ConversionStatus.Running => "▶",
            ConversionStatus.Done    => "✅",
            ConversionStatus.Failed  => "❌",
            ConversionStatus.Skipped => "⏭",
            _ => "❓"
        };

        public string StatusText => Status switch
        {
            ConversionStatus.Pending => "Wachten",
            ConversionStatus.Running => "Bezig",
            ConversionStatus.Done    => "Klaar",
            ConversionStatus.Failed  => "Mislukt",
            ConversionStatus.Skipped => "Overgeslagen",
            _ => Status.ToString()
        };

        public string StatusColor => Status switch
        {
            ConversionStatus.Pending => "#6C757D",
            ConversionStatus.Running => "#007BFF",
            ConversionStatus.Done    => "#28A745",
            ConversionStatus.Failed  => "#DC3545",
            ConversionStatus.Skipped => "#FFC107",
            _ => "#6C757D"
        };

        public string ProgressText => Status == ConversionStatus.Running ? $"{Progress}%" : string.Empty;

        public bool IsRunning => Status == ConversionStatus.Running;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
