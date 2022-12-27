// Authors:
// Christian Peter
//
// Copyright (con) 2022 Christian Peter
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
using BlueControls.Interfaces;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using static BlueBasics.Converter;

namespace BlueControls.ItemCollection;

public class VariableFieldPadItem : CustomizableShowPadItem, IReadableText, IAcceptAndSends, IContentHolder, IItemToControl {

    #region Fields

    private EditTypeFormula _bearbeitung = EditTypeFormula.Textfeld;
    private string _überschrift = string.Empty;
    private ÜberschriftAnordnung _überschriftanordung = ÜberschriftAnordnung.Über_dem_Feld;
    private string _variable = string.Empty;

    #endregion

    #region Constructors

    public VariableFieldPadItem(string internalname) : base(internalname) { }

    #endregion

    #region Properties

    public ÜberschriftAnordnung CaptionPosition {
        get => _überschriftanordung;
        set {
            if (_überschriftanordung == value) { return; }
            _überschriftanordung = value;
            OnChanged();
            RaiseVersion();
        }
    }

    public string Überschrift {
        get => _überschrift;
        set {
            if (_überschrift == value) { return; }
            _überschrift = value;
            OnChanged();
            RaiseVersion();
        }
    }

    [Description("Es können Variablen aus dem Skript benutzt werden.\r\nDiese müssen im Format ~variable~ angegeben werden.")]
    public string Variable {
        get => _variable;
        set {
            if (_variable == value) { return; }
            _variable = value;
            OnChanged();
            RaiseVersion();
        }
    }

    #endregion

    #region Methods

    public override System.Windows.Forms.Control CreateControl(ConnectedFormulaView parent) {
        var con = new FlexiControl();
        con.Caption = Überschrift + ":";
        con.EditType = _bearbeitung;
        con.CaptionPosition = CaptionPosition;
        con.Name = DefaultItemToControlName();
        con.OriginalText = _variable;
        if (GetRowFrom is ICalculateRowsItemLevel rfw2) {
            var ff = parent.SearchOrGenerate(rfw2);

            if (ff is ICalculateRowsControlLevel cc) { cc.Childs.Add(con); }
            con.DisabledReason = "Dieser Wert ist nur eine Anzeige.";
        } else {
            con.DisabledReason = "Keine gültige Verknüpfung";
        }

        return con;
    }

    public override List<GenericControl> GetStyleOptions() {
        List<GenericControl> l = new();
        l.AddRange(base.GetStyleOptions());

        l.Add(new FlexiControlForProperty<string>(() => Überschrift));
        l.Add(new FlexiControlForProperty<string>(() => Variable));

        var u = new ItemCollection.ItemCollectionList.ItemCollectionList();
        u.AddRange(typeof(ÜberschriftAnordnung));
        l.Add(new FlexiControlForProperty<ÜberschriftAnordnung>(() => CaptionPosition, u));
        //var b = new ItemCollection.ItemCollectionList.ItemCollectionList();
        //b.AddRange(typeof(EditTypeFormula));
        //l.GenerateAndAdd(new FlexiControlForProperty<EditTypeFormula>(() => EditType, b));
        l.Add(new FlexiControl());

        return l;
    }

    public bool IsRecursiveWith(IAcceptAndSends obj) {
        if (obj == this) { return true; }

        if (GetRowFrom is IAcceptAndSends i) { return i.IsRecursiveWith(obj); }
        return false;
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }

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

    public string ReadableText() => "Wert aus: " + Überschrift;

    public QuickImage? SymbolForReadableText() {
        if (GetRowFrom == null) { return null; }

        return QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, Skin.IDColor(GetRowFrom.Id));
    }

    public override string ToString() {
        var t = base.ToString();
        t = t.Substring(0, t.Length - 1) + ", ";

        t = t + "CaptionText=" + _überschrift.ToNonCritical() + ", ";
        t = t + "Variable=" + _variable.ToNonCritical() + ", ";

        t = t + "EditType=" + ((int)_bearbeitung).ToString() + ", ";
        t = t + "Caption=" + ((int)_überschriftanordung).ToString() + ", ";

        return t.Trim(", ") + "}";
    }

    protected override string ClassId() => "FI-VariableField";

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        var id = -1;
        if (GetRowFrom != null) { id = GetRowFrom.Id; }

        if (!forPrinting) {
            DrawColorScheme(gr, positionModified, zoom, id);
        }

        if (GetRowFrom == null) {
            Skin.Draw_FormatedText(gr, "Datenquelle fehlt", QuickImage.Get(ImageCode.Warnung, (int)(16 * zoom)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), CaptionFnt.Scale(zoom), true);
        } else {
            DrawFakeControl(gr, positionModified, zoom, CaptionPosition, Überschrift);
        }

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
    }

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
    protected override BasicPadItem? TryCreate(string id, string name) {
        if (id.Equals(ClassId(), StringComparison.OrdinalIgnoreCase)) {
            return new VariableFieldPadItem(name);
        }
        return null;
    }

    #endregion

    // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    // ~BitmapPadItem()
    // {
    //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
    //     Dispose(disposing: false);
    // }
}