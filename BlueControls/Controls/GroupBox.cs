// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Windows.Forms;

namespace BlueControls.Controls;

[ToolboxBitmap(typeof(System.Windows.Forms.GroupBox))]
public partial class GroupBox : System.Windows.Forms.GroupBox {

    #region Constructors

    public GroupBox() : base() => SetStandardValues();

    #endregion

    #region Properties

    [DefaultValue(GroupBoxStyle.Normal)]
    public GroupBoxStyle GroupBoxStyle {
        get;
        set {
            if (field == value) { return; }
            field = value;
            SetStandardValues();
            Invalidate();
        }
    } = GroupBoxStyle.Normal;

    /// <summary>
    /// RoundRect- und Dropdown-Stile zeichnen keine Caption. Deshalb wird oben
    /// kein Platz für eine Überschrift reserviert — das Padding gilt auf allen
    /// Seiten gleich.
    /// </summary>
    public override Rectangle DisplayRectangle {
        get {
            if (GroupBoxStyle is not (GroupBoxStyle.RoundRect or GroupBoxStyle.DropdownMenu)) { return base.DisplayRectangle; }

            var p = Padding;
            return new Rectangle(p.Left, p.Top, Math.Max(Width - p.Horizontal, 0), Math.Max(Height - p.Vertical, 0));
        }
    }

    #endregion

    #region Methods

    public static void DrawGroupBox(Control c, Graphics gr, States state, GroupBoxStyle _groupBoxStyle, string caption) {
        var r = new Rectangle(0, 0, c.Width, c.Height);
        gr.Clear(c.BackColor);

        switch (_groupBoxStyle) {
            case GroupBoxStyle.RibbonBar:
                Skin.Draw_Border(gr, Design.RibbonBar_Frame, state, r);
                if (!string.IsNullOrEmpty(caption)) {
                    var bottomTxt = new Rectangle(0, 0, c.Width, c.Height + 2);
                    Skin.Draw_FormatedText(gr, caption, null, Alignment.Bottom_HorizontalCenter, bottomTxt, Design.RibbonBar_Frame, state, c, false, true);
                }
                break;

            case GroupBoxStyle.Normal:
                if (c.Height > 33) {
                    Skin.Draw_Border(gr, Design.GroupBox, state, r);
                    if (!string.IsNullOrEmpty(caption)) {
                        var topTxt = new Rectangle(Skin.Padding, 0, c.Width, c.Height);
                        Skin.Draw_FormatedText(gr, caption, null, Alignment.Top_Left, topTxt, Design.GroupBox, state, c, true, true);
                    }
                }
                break;

            case GroupBoxStyle.RoundRect:
                if (c.Height > 10) {
                    Skin.Draw_Back(gr, Design.GroupBox_RoundRect, state, r, null, true);
                    Skin.Draw_Border(gr, Design.GroupBox_RoundRect, state, r);
                }
                break;

            case GroupBoxStyle.DropdownMenu:
                if (c.Height > 10) {
                    Skin.Draw_Back(gr, Design.Form_SelectBox_Dropdown, state, r, null, true);
                    Skin.Draw_Border(gr, Design.Form_SelectBox_Dropdown, state, r);
                }
                break;

            case GroupBoxStyle.NormalBold:
                if (c.Height > 33) {
                    Skin.Draw_Border(gr, Design.GroupBox_Bold, state, r);
                    if (!string.IsNullOrEmpty(caption)) {
                        var topTxt = new Rectangle(Skin.Padding, Skin.PaddingSmal, c.Width, c.Height);
                        Skin.Draw_FormatedText(gr, caption, null, Alignment.Top_Left, topTxt, Design.GroupBox_Bold, state, c, false, true);
                    }
                } else {
                    var d = Skin.DesignOf(Design.GroupBox_Bold, state);
                    gr.Clear(d.BorderColor1);
                    if (!string.IsNullOrEmpty(caption)) {
                        var topTxt = new Rectangle(Skin.Padding, 0, c.Width, c.Height);
                        Skin.Draw_FormatedText(gr, caption, null, Alignment.VerticalCenter_Left, topTxt, Design.GroupBox_Bold, state, c, false, true);
                    }
                }
                break;

            case GroupBoxStyle.NormalBoldBottom:
                if (c.Height > 33) {
                    Skin.Draw_Border(gr, Design.GroupBox_BoldBottom, state, r);
                    if (!string.IsNullOrEmpty(caption)) {
                        var bottomTxt = new Rectangle(Skin.Padding, -Skin.PaddingSmal, c.Width, c.Height);
                        Skin.Draw_FormatedText(gr, caption, null, Alignment.Bottom_Left, bottomTxt, Design.GroupBox_BoldBottom, state, c, false, true);
                    }
                } else {
                    var dBottom = Skin.DesignOf(Design.GroupBox_BoldBottom, state);
                    gr.Clear(dBottom.BorderColor1);
                    if (!string.IsNullOrEmpty(caption)) {
                        var bottomTxt = new Rectangle(Skin.Padding, 0, c.Width, c.Height);
                        Skin.Draw_FormatedText(gr, caption, null, Alignment.VerticalCenter_Left, bottomTxt, Design.GroupBox_BoldBottom, state, c, false, true);
                    }
                }
                break;

            default:
                Skin.Draw_Back_Transparent(gr, c.DisplayRectangle, c);
                break;
        }
    }

    protected override void OnControlAdded(ControlEventArgs e) {
        base.OnControlAdded(e);
        SetStandardValues();
        ChildControls_RibbonBar();
    }

    protected override void OnControlRemoved(ControlEventArgs e) {
        base.OnControlRemoved(e);
        SetStandardValues();
        ChildControls_RibbonBar();
    }

    protected override void OnDockChanged(System.EventArgs e) {
        base.OnDockChanged(e);
        SetStandardValues();
        ChildControls_RibbonBar();
    }

    protected override void OnPaint(PaintEventArgs e) {
        var state = States.Standard;
        if (!GenericControl.AllEnabled(Parent)) { state = States.Standard_Disabled; }
        DrawGroupBox(this, e.Graphics, state, GroupBoxStyle, Text);
        if (DesignMode) { ChildControls_RibbonBar(); }
    }

    protected override void OnPaintBackground(PaintEventArgs pevent) { }

    protected override void OnParentChanged(System.EventArgs e) {
        base.OnParentChanged(e);
        SetStandardValues();
        ChildControls_RibbonBar();
    }

    private void ChildControls_RibbonBar() {
        if (GroupBoxStyle != GroupBoxStyle.RibbonBar) { return; }
        if (Width < 10 || Height < 10) { return; }
        if (Controls.Count == 0) { return; }

        foreach (Control thisControl in Controls) {
            switch (thisControl) {
                case Caption:
                case Line:
                case Button:
                case ComboBox:
                case ListBox:
                case TextBox:
                    thisControl.Top = ((int)(thisControl.Top / 22.0) * 22) + 2;
                    thisControl.Height = Math.Max((int)(thisControl.Height / 22.0) * 22, 22);
                    break;
            }
        }
    }

    private void SetStandardValues() {
        var l = GenericControl.Typ(Parent);
        if (GroupBoxStyle == GroupBoxStyle.RibbonBar) { l = ParentType.RibbonPage; }

        if (GroupBoxStyle is GroupBoxStyle.RoundRect or GroupBoxStyle.DropdownMenu) {
            var design = GroupBoxStyle == GroupBoxStyle.DropdownMenu ? Design.Form_SelectBox_Dropdown : Design.GroupBox_RoundRect;
            base.BackColor = Skin.Color_Back(design, States.Standard);
            return;
        }

        switch (l) {
            case ParentType.RibbonPage:
                GroupBoxStyle = GroupBoxStyle.RibbonBar;
                base.BackColor = Skin.Color_Back(Design.RibbonBar_Body, States.Standard);
                break;

            case ParentType.TabPage:
                base.BackColor = Skin.Color_Back(Design.TabStrip_Body, States.Standard);
                break;

            case ParentType.Form:
                base.BackColor = Skin.Color_Back(Design.Form_Standard, States.Standard);
                break;
        }
    }

    #endregion
}