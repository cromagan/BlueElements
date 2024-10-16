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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

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

    public override bool DatabaseInputMustMatchOutputDatabase => false;
    public override string Description => "Ein Tab-Control, dass weitere Unterformulare enthalten kann.";
    public override bool InputMustBeOneRow => true;
    public override bool MustBeInDrawingArea => true;

    protected override int SaveOrder => 1000;

    #endregion

    #region Methods

    public System.Windows.Forms.Control CreateControl(ConnectedFormulaView parent, string mode) {
        var con = new TabControl();
        con.Name = this.DefaultItemToControlName(ParentFormula?.Filename);
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

        foreach (var thisc in _childs) {
            ConnectedFormula.ConnectedFormula? cf;
            string pg;
            string pgvis;

            #region Connected Formuala (cf)  ermitteln und evtl. von festplatte laden

            if (thisc.EndsWith(".cfo", StringComparison.OrdinalIgnoreCase)) {
                cf = ConnectedFormula.ConnectedFormula.GetByFilename(thisc);
                pg = "Head";
                pgvis = string.Empty;
            } else {
                cf = ParentFormula;
                pg = thisc;
                pgvis = thisc;
            }

            #endregion

            #region Prüfen, ob der Tab schon vorhanden ist (existsTab)

            System.Windows.Forms.TabPage? existTab = null;

            foreach (var thisTab in tabctrl.TabPages) {
                if (thisTab is System.Windows.Forms.TabPage tb) {
                    if (tb.Name == thisc.FileNameWithoutSuffix()) {
                        existTab = tb;
                        break;
                    }
                }
            }

            #endregion

            if (cf != null) {
                if (cf.HasVisibleItemsForMe(pgvis, mode)) {
                    ConnectedFormulaView? cc;

                    if (existTab == null) {

                        #region Neuen Tab und ConnectedFormulaView (cc) erstellen

                        var t = new System.Windows.Forms.TabPage {
                            Name = thisc.FileNameWithoutSuffix(),
                            Text = thisc.FileNameWithoutSuffix()
                        };
                        tabctrl.TabPages.Add(t);

                        cc = new ConnectedFormulaView(mode, pg);
                        cc.GroupBoxStyle = GroupBoxStyle.Nothing;
                        t.Controls.Add(cc);
                        cc.InitFormula(cf, cc.DatabaseInput);
                        cc.Dock = System.Windows.Forms.DockStyle.Fill;
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

            case "notallowedchilds":
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() {
        const string txt = "Register-Karten: ";

        return txt + DatabaseInput?.Caption;
    }

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Registersammlung, 16, Color.Transparent, Skin.IdColor(InputColorId));

    protected override void Dispose(bool disposing) {
        if (disposing) {
            if (ParentFormula != null) {
                ParentFormula.PropertyChanged -= ParentFormula_PropertyChanged;
            }
        }
    }

    protected override void DrawExplicit(Graphics gr, Rectangle visibleArea, RectangleF positionModified, float scale, float shiftX, float shiftY) {
        DrawColorScheme(gr, positionModified, scale, null, false, false, false);
        var headh = 25 * scale;
        var headb = 70 * scale;

        var body = positionModified with { Y = positionModified.Y + headh, Height = positionModified.Height - headh };
        var c = -1;
        foreach (var thisC in _childs) {
            c++;
            var it = new RectangleF(positionModified.X + (c * headb), positionModified.Y, headb, headh);

            gr.FillRectangle(new SolidBrush(Color.FromArgb(255, 200, 200, 200)), it);

            Skin.Draw_FormatedText(gr, thisC.FileNameWithoutSuffix(), null, Alignment.Horizontal_Vertical_Center, it.ToRect(), ColumnFont?.Scale(scale), false);
            gr.DrawRectangle(new Pen(Color.Black, scale), it);
        }

        gr.FillRectangle(new SolidBrush(Color.FromArgb(255, 200, 200, 200)), body);
        gr.DrawRectangle(new Pen(Color.Black, scale), body);

        //Skin.Draw_FormatedText(gr, _text, QuickImage.Get(ImageCode.Textfeld, (int)(zoom * 16)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), ColumnPadItem.ColumnFont.Scale(zoom), false);
        //Skin.Draw_FormatedText(gr, "Register-\r\nkarten", null, Alignment.Horizontal_Vertical_Center, body.ToRect(), ColumnFont?.Scale(zoom), false);

        if (!ForPrinting) {
            DrawColorScheme(gr, positionModified, scale, InputColorId, true, true, true);
        }

        base.DrawExplicit(gr, visibleArea, positionModified, scale, shiftX, shiftY);

        DrawArrorInput(gr, positionModified, scale, ForPrinting, InputColorId);
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

        ParentFormula?.AddChilds(childs.Suggestions, ParentFormula.NotAllowedChilds);

        foreach (var thisf in _childs) {
            if (File.Exists(thisf)) {
                var c = ConnectedFormula.ConnectedFormula.GetByFilename(thisf);

                if (c != null) {
                    c.Editor = typeof(ConnectedFormulaEditor);
                    childs.AddAndCheck(ItemOf(c));
                }
            } else {
                childs.AddAndCheck(new TextListItem(thisf, thisf, QuickImage.Get(ImageCode.Register, 16), false, true, string.Empty));
            }
        }

        childs.ItemCheckedChanged += Childs_ItemCheckedChanged;
        childs.Disposed += Childs_Disposed;

        return childs;
    }

    private void Childs_Disposed(object sender, System.EventArgs e) {
        if (sender is ListBox childs) {
            childs.ItemCheckedChanged -= Childs_ItemCheckedChanged;
            childs.Disposed -= Childs_Disposed;
        }
    }

    private void Childs_ItemCheckedChanged(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        _childs.Clear();
        _childs.AddRange(((ListBox)sender).Checked);
        OnPropertyChanged();
        this.RaiseVersion();
        OnDoUpdateSideOptionMenu();
    }

    private void ParentFormula_PropertyChanged(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        if (ParentFormula == null) { return; }

        foreach (var thisl in ParentFormula.NotAllowedChilds) {
            _ = _childs.Remove(thisl);
        }

        //OnPropertyChanged();
    }

    #endregion
}