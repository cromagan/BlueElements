﻿// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace BlueControls.Controls {

    [Designer(typeof(BasicDesigner))]
    [DefaultEvent("Click")]
    public partial class Caption : GenericControl, IContextMenu, IBackgroundNone {

        #region Fields

        private enDesign _design = enDesign.Undefiniert;

        private string _Text = string.Empty;

        private enSteuerelementVerhalten _TextAnzeigeverhalten = enSteuerelementVerhalten.Text_Abschneiden;

        private ExtText eText;

        #endregion

        #region Constructors

        public Caption() : base(false, false) {
            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();
            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            SetNotFocusable();
            _MouseHighlight = false;
        }

        #endregion

        #region Events

        public event EventHandler<ContextMenuInitEventArgs> ContextMenuInit;

        public event EventHandler<ContextMenuItemClickedEventArgs> ContextMenuItemClicked;

        #endregion

        #region Properties

        public new int Height {
            get => Convert.ToBoolean(_TextAnzeigeverhalten & enSteuerelementVerhalten.Steuerelement_Anpassen)
                    ? TextRequiredSize().Height
                    : base.Height;
            set {
                GetDesign();
                if (Convert.ToBoolean(_TextAnzeigeverhalten & enSteuerelementVerhalten.Steuerelement_Anpassen)) { return; }
                base.Height = value;
            }
        }

        public new Size Size {
            get => Convert.ToBoolean(_TextAnzeigeverhalten & enSteuerelementVerhalten.Steuerelement_Anpassen) ? TextRequiredSize() : base.Size;
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
            get => _Text;
            set {
                if (value is null) { value = string.Empty; }
                if (_Text == value) { return; }
                _Text = value;
                ResetETextAndInvalidate();
            }
        }

        [DefaultValue(enSteuerelementVerhalten.Text_Abschneiden)]
        public enSteuerelementVerhalten TextAnzeigeVerhalten {
            get => _TextAnzeigeverhalten;
            set {
                if (_TextAnzeigeverhalten == value) { return; }
                _TextAnzeigeverhalten = value;
                ResetETextAndInvalidate();
            }
        }

        public new int Width {
            get => Convert.ToBoolean(_TextAnzeigeverhalten & enSteuerelementVerhalten.Steuerelement_Anpassen)
                    ? TextRequiredSize().Width
                    : base.Width;
            set {
                GetDesign();
                if (Convert.ToBoolean(_TextAnzeigeverhalten & enSteuerelementVerhalten.Steuerelement_Anpassen)) { return; }
                base.Width = value;
            }
        }

        #endregion

        #region Methods

        public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) => false;

        public void GetContextMenuItems(System.Windows.Forms.MouseEventArgs e, ItemCollectionList Items, out object HotItem, List<string> Tags, ref bool Cancel, ref bool Translate) => HotItem = null;

        public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

        public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) => ContextMenuItemClicked?.Invoke(this, e);

        public void ResetETextAndInvalidate() {
            eText = null;
            if (!QuickModePossible()) { SetDoubleBuffering(); }
            Invalidate();
        }

        public Size TextRequiredSize() {
            if (QuickModePossible()) {
                if (_design == enDesign.Undefiniert) { GetDesign(); }
                var s = BlueFont.MeasureString(_Text, Skin.GetBlueFont(_design, enStates.Standard).Font());
                return new Size((int)(s.Width + 1), (int)(s.Height + 1));
            }
            if (eText == null) {
                if (DesignMode) { Refresh(); }// Damit das skin Geinittet wird
                DrawControl(null, enStates.Standard);
            }
            return eText != null ? eText.LastSize() : new Size(1, 1);
        }

        protected override void DrawControl(Graphics gr, enStates state) {
            try {
                if (_design == enDesign.Undefiniert) {
                    GetDesign();
                    if (_design == enDesign.Undefiniert) { return; }
                }
                if (state is not enStates.Standard and not enStates.Standard_Disabled) {
                    Develop.DebugPrint(state);
                    return;
                }
                if (!string.IsNullOrEmpty(_Text)) {
                    if (QuickModePossible()) {
                        if (gr == null) { return; }
                        Skin.Draw_Back_Transparent(gr, DisplayRectangle, this);
                        Skin.Draw_FormatedText(gr, _Text, _design, state, null, enAlignment.Top_Left, new Rectangle(), null, false, Translate);
                        return;
                    }
                    if (eText == null) {
                        eText = new ExtText(_design, state) {
                            HtmlText = BlueDatabase.LanguageTool.DoTranslate(_Text, Translate)
                        };
                        //eText.Zeilenabstand = _Zeilenabstand;
                    }
                    eText.State = state;
                    eText.Multiline = true;
                    switch (_TextAnzeigeverhalten) {
                        case enSteuerelementVerhalten.Steuerelement_Anpassen:
                            eText.TextDimensions = Size.Empty;
                            Size = eText.LastSize();
                            break;

                        case enSteuerelementVerhalten.Text_Abschneiden:
                            eText.TextDimensions = Size.Empty;
                            break;

                        case enSteuerelementVerhalten.Scrollen_mit_Textumbruch:
                            eText.TextDimensions = new Size(base.Size.Width, -1);
                            break;

                        case enSteuerelementVerhalten.Scrollen_ohne_Textumbruch:
                            eText.TextDimensions = Size.Empty;
                            break;
                    }
                    eText.DrawingArea = ClientRectangle;
                }
                if (gr == null) { return; }// Wenn vorab die Größe abgefragt wird
                Skin.Draw_Back_Transparent(gr, DisplayRectangle, this);
                if (!string.IsNullOrEmpty(_Text)) { eText.Draw(gr, 1); }
            } catch {
            }
        }

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseUp(e);
            if (e.Button == System.Windows.Forms.MouseButtons.Right) { FloatingInputBoxListBoxStyle.ContextMenuShow(this, e); }
        }

        private void GetDesign() {
            _design = enDesign.Undefiniert;
            if (Parent == null) { return; }
            if (Parent is Form fm) { _design = fm.Design; }
            switch (_design) {
                case enDesign.Form_QuickInfo:

                case enDesign.Form_DesktopBenachrichtigung:

                case enDesign.Form_BitteWarten:

                case enDesign.Form_KontextMenu:

                case enDesign.Form_SelectBox_Dropdown:

                case enDesign.Form_AutoFilter:
                    return;
            }
            switch (ParentType()) {
                case enPartentType.RibbonGroupBox:

                case enPartentType.RibbonPage:
                    _design = enDesign.Ribbonbar_Caption;
                    break;

                case enPartentType.GroupBox:

                case enPartentType.TabPage:

                case enPartentType.Form:

                case enPartentType.FlexiControlForCell:

                case enPartentType.Unbekannt: // UserForms und anderes

                case enPartentType.Nothing: // UserForms und anderes

                case enPartentType.ListBox:
                    _design = enDesign.Caption;
                    return;

                default:
                    _design = enDesign.Caption;
                    break;
            }
        }

        private bool QuickModePossible() {
            if (_TextAnzeigeverhalten != enSteuerelementVerhalten.Text_Abschneiden) { return false; }
            //if (Math.Abs(_Zeilenabstand - 1) > 0.01) { return false; }
            return !_Text.Contains("<");
        }

        #endregion
    }
}