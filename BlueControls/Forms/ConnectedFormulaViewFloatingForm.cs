// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using System.Windows.Forms;

namespace BlueControls.Forms;

public partial class ConnectedFormulaViewFloatingForm : Form {

    #region Fields

    private static ConnectedFormulaViewFloatingForm? _active;

    #endregion

    #region Constructors

    private ConnectedFormulaViewFloatingForm() => InitializeComponent();

    #endregion

    #region Methods

    /// <summary>
    /// Zeigt den Inhalt des übergebenen ConnectedFormulaView als freie, nicht-modale Toolbar
    /// ohne OK/Abbrechen, exakt an dessen Bildschirm-Position und -Größe.
    /// </summary>
    public static void ShowFor(ConnectedFormulaView source) {
        if (source is not { IsDisposed: false }) { return; }
        if (source.Page is null) { return; }
        if (source.RowSingleOrNull() is not { IsDisposed: false } row) { return; }

        var f = _active is { IsDisposed: false } active ? active : new ConnectedFormulaViewFloatingForm();
        _active = f;

        f.CFormula.Page = source.Page;
        f.CFormula.Mode = source.Mode;
        f.CFormula.GroupBoxStyle = source.GroupBoxStyle;
        f.CFormula.Text = source.Text;
        f.CFormula.SetToRow(row);

        f.Text = source.Text is { Length: > 0 } t ? t : "Formularansicht";
        f.StartPosition = FormStartPosition.Manual;
        f.ClientSize = source.Size;
        f.PositionTo(source.RectangleToScreen(new Rectangle(Point.Empty, source.Size)));

        if (f.Visible) {
            f.Activate();
        } else {
            f.Show();
        }
    }

    private void PositionTo(Rectangle target) {
        _ = Handle; // Handle erzeugen, um den Rahmen-Versatz korrekt zu messen
        var clientOrigin = PointToScreen(Point.Empty);
        Location = new Point(Left + target.Left - clientOrigin.X, Top + target.Top - clientOrigin.Y);
    }

    #endregion
}
