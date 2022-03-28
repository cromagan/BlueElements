// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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
using BlueControls.Controls;
using BlueControls.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using BlueControls.Interfaces;
using System.Windows.Forms;
using BlueControls.Forms;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueBasics.MultiUserFile;

namespace BlueControls.ItemCollection {

    public class ChildFormulaPaditem : RectanglePadItem, IItemToControl {

        #region Fields

        public static BlueFont? CellFont = Skin.GetBlueFont(Design.Table_Cell, States.Standard);
        public static BlueFont? ChapterFont = Skin.GetBlueFont(Design.Table_Cell_Chapter, States.Standard);
        public static BlueFont? ColumnFont = Skin.GetBlueFont(Design.Table_Column, States.Standard);
        public Controls.ListBox Childs = new();
        public ListExt<string> NotAllowedChilds = new();
        private string _path = string.Empty;

        #endregion

        #region Constructors

        public ChildFormulaPaditem() : this(UniqueInternal(), string.Empty, null) { }

        public ChildFormulaPaditem(string intern, string filename, List<string>? notAllowedChilds) : base(intern) {
            _path = filename.FilePath();

            if (notAllowedChilds != null) {
                NotAllowedChilds.AddRange(notAllowedChilds);
            }
            NotAllowedChilds.AddIfNotExists(filename);

            NotAllowedChilds.Changed += NotAllowedChilds_Changed;
            Childs.ListOrItemChanged += Childs_ListOrItemChanged;
            Childs.ContextMenuInit += Childs_ContextMenuInit;
            Childs.ContextMenuItemClicked += Childs_ContextMenuItemClicked;
        }

        public ChildFormulaPaditem(string intern) : this(intern, string.Empty, null) { }

        #endregion

        #region Properties

        public ListExt<string> Formulare { get; } = new();

        protected override int SaveOrder => 1000;

        #endregion

        #region Methods

        public Control GenerateControl(ConnectedFormulaView parent) {
            var c3 = new Controls.TabControl();
            c3.Tag = Internal;

            foreach (var thisc in Childs.Item) {
                var t = new TabPage();
                t.Text = thisc.Internal.FileNameWithoutSuffix();
                c3.TabPages.Add(t);

                var cf = ConnectedFormula.ConnectedFormula.GetByFilename(thisc.Internal);

                if (cf != null) {
                    var cc = new ConnectedFormulaView();

                    t.Controls.Add(cc);
                    cc.ConnectedFormula = cf;
                    cc.Dock = DockStyle.Fill;
                }
            }

            return c3;
        }

        public override List<GenericControl> GetStyleOptions() {
            List<GenericControl> l = new() { };

            UpdateList();
            l.Add(Childs);

            //l.Add(new FlexiControlForProperty<string>(() => Text));
            return l;
        }

        public override bool ParseThis(string tag, string value) {
            if (base.ParseThis(tag, value)) { return true; }
            switch (tag) {
                case "path":
                    _path = value.FromNonCritical();
                    return true;

                case "childs":
                    var tmp = value.FromNonCritical().SplitAndCutBy("|");
                    Childs.Item.Clear();
                    Childs.Item.AddRange(tmp);
                    return true;

                case "notallowedchilds":
                    var tmp2 = value.FromNonCritical().SplitAndCutBy("|");
                    NotAllowedChilds.Clear();
                    NotAllowedChilds.AddRange(tmp2);
                    return true;
            }
            return false;
        }

        //    return false;
        //}
        public override string ToString() {
            var t = base.ToString();
            t = t.Substring(0, t.Length - 1) + ", ";

            t = t + "Path=" + _path.ToNonCritical() + ", ";
            t = t + "Childs=" + Childs.Item.ToListOfString().JoinWith("|").ToNonCritical() + ", ";
            t = t + "NotAllowedChilds=" + NotAllowedChilds.JoinWith("|").ToNonCritical() + ", ";
            return t.Trim(", ") + "}";
        }

        //public bool IsRecursiveWith(IRecursiveCheck obj) {
        //    if (obj == this) { return true; }
        protected override string ClassId() => "ChildFormula";

        protected override void Dispose(bool disposing) {
            if (disposing) {
                NotAllowedChilds.Changed -= NotAllowedChilds_Changed;
                Childs.ListOrItemChanged -= Childs_ListOrItemChanged;
                Childs.ContextMenuInit -= Childs_ContextMenuInit;
                Childs.ContextMenuItemClicked -= Childs_ContextMenuItemClicked;
            }
        }

        protected override void DrawExplicit(Graphics gr, RectangleF modifiedPosition, float zoom, float shiftX, float shiftY, bool forPrinting) {
            //DrawColorScheme(gr, modifiedPosition, zoom, Id);
            //s
            gr.DrawRectangle(new Pen(Color.Black, zoom), modifiedPosition);

            //Skin.Draw_FormatedText(gr, _text, QuickImage.Get(ImageCode.Textfeld, (int)(zoom * 16)), Alignment.Horizontal_Vertical_Center, modifiedPosition.ToRect(), ColumnPadItem.ColumnFont.Scale(zoom), false);

            gr.FillRectangle(new SolidBrush(Color.FromArgb(128, 255, 255, 255)), modifiedPosition);

            base.DrawExplicit(gr, modifiedPosition, zoom, shiftX, shiftY, forPrinting);
        }

        protected override BasicPadItem? TryCreate(string id, string name) {
            if (id.Equals(ClassId(), StringComparison.OrdinalIgnoreCase)) {
                return new ChildFormulaPaditem(name);
            }
            return null;
        }

        private void Childs_ContextMenuInit(object sender, EventArgs.ContextMenuInitEventArgs e) {
            e.UserMenu.Add(ContextMenuComands.Bearbeiten);
        }

        private void Childs_ContextMenuItemClicked(object sender, EventArgs.ContextMenuItemClickedEventArgs e) {
            if (e.ClickedComand.ToLower() == "bearbeiten") {
                MultiUserFile.SaveAll(false);

                var x = new ConnectedFormulaEditor(((BasicListItem)e.HotItem).Internal, NotAllowedChilds);
                x.ShowDialog();
                MultiUserFile.SaveAll(false);
                x.Dispose();
            }
        }

        private void Childs_ListOrItemChanged(object sender, System.EventArgs e) {
            OnChanged();
        }

        private void NotAllowedChilds_Changed(object sender, System.EventArgs e) {
            UpdateList();

            foreach (var thisl in NotAllowedChilds) {
                Childs.Item.Remove(thisl);
            }

            OnChanged();
        }

        private void UpdateList() {
            Childs.AddAllowed = AddType.OnlySuggests;
            Childs.RemoveAllowed = true;
            Childs.MoveAllowed = true;

            var l = new List<string>();
            l.AddRange(System.IO.Directory.GetFiles(_path, "*.cfo"));
            l.RemoveRange(NotAllowedChilds);
            Childs.Suggestions.Clear();
            Childs.Suggestions.AddRange(l);
        }

        #endregion
    }
}