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
using BlueControls.Interfaces;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueDatabase;

using static BlueDatabase.Database;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using static BlueBasics.Converter;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

/// <summary>
/// Erzeugt eine liste mit Zeile, die eine andere Tabelle befüllen können
/// </summary>
public class RowAdder : FakeControlPadItem, IReadableText, IItemToControl, IItemAcceptFilter, IItemSendFilter, IHasVersion, IAutosizable {

    #region Fields

    private readonly ItemAcceptFilter _itemAccepts;
    private readonly ItemSendFilter _itemSends;

    private List<RowAdderSingle> _addersingle = new();

    /// <summary>
    /// Wo der Zusätzliche Text in die Zieldatenbank geschrieben werden soll
    /// </summary>
    private string _additinalTextColumnName = string.Empty;

    /// <summary>
    /// Wo die normale ID in die Zieldatenbank geschrieben werden soll.
    /// </summary>
    private string _iDColumnName = string.Empty;

    /// <summary>
    /// Zusätzliche Erkennung für individuelle Items. Wird mit Variablen erzeugt
    /// </summary>
    private string _inputDatabase_UniqueSaveID = string.Empty;

    /// <summary>
    /// Wo Zeilen-Id in die Zieldatenbank geschrieben werden soll
    /// </summary>
    private string _uniqueRowIDColumnName = string.Empty;

    /// <summary>
    /// Wo die Vorfilterungs-ID in die Zieldatenbank geschrieben werden soll
    /// </summary>
    private string _uniqueSaveIDColumnName = string.Empty;

    #endregion

    #region Constructors

    public RowAdder(string keyName) : this(keyName, null, null) { }

    public RowAdder(string keyName, ConnectedFormula.ConnectedFormula? cformula) : this(keyName, null, cformula) { }

    public RowAdder(string keyName, Database? db, ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula) {
        _itemAccepts = new();
        _itemSends = new();

        DatabaseOutput = db;
    }

    #endregion

    #region Properties

    public static string ClassId => "FI-RowAdder";

    public ColumnItem? AdditinalTextColumn {
        get {
            if (_itemSends.DatabaseOutputGet() is not Database dbout || dbout.IsDisposed) { return null; }

            var c = dbout?.Column[_additinalTextColumnName];
            return c == null || c.IsDisposed ? null : c;
        }
    }

    [Description("Eine Spalte in der Ziel-Datenbank.\r\nIn diese wird die generierte ID der Eingehenden Datenbank gespeichert.\r\nDadurch können verschiedene Datensätze gespeichert werden.")]
    public string AdditinalTextColumnName {
        get => _additinalTextColumnName;
        set {
            if (IsDisposed) { return; }
            if (_additinalTextColumnName == value) { return; }
            _additinalTextColumnName = value;
            OnPropertyChanged();
            //UpdateSideOptionMenu();
        }
    }

    public AllowedInputFilter AllowedInputFilter => AllowedInputFilter.One;

    public bool AutoSizeableHeight => true;

    public ReadOnlyCollection<string> ChildIds {
        get => _itemSends.ChildIdsGet();
        set => _itemSends.ChildIdsSet(value, this);
    }

    public Database? DatabaseInput => _itemAccepts.DatabaseInput(this);

    public bool DatabaseInputMustMatchOutputDatabase => false;

    public Database? DatabaseOutput {
        get => _itemSends.DatabaseOutputGet();
        set => _itemSends.DatabaseOutputSet(value, this);
    }

    public override string Description => "Ein Steuerelement, das eine andere Tabelle befüllen kann.\r\n" +
                                          "Aus der eingehenden Zeile wird eine ID generiert, diese wird zum dauerhaften Speichern in der Ausgangsdatenbank benutzt.\r\n" +
                                            "Diese ID wird auch aus Ausgangefilter weitergegeben.";

    public ColumnItem? IDColumn {
        get {
            if (_itemSends.DatabaseOutputGet() is not Database dbout || dbout.IsDisposed) { return null; }

            var c = dbout?.Column[_iDColumnName];
            return c == null || c.IsDisposed ? null : c;
        }
    }

    [Description("Eine Spalte in der Ziel-Datenbank.\r\nIn diese wird die ID der Zeile gespeichert.")]
    public string IDColumnName {
        get => _iDColumnName;
        set {
            if (IsDisposed) { return; }
            if (_iDColumnName == value) { return; }
            _iDColumnName = value;
            OnPropertyChanged();
            //UpdateSideOptionMenu();
        }
    }

    public List<int> InputColorId => _itemAccepts.InputColorIdGet(this);

    public override bool MustBeInDrawingArea => true;

    public bool MustBeOneRow => true;

    public int OutputColorId {
        get => _itemSends.OutputColorIdGet();
        set => _itemSends.OutputColorIdSet(value, this);
    }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ReadOnlyCollection<string> Parents {
        get => _itemAccepts.GetFilterFromKeysGet();
        set => _itemAccepts.GetFilterFromKeysSet(value, this);
    }

    public ColumnItem? UniqueRowIDColumn {
        get {
            if (_itemSends.DatabaseOutputGet() is not Database dbout || dbout.IsDisposed) { return null; }

            var c = dbout?.Column[_uniqueRowIDColumnName];
            return c == null || c.IsDisposed ? null : c;
        }
    }

    [Description("Eine Spalte in der Ziel-Datenbank.\r\nIn diese wird die generierte ID des Klickbaren Elements gespeichert.")]
    public string UniqueRowIDColumnName {
        get => _uniqueRowIDColumnName;
        set {
            if (IsDisposed) { return; }
            if (_uniqueRowIDColumnName == value) { return; }
            _uniqueRowIDColumnName = value;
            OnPropertyChanged();
            //UpdateSideOptionMenu();
        }
    }

    [Description("Eine eindeutige ID, die aus der eingehenen Zeile mit Variablen generiert wird.\r\nDadurch können verschiedene Datensätze gespeichert werden.")]
    public string UniqueSaveID {
        get => _inputDatabase_UniqueSaveID;
        set {
            if (IsDisposed) { return; }
            if (_inputDatabase_UniqueSaveID == value) { return; }
            _inputDatabase_UniqueSaveID = value;
            OnPropertyChanged();
        }
    }

    public ColumnItem? UniqueSaveIDColumn {
        get {
            if (_itemSends.DatabaseOutputGet() is not Database dbout || dbout.IsDisposed) { return null; }

            var c = dbout?.Column[_uniqueSaveIDColumnName];
            return c == null || c.IsDisposed ? null : c;
        }
    }

    [Description("Eine Spalte in der Ziel-Datenbank.\r\nIn diese wird die generierte ID der Eingehenden Datenbank gespeichert.\r\nDadurch können verschiedene Datensätze gespeichert werden.")]
    public string UniqueSaveIDColumnName {
        get => _uniqueSaveIDColumnName;
        set {
            if (IsDisposed) { return; }
            if (_uniqueSaveIDColumnName == value) { return; }
            _uniqueSaveIDColumnName = value;
            OnPropertyChanged();
            //UpdateSideOptionMenu();
        }
    }

    #endregion

    #region Methods

    public void AddChild(IHasKeyName add) => _itemSends.AddChild(this, add);

    public override void AddedToCollection() {
        base.AddedToCollection();
        //_itemSends.DoCreativePadAddedToCollection(this);
        _itemAccepts.DoCreativePadAddedToCollection(this);
        //RepairConnections();
    }

    public void CalculateInputColorIds() => _itemAccepts.CalculateInputColorIds(this);

    public override System.Windows.Forms.Control CreateControl(ConnectedFormulaView parent) {
        //var ff = parent.SearchOrGenerate(rfw2);

        var con = new FlexiControlForCell {
            //ColumnName = _columnName
            //EditType = EditType,
            //CaptionPosition = CaptionPosition
        };

        con.DoInputSettings(parent, this);
        //con.DoOutputSettings(this);
        return con;
    }

    public override string ErrorReason() {
        var b = base.ErrorReason();
        if (!string.IsNullOrEmpty(b)) { return b; }

        b = _itemAccepts.ErrorReason(this);
        if (!string.IsNullOrEmpty(b)) { return b; }

        b = _itemSends.ErrorReason(this);
        if (!string.IsNullOrEmpty(b)) { return b; }

        if (string.IsNullOrEmpty(_inputDatabase_UniqueSaveID)) { return "Id-Generierung fehlt"; }
        if (!_inputDatabase_UniqueSaveID.Contains("~")) { return "ID-Generierung muss mit Variablen definiert werden."; }

        if (UniqueSaveIDColumn == null || UniqueSaveIDColumn.IsDisposed) {
            return "Spalte, in der die SaveID geschrieben werden soll, fehlt";
        }

        if (UniqueRowIDColumn == null || UniqueRowIDColumn.IsDisposed) {
            return "Spalte, in der die einzigartige Zeilen-ID geschrieben werden soll, fehlt";
        }

        if (IDColumn == null || IDColumn.IsDisposed) {
            return "Spalte, in der die Zeilen-ID geschrieben werden soll, fehlt";
        }

        if (AdditinalTextColumn == null || AdditinalTextColumn.IsDisposed) {
            return "Spalte, in der der Zusätzliche Text geschrieben werden soll, fehlt";
        }

        foreach (var thisAdder in _addersingle) {
            b = thisAdder.ErrorReason();
            if (!string.IsNullOrEmpty(b)) { return "Ein Eintrag der Ergänzer ist falsch:" + b; }
        }

        return string.Empty;
    }

    public override List<GenericControl> GetStyleOptions(int widthOfControl) {
        var l = new List<GenericControl>();

        l.AddRange(_itemAccepts.GetStyleOptions(this, widthOfControl));

        var inr = _itemAccepts.GetFilterFromGet(this);
        if (inr.Count > 0 && inr[0].DatabaseOutput is Database dbin && !dbin.IsDisposed) {
            l.Add(new FlexiControlForProperty<string>(() => UniqueSaveID));
        }

        l.AddRange(_itemSends.GetStyleOptions(this, widthOfControl));

        if (_itemSends.DatabaseOutputGet() is Database dbout && !dbout.IsDisposed) {
            var lst = new List<AbstractListItem>();
            lst.AddRange(ItemsOf(dbout.Column, true));

            l.Add(new FlexiControlForProperty<string>(() => UniqueSaveIDColumnName, lst));

            l.Add(new FlexiControlForProperty<string>(() => IDColumnName, lst));

            l.Add(new FlexiControlForProperty<string>(() => AdditinalTextColumnName, lst));
        }

        //l.Add(new FlexiControlForProperty<Database?>(() => Target_Database, ItemSendFilter.AllAvailableTables()));

        //if (Target_Database == null) { return l; }

        //if (Column == null || Column.IsDisposed) { return l; }

        //var u = new List<AbstractListItem>();
        //u.AddRange(ItemsOf(typeof(CaptionPosition)));
        //l.Add(new FlexiControlForProperty<string>(() => Caption));

        //l.Add(new FlexiControl());
        //l.AddRange(base.GetStyleOptions(widthOfControl));
        return l;
    }

    public override void ParseFinished(string parsed) {
        base.ParseFinished(parsed);
        //_itemSends.ParseFinished(this);
        _itemAccepts.ParseFinished(this);
    }

    public override bool ParseThis(string key, string value) {
        if (base.ParseThis(key, value)) { return true; }
        if (_itemAccepts.ParseThis(key, value)) { return true; }

        switch (key) {
            case "uniquesaveid":
                _inputDatabase_UniqueSaveID = value.FromNonCritical();
                return true;

            case "uniquesaveidcolumnname":
                _uniqueSaveIDColumnName = value;
                return true;

            case "idcolumnname":
                _iDColumnName = value;
                return true;

            case "uniquerowidcolumnname":
                _uniqueRowIDColumnName = value;
                return true;

            case "additinalTextColumnName":
                _additinalTextColumnName = value;
                return true;

                //case "edittype":
                //    _bearbeitung = (EditTypeFormula)IntParse(value);
                //    return true;

                //case "caption":
                //    _überschriftanordung = (CaptionPosition)IntParse(value);
                //    return true;

                //case "autodistance":
                //    _autoX = value.FromPlusMinus();
                //    return true;
        }
        return false;
    }

    public override string ReadableText() {
        const string txt = "Zeilengenerator ";

        //if (this.IsOk() && Column != null) {
        //    return txt + Column.Caption;
        //}

        return txt + ErrorReason();
    }

    public void RemoveChild(IItemAcceptFilter remove) => _itemSends.RemoveChild(remove, this);

    public override QuickImage? SymbolForReadableText() {
        //if (this.IsOk() && Column != null) {
        //    return Column.SymbolForReadableText();

        //    //return QuickImage.Get(ImageCode.Zeile, 16, Color.Transparent, Skin.IdColor(InputColorId));
        //}

        return QuickImage.Get(ImageCode.Warnung, 16);
    }

    public override string ToString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = [.. _itemAccepts.ParsableTags(), .. _itemSends.ParsableTags()];

        //result.ParseableAdd("TargetDatabase", _targetDatabase); // Nicht _database, weil sie evtl. noch nicht geladen ist

        result.ParseableAdd("UniqueSaveID", _inputDatabase_UniqueSaveID);
        result.ParseableAdd("UniqueSaveIDColumnName", _uniqueSaveIDColumnName);
        result.ParseableAdd("UniqueRowIDColumnName", _uniqueRowIDColumnName);
        result.ParseableAdd("IDColumnName", _iDColumnName);
        result.ParseableAdd("AdditinalTextColumnName", _additinalTextColumnName);

        //result.ParseableAdd("EditType", _bearbeitung);
        //result.ParseableAdd("Caption", _überschriftanordung);
        //result.ParseableAdd("AutoDistance", _autoX);
        return result.Parseable(base.ToString());
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        if (!forPrinting) {
            DrawColorScheme(gr, positionModified, zoom, InputColorId, true, true, false);
        }

        //if (Column  ==null || Column .IsDisposed) {
        //    Skin.Draw_FormatedText(gr, "Spalte fehlt", QuickImage.Get(ImageCode.Warnung, (int)(16 * zoom)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), CaptionFnt.Scale(zoom), true);
        //} else {
        //DrawFakeControl(gr, positionModified, zoom, CaptionPosition, Column?.ReadableText() + ":", EditType);
        //}

        if (!forPrinting) {
            DrawColorScheme(gr, positionModified, zoom, InputColorId, true, true, true);
        }

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);

        DrawArrorInput(gr, positionModified, zoom, shiftX, shiftY, forPrinting, InputColorId);
    }

    #endregion
}