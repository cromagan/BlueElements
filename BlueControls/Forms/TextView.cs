// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueControls.Forms;

public partial class TextView : Form {

    #region Constructors

    public TextView() : this(null, null) { }

    public TextView(string? text) : this(text, null) { }

    public TextView(string? text, string? windowCaption) : base() {
        InitializeComponent();

        txbText.Text = text ?? string.Empty;

        if (!string.IsNullOrEmpty(windowCaption)) { Text = windowCaption; }
    }

    #endregion

    #region Properties

    /// <summary>
    /// Der angezeigte bzw. bearbeitbare Text.
    /// </summary>
    public string TextContent {
        get => txbText.Text;
        set => txbText.Text = value;
    }

    #endregion

    #region Methods

    private void btnCopy_Click(object sender, System.EventArgs e) {
        if (CopytoClipboard(txbText.Text)) {
            QuickNote.Show(NoteSymbols.Ok, "Kopiert");
        } else {
            QuickNote.Show(NoteSymbols.Critical, "Fehlgeschlagen");
        }
    }

    private void btnSortDistinct_Click(object sender, System.EventArgs e) => txbText.Text = string.Join('\r', txbText.Text.SplitAndCutByCr().SortedDistinctList());

    #endregion
}
