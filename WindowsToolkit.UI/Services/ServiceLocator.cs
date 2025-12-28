using WindowsToolkit.Core.Interfaces;
using WindowsToolkit.Core.Services.PackageManager;
using WindowsToolkit.Core.Services.DownloadsCleanup;

namespace WindowsToolkit.UI.Services
{
    /// <summary>
    /// Simple service locator for accessing application services
    /// </summary>
    public class ServiceLocator
    {
        private static ServiceLocator? _instance;
        private static readonly object _lock = new object();

        private readonly IPackageManagerService _packageManagerService;
        private readonly IDownloadsCleanupService _downloadsCleanupService;

        private ServiceLocator()
        {
            // Initialize services
            _packageManagerService = new PackageManagerService();
            _downloadsCleanupService = new DownloadsCleanupService();
        }

        public static ServiceLocator Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ServiceLocator();
                        }
                    }
                }
                return _instance;
            }
        }

        public IPackageManagerService PackageManagerService => _packageManagerService;
        public IDownloadsCleanupService DownloadsCleanupService => _downloadsCleanupService;

        /// <summary>
        /// For testing purposes - allows resetting the singleton
        /// </summary>
        internal static void Reset()
        {
            lock (_lock)
            {
                _instance = null;
            }
        }
    }
}
