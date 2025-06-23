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
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueDatabase.Enums;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

public class EasyPicPadItem : ReciverControlPadItem, IItemToControl, IAutosizable {

    #region Fields

    private bool _bearbeitbar;
    private string _bild_dateiname = string.Empty;

    #endregion

    #region Constructors

    public EasyPicPadItem() : this(string.Empty, null) { }

    public EasyPicPadItem(string keyName, ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula) => SetCoordinates(new RectangleF(0, 0, 50, 30));

    #endregion

    #region Properties

    // ReSharper disable once UnusedMember.Global
    public static string ClassId => "FI-EasyPic";

    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.One;
    public bool AutoSizeableHeight => true;

    public bool Bearbeitbar {
        get => _bearbeitbar;

        set {
            if (IsDisposed) { return; }
            if (value == _bearbeitbar) { return; }
            _bearbeitbar = value;
            OnPropertyChanged();
        }
    }

    [Description("Der Dateiname des Bildes, das angezeigt werden sollen.\r\nEs können Variablen aus dem Skript benutzt werden.\r\nDiese müssen im Format ~variable~ angegeben werden.")]
    public string Bild_Dateiname {
        get => _bild_dateiname;

        set {
            if (IsDisposed) { return; }
            if (value == _bild_dateiname) { return; }
            _bild_dateiname = value;
            OnPropertyChanged();
        }
    }

    public override bool DatabaseInputMustMatchOutputDatabase => false;
    public override string Description => "Eine Bild-Anzeige,\r\nmit welchem der Benutzer interagieren kann.";

    public override bool InputMustBeOneRow => true;
    public override bool MustBeInDrawingArea => true;

    protected override int SaveOrder => 4;

    #endregion

    #region Methods

    public Control CreateControl(ConnectedFormulaView parent, string mode) {
        var con = new EasyPic {
            OriginalText = Bild_Dateiname,
            Editable = Bearbeitbar
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
            new FlexiControlForProperty<bool>(() => Bearbeitbar),

        ];
        return result;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("ImageName", _bild_dateiname);
        result.ParseableAdd("Editable", _bearbeitbar);
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "imagename":
                _bild_dateiname = value.FromNonCritical();
                return true;

            case "editable":
                _bearbeitbar = value.FromPlusMinus();
                return true;
        }

        return base.ParseThis(key, value);
    }

    public override string ReadableText() {
        const string txt = "Bild-Editor: ";

        return txt + DatabaseInput?.Caption;
    }

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Bild, 16, Color.Transparent, Skin.IdColor(InputColorId));

    protected override void DrawExplicit(Graphics gr, Rectangle visibleArea, RectangleF positionModified, float scale, float shiftX, float shiftY) {
        //var id = GetRowFrom?.OutputColorId ?? - 1;

        if (!ForPrinting) {
            DrawColorScheme(gr, positionModified, scale, InputColorId, true, true, false);
        }

        DrawFakeControl(gr, positionModified, scale, CaptionPosition.Über_dem_Feld, "Bilddatei", EditTypeFormula.Listbox);

        base.DrawExplicit(gr, visibleArea, positionModified, scale, shiftX, shiftY);
        DrawArrorInput(gr, positionModified, scale, ForPrinting, InputColorId);
    }

    #endregion
}