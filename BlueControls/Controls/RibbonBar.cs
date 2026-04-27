// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using System.Windows.Forms;

namespace BlueControls.Controls;

public sealed class RibbonBar : AbstractTabControl {

    #region Constructors

    public RibbonBar() : base() {
        Height = 110;
        SendToBack();
        Dock = DockStyle.Top;
        var state = Enabled ? States.Standard : States.Standard_Disabled;
        BackColor = Skin.Color_Back(Design.RibbonBar_Body, state);
    }

    #endregion

    #region Methods

    protected override void OnControlAdded(ControlEventArgs e) {
        base.OnControlAdded(e);
        if (e.Control is not TabPage tp) {
            return;
        }

        var state = Enabled ? States.Standard : States.Standard_Disabled;
        tp.BackColor = Skin.Color_Back(Design.RibbonBar_Body, state);
        Invalidate();
    }

    // Korrektur: Hintergrundfarben bei Statusänderung anpassen
    protected override void OnEnabledChanged(System.EventArgs e) {
        base.OnEnabledChanged(e);
        var state = Enabled ? States.Standard : States.Standard_Disabled;

        BackColor = Skin.Color_Back(Design.RibbonBar_Body, state);

        foreach (Control control in Controls) {
            if (control is TabPage tp) {
                tp.BackColor = Skin.Color_Back(Design.RibbonBar_Body, state);
            }
        }

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e) => DrawControl(e, Design.RibbonBar_Back);

    #endregion
}