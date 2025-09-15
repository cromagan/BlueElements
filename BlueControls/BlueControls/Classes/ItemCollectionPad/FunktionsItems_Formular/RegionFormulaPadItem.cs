// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static BlueBasics.Converter;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

/// <summary>
/// Erzeut ein Unter-Element von ConnectedFormulaView
/// </summary>
public class RegionFormulaPadItem : ReciverControlPadItem, IItemToControl, IAutosizable {

    #region Fields

    private GroupBoxStyle _borderStyle = GroupBoxStyle.Normal;
    private string _child = string.Empty;

    #endregion

    #region Constructors

    public RegionFormulaPadItem() : this(string.Empty, null) { }

    public RegionFormulaPadItem(string keyName, ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula) {
        if (ParentFormula != null) {
            ParentFormula.PropertyChanged += ParentFormula_PropertyChanged;
        }
    }

    #endregion

    #region Properties

    public static string ClassId => "FI-RegionFormula";
    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.None | AllowedInputFilter.More;
    public bool AutoSizeableHeight => true;

    public string Child {
        get => _child;
        set {
            if (IsDisposed) { return; }
            if (_child == value) { return; }
            _child = value;
            OnPropertyChanged();
        }
    }

    public override bool TableInputMustMatchOutputTable => false;
    public override string Description => "Ein Steuerelement, mit dem ein untergeordnetes Formular angezeigt werden kann.";

    public override bool InputMustBeOneRow => true;
    public override bool MustBeInDrawingArea => true;

    [DefaultValue(GroupBoxStyle.Normal)]
    public GroupBoxStyle RahmenStil {
        get => _borderStyle;
        set {
            if (_borderStyle == value) { return; }
            _borderStyle = value;
            OnPropertyChanged();
        }
    }

    protected override int SaveOrder => 1000;

    #endregion

    #region Methods

    public Control CreateControl(ConnectedFormulaView parent, string mode) {
        var icpi = GetChild(_child);

        var con = new ConnectedFormulaView(mode, icpi) {
            GroupBoxStyle = _borderStyle
        };

        if (_borderStyle != GroupBoxStyle.Nothing) {
            con.Text = icpi?.BestCaption() ?? "?";
        }

        con.DoDefaultSettings(parent, this, mode);

        return con;
    }

    public override string ErrorReason() {
        if (string.IsNullOrEmpty(_child)) {
            return "Keine Formular gewählt.";
        }

        return base.ErrorReason();
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        var cl = ParentFormula?.AllKnownChilds(ParentFormula.NotAllowedChilds);

        var u = new List<AbstractListItem>();
        u.AddRange(ItemsOf(typeof(GroupBoxStyle)));

        List<GenericControl> result =
            [.. base.GetProperties(widthOfControl),
                new FlexiControl("Einstellungen:", widthOfControl, true),
                new FlexiControlForProperty<string>(() => Child, cl),

                new FlexiControlForProperty<GroupBoxStyle>(() => RahmenStil, u)
            ];

        return result;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("Parent", ParentFormula?.Filename ?? string.Empty);
        result.ParseableAdd("Child", _child);
        result.ParseableAdd("BorderStyle", _borderStyle);
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "parent":
                ParentFormula = ConnectedFormula.ConnectedFormula.GetByFilename(value.FromNonCritical());
                if (ParentFormula != null) {
                    ParentFormula.PropertyChanged += ParentFormula_PropertyChanged;
                }
                return true;

            case "child":
                _child = value.FromNonCritical();
                return true;

            case "borderstyle":
                _borderStyle = (GroupBoxStyle)IntParse(value);
                return true;

            case "style":
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() {
        const string txt = "Unterformular: ";

        return txt + TableInput?.Caption;
    }

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Groupbox);

    protected override void Dispose(bool disposing) {
        if (disposing) {
            if (ParentFormula != null) {
                ParentFormula.PropertyChanged -= ParentFormula_PropertyChanged;
            }
        }
    }

    protected override void DrawExplicit(Graphics gr, Rectangle visibleArea, RectangleF positionModified, float scale, float shiftX, float shiftY) {
        DrawColorScheme(gr, positionModified, scale, null, false, false, false);
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

        //if (!ForPrinting) {
        //    DrawColorScheme(gr, positionModified, scale, InputColorId, true, true, true);
        //}

        base.DrawExplicit(gr, visibleArea, positionModified, scale, shiftX, shiftY);

        DrawArrorInput(gr, positionModified, scale, ForPrinting, InputColorId);
    }

    private void ParentFormula_PropertyChanged(object sender, PropertyChangedEventArgs e) {
        if (IsDisposed) { return; }
        if (ParentFormula == null) { return; }

        if (ParentFormula.NotAllowedChilds.Contains(_child)) {
            Child = string.Empty;
        }

        //OnPropertyChanged(string propertyname);
    }

    #endregion
}