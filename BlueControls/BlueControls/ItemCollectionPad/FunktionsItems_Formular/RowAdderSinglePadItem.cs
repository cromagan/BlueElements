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
public class RowAdderSingle : FakeControlPadItem, IReadableText, IItemToControl, IItemAcceptFilter, IHasVersion, IAutosizable {

    #region Fields

    private readonly ItemAcceptFilter _itemAccepts;

    /// <summary>
    /// Überschrift des Steuerelementes
    /// </summary>
    private string _caption = string.Empty;

    /// <summary>
    /// Zusätzlicher Text der in eine Spalte geschrieben wird. Wird mit Variablen erzeugt
    /// </summary>
    private string _individualText = string.Empty;

    /// <summary>
    /// Zusätzliche Erkennung für individuelle Items. Wird mit Variablen erzeugt
    /// </summary>
    private string _inputDatabase_AdditionalID = string.Empty;

    /// <summary>
    /// Id des klickbaren elements (InputDatabase)
    /// </summary>
    private string _inputDatabase_IDColumn = string.Empty;

    /// <summary>
    /// Diese Datenbank wird bei einen Klick beschrieben
    /// </summary>
    private Database? _targetDatabase = null;

    /// <summary>
    /// wo die zusätzliche ID hingeschrieben wird
    /// </summary>
    private string _targetDatabase_AdditionalIDColumn = string.Empty;

    /// <summary>
    /// Id des klickbaren elements, wo es hingeschrieben wird
    /// </summary>
    private string _targetDatabase_IDColumn = string.Empty;

    /// <summary>
    /// wo der zusätzliche Text hingeschrieben wird
    /// </summary>
    private string _targetDatabase_individualTextColumn = string.Empty;

    /// <summary>
    /// Wo die Vorfilterung in die Zieldatenbank geschrieben werden soll
    /// </summary>
    private string _targetDatabase_PreFilterColumn = string.Empty;

    #endregion

    #region Constructors

    public RowAdderSingle(string keyName) : this(keyName, null as ConnectedFormula.ConnectedFormula) { }

    public RowAdderSingle(string keyName, ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula) => _itemAccepts = new();

    #endregion

    #region Properties

    public static string ClassId => "FI-RowAdderSingle";

    public AllowedInputFilter AllowedInputFilter => AllowedInputFilter.One;

    public bool AutoSizeableHeight => true;

    public string Caption {
        get => _caption;
        set {
            if (IsDisposed) { return; }
            if (_caption == value) { return; }
            _caption = value;
            OnPropertyChanged();
        }
    }

    public Database? DatabaseInput => _itemAccepts.DatabaseInput(this);

    public bool DatabaseInputMustMatchOutputDatabase => false;

    public override string Description => "Ein Steuerelement, das eine andere Tabelle befüllen kann. Es werden alle Zellen des eingehenden Filters als drückbare Knöpfe angezeigt";

    public List<int> InputColorId => _itemAccepts.InputColorIdGet(this);

    public override bool MustBeInDrawingArea => true;

    public bool MustBeOneRow => false;

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ReadOnlyCollection<string> Parents {
        get => _itemAccepts.GetFilterFromKeysGet();
        set => _itemAccepts.GetFilterFromKeysSet(value, this);
    }

    [Description("Die Ziel-Datenbank. In diese Datenbank wird das Ergebniss des Benutzers gespeichert")]
    public Database? Target_Database {
        get => _targetDatabase; set {
            if (IsDisposed) { return; }
            if (_targetDatabase == value) { return; }
            _targetDatabase = value;
            OnPropertyChanged();

            UpdateSideOptionMenu();
        }
    }

    public ColumnItem? TargetDatabase_PreFilterColumn {
        get {
            var c = Target_Database?.Column[_targetDatabase_PreFilterColumn];
            return c == null || c.IsDisposed ? null : c;
        }
    }

    [Description("Eine Spalte in der Ziel-Datenbank. In diese wird die eingehende Vorfilterung gespeichert. Dadurch können verschiedene Datensätze gespeichert werden. ")]
    public string TargetDatabase_PreFilterColumnName {
        get => _targetDatabase_PreFilterColumn;
        set {
            if (IsDisposed) { return; }
            if (_targetDatabase_PreFilterColumn == value) { return; }
            _targetDatabase_PreFilterColumn = value;
            OnPropertyChanged();
            //UpdateSideOptionMenu();
        }
    }

    #endregion

    #region Methods

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

        //b = _itemSends.ErrorReason(this);
        //if (!string.IsNullOrEmpty(b)) { return b; }

        if (_targetDatabase == null || _targetDatabase.IsDisposed) {
            return "Ziel-Datenbank fehlt";
        }

        return string.Empty;
    }

    public override List<GenericControl> GetStyleOptions(int widthOfControl) {
        List<GenericControl> l = [.. _itemAccepts.GetStyleOptions(this, widthOfControl)];

        if (DatabaseInput is not Database db || db.IsDisposed) { return l; }

        l.Add(new FlexiControlForProperty<Database?>(() => Target_Database, ItemSendSomething.AllAvailableTables()));

        if (Target_Database == null) { return l; }

        var lst = new List<AbstractListItem>();
        lst.AddRange(ItemsOf(db.Column, false));

        l.Add(new FlexiControlForProperty<string>(() => TargetDatabase_PreFilterColumnName, lst));

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

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }
        if (_itemAccepts.ParseThis(tag, value)) { return true; }

        switch (tag) {
            //case "column":
            //    //Column = GetRowFrom?.Database?.Column.SearchByKey(LongParse(value));
            //    return true;

            //case "columnname":
            //    _columnName = value;
            //    return true;

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
        const string txt = "Wert aus: ";

        //if (this.IsOk() && Column != null) {
        //    return txt + Column.Caption;
        //}

        return txt + ErrorReason();
    }

    public override QuickImage? SymbolForReadableText() {
        //if (this.IsOk() && Column != null) {
        //    return Column.SymbolForReadableText();

        //    //return QuickImage.Get(ImageCode.Zeile, 16, Color.Transparent, Skin.IdColor(InputColorId));
        //}

        return QuickImage.Get(ImageCode.Warnung, 16);
    }

    public override string ToString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = [.. _itemAccepts.ParsableTags()];

        //result.ParseableAdd("ColumnName", _columnName);
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