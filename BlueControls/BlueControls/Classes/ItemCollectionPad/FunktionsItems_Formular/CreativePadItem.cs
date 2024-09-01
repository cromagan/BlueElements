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
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

public class CreativePadItem : ReciverControlPadItem, IItemToControl, IAutosizable {

    #region Fields

    private string _formular = string.Empty;

    #endregion

    #region Constructors

    public CreativePadItem(string keyName) : this(keyName, null) { }

    public CreativePadItem(string keyName, ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-CreativePad";
    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.One;
    public bool AutoSizeableHeight => true;

    public override bool DatabaseInputMustMatchOutputDatabase => false;

    public override string Description => "Ein Steuerelement, das ein generirtes optisches Dokument anzeigt.";

    public string Formular {
        get => _formular;
        set {
            if (IsDisposed) { return; }
            if (_formular == value) { return; }
            _formular = value;
            OnPropertyChanged();
        }
    }

    public override bool InputMustBeOneRow => true;
    public override bool MustBeInDrawingArea => true;
    public override string MyClassId => ClassId;

    protected override int SaveOrder => 1000;

    #endregion

    #region Methods

    public System.Windows.Forms.Control CreateControl(ConnectedFormulaView parent, string mode) {
        CreativePad? con;

        if (_formular.EndsWith(".br", StringComparison.OrdinalIgnoreCase)) {
            con = new CreativePad(new ItemCollectionPad(_formular));
        } else {
            con = new CreativePad();
        }

        con.DoDefaultSettings(parent, this, mode);

        return con;
    }

    public override string ErrorReason() {
        if (string.IsNullOrEmpty(_formular)) {
            return "Kein Formular gewählt.";
        }

        return base.ErrorReason();
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        var cl = new List<AbstractListItem>();

        if (DatabaseInput is { IsDisposed: false } db) {
            if (Directory.Exists(db.AdditionalFilesPfad)) {
                var f = Directory.GetFiles(db.AdditionalFilesPfad, "*.bcr");

                cl.AddRange(ItemsOf(f));
            }
        }

        List<GenericControl> result =
            [.. base.GetProperties(widthOfControl),
                new FlexiControl("Einstellungen:", widthOfControl, true),
                new FlexiControlForProperty<string>(() => Formular, cl),
            ];

        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "formula":
                _formular = value.FromNonCritical();
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() {
        const string txt = "Formular: ";

        if (this.IsOk()) {
            return txt + _formular;
        }

        return txt + ErrorReason();
    }

    public override QuickImage SymbolForReadableText() {
        if (this.IsOk()) {
            return QuickImage.Get(ImageCode.Registersammlung, 16, Color.Transparent, Skin.IdColor(InputColorId));
        }

        return QuickImage.Get(ImageCode.Warnung, 16);
    }

    public override string ToParseableString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = [];
        result.ParseableAdd("Formula", _formular);
        return result.Parseable(base.ToParseableString());
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        DrawColorScheme(gr, positionModified, zoom, null, false, false, false);
        //var headh = 25 * zoom;
        //var headb = 70 * zoom;

        //var body = positionModified with { Y = positionModified.Y + headh, Height = positionModified.Height - headh };
        //var c = -1;
        //foreach (var thisC in _childs) {
        //    c++;
        //    var it = new RectangleF(positionModified.X + (c * headb), positionModified.Y, headb, headh);

        //    gr.FillRectangle(new SolidBrush(Color.FromArgb(255, 200, 200, 200)), it);

        //    Skin.Draw_FormatedText(gr, thisC.FileNameWithoutSuffix(), null, Alignment.Horizontal_Vertical_Center, it.ToRect(), ColumnFont?.Scale(zoom), false);
        //    gr.DrawRectangle(new Pen(Color.Black, zoom), it);
        //}

        //gr.FillRectangle(new SolidBrush(Color.FromArgb(255, 200, 200, 200)), body);
        //gr.DrawRectangle(new Pen(Color.Black, zoom), body);

        ////Skin.Draw_FormatedText(gr, _text, QuickImage.Get(ImageCode.Textfeld, (int)(zoom * 16)), Alignment.Horizontal_Vertical_Center, positionModified.ToRect(), ColumnPadItem.ColumnFont.Scale(zoom), false);
        ////Skin.Draw_FormatedText(gr, "Register-\r\nkarten", null, Alignment.Horizontal_Vertical_Center, body.ToRect(), ColumnFont?.Scale(zoom), false);

        //if (!forPrinting) {
        //    DrawColorScheme(gr, positionModified, zoom, InputColorId, true, true, true);
        //}

        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);

        DrawArrorInput(gr, positionModified, zoom, forPrinting, InputColorId);
    }

    #endregion
}