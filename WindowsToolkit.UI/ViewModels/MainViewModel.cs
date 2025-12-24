using System.Windows.Input;
using WindowsToolkit.UI.Helpers;
using WindowsToolkit.UI.Views;

namespace WindowsToolkit.UI.ViewModels
{
    /// <summary>
    /// Main ViewModel for the application
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private object? _currentView;
        private string _currentPageTitle = "Package Manager";

        public MainViewModel()
        {
            // Initialize commands
            NavigateToPackageManagerCommand = new RelayCommand(NavigateToPackageManager);
            NavigateToConfigSyncCommand = new RelayCommand(NavigateToConfigSync);
            NavigateToMediaConverterCommand = new RelayCommand(NavigateToMediaConverter);
            NavigateToVideoToolsCommand = new RelayCommand(NavigateToVideoTools);
            NavigateToUtilitiesCommand = new RelayCommand(NavigateToUtilities);
            NavigateToAboutCommand = new RelayCommand(NavigateToAbout);

            // Set default view
            NavigateToPackageManager();
        }

        /// <summary>
        /// Currently displayed view
        /// </summary>
        public object? CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        /// <summary>
        /// Title of the current page
        /// </summary>
        public string CurrentPageTitle
        {
            get => _currentPageTitle;
            set => SetProperty(ref _currentPageTitle, value);
        }

        // Navigation Commands
        public ICommand NavigateToPackageManagerCommand { get; }
        public ICommand NavigateToConfigSyncCommand { get; }
        public ICommand NavigateToMediaConverterCommand { get; }
        public ICommand NavigateToVideoToolsCommand { get; }
        public ICommand NavigateToUtilitiesCommand { get; }
        public ICommand NavigateToAboutCommand { get; }

        private void NavigateToPackageManager()
        {
            CurrentPageTitle = "Package Manager";
            var view = new PackageManagerView
            {
                DataContext = new PackageManagerViewModel()
            };
            CurrentView = view;
        }

        private void NavigateToConfigSync()
        {
            CurrentPageTitle = "Configuration Sync";
            // TODO: Implement ConfigSyncViewModel
            CurrentView = CreatePlaceholderView("Configuration Sync");
        }

        private void NavigateToMediaConverter()
        {
            CurrentPageTitle = "Media Converter";
            // TODO: Implement MediaConverterViewModel
            CurrentView = CreatePlaceholderView("Media Converter");
        }

        private void NavigateToVideoTools()
        {
            CurrentPageTitle = "Video Tools";
            // TODO: Implement VideoToolsViewModel
            CurrentView = CreatePlaceholderView("Video Tools");
        }

        private void NavigateToUtilities()
        {
            CurrentPageTitle = "Utilities";
            // TODO: Implement UtilitiesViewModel
            CurrentView = CreatePlaceholderView("Utilities");
        }

        private void NavigateToAbout()
        {
            CurrentPageTitle = "About";
            // TODO: Implement AboutViewModel
            CurrentView = CreatePlaceholderView("About");
        }

        private object CreatePlaceholderView(string title)
        {
            return new PlaceholderViewModel { Title = title };
        }
    }

    /// <summary>
    /// Temporary placeholder ViewModel for views that haven't been implemented yet
    /// </summary>
    public class PlaceholderViewModel : ViewModelBase
    {
        private string _title = "Coming Soon";

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Message => $"{Title} functionality will be implemented soon.";
    }
}
