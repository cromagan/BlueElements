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
using BlueControls.Interfaces;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using static BlueBasics.Converter;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

/// <summary>
/// Erzeut ein Unter-Element von ConnectedFormulaView
/// </summary>
public class RegionFormulaPadItem : ReciverControlPadItem, IItemToControl, IAutosizable {

    #region Fields

    private string _child = string.Empty;
    private GroupBoxStyle _rahmenStil = GroupBoxStyle.Normal;

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
    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.None | AllowedInputFilter.More ;
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

    public override bool DatabaseInputMustMatchOutputDatabase => false;
    public override string Description => "Ein Steuerelement, mit dem ein untergeordnetes Formular angezeigt werden kann.";

    public override bool InputMustBeOneRow => true;
    public override bool MustBeInDrawingArea => true;
    public override string MyClassId => ClassId;

    [DefaultValue(GroupBoxStyle.Normal)]
    public GroupBoxStyle RahmenStil {
        get => _rahmenStil;
        set {
            if (_rahmenStil == value) { return; }
            _rahmenStil = value;
            OnPropertyChanged();
        }
    }

    protected override int SaveOrder => 1000;

    #endregion

    #region Methods

    public System.Windows.Forms.Control CreateControl(ConnectedFormulaView parent, string mode) {
        ConnectedFormula.ConnectedFormula? cf = null;

        var txt = "?";

        if (_child.EndsWith(".cfo", StringComparison.OrdinalIgnoreCase)) {
            cf = ConnectedFormula.ConnectedFormula.GetByFilename(_child);
            txt = _child.FileNameWithoutSuffix();
        }

        var con = new ConnectedFormulaView(mode, "Head") {
            GroupBoxStyle = _rahmenStil
        };

        if (_rahmenStil != GroupBoxStyle.Nothing) {
            con.Text = txt;
        }

        con.DoDefaultSettings(parent, this, mode);
        con.InitFormula(cf, DatabaseInput);

        return con;
    }

    public override string ErrorReason() {
        if (string.IsNullOrEmpty(_child)) {
            return "Keine Formular gewählt.";
        }

        return base.ErrorReason();
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        var cl = new List<AbstractListItem>();

        ParentFormula?.AddChilds(cl, ParentFormula.NotAllowedChilds);

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
                _rahmenStil = (GroupBoxStyle)IntParse(value);
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() {
        const string txt = "Unterformular: ";

        return txt + DatabaseInput?.Caption;
    }

    public override QuickImage SymbolForReadableText() {
        return QuickImage.Get(ImageCode.Registersammlung, 16, Color.Transparent, Skin.IdColor(InputColorId));
    }

    public override string ToParseableString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = [];

        result.ParseableAdd("Parent", ParentFormula?.Filename ?? string.Empty);
        result.ParseableAdd("Child", _child);
        result.ParseableAdd("BorderStyle", _rahmenStil);
        return result.Parseable(base.ToParseableString());
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            if (ParentFormula != null) {
                ParentFormula.PropertyChanged -= ParentFormula_PropertyChanged;
            }
        }
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


    //private void Childs_ContextMenuInit(object sender, ContextMenuInitEventArgs e) => e.ContextMenu.Add(ItemOf(ContextMenuCommands.Bearbeiten));

    //private void Childs_ContextMenuItemClicked(object sender, ContextMenuItemClickedEventArgs e) {
    //    if (e.HotItem is not AbstractListItem it) { return; }

    //    if (e.Item.KeyName.ToLowerInvariant() == "bearbeiten") {
    //        MultiUserFile.SaveAll(false);

    //        var x = new ConnectedFormulaEditor(it.KeyName, ParentFormula?.NotAllowedChilds);
    //        _ = x.ShowDialog();
    //        MultiUserFile.SaveAll(false);
    //        x.Dispose();
    //    }
    //}

    private void ParentFormula_PropertyChanged(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        if (ParentFormula == null) { return; }

        if (ParentFormula.NotAllowedChilds.Contains(_child)) {
            Child = string.Empty;
        }

        //OnPropertyChanged();
    }

    #endregion
}