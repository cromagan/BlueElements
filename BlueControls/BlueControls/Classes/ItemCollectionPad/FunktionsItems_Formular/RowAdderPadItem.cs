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
using System.ComponentModel;
using System.Drawing;
using BlueDatabase.Enums;
using BlueControls.BlueDatabaseDialogs;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

/// <summary>
/// Erzeugt eine liste mit Zeile, die eine andere Tabelle befüllen können
/// </summary>
public class RowAdderPadItem : ReciverSenderControlPadItem, IItemToControl, IReadableText, IAutosizable, ISimpleEditor {

    #region Fields

    private string _additinalInfoColumnName = string.Empty;

    /// <summary>
    /// Eine eindeutige ID, die aus der eingehenen Zeile mit Variablen generiert wird.
    /// Dadurch können verschiedene Datensätze gespeichert werden.
    /// Beispiele: Rezepetname, Personenname, Beleg-Nummer
    /// </summary>
    private string _entityId = string.Empty;

    /// <summary>
    /// Eine Spalte in der Ziel-Datenbank.
    /// In diese wird die generierte ID des klickbaren Elements gespeichert.
    /// Diese wird automatisch generiert - es muss nur eine Spalte zur Verfügung gestellt werden.
    /// Beispiel: Zutaten#Vegetarisch/Mehl#3FFDKKJ34fJ4#1
    /// </summary>
    private string _originIdColumnName = string.Empty;

    private string _script = string.Empty;

    #endregion

    #region Constructors

    public RowAdderPadItem(string keyName) : this(keyName, null, null) { }

    public RowAdderPadItem(string keyName, ConnectedFormula.ConnectedFormula? cformula) : this(keyName, null, cformula) { }

    public RowAdderPadItem(string keyName, Database? db, ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula, db) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-RowAdder";

    public ColumnItem? AdditionalInfoColumn {
        get {
            if (DatabaseOutput is not Database dbout || dbout.IsDisposed) { return null; }

            var c = dbout.Column[_additinalInfoColumnName];
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

    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.One;
    public bool AutoSizeableHeight => true;

    public override bool DatabaseInputMustMatchOutputDatabase => false;

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
        get => _entityId;
        set {
            if (IsDisposed) { return; }
            if (_entityId == value) { return; }
            _entityId = value;
            OnPropertyChanged();
        }
    }

    public override bool InputMustBeOneRow => true;
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
            if (DatabaseOutput is not Database dbout || dbout.IsDisposed) { return null; }

            var c = dbout.Column[_originIdColumnName];
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
        get => _originIdColumnName;
        set {
            if (IsDisposed) { return; }
            if (_originIdColumnName == value) { return; }
            _originIdColumnName = value;
            OnPropertyChanged();
            //UpdateSideOptionMenu();
        }
    }

    public string Script {
        get => _script;

        set {
            if (value == _script) { return; }
            _script = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

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
        if (string.IsNullOrEmpty(_entityId)) { return "Id-Generierung fehlt"; }
        if (!_entityId.Contains("~")) { return "ID-Generierung muss mit Variablen definiert werden."; }

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

        return base.ErrorReason();
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        var l = new List<GenericControl>();

        l.AddRange(base.GetProperties(widthOfControl));

        l.Add(new FlexiControl("Eigenschaften:", widthOfControl, true));
        var inr = GetFilterFromGet();
        if (inr.Count > 0 && inr[0].DatabaseOutput is Database dbin && !dbin.IsDisposed) {
            l.Add(new FlexiControlForProperty<string>(() => EntityID));
            l.Add(new FlexiControlForDelegate(Skript_Bearbeiten, "Skript bearbeiten", ImageCode.Skript));
        }

        if (DatabaseOutput is Database dbout && !dbout.IsDisposed) {
            var lst = new List<AbstractListItem>();
            lst.AddRange(ItemsOf(dbout.Column, true));

            l.Add(new FlexiControlForProperty<string>(() => OriginIDColumnName, lst));
            l.Add(new FlexiControlForProperty<string>(() => AdditionalInfoColumnName, lst));
        }

        l.AddRange(base.GetProperties(widthOfControl));

        return l;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "entityid":
                _entityId = value.FromNonCritical();
                return true;

            case "entityidcolumnname":
                return true;

            case "textkeycolumnname":
                return true;

            case "originidcolumnname":
                _originIdColumnName = value;
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
        return base.ParseThis(key, value);
    }

    public override string ReadableText() {
        const string txt = "Zeilengenerator: ";

        if (this.IsOk() && DatabaseOutput is Database dbout && !dbout.IsDisposed) {
            return txt + dbout.Caption;
        }

        return txt + ErrorReason();
    }

    public override QuickImage SymbolForReadableText() {
        if (this.IsOk()) {
            return QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, Skin.IdColor(OutputColorId));

            //return QuickImage.Get(ImageCode.Datenbank, 16, Color.Transparent, Skin.IdColor(InputColorId));
        }

        return QuickImage.Get(ImageCode.Warnung, 16);
    }

    public override string ToParseableString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = [];
        result.ParseableAdd("EntityID", _entityId);
        result.ParseableAdd("OriginIDColumnName", _originIdColumnName);
        result.ParseableAdd("AdditionalInfoColumnName", _additinalInfoColumnName);
        result.ParseableAdd("Script", Script);
        return result.Parseable(base.ToParseableString());
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

    private void Skript_Bearbeiten() {
        var x = new AdderScriptEditor();
        x.Item = this;
        x.Database = DatabaseInput;
        x.ShowDialog();
    }

    #endregion
}