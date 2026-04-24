// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.Extended_Text;
using BlueControls.Interfaces;
using BlueTable.Classes;
using BlueTable.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Form = BlueControls.Forms.Form;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
[DefaultEvent(nameof(Click))]
public partial class Caption : GenericControl, IContextMenu, IBackgroundNone, ITranslateable {

    #region Fields

    private Design _design = Design.Undefined;

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

    #region Properties

    [DefaultValue(true)]
    public bool ContextMenuDefault { get; set; } = true;

    [DefaultValue(null)]
    public ReadOnlyCollection<AbstractListItem>? CustomContextMenuItems { get; set; } = null;

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

    [DefaultValue(true)]
    public bool Translate { get; set; } = true;

    #endregion

    #region Methods

    public static Size RequiredTextSize(string text, Design design, bool translate, int maxwidth) {
        if (QuickModePossible(text)) {
            var s = Skin.GetBlueFont(design, States.Standard).MeasureString(text);
            return new Size((int)(s.Width + 1), (int)(s.Height + 1));
        }
        var eText = GetEText(text, design, States.Standard, translate, maxwidth);
        return eText.LastSize();
    }

    public List<AbstractListItem>? GetContextMenuItems(object? hotItem) => null;

    public void ResetETextAndInvalidate() {
        Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
        _eText = null;
        Invalidate();
    }

    internal void FitSize() {
        if (_design == Design.Undefined) { GetDesign(); }
        var s = RequiredTextSize(Text, _design, Translate, -1);
        Width = s.Width;
        Height = s.Height;
    }

    protected override void Dispose(bool disposing) {
        try {
            if (disposing) {
                _eText = null;
            }
        } finally {
            base.Dispose(disposing);
        }
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (IsDisposed) { return; }
        base.DrawControl(gr, state);

        try {
            Skin.Draw_Back_Transparent(gr, DisplayRectangle, this);

            if (_design == Design.Undefined) {
                GetDesign();
                if (_design == Design.Undefined) { return; }
            }

            if (state is not States.Standard and not States.Standard_Disabled) {
                Develop.DebugPrint(state);
                return;
            }

            if (!string.IsNullOrEmpty(Text)) {
                if (QuickModePossible(Text)) {
                    Skin.Draw_FormatedText(gr, Text, null, Alignment.Top_Left, new Rectangle(), _design, state, null, false, Translate);
                } else {
                    UseBackgroundBitmap = true;
                    _eText ??= GetEText(Text, _design, state, Translate, Width);
                    _eText.AreaControl = ClientRectangle;
                    _eText.Draw(gr, 1, 0, 0);
                }
            }
        } catch { }
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        base.OnMouseUp(e);
        if (e.Button == MouseButtons.Right) { ((IContextMenu)this).ContextMenuShow(this); }
    }

    private static ExtText GetEText(string text, Design design, States state, bool translate, int maxwidth) => new ExtText(design, state) {
        HtmlText = LanguageTool.DoTranslate(text, translate),
        Multiline = true,
        TextDimensions = new Size(maxwidth, -1),
    };

    private static bool QuickModePossible(string text) => !text.Contains('<') && !text.Contains('\r');

    private void GetDesign() {
        _design = Design.Undefined;
        if (Parent == null) { return; }
        if (Parent is Form fm) { _design = fm.Design; }
        switch (_design) {
            case Design.Form_QuickInfo:
            case Design.Form_Notification:
            case Design.Form_Progressbar:
            case Design.Form_ContextMenu:
            case Design.Form_SelectBox_Dropdown:
            case Design.Form_AutoFilter:
                return;
        }
        switch (GetParentType()) {
            case ParentType.RibbonGroupBox:
            case ParentType.RibbonPage:
                _design = Design.RibbonBar_Caption;
                break;

            default:
                _design = Design.Caption;
                break;
        }
    }

    #endregion
}