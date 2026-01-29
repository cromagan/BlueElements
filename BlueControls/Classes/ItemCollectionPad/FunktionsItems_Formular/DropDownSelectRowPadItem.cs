// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueBasics;
using BlueBasics.Classes;
using BlueBasics.Enums;
using BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueControls.ConnectedFormula;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueTable.Classes;
using BlueTable.Enums;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static BlueBasics.ClassesStatic.Converter;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular;

public class DropDownSelectRowPadItem : ReciverSenderControlPadItem, IItemToControl, IAutosizable {

    #region Fields

    private string _anzeige = string.Empty;
    private EditTypeFormula _bearbeitung = EditTypeFormula.Textfeld_mit_Auswahlknopf;
    private string _caption = string.Empty;
    private CaptionPosition _captionPosition = CaptionPosition.Über_dem_Feld;

    #endregion

    #region Constructors

    public DropDownSelectRowPadItem() : this(string.Empty, null, null) { }

    public DropDownSelectRowPadItem(string keyName, Controls.ConnectedFormula.ConnectedFormula? cformula) : this(keyName, null, cformula) { }

    public DropDownSelectRowPadItem(string keyName, Table? db, Controls.ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula, db) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-SelectRowWithDropDownMenu";

    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.More;

    [Description("Nach welchem Format die Zeilen angezeigt werden sollen. Es können Variablen im Format ~Variable~ benutzt werden. Achtung, KEINE Skript-Variaben, nur Spaltennamen.")]
    public string Anzeige {
        get => _anzeige;
        set {
            if (IsDisposed) { return; }
            if (_anzeige == value) { return; }
            _anzeige = value;
            OnPropertyChanged();
        }
    }

    public bool AutoSizeableHeight => false;

    public string Caption {
        get => _caption;
        set {
            if (IsDisposed) { return; }
            if (_caption == value) { return; }
            _caption = value;
            OnPropertyChanged();
        }
    }

    public CaptionPosition CaptionPosition {
        get => _captionPosition;
        set {
            if (IsDisposed) { return; }
            if (_captionPosition == value) { return; }
            _captionPosition = value;
            OnPropertyChanged();
        }
    }

    public override string Description => "Ein Auswahlmenü, aus dem der Benutzer eine Zeile wählen kann, die durch die Vor-Filter bestimmt wurden.";
    public override bool InputMustBeOneRow => false;
    public override bool MustBeInDrawingArea => true;
    public override bool TableInputMustMatchOutputTable => true;
    protected override int SaveOrder => 1;

    #endregion

    #region Methods

    public Control CreateControl(ConnectedFormulaView parent, string mode) {
        var con = new FlexiControlForRowSelector(TableOutput, _caption, _anzeige) {
            EditType = _bearbeitung,
            CaptionPosition = CaptionPosition
        };

        con.DoDefaultSettings(parent, this, mode);

        return con;
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [
            .. base.GetProperties(widthOfControl),
            new FlexiControl("Einstellungen:", -1, true),
            new FlexiControlForProperty<string>(() => Caption),
            new FlexiControlForProperty<string>(() => Anzeige),
        ];

        result.Add(new FlexiControlForProperty<CaptionPosition>(() => CaptionPosition, ItemsOf(typeof(CaptionPosition))));

        return result;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("CaptionText", _caption);
        result.ParseableAdd("ShowFormat", _anzeige);
        result.ParseableAdd("EditType", _bearbeitung);
        result.ParseableAdd("Caption", _captionPosition);
        //result.ParseableAdd("ID", ColorId);

        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "id":
            case "style":
                return true;

            case "edittype":
                _bearbeitung = (EditTypeFormula)IntParse(value);
                return true;

            case "caption":
                _captionPosition = (CaptionPosition)IntParse(value);
                return true;

            case "captiontext":
                _caption = value.FromNonCritical();
                return true;

            case "showformat":
                _anzeige = value.FromNonCritical();
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() {
        const string txt = "Zeilenauswahl: ";

        return txt + TableOutput?.Caption;
    }

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, Skin.IdColor(OutputColorId));

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float zoom, float offsetX, float offsetY, bool forPrinting) {
        if (!forPrinting) {
            DrawArrowOutput(gr, positionControl, zoom, forPrinting, OutputColorId);
            DrawFakeControl(gr, positionControl, zoom, CaptionPosition, _caption, EditTypeFormula.Textfeld_mit_Auswahlknopf);
            DrawColorScheme(gr, positionControl, zoom, null, true, true, true);
        } else {
            DrawFakeControl(gr, positionControl, zoom, CaptionPosition, _caption, EditTypeFormula.Textfeld_mit_Auswahlknopf);
        }

        base.DrawExplicit(gr, visibleAreaControl, positionControl, zoom, offsetX, offsetY, forPrinting);
        DrawArrorInput(gr, positionControl, zoom, forPrinting, InputColorId);
    }

    #endregion
}