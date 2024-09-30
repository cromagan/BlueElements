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
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Extended_Text;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.Abstract;
using BlueScript.Variables;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using BlueControls.ItemCollectionList;

using static BlueBasics.Converter;

namespace BlueControls.ItemCollectionPad;

public class TextPadItem : RectanglePadItem, ICanHaveVariables {

    #region Fields

    private Alignment _ausrichtung;

    /// <summary>
    /// Der Original-Text. Bei änderungen deses Textes wird die Variable _text_replaced ebenfalls zurückgesetzt.
    /// </summary>
    private string _textOriginal;

    /// <summary>
    /// Kopie von _text_original - aber mit evtl. ersetzten Variablen
    /// </summary>
    private string _textReplaced;

    /// <summary>
    /// Dieses Element ist nur temporär und ist der tatsächlich angezeigte Text - mit Bildern, verschieden Größen, etc.
    /// Wird immer von _text_replaced abgeleitet.
    /// </summary>
    private ExtText? _txt;

    #endregion

    #region Constructors

    public TextPadItem() : this(string.Empty, string.Empty) { }

    public TextPadItem(string keyName, string readableText) : base(keyName) {
        _textReplaced = readableText;
        _textOriginal = readableText;
        _ausrichtung = Alignment.Top_Left;
        InvalidateText();
    }

    #endregion

    #region Properties

    public static string ClassId => "TEXT";

    public Alignment Ausrichtung {
        get => _ausrichtung;
        set {
            if (IsDisposed) { return; }
            if (value == _ausrichtung) { return; }
            _ausrichtung = value;
            InvalidateText();
            OnPropertyChanged();
        }
    }

    public override string Description => string.Empty;

    public override string MyClassId => ClassId;

    //http://www.kurztutorial.info/programme/punkt-mm/rechner.html
    // Dim Ausgleich As float = MmToPixel(1 / 72 * 25.4, ItemCollectionPad.Dpi)
    public float Skalierung { get; set; } = 3.07f;

    /// <summary>
    ///
    /// </summary>
    [Description("Text der angezeigt werden soll.<br>Alternativ kann ein (oder mehrere) Variablenname im Format ~Name~ angegeben werden.")]
    public string Text {
        get => _textOriginal;
        set {
            if (IsDisposed) { return; }
            if (value == _textOriginal) { return; }
            _textOriginal = value;
            _textReplaced = value;
            InvalidateText();
            SizeChanged();
            OnPropertyChanged();
        }
    }

    protected override int SaveOrder => 999;

    #endregion

    #region Methods

    public override List<GenericControl> GetProperties(int widthOfControl) {
        var aursicht = new List<AbstractListItem>();
        aursicht.Add(ItemOf("Linksbündig ausrichten", ((int)Alignment.Top_Left).ToString(), ImageCode.Linksbündig));
        aursicht.Add(ItemOf("Zentrieren", ((int)Alignment.Top_HorizontalCenter).ToString(), ImageCode.Zentrieren));
        aursicht.Add(ItemOf("Rechtsbündig ausrichten", ((int)Alignment.Top_Right).ToString(), ImageCode.Rechtsbündig));

        List<GenericControl> result =
        [
            new FlexiControlForProperty<string>(() => Text, 5)
        ];
        result.Add(new FlexiControlForProperty<Alignment>(() => Ausrichtung, aursicht));
        result.Add(new FlexiControlForProperty<float>(() => Skalierung));
        result.Add(new FlexiControlForProperty<PadStyles>(() => Stil, Skin.GetRahmenArt(Parent?.SheetStyle, true)));
        result.AddRange(base.GetProperties(widthOfControl));
        return result;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("ReadableText", _textOriginal);
        result.ParseableAdd("Alignment", _ausrichtung);
        result.ParseableAdd("AdditionalScale", Skalierung);
        return result;
    }

    public override void ParseFinished(string parsed) {
        base.ParseFinished(parsed);
        InvalidateText();
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "readabletext":
                _textReplaced = value.FromNonCritical();
                _textOriginal = _textReplaced;
                return true;

            case "alignment":
                _ausrichtung = (Alignment)byte.Parse(value);
                return true;

            //case "format":
            //    Format = (DataFormat)IntParse(value);
            //    return true;

            case "additionalscale":
                Skalierung = FloatParse(value.FromNonCritical());
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override void ProcessStyleChange() => InvalidateText();

    public override string ReadableText() => "Text";

    /// <summary>
    /// Löst die angegebene Variable in _text_replaced auf, falls diese (noch) vorhanden ist.
    /// </summary>
    /// <param name="variable"></param>
    /// <returns></returns>
    public bool ReplaceVariable(Variable variable) {
        if (IsDisposed) { return false; }
        var nt = variable.ReplaceInText(_textReplaced);

        if (nt == _textReplaced) { return false; }
        _textReplaced = nt;
        InvalidateText();
        OnPropertyChanged();
        return true;
    }

    public bool ResetVariables() {
        if (IsDisposed) { return false; }
        if (_textOriginal == _textReplaced) { return false; }
        _textReplaced = _textOriginal;
        InvalidateText();
        OnPropertyChanged();
        return true;
    }

    public override void SizeChanged() {
        base.SizeChanged();
        InvalidateText();
    }

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Textfeld2, 16);

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        if (Stil != PadStyles.Undefiniert) {
            gr.SetClip(positionModified);
            var trp = positionModified.PointOf(Alignment.Horizontal_Vertical_Center);
            gr.TranslateTransform(trp.X, trp.Y);
            gr.RotateTransform(-Drehwinkel);

            if (_txt == null) { MakeNewETxt(); }

            if (_txt != null && Parent != null) {
                _txt.DrawingPos = new Point((int)(positionModified.Left - trp.X), (int)(positionModified.Top - trp.Y));
                _txt.DrawingArea = Rectangle.Empty; // new Rectangle(drawingCoordinates.Left, drawingCoordinates.Top, drawingCoordinates.Width, drawingCoordinates.Height);
                if (!string.IsNullOrEmpty(_textReplaced) || !forPrinting) {
                    _txt.Draw(gr, zoom * Skalierung * Parent.SheetStyleScale);
                }
            }
            gr.TranslateTransform(-trp.X, -trp.Y);
            gr.ResetTransform();
            gr.ResetClip();
        }

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
    }

    private void InvalidateText() => _txt = null;

    private void MakeNewETxt() {
        _txt = null;
        if (Stil != PadStyles.Undefiniert) {
            if (Parent == null) {
                Develop.DebugPrint(FehlerArt.Fehler, "Parent is Nothing, wurde das Objekt zu einer Collection hinzugefügt?");
                return;
            }

            _txt = new ExtText(Stil, Parent.SheetStyle);
            _txt.HtmlText = !string.IsNullOrEmpty(_textReplaced) ? _textReplaced : "{Text}";
            //// da die Font 1:1 berechnet wird, aber bei der Ausgabe evtl. skaliert,
            //// muss etxt vorgegaukelt werden, daß der Drawberehich xxx% größer ist
            //etxt.DrawingArea = new Rectangle((int)UsedArea().Left, (int)UsedArea().Top, (int)(UsedArea().Width / AdditionalScale / Parent.SheetStyleScale), -1);
            //etxt.LineBreakWidth = etxt.DrawingArea.Width;
            _txt.TextDimensions = new Size((int)(UsedArea.Width / Skalierung / Parent.SheetStyleScale), -1);
            _txt.Ausrichtung = _ausrichtung;
        }
    }

    #endregion
}