// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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
using BlueDatabase;
using BlueDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.ItemCollectionList;
using Form = BlueControls.Forms.Form;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
[DefaultEvent("Click")]
public partial class Caption : GenericControl, IContextMenu, IBackgroundNone, ITranslateable {

    #region Fields

    private Design _design = Design.Undefiniert;

    private ExtText? _eText;
    private string _text = string.Empty;

    private SteuerelementVerhalten _textAnzeigeverhalten = SteuerelementVerhalten.Text_Abschneiden;

    #endregion

    #region Constructors

    public Caption() : base(true, false) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        SetNotFocusable();
        SetDoubleBuffering();
        MouseHighlight = false;
    }

    #endregion

    #region Events

    public event EventHandler<ContextMenuInitEventArgs>? ContextMenuInit;

    public event EventHandler<ContextMenuItemClickedEventArgs>? ContextMenuItemClicked;

    #endregion

    #region Properties

    public new int Height {
        get => _textAnzeigeverhalten.HasFlag(SteuerelementVerhalten.Steuerelement_Anpassen)
            ? RequiredTextSize().Height
            : base.Height;
        set {
            GetDesign();
            if (_textAnzeigeverhalten.HasFlag(SteuerelementVerhalten.Steuerelement_Anpassen)) { return; }
            base.Height = value;
        }
    }

    public new Size Size {
        get => _textAnzeigeverhalten.HasFlag(SteuerelementVerhalten.Steuerelement_Anpassen) ? RequiredTextSize() : base.Size;
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
        // ReSharper disable once ValueParameterNotUsed
        set => base.TabIndex = 0;
    }

    /// <summary>
    /// Benötigt, dass der Designer das nicht erstellt
    /// </summary>
    [DefaultValue(false)]
    public new bool TabStop {
        get => false;
        // ReSharper disable once ValueParameterNotUsed
        set => base.TabStop = false;
    }

    [DefaultValue("")]
    public new string Text {
        get => _text;
        set {
            Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
            if (_text == value) { return; }
            _text = value;
            ResetETextAndInvalidate();
        }
    }

    [DefaultValue(SteuerelementVerhalten.Text_Abschneiden)]
    public SteuerelementVerhalten TextAnzeigeVerhalten {
        get => _textAnzeigeverhalten;
        set {
            if (_textAnzeigeverhalten == value) { return; }
            _textAnzeigeverhalten = value;
            ResetETextAndInvalidate();
        }
    }

    [DefaultValue(true)]
    public bool Translate { get; set; } = true;

    public new int Width {
        get => _textAnzeigeverhalten.HasFlag(SteuerelementVerhalten.Steuerelement_Anpassen)
            ? RequiredTextSize().Width
            : base.Width;
        set {
            GetDesign();
            if (_textAnzeigeverhalten.HasFlag(SteuerelementVerhalten.Steuerelement_Anpassen)) { return; }
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

        eText.Design = design;
        eText.HtmlText = LanguageTool.DoTranslate(text, translate);
        eText.Multiline = true;
        eText.TextDimensions = new Size(maxwidth, eText.TextDimensions.Height);

        return eText.LastSize();
    }

    public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) => false;

    public void GetContextMenuItems(MouseEventArgs? e, List<AbstractListItem> items, out object? hotItem) => hotItem = null;

    public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

    public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) => ContextMenuItemClicked?.Invoke(this, e);

    public Size RequiredTextSize() {
        if (_design == Design.Undefiniert) { GetDesign(); }

        if (!QuickModePossible(_text, _textAnzeigeverhalten) && _eText == null) {
            if (DesignMode) { Refresh(); }// Damit das Skin initialisiert wird
            DrawControl(null, States.Standard);
        }

        return RequiredTextSize(_text, _textAnzeigeverhalten, _design, _eText, Translate, -1);
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

            if (!string.IsNullOrEmpty(_text)) {
                if (QuickModePossible(_text, _textAnzeigeverhalten)) {
                    if (gr == null) { return; }
                    Skin.Draw_Back_Transparent(gr, DisplayRectangle, this);
                    Skin.Draw_FormatedText(gr, _text, _design, state, null, Alignment.Top_Left, new Rectangle(), null, false, Translate);
                    return;
                }

                _eText ??= new ExtText(_design, state) {
                    HtmlText = LanguageTool.DoTranslate(_text, Translate),
                    Multiline = true
                };
                _eText.State = state;

                switch (_textAnzeigeverhalten) {
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
            if (!string.IsNullOrEmpty(_text)) { _eText?.Draw(gr, 1); }
        } catch { }
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        base.OnMouseUp(e);
        if (e.Button == MouseButtons.Right) { FloatingInputBoxListBoxStyle.ContextMenuShow(this, e); }
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