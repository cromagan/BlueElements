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
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueDatabase.Enums;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

public class EasyPicPadItem : ReciverControlPadItem, IItemToControl, IAutosizable {

    #region Fields

    private string _bild_Dateiname = string.Empty;

    #endregion

    #region Constructors

    public EasyPicPadItem(string keyName) : this(keyName, null) { }

    public EasyPicPadItem(string keyName, ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula) {
        SetCoordinates(new RectangleF(0, 0, 50, 30), true);
    }

    #endregion

    #region Properties

    public static string ClassId => "FI-EasyPic";
    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.One;
    public bool AutoSizeableHeight => true;

    [Description("Der Dateiname des Bildes, das angezeigt werden sollen.\r\nEs können Variablen aus dem Skript benutzt werden.\r\nDiese müssen im Format ~variable~ angegeben werden.")]
    public string Bild_Dateiname {
        get => _bild_Dateiname;

        set {
            if (IsDisposed) { return; }
            if (value == _bild_Dateiname) { return; }
            _bild_Dateiname = value;
            OnPropertyChanged();
        }
    }

    public override bool DatabaseInputMustMatchOutputDatabase => false;
    public override string Description => "Eine Bild-Anzeige,\r\nmit welchem der Benutzer interagieren kann.";

    public override bool InputMustBeOneRow => true;
    public override bool MustBeInDrawingArea => true;
    public override string MyClassId => ClassId;

    protected override int SaveOrder => 4;

    #endregion

    #region Methods

    public System.Windows.Forms.Control CreateControl(ConnectedFormulaView parent, string mode) {
        var con = new EasyPic {
            OriginalText = Bild_Dateiname
        };

        con.DoDefaultSettings(parent, this, mode);
        return con;
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [
            .. base.GetProperties(widthOfControl),
            new FlexiControl("Einstellungen:", widthOfControl, true),
            new FlexiControlForProperty<string>(() => Bild_Dateiname),
       
        ];
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "imagename":
                _bild_Dateiname = value.FromNonCritical();
                return true;
        }

        return base.ParseThis(key, value);
    }

    public override string ReadableText() {
        const string txt = "Bild: ";

        if (this.IsOk() && DatabaseInput != null) {
            return txt + DatabaseInput.Caption;
        }

        return txt + ErrorReason();
    }

    public override QuickImage SymbolForReadableText() {
        if (this.IsOk()) {
            return QuickImage.Get(ImageCode.Bild, 16, Color.Transparent, Skin.IdColor(InputColorId));
        }

        return QuickImage.Get(ImageCode.Warnung, 16);
    }

    public override string ToParseableString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = [];

        result.ParseableAdd("ImageName", _bild_Dateiname);
        return result.Parseable(base.ToParseableString());
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        //var id = GetRowFrom?.OutputColorId ?? - 1;

        if (!forPrinting) {
            DrawColorScheme(gr, positionModified, zoom, InputColorId, true, true, false);
        }

        DrawFakeControl(gr, positionModified, zoom, CaptionPosition.Über_dem_Feld, "Bilddatei", EditTypeFormula.Listbox);

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
        DrawArrorInput(gr, positionModified, zoom, forPrinting, InputColorId);
    }

    #endregion

    //protected override AbstractPadItem? TryCreate(string id, string name) {
    //    if (id.Equals(ClassId, StringComparison.OrdinalIgnoreCase)) {
    //        return new EasyPicPadItem(name);
    //    }
    //    return null;
    //}
}