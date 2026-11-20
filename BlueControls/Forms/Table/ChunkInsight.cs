// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueTable.EventArgs;
using System.IO;
using static BlueBasics.ClassesStatic.Develop;
using static BlueBasics.ClassesStatic.IO;

namespace BlueControls.BlueTableDialogs;

public sealed partial class ChunkInsight : FormWithStatusBar {

    #region Constructors

    public ChunkInsight() : base() =>
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();

    #endregion

    #region Methods

    [StandaloneInfo("Fragment-Insight", ImageCode.Puzzle, "Admin", "Rohe Fragment-Sätze als Tabelle anzeigen", 900)]
    public static System.Windows.Forms.Form Start() => new ChunkInsight();

    private void FragmentInsight_VisibleRowsChanged(object sender, TableEventArgs e) {
        if (InvokeRequired) {
            Invoke(new Action(() => FragmentInsight_VisibleRowsChanged(sender, e)));
            return;
        }

        capZeilen2.Text = $"<imagecode=Information|16> {LanguageTool.DoTranslate("Einzigartige Zeilen:")} {e.Table?.Row.Count ?? 0} {LanguageTool.DoTranslate("St.")}";

        capZeilen2.Refresh();
    }

    private void btnOeffnen_Click(object sender, System.EventArgs e) {
        FormManager.SaveAllFiles();

        OpenTab.ShowDialog();
        if (string.IsNullOrEmpty(OpenTab.FileName)) { return; }

        LoadChunk(OpenTab.FileName);
    }

    private void LoadChunk(string filename) {
        if (!FileExists(filename)) { return; }

        var data = ReadAllBytes(filename, 10);

        if (data.IsFailed || data.Value is not byte[] content) {
            Notification.Show($"Fragment-Daten konnten nicht geladen werden:<br>{filename}<br><br>{data.FailedReason}", ImageCode.Warnung);
            return;
        }

        if (content.IsZipped()) { content = content.UnzipIt() ?? []; }

        capInfo.Text = QuickImage.Get(ImageCode.Puzzle, 16).HTMLCode + " " +
                       filename.FileNameWithSuffix() +
                       "  |  Key: <b>" + filename.FileNameWithoutSuffix() + "</b>" +
                       "  |  Größe (ungepackt): <b>" + (content.Length / 1024 / 1024).ToString($"N2", OriginCultureInfo) + " mb</b>";

        Text = "Fragment-Insight - " + filename.FileNameWithSuffix();

        fragmentInsight.InputItem = new MemoryStream(content);
    }

    #endregion
}
