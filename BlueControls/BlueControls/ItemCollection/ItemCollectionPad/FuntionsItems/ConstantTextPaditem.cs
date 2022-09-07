// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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
using System;
using System.Collections.Generic;
using System.Drawing;
using BlueBasics.Interfaces;
using BlueControls.Interfaces;
using System.Windows.Forms;
using BlueDatabase.Enums;

namespace BlueControls.ItemCollection;

public class ConstantTextPaditem : FixedRectanglePadItem, IReadableText, IContentHolder, IItemToControl {

    #region Fields

    public static BlueFont? CellFont = Skin.GetBlueFont(Design.Table_Cell, States.Standard);
    public static BlueFont? ChapterFont = Skin.GetBlueFont(Design.Table_Cell_Chapter, States.Standard);
    public static BlueFont? ColumnFont = Skin.GetBlueFont(Design.Table_Column, States.Standard);

    private string _text = string.Empty;

    #endregion

    #region Constructors

    public ConstantTextPaditem() : this(UniqueInternal(), string.Empty) { }

    public ConstantTextPaditem(string intern, string text) : base(intern) {
        _text = text;
        Size = new Size(150, 24);
    }

    public ConstantTextPaditem(string intern) : this(intern, string.Empty) { }

    #endregion

    #region Properties

    public string Text {
        get => _text;
        set {
            if (_text == value) { return; }
            _text = value;
            OnChanged();
        }
    }

    protected override int SaveOrder => 999;

    #endregion

    #region Methods

    public Control? CreateControl(ConnectedFormulaView parent) {
        var c3 = new FlexiControl();
        c3.Width = 100;
        c3.Height = 16;

        c3.CaptionPosition = ÜberschriftAnordnung.ohne;

        c3.EditType = EditTypeFormula.Textfeld;
        c3.DisabledReason = "Konstanter Wert";

        c3.ValueSet(Text, true, true);
        c3.Tag = Internal;
        return c3;
    }

    public override List<GenericControl> GetStyleOptions() {
        List<GenericControl> l = new();
        l.Add(new FlexiControlForProperty<string>(() => Text));
        return l;
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }
        switch (tag) {
            case "text":
                Text = value.FromNonCritical();
                return true;
        }
        return false;
    }

    public string ReadableText() {
        return "Konstanter Wert: " + _text;
    }

    public QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Textfeld, 16);

    public override string ToString() {
        var t = base.ToString();
        t = t.Substring(0, t.Length - 1) + ", ";
        t = t + "Text=" + _text.ToNonCritical() + ", ";
        return t.Trim(", ") + "}";
    }

    protected override string ClassId() => "FI-ConstantText";

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        gr.DrawRectangle(new Pen(Color.Black, zoom), positionModified);

        var t = string.Empty;
        t = t + _text;

        Skin.Draw_FormatedText(gr, t, QuickImage.Get(ImageCode.Textfeld, (int)(zoom * 16)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), ColumnPadItem.ColumnFont.Scale(zoom), false);

        gr.FillRectangle(new SolidBrush(Color.FromArgb(128, 255, 255, 255)), positionModified);

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
    }

    protected override BasicPadItem? TryCreate(string id, string name) {
        if (id.Equals(ClassId(), StringComparison.OrdinalIgnoreCase)) {
            return new ConstantTextPaditem(name);
        }
        return null;
    }

    #endregion
}