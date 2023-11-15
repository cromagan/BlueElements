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

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Designer_Support;
using BlueControls.Interfaces;
using BlueDatabase;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public partial class FlexiControlForFilterNew : FlexiControl, IControlAcceptSomething, IControlSendSomething {

    #region Constructors

    public FlexiControlForFilterNew(DatabaseAbstract? database) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        Size = new Size(300, 300);
        FilterOutput.Database = database;

        UpdateFilterData();
    }

    #endregion

    #region Properties

    public List<IControlAcceptSomething> Childs { get; } = new();

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection? FilterInput { get; set; }

    public bool FilterManualSeted { get; set; } = false;
    public FilterCollection FilterOutput { get; } = new();

    public List<IControlSendSomething> Parents { get; } = new();

    #endregion

    #region Methods

    public void FilterInput_Changed(object sender, System.EventArgs e) {
        FilterInput = this.FilterOfSender();
        Invalidate();
    }

    public void FilterInput_Changing(object sender, System.EventArgs e) { }

    internal bool WasThisValueClicked() {
        var cb = GetComboBox();
        return cb != null && cb.WasThisValueClicked();
    }

    /// <summary>
    /// Verwendete Ressourcen bereinigen.
    /// </summary>
    /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
    protected override void Dispose(bool disposing) {
        if (disposing && (components != null)) {
            components?.Dispose();
            FilterInput?.Dispose();
            FilterOutput.Dispose();
        }
        base.Dispose(disposing);
    }

    protected override void OnControlAdded(ControlEventArgs e) {
        base.OnControlAdded(e);

        if (e.Control is ComboBox cbx) {
            ItemCollectionList.ItemCollectionList item2 = new(true)
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
        //if (e.Control is Button btn) {
        //    btn.Translate = false;

        //    if (CaptionPosition == ÜberschriftAnordnung.ohne) {
        //        btn.ImageCode = "Trichter|16||1";
        //        btn.Text = Filter.ReadableText();
        //    } else {
        //        if (Filter.SearchValue.Count > 0 && !string.IsNullOrEmpty(Filter.SearchValue[0])) {
        //            btn.ImageCode = "Trichter|16";
        //            btn.Text = LanguageTool.DoTranslate("wählen ({0})", true, Filter.SearchValue.Count.ToString());
        //        } else {
        //            btn.ImageCode = "Trichter|16";
        //            btn.Text = LanguageTool.DoTranslate("wählen");
        //        }
        //    }
        //}
    }

    private void Cbx_DropDownShowing(object sender, System.EventArgs e) {
        //var cbx = (ComboBox)sender;
        //cbx.Item.Clear();
        //cbx.Item.CheckBehavior = CheckBehavior.MultiSelection;
        //if (TableView == null) {
        //    _ = cbx.Item.Add("Anzeigefehler", "|~", ImageCode.Kreuz, false);
        //    return;
        //}
        //var listFilterString = AutoFilter.Autofilter_ItemList(Filter.Column, TableView.Filter, TableView.PinnedRows);
        //if (listFilterString.Count == 0) {
        //    _ = cbx.Item.Add("Keine weiteren Einträge vorhanden", "|~", ImageCode.Kreuz, false);
        //} else if (listFilterString.Count < 400) {
        //    if (Filter.Column != null) { cbx.Item.AddRange(listFilterString, Filter.Column, ShortenStyle.Replaced, Filter.Column.BehaviorOfImageAndText); }
        //    //cbx.Item.Sort(); // Wichtig, dieser Sort kümmert sich, dass das Format (z. B.  Zahlen) berücksichtigt wird
        //} else {
        //    _ = cbx.Item.Add("Zu viele Einträge", "|~", ImageCode.Kreuz, false);
        //}
    }

    private void UpdateFilterData() {
        //if (Filter.Column == null) {
        //    DisabledReason = "Bezug zum Filter verloren.";
        //    Caption = string.Empty;
        //    EditType = EditTypeFormula.None;
        //    QuickInfo = string.Empty;
        //    ValueSet(string.Empty, true, true);
        //} else {
        //    DisabledReason = !string.IsNullOrEmpty(Filter.Herkunft) ? "Dieser Filter ist automatisch<br>gesetzt worden." : string.Empty;
        //    var qi = Filter.Column.QuickInfoText(string.Empty);
        //    if (string.IsNullOrEmpty(qi)) {
        //        QuickInfo = "<b>Filter:</b><br>" + Filter.ReadableText().CreateHtmlCodes(false);
        //    } else {
        //        QuickInfo = "<b>Filter:</b><br>" + Filter.ReadableText().CreateHtmlCodes(false) +
        //                    "<br><br><b>Info:</b><br>" + qi.CreateHtmlCodes(false);
        //    }

        //    if (!Filter.Column.AutoFilterSymbolPossible()) {
        //        EditType = EditTypeFormula.None;
        //    } else {
        //        var showDelFilterButton = true;
        //        if (Filter.FilterType == FilterType.Instr_GroßKleinEgal && Filter.SearchValue != null && Filter.SearchValue.Count == 1) {
        //            CaptionPosition = ÜberschriftAnordnung.Links_neben_Dem_Feld;
        //            showDelFilterButton = false;
        //            Caption = Filter.Column.ReadableText() + ":";
        //            EditType = EditTypeFormula.Textfeld_mit_Auswahlknopf;
        //            ValueSet(Filter.SearchValue[0], true, true);
        //        }
        //        if (Filter.Column.FilterOptions is FilterOptions.Enabled_OnlyAndAllowed or FilterOptions.Enabled_OnlyOrAllowed) {
        //            showDelFilterButton = false;
        //            CaptionPosition = ÜberschriftAnordnung.Links_neben_Dem_Feld;
        //            Caption = Filter.Column.ReadableText() + ":";
        //            EditType = EditTypeFormula.Button;
        //        }
        //        if (showDelFilterButton) {
        //            CaptionPosition = ÜberschriftAnordnung.ohne;
        //            EditType = EditTypeFormula.Button;
        //        }
        //    }
        //}
    }

    #endregion
}