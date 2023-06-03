// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Interfaces;
using BlueDatabase;
using BlueDatabase.Enums;
using static BlueBasics.Converter;

namespace BlueControls.ItemCollection;

/// <summary>
/// Erzeut ein Flexi-Controll, dass nur einen Wert anzeigen kann. z.B. eine Variable
/// </summary>

public class VariableFieldPadItem : FakeControlPadItem, IReadableText, IItemAcceptRow, IAutosizable {

    #region Fields

    private readonly ItemAcceptRow _itemAccepts;

    private EditTypeFormula _bearbeitung = EditTypeFormula.Textfeld;

    private string _überschrift = string.Empty;

    private ÜberschriftAnordnung _überschriftanordung = ÜberschriftAnordnung.Über_dem_Feld;

    private string _variable = string.Empty;

    #endregion

    #region Constructors

    public VariableFieldPadItem(string internalname) : base(internalname) => _itemAccepts = new();

    #endregion

    #region Properties

    public static string ClassId => "FI-VariableField";

    public bool AutoSizeableHeight {
        get {
            if (_bearbeitung == EditTypeFormula.nur_als_Text_anzeigen) {
                return UsedArea.Height > IAutosizableExtension.MinHeigthCaption;
            }

            if (_überschriftanordung is ÜberschriftAnordnung.Links_neben_Dem_Feld or
                ÜberschriftAnordnung.Ohne_mit_Abstand or
                ÜberschriftAnordnung.ohne) {
                return UsedArea.Height > IAutosizableExtension.MinHeigthTextBox;
            }

            return UsedArea.Height > IAutosizableExtension.MinHeigthCapAndBox;
        }
    }

    public ÜberschriftAnordnung CaptionPosition {
        get => _überschriftanordung;
        set {
            if (_überschriftanordung == value) { return; }
            _überschriftanordung = value;
            OnChanged();
            this.RaiseVersion();
        }
    }

    public override string Description => "Dieses Element erzeugt eine Steuerelement,\r\ndas eine vom Skript berechnete Variable anzeigen kann.";

    public IItemSendRow? GetRowFrom {
        get => _itemAccepts.GetRowFromGet(this);
        set => _itemAccepts.GetRowFromSet(value, this);
    }

    public List<int> InputColorId => _itemAccepts.InputColorIdGet(this);

    public DatabaseAbstract? InputDatabase => _itemAccepts.InputDatabase(this);

    public string Überschrift {
        get => _überschrift;
        set {
            if (_überschrift == value) { return; }
            _überschrift = value;
            OnChanged();
            this.RaiseVersion();
        }
    }

    [Description("Es können Variablen aus dem Skript benutzt werden.\r\nDiese müssen im Format ~variable~ angegeben werden.")]
    public string Variable {
        get => _variable;
        set {
            if (_variable == value) { return; }
            _variable = value;
            OnChanged();
            this.RaiseVersion();
        }
    }

    #endregion

    #region Methods

    public void CalculateInputColorIds() => _itemAccepts.CalculateInputColorIds(this);

    public override Control CreateControl(ConnectedFormulaView parent) {
        var con = new FlexiControlVariable {
            Caption = Überschrift + ":",
            EditType = _bearbeitung,
            CaptionPosition = CaptionPosition,
            OriginalText = _variable
        };

        con.DoInputSettings(parent, this);
        //con.DoOutputSettings(parent, this);
        return con;
    }

    public override string ErrorReason() {
        if (InputDatabase == null || InputDatabase.IsDisposed) {
            return "Quelle fehlt";
        }
        //if (OutputDatabase == null || OutputDatabase.IsDisposed) {
        //    return "Ziel fehlt";
        //}
        return string.Empty;
    }

    public override List<GenericControl> GetStyleOptions() {
        List<GenericControl> l = new();
        l.AddRange(_itemAccepts.GetStyleOptions(this));

        l.Add(new FlexiControlForProperty<string>(() => Überschrift));
        l.Add(new FlexiControlForProperty<string>(() => Variable));

        var u = new ItemCollectionList.ItemCollectionList(false);
        u.AddRange(typeof(ÜberschriftAnordnung));
        l.Add(new FlexiControlForProperty<ÜberschriftAnordnung>(() => CaptionPosition, u));
        //var b = new ItemCollection.ItemCollectionList.ItemCollectionList();
        //b.AddRange(typeof(EditTypeFormula));
        //l.GenerateAndAdd(new FlexiControlForProperty<EditTypeFormula>(() => EditType, b));
        l.Add(new FlexiControl());
        l.AddRange(base.GetStyleOptions());
        return l;
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }
        if (_itemAccepts.ParseThis(tag, value)) { return true; }

        switch (tag) {
            case "edittype":
                _bearbeitung = (EditTypeFormula)IntParse(value);
                return true;

            case "caption":
                _überschriftanordung = (ÜberschriftAnordnung)IntParse(value);
                return true;

            case "captiontext":
                _überschrift = value.FromNonCritical();
                return true;

            case "variable":
                _variable = value.FromNonCritical();
                return true;
        }
        return false;
    }

    public override string ReadableText() {
        var txt = "Variablenansicht: ";

        if (IsOk() && InputDatabase != null) {
            return txt + InputDatabase.Caption;
        }

        return txt + ErrorReason();
    }

    public override QuickImage? SymbolForReadableText() {
        if (IsOk()) {
            return QuickImage.Get(ImageCode.Variable, 16, Color.Transparent, Skin.IdColor(InputColorId));
        }

        return QuickImage.Get(ImageCode.Warnung, 16);
    }

    public override string ToString() {
        var result = new List<string>();

        result.AddRange(_itemAccepts.ParsableTags());

        result.ParseableAdd("CaptionText", _überschrift);
        result.ParseableAdd("Variable", _variable);
        result.ParseableAdd("EditType", _bearbeitung);
        result.ParseableAdd("Caption", _überschriftanordung);
        return result.Parseable(base.ToString());
    }

    internal override void AddedToCollection() {
        base.AddedToCollection();
        //_itemSends.DoCreativePadAddedToCollection(this);
        _itemAccepts.DoCreativePadAddedToCollection(this);
        //RepairConnections();
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        //var id = GetRowFrom?.OutputColorId ?? -1;

        if (!forPrinting) {
            DrawColorScheme(gr, positionModified, zoom, InputColorId, true, true, false);
        }

        if (GetRowFrom == null) {
            Skin.Draw_FormatedText(gr, "Datenquelle fehlt", QuickImage.Get(ImageCode.Warnung, (int)(16 * zoom)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), CaptionFnt.Scale(zoom), true);
        } else {
            DrawFakeControl(gr, positionModified, zoom, CaptionPosition, Überschrift);
        }

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);

        DrawArrorInput(gr, positionModified, zoom, shiftX, shiftY, forPrinting, "Zeile", InputColorId);
    }

    protected override void ParseFinished() {
        base.ParseFinished();
        //_itemSends.ParseFinished(this);
        _itemAccepts.ParseFinished(this);
    }

    #endregion

    //public bool ReplaceVariable(Variable variable) {
    //    if (string.IsNullOrEmpty(Platzhalter_Für_Layout)) { return false; }
    //    if ("~" + variable.Name.ToLower() + "~" != Platzhalter_Für_Layout.ToLower()) { return false; }
    //    if (variable is not VariableBitmap vbmp) { return false; }
    //    var ot = vbmp.ValueBitmap;
    //    if (ot is Bitmap bmp) {
    //        Bitmap = bmp;
    //        OnChanged();
    //        return true;
    //    }
    //protected override BasicPadItem? TryCreate(string id, string name) {
    //    if (id.Equals(ClassId, StringComparison.OrdinalIgnoreCase)) {
    //        return new VariableFieldPadItem(name);
    //    }
    //    return null;
    //}

    // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    // ~BitmapPadItem()
    // {
    //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
    //     Dispose(disposing: false);
    // }
}