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
using BlueBasics.Enums;

namespace BlueControls.ItemCollection;

public class TabFormulaPaditem : CustomizableShowPadItem, IItemToControl {

    #region Fields

    public static BlueFont? CellFont = Skin.GetBlueFont(Design.Table_Cell, States.Standard);
    public static BlueFont? ChapterFont = Skin.GetBlueFont(Design.Table_Cell_Chapter, States.Standard);
    public static BlueFont? ColumnFont = Skin.GetBlueFont(Design.Table_Column, States.Standard);
    public Controls.ListBox Childs = new();

    private ConnectedFormula.ConnectedFormula? _cf;

    #endregion

    #region Constructors

    public TabFormulaPaditem() : this(UniqueInternal(), null) { }

    public TabFormulaPaditem(string intern, ConnectedFormula.ConnectedFormula? cf) : base(intern) {
        _cf = cf;
        if (_cf != null) {
            _cf.NotAllowedChilds.Changed += NotAllowedChilds_Changed;
        }
        Childs.ListOrItemChanged += Childs_ListOrItemChanged;
        Childs.ContextMenuInit += Childs_ContextMenuInit;
        Childs.ContextMenuItemClicked += Childs_ContextMenuItemClicked;
    }

    public TabFormulaPaditem(string intern) : this(intern, null) { }

    #endregion

    #region Properties

    protected override int SaveOrder => 1000;

    #endregion

    #region Methods

    public override System.Windows.Forms.Control? CreateControl(ConnectedFormulaView parent) {
        var c3 = new Controls.TabControl();
        c3.Tag = Internal;

        //CreateTabs(c3);

        if (GetRowFrom is ICalculateOneRowItemLevel rfw2) {
            var ff = parent.SearchOrGenerate((BasicPadItem)rfw2);
            if (ff is ICalculateRowsControlLevel cc) { cc.Childs.Add(c3); }
        }

        return c3;
    }

    public void CreateTabs(Controls.TabControl c3, string? myGroup, string? myName) {

        #region  Tabs erstellen

        foreach (var thisc in Childs.Item) {
            ConnectedFormula.ConnectedFormula? cf;
            string pg;
            string pgvis;

            if (thisc.Internal.EndsWith(".cfo", StringComparison.InvariantCultureIgnoreCase)) {
                cf = ConnectedFormula.ConnectedFormula.GetByFilename(thisc.Internal);
                pg = "Head";
                pgvis = string.Empty;
            } else {
                cf = _cf;
                pg = thisc.Internal;
                pgvis = thisc.Internal;
            }

            TabPage? existTab = null;

            foreach (var thisTab in c3.TabPages) {
                if (thisTab is TabPage tb) {
                    if (tb.Name == thisc.Internal.FileNameWithoutSuffix()) {
                        existTab = tb;
                        break;
                    }
                }
            }

            if (cf != null) {
                if (cf.HasVisibleItemsForMe(pgvis, myGroup, myName)) {
                    if (existTab == null) {
                        var t = new TabPage();
                        t.Name = thisc.Internal.FileNameWithoutSuffix();
                        t.Text = thisc.Internal.FileNameWithoutSuffix();
                        c3.TabPages.Add(t);

                        var cc = new ConnectedFormulaView();
                        t.Controls.Add(cc);
                        cc.ConnectedFormula = cf;
                        cc.Page = pg;
                        cc.Dock = DockStyle.Fill;
                    }
                } else {
                    if (existTab != null) {
                        foreach (var thisC in existTab.Controls) {
                            if (thisC is IDisposable c) {
                                c.Dispose();
                            }
                        }

                        c3.TabPages.Remove(existTab);
                        existTab.Dispose();
                    }
                }
            }
        }

        #endregion
    }

    public override List<GenericControl> GetStyleOptions() {
        List<GenericControl> l = new();

        UpdateList();
        l.AddRange(base.GetStyleOptions());
        l.Add(Childs);

        return l;
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }
        switch (tag) {
            case "parent":
                _cf = ConnectedFormula.ConnectedFormula.GetByFilename(value.FromNonCritical());
                if (_cf != null) {
                    _cf.NotAllowedChilds.Changed += NotAllowedChilds_Changed;
                }
                //_path = value.FromNonCritical();
                return true;

            case "path":
                //_path = value.FromNonCritical();
                return true;

            case "childs":
                var tmp = value.FromNonCritical().SplitAndCutBy("|");
                Childs.Item.Clear();
                Childs.Item.AddRange(tmp);
                return true;

            case "notallowedchilds":
                //var tmp2 = value.FromNonCritical().SplitAndCutBy("|");
                //NotAllowedChilds.Clear();
                //NotAllowedChilds.AddRange(tmp2);
                return true;
        }
        return false;
    }

    //    return false;
    //}
    public override string ToString() {
        var t = base.ToString();
        t = t.Substring(0, t.Length - 1) + ", ";

        if (_cf != null) {
            t = t + "Parent=" + _cf.Filename.ToNonCritical() + ", ";
        }
        //t = t + "Path=" + _path.ToNonCritical() + ", ";
        t = t + "Childs=" + Childs.Item.ToListOfString().JoinWith("|").ToNonCritical() + ", ";
        //t = t + "NotAllowedChilds=" + NotAllowedChilds.JoinWith("|").ToNonCritical() + ", ";
        return t.Trim(", ") + "}";
    }

    //public bool IsRecursiveWith(IRecursiveCheck obj) {
    //    if (obj == this) { return true; }
    protected override string ClassId() => "FI-ChildFormula";

    protected override void Dispose(bool disposing) {
        if (disposing) {
            if (_cf != null) {
                _cf.NotAllowedChilds.Changed -= NotAllowedChilds_Changed;
            }

            Childs.ListOrItemChanged -= Childs_ListOrItemChanged;
            Childs.ContextMenuInit -= Childs_ContextMenuInit;
            Childs.ContextMenuItemClicked -= Childs_ContextMenuItemClicked;
        }
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        //DrawColorScheme(gr, positionModified, zoom, Id);
        //s
        var headh = 25 * zoom;
        var headb = 70 * zoom;

        var body = new RectangleF(positionModified.X, positionModified.Y + headh, positionModified.Width, positionModified.Height - headh);
        var c = -1;
        foreach (var thisC in Childs.Item) {
            c++;
            var it = new RectangleF(positionModified.X + c * headb, positionModified.Y, headb, headh);

            gr.FillRectangle(new SolidBrush(Color.FromArgb(255, 200, 200, 200)), it);

            Skin.Draw_FormatedText(gr, thisC.Internal.FileNameWithoutSuffix(), null, Alignment.Horizontal_Vertical_Center, it.ToRect(), ColumnPadItem.ColumnFont.Scale(zoom), false);
            gr.DrawRectangle(new Pen(Color.Black, zoom), it);
        }

        gr.FillRectangle(new SolidBrush(Color.FromArgb(255, 200, 200, 200)), body);
        gr.DrawRectangle(new Pen(Color.Black, zoom), body);

        //Skin.Draw_FormatedText(gr, _text, QuickImage.Get(ImageCode.Textfeld, (int)(zoom * 16)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), ColumnPadItem.ColumnFont.Scale(zoom), false);
        Skin.Draw_FormatedText(gr, "Register-\r\nkarten", null, Alignment.Horizontal_Vertical_Center, body.ToRect(), ColumnPadItem.ColumnFont.Scale(zoom), false);
        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
    }

    protected override BasicPadItem? TryCreate(string id, string name) {
        if (id.Equals(ClassId(), StringComparison.OrdinalIgnoreCase)) {
            return new TabFormulaPaditem(name);
        }
        return null;
    }

    private void Childs_ContextMenuInit(object sender, EventArgs.ContextMenuInitEventArgs e) {
        e.UserMenu.Add(ContextMenuComands.Bearbeiten);
    }

    private void Childs_ContextMenuItemClicked(object sender, EventArgs.ContextMenuItemClickedEventArgs e) {
        if (e.ClickedComand.ToLower() == "bearbeiten") {
            MultiUserFile.SaveAll(false);

            var x = new ConnectedFormulaEditor(((BasicListItem)e.HotItem).Internal, _cf?.NotAllowedChilds);
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

        foreach (var thisl in _cf.NotAllowedChilds) {
            Childs.Item.Remove(thisl);
        }

        OnChanged();
    }

    private void UpdateList() {
        Childs.AddAllowed = AddType.OnlySuggests;
        Childs.RemoveAllowed = true;
        Childs.MoveAllowed = true;
        Childs.Suggestions.Clear();

        if (_cf != null && System.IO.File.Exists(_cf.Filename)) {
            foreach (var thisf in System.IO.Directory.GetFiles(_cf.Filename.FilePath(), "*.cfo")) {
                if (!_cf.NotAllowedChilds.Contains(thisf)) {
                    Childs.Suggestions.Add(thisf, ImageCode.Diskette);
                }
            }
        }

        if (_cf != null && _cf.PadData != null) {
            foreach (var thisf in _cf.PadData.AllPages()) {
                if (!_cf.NotAllowedChilds.Contains(thisf) && !string.Equals("Head", thisf, StringComparison.InvariantCultureIgnoreCase)) {
                    Childs.Suggestions.Add(thisf, ImageCode.Formel);
                }
            }
        }
    }

    #endregion
}