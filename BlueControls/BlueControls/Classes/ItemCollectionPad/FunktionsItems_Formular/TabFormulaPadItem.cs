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
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
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

    public TabFormulaPadItem() : this(Generic.GetUniqueKey(), null) {
    }

    public TabFormulaPadItem(string keyName, ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula) {
        if (ParentFormula != null) {
            ParentFormula.PropertyChanged += ParentFormula_PropertyChanged;
        }
    }

    public TabFormulaPadItem(string keyName) : this(keyName, null) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-ChildFormula";
    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.One;
    public bool AutoSizeableHeight => true;

    public override bool DatabaseInputMustMatchOutputDatabase => false;
    public override string Description => "Ein Tab-Control, dass weitere Unterformulare enthalten kann.";
    public override bool InputMustBeOneRow => true;
    public override bool MustBeInDrawingArea => true;
    public override string MyClassId => ClassId;

    protected override int SaveOrder => 1000;

    #endregion

    #region Methods

    public Control CreateControl(Controls.ConnectedFormulaView parent, string mode) {
        var con = new Controls.TabControl();
        con.Name = this.DefaultItemToControlName();
        // Die Input-Settings werden direkt auf das erzeugte
        //con.DoInputSettings(parent, this);
        //con.DoOutputSettings(parent, this);
        return con;
    }

    public void CreateTabs(Controls.TabControl tabctrl, Controls.ConnectedFormulaView parentView, string mode) {
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

            TabPage? existTab = null;

            foreach (var thisTab in tabctrl.TabPages) {
                if (thisTab is TabPage tb) {
                    if (tb.Name == thisc.FileNameWithoutSuffix()) {
                        existTab = tb;
                        break;
                    }
                }
            }

            #endregion

            if (cf != null) {
                if (cf.HasVisibleItemsForMe(pgvis, mode)) {
                    Controls.ConnectedFormulaView? cc;

                    if (existTab == null) {

                        #region Neuen Tab und ConnectedFormulaView (cc) erstellen

                        var t = new TabPage {
                            Name = thisc.FileNameWithoutSuffix(),
                            Text = thisc.FileNameWithoutSuffix()
                        };
                        tabctrl.TabPages.Add(t);

                        cc = new Controls.ConnectedFormulaView(mode, pg);
                        cc.GroupBoxStyle = GroupBoxStyle.Nothing;
                        t.Controls.Add(cc);
                        cc.InitFormula(cf, cc.DatabaseInput);
                        cc.Dock = DockStyle.Fill;
                        cc.DoDefaultSettings(parentView, this, mode);

                        //cc.HandleChangesNow();

                        //cc.GenerateView();

                        #endregion
                    } else {

                        #region ConnectedFormulaView (cc) im Tab Suchen

                        foreach (var thisControl in existTab.Controls) {
                            if (thisControl is Controls.ConnectedFormulaView cctmp) {
                                cc = cctmp;
                                break;
                            }
                        }

                        #endregion
                    }

                    //if (cc != null) {
                    //    cc.UserGroup = myGroup;
                    //    cc.UserName = myName;
                    //}
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
        List<GenericControl> l =
        [
            .. base.GetProperties(widthOfControl),
            new FlexiControl("Eigenschaften:", widthOfControl, true),
            new FlexiControl("Formulare:", -1, false),
            Childs(),
            .. base.GetProperties(widthOfControl),
        ];
        return l;
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
        const string txt = "Formulare: ";

        if (this.IsOk() && DatabaseInput != null) {
            return txt + DatabaseInput.Caption;
        }

        return txt + ErrorReason();
    }

    public override QuickImage SymbolForReadableText() {
        if (this.IsOk()) {
            return QuickImage.Get(ImageCode.Registersammlung, 16, Color.Transparent, Skin.IdColor(InputColorId));
        }

        return QuickImage.Get(ImageCode.Warnung, 16);
    }

    public override string ToParseableString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = [];

        result.ParseableAdd("Parent", ParentFormula?.Filename ?? string.Empty);
        result.ParseableAdd("Childs", _childs, false);
        return result.Parseable(base.ToParseableString());
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            if (ParentFormula != null) {
                ParentFormula.PropertyChanged -= ParentFormula_PropertyChanged;
            }
        }
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        DrawColorScheme(gr, positionModified, zoom, null, false, false, false);
        var headh = 25 * zoom;
        var headb = 70 * zoom;

        var body = positionModified with { Y = positionModified.Y + headh, Height = positionModified.Height - headh };
        var c = -1;
        foreach (var thisC in _childs) {
            c++;
            var it = new RectangleF(positionModified.X + (c * headb), positionModified.Y, headb, headh);

            gr.FillRectangle(new SolidBrush(Color.FromArgb(255, 200, 200, 200)), it);

            Skin.Draw_FormatedText(gr, thisC.FileNameWithoutSuffix(), null, Alignment.Horizontal_Vertical_Center, it.ToRect(), ColumnFont?.Scale(zoom), false);
            gr.DrawRectangle(new Pen(Color.Black, zoom), it);
        }

        gr.FillRectangle(new SolidBrush(Color.FromArgb(255, 200, 200, 200)), body);
        gr.DrawRectangle(new Pen(Color.Black, zoom), body);

        //Skin.Draw_FormatedText(gr, _text, QuickImage.Get(ImageCode.Textfeld, (int)(zoom * 16)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), ColumnPadItem.ColumnFont.Scale(zoom), false);
        //Skin.Draw_FormatedText(gr, "Register-\r\nkarten", null, Alignment.Horizontal_Vertical_Center, body.ToRect(), ColumnFont?.Scale(zoom), false);

        if (!forPrinting) {
            DrawColorScheme(gr, positionModified, zoom, InputColorId, true, true, true);
        }

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);

        DrawArrorInput(gr, positionModified, zoom, forPrinting, InputColorId);
    }

    private Controls.ListBox Childs() {
        var childs = new Controls.ListBox {
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
        if (sender is Controls.ListBox childs) {
            childs.ItemCheckedChanged -= Childs_ItemCheckedChanged;
            childs.Disposed -= Childs_Disposed;
        }
    }

    private void Childs_ItemCheckedChanged(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        _childs.Clear();
        _childs.AddRange(((Controls.ListBox)sender).Checked);
        OnPropertyChanged();
        this.RaiseVersion();
        UpdateSideOptionMenu();
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