// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Extended_Text;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueTable;
using BlueTable.Interfaces;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Form = BlueControls.Forms.Form;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
[DefaultEvent("Click")]
public partial class Caption : GenericControl, IContextMenuWithInternalHandling, IBackgroundNone, ITranslateable {

    #region Fields

    private Design _design = Design.Undefiniert;

    private ExtText? _eText;

    #endregion

    #region Constructors

    public Caption() : base(true, false, false) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        SetNotFocusable();
    }

    #endregion

    #region Events

    public event EventHandler<ContextMenuInitEventArgs>? ContextMenuInit;

    public event EventHandler<ContextMenuItemClickedEventArgs>? ContextMenuItemClicked;

    #endregion

    #region Properties

    public new int Height {
        get => TextAnzeigeVerhalten.HasFlag(SteuerelementVerhalten.Steuerelement_Anpassen)
            ? RequiredTextSize().Height
            : base.Height;
        set {
            GetDesign();
            if (TextAnzeigeVerhalten.HasFlag(SteuerelementVerhalten.Steuerelement_Anpassen)) { return; }
            base.Height = value;
        }
    }

    public new Size Size {
        get => TextAnzeigeVerhalten.HasFlag(SteuerelementVerhalten.Steuerelement_Anpassen) ? RequiredTextSize() : base.Size;
        set {
            GetDesign();
            if (value.Width == base.Size.Width && value.Height == base.Size.Height) { return; }
            base.Size = value;
        }
    }

    /// <summary>
    /// Benötigt, dass der Designer das nicht erstellt
    /// </summary>
    [DefaultValue(0)]
    public new int TabIndex {
        get => 0;
  
        set => base.TabIndex = 0;
    }

    /// <summary>
    /// Benötigt, dass der Designer das nicht erstellt
    /// </summary>
    [DefaultValue(false)]
    public new bool TabStop {
        get => false;
  
        set => base.TabStop = false;
    }

    [DefaultValue("")]
    public new string Text {
        get;
        set {
            Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
            if (field == value) { return; }
            field = value;
            ResetETextAndInvalidate();
        }
    } = string.Empty;

    [DefaultValue(SteuerelementVerhalten.Text_Abschneiden)]
    public SteuerelementVerhalten TextAnzeigeVerhalten {
        get;
        set {
            if (field == value) { return; }
            field = value;
            ResetETextAndInvalidate();
        }
    } = SteuerelementVerhalten.Text_Abschneiden;

    [DefaultValue(true)]
    public bool Translate { get; set; } = true;

    public new int Width {
        get => TextAnzeigeVerhalten.HasFlag(SteuerelementVerhalten.Steuerelement_Anpassen)
            ? RequiredTextSize().Width
            : base.Width;
        set {
            GetDesign();
            if (TextAnzeigeVerhalten.HasFlag(SteuerelementVerhalten.Steuerelement_Anpassen)) { return; }
            base.Width = value;
        }
    }

    #endregion

    #region Methods

    public static Size RequiredTextSize(string text, SteuerelementVerhalten textAnzeigeverhalten, Design design, ExtText? eText, bool translate, int maxwidth) {
        if (QuickModePossible(text, textAnzeigeverhalten)) {
            var s = Skin.GetBlueFont(design, States.Standard).MeasureString(text);
            return new Size((int)(s.Width + 1), (int)(s.Height + 1));
        }

        eText ??= new ExtText(design, States.Standard);

        //eText.Design = design;
        eText.HtmlText = LanguageTool.DoTranslate(text, translate);
        eText.Multiline = true;
        eText.TextDimensions = eText.TextDimensions with { Width = maxwidth };

        return eText.LastSize();
    }

    public void DoContextMenuItemClick(ContextMenuItemClickedEventArgs e) => OnContextMenuItemClicked(e);

    public void GetContextMenuItems(ContextMenuInitEventArgs e) => OnContextMenuInit(e);

    public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

    public Size RequiredTextSize() {
        if (_design == Design.Undefiniert) { GetDesign(); }

        if (!QuickModePossible(Text, TextAnzeigeVerhalten) && _eText == null) {
            if (DesignMode) { Refresh(); }// Damit das Skin initialisiert wird
            DrawControl(null, States.Standard);
        }

        return RequiredTextSize(Text, TextAnzeigeVerhalten, _design, _eText, Translate, -1);
    }

    public void ResetETextAndInvalidate() {
        Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
        _eText = null;
        //if (!QuickModePossible()) { SetDoubleBuffering(); }
        Invalidate();
    }

    protected override void DrawControl(Graphics? gr, States state) {
        try {
            if (_design == Design.Undefiniert) {
                GetDesign();
                if (_design == Design.Undefiniert) { return; }
            }

            if (state is not States.Standard and not States.Standard_Disabled) {
                Develop.DebugPrint(state);
                return;
            }

            if (!string.IsNullOrEmpty(Text)) {
                if (QuickModePossible(Text, TextAnzeigeVerhalten)) {
                    if (gr == null) { return; }
                    Skin.Draw_Back_Transparent(gr, DisplayRectangle, this);
                    Skin.Draw_FormatedText(gr, Text, null, Alignment.Top_Left, new Rectangle(), _design, state, null, false, Translate);
                    return;
                }

                UseBackgroundBitmap = true;

                _eText ??= new ExtText(_design, state) {
                    HtmlText = LanguageTool.DoTranslate(Text, Translate),
                    Multiline = true
                };

                switch (TextAnzeigeVerhalten) {
                    case SteuerelementVerhalten.Steuerelement_Anpassen:
                        _eText.TextDimensions = Size.Empty;
                        Size = _eText.LastSize();
                        break;

                    case SteuerelementVerhalten.Text_Abschneiden:
                        _eText.TextDimensions = Size.Empty;
                        break;

                    case SteuerelementVerhalten.Scrollen_mit_Textumbruch:
                        _eText.TextDimensions = new Size(base.Size.Width, -1);
                        break;

                    case SteuerelementVerhalten.Scrollen_ohne_Textumbruch:
                        _eText.TextDimensions = Size.Empty;
                        break;
                }
                _eText.DrawingArea = ClientRectangle;
            }
            if (gr == null) { return; }// Wenn vorab die Größe abgefragt wird
            Skin.Draw_Back_Transparent(gr, DisplayRectangle, this);
            if (!string.IsNullOrEmpty(Text)) { _eText?.Draw(gr, 1); }
        } catch { }
    }

    protected void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) => ContextMenuItemClicked?.Invoke(this, e);

    protected override void OnMouseUp(MouseEventArgs e) {
        base.OnMouseUp(e);
        if (e.Button == MouseButtons.Right) { FloatingInputBoxListBoxStyle.ContextMenuShow(this, this, e); }
    }

    private static bool QuickModePossible(string text, SteuerelementVerhalten textAnzeigeverhalten) {
        if (textAnzeigeverhalten != SteuerelementVerhalten.Text_Abschneiden) { return false; }
        //if (Math.Abs(_Zeilenabstand - 1) > 0.01) { return false; }
        return !text.Contains("<");
    }

    private void GetDesign() {
        _design = Design.Undefiniert;
        if (Parent == null) { return; }
        if (Parent is Form fm) { _design = fm.Design; }
        switch (_design) {
            case Design.Form_QuickInfo:
            case Design.Form_DesktopBenachrichtigung:
            case Design.Form_BitteWarten:
            case Design.Form_KontextMenu:
            case Design.Form_SelectBox_Dropdown:
            case Design.Form_AutoFilter:
                return;
        }
        switch (GetParentType()) {
            case ParentType.RibbonGroupBox:
            case ParentType.RibbonPage:
                _design = Design.Ribbonbar_Caption;
                break;

            //case ParentType.GroupBox:
            //case ParentType.TabPage:
            //case ParentType.Form:
            //case ParentType.FlexiControlForCell:
            //case ParentType.Unbekannt: // UserForms und anderes
            //case ParentType.Nothing: // UserForms und anderes
            //case ParentType.ListBox:
            //    _design = Design.Caption;
            //    return;

            default:
                _design = Design.Caption;
                break;
        }
    }

    #endregion
}