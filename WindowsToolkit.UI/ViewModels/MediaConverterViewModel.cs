using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using WindowsToolkit.Core.Interfaces;
using WindowsToolkit.Core.Models;
using WindowsToolkit.Core.Services.MediaConverter;
using WindowsToolkit.UI.Helpers;
using WindowsToolkit.UI.Models;

namespace WindowsToolkit.UI.ViewModels
{
    public class MediaConverterViewModel : ViewModelBase
    {
        private static readonly string[] VideoExtensions =
        {
            ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm", ".m4v", ".ts", ".m2ts", ".mpg", ".mpeg"
        };

        private readonly IMediaConverterService _service;

        private bool _isConverting;
        private int _totalProgress;
        private string _statusMessage = "Bestanden toevoegen om te beginnen.";
        private string _ffmpegStatus = "FFmpeg controleren...";
        private bool _ffmpegAvailable;
        private string _detectedGpu = "Niet gedetecteerd";

        // Settings
        private string _selectedCodec = "H.265";
        private string _selectedFormat = "MKV";
        private int _crf = 23;
        private bool _preferGpu = true;
        private bool _validateDuration = true;

        private CancellationTokenSource? _cts;

        public ObservableCollection<ConversionJobItem> Jobs { get; } = new();

        public string SelectedCodec
        {
            get => _selectedCodec;
            set => SetProperty(ref _selectedCodec, value);
        }

        public string SelectedFormat
        {
            get => _selectedFormat;
            set => SetProperty(ref _selectedFormat, value);
        }

        public int Crf
        {
            get => _crf;
            set => SetProperty(ref _crf, Math.Clamp(value, 0, 51));
        }

        public bool PreferGpu
        {
            get => _preferGpu;
            set => SetProperty(ref _preferGpu, value);
        }

        public bool ValidateDuration
        {
            get => _validateDuration;
            set => SetProperty(ref _validateDuration, value);
        }

        public bool IsConverting
        {
            get => _isConverting;
            set
            {
                SetProperty(ref _isConverting, value);
                OnPropertyChanged(nameof(IsNotConverting));
            }
        }

        public bool IsNotConverting => !_isConverting;

        public int TotalProgress
        {
            get => _totalProgress;
            set => SetProperty(ref _totalProgress, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public string FFmpegStatus
        {
            get => _ffmpegStatus;
            set => SetProperty(ref _ffmpegStatus, value);
        }

        public bool FFmpegAvailable
        {
            get => _ffmpegAvailable;
            set => SetProperty(ref _ffmpegAvailable, value);
        }

        public string DetectedGpu
        {
            get => _detectedGpu;
            set => SetProperty(ref _detectedGpu, value);
        }

        // Commands
        public ICommand AddFilesCommand { get; }
        public ICommand AddFolderCommand { get; }
        public ICommand RemoveSelectedCommand { get; }
        public ICommand ClearCompletedCommand { get; }
        public ICommand ConvertAllCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand OpenOutputFolderCommand { get; }

        public MediaConverterViewModel()
        {
            _service = new MediaConverterService();

            AddFilesCommand = new RelayCommand(AddFiles, () => IsNotConverting);
            AddFolderCommand = new RelayCommand(AddFolder, () => IsNotConverting);
            RemoveSelectedCommand = new RelayCommand(RemoveSelected, () => IsNotConverting && Jobs.Any());
            ClearCompletedCommand = new RelayCommand(ClearCompleted, () => Jobs.Any(j => j.Status == ConversionStatus.Done || j.Status == ConversionStatus.Failed));
            ConvertAllCommand = new RelayCommand(async () => await ConvertAllAsync(), () => IsNotConverting && Jobs.Any(j => j.Status == ConversionStatus.Pending));
            CancelCommand = new RelayCommand(Cancel, () => IsConverting);
            OpenOutputFolderCommand = new RelayCommand(OpenOutputFolder);

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            FFmpegStatus = "FFmpeg controleren...";
            FFmpegAvailable = await _service.IsFFmpegAvailableAsync();

            if (FFmpegAvailable)
            {
                var path = _service.GetFFmpegPath() ?? "ffmpeg";
                FFmpegStatus = $"FFmpeg gevonden: {path}";
                await DetectGpuAsync();
            }
            else
            {
                FFmpegStatus = "FFmpeg niet gevonden. Installeer FFmpeg en voeg het toe aan PATH.";
            }
        }

        private async Task DetectGpuAsync()
        {
            var codec = ParseCodec(SelectedCodec);
            var encoders = (await _service.DetectGpuEncodersAsync(codec)).ToList();
            DetectedGpu = encoders.Count > 0
                ? string.Join(", ", encoders)
                : "Geen GPU-encoder gevonden (CPU wordt gebruikt)";
        }

        private void AddFiles()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Videobestanden toevoegen",
                Multiselect = true,
                Filter = "Videobestanden|*.mp4;*.mkv;*.avi;*.mov;*.wmv;*.flv;*.webm;*.m4v;*.ts;*.m2ts;*.mpg;*.mpeg|Alle bestanden|*.*"
            };

            if (dialog.ShowDialog() != true) return;

            foreach (var file in dialog.FileNames)
                AddJobIfNew(file);
        }

        private void AddFolder()
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Map selecteren"
            };

            if (dialog.ShowDialog() != true) return;

            var folder = dialog.FolderName;
            var files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories)
                .Where(f => VideoExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()));

            foreach (var file in files)
                AddJobIfNew(file);

            StatusMessage = $"{Jobs.Count} bestand(en) in wachtrij.";
        }

        private void AddJobIfNew(string filePath)
        {
            if (Jobs.Any(j => j.InputPath.Equals(filePath, StringComparison.OrdinalIgnoreCase)))
                return;

            var suffix = SelectedCodec.Replace(".", "");
            var ext = SelectedFormat.ToLowerInvariant();
            var dir = Path.GetDirectoryName(filePath) ?? string.Empty;
            var name = Path.GetFileNameWithoutExtension(filePath);
            var outputPath = Path.Combine(dir, $"{name}_{suffix}.{ext}");

            Jobs.Add(new ConversionJobItem
            {
                InputPath = filePath,
                OutputPath = outputPath
            });
        }

        private void RemoveSelected()
        {
            var toRemove = Jobs
                .Where(j => j.Status == ConversionStatus.Pending)
                .ToList();
            foreach (var job in toRemove)
                Jobs.Remove(job);
        }

        private void ClearCompleted()
        {
            var toRemove = Jobs
                .Where(j => j.Status == ConversionStatus.Done || j.Status == ConversionStatus.Failed)
                .ToList();
            foreach (var job in toRemove)
                Jobs.Remove(job);
        }

        private async Task ConvertAllAsync()
        {
            if (!FFmpegAvailable)
            {
                MessageBox.Show(
                    "FFmpeg is niet beschikbaar. Installeer FFmpeg en start de app opnieuw.",
                    "FFmpeg niet gevonden",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            IsConverting = true;
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;

            var settings = BuildSettings();
            var pending = Jobs.Where(j => j.Status == ConversionStatus.Pending).ToList();
            var total = pending.Count;
            var done = 0;

            TotalProgress = 0;
            StatusMessage = $"Converteren... 0 / {total}";

            // Update output paths based on current settings before starting
            foreach (var job in pending)
            {
                var suffix = SelectedCodec.Replace(".", "");
                var ext = SelectedFormat.ToLowerInvariant();
                var dir = Path.GetDirectoryName(job.InputPath) ?? string.Empty;
                var name = Path.GetFileNameWithoutExtension(job.InputPath);
                job.OutputPath = Path.Combine(dir, $"{name}_{suffix}.{ext}");
            }

            foreach (var job in pending)
            {
                if (ct.IsCancellationRequested) break;

                job.Status = ConversionStatus.Running;
                job.StatusMessage = "Bezig...";
                job.Progress = 0;

                var jobProgress = new Progress<int>(pct =>
                {
                    Application.Current.Dispatcher.Invoke(() => job.Progress = pct);
                });

                try
                {
                    var result = await _service.ConvertAsync(
                        job.InputPath,
                        job.OutputPath,
                        settings,
                        jobProgress,
                        ct);

                    if (result.Success)
                    {
                        job.Status = ConversionStatus.Done;
                        job.Progress = 100;
                        var mismatch = result.DurationMismatch ? " (duratie-afwijking!)" : string.Empty;
                        job.StatusMessage = $"Klaar in {result.ProcessingTime:mm\\:ss}{mismatch}";
                        job.InputDurationSec = result.InputDurationSec;
                        job.OutputDurationSec = result.OutputDurationSec;
                    }
                    else
                    {
                        job.Status = ConversionStatus.Failed;
                        job.StatusMessage = result.ErrorMessage;
                    }
                }
                catch (OperationCanceledException)
                {
                    job.Status = ConversionStatus.Pending;
                    job.StatusMessage = "Geannuleerd";
                    break;
                }

                done++;
                TotalProgress = total > 0 ? (done * 100) / total : 0;
                StatusMessage = $"Converteren... {done} / {total}";
            }

            IsConverting = false;
            _cts?.Dispose();
            _cts = null;

            var failCount = Jobs.Count(j => j.Status == ConversionStatus.Failed);
            var doneCount = Jobs.Count(j => j.Status == ConversionStatus.Done);
            StatusMessage = $"Gereed. {doneCount} geslaagd, {failCount} mislukt.";
            TotalProgress = 100;
        }

        private void Cancel()
        {
            _cts?.Cancel();
            StatusMessage = "Annuleren...";
        }

        private void OpenOutputFolder()
        {
            var firstDone = Jobs.FirstOrDefault(j => j.Status == ConversionStatus.Done);
            var folder = firstDone != null
                ? Path.GetDirectoryName(firstDone.OutputPath)
                : null;

            if (folder == null)
            {
                var firstJob = Jobs.FirstOrDefault();
                folder = firstJob != null ? Path.GetDirectoryName(firstJob.InputPath) : null;
            }

            if (folder != null && Directory.Exists(folder))
                Process.Start("explorer.exe", folder);
        }

        private ConversionSettings BuildSettings()
        {
            return new ConversionSettings
            {
                Codec = ParseCodec(SelectedCodec),
                Format = SelectedFormat.Equals("MKV", StringComparison.OrdinalIgnoreCase)
                    ? OutputFormat.MKV
                    : OutputFormat.MP4,
                Crf = Crf,
                PreferGpu = PreferGpu,
                ValidateDuration = ValidateDuration
            };
        }

        private static VideoCodec ParseCodec(string codec) => codec switch
        {
            "H.265" => VideoCodec.H265,
            "H.264" => VideoCodec.H264,
            "AV1"   => VideoCodec.AV1,
            _ => VideoCodec.H265
        };
    }
}
