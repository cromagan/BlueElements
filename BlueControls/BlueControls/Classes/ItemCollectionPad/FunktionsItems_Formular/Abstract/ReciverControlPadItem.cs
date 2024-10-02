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
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad.Abstract;
using BlueDatabase;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using static BlueBasics.Converter;
using static BlueBasics.Generic;
using static BlueBasics.Polygons;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

#nullable enable

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;

/// <summary>
/// Standard für Objekte, die einen Datenbank/Zeilenbezug haben.
/// Stellt auch alle Methode breit, zum Einrichten der Breite und Benutzer-Sichtbarkeiten.
/// Nur Tabs, die ein solches Objekt haben, werden als anzeigewürdig gewertet.
/// </summary>

public abstract class ReciverControlPadItem : RectanglePadItem, IHasKeyName, IPropertyChangedFeedback, IHasVersion, IReadableText, IErrorCheckable {

    #region Fields

    public static readonly BlueFont CaptionFnt = Skin.GetBlueFont(Design.Caption, States.Standard);
    private readonly List<string> _getFilterFromKeys = [];
    private ReadOnlyCollection<ReciverSenderControlPadItem>? _getFilterFrom;
    private List<int> _inputColorId = [];
    private ReadOnlyCollection<string> _visibleFor = new([]);

    private XPosition _X_Position = XPosition.frei;

    #endregion

    #region Constructors

    protected ReciverControlPadItem(string keyName, ConnectedFormula.ConnectedFormula? parentFormula) : base(keyName) {
        ParentFormula = parentFormula;
        SetCoordinates(new RectangleF(0, 0, 50, 30));
    }

    #endregion

    #region Properties

    public abstract AllowedInputFilter AllowedInputFilter { get; }

    /// <summary>
    /// Holt die Datebank aus dem erst Parent, da das Output
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public Database? DatabaseInput {
        get {
            //if (item.DatabaseInputMustMatchOutputDatabase) {
            //    return item is ReciverSenderControlPadItem iiss ? iiss.DatabaseOutput : null;
            //}

            var g = GetFilterFromGet();

            return g.Count == 0 ? null : g[0].DatabaseOutput;
        }
    }

    public abstract bool DatabaseInputMustMatchOutputDatabase { get; }

    public List<int> InputColorId {
        get {
            if (_inputColorId.Count == 0) {
                CalculateInputColorIds();
            }

            return _inputColorId;
        }
    }

    public abstract bool InputMustBeOneRow { get; }

    public override bool MoveXByMouse {
        get {
            if (_X_Position != XPosition.frei) { return false; }
            return base.MoveXByMouse;
        }
    }

    public abstract bool MustBeInDrawingArea { get; }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ConnectedFormula.ConnectedFormula? ParentFormula { get; set; }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ReadOnlyCollection<string> Parents {
        get => new(_getFilterFromKeys);

        set {
            // Dann die Collection leeren
            _getFilterFrom = null;
            _getFilterFromKeys.Clear();

            // Die Collection befüllen
            _getFilterFromKeys.AddRange(value);

            CalculateInputColorIds();
            OnPropertyChanged();
            OnDoUpdateSideOptionMenu();
        }
    }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Version { get; set; }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ReadOnlyCollection<string> VisibleFor {
        get => _visibleFor;
        set {
            var tmp = Database.RepairUserGroups(value);
            if (!_visibleFor.IsDifferentTo(tmp)) { return; }

            _visibleFor = tmp.AsReadOnly();
            OnPropertyChanged();
        }
    }

    public XPosition X_Position {
        get {
            return _X_Position;
        }

        set {
            if (_X_Position == value) { return; }
            _X_Position = value;
            OnPropertyChanged();

            if (_X_Position != XPosition.frei) {
                PointMoved(_pLo, new MoveEventArgs(false));
            }
        }
    }

    protected override int SaveOrder => 3;

    #endregion

    #region Methods

    public override void AddedToCollection() {
        base.AddedToCollection();
        CalculateColorIds();
        OnPropertyChanged();
    }

    public List<int> CalculateColorIds() {
        var l = new List<int>();

        foreach (var thisId in Parents) {
            if (Parent?[thisId] is ReciverSenderControlPadItem i) {
                l.Add(i.OutputColorId);
            }
        }

        if (l.Count == 0) { l.Add(-1); }

        return l;
    }

    public void CalculateInputColorIds() {
        if (IsDisposed) { return; }
        var nl = CalculateColorIds();

        if (nl.IsDifferentTo(_inputColorId)) {
            _inputColorId = nl;
            //item.on;
        }
    }

    public virtual string ErrorReason() {
        if (Parent == null) { return "Keiner Ansicht zugeordnet."; }

        if (MustBeInDrawingArea && !IsInDrawingArea(UsedArea, Parent.UsedArea.ToRect())) {
            return "Element ist nicht im Zeichenbereich."; // Invalidate löste die Berechnungen aus, muss sein, weil mehrere Filter die Berechnungen triggern
        }

        var p = GetFilterFromGet();

        if (AllowedInputFilter == AllowedInputFilter.None) {
            if (p.Count > 0) { return "Keine Parents erlaubt."; }
        } else if (AllowedInputFilter == AllowedInputFilter.One) {
            if (p.Count != 1) { return "Es muss genau ein Parent gewählt werden."; }
        } else if (!AllowedInputFilter.HasFlag(AllowedInputFilter.None)) {
            if (p.Count == 0) { return "Parents müssen gewählt werden."; }
        }

        return string.Empty;
    }

    public ReadOnlyCollection<ReciverSenderControlPadItem> GetFilterFromGet() {
        if (Parent == null) {
            //Develop.DebugPrint(FehlerArt.Warnung, "Parent nicht initialisiert!");
            return new ReadOnlyCollection<ReciverSenderControlPadItem>(new List<ReciverSenderControlPadItem>());
        }

        if (_getFilterFrom == null || _getFilterFrom.Count != _getFilterFromKeys.Count) {
            var l = new List<ReciverSenderControlPadItem>();

            foreach (var thisk in _getFilterFromKeys) {
                if (Parent[thisk] is ReciverSenderControlPadItem isf) {
                    l.Add(isf);
                }
            }

            _getFilterFrom = new(l);
        }

        return _getFilterFrom;
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result = [];

        if (Parent is null) { return result; }

        Database? outp = null;

        if (this is ReciverSenderControlPadItem iiss) {
            outp = iiss.DatabaseOutput;
        }

        if (AllowedInputFilter != AllowedInputFilter.None) {
            result.Add(new FlexiControl("Eingang:", widthOfControl, true));

            var x = new List<AbstractListItem>();

            // Die _internal, die man noch wählen könnte
            foreach (var thisR in Parent) {
                if (thisR is ReciverSenderControlPadItem rfp) {
                    if (rfp != this) {
                        x.Add(ItemOf(rfp.ReadableText(), rfp.KeyName, rfp.SymbolForReadableText(), true, "1"));
                    }
                }
            }

            switch (AllowedInputFilter) {
                case AllowedInputFilter.One:
                    result.Add(new FlexiControlForProperty<ReadOnlyCollection<string>>(() => Parents, string.Empty, 3, x, CheckBehavior.SingleSelection, AddType.None, System.Windows.Forms.ComboBoxStyle.DropDownList));
                    break;

                case AllowedInputFilter.More:
                    result.Add(new FlexiControlForProperty<ReadOnlyCollection<string>>(() => Parents, string.Empty, 3, x, CheckBehavior.MultiSelection, AddType.None, System.Windows.Forms.ComboBoxStyle.DropDownList));
                    break;

                case AllowedInputFilter.More | AllowedInputFilter.None:
                    result.Add(new FlexiControlForProperty<ReadOnlyCollection<string>>(() => Parents, string.Empty, 3, x, CheckBehavior.MultiSelection, AddType.None, System.Windows.Forms.ComboBoxStyle.DropDownList));
                    break;

                case AllowedInputFilter.One | AllowedInputFilter.None:
                    result.Add(new FlexiControlForProperty<ReadOnlyCollection<string>>(() => Parents, string.Empty, 3, x, CheckBehavior.SingleSelection, AddType.None, System.Windows.Forms.ComboBoxStyle.DropDownList));
                    break;

                default:

                    //case AllowedInputFilter.None:
                    break;
            }
        }

        if (Bei_Export_sichtbar) {
            var u = new List<AbstractListItem>();
            u.AddRange(ItemsOf(typeof(XPosition)));

            result.Add(new FlexiControl("Sichtbarkeit:", widthOfControl, true));
            result.Add(new FlexiControlForProperty<XPosition>(() => X_Position, u));
            //result.Add(new FlexiControlForDelegate(Breite_berechnen, "Breite berechnen", ImageCode.Zeile));
            //result.Add(new FlexiControlForDelegate(Standardhöhe_setzen, "Standardhöhe setzen", ImageCode.Zeile));

            if (MustBeInDrawingArea) {
                var vf = new List<AbstractListItem>();
                vf.AddRange(ItemsOf(ConnectedFormula.ConnectedFormula.VisibleFor_AllUsed()));
                result.Add(new FlexiControlForProperty<ReadOnlyCollection<string>>(() => VisibleFor, "In diesen Ansichten sichtbar:", 5, vf, CheckBehavior.MultiSelection, AddType.Text, System.Windows.Forms.ComboBoxStyle.DropDownList));
            }
        }

        return result;
    }

    public bool IsVisibleForMe(string mode, bool nowDrawing) {
        //if(!Bei_Export_sichtbar ) { return false; }
        if (!MustBeInDrawingArea && !nowDrawing) { return false; } // Unwichtiges Element

        if (!MustBeInDrawingArea && VisibleFor.Count == 0) { return true; } // Unwichtiges Element

        if (VisibleFor.Count == 0) { return false; }
        if (string.IsNullOrEmpty(mode)) { return true; }

        if (VisibleFor.Contains(Constants.Everybody, false)) { return true; }
        if (VisibleFor.Contains(mode, false)) { return true; }
        if (string.Equals(UserGroup, Constants.Administrator, StringComparison.OrdinalIgnoreCase) &&
    VisibleFor.Contains(Constants.Administrator, false)) { return true; }

        if (VisibleFor.Contains("#USER: " + UserName, false)) { return true; }
        if (VisibleFor.Contains("#USER:" + UserName, false)) { return true; }

        //if (!string.IsNullOrEmpty(modus) && Modes.Count > 0) {
        //}

        //if (string.IsNullOrEmpty(UserGroup) || string.IsNullOrEmpty(UserName)) { return true; }

        //if (UserGroup.Equals(Constants.Administrator, StringComparison.OrdinalIgnoreCase)) { return true; }

        //if (VisibleFor.Contains(UserGroup, false)) { return true; }

        //   return modes.Any(mode => VisibleFor.Any(visible => string.Equals(mode, visible, StringComparison.OrdinalIgnoreCase)));

        return false;
    }

    public override void OnPropertyChanged() {
        if (IsDisposed) { return; }
        base.OnPropertyChanged();
        this.RaiseVersion();
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("Version", Version);

        if (MustBeInDrawingArea) {
            result.ParseableAdd("VisibleFor", VisibleFor, false);
        }

        result.ParseableAdd("XLock", _X_Position);

        result.ParseableAdd("GetFilterFromKeys", _getFilterFromKeys, false);
        //result.ParseableAdd("GetValueFromKey", _getValueFromkey ?? string.Empty);

        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "version":
                Version = IntParse(value);
                return true;

            case "visiblefor":
                value = value.Replace("\r", "|");
                var tmpv = value.FromNonCritical().SplitBy("|").ToList();
                if (tmpv.Count == 0) { tmpv.Add(Constants.Everybody); }
                VisibleFor = Database.RepairUserGroups(tmpv).AsReadOnly();
                return true;

            case "xlock":
                _X_Position = (XPosition)IntParse(value);
                return true;

            case "getvaluefrom":
            case "getvaluefromkey":
            case "getfilterfromkeys":
                var tmp = value.FromNonCritical().SplitBy("|");
                _getFilterFromKeys.Clear();
                foreach (var thiss in tmp) {
                    _getFilterFromKeys.Add(thiss.FromNonCritical());
                }
                _getFilterFrom = null;
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override void PointMoved(object sender, MoveEventArgs e) {
        if (_X_Position == XPosition.frei ||
            Parent == null) {
            base.PointMoved(sender, e);
            return;
        }

        var anzahlSpaltenImFormular = (int)_X_Position / 100;
        var aufXPosition = (int)(_X_Position - anzahlSpaltenImFormular * 100);

        var wi = (Parent.UsedArea.Width - (AutosizableExtension.GridSize * (anzahlSpaltenImFormular - 1))) / anzahlSpaltenImFormular;
        var xpos = (wi * (aufXPosition - 1)) + (AutosizableExtension.GridSize * (aufXPosition - 1));

        _pLo.X = xpos;
        _pl.X = xpos;
        _pLu.X = xpos;

        _pRu.X = xpos + wi;
        _pr.X = xpos + wi;
        _pRu.X = xpos + wi;

        base.PointMoved(sender, e);
    }

    //    if (x.Height < he) { x.Height = he; }
    //    SetCoordinates(x, true);
    //    OnPropertyChanged();
    //}
    protected static void DrawArrow(Graphics gr, RectangleF positionModified, float scale, int colorId, Alignment al, float valueArrow, float xmod) {
        var p = positionModified.PointOf(al);
        var width = (int)(scale * 25);
        var height = (int)(scale * 12);
        var pa = Poly_Arrow(new Rectangle(0, -(width / 2), height, width));

        var c = Skin.IdColor(colorId);
        var c2 = c.Darken(0.4);

        gr.TranslateTransform(p.X + xmod, p.Y - valueArrow);

        //Info: das ergibt zwei übernanderliegenede Ellipsen
        //gr.DrawEllipse(Pens.MediumSeaGreen, new Rectangle(-20,-10,40,20));
        //gr.RotateTransform(90);
        //gr.DrawEllipse(Pens.MediumSeaGreen, new Rectangle(-10, -20, 20, 40));

        gr.RotateTransform(90);

        gr.FillPath(new SolidBrush(c), pa);
        gr.DrawPath(new Pen(c2, 1 * scale), pa);

        gr.RotateTransform(-90);
        gr.TranslateTransform(-p.X - xmod, -p.Y + valueArrow);

        //var x = QuickImage.GenerateCode("Pfeil_Unten", (int)(10 * zoom), (int)(10 * zoom), ImageCodeEffect.Ohne, string.Empty, string.Empty, 100, 100, 0, 0, symbol);

        //var sy2 = QuickImage.Get(x);
        //gr.DrawImage(sy2, p.X - (sy2.Width / 2) + xmod, p.Y - valueSymbol);

        //if (!string.IsNullOrEmpty(symbol)) {
        //    var co = QuickImage.GenerateCode(symbol, (int)(5 * zoom), (int)(5 * zoom), ImageCodeEffect.Ohne, string.Empty, string.Empty, 120, 120, 0, 20, string.Empty);
        //    var sy = QuickImage.Get(co);
        //    gr.DrawImage(sy, p.X - (sy.Width / 2) + xmod, p.Y - valueSymbol);
        //}
    }

    //    var he = MmToPixel(ConnectedFormula.ConnectedFormula.StandardHöhe, ItemCollectionPadItem.Dpi);
    //    var he1 = MmToPixel(1, ItemCollectionPadItem.Dpi);
    //    x.Height = (int)(x.Height / he) * he;
    //    x.Height = (int)((x.Height / he1) + 0.99) * he1;
    protected void DrawArrorInput(Graphics gr, RectangleF positionModified, float scale, bool forPrinting, List<int>? colorId) {
        if (forPrinting) { return; }

        var arrowY = (int)(scale * 12) * 0.35f;

        var width = (int)(scale * 25);

        colorId ??= [];

        if (colorId.Count == 0) { colorId.Add(-1); }

        var start = -((colorId.Count - 1) * width / 2);

        foreach (var thisColorId in colorId) {
            DrawArrow(gr, positionModified, scale, thisColorId, Alignment.Top_HorizontalCenter, arrowY, start);
            start += width;
        }
    }

    //public void Standardhöhe_setzen() {
    //    var x = UsedArea;
    protected void DrawArrowOutput(Graphics gr, RectangleF positionModified, float scale, bool forPrinting, int colorId) {
        if (forPrinting) { return; }
        var arrowY = (int)(scale * 12) * 0.45f;
        DrawArrow(gr, positionModified, scale, colorId, Alignment.Bottom_HorizontalCenter, arrowY, 0);
    }

    protected void DrawColorScheme(Graphics gr, RectangleF drawingCoordinates, float scale, List<int>? id, bool drawSymbol, bool drawText, bool transparent) {
        if (!transparent) {
            gr.FillRectangle(Brushes.White, drawingCoordinates);
        }

        var w = scale * 3;

        var tmp = drawingCoordinates;
        tmp.Inflate(-w, -w);

        var c = Skin.IdColor(id);
        var c2 = c;

        if (c.IsNearWhite(0.99)) {
            c2 = Color.Gray;
            c = Color.Transparent;
        }

        if (transparent) {
            if (c.A > 128) {
                gr.DrawRectangle(new Pen(c.Brighten(0.9).MakeTransparent(128), w * 2), tmp);
            }

            gr.DrawRectangle(new Pen(c2.Brighten(0.6).MakeTransparent(128), scale), drawingCoordinates);
        } else {
            gr.DrawRectangle(new Pen(c.Brighten(0.9), w * 2), tmp);
            gr.DrawRectangle(new Pen(c2.Brighten(0.6), scale), drawingCoordinates);
        }

        if (drawSymbol && drawText) {
            Skin.Draw_FormatedText(gr, ReadableText(), SymbolForReadableText()?.Scale(scale), Alignment.Horizontal_Vertical_Center, drawingCoordinates.ToRect(), null, true, ColumnFont?.Scale(scale), false);
        } else if (drawSymbol) {
            Skin.Draw_FormatedText(gr, string.Empty, SymbolForReadableText()?.Scale(scale), Alignment.Horizontal_Vertical_Center, drawingCoordinates.ToRect(), null, true, ColumnFont?.Scale(scale), false);
        } else if (drawText) {
            Skin.Draw_FormatedText(gr, ReadableText(), null, Alignment.Horizontal_Vertical_Center, drawingCoordinates.ToRect(), null, true, ColumnFont?.Scale(scale), false);
        }
    }

    protected override void DrawExplicit(Graphics gr, Rectangle visibleArea, RectangleF positionModified, float scale, float shiftX, float shiftY) {
        base.DrawExplicit(gr, visibleArea, positionModified, scale, shiftX, shiftY);
        CalculateColorIds();
    }

    protected void DrawFakeControl(Graphics gr, RectangleF positionModified, float scale, CaptionPosition captionPosition, string captiontxt, EditTypeFormula edittype) {
        Point cap;
        var uc = positionModified.ToRect();

        switch (captionPosition) {
            case CaptionPosition.ohne:
                cap = new Point(-1, -1);
                break;

            case CaptionPosition.Links_neben_dem_Feld_unsichtbar:
            case CaptionPosition.Links_neben_dem_Feld:
                cap = new Point(0, 0);
                uc.X += (int)(100 * scale);
                uc.Width -= (int)(100 * scale);
                break;

            default:
                cap = new Point(0, 0);
                uc.Y += (int)(19 * scale);
                uc.Height -= (int)(19 * scale);
                break;
        }

        if (cap.X >= 0) {
            var e = new RectangleF(positionModified.Left + (cap.X * scale), positionModified.Top + (cap.Y * scale), positionModified.Width, 16 * scale);
            Skin.Draw_FormatedText(gr, captiontxt, null, Alignment.Top_Left, e.ToRect(), CaptionFnt.Scale(scale), true);
        }

        if (uc is { Width: > 0, Height: > 0 }) {
            Skin.Draw_Back(gr, Design.TextBox, States.Standard, uc, null, false);
            Skin.Draw_Border(gr, Design.TextBox, States.Standard, uc);

            //gr.FillRectangle(Brushes.LightGray, uc);
            //gr.DrawRectangle(new Pen(Color.Black, zoom), uc);
        }
    }

    #endregion
}