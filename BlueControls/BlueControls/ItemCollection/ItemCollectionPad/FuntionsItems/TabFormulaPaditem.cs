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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.MultiUserFile;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollection.ItemCollectionList;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.ItemCollection;

public class TabFormulaPadItem : CustomizableShowPadItem, IItemToControl {

    #region Fields

    private readonly List<string> _childs = new();

    private ConnectedFormula.ConnectedFormula? _cf;

    #endregion

    #region Constructors

    public TabFormulaPadItem() : this(BlueBasics.Generic.UniqueInternal(), null) { }

    public TabFormulaPadItem(string intern, ConnectedFormula.ConnectedFormula? cf) : base(intern) {
        _cf = cf;
        if (_cf != null) {
            _cf.NotAllowedChilds.Changed += NotAllowedChilds_Changed;
        }
    }

    public TabFormulaPadItem(string intern) : this(intern, null) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-ChildFormula";

    protected override int SaveOrder => 1000;

    #endregion

    #region Methods

    public override Control CreateControl(ConnectedFormulaView parent) {
        var con = new Controls.TabControl {
            Name = DefaultItemToControlName()
        };

        if (GetRowFrom is ICalculateRowsItemLevel rfw2) {
            var ff = parent.SearchOrGenerate(rfw2);
            if (ff is ICalculateRowsControlLevel cc) { cc.Childs.Add(con); }
        }

        return con;
    }

    public void CreateTabs(Controls.TabControl c3, string? myGroup, string? myName) {
        // Eigentlich überpowert die Routine.
        // Sie checkt und aktualisiert die Tabs.
        // Da der Versioncheck aber verlangt, dass immer das tab-Control gelöscht und neu erstellt wird
        // ist das eigentlich nicht nötig

        #region  Tabs erstellen

        foreach (var thisc in _childs) {
            ConnectedFormula.ConnectedFormula? cf;
            string pg;
            string pgvis;

            if (thisc.EndsWith(".cfo", StringComparison.OrdinalIgnoreCase)) {
                cf = ConnectedFormula.ConnectedFormula.GetByFilename(thisc);
                pg = "Head";
                pgvis = string.Empty;
            } else {
                cf = _cf;
                pg = thisc;
                pgvis = thisc;
            }

            TabPage? existTab = null;

            foreach (var thisTab in c3.TabPages) {
                if (thisTab is TabPage tb) {
                    if (tb.Name == thisc.FileNameWithoutSuffix()) {
                        existTab = tb;
                        break;
                    }
                }
            }

            if (cf != null) {
                if (cf.HasVisibleItemsForMe(pgvis, myGroup, myName)) {
                    if (existTab == null) {
                        var t = new TabPage {
                            Name = thisc.FileNameWithoutSuffix(),
                            Text = thisc.FileNameWithoutSuffix()
                        };
                        c3.TabPages.Add(t);

                        var cc = new ConnectedFormulaView(pg);
                        t.Controls.Add(cc);
                        cc.ConnectedFormula = cf;
                        cc.Dock = DockStyle.Fill;
                    }
                } else {
                    if (existTab != null) {
                        foreach (var thisC in existTab.Controls) {
                            if (thisC is IDisposable c) {
                                c?.Dispose();
                            }
                        }

                        c3.TabPages.Remove(existTab);
                        existTab?.Dispose();
                    }
                }
            }
        }

        #endregion
    }

    public override List<GenericControl> GetStyleOptions() {
        List<GenericControl> l = new();
        l.AddRange(base.GetStyleOptions());
        l.Add(Childs());

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
                return true;

            case "path":
                return true;

            case "childs":
                var tmp = value.FromNonCritical().SplitAndCutBy("|");
                _childs.Clear();
                _childs.AddRange(tmp);
                return true;

            case "notallowedchilds":
                return true;
        }
        return false;
    }

    public override string ToString() {
        var result = new List<string>();
        result.ParseableAdd("Parent", _cf);
        result.ParseableAdd("Childs", _childs);
        return result.Parseable(base.ToString());
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            if (_cf != null) {
                _cf.NotAllowedChilds.Changed -= NotAllowedChilds_Changed;
            }
        }
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        //DrawColorScheme(gr, positionModified, zoom, Id);

        var headh = 25 * zoom;
        var headb = 70 * zoom;

        var body = positionModified with { Y = positionModified.Y + headh, Height = positionModified.Height - headh };
        var c = -1;
        foreach (var thisC in _childs) {
            c++;
            var it = new RectangleF(positionModified.X + (c * headb), positionModified.Y, headb, headh);

            gr.FillRectangle(new SolidBrush(Color.FromArgb(255, 200, 200, 200)), it);

            Skin.Draw_FormatedText(gr, thisC.FileNameWithoutSuffix(), null, Alignment.Horizontal_Vertical_Center, it.ToRect(), ColumnFont.Scale(zoom), false);
            gr.DrawRectangle(new Pen(Color.Black, zoom), it);
        }

        gr.FillRectangle(new SolidBrush(Color.FromArgb(255, 200, 200, 200)), body);
        gr.DrawRectangle(new Pen(Color.Black, zoom), body);

        //Skin.Draw_FormatedText(gr, _text, QuickImage.Get(ImageCode.Textfeld, (int)(zoom * 16)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), ColumnPadItem.ColumnFont.Scale(zoom), false);
        Skin.Draw_FormatedText(gr, "Register-\r\nkarten", null, Alignment.Horizontal_Vertical_Center, body.ToRect(), ColumnFont.Scale(zoom), false);
        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
    }

    //protected override BasicPadItem? TryCreate(string id, string name) {
    //    if (id.Equals(ClassId, StringComparison.OrdinalIgnoreCase)) {
    //        return new TabFormulaPadItem(name);
    //    }
    //    return null;
    //}

    private Controls.ListBox Childs() {
        var childs = new Controls.ListBox {
            AddAllowed = AddType.OnlySuggests,
            RemoveAllowed = true,
            MoveAllowed = true
        };
        childs.Suggestions.Clear();

        if (_cf != null && System.IO.File.Exists(_cf.Filename)) {
            foreach (var thisf in System.IO.Directory.GetFiles(_cf.Filename.FilePath(), "*.cfo")) {
                if (!_cf.NotAllowedChilds.Contains(thisf)) {
                    _ = childs.Suggestions.Add(thisf, ImageCode.Diskette);
                }
            }
        }

        if (_cf != null && _cf.PadData != null) {
            foreach (var thisf in _cf.PadData.AllPages()) {
                if (!_cf.NotAllowedChilds.Contains(thisf) && !string.Equals("Head", thisf, StringComparison.OrdinalIgnoreCase)) {
                    _ = childs.Suggestions.Add(thisf, ImageCode.Formel);
                }
            }
        }

        foreach (var thisf in _childs) {
            _ = childs.Item.Add(thisf, System.IO.File.Exists(thisf) ? ImageCode.Diskette : ImageCode.Formel);
        }

        childs.ListOrItemChanged += Childs_ListOrItemChanged;
        childs.ContextMenuInit += Childs_ContextMenuInit;
        childs.ContextMenuItemClicked += Childs_ContextMenuItemClicked;
        childs.Disposed += Childs_Disposed;

        return childs;
    }

    private void Childs_ContextMenuInit(object sender, EventArgs.ContextMenuInitEventArgs e) => e.UserMenu.Add(ContextMenuComands.Bearbeiten);

    private void Childs_ContextMenuItemClicked(object sender, EventArgs.ContextMenuItemClickedEventArgs e) {
        if (e.ClickedComand.ToLower() == "bearbeiten") {
            MultiUserFile.SaveAll(false);

            var x = new ConnectedFormulaEditor(((BasicListItem)e.HotItem).KeyName, _cf?.NotAllowedChilds);
            _ = x.ShowDialog();
            MultiUserFile.SaveAll(false);
            x?.Dispose();
        }
    }

    private void Childs_Disposed(object sender, System.EventArgs e) {
        if (sender is Controls.ListBox childs) {
            childs.ListOrItemChanged -= Childs_ListOrItemChanged;
            childs.ContextMenuInit -= Childs_ContextMenuInit;
            childs.ContextMenuItemClicked -= Childs_ContextMenuItemClicked;
            childs.Disposed -= Childs_Disposed;
        }
    }

    private void Childs_ListOrItemChanged(object sender, System.EventArgs e) {
        _childs.Clear();
        _childs.AddRange(((Controls.ListBox)sender).Item.ToListOfString());
        OnChanged();
        RaiseVersion();
    }

    private void NotAllowedChilds_Changed(object sender, System.EventArgs e) {
        foreach (var thisl in _cf.NotAllowedChilds) {
            _ = _childs.Remove(thisl);
        }

        OnChanged();
    }

    #endregion
}