// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using System.Windows.Forms;

namespace BlueControls.Controls;

public class TabControl : AbstractTabControl {

    #region Constructors

    public TabControl() : base() => BackColor = Skin.Color_Back(Design.TabStrip_Body, States.Standard);

    #endregion

    #region Events

    public event EventHandler<ControlEventArgs>? ChildGotFocus;

    #endregion

    #region Properties

    public override sealed Color BackColor {
        get => base.BackColor;
        set => base.BackColor = value;
    }

    #endregion

    #region Methods

    protected override void OnControlAdded(ControlEventArgs e) {
        base.OnControlAdded(e);
        if (e.Control is TabPage tab) {
            tab.BackColor = Skin.Color_Back(Design.TabStrip_Body, States.Standard);
            Invalidate();
            tab.ControlAdded += AddedControlInTabPage;
            tab.ControlRemoved += RemovedControlInTabPage;
        }
    }

    protected override void OnControlRemoved(ControlEventArgs e) {
        base.OnControlRemoved(e);
        if (e.Control is TabPage tab) {
            tab.ControlAdded -= AddedControlInTabPage;
            tab.ControlRemoved -= RemovedControlInTabPage;
        }
    }

    protected override void OnEnabledChanged(System.EventArgs e) {
        base.OnEnabledChanged(e);
        Invalidate(); // Erzwingt Neuzeichnen des TabControls im neuen Status (z.B. Grayed/Disabled)

        // Child-TabPages explizit informieren, falls diese den Status nicht automatisch erben
        foreach (Control control in Controls) {
            if (control is TabPage tab) {
                tab.Invalidate();
            }
        }
    }

    protected override void OnPaint(PaintEventArgs e) => DrawControl(e, Design.TabStrip_Back);

    private void AddedControlInTabPage(object? sender, ControlEventArgs e) {
        if (e.Control is TabPage) { return; }

        if (e.Control is TabControl sfc) {
            sfc.ChildGotFocus += Sfc_ChildGotFocus;
        }
        e.Control.GotFocus += ControlInTabPage_GotFocus;
    }

    private void ControlInTabPage_GotFocus(object? sender, System.EventArgs e) {
        if (sender is not Control c) { return; }
        OnChildGotFocus(new ControlEventArgs(c));
    }

    private void OnChildGotFocus(ControlEventArgs e) => ChildGotFocus?.Invoke(this, e);

    private void RemovedControlInTabPage(object? sender, ControlEventArgs e) {
        if (e.Control is TabPage) { return; }

        if (e.Control is TabControl sfc) {
            sfc.ChildGotFocus -= Sfc_ChildGotFocus;
        }
        e.Control.GotFocus -= ControlInTabPage_GotFocus;
    }

    private void Sfc_ChildGotFocus(object? sender, ControlEventArgs e) => OnChildGotFocus(e);

    #endregion
}