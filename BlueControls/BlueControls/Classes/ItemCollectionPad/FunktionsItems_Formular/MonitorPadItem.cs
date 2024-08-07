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
using BlueDatabase;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;

#nullable enable

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

public class MonitorPadItem : FakeControlPadItem, IItemToControl, IItemAcceptFilter, IAutosizable {

    #region Fields

    private readonly ItemAcceptFilter _itemAccepts;

    #endregion

    #region Constructors

    public MonitorPadItem(string keyName) : this(keyName, null) { }

    public MonitorPadItem(string keyName, ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula) {
        _itemAccepts = new();
        SetCoordinates(new RectangleF(0, 0, 50, 30), true);
    }

    #endregion

    #region Properties

    public static string ClassId => "FI-Monitor";
    public AllowedInputFilter AllowedInputFilter => AllowedInputFilter.One;
    public bool AutoSizeableHeight => true;
    public Database? DatabaseInput => _itemAccepts.DatabaseInputGet(this);
    public bool DatabaseInputMustMatchOutputDatabase => false;
    public override string Description => "Zeigt Änderungen einer Zeile an.";
    public List<int> InputColorId => _itemAccepts.InputColorIdGet(this);
    public bool InputMustBeOneRow => true;
    public override bool MustBeInDrawingArea => true;
    public override string MyClassId => ClassId;

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ReadOnlyCollection<string> Parents {
        get => _itemAccepts.GetFilterFromKeysGet();
        set => _itemAccepts.GetFilterFromKeysSet(value, this);
    }

    protected override int SaveOrder => 5;

    #endregion

    #region Methods

    public override void AddedToCollection() {
        base.AddedToCollection();
        //_itemSends.DoCreativePadAddedToCollection(this);
        _itemAccepts.DoCreativePadAddedToCollection(this);
        //RepairConnections();
    }

    public void CalculateInputColorIds() => _itemAccepts.CalculateInputColorIds(this);

    public System.Windows.Forms.Control CreateControl(ConnectedFormulaView parent, string mode) {
        var con = new Monitor();
        con.DoDefaultSettings(parent, this, mode);

        return con;
    }

    public override string ErrorReason() {
        var b = base.ErrorReason();
        if (!string.IsNullOrEmpty(b)) { return b; }

        b = _itemAccepts.ErrorReason(this);
        if (!string.IsNullOrEmpty(b)) { return b; }

        return string.Empty;
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> l =
        [
            .. _itemAccepts.GetProperties(this, widthOfControl),
            //new FlexiControlForProperty<string>(() => Pfad),
            //new FlexiControlForProperty<bool>(() => Bei_Bedarf_erzeugen),
            //new FlexiControlForProperty<bool>(() => Leere_Ordner_löschen),
            //new FlexiControl(),
            .. base.GetProperties(widthOfControl),
        ];
        return l;
    }

    public override void ParseFinished(string parsed) {
        base.ParseFinished(parsed);
        //_itemSends.ParseFinished(this);
        _itemAccepts.ParseFinished(this);
    }

    public override bool ParseThis(string key, string value) {
        if (base.ParseThis(key, value)) { return true; }
        if (_itemAccepts.ParseThis(key, value)) { return true; }

        switch (key) {
            //case "pfad":
            //    _pfad = value.FromNonCritical();
            //    return true;

            //case "createdir":
            //    _bei_Bedarf_Erzeugen = value.FromPlusMinus();
            //    return true;

            //case "deletedir":
            //    _leere_Ordner_Löschen = value.FromPlusMinus();
            //    return true;
        }
        return false;
    }

    public override string ReadableText() {
        const string txt = "Monitor: ";

        if (this.IsOk() && DatabaseInput != null) {
            return txt + DatabaseInput.Caption;
        }

        return txt + ErrorReason();
    }

    public override QuickImage SymbolForReadableText() {
        if (this.IsOk()) {
            return QuickImage.Get(ImageCode.Ordner, 16, Color.Transparent, Skin.IdColor(InputColorId));
        }

        return QuickImage.Get(ImageCode.Textdatei, 16);
    }

    public override string ToParseableString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = [.. _itemAccepts.ParsableTags()];

        //result.ParseableAdd("Pfad", _pfad);
        //result.ParseableAdd("CreateDir", _bei_Bedarf_Erzeugen);
        //result.ParseableAdd("DeleteDir", _leere_Ordner_Löschen);
        return result.Parseable(base.ToParseableString());
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        //var id = GetRowFrom?.OutputColorId ?? -1;

        if (!forPrinting) {
            DrawColorScheme(gr, positionModified, zoom, InputColorId, true, true, false);
        }

        //DrawFakeControl(gr, positionModified, zoom, CaptionPosition.Über_dem_Feld, "Monitor", EditTypeFormula.Listbox);

        if (!forPrinting) {
            DrawColorScheme(gr, positionModified, zoom, InputColorId, true, true, true);
        }

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
        DrawArrorInput(gr, positionModified, zoom, forPrinting, InputColorId);
    }

    #endregion
}