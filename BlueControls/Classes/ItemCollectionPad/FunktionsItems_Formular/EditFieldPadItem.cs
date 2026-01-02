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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueTable;
using BlueTable.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static BlueBasics.Converter;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular;

/// <summary>
/// Erzeut ein FlexiControllForCell
/// Standard-Bearbeitungs-Feld
/// </summary>
public class EditFieldPadItem : ReciverControlPadItem, IItemToControl, IAutosizable {

    #region Fields

    private bool _autoX = true;
    private EditTypeFormula _bearbeitung = EditTypeFormula.Textfeld;
    private CaptionPosition _captionPosition = CaptionPosition.Über_dem_Feld;
    private string _columnName = string.Empty;

    #endregion

    #region Constructors

    public EditFieldPadItem() : this(string.Empty, null) { }

    public EditFieldPadItem(string keyName, ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-EditField";

    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.One;

    public bool AutoSizeableHeight {
        get {
            if (_bearbeitung == EditTypeFormula.nur_als_Text_anzeigen) {
                return (int)CanvasUsedArea.Height > AutosizableExtension.MinHeigthCaption;
            }

            if (_captionPosition is CaptionPosition.Links_neben_dem_Feld or CaptionPosition.ohne) {
                return (int)CanvasUsedArea.Height > AutosizableExtension.MinHeigthTextBox;
            }

            return (int)CanvasUsedArea.Height > AutosizableExtension.MinHeigthCapAndBox;
        }
    }

    [DefaultValue(true)]
    public bool AutoX {
        get => _autoX;
        set {
            if (IsDisposed) { return; }
            if (_autoX == value) { return; }
            _autoX = value;
            OnPropertyChanged();
        }
    }

    public CaptionPosition CaptionPosition {
        get => _captionPosition;
        set {
            if (IsDisposed) { return; }
            if (_captionPosition == value) { return; }
            _captionPosition = value;
            OnPropertyChanged();
        }
    }

    public ColumnItem? Column {
        get {
            var c = TableInput?.Column[_columnName];
            return c is not { IsDisposed: false } ? null : c;
        }
    }

    public string ColumnName {
        get => _columnName;
        set {
            if (IsDisposed) { return; }
            if (_columnName == value) { return; }
            _columnName = value;
            OnPropertyChanged();
            OnDoUpdateSideOptionMenu();
        }
    }

    public override string Description => "Standard Bearbeitungs-Steuerelement für Zellen.";

    public EditTypeFormula EditType {
        get => _bearbeitung;
        set {
            if (IsDisposed) { return; }
            if (_bearbeitung == value) { return; }
            _bearbeitung = value;
            OnPropertyChanged();
        }
    }

    public override bool InputMustBeOneRow => true;
    public override bool MustBeInDrawingArea => true;
    public override bool TableInputMustMatchOutputTable => false;

    #endregion

    #region Methods

    public static List<AbstractListItem> GetAllowedEditTypes(ColumnItem? column) {
        var l = new List<AbstractListItem>();
        if (column is not { IsDisposed: false }) { return l; }
        var t = typeof(EditTypeFormula);

        foreach (int z1 in Enum.GetValues(t)) {
            if (column.UserEditDialogTypeInFormula((EditTypeFormula)z1)) {
                l.Add(new TextListItem(Enum.GetName(t, z1).Replace("_", " "), z1.ToString1(), null, false, true, string.Empty));
            }
        }
        return l;
    }

    public Control CreateControl(ConnectedFormulaView parent, string mode) {
        //var ff = parent.SearchOrGenerate(rfw2);

        var con = new FlexiCellControl {
            ColumnName = _columnName,
            EditType = EditType,
            CaptionPosition = CaptionPosition,
        };

        con.DoDefaultSettings(parent, this, mode);

        return con;
    }

    public override string ErrorReason() {
        var f = base.ErrorReason();

        if (!string.IsNullOrWhiteSpace(f)) { return f; }

        if (Column is not { IsDisposed: false }) { return "Spaltenangabe fehlt"; }

        return string.Empty;
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result = [.. base.GetProperties(widthOfControl)];

        if (TableInput is not { IsDisposed: false } tb) { return result; }

        result.Add(new FlexiControl("Einstellungen:", widthOfControl, true));

        var lst = new List<AbstractListItem>();
        lst.AddRange(ItemsOf(tb.Column, true));

        result.Add(new FlexiControlForProperty<string>(() => ColumnName, lst));

        if (Column is not { IsDisposed: false }) { return result; }

        result.Add(new FlexiControlForProperty<CaptionPosition>(() => CaptionPosition, ItemsOf(typeof(CaptionPosition))));
        result.Add(new FlexiControlForProperty<bool>(() => AutoX));
        result.Add(new FlexiControlForProperty<EditTypeFormula>(() => EditType, GetAllowedEditTypes(Column)));

        return result;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("ColumnName", _columnName);
        result.ParseableAdd("EditType", _bearbeitung);
        result.ParseableAdd("Caption", _captionPosition);
        result.ParseableAdd("AutoDistance", _autoX);
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "column":
                //Column = GetRowFrom?.Table?.Column.GetByKey(LongParse(value));
                return true;

            case "columnname":
                _columnName = value;
                return true;

            case "fieldid":
                return true;

            case "edittype":
                _bearbeitung = (EditTypeFormula)IntParse(value);
                return true;

            case "caption":
                _captionPosition = (CaptionPosition)IntParse(value);
                return true;

            case "autodistance":
                _autoX = value.FromPlusMinus();
                return true;

            case "nosave":
                return true;

            case "style":
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() {
        const string txt = "Zelle: ";

        return txt + Column?.Caption;
    }

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Stift, 16);

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float zoom, float offsetX, float offsetY) {
        if (!ForPrinting) {
            DrawColorScheme(gr, positionControl, zoom, InputColorId, true, true, false);
        }

        //if (Column  ==null || Column .IsDisposed) {
        //    Skin.Draw_FormatedText(gr, "Spalte fehlt", QuickImage.Get(ImageCode.Warnung, (int)(16 * zoom)), Alignment.Horizontal_Vertical_Center, positionControl.ToRect(), CaptionFnt.Scale(zoom), true);
        //} else {
        DrawFakeControl(gr, positionControl, zoom, CaptionPosition, Column?.ReadableText() + ":", EditType);
        //}

        if (!ForPrinting) {
            DrawColorScheme(gr, positionControl, zoom, InputColorId, true, true, true);
        }

        base.DrawExplicit(gr, visibleAreaControl, positionControl, zoom, offsetX, offsetY);

        DrawArrorInput(gr, positionControl, zoom, ForPrinting, InputColorId);
    }

    #endregion
}