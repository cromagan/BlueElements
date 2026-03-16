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
using BlueBasics.Enums;
using BlueControls.BlueTableDialogs;
using BlueControls.CellRenderer;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.Abstract;
using BlueScript;
using BlueScript.Enums;
using BlueScript.Methods;
using BlueScript.Structures;
using BlueScript.Variables;
using BlueTable.Enums;
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.Converter;

namespace BlueControls.ItemCollectionPad;

public class DynamicSymbolPadItem : RectanglePadItem, IStyleableOne {

    #region Fields

    private string _script = string.Empty;

    private PadStyles _style = PadStyles.Standard;

    #endregion

    #region Constructors

    public DynamicSymbolPadItem() : base(string.Empty) {
        //Symbol = Symbol.Pfeil;
        //Hintergrundfarbe = Color.White;
        //Randfarbe = Color.Black;
        //Randdicke = 1;
    }

    #endregion

    #region Properties

    public static string ClassId => "DynamicSymbol";

    public override string Description => string.Empty;

    public BlueFont? Font { get; set; }

    public string Script {
        get => _script;

        set {
            if (value == _script) { return; }
            _script = value;
            OnPropertyChanged();
        }
    }

    public string SheetStyle => Parent is IStyleable ist ? ist.SheetStyle : string.Empty;

    public PadStyles Style {
        get => _style;
        set {
            if (_style == value) { return; }
            _style = value;
            this.InvalidateFont();
            OnPropertyChanged();
        }
    }

    protected override int SaveOrder => 999;

    #endregion

    #region Methods

    public static ScriptEndedFeedback ExecuteScript(string scripttext, string mode, Bitmap bmp) {
        //var generatedentityID = rowIn.ReplaceVariables(entitiId, true, null);

        VariableCollection vars =
        [
            new VariableString("Application", Develop.AppName(), true, "Der Name der App, die gerade geöffnet ist."),
            new VariableString("User", Generic.UserName, true,
                "ACHTUNG: Keinesfalls dürfen benutzerabhängig Werte verändert werden."),

            new VariableString("Usergroup", Generic.UserGroup, true,
                "ACHTUNG: Keinesfalls dürfen gruppenabhängig Werte verändert werden."),
            new VariableString("Mode", mode, true, "In welchem Modus die Formulare angezeigt werden."),
            //vars.Add(new VariableListString("Menu", null, false, "Diese Variable muss das Rückgabemenü enthalten."));
            //vars.Add(new VariableListString("Infos", null, false, "Diese Variable kann Zusatzinfos zum Menu enthalten."));
            ////vars.Add(new VariableListString("CurrentlySelected", selected, true, "Was der Benutzer aktuell angeklickt hat."));
            //vars.Add(new VariableString("EntityId", generatedentityID, true, "Dies ist die Eingangsvariable."));
            //vars.Add(new VariableDouble("Width", (float)Math.Round(PixelToMm(bmp.Width, ItemCollectionPadItem.Dpi), 2, MidpointRounding.AwayFromZero), true, "Breite des Objekts in mm"));
            //vars.Add(new VariableDouble("Height", (float)Math.Round(PixelToMm(bmp.Height, ItemCollectionPadItem.Dpi), 2, MidpointRounding.AwayFromZero), true, "Höhe des Objekts in mm"));
            ////vars.Add(new VariableDouble("Scale", (float)Math.Round(scale, 2, MidpointRounding.AwayFromZero)), true, "Anzwuen"));
            new VariableDouble("Width", bmp.Width, true, "Breite des Objekts in Pixel"),
            new VariableDouble("Height", bmp.Height, true, "Höhe des Objekts in Pixel"),
            new VariableBitmap("BMP", bmp, true, "Das Objekt,auf dem gezeichnet werden kann")
        ];

        var m = Method.GetMethods(MethodType.Standard);

        //using var gr = Graphics.FromImage(bmp);

        var scp = new ScriptProperties("DynamicSymbol", m, true, [], bmp, "DynamicSymbol", "DynamicSymbol");

        var sc = new Script(vars, scp) {
            ScriptText = scripttext
        };
        return sc.Parse(0, "Main", null, null);
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [new FlexiControlForDelegate(Skript_Bearbeiten, "Skript bearbeiten", ImageCode.Skript),
        .. base.GetProperties(widthOfControl),
        ];
        return result;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("Style", _style);
        result.ParseableAdd("Script", _script);
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "script":
                _script = value.FromNonCritical();
                return true;

            case "style":
                _style = (PadStyles)IntParse(value);
                _style = Skin.RepairStyle(_style);
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() => "Dynamisches Symbol";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Formel, 16);

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        UnRegisterEvents();
    }

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float zoom, float offsetX, float offsetY, bool forPrinting) => Renderer_DynamicSymbol.Method.Draw(gr, _script, null, positionControl.ToRect(), TranslationType.Original_Anzeigen, Alignment.Left, zoom);//var trp = positionControl.PointOf(Alignment.Horizontal_Vertical_Center);//gr.TranslateTransform(trp.ControlX, trp.Y);//gr.RotateTransform(-Drehwinkel);//GraphicsPath? p = null;//// Wegen der Nullpunktverschiebung wird ein temporäres Rechteck benötigt//var d2 = positionControl;//d2.ControlX = -positionControl.Width / 2;//d2.Y = -positionControl.Height / 2;//switch (Symbol) {//    case Symbol.Ohne://        break;//    case Symbol.Pfeil://        p = Poly_Arrow(d2.ToRect());//        break;//    case Symbol.Bruchlinie://        p = Poly_Bruchlinie(d2.ToRect());//        break;//    case Symbol.Rechteck://        p = Poly_Rechteck(d2.ToRect());//        break;//    case Symbol.Rechteck_gerundet://        p = Poly_RoundRec(d2.ToRect(), (int)(20 * zoom));//        break;//    default://        Develop.DebugPrint(Symbol);//        break;//}//if (p != null && Parent != null) {//    gr.FillPath(new SolidBrush(Hintergrundfarbe), p);//    gr.DrawPath(new Pen(Randfarbe, Randdicke * zoom * SheetStyleScale), p);//}//gr.TranslateTransform(-trp.ControlX, -trp.Y);//gr.ResetTransform();

    protected override void OnParentChanged() {
        base.OnParentChanged();
        this.InvalidateFont();
        if (Parent is ItemCollectionPadItem icpi) {
            icpi.StyleChanged += Icpi_StyleChanged;
        }
    }

    protected override void OnParentChanging() {
        base.OnParentChanging();
        UnRegisterEvents();
    }

    private void Icpi_StyleChanged(object sender, System.EventArgs e) => this.InvalidateFont();

    private void Skript_Bearbeiten() => IUniqueWindowExtension.ShowOrCreate<DynamicSymbolScriptEditor>(this);

    private void UnRegisterEvents() {
        if (Parent is ItemCollectionPadItem icpi) {
            icpi.StyleChanged -= Icpi_StyleChanged;
        }
    }

    #endregion
}