// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Extended_Text;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Interfaces;
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

    public Caption() : base(false, false) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        SetNotFocusable();
        MouseHighlight = false;
    }

    #endregion

    #region Events

    public event EventHandler<ContextMenuInitEventArgs>? ContextMenuInit;

    public event EventHandler<ContextMenuItemClickedEventArgs>? ContextMenuItemClicked;

    #endregion

    #region Properties

    public new int Height {
        get => Convert.ToBoolean(_textAnzeigeverhalten & SteuerelementVerhalten.Steuerelement_Anpassen)
            ? TextRequiredSize().Height
            : base.Height;
        set {
            GetDesign();
            if (Convert.ToBoolean(_textAnzeigeverhalten & SteuerelementVerhalten.Steuerelement_Anpassen)) { return; }
            base.Height = value;
        }
    }

    public new Size Size {
        get => Convert.ToBoolean(_textAnzeigeverhalten & SteuerelementVerhalten.Steuerelement_Anpassen) ? TextRequiredSize() : base.Size;
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
        get => Convert.ToBoolean(_textAnzeigeverhalten & SteuerelementVerhalten.Steuerelement_Anpassen)
            ? TextRequiredSize().Width
            : base.Width;
        set {
            GetDesign();
            if (Convert.ToBoolean(_textAnzeigeverhalten & SteuerelementVerhalten.Steuerelement_Anpassen)) { return; }
            base.Width = value;
        }
    }

    #endregion

    #region Methods

    public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) => false;

    public void GetContextMenuItems(MouseEventArgs? e, ItemCollectionList items, out object? hotItem, ref bool cancel, ref bool translate) => hotItem = null;

    public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

    public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) => ContextMenuItemClicked?.Invoke(this, e);

    public void ResetETextAndInvalidate() {
        Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
        _eText = null;
        if (!QuickModePossible()) { SetDoubleBuffering(); }
        Invalidate();
    }

    public Size TextRequiredSize() {
        if (QuickModePossible()) {
            if (_design == Design.Undefiniert) { GetDesign(); }
            var s = BlueFont.MeasureString(_text, Skin.GetBlueFont(_design, States.Standard).Font());
            return new Size((int)(s.Width + 1), (int)(s.Height + 1));
        }
        if (_eText == null) {
            if (DesignMode) { Refresh(); }// Damit das skin Geinittet wird
            DrawControl(null, States.Standard);
        }
        return _eText?.LastSize() ?? new Size(1, 1);
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
                if (QuickModePossible()) {
                    if (gr == null) { return; }
                    Skin.Draw_Back_Transparent(gr, DisplayRectangle, this);
                    Skin.Draw_FormatedText(gr, _text, _design, state, null, Alignment.Top_Left, new Rectangle(), null, false, Translate);
                    return;
                }

                _eText ??= new ExtText(_design, state) {
                    HtmlText = LanguageTool.DoTranslate(_text, Translate),
                    State = state,
                    Multiline = true
                };

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
        switch (ParentType()) {
            case PartentType.RibbonGroupBox:

            case PartentType.RibbonPage:
                _design = Design.Ribbonbar_Caption;
                break;

            case PartentType.GroupBox:

            case PartentType.TabPage:

            case PartentType.Form:

            case PartentType.FlexiControlForCell:

            case PartentType.Unbekannt: // UserForms und anderes

            case PartentType.Nothing: // UserForms und anderes

            case PartentType.ListBox:
                _design = Design.Caption;
                return;

            default:
                _design = Design.Caption;
                break;
        }
    }

    private bool QuickModePossible() {
        if (_textAnzeigeverhalten != SteuerelementVerhalten.Text_Abschneiden) { return false; }
        //if (Math.Abs(_Zeilenabstand - 1) > 0.01) { return false; }
        return !_text.Contains("<");
    }

    #endregion
}