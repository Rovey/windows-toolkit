using System;
using System.Windows;
using System.Windows.Controls;

namespace WindowsToolkit.UI.Views
{
    /// <summary>
    /// Interaction logic for VideoCutterView.xaml
    /// </summary>
    public partial class VideoCutterView : UserControl
    {
        public VideoCutterView()
        {
            InitializeComponent();

            // Koppel events na initialisatie
            this.Loaded += VideoCutterView_Loaded;
        }

        private void VideoCutterView_Loaded(object sender, RoutedEventArgs e)
        {
            if (VideoPreview != null)
            {
                VideoPreview.MediaOpened += VideoPreview_MediaOpened;
                VideoPreview.MediaEnded += VideoPreview_MediaEnded;
            }

            var vm = DataContext as ViewModels.VideoCutterViewModel;
            if (vm != null)
            {
                // Sync play/pause
                vm.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(vm.IsPlaying))
                    {
                        if (vm.IsPlaying)
                        {
                            VideoPreview?.Play();
                        }
                        else
                        {
                            VideoPreview?.Pause();
                        }
                    }
                    else if (args.PropertyName == nameof(vm.PreviewPositionSeconds))
                    {
                        if (VideoPreview != null && Math.Abs(VideoPreview.Position.TotalSeconds - vm.PreviewPositionSeconds) > 0.5)
                        {
                            VideoPreview.Position = TimeSpan.FromSeconds(vm.PreviewPositionSeconds);
                        }
                    }
                };

                // Sync slider als video speelt
                var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
                timer.Tick += (s, args) =>
                {
                    if (vm.IsPlaying && VideoPreview != null && VideoPreview.NaturalDuration.HasTimeSpan)
                    {
                        vm.PreviewPositionSeconds = VideoPreview.Position.TotalSeconds;
                    }
                };
                timer.Start();
            }
        }

        private void VideoPreview_MediaOpened(object sender, RoutedEventArgs e)
        {
            // Optioneel: auto-play bij openen
        }

        private void VideoPreview_MediaEnded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.VideoCutterViewModel;
            if (vm != null)
            {
                vm.IsPlaying = false;
            }
        }
    }
}
