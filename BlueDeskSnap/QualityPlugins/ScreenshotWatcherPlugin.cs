// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueDeskSnap.QualityPlugins;

/// <summary>
/// Öffnet neue Screenshots aus dem Screenshot-Ordner automatisch in BluePaint.
/// </summary>
public sealed class ScreenshotWatcherPlugin : QualityPlugin {

    #region Fields

    private static readonly List<string> ImageExtensions = ["png", "jpg", "jpeg", "bmp"];

    private readonly List<string> _known = [];

    private bool _initialized;

    #endregion

    #region Properties

    public override string KeyName => "ScreenshotWatcher";

    public override string Name => "Screenshot-Monitor";

    public override string QuickInfo => "Öffnet neue Screenshots aus dem Screenshot-Ordner automatisch in BluePaint.";

    public override ImageCode Symbol => ImageCode.Kamera;

    #endregion

    #region Methods

    protected override void ExecuteTick() {
        var folder = FolderOf();
        if (!Directory.Exists(folder)) { return; }

        var images = Directory.GetFiles(folder).Where(ImageFile).Order().ToList();

        if (!_initialized) {
            // Der beim Einschalten vorhandene Bestand wird nur als bekannt gemerkt, nicht nachträglich geöffnet.
            _known.AddRange(images);
            _initialized = true;
            return;
        }

        foreach (var image in images) {
            if (_known.Contains(image)) { continue; }
            _known.Add(image);
            OpenInBluePaint(image);
        }
    }

    /// <summary>
    /// Der Screenshot-Ordner, in dem Windows standardmäßig Snips und Screenshots ablegt.
    /// </summary>
    private static string FolderOf() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Screenshots");

    private static bool ImageFile(string file) => ImageExtensions.Contains(file.FileSuffix().ToLowerInvariant());

    private static void OpenInBluePaint(string filename) {
        var frm = new BluePaint.MainWindow(true);
        frm.LoadFromDisk(filename);
        FormManager.RegisterForm(frm);
        frm.Show();
    }

    #endregion
}
