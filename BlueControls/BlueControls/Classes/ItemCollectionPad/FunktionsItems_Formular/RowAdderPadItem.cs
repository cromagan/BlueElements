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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using BlueDatabase.Enums;
using BlueControls.BlueDatabaseDialogs;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

/// <summary>
/// Erzeugt eine liste mit Zeile, die eine andere Tabelle befüllen können
/// </summary>
public class RowAdderPadItem : FakeControlPadItem, IItemToControl, IReadableText, IItemAcceptFilter, IItemSendFilter, IAutosizable, ISimpleEditor {

    #region Fields

    private readonly ItemAcceptFilter _itemAccepts;
    private readonly ItemSendFilter _itemSends;
    private string _additinalInfoColumnName = string.Empty;

    /// <summary>
    /// Eine eindeutige ID, die aus der eingehenen Zeile mit Variablen generiert wird.
    /// Dadurch können verschiedene Datensätze gespeichert werden.
    /// Beispiele: Rezepetname, Personenname, Beleg-Nummer
    /// </summary>
    private string _entityID = string.Empty;

    /// <summary>
    /// Eine Spalte in der Ziel-Datenbank.
    /// In diese wird die generierte ID des klickbaren Elements gespeichert.
    /// Diese wird automatisch generiert - es muss nur eine Spalte zur Verfügung gestellt werden.
    /// Beispiel: Zutaten#Vegetarisch/Mehl#3FFDKKJ34fJ4#1
    /// </summary>
    private string _originIDColumnName = string.Empty;

    private string _script = string.Empty;

    #endregion

    #region Constructors

    public RowAdderPadItem(string keyName) : this(keyName, null, null) { }

    public RowAdderPadItem(string keyName, ConnectedFormula.ConnectedFormula? cformula) : this(keyName, null, cformula) { }

    public RowAdderPadItem(string keyName, Database? db, ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula) {
        _itemAccepts = new();
        _itemSends = new();

        DatabaseOutput = db;
    }

    #endregion

    #region Properties

    public static string ClassId => "FI-RowAdder";

    public ColumnItem? AdditionalInfoColumn {
        get {
            if (_itemSends.DatabaseOutputGet(this) is not Database dbout || dbout.IsDisposed) { return null; }

            var c = dbout?.Column[_additinalInfoColumnName];
            return c == null || c.IsDisposed ? null : c;
        }
    }

    [Description("Eine Spalte in der Ziel-Datenbank.\r\nIn diese wird eine Zusatzinfo gespeichert.\r\nDiese wird automatisch generiert - es muss nur eine Spalte zur Verfügung gestellt werden.")]
    public string AdditionalInfoColumnName {
        get => _additinalInfoColumnName;
        set {
            if (IsDisposed) { return; }
            if (_additinalInfoColumnName == value) { return; }
            _additinalInfoColumnName = value;
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

    public Database? DatabaseInput => _itemAccepts.DatabaseInputGet(this);
    public bool DatabaseInputMustMatchOutputDatabase => false;

    public Database? DatabaseOutput {
        get => _itemSends.DatabaseOutputGet(this);
        set => _itemSends.DatabaseOutputSet(value, this);
    }

    public override string Description => "Ein Steuerelement, das eine andere Tabelle befüllen kann.\r\n" +
                                          "Aus der eingehenden Zeile wird eine ID generiert, diese wird zum dauerhaften Speichern in der Ausgangsdatenbank benutzt.\r\n" +
                                            "Diese ID wird auch aus Ausgangsfilter weitergegeben.";

    /// <summary>
    /// Eine eindeutige ID, die aus der eingehenen Zeile mit Variablen generiert wird.
    /// Dadurch können verschiedene Datensätze gespeichert werden.
    /// Beispiele: Rezepetname, Personenname, Beleg-Nummer
    /// </summary>
    [Description("Eine eindeutige ID, die aus der eingehenen Zeile mit Variablen generiert wird.\r\nDadurch können verschiedene Datensätze gespeichert werden.\r\nBeispiele: Rezepetname, Personenname, Beleg-Nummer")]
    public string EntityID {
        get => _entityID;
        set {
            if (IsDisposed) { return; }
            if (_entityID == value) { return; }
            _entityID = value;
            OnPropertyChanged();
        }
    }

    public List<int> InputColorId => _itemAccepts.InputColorIdGet(this);
    public bool InputMustBeOneRow => true;
    public override bool MustBeInDrawingArea => true;
    public override string MyClassId => ClassId;

    /// <summary>
    /// Eine Spalte in der Ziel-Datenbank.
    /// In diese wird die generierte ID des klickbaren Elements gespeichert.
    /// Diese wird automatisch generiert - es muss nur eine Spalte zur Verfügung gestellt werden.
    /// Beispiel: Zutaten#Vegetarisch/Mehl#3FFDKKJ34fJ4#1
    /// </summary>
    public ColumnItem? OriginIDColumn {
        get {
            if (_itemSends.DatabaseOutputGet(this) is not Database dbout || dbout.IsDisposed) { return null; }

            var c = dbout?.Column[_originIDColumnName];
            return c == null || c.IsDisposed ? null : c;
        }
    }

    /// <summary>
    /// Eine Spalte in der Ziel-Datenbank.
    /// In diese wird die generierte ID des klickbaren Elements gespeichert.
    /// Diese wird automatisch generiert - es muss nur eine Spalte zur Verfügung gestellt werden.
    /// Beispiel: Zutaten#Vegetarisch/Mehl#3FFDKKJ34fJ4#1
    /// </summary>
    [Description("Eine Spalte in der Ziel-Datenbank.\r\nIn diese wird die generierte ID des klickbaren Elements gespeichert.\r\nDiese wird automatisch generiert - es muss nur eine Spalte zur Verfügung gestellt werden.")]
    public string OriginIDColumnName {
        get => _originIDColumnName;
        set {
            if (IsDisposed) { return; }
            if (_originIDColumnName == value) { return; }
            _originIDColumnName = value;
            OnPropertyChanged();
            //UpdateSideOptionMenu();
        }
    }

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

    public string Script {
        get { return _script; }

        set {
            if (value == _script) { return; }
            _script = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    public void AddChild(IHasKeyName add) => _itemSends.AddChild(this, add);

    public override void AddedToCollection() {
        base.AddedToCollection();
        _itemSends.DoCreativePadAddedToCollection(this);
        _itemAccepts.DoCreativePadAddedToCollection(this);
        //RepairConnections();
    }

    public void CalculateInputColorIds() => _itemAccepts.CalculateInputColorIds(this);

    public System.Windows.Forms.Control CreateControl(ConnectedFormulaView parent, string mode) {
        var con = new RowAdder {
            EntityID = EntityID,
            OriginIDColumn = OriginIDColumn,
            AdditionalInfoColumn = AdditionalInfoColumn,
            Script = Script
        };

        con.DoDefaultSettings(parent, this, mode);

        return con;
    }

    public override string ErrorReason() {
        var b = base.ErrorReason();
        if (!string.IsNullOrEmpty(b)) { return b; }

        b = _itemAccepts.ErrorReason(this);
        if (!string.IsNullOrEmpty(b)) { return b; }

        b = _itemSends.ErrorReason(this);
        if (!string.IsNullOrEmpty(b)) { return b; }

        if (string.IsNullOrEmpty(_entityID)) { return "Id-Generierung fehlt"; }
        if (!_entityID.Contains("~")) { return "ID-Generierung muss mit Variablen definiert werden."; }

        if (OriginIDColumn is not ColumnItem oic || oic.IsDisposed) {
            return "Spalte, in der die Herkunft-ID geschrieben werden soll, fehlt";
        }

        if (oic.Function != ColumnFunction.Schlüsselspalte) {
            return "Die Herkunft-ID-Spalte muss eine Schlüsselspalte sein.";
        }

        if (AdditionalInfoColumn is ColumnItem aic && !aic.IsDisposed) {
            if (aic.Function != ColumnFunction.Schlüsselspalte) {
                return "Die Zusatzinfo-Spalte muss eine Schlüsselspalte sein.";
            }
        }

        if (string.IsNullOrEmpty(Script)) {
            return "Kein Skript definiert.";
        }

        return string.Empty;
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        var l = new List<GenericControl>();

        l.AddRange(_itemAccepts.GetProperties(this, widthOfControl));

        l.AddRange(_itemSends.GetProperties(this, widthOfControl));

        l.Add(new FlexiControl("Eigenschaften:", widthOfControl, true));
        var inr = _itemAccepts.GetFilterFromGet(this);
        if (inr.Count > 0 && inr[0].DatabaseOutput is Database dbin && !dbin.IsDisposed) {
            l.Add(new FlexiControlForProperty<string>(() => EntityID));
            l.Add(new FlexiControlForDelegate(Skript_Bearbeiten, "Skript bearbeiten", ImageCode.Skript));
        }

        if (_itemSends.DatabaseOutputGet(this) is Database dbout && !dbout.IsDisposed) {
            var lst = new List<AbstractListItem>();
            lst.AddRange(ItemsOf(dbout.Column, true));

            l.Add(new FlexiControlForProperty<string>(() => OriginIDColumnName, lst));
            l.Add(new FlexiControlForProperty<string>(() => AdditionalInfoColumnName, lst));
        }

        l.AddRange(base.GetProperties(widthOfControl));

        return l;
    }

    public override void ParseFinished(string parsed) {
        base.ParseFinished(parsed);
        _itemSends.ParseFinished(this);
        _itemAccepts.ParseFinished(this);
    }

    public override bool ParseThis(string key, string value) {
        if (base.ParseThis(key, value)) { return true; }
        if (_itemSends.ParseThis(key, value)) { return true; }
        if (_itemAccepts.ParseThis(key, value)) { return true; }

        switch (key) {
            case "entityid":
                _entityID = value.FromNonCritical();
                return true;

            case "entityidcolumnname":
                return true;

            case "textkeycolumnname":
                return true;

            case "originidcolumnname":
                _originIDColumnName = value;
                return true;

            case "additionalinfocolumnname":
                _additinalInfoColumnName = value;
                return true;

            case "counter":
                return true;

            case "adders":
                return true;

            case "script":
                Script = value.FromNonCritical();
                return true;
        }
        return false;
    }

    public override string ReadableText() {
        const string txt = "Zeilengenerator: ";

        if (this.IsOk() && DatabaseOutput is Database dbout && !dbout.IsDisposed) {
            return txt + dbout.Caption;
        }

        return txt + ErrorReason();
    }

    public void RemoveChild(IItemAcceptFilter remove) => _itemSends.RemoveChild(remove, this);

    public void Skript_Bearbeiten() {
        var x = new AdderScriptEditor();
        x.Item = this;
        x.Database = this.DatabaseInput;
        x.ShowDialog();
    }

    public override QuickImage? SymbolForReadableText() {
        if (this.IsOk()) {
            return QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, Skin.IdColor(OutputColorId));

            //return QuickImage.Get(ImageCode.Datenbank, 16, Color.Transparent, Skin.IdColor(InputColorId));
        }

        return QuickImage.Get(ImageCode.Warnung, 16);
    }

    public override string ToString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = [.. _itemAccepts.ParsableTags(), .. _itemSends.ParsableTags(this)];
        result.ParseableAdd("EntityID", _entityID);
        result.ParseableAdd("OriginIDColumnName", _originIDColumnName);
        result.ParseableAdd("AdditionalInfoColumnName", _additinalInfoColumnName);
        result.ParseableAdd("Script", Script);
        return result.Parseable(base.ToString());
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        DrawArrowOutput(gr, positionModified, zoom, shiftX, shiftY, forPrinting, OutputColorId);

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

        DrawArrorInput(gr, positionModified, zoom, forPrinting, InputColorId);
    }

    #endregion
}