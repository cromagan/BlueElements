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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueDatabase;
using BlueDatabase.Enums;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

public class EasyPicPadItem : FakeControlPadItem, IItemToControl, IItemAcceptFilter, IAutosizable {

    #region Fields

    private readonly ItemAcceptSomething _itemAccepts;

    private string _bild_Dateiname = string.Empty;

    #endregion

    #region Constructors

    public EasyPicPadItem(string internalname) : base(internalname) {
        _itemAccepts = new();
        SetCoordinates(new RectangleF(0, 0, 50, 30), true);
    }

    #endregion

    #region Properties

    public static string ClassId => "FI-EasyPic";
    public bool AutoSizeableHeight => true;

    [Description("Der Dateiname des Bildes, das angezeigt werden sollen.\r\nEs können Variablen aus dem Skript benutzt werden.\r\nDiese müssen im Format ~variable~ angegeben werden.")]
    public string Bild_Dateiname {
        get => _bild_Dateiname;

        set {
            if (IsDisposed) { return; }
            if (value == _bild_Dateiname) { return; }
            _bild_Dateiname = value;
            this.RaiseVersion();
            OnChanged();
        }
    }

    public Database? DatabaseInput => _itemAccepts.DatabaseInput(this);
    public bool DatabaseInputMustMatchOutputDatabase => false;

    public override string Description => "Eine Bild-Anzeige,\r\nmit welchem der Benutzer interagieren kann.";
    public List<int> InputColorId => _itemAccepts.InputColorIdGet(this);
    public override bool MustBeInDrawingArea => true;

    public bool MustBeOneRow => true;

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ReadOnlyCollection<string> Parents {
        get => _itemAccepts.GetFilterFromKeysGet();
        set => _itemAccepts.GetFilterFromKeysSet(value, this);
    }

    protected override int SaveOrder => 4;

    #endregion

    #region Methods

    public void CalculateInputColorIds() => _itemAccepts.CalculateInputColorIds(this);

    public override System.Windows.Forms.Control CreateControl(ConnectedFormulaView parent) {
        var con = new EasyPic {
            OriginalText = Bild_Dateiname
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

        return string.Empty;
    }

    public override List<GenericControl> GetStyleOptions(int widthOfControl) {
        List<GenericControl> l =
        [
            .. _itemAccepts.GetStyleOptions(this, widthOfControl),
            new FlexiControlForProperty<string>(() => Bild_Dateiname),
            new FlexiControl(),
            .. base.GetStyleOptions(widthOfControl),
        ];
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
            case "imagename":
                _bild_Dateiname = value.FromNonCritical();
                return true;
        }
        return false;
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

    public override string ToString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = [.. _itemAccepts.ParsableTags()];

        result.ParseableAdd("ImageName", _bild_Dateiname);
        return result.Parseable(base.ToString());
    }

    internal override void AddedToCollection() {
        base.AddedToCollection();
        //_itemSends.DoCreativePadAddedToCollection(this);
        _itemAccepts.DoCreativePadAddedToCollection(this);
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        //var id = GetRowFrom?.OutputColorId ?? - 1;

        if (!forPrinting) {
            DrawColorScheme(gr, positionModified, zoom, InputColorId, true, true, false);
        }

        DrawFakeControl(gr, positionModified, zoom, CaptionPosition.Über_dem_Feld, "Bilddatei", EditTypeFormula.Listbox);

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
        DrawArrorInput(gr, positionModified, zoom, shiftX, shiftY, forPrinting, InputColorId);
    }

    #endregion

    //protected override AbstractPadItem? TryCreate(string id, string name) {
    //    if (id.Equals(ClassId, StringComparison.OrdinalIgnoreCase)) {
    //        return new EasyPicPadItem(name);
    //    }
    //    return null;
    //}
}