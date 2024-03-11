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
using BlueControls.Enums;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueDatabase;
using BlueDatabase.Enums;
using MessageBox = BlueControls.Forms.MessageBox;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

public class TextGeneratorPadItem : FakeControlPadItem, IItemToControl, IItemAcceptFilter, IAutosizable {

    #region Fields

    private readonly ItemAcceptFilter _itemAccepts;

    private string _auswahl1;

    private string _auswahl2;

    private string _auswahl3;

    private string _textc;

    #endregion

    #region Constructors

    public TextGeneratorPadItem(string internalname) : base(internalname) {
        _itemAccepts = new();
        SetCoordinates(new RectangleF(0, 0, 50, 30), true);
    }

    #endregion

    #region Properties

    public static string ClassId => "FI-TextGeneratorPadItem";
    public AllowedInputFilter AllowedInputFilter => AllowedInputFilter.None | AllowedInputFilter.More;
    public bool AutoSizeableHeight => true;
    public Database? DatabaseInput => _itemAccepts.DatabaseInput(this);
    public bool DatabaseInputMustMatchOutputDatabase => false;
    public override string Description => "Dieses Element erzeugt einen Text-Generator-Baustein";
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

    public override void AddedToCollection() {
        base.AddedToCollection();
        //_itemSends.DoCreativePadAddedToCollection(this);
        _itemAccepts.DoCreativePadAddedToCollection(this);
    }

    [Description("Diese Inhalte dieser Spalte werden als Auswahlmenü der ersten Stufe angezeigt")]
    public void Auswahl_Spalte_1_wählen() => ChooseColumn(1, ref _auswahl1);

    [Description("Diese Inhalte dieser Spalte werden als Auswahlmenü der zweiten Stufe angezeigt")]
    public void Auswahl_Spalte_2_wählen() => ChooseColumn(2, ref _auswahl2);

    [Description("Diese Inhalte dieser Spalte werden als Auswahlmenü der dritte Stufe angezeigt")]
    public void Auswahl_Spalte_3_wählen() => ChooseColumn(4, ref _auswahl3);

    public void CalculateInputColorIds() => _itemAccepts.CalculateInputColorIds(this);

    public override System.Windows.Forms.Control CreateControl(ConnectedFormulaView parent) {
        var con = new TextGenerator {
            Text_Spalte = _textc,
            AuswahlSpalte1 = _auswahl1,
            AuswahlSpalte2 = _auswahl2,
            AuswahlSpalte3 = _auswahl3
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
            //l.Add(new FlexiControlForProperty<string>(() => Bild_Dateiname));
            new FlexiControlForDelegate(Text_Spalte_wählen, "Text-Spalte wählen", ImageCode.Pfeil_Rechts),
            new FlexiControlForDelegate(Auswahl_Spalte_1_wählen, "Auswahl-Spalte 1 wählen", ImageCode.Pfeil_Rechts),
            new FlexiControlForDelegate(Auswahl_Spalte_2_wählen, "Auswahl-Spalte 2 wählen", ImageCode.Pfeil_Rechts),
            new FlexiControlForDelegate(Auswahl_Spalte_3_wählen, "Auswahl-Spalte 3 wählen", ImageCode.Pfeil_Rechts),
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
            case "column text":
                _textc = value;
                return true;

            case "column menu 1":
                _auswahl1 = value;
                return true;

            case "column menu 2":
                _auswahl2 = value;
                return true;

            case "column menu 3":
                _auswahl3 = value;
                return true;
        }
        return false;
    }

    public override string ReadableText() {
        const string txt = "Text-Generator: ";

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

    [Description("Aus dieser Spalte wird der Text erzeugt.")]
    public void Text_Spalte_wählen() => ChooseColumn(0, ref _textc);

    public override string ToString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = [.. _itemAccepts.ParsableTags()];

        result.ParseableAdd("Column Text", _textc);
        result.ParseableAdd("Column Menu 1", _auswahl1);
        result.ParseableAdd("Column Menu 2", _auswahl2);
        result.ParseableAdd("Column Menu 3", _auswahl3);
        return result.Parseable(base.ToString());
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

    private void ChooseColumn(int no, ref string col) {
        if (IsDisposed) { return; }

        if (DatabaseInput is not Database db || db.IsDisposed) {
            MessageBox.Show("Quelle fehlerhaft!");
            return;
        }

        var lst = new ItemCollectionList.ItemCollectionList(true);
        lst.AddRange(db.Column, false);

        lst.Remove(_textc);
        lst.Remove(_auswahl1);
        lst.Remove(_auswahl2);
        lst.Remove(_auswahl3);

        var sho = InputBoxListBoxStyle.Show("Spalte wählen:", lst, CheckBehavior.SingleSelection, null, AddType.None);

        if (sho == null || sho.Count != 1) { return; }

        var col2 = (db.Column[sho[0]]?.KeyName ?? string.Empty).ToUpper();

        if (col == col2) { return; }
        col = col2;

        UpdateSideOptionMenu();
        OnChanged();
    }

    #endregion
}