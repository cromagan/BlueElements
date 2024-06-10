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
using BlueControls.Interfaces;
using BlueDatabase;
using System.Collections.Generic;
using System.ComponentModel;
using System;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using BlueControls.Enums;
using System.Drawing;
using System.Windows.Forms;

using BlueBasics;
using BlueControls.Interfaces;
using BlueDatabase;
using System.Collections.Generic;
using System.ComponentModel;
using System;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using BlueControls.Enums;
using System.Drawing;
using System.Windows.Forms;

using BlueDatabase.Interfaces;
using BlueBasics.Interfaces;

using BlueBasics;

using BlueBasics.Enums;

using BlueBasics.Interfaces;

using BlueDatabase.Enums;
using BlueDatabase.EventArgs;

using BlueDatabase.Interfaces;
using System;
using System.Collections.Generic;

using System.Collections.ObjectModel;

using System.Drawing;

using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static BlueBasics.Constants;
using static BlueBasics.Converter;
using static BlueBasics.IO;
using static BlueDatabase.Database;
using BlueControls.EventArgs;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using BlueControls.ItemCollectionList;
using BlueScript;
using BlueScript.Methods;

namespace BlueControls.Controls;

public partial class RowAdder : System.Windows.Forms.UserControl, IControlAcceptFilter, IControlSendFilter, IControlUsesRow {

    #region Fields

    public List<RowAdderSingle> AdderSingle = new();
    private FilterCollection? _filterInput;

    private List<string> _selected = new();

    #endregion

    #region Constructors

    public RowAdder() {
        InitializeComponent();
    }

    #endregion

    #region Events

    public event EventHandler? DisposingEvent;

    #endregion

    #region Properties

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ColumnItem? AdditinalTextColumn { get; internal set; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<IControlAcceptFilter> Childs { get; } = [];

    /// <summary>
    /// Eine eindeutige ID, die aus der eingehenen Zeile mit Variablen generiert wird.
    /// Dadurch können verschiedene Datensätze gespeichert werden.
    /// Beispiele: Rezepetname, Personenname, Beleg-Nummer
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string EntityID { get; internal set; }

    /// <summary>
    /// Eine eindeutige ID, die aus der eingehenen Zeile mit Variablen generiert wird.
    /// Dadurch können verschiedene Datensätze gespeichert werden.
    /// Beispiele: Rezepetname, Personenname, Beleg-Nummer
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ColumnItem? EntityIDColumn { get; internal set; }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection? FilterInput {
        get => _filterInput;
        set {
            if (_filterInput == value) { return; }
            this.UnRegisterEventsAndDispose();
            _filterInput = value;
            ((IControlAcceptFilter)this).RegisterEvents();
        }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool FilterInputChangedHandled { get; set; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection FilterOutput { get; } = new("FilterOutput 08");

    /// <summary>
    /// Eine Spalte in der Ziel-Datenbank.
    /// In diese wird die generierte ID des klickbaren Elements gespeichert.
    /// Diese wird automatisch generiert - es muss nur eine Spalte zur Verfügung gestellt werden.
    /// Beispiel: Zutaten#Vegetarisch/Mehl#100 g
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ColumnItem? OriginIDColumn { get; internal set; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<IControlSendFilter> Parents { get; } = [];

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<RowItem>? RowsInput { get; set; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool RowsInputChangedHandled { get; set; }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool RowsInputManualSeted { get; set; } = false;

    /// <summary>
    /// Die Herkunft-Id, die mit Variablen der erzeugt wird.
    /// Diese Id muss für jede Zeile der eingehenden Datenbank einmalig sein.
    /// Die Struktur muss wie ein Dateipfad aufgebaut sein. z.B. Kochen\\Zutaten\\Vegetarisch\\Mehl
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ColumnItem? TextKey { get; internal set; }

    /// <summary>
    /// Die Herkunft-Id, die mit Variablen der erzeugt wird.
    /// Diese Id muss für jede Zeile der eingehenden Datenbank einmalig sein.
    /// Die Struktur muss wie ein Dateipfad aufgebaut sein. z.B. Kochen\\Zutaten\\Vegetarisch\\Mehl
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ColumnItem? TextKeyColumn { get; internal set; }

    #endregion

    #region Methods

    /// <summary>
    ///
    /// </summary>
    /// <param name="textkey"></param>
    /// <returns>PFAD1/PFAD2/PFAD3/</returns>
    public static string RepairTextKey(string textkey) => textkey.Replace("\\", "/").Trim("/").ToUpper() + "/";

    public void FilterInput_DispodingEvent(object sender, System.EventArgs e) => this.FilterInput_DispodingEvent();

    public void FilterInput_RowsChanged(object sender, System.EventArgs e) => this.FilterInput_RowsChanged();

    public void FilterOutput_DispodingEvent(object sender, System.EventArgs e) => this.FilterOutput_DispodingEvent();

    public void FilterOutput_PropertyChanged(object sender, System.EventArgs e) => this.FilterOutput_PropertyChanged();

    public void HandleChangesNow() {
        if (IsDisposed) { return; }
        if (RowsInputChangedHandled && FilterInputChangedHandled) { return; }

        if (string.IsNullOrEmpty(EntityID) || EntityIDColumn == null) { return; }

        if (TextKeyColumn == null) { return; }

        if (!FilterInputChangedHandled) {
            FilterInputChangedHandled = true;
            this.DoInputFilter(FilterOutput.Database, true);
        }

        RowsInputChangedHandled = true;

        this.DoRows();

        var rowIn = this.RowSingleOrNull();

        if (rowIn == null) {
            this.Invalidate_FilterOutput();
            return;
        }

        var generatedentityID = rowIn.ReplaceVariables(EntityID, false, true, null);

        if (generatedentityID == EntityID) { return; }

        FilterOutput.ChangeTo(new FilterItem(EntityIDColumn, BlueDatabase.Enums.FilterType.Istgleich_GroßKleinEgal, generatedentityID));

        foreach (var thisAdder in AdderSingle) {
            if (thisAdder.Database is not Database db || db.IsDisposed) { continue; }

            foreach (var thisRow in db.Row) {
                if (thisRow == null || thisRow.IsDisposed) { continue; }

                var generatedTextKey = thisRow.ReplaceVariables(thisAdder.TextKey, false, true, null);

                if (generatedTextKey == thisAdder.TextKey) { continue; }

                if (!ShowMe(generatedTextKey)) { continue; }

                var additionaltext = thisRow.ReplaceVariables(thisAdder.AdditionalText, false, true, null);

                var it = new AdderItem(db, EntityIDColumn, generatedentityID, OriginIDColumn, TextKeyColumn, generatedTextKey, AdditinalTextColumn, additionaltext);

                lstTexte.ItemAdd(ItemOf(it));
            }
        }

        //#region Combobox suchen

        //ComboBox? cb = null;
        //foreach (var thiscb in Controls) {
        //    if (thiscb is ComboBox cbx) { cb = cbx; break; }
        //}

        //#endregion

        //if (cb == null) { return; }

        //var ex = cb.Items().ToList();

        //#region Zeilen erzeugen

        //if (RowsInput == null || !RowsInputChangedHandled) { return; }

        //foreach (var thisR in RowsInput) {
        //    if (cb[thisR.KeyName] == null) {
        //        var tmpQuickInfo = thisR.ReplaceVariables(_showformat, true, true, null);
        //        cb.ItemAdd(ItemOf(tmpQuickInfo, thisR.KeyName));
        //    } else {
        //        ex.Remove(thisR.KeyName);
        //    }
        //}

        //#endregion

        //#region Veraltete Zeilen entfernen

        //foreach (var thisit in ex) {
        //    cb?.Remove(thisit);
        //}

        //#endregion

        //#region Nur eine Zeile? auswählen!

        //// nicht vorher auf null setzen, um Blinki zu vermeiden
        //if (cb.ItemCount == 1) {
        //    ValueSet(cb[0].KeyName, true);
        //}

        //if (cb.ItemCount < 2) {
        //    DisabledReason = "Keine Auswahl möglich.";
        //} else {
        //    DisabledReason = string.Empty;
        //}

        //#endregion

        //#region  Prüfen ob die aktuelle Auswahl passt

        //// am Ende auf null setzen, um Blinki zu vermeiden

        //if (cb[Value] == null) {
        //    ValueSet(string.Empty, true);
        //}

        //#endregion
    }

    public void OnDisposingEvent() => DisposingEvent?.Invoke(this, System.EventArgs.Empty);

    public void ParentFilterOutput_Changed() { }

    public void RowsInput_Changed() { }

    protected override void OnPaint(PaintEventArgs e) {
        HandleChangesNow();
        base.OnPaint(e);
    }

    private bool Selected(string textkey) => _selected.Contains(RepairTextKey(textkey));

    private bool ShowMe(string textkey) {
        var t = RepairTextKey(textkey);
        if (t.CountString("/") == 1) { return true; }
        if (Selected(t)) { return true; }
        return Selected(t.PathParent());
    }

    #endregion
}