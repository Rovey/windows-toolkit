using WindowsToolkit.Core.Interfaces;
using WindowsToolkit.Core.Services.PackageManager;
using WindowsToolkit.Core.Services.DownloadsCleanup;
using WindowsToolkit.Core.Services.VideoEditor;
using WindowsToolkit.Core.Services.MediaConverter;

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
        private readonly IVideoCutterService _videoCutterService;
        private readonly IMediaConverterService _mediaConverterService;

        private ServiceLocator()
        {
            // Initialize services
            _packageManagerService = new PackageManagerService();
            _downloadsCleanupService = new DownloadsCleanupService();
            _videoCutterService = new VideoCutterService();
            _mediaConverterService = new MediaConverterService();
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
        public IVideoCutterService VideoCutterService => _videoCutterService;
        public IMediaConverterService MediaConverterService => _mediaConverterService;

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
