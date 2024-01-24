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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueBasics.MultiUserFile;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueDatabase;
using ListBox = BlueControls.Controls.ListBox;
using TabControl = BlueControls.Controls.TabControl;

#nullable enable

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

/// <summary>
/// Erzeut ein Tab-Formula, das weitere Formulare enthalten kann
/// </summary>
public class TabFormulaPadItem : FakeControlPadItem, IHasConnectedFormula, IItemAcceptSomething, IAutosizable {

    #region Fields

    private readonly List<string> _childs = [];
    private readonly ItemAcceptSomething _itemAccepts;

    #endregion

    #region Constructors

    public TabFormulaPadItem() : this(Generic.UniqueInternal(), null) {
    }

    public TabFormulaPadItem(string intern, ConnectedFormula.ConnectedFormula? cf) : base(intern) {
        _itemAccepts = new();

        CFormula = cf;
        if (CFormula != null) {
            CFormula.NotAllowedChildsChanged += NotAllowedChilds_Changed;
        }
    }

    public TabFormulaPadItem(string intern) : this(intern, null) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-ChildFormula";

    public bool AutoSizeableHeight => true;

    /// <summary>
    /// Wird benötigt bei ToString - um die eigenen Ansichten wieder zu finden.
    /// </summary>
    public ConnectedFormula.ConnectedFormula? CFormula { get; set; }

    public Database? DatabaseInput => _itemAccepts.DatabaseInput(this);
    public Database? DatabaseInputMustBe => null;
    public override string Description => "Ein Tab-Control, dass weitere Unterformulare enthalten kann.";

    public List<int> InputColorId => _itemAccepts.InputColorIdGet(this);
    public override bool MustBeInDrawingArea => true;

    public bool MustBeOneRow => true;

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ReadOnlyCollection<string> Parents {
        get => _itemAccepts.GetFilterFromKeysGet();
        set => _itemAccepts.GetFilterFromKeysSet(value, this);
    }

    public bool WaitForDatabase => false;
    protected override int SaveOrder => 1000;

    #endregion

    #region Methods

    public void CalculateInputColorIds() => _itemAccepts.CalculateInputColorIds(this);

    public override Control CreateControl(ConnectedFormulaView parent) {
        var con = new TabControl();
        con.Name = this.DefaultItemToControlName();
        // Die Input-Settings werden direkt auf das erzeugte
        //con.DoInputSettings(parent, this);
        //con.DoOutputSettings(parent, this);
        return con;
    }

    public void CreateTabs(TabControl tabctrl, ConnectedFormulaView parentView) {
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
                cf = CFormula;
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
                if (cf.HasVisibleItemsForMe(pgvis)) {
                    ConnectedFormulaView? cc;

                    if (existTab == null) {

                        #region Neuen Tab und ConnectedFormulaView (cc) erstellen

                        var t = new TabPage {
                            Name = thisc.FileNameWithoutSuffix(),
                            Text = thisc.FileNameWithoutSuffix()
                        };
                        tabctrl.TabPages.Add(t);

                        cc = new ConnectedFormulaView(pg);
                        t.Controls.Add(cc);
                        cc.InitFormula(cf, null);
                        cc.Dock = DockStyle.Fill;
                        cc.DoInputSettings(parentView, this);
                        cc.DoInputFilter();
                        cc.DoOutputSettings(cc.FilterInput?.Database, cc.Name);
                        cc.GenerateView();

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
        var b = base.ErrorReason();
        if (!string.IsNullOrEmpty(b)) { return b; }

        b = _itemAccepts.ErrorReason(this);
        if (!string.IsNullOrEmpty(b)) { return b; }

        //b = _itemSends.ErrorReason(this);
        //if (!string.IsNullOrEmpty(b)) { return b; }

        if (_childs.Count == 0) {
            return "Keine Formulare gewählt.";
        }

        return string.Empty;
    }

    public override List<GenericControl> GetStyleOptions(int widthOfControl) {
        List<GenericControl> l =
        [
            .. _itemAccepts.GetStyleOptions(this, widthOfControl),
            new FlexiControl("Formulare:", -1),
            Childs(),
            new FlexiControl(),
            .. base.GetStyleOptions(widthOfControl),
        ];
        return l;
    }

    public override void ParseFinished(string parsed) {
        base.ParseFinished(parsed);
        //_itemSends.ParseFinished(this);
        _itemAccepts.ParseFinished(this);
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }
        if (_itemAccepts.ParseThis(tag, value)) { return true; }

        switch (tag) {
            case "parent":
                CFormula = ConnectedFormula.ConnectedFormula.GetByFilename(value.FromNonCritical());
                if (CFormula != null) {
                    CFormula.NotAllowedChildsChanged += NotAllowedChilds_Changed;
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
        return false;
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

    public override string ToString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = [.. _itemAccepts.ParsableTags()];

        result.ParseableAdd("Parent", CFormula);
        result.ParseableAdd("Childs", _childs);
        return result.Parseable(base.ToString());
    }

    internal override void AddedToCollection() {
        base.AddedToCollection();
        //_itemSends.DoCreativePadAddedToCollection(this);
        _itemAccepts.DoCreativePadAddedToCollection(this);
        //RepairConnections();
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            if (CFormula != null) {
                CFormula.NotAllowedChildsChanged -= NotAllowedChilds_Changed;
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

        DrawArrorInput(gr, positionModified, zoom, shiftX, shiftY, forPrinting, InputColorId);
    }

    //protected override AbstractPadItem? TryCreate(string id, string name) {
    //    if (id.Equals(ClassId, StringComparison.OrdinalIgnoreCase)) {
    //        return new TabFormulaPadItem(name);
    //    }
    //    return null;
    //}

    private ListBox Childs() {
        var childs = new ListBox {
            AddAllowed = AddType.OnlySuggests,
            RemoveAllowed = true,
            MoveAllowed = true,
            AutoSort = false
        };
        childs.Suggestions.Clear();

        if (CFormula != null && File.Exists(CFormula.Filename)) {
            foreach (var thisf in Directory.GetFiles(CFormula.Filename.FilePath(), "*.cfo")) {
                if (!CFormula.NotAllowedChilds.Contains(thisf)) {
                    _ = childs.Suggestions.Add(thisf, ImageCode.Diskette);
                }
            }
        }

        if (CFormula != null) {
            foreach (var thisf in ConnectedFormula.ConnectedFormula.AllFiles) {
                if (!CFormula.NotAllowedChilds.Contains(thisf.Filename)) {
                    if (childs.Suggestions[thisf.Filename] == null) {
                        _ = childs.Suggestions.Add(thisf.Filename, ImageCode.Diskette);
                    }
                }
            }
        }

        if (CFormula?.PadData != null) {
            foreach (var thisf in CFormula.PadData.AllPages()) {
                if (!CFormula.NotAllowedChilds.Contains(thisf) && !string.Equals("Head", thisf, StringComparison.OrdinalIgnoreCase)) {
                    _ = childs.Suggestions.Add(thisf, ImageCode.Formel);
                }
            }
        }

        foreach (var thisf in _childs) {
            _ = childs.Item.Add(thisf, File.Exists(thisf) ? ImageCode.Diskette : ImageCode.Formel);
        }

        childs.CollectionChanged += Childs_CollectionChanged;
        childs.ContextMenuInit += Childs_ContextMenuInit;
        childs.ContextMenuItemClicked += Childs_ContextMenuItemClicked;
        childs.Disposed += Childs_Disposed;

        return childs;
    }

    private void Childs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
        if (IsDisposed) { return; }
        _childs.Clear();
        _childs.AddRange(((ListBox)sender).Item.ToListOfString());
        OnChanged();
        this.RaiseVersion();
        UpdateSideOptionMenu();
    }

    private void Childs_ContextMenuInit(object sender, ContextMenuInitEventArgs e) => e.UserMenu.Add(ContextMenuCommands.Bearbeiten);

    private void Childs_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
        if (e.HotItem is not AbstractListItem it) { return; }

        if (e.ClickedCommand.ToLower() == "bearbeiten") {
            MultiUserFile.SaveAll(false);

            var x = new ConnectedFormulaEditor(it.KeyName, CFormula?.NotAllowedChilds);
            _ = x.ShowDialog();
            MultiUserFile.SaveAll(false);
            x.Dispose();
        }
    }

    private void Childs_Disposed(object sender, System.EventArgs e) {
        if (sender is ListBox childs) {
            childs.CollectionChanged -= Childs_CollectionChanged;
            childs.ContextMenuInit -= Childs_ContextMenuInit;
            childs.ContextMenuItemClicked -= Childs_ContextMenuItemClicked;
            childs.Disposed -= Childs_Disposed;
        }
    }

    private void NotAllowedChilds_Changed(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        if (CFormula == null) { return; }

        foreach (var thisl in CFormula.NotAllowedChilds) {
            _ = _childs.Remove(thisl);
        }

        OnChanged();
    }

    #endregion
}