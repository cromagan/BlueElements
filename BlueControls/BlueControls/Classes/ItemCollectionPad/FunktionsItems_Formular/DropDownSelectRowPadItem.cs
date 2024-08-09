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
using BlueControls.ConnectedFormula;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueDatabase;
using BlueDatabase.Enums;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using static BlueBasics.Converter;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

public class DropDownSelectRowPadItem : ReciverSenderControlPadItem, IItemToControl, IReadableText, IAutosizable {

    #region Fields

    private string _anzeige = string.Empty;
    private EditTypeFormula _bearbeitung = EditTypeFormula.Textfeld_mit_Auswahlknopf;
    private string _überschrift = string.Empty;
    private CaptionPosition _überschriftanordung = CaptionPosition.Über_dem_Feld;

    #endregion

    #region Constructors

    public DropDownSelectRowPadItem(string keyName) : this(keyName, null, null) { }

    public DropDownSelectRowPadItem(string keyName, Database? db, ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula, db) { }

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

    public CaptionPosition CaptionPosition {
        get => _überschriftanordung;
        set {
            if (IsDisposed) { return; }
            if (_überschriftanordung == value) { return; }
            _überschriftanordung = value;
            OnPropertyChanged();
        }
    }

    public override bool DatabaseInputMustMatchOutputDatabase => true;

    public override string Description => "Ein Auswahlmenü, aus dem der Benutzer eine Zeile wählen kann, die durch die Vor-Filter bestimmt wurden.";
    public override bool InputMustBeOneRow => false;
    public override bool MustBeInDrawingArea => true;
    public override string MyClassId => ClassId;

    public string Überschrift {
        get => _überschrift;
        set {
            if (IsDisposed) { return; }
            if (_überschrift == value) { return; }
            _überschrift = value;
            OnPropertyChanged();
        }
    }

    protected override int SaveOrder => 1;

    #endregion

    #region Methods

    public System.Windows.Forms.Control CreateControl(ConnectedFormulaView parent, string mode) {
        var con = new FlexiControlRowSelector(DatabaseOutput, _überschrift, _anzeige) {
            EditType = _bearbeitung,
            CaptionPosition = CaptionPosition
        };

        con.DoDefaultSettings(parent, this, mode);

        return con;
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> l =
        [
            .. base.GetProperties(widthOfControl),
            new FlexiControl("Einstellungen:", -1, true),
            new FlexiControlForProperty<string>(() => Überschrift),
            new FlexiControlForProperty<string>(() => Anzeige),
        ];

        var u = new List<AbstractListItem>();
        u.AddRange(ItemsOf(typeof(CaptionPosition)));
        l.Add(new FlexiControlForProperty<CaptionPosition>(() => CaptionPosition, u));

        //l.Add(new FlexiControl());
        l.AddRange(base.GetProperties(widthOfControl));
        return l;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "id":
                //ColorId = IntParse(value);
                return true;

            case "edittype":
                _bearbeitung = (EditTypeFormula)IntParse(value);
                return true;

            case "caption":
                _überschriftanordung = (CaptionPosition)IntParse(value);
                return true;

            case "captiontext":
                _überschrift = value.FromNonCritical();
                return true;

            case "showformat":
                _anzeige = value.FromNonCritical();
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() {
        const string txt = "Zeilenauswahl: ";

        if (this.IsOk() && DatabaseOutput != null) {
            return txt + DatabaseOutput.Caption;
        }

        return txt + ErrorReason();
    }

    public override QuickImage SymbolForReadableText() {
        if (this.IsOk()) {
            return QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, Skin.IdColor(OutputColorId));
        }

        return QuickImage.Get(ImageCode.Warnung, 16);
    }

    public override string ToParseableString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = [];

        result.ParseableAdd("CaptionText", _überschrift);
        result.ParseableAdd("ShowFormat", _anzeige);
        result.ParseableAdd("EditType", _bearbeitung);
        result.ParseableAdd("Caption", _überschriftanordung);
        //result.ParseableAdd("ID", ColorId);

        return result.Parseable(base.ToParseableString());
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        if (!forPrinting) {
            DrawArrowOutput(gr, positionModified, zoom, shiftX, shiftY, forPrinting, OutputColorId);
            DrawFakeControl(gr, positionModified, zoom, CaptionPosition, _überschrift, EditTypeFormula.Textfeld_mit_Auswahlknopf);
            DrawColorScheme(gr, positionModified, zoom, null, true, true, true);
        } else {
            DrawFakeControl(gr, positionModified, zoom, CaptionPosition, _überschrift, EditTypeFormula.Textfeld_mit_Auswahlknopf);
        }

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, true);
        DrawArrorInput(gr, positionModified, zoom, forPrinting, InputColorId);
    }

    #endregion
}