// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using BlueControls.BlueTableDialogs;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueTable;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

/// <summary>
/// Erzeugt eine liste mit Zeile, die eine andere Tabelle befüllen können
/// </summary>
public class RowAdderPadItem : ReciverSenderControlPadItem, IItemToControl, IAutosizable, ISimpleEditor {

    #region Fields

    private string _additinalInfoColumnName = string.Empty;

    /// <summary>
    /// Eine eindeutige ID, die aus der eingehenen Zeile mit Variablen generiert wird.
    /// Dadurch können verschiedene Datensätze gespeichert werden.
    /// Beispiele: Rezepetname, Personenname, Beleg-Nummer
    /// </summary>
    private string _entityId = string.Empty;

    /// <summary>
    /// Eine Spalte in der Ziel-Tabelle.
    /// In diese wird die generierte ID des klickbaren Elements gespeichert.
    /// Diese wird automatisch generiert - es muss nur eine Spalte zur Verfügung gestellt werden.
    /// Beispiel: Zutaten#Vegetarisch/Mehl#3FFDKKJ34fJ4#1
    /// </summary>
    private string _originIdColumnName = string.Empty;

    private string _script_After = string.Empty;
    private string _script_Before = string.Empty;
    private string _script_MenuGeneration = string.Empty;

    #endregion

    #region Constructors

    public RowAdderPadItem() : this(string.Empty, null, null) { }

    public RowAdderPadItem(string keyName, ConnectedFormula.ConnectedFormula? cformula) : this(keyName, null, cformula) { }

    public RowAdderPadItem(string keyName, Table? db, ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula, db) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-RowAdder";

    public ColumnItem? AdditionalInfoColumn {
        get {
            if (TableOutput is not { IsDisposed: false } dbout) { return null; }

            var c = dbout.Column[_additinalInfoColumnName];
            return c is not { IsDisposed: false } ? null : c;
        }
    }

    [Description("Eine Spalte in der Ziel-Tabelle.\r\nIn diese wird eine Zusatzinfo gespeichert.\r\nDiese wird automatisch generiert - es muss nur eine Spalte zur Verfügung gestellt werden.")]
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

    public override string Description => "Ein Steuerelement, das eine andere Tabelle befüllen kann.\r\n" +
                                          "Aus der eingehenden Zeile wird eine ID generiert, diese wird zum dauerhaften Speichern in der Ausgangstabelle benutzt.\r\n" +
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

    /// <summary>
    /// Eine Spalte in der Ziel-Tabelle.
    /// In diese wird die generierte ID des klickbaren Elements gespeichert.
    /// Diese wird automatisch generiert - es muss nur eine Spalte zur Verfügung gestellt werden.
    /// Beispiel: Zutaten#Vegetarisch/Mehl#3FFDKKJ34fJ4#1
    /// </summary>
    public ColumnItem? OriginIDColumn {
        get {
            if (TableOutput is not { IsDisposed: false } dbout) { return null; }

            var c = dbout.Column[_originIdColumnName];
            return c is not { IsDisposed: false } ? null : c;
        }
    }

    /// <summary>
    /// Eine Spalte in der Ziel-Tabelle.
    /// In diese wird die generierte ID des klickbaren Elements gespeichert.
    /// Diese wird automatisch generiert - es muss nur eine Spalte zur Verfügung gestellt werden.
    /// Beispiel: Zutaten#Vegetarisch/Mehl#3FFDKKJ34fJ4#1
    /// </summary>
    [Description("Eine Spalte in der Ziel-Tabelle.\r\nIn diese wird die generierte ID des klickbaren Elements gespeichert.\r\nDiese wird automatisch generiert - es muss nur eine Spalte zur Verfügung gestellt werden.")]
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

    public string Script_After {
        get => _script_After;

        set {
            if (value == _script_After) { return; }
            _script_After = value;
            OnPropertyChanged();
        }
    }

    public string Script_Before {
        get => _script_Before;

        set {
            if (value == _script_Before) { return; }
            _script_Before = value;
            OnPropertyChanged();
        }
    }

    public string Script_MenuGeneration {
        get => _script_MenuGeneration;

        set {
            if (value == _script_MenuGeneration) { return; }
            _script_MenuGeneration = value;
            OnPropertyChanged();
        }
    }

    public override bool TableInputMustMatchOutputTable => false;

    #endregion

    #region Methods

    public Control CreateControl(ConnectedFormulaView parent, string mode) {
        var con = new RowAdder {
            EntityID = EntityID,
            OriginIDColumn = OriginIDColumn,
            AdditionalInfoColumn = AdditionalInfoColumn,
            Script_MenuGeneration = Script_MenuGeneration,
            Script_Before = Script_Before,
            Script_After = Script_After,
        };

        con.DoDefaultSettings(parent, this, mode);

        return con;
    }

    public override string ErrorReason() {
        if (string.IsNullOrEmpty(_entityId)) { return "Id-Generierung fehlt"; }
        if (!_entityId.Contains("~")) { return "ID-Generierung muss mit Variablen definiert werden."; }

        if (OriginIDColumn is not { IsDisposed: false } oic) {
            return "Spalte, in der die Herkunft-ID geschrieben werden soll, fehlt";
        }

        if (!oic.IsKeyColumn && !oic.IsFirst) {
            return $"Die Herkunft-ID-Spalte '{oic.Caption}' muss eine Schlüsselspalte oder die erste Spalte sein.";
        }

        if (AdditionalInfoColumn is not { IsDisposed: false } aci) {
            return "Spalte, in der die Zusatzinfo geschrieben werden soll, fehlt";
        }

        if (!aci.IsKeyColumn && !aci.IsFirst) {
            return $"Die Zusatzinfo-Spalte '{aci.Caption}' muss eine Schlüsselspalte oder die erste Spalte sein.";
        }

        if (string.IsNullOrEmpty(Script_MenuGeneration)) {
            return "Kein Skript für die Menugenerierung definiert.";
        }

        return base.ErrorReason();
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [
            .. base.GetProperties(widthOfControl),
            new FlexiControl("Einstellungen:", widthOfControl, true)
        ];

        var inr = GetFilterFromGet();
        if (inr.Count > 0 && inr[0].TableOutput is { IsDisposed: false }) {
            result.Add(new FlexiControlForProperty<string>(() => EntityID));
            result.Add(new FlexiDelegateControl(Skript_Bearbeiten, "Skript bearbeiten", ImageCode.Skript));
        }

        if (TableOutput is { IsDisposed: false } dbout) {
            var lst = new List<AbstractListItem>();
            lst.AddRange(ItemsOf(dbout.Column, true));

            result.Add(new FlexiControlForProperty<string>(() => OriginIDColumnName, lst));
            result.Add(new FlexiControlForProperty<string>(() => AdditionalInfoColumnName, lst));
        }

        return result;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("EntityID", _entityId);
        result.ParseableAdd("OriginIDColumnName", _originIdColumnName);
        result.ParseableAdd("AdditionalInfoColumnName", _additinalInfoColumnName);
        result.ParseableAdd("ScriptMenu", _script_MenuGeneration);
        result.ParseableAdd("ScriptAfter", _script_After);
        result.ParseableAdd("ScriptBefore", _script_Before);
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "entityid":
                _entityId = value.FromNonCritical();
                return true;

            case "originidcolumnname":
                _originIdColumnName = value;
                return true;

            case "additionalinfocolumnname":
                _additinalInfoColumnName = value;
                return true;

            case "script":
            case "scriptmenu":
                _script_MenuGeneration = value.FromNonCritical();
                return true;

            case "scriptbefore":
                _script_Before = value.FromNonCritical();
                return true;

            case "scriptafter":
                _script_After = value.FromNonCritical();
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() {
        const string txt = "Zeilengenerator: ";

        return txt + TableOutput?.Caption;
    }

    /// <summary>
    /// Internes Skript
    /// </summary>
    public void Skript_Bearbeiten() {
        var se = IUniqueWindowExtension.ShowOrCreate<RowAdderScriptEditor>(this);
        se.Table = TableInput;
    }

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, Skin.IdColor(OutputColorId));

    protected override void DrawExplicit(Graphics gr, Rectangle visibleArea, RectangleF positionModified, float scale, float shiftX, float shiftY) {
        DrawArrowOutput(gr, positionModified, scale, ForPrinting, OutputColorId);

        if (!ForPrinting) {
            DrawColorScheme(gr, positionModified, scale, InputColorId, true, true, false);
        }

        //if (Column  ==null || Column .IsDisposed) {
        //    Skin.Draw_FormatedText(gr, "Spalte fehlt", QuickImage.Get(ImageCode.Warnung, (int)(16 * zoom)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), CaptionFnt.Scale(zoom), true);
        //} else {
        //DrawFakeControl(gr, positionModified, scale, CaptionPosition, Column?.ReadableText() + ":", EditType);
        //}

        if (!ForPrinting) {
            DrawColorScheme(gr, positionModified, scale, InputColorId, true, true, true);
        }

        base.DrawExplicit(gr, visibleArea, positionModified, scale, shiftX, shiftY);

        DrawArrorInput(gr, positionModified, scale, ForPrinting, InputColorId);
    }

    #endregion
}