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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using ListBox = BlueControls.Controls.ListBox;
using TabControl = BlueControls.Controls.TabControl;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

/// <summary>
/// Erzeut ein Tab-Formula, das weitere Formulare enthalten kann
/// </summary>
public class TabFormulaPadItem : ReciverControlPadItem, IItemToControl, IAutosizable {

    #region Fields

    private readonly List<string> _childs = [];

    #endregion

    #region Constructors

    public TabFormulaPadItem() : this(string.Empty, null) { }

    public TabFormulaPadItem(string keyName, ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula) {
        if (ParentFormula != null) {
            ParentFormula.PropertyChanged += ParentFormula_PropertyChanged;
        }
    }

    #endregion

    #region Properties

    public static string ClassId => "FI-ChildFormula";
    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.One;
    public bool AutoSizeableHeight => true;

    public override string Description => "Ein Tab-Control, dass weitere Unterformulare enthalten kann.";
    public override bool InputMustBeOneRow => true;
    public override bool MustBeInDrawingArea => true;
    public override bool TableInputMustMatchOutputTable => false;
    protected override int SaveOrder => 1000;

    #endregion

    #region Methods

    public Control CreateControl(ConnectedFormulaView parent, string mode) {
        var con = new TabControl {
            Name = this.DefaultItemToControlName(parent?.Page?.UniqueId)
        };
        // Die Input-Settings werden direkt auf das erzeugte
        //con.DoInputSettings(parent, this);
        //con.DoOutputSettings(parent, this);
        return con;
    }

    public void CreateTabs(TabControl tabctrl, ConnectedFormulaView parentView, string mode) {
        // Eigentlich überpowert die Routine.
        // Sie checkt und aktualisiert die Tabs.
        // Da der Versioncheck aber verlangt, dass immer das tab-Control gelöscht und neu erstellt wird
        // ist das eigentlich nicht nötig

        var tmpc = ListOfChildsPages();

        foreach (var thisPage in tmpc) {

            #region Prüfen, ob der Tab schon vorhanden ist (existsTab)

            TabPage? existTab = null;

            foreach (var thisTab in tabctrl.TabPages) {
                if (thisTab is TabPage tb) {
                    if (tb.Name == thisPage.KeyName) {
                        existTab = tb;
                        break;
                    }
                }
            }

            #endregion

            if (thisPage.HasVisibleItemsForMe(mode)) {
                ConnectedFormulaView? cc;

                if (existTab == null) {

                    #region Neuen Tab und ConnectedFormulaView (cc) erstellen

                    var t = new TabPage {
                        Name = thisPage.KeyName,
                        Text = thisPage.Caption,
                    };
                    tabctrl.TabPages.Add(t);

                    cc = new ConnectedFormulaView(mode, thisPage) {
                        GroupBoxStyle = GroupBoxStyle.Nothing
                    };
                    t.Controls.Add(cc);
                    //cc.InitFormula(thisPage, cc.TableInput);
                    cc.Dock = DockStyle.Fill;
                    cc.DoDefaultSettings(parentView, this, mode);

                    //cc.HandleChangesNow();

                    //cc.GenerateView();

                    #endregion
                } else {

                    #region ConnectedFormulaView (cc) im Tab Suchen

                    foreach (var thisControl in existTab.Controls) {
                        if (thisControl is ConnectedFormulaView cctmp) {
                            cc = cctmp;
                            break;
                        }
                    }

                    #endregion
                }
            } else {
                if (existTab != null) {

                    #region Tab löschen

                    foreach (var thisC in existTab.Controls) {
                        if (thisC is IDisposable c) {
                            c.Dispose();
                        }
                    }

                    tabctrl.TabPages.Remove(existTab);
                    existTab.Dispose();

                    #endregion
                }
            }
        }
    }

    public override string ErrorReason() {
        if (_childs.Count == 0) {
            return "Keine Formulare gewählt.";
        }

        return base.ErrorReason();
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [
            .. base.GetProperties(widthOfControl),
            new FlexiControl("Einstellungen:", widthOfControl, true),
            new FlexiControl("Formulare:", -1, false),
            Childs()
        ];
        return result;
    }

    public List<ItemCollectionPadItem> ListOfChildsPages() {
        var tmp = new List<ItemCollectionPadItem>();

        foreach (var thisc in _childs) {
            if (GetChild(thisc) is { IsDisposed: false } icpi) { tmp.AddIfNotExists(icpi); }
        }
        return tmp;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("Parent", ParentFormula?.Filename ?? string.Empty);
        result.ParseableAdd("Childs", _childs, false);
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "parent":
                ParentFormula = ConnectedFormula.ConnectedFormula.GetByFilename(value.FromNonCritical());
                if (ParentFormula != null) {
                    ParentFormula.PropertyChanged += ParentFormula_PropertyChanged;
                }
                return true;

            case "path":
                return true;

            case "childs":
                var tmp = value.FromNonCritical().SplitBy("|");
                _childs.Clear();
                foreach (var thiss in tmp) {
                    _childs.AddIfNotExists(thiss.FromNonCritical());
                }
                return true;

            case "style":
            case "notallowedchilds":
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() {
        const string txt = "Register-Karten: ";

        return txt + TableInput?.Caption;
    }

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Registersammlung, 16, Color.Transparent, Skin.IdColor(InputColorId));

    protected override void Dispose(bool disposing) {
        if (disposing) {
            if (ParentFormula != null) {
                ParentFormula.PropertyChanged -= ParentFormula_PropertyChanged;
            }
        }
    }

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float zoom, float offsetX, float offsetY) {
        DrawColorScheme(gr, positionControl, zoom, null, false, false, false);
        var headh = 25.CanvasToControl(zoom);
        var headb = 70.CanvasToControl(zoom);

        var body = positionControl with { Y = positionControl.Y + headh, Height = positionControl.Height - headh };
        var c = -1;

        var lc = ListOfChildsPages();

        foreach (var thisC in lc) {
            c++;
            var it = new RectangleF(positionControl.X + (c * headb), positionControl.Y, headb, headh);

            gr.FillRectangle(new SolidBrush(Color.FromArgb(255, 200, 200, 200)), it);

            Skin.Draw_FormatedText(gr, thisC.BestCaption(), null, Alignment.Horizontal_Vertical_Center, it.ToRect(), ColumnFont?.Scale(zoom), false);
            gr.DrawRectangle(new Pen(Color.Black, zoom), it);
        }

        gr.FillRectangle(new SolidBrush(Color.FromArgb(255, 200, 200, 200)), body);
        gr.DrawRectangle(new Pen(Color.Black, zoom), body);

        if (!ForPrinting) {
            DrawColorScheme(gr, positionControl, zoom, InputColorId, true, true, true);
        }

        base.DrawExplicit(gr, visibleAreaControl, positionControl, zoom, offsetX, offsetY);

        DrawArrorInput(gr, positionControl, zoom, ForPrinting, InputColorId);
    }

    private ListBox Childs() {
        var childs = new ListBox {
            AddAllowed = AddType.OnlySuggests,
            RemoveAllowed = true,
            MoveAllowed = true,
            AutoSort = false,
            ItemEditAllowed = true,
            CheckBehavior = CheckBehavior.AllSelected,
        };

        var tmp = ListOfChildsPages();

        List<string> notUse = [.. tmp.ToListOfString(), .. ParentFormula?.NotAllowedChilds];

        var allchilds = ParentFormula?.AllKnownChilds(notUse.AsReadOnly());
        childs.Suggestions.Clear();
        childs.Suggestions.AddRange(allchilds);

        foreach (var thisc in tmp) {
            childs.AddAndCheck(ItemOf(thisc));
        }

        childs.ItemCheckedChanged += Childs_ItemCheckedChanged;
        childs.Disposed += Childs_Disposed;
        childs.ParentChanged += Childs_ParentChanged;

        return childs;
    }

    private void Childs_Disposed(object sender, System.EventArgs e) {
        if (sender is ListBox childs) {
            childs.ItemCheckedChanged -= Childs_ItemCheckedChanged;
            childs.Disposed -= Childs_Disposed;
            childs.ParentChanged -= Childs_ParentChanged;
        }
    }

    private void Childs_ItemCheckedChanged(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        if (sender is not ListBox lb) { return; }

        _childs.Clear();

        foreach (var thisIt in lb.Items) {
            if (thisIt is ReadableListItem rli && rli.Item is ItemCollectionPadItem icpi) {
                if (icpi.IsHead()) {
                    _childs.AddIfNotExists(icpi.GetConnectedFormula()?.Filename ?? string.Empty);
                } else {
                    _childs.AddIfNotExists(icpi.Caption);
                }
            } else {
                _childs.AddIfNotExists(thisIt.KeyName);
            }
        }

        //_childs.AddRange(((ListBox)sender).Checked);
        OnPropertyChanged();
        this.RaiseVersion();
        OnDoUpdateSideOptionMenu();
    }

    private void Childs_ParentChanged(object sender, System.EventArgs e) {
        if (sender is ListBox { Parent: null } childs) {
            childs.Dispose();
        }
    }

    private void ParentFormula_PropertyChanged(object sender, PropertyChangedEventArgs e) {
        if (IsDisposed) { return; }
        if (ParentFormula == null) { return; }

        foreach (var thisl in ParentFormula.NotAllowedChilds) {
            _childs.Remove(thisl);
        }

        //OnPropertyChanged(string propertyname);
    }

    #endregion
}