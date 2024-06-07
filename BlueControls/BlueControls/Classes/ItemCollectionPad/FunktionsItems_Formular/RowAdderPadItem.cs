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
using BlueControls.Editoren;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

/// <summary>
/// Erzeugt eine liste mit Zeile, die eine andere Tabelle befüllen können
/// </summary>
public class RowAdderPadItem : FakeControlPadItem, IReadableText, IItemToControl, IItemAcceptFilter, IItemSendFilter, IHasVersion, IAutosizable, ISimpleEditor {

    #region Fields

    private readonly ItemAcceptFilter _itemAccepts;
    private readonly ItemSendFilter _itemSends;

    private List<RowAdderSingle> _addersingle = new();

    private string _additionalTextColumnName = string.Empty;

    private string _entityIDColumnName = string.Empty;

    private string _inputDatabase_EntityID = string.Empty;

    private string _originIDColumnName = string.Empty;

    private string _textKeyColumnName = string.Empty;

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

    public ColumnItem? AdditinalTextColumn {
        get {
            if (_itemSends.DatabaseOutputGet() is not Database dbout || dbout.IsDisposed) { return null; }

            var c = dbout?.Column[_additionalTextColumnName];
            return c == null || c.IsDisposed ? null : c;
        }
    }

    [Description("Eine Spalte in der Ziel-Datenbank.\r\nIn diese wird der Zusatztext gespeichert.\r\nBeispiele: Anzahl oder Objekt-Beschreibungen")]
    public string AdditionalTextColumnName {
        get => _additionalTextColumnName;
        set {
            if (IsDisposed) { return; }
            if (_additionalTextColumnName == value) { return; }
            _additionalTextColumnName = value;
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
                                            "Diese ID wird auch aus Ausgangsfilter weitergegeben.";

    [Description("Eine eindeutige ID, die aus der eingehenen Zeile mit Variablen generiert wird.\r\nDadurch können verschiedene Datensätze gespeichert werden.\r\nBeispiele: Rezepetname, Personenname, Beleg-Nummer")]
    public string EntityID {
        get => _inputDatabase_EntityID;
        set {
            if (IsDisposed) { return; }
            if (_inputDatabase_EntityID == value) { return; }
            _inputDatabase_EntityID = value;
            OnPropertyChanged();
        }
    }

    public ColumnItem? EntityIDColumn {
        get {
            if (_itemSends.DatabaseOutputGet() is not Database dbout || dbout.IsDisposed) { return null; }

            var c = dbout?.Column[_entityIDColumnName];
            return c == null || c.IsDisposed ? null : c;
        }
    }

    [Description("Eine Spalte in der Ziel-Datenbank.\r\nIn diese wird die generierte ID der eingehenden Datenbank gespeichert.\r\nDadurch können verschiedene Datensätze gespeichert werden.")]
    public string EntityIDColumnName {
        get => _entityIDColumnName;
        set {
            if (IsDisposed) { return; }
            if (_entityIDColumnName == value) { return; }
            _entityIDColumnName = value;
            OnPropertyChanged();
            //UpdateSideOptionMenu();
        }
    }

    public List<int> InputColorId => _itemAccepts.InputColorIdGet(this);

    public override bool MustBeInDrawingArea => true;

    public bool MustBeOneRow => true;

    public ColumnItem? OriginIDColumn {
        get {
            if (_itemSends.DatabaseOutputGet() is not Database dbout || dbout.IsDisposed) { return null; }

            var c = dbout?.Column[_originIDColumnName];
            return c == null || c.IsDisposed ? null : c;
        }
    }

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

    public ColumnItem? TextKeyColumn {
        get {
            if (_itemSends.DatabaseOutputGet() is not Database dbout || dbout.IsDisposed) { return null; }

            var c = dbout?.Column[_textKeyColumnName];
            return c == null || c.IsDisposed ? null : c;
        }
    }

    [Description("Eine Spalte in der Ziel-Datenbank.\r\nIn diese wird ein Textschlüssel gespeichert. Mit diesem kann dann die Ziel-Datenbank weiter Aktionen ausführen..")]
    public string TextKeyColumnName {
        get => _textKeyColumnName;
        set {
            if (IsDisposed) { return; }
            if (_textKeyColumnName == value) { return; }
            _textKeyColumnName = value;
            OnPropertyChanged();
            //UpdateSideOptionMenu();
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

        if (string.IsNullOrEmpty(_inputDatabase_EntityID)) { return "Id-Generierung fehlt"; }
        if (!_inputDatabase_EntityID.Contains("~")) { return "ID-Generierung muss mit Variablen definiert werden."; }

        if (EntityIDColumn is not ColumnItem eic || eic.IsDisposed) {
            return "Spalte, in der die Entitäten-ID geschrieben werden soll, fehlt";
        }

        if (eic.Function != BlueDatabase.Enums.ColumnFunction.Schlüsselspalte) {
            return "Die Entitäten-ID-Spalte muss eine Schlüsselspalte sein.";
        }

        if (OriginIDColumn is not ColumnItem oic || oic.IsDisposed) {
            return "Spalte, in der die  Herkunft-ID geschrieben werden soll, fehlt";
        }

        if (oic.Function != BlueDatabase.Enums.ColumnFunction.Schlüsselspalte) {
            return "Die Herkunft-ID-Spalte muss eine Schlüsselspalte sein.";
        }

        if (TextKeyColumn == null || TextKeyColumn.IsDisposed) {
            return "Spalte, in der die Entitäten-ID geschrieben werden soll, fehlt";
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

    public override List<GenericControl> GetProperties(int widthOfControl) {
        var l = new List<GenericControl>();

        l.AddRange(_itemAccepts.GetStyleOptions(this, widthOfControl));

        l.AddRange(_itemSends.GetStyleOptions(this, widthOfControl));

        l.Add(new FlexiControl("Eigenschaften:", widthOfControl, true));
        var inr = _itemAccepts.GetFilterFromGet(this);
        if (inr.Count > 0 && inr[0].DatabaseOutput is Database dbin && !dbin.IsDisposed) {
            l.Add(new FlexiControlForProperty<string>(() => EntityID));
        }

        if (_itemSends.DatabaseOutputGet() is Database dbout && !dbout.IsDisposed) {
            var lst = new List<AbstractListItem>();
            lst.AddRange(ItemsOf(dbout.Column, true));

            l.Add(new FlexiControlForProperty<string>(() => EntityIDColumnName, lst));

            l.Add(new FlexiControlForProperty<string>(() => OriginIDColumnName, lst));

            l.Add(new FlexiControlForProperty<string>(() => TextKeyColumnName, lst));

            l.Add(new FlexiControlForProperty<string>(() => AdditionalTextColumnName, lst));
        }

        l.Add(new FlexiControl("Bausteine:", widthOfControl, true));
        l.Add(Childs());

        return l;
    }

    public AbstractListItem? NewChild() {
        var l = new RowAdderSingle(this);
        l.Editor = typeof(RowAdderSingleEditor);
        return ItemOf(l);
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
                _inputDatabase_EntityID = value.FromNonCritical();
                return true;

            case "entityidcolumnname":
                _entityIDColumnName = value;
                return true;

            case "textkeycolumnname":
                _textKeyColumnName = value;
                return true;

            case "originidcolumnname":
                _originIDColumnName = value;
                return true;

            case "additionaltextcolumnname":
                _additionalTextColumnName = value;
                return true;

            case "adders":
                foreach (var pair2 in value.GetAllTags()) {
                    _addersingle.Add(new RowAdderSingle(this, pair2.Value.FromNonCritical()));
                }

                break;

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
        if (this.IsOk()) {
            return QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, Skin.IdColor(OutputColorId));

            //return QuickImage.Get(ImageCode.Datenbank, 16, Color.Transparent, Skin.IdColor(InputColorId));
        }

        return QuickImage.Get(ImageCode.Warnung, 16);
    }

    public override string ToString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = [.. _itemAccepts.ParsableTags(), .. _itemSends.ParsableTags()];

        //result.ParseableAdd("TargetDatabase", _targetDatabase); // Nicht _database, weil sie evtl. noch nicht geladen ist

        result.ParseableAdd("EntityID", _inputDatabase_EntityID);
        result.ParseableAdd("EntityIDColumnName", _entityIDColumnName);
        result.ParseableAdd("OriginIDColumnName", _originIDColumnName);
        result.ParseableAdd("TextKeyColumnName", _textKeyColumnName);
        result.ParseableAdd("AdditionalTextColumnName", _additionalTextColumnName);
        result.ParseableAdd("Adders", "Item", _addersingle);

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

        DrawArrorInput(gr, positionModified, zoom, shiftX, shiftY, forPrinting, InputColorId);
    }

    private ListBox Childs() {
        var childs = new ListBox {
            AddAllowed = AddType.UserDef,
            RemoveAllowed = true,
            MoveAllowed = true,
            AutoSort = false,
            ItemEditAllowed = true,
            CheckBehavior = CheckBehavior.AllSelected,
            AddMethod = NewChild
        };

        CFormula?.AddChilds(childs.Suggestions, CFormula.NotAllowedChilds);

        foreach (var thisf in _addersingle) {
            //if (File.Exists(thisf)) {
            //    childs.AddAndCheck(new TextListItem(thisf.FileNameWithoutSuffix(), thisf, QuickImage.Get(ImageCode.Diskette, 16), false, true, string.Empty));
            //} else {
            childs.AddAndCheck(ItemOf(thisf));
            //}
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
        _addersingle.Clear();

        foreach (var item in ((ListBox)sender).CheckedItems()) {
            if (item is ReadableListItem rli && rli.Item is RowAdderSingle ras) {
                _addersingle.Add(ras);
            }
        }
        OnPropertyChanged();
        this.RaiseVersion();
        UpdateSideOptionMenu();
    }

    #endregion
}