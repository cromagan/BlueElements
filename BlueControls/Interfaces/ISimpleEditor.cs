// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.Enums;
using System.Windows.Forms;

namespace BlueControls.Interfaces;

/// <summary>
/// Wird verwendet, wenn eine Klasse bearbeitbar ist, es sie selbst die Properties ausgeben kann.
/// Ein allgemeiner Dialog, eingeschränkte Design-Möglichkeiten.
/// Mit der Extension kann mit DoForm die Properties in ein Steuerelement generiert werden.
/// </summary>
public interface ISimpleEditor {

    #region Events

    event EventHandler? DoUpdateSideOptionMenu;

    #endregion

    #region Properties

    string Description { get; }

    #endregion

    #region Methods

    List<GenericControl> GetProperties(int widthOfControl);

    #endregion
}

public static class SimpleEditorExtension {

    #region Methods

    public static void DoForm(this ISimpleEditor? element, ScrollPanel control) {

        #region Controls leeren

        var oldControls = new List<System.Windows.Forms.Control>();
        foreach (System.Windows.Forms.Control c in control.Controls) { oldControls.Add(c); }
        control.Controls.Clear();

        foreach (var c in oldControls) {
            if (c is IDisposable d) { d.Dispose(); }
        }

        #endregion

        if (element is null) { return; }
        control.OffsetX = 0;
        control.OffsetY = 0;

        control.ChildLayout = ChildLayout.StackVertical | ChildLayout.FullWidth | ChildLayout.Slider;

        var flexis = element.GetProperties(control.Width);
        if (flexis.Count == 0) { return; }

        if (element is IErrorCheckable iec && !iec.IsOk()) {
            flexis.Insert(0, new FlexiControl("<Imagecode=Warnung|16> " + iec.ErrorReason(), control.Width, false));
            flexis.Insert(0, new FlexiControl("Achtung!", control.Width, true));
        }

        if (element.Description is { Length: > 0 } desc) {
            flexis.Insert(0, new FlexiControl(desc, control.Width, false));
            flexis.Insert(0, new FlexiControl("Beschreibung:", control.Width, true));
        }

        foreach (var thisFlexi in flexis) {
            if (thisFlexi is { IsDisposed: false }) {
                control.Controls.Add(thisFlexi);
            }
        }

        control.Invalidate();
    }

    public static ScrollPanel GetControl(this ISimpleEditor? element, int widthOfControl) {
        var l = new ScrollPanel {
            Width = widthOfControl,
            Height = 100,
            Visible = true
        };

        element.DoForm(l);

        foreach (var control in l.Controls) {
            if (control is Control c) {
                l.Height = Math.Max(l.Height, c.Bottom);
            }
        }
        return l;
    }

    #endregion
}