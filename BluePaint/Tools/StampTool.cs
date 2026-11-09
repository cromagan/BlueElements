// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.EventArgs;
using BlueTable.Classes;
using System.Windows.Forms;

namespace BluePaint;

/// <summary>
/// Stampelt das ausgewählte Symbol in variabler Größe in das Bild.
/// </summary>
public partial class StampTool {

    #region Fields

    private bool _isStamping;

    private int _size = 32;

    private string _symbol = string.Empty;

    #endregion

    #region Constructors

    public StampTool() : base() => InitializeComponent();

    #endregion

    #region Methods

    /// <summary>
    /// Zeichnet eine Vorschau des Stempels an der Mausposition.
    /// </summary>
    public override void DrawOverlay(Graphics gr, float zoom, int offsetX, int offsetY, TrimmedCanvasMouseEventArgs? mouseDown, TrimmedCanvasMouseEventArgs? mouseCurrent) {
        if (mouseCurrent is null) { return; }
        if (CurrentQuickImage() is not { IsError: false } qi) { return; }
        var left = (mouseCurrent.TrimmedCanvasX - _size / 2f).CanvasToControl(zoom, offsetX);
        var top = (mouseCurrent.TrimmedCanvasY - _size / 2f).CanvasToControl(zoom, offsetY);
        gr.DrawImage(qi, left, top, _size * zoom, _size * zoom);
    }

    public override void MouseDown(TrimmedCanvasMouseEventArgs e, Bitmap? originalPic) {
        if (e.Button != MouseButtons.Left) { return; }

        OnForceUndoSaving();
        _isStamping = true;
        Stempeln(originalPic, e.TrimmedCanvasX, e.TrimmedCanvasY);
        OnDoInvalidate();
    }

    /// <summary>
    /// Bei gedrückter Maustaste wird an jeder Position gestempelt, sonst nur die Vorschau aktualisiert.
    /// </summary>
    public override void MouseMove(TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs e, Bitmap? originalPic) {
        if (_isStamping && e.MouseCurrent.Button == MouseButtons.Left) {
            Stempeln(originalPic, e.MouseCurrent.TrimmedCanvasX, e.MouseCurrent.TrimmedCanvasY);
        }

        OnDoInvalidate();
    }

    public override void MouseUp(TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs e, Bitmap? originalPic) {
        _isStamping = false;
        OnDoInvalidate();
    }

    public override void OnToolChanging() => _isStamping = false;

    public override string ReadableText() => "Stempel";

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Stern, 16);

    /// <summary>
    /// Füllt die Symbol-Liste: eingebaute Symbole, Symbole aus den Symbol-Ordnern und Asset-Ordner der geladenen Tabellen.
    /// </summary>
    public override void ToolFirstShown() {
        lstSymbole.ItemClear();

        var l = new List<ListItem>();
        foreach (var s in AlleSymbole()) {
            l.Add(ItemOf(s, s, QuickImage.Get(s, 16)));
        }

        lstSymbole.ItemAddRange(l);
    }

    /// <summary>
    /// Sammelt alle verfügbaren Symbolnamen.
    /// </summary>
    private static List<string> AlleSymbole() {
        var l = new List<string>();

        l.AddRange(QuickImage.Images());
        l.AddRange(QuickImage.UserSymbols());

        // Symbole aus den Asset-Ordnern aller geladenen Tabellen
        lock (Table.AllFilesLocker) {
            foreach (var tb in Table.AllInstances()) {
                if (tb.IsDisposed) { continue; }
                if (tb.AssetFolderWhole() is not { Length: > 0 } p || !p.IsValidFilePath()) { continue; }
                foreach (var f in IO.GetFiles(p, "*.png", System.IO.SearchOption.TopDirectoryOnly)) {
                    l.Add(f.FileNameWithoutSuffix());
                }
            }
        }

        return l.SortedDistinctList();
    }

    /// <summary>
    /// Liefert den aktuellen Stempel in der gewählten Größe.
    /// </summary>
    private QuickImage? CurrentQuickImage() => string.IsNullOrEmpty(_symbol) ? null : QuickImage.Get(_symbol, _size);

    /// <summary>
    /// Drückt das Symbol mittig an der Position ins Bild.
    /// </summary>
    private void Stempeln(Bitmap? pic, int x, int y) {
        if (pic is null) { return; }
        if (CurrentQuickImage() is not { IsError: false } qi) { return; }

        using var g = Graphics.FromImage(pic);
        g.DrawImage(qi, x - _size / 2f, y - _size / 2f, _size, _size);
    }

    private void LstSymbole_ItemClicked(object sender, ListItemEventArgs e) {
        _symbol = e.Item.KeyName;
        lstSymbole.Check(e.Item);
        OnDoInvalidate();
    }

    private void SldSize_ValueChanged(object sender, EventArgs e) {
        _size = (int)Math.Round(sldSize.Value, MidpointRounding.AwayFromZero);
        capSize.Text = _size.ToString1();
        OnDoInvalidate();
    }

    private void BtnFilterDel_Click(object sender, EventArgs e) => txbSuche.Text = string.Empty;

    private void TxbSuche_TextChanged(object sender, EventArgs e) {
        lstSymbole.FilterText = txbSuche.Text;
        btnFilterDel.Enabled = Enabled && !string.IsNullOrEmpty(txbSuche.Text);
    }

    #endregion
}
