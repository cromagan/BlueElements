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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Extended_Text;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad.Abstract;
using BlueScript.Variables;
using static BlueBasics.Converter;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.ItemCollectionPad;

public class TextPadItem : RectanglePadItem, ICanHaveVariables, IStyleableOne, IStyleableChild {

    #region Fields

    private Alignment _ausrichtung;
    private PadStyles _style = PadStyles.Standard;

    /// <summary>
    /// Der Original-Text. Bei änderungen deses Textes wird die Variable _text_replaced ebenfalls zurückgesetzt.
    /// </summary>
    private string _textOriginal;

    /// <summary>
    /// Kopie von _text_original - aber mit evtl. ersetzten Variablen
    /// </summary>
    private string _textReplaced;

    private float _textScale = 3.07f;

    /// <summary>
    /// Dieses Element ist nur temporär und ist der tatsächlich angezeigte Text - mit Bildern, verschieden Größen, etc.
    /// Wird immer von _text_replaced abgeleitet.
    /// </summary>
    private ExtText? _txt;

    #endregion

    #region Constructors

    public TextPadItem() : this(string.Empty, string.Empty) { }

    public TextPadItem(string keyName, string visibleText) : base(keyName) {
        _textReplaced = visibleText;
        _textOriginal = visibleText;
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
    public BlueFont? Font { get; set; }

    public string SheetStyle {
        get {
            if (_parent is IStyleable ist) { return ist.SheetStyle; }
            return string.Empty;
        }
    }

    public PadStyles Stil {
        get => _style;
        set {
            if (_style == value) { return; }
            _style = value;
            InvalidateText();
            OnPropertyChanged();
        }
    }

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
            //CalculateSlavePoints();
            OnPropertyChanged();
        }
    }

    public float TextScale {
        get => _textScale;
        set {
            value = Math.Max(value, 0.01f);
            value = Math.Min(value, 20);
            if (Math.Abs(value - _textScale) < Constants.DefaultTolerance) { return; }
            _textScale = value;
            OnPropertyChanged();
        }
    }

    protected override int SaveOrder => 999;

    #endregion

    #region Methods

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<AbstractListItem> aursicht =
        [
            ItemOf("Linksbündig ausrichten", ((int)Alignment.Top_Left).ToString(), ImageCode.Linksbündig),
            ItemOf("Zentrieren", ((int)Alignment.Top_HorizontalCenter).ToString(), ImageCode.Zentrieren),
            ItemOf("Rechtsbündig ausrichten", ((int)Alignment.Top_Right).ToString(), ImageCode.Rechtsbündig)
        ];

        List<GenericControl> result =
        [
            new FlexiControlForProperty<string>(() => Text, 5),
            new FlexiControlForProperty<Alignment>(() => Ausrichtung, aursicht),
            new FlexiControlForProperty<float>(() => TextScale),
            new FlexiControlForProperty<PadStyles>(() => Stil, Skin.GetRahmenArt(SheetStyle, true))
        ];
        result.AddRange(base.GetProperties(widthOfControl));
        return result;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("ReadableText", _textOriginal);
        result.ParseableAdd("Alignment", _ausrichtung);
        result.ParseableAdd("AdditionalScale", _textScale);
        result.ParseableAdd("Style", _style);
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

            case "style":
                _style = (PadStyles)IntParse(value);
                return true;

            case "additionalscale":
                _textScale = FloatParse(value.FromNonCritical());
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override void PointMoved(object sender, MoveEventArgs e) {
        base.PointMoved(sender, e);
        InvalidateText();
    }

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

    //public override void CalculateSlavePoints() {
    //    base.CalculateSlavePoints();
    //    InvalidateText();
    //}

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Textfeld2, 16);

    protected override void DrawExplicit(Graphics gr, Rectangle visibleArea, RectangleF positionModified, float scale, float shiftX, float shiftY) {
        if (_style != PadStyles.Undefiniert) {
            gr.SetClip(positionModified);
            var trp = positionModified.PointOf(Alignment.Horizontal_Vertical_Center);
            gr.TranslateTransform(trp.X, trp.Y);
            gr.RotateTransform(-Drehwinkel);

            if (_txt == null) { MakeNewETxt(); }

            if (_txt != null && Parent != null) {
                _txt.DrawingPos = new Point((int)(positionModified.Left - trp.X), (int)(positionModified.Top - trp.Y));
                _txt.DrawingArea = Rectangle.Empty; // new Rectangle(drawingCoordinates.Left, drawingCoordinates.Top, drawingCoordinates.Width, drawingCoordinates.Height);
                if (!string.IsNullOrEmpty(_textReplaced) || !ForPrinting) {
                    _txt.Draw(gr, scale * _textScale);
                }
            }
            gr.TranslateTransform(-trp.X, -trp.Y);
            gr.ResetTransform();
            gr.ResetClip();
        }
    }

    private void InvalidateText() {
        this.InvalidateFont();
        _txt = null;
    }

    private void MakeNewETxt() {
        _txt = null;
        if (_style != PadStyles.Undefiniert) {
            if (Parent == null) {
                Develop.DebugPrint(FehlerArt.Fehler, "Parent is Nothing, wurde das Objekt zu einer Collection hinzugefügt?");
                return;
            }

            _txt = new ExtText(SheetStyle);
            _txt.HtmlText = !string.IsNullOrEmpty(_textReplaced) ? _textReplaced : "{Text}";
            //// da die Font 1:1 berechnet wird, aber bei der Ausgabe evtl. skaliert,
            //// muss etxt vorgegaukelt werden, daß der Drawberehich xxx% größer ist
            //etxt.DrawingArea = new Rectangle((int)UsedArea().Left, (int)UsedArea().Top, (int)(UsedArea().Width / AdditionalScale / SheetStyleScale), -1);
            //etxt.LineBreakWidth = etxt.DrawingArea.Width;
            _txt.TextDimensions = new Size((int)(UsedArea.Width / _textScale), -1);
            _txt.Ausrichtung = _ausrichtung;
        }
    }

    #endregion
}