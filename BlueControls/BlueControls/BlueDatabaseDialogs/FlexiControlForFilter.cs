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
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public partial class FlexiControlForFilter : FlexiControl, IContextMenu {
    //public FlexiControlForFilter() : this(null, null) { }

    #region Constructors

    public FlexiControlForFilter(Table? tableView, FilterItem filter) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        Size = new Size(300, 300);
        TableView = tableView;
        Filter = filter;
        UpdateFilterData();
        Filter.Changed += Filter_Changed;
    }

    #endregion

    #region Events

    public event EventHandler<ContextMenuInitEventArgs>? ContextMenuInit;

    public event EventHandler<ContextMenuItemClickedEventArgs>? ContextMenuItemClicked;

    #endregion

    #region Properties

    /// <summary>
    /// ACHTUNG: Das Control wird niemals den Filter selbst ändern.
    /// Der Filter wird nur zur einfacheren Identifizierung der nachfolgenden Steuerelemente behalten.
    /// </summary>
    public FilterItem Filter { get; }

    public Table? TableView { get; }

    #endregion

    #region Methods

    public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) {
        switch (e.ClickedComand.ToLower()) {
            case "#columnedit":
                if (e.HotItem is ColumnItem col) {
                    Forms.TableView.OpenColumnEditor(col, null);
                }
                return true;

                //case "#filterverschieben":
                //    if (e.HotItem is ColumnItem col2) {
                //        var pc = (Filterleiste)Parent; // Parent geht verlren, wenn der Filter selbst disposed und neu erzeugt wird
                //        while (true) {
                //            var nx = InputBox.Show("X, von 0 bis 10000", col2.DauerFilterPos.X.ToString(), enFormatHolder.Integer);
                //            if (string.IsNullOrEmpty(nx)) { return true; }
                //            var nxi = Converter.IntParse(nx);
                //            nxi = Math.Max(nxi, 0);
                //            nxi = Math.Min(nxi, 10000);
                //            var ny = InputBox.Show("Y, von 0 bis 10000", col2.DauerFilterPos.Y.ToString(), enFormatHolder.Integer);
                //            if (string.IsNullOrEmpty(ny)) { return true; }
                //            var nyi = Converter.IntParse(ny);
                //            nyi = Math.Max(nyi, 0);
                //            nyi = Math.Min(nyi, 10000);
                //            col2.DauerFilterPos = new Point(nxi, nyi);
                //            pc.FillFilters();
                //        }
                //    }
                //    return true;

                //case "#bildpfad":
                //    var p = (string)((Filterleiste)Parent).pic.Tag;
                //    ExecuteFile(p.FilePath());
                //    MessageBox.Show("Aktuelle Datei:<br>" + p);
                //    return true;

                //default:
                //    //if (Parent is Formula f) {
                //    //    return f.ContextMenuItemClickedInternalProcessig(sender, e);
                //    //}
                //    break;
        }
        return false;
    }

    public void GetContextMenuItems(MouseEventArgs? e, ItemCollectionList items, out object? hotItem, List<string> tags, ref bool cancel, ref bool translate) {
        hotItem = null;
        if (Filter.Column?.Database == null || !Filter.Column.Database.IsAdministrator()) { return; }

        hotItem = Filter.Column;
        _ = items.Add("Spalte bearbeiten", "#ColumnEdit", QuickImage.Get(ImageCode.Spalte));

        //if (Parent is Filterleiste f) {
        //    if (f.pic.Visible) {
        //        Items.GenerateAndAdd("Filter verschieben", "#FilterVerschieben", QuickImage.Get(ImageCode.Trichter));
        //        Items.GenerateAndAdd("Bild-Pfad öffnen", "#BildPfad", QuickImage.Get(ImageCode.Ordner));
        //    }
        //}
    }

    public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

    public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) => ContextMenuItemClicked?.Invoke(this, e);

    internal ComboBox? GetComboBox() {
        foreach (var thisc in Controls) {
            if (thisc is ComboBox cbx) {
                return cbx;
            }
        }
        return null;
    }

    internal bool WasThisValueClicked() {
        var cb = GetComboBox();
        return cb != null && cb.WasThisValueClicked();
    }

    protected override void OnControlAdded(ControlEventArgs e) {
        base.OnControlAdded(e);
        e.Control.MouseUp += Control_MouseUp;
        if (e.Control is ComboBox cbx) {
            ItemCollectionList item2 = new(true)
            {
                { "Keine weiteren Einträge vorhanden", "|~" }
            };
            //var c = Filter.Column.Contents(null);
            //foreach (var thiss in c)
            //{
            //    Item2.GenerateAndAdd("|" + thiss, thiss));
            //}
            StyleComboBox(cbx, item2, ComboBoxStyle.DropDown, false);
            cbx.DropDownShowing += Cbx_DropDownShowing;
        }
        if (e.Control is Button btn) {
            btn.Translate = false;

            if (CaptionPosition == ÜberschriftAnordnung.ohne) {
                btn.ImageCode = "Trichter|16||1";
                btn.Text = Filter.ReadableText();
            } else {
                if (Filter.SearchValue.Count > 0 && !string.IsNullOrEmpty(Filter.SearchValue[0])) {
                    btn.ImageCode = "Trichter|16";
                    btn.Text = LanguageTool.DoTranslate("wählen ({0})", true, Filter.SearchValue.Count.ToString());
                } else {
                    btn.ImageCode = "Trichter|16";
                    btn.Text = LanguageTool.DoTranslate("wählen");
                }
            }
        }
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        base.OnMouseUp(e);
        if (e.Button == MouseButtons.Right) {
            FloatingInputBoxListBoxStyle.ContextMenuShow(this, e);
        }
    }

    private void Cbx_DropDownShowing(object sender, System.EventArgs e) {
        var cbx = (ComboBox)sender;
        cbx.Item.Clear();
        cbx.Item.CheckBehavior = CheckBehavior.MultiSelection;
        if (TableView == null) {
            _ = cbx.Item.Add("Anzeigefehler", "|~", ImageCode.Kreuz, false);
            return;
        }
        var listFilterString = AutoFilter.Autofilter_ItemList(Filter.Column, TableView.Filter, TableView.PinnedRows);
        if (listFilterString.Count == 0) {
            _ = cbx.Item.Add("Keine weiteren Einträge vorhanden", "|~", ImageCode.Kreuz, false);
        } else if (listFilterString.Count < 400) {
            if (Filter.Column != null) { cbx.Item.AddRange(listFilterString, Filter.Column, ShortenStyle.Replaced, Filter.Column.BehaviorOfImageAndText); }
            //cbx.Item.Sort(); // Wichtig, dieser Sort kümmert sich, dass das Format (z. B.  Zahlen) berücksichtigt wird
        } else {
            _ = cbx.Item.Add("Zu viele Einträge", "|~", ImageCode.Kreuz, false);
        }
    }

    private void Control_MouseUp(object sender, MouseEventArgs e) {
        if (e.Button == MouseButtons.Right) {
            FloatingInputBoxListBoxStyle.ContextMenuShow(this, e);
        }
    }

    private void Filter_Changed(object sender, System.EventArgs e) => UpdateFilterData();

    private void UpdateFilterData() {
        if (Filter.Column == null) {
            DisabledReason = "Bezug zum Filter verloren.";
            Caption = string.Empty;
            EditType = EditTypeFormula.None;
            QuickInfo = string.Empty;
            ValueSet(string.Empty, true, true);
        } else {
            DisabledReason = !string.IsNullOrEmpty(Filter.Herkunft) ? "Dieser Filter ist automatisch<br>gesetzt worden." : string.Empty;
            var qi = Filter.Column.QuickInfoText(string.Empty);
            if (string.IsNullOrEmpty(qi)) {
                QuickInfo = "<b>Filter:</b><br>" + Filter.ReadableText().CreateHtmlCodes(false);
            } else {
                QuickInfo = "<b>Filter:</b><br>" + Filter.ReadableText().CreateHtmlCodes(false) +
                            "<br><br><b>Info:</b><br>" + qi.CreateHtmlCodes(false);
            }

            if (!Filter.Column.AutoFilterSymbolPossible()) {
                EditType = EditTypeFormula.None;
            } else {
                var showDelFilterButton = true;
                if (Filter.FilterType == FilterType.Instr_GroßKleinEgal && Filter.SearchValue != null && Filter.SearchValue.Count == 1) {
                    CaptionPosition = ÜberschriftAnordnung.Links_neben_Dem_Feld;
                    showDelFilterButton = false;
                    Caption = Filter.Column.ReadableText() + ":";
                    EditType = EditTypeFormula.Textfeld_mit_Auswahlknopf;
                    ValueSet(Filter.SearchValue[0], true, true);
                }
                if (Filter.Column.FilterOptions is FilterOptions.Enabled_OnlyAndAllowed or FilterOptions.Enabled_OnlyOrAllowed) {
                    showDelFilterButton = false;
                    CaptionPosition = ÜberschriftAnordnung.Links_neben_Dem_Feld;
                    Caption = Filter.Column.ReadableText() + ":";
                    EditType = EditTypeFormula.Button;
                }
                if (showDelFilterButton) {
                    CaptionPosition = ÜberschriftAnordnung.ohne;
                    EditType = EditTypeFormula.Button;
                }
            }
        }
    }

    #endregion
}