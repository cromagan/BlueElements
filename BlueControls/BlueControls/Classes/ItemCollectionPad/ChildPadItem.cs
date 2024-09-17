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
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad.Abstract;
using BlueScript.Variables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static BlueBasics.Constants;
using static BlueBasics.Converter;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.ItemCollectionPad;

public class ChildPadItem : RectanglePadItem, IMouseAndKeyHandle, ICanHaveVariables {

    #region Fields

    public readonly string Seite;

    public readonly List<AbstractPadItem>? ZoomItems;

    private string _name;

    private CreativePad? _padInternal;

    private Bitmap? _tmpBmp;

    #endregion

    #region Constructors

    public ChildPadItem(CreativePad? pad) : base(string.Empty) {
        PadInternal = pad;
        Seite = string.Empty;
        ZoomItems = null;
        _name = string.Empty;
        Textlage = (Alignment)(-1);
        Randfarbe = Color.Transparent;
        Eingebettete_Ansichten = [];
    }

    public ChildPadItem() : this(null) { }

    #endregion

    #region Properties

    public static string ClassId => "CHILDPAD";

    public override string Description => string.Empty;

    [Description("Soll eine Umrandung einer anderen Ansicht hier angezeigt werden,<br>muss dessen Name hier eingegeben werden.")]
    public List<string> Eingebettete_Ansichten { get; }

    public override string MyClassId => ClassId;

    [Description("Name und gleichzeitig eventuelle Beschriftung dieser Ansicht.")]
    public string Name {
        get => _name;
        set {
            if (IsDisposed) { return; }
            if (value == _name) { return; }
            _name = value;
            OnPropertyChanged();
        }
    }

    public CreativePad? PadInternal {
        get => _padInternal;
        set {
            if (_padInternal?.Items != null) {
                _padInternal.Items.PropertyChanged -= _Pad_DoInvalidate;
            }
            _padInternal = value;
            if (_padInternal?.Items != null) {
                _padInternal.Items.PropertyChanged += _Pad_DoInvalidate;
            }
        }
    }

    public Color Randfarbe { get; set; }

    public Alignment Textlage { get; set; }

    protected override int SaveOrder => 1000;

    #endregion

    #region Methods

    public override List<GenericControl> GetProperties(int widthOfControl) {
        var lage = new List<AbstractListItem>();
        lage.Add(ItemOf("ohne", "-1"));
        lage.Add(ItemOf("Links oben", ((int)Alignment.Top_Left).ToString()));

        List<GenericControl> result =
        [
            new FlexiControlForProperty<string>(() => Name),
            new FlexiControlForProperty<Color>(() => Randfarbe)
        ];

        result.Add(new FlexiControlForProperty<Alignment>(() => Textlage, lage));
        result.Add(new FlexiControlForProperty<List<string>>(() => Eingebettete_Ansichten, 5));

        result.Add(new FlexiControl());
        result.AddRange(base.GetProperties(widthOfControl));

        return result;
    }

    public List<AbstractPadItem> HotItems(MouseEventArgs e, float zoom, float shiftX, float shiftY) {
        var e2 = ZoomMouse(e, zoom, shiftX, shiftY);
        if (e2 == null || PadInternal == null) { return []; }
        return PadInternal.HotItems(e2);
    }

    public bool KeyUp(KeyEventArgs e, float cZoom, float shiftX, float shiftY) {
        if (PadInternal?.Items is not { Count: not 0 }) { return false; }
        PadInternal.DoKeyUp(e, false);
        return true;
    }

    public bool MouseDown(MouseEventArgs e, float zoom, float shiftX, float shiftY) {
        var e2 = ZoomMouse(e, zoom, shiftX, shiftY);
        if (e2 == null) { return false; }
        PadInternal?.DoMouseDown(e2);
        return true;
    }

    public bool MouseMove(MouseEventArgs e, float zoom, float shiftX, float shiftY) {
        var e2 = ZoomMouse(e, zoom, shiftX, shiftY);
        if (e2 == null) { return false; }
        PadInternal?.DoMouseMove(e2);
        return true;
    }

    public bool MouseUp(MouseEventArgs e, float zoom, float shiftX, float shiftY) {
        var e2 = ZoomMouse(e, zoom, shiftX, shiftY);
        if (e2 == null) { return false; }
        PadInternal?.DoMouseUp(e2);
        return true;
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            //
            case "readabletext":
                //    _ReadableText = value.FromNonCritical();
                //    _VariableText = _ReadableText;
                return true;

            case "fixsize": // TODO: Entfernt am 24.05.2021
                //Größe_fixiert = value.FromPlusMinus();
                return true;

            case "name":
                _name = value.FromNonCritical();
                return true;

            case "data":
                PadInternal = new CreativePad();
                var it = new ItemCollectionPad();
                it.Parse(value);
                PadInternal.Items = it;
                return true;

            case "checked":
                return true;

            case "embedded":

                value = value.Replace("\r", "|");

                var tmp = value.FromNonCritical().SplitBy("|");
                Eingebettete_Ansichten.Clear();
                foreach (var thiss in tmp) {
                    Eingebettete_Ansichten.Add(thiss.FromNonCritical());
                }
                return true;

            case "color":
                Randfarbe = value.FromHtmlCode();
                return true;

            case "pos":
                Textlage = (Alignment)IntParse(value);
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override void ProcessStyleChange() {
        if (PadInternal?.Items == null || Parent == null) { return; }

        RemovePic();
        PadInternal.Items.SheetStyle = Parent.SheetStyle;
        PadInternal.Items.SheetStyleScale = Parent.SheetStyleScale;
    }

    public override string ReadableText() => "Unterstufe";

    public bool ReplaceVariable(Variable variable) {
        if (IsDisposed) { return false; }
        if (PadInternal?.Items == null) { return false; }
        var b = PadInternal.Items.ReplaceVariable(variable);
        if (b) { OnPropertyChanged(); }
        return b;
    }

    public bool ResetVariables() {
        if (IsDisposed) { return false; }
        if (PadInternal?.Items == null) { return false; }
        var b = PadInternal.Items.ResetVariables();
        if (b) { OnPropertyChanged(); }
        return b;
    }

    public override QuickImage? SymbolForReadableText() => QuickImage.Get(ImageCode.Pfeil_Oben, 16);

    public override string ToParseableString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = [];

        result.ParseableAdd("Name", _name);

        if (Textlage != (Alignment)(-1)) { result.ParseableAdd("Pos", Textlage); }
        result.ParseableAdd("Embedded", Eingebettete_Ansichten, false);
        result.ParseableAdd("Color", Randfarbe.ToArgb());
        result.ParseableAdd("Data", "Item", PadInternal?.Items);

        return result.Parseable(base.ToParseableString());
    }

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        if (PadInternal?.Items == null || Parent == null) { return; }

        try {
            var trp = positionModified.PointOf(Alignment.Horizontal_Vertical_Center);
            gr.TranslateTransform(trp.X, trp.Y);
            gr.RotateTransform(-Drehwinkel);
            Font font = new("Arial", 30 * zoom);
            if (PadInternal?.Items != null) {
                PadInternal.Items.SheetStyle = Parent.SheetStyle;
                PadInternal.Items.SheetStyleScale = Parent.SheetStyleScale;
                if (_tmpBmp != null) {
                    if (Math.Abs(_tmpBmp.Width - positionModified.Width) > IntTolerance || Math.Abs(positionModified.Height - _tmpBmp.Height) > IntTolerance) {
                        _tmpBmp?.Dispose();
                        RemovePic();
                        Generic.CollectGarbage();
                    }
                }
                if (positionModified.Width < 1 || positionModified.Height < 1 || positionModified.Width > 20000 || positionModified.Height > 20000) { return; }
                _tmpBmp ??= new Bitmap((int)Math.Abs(positionModified.Width), (int)Math.Abs(positionModified.Height));
                var mb = PadInternal.Items.MaxBounds(ZoomItems);
                var zoomv = ItemCollectionPad.ZoomFitValue(mb, _tmpBmp.Size);
                var centerpos = ItemCollectionPad.CenterPos(mb, _tmpBmp.Size, zoomv);
                var slidervalues = ItemCollectionPad.SliderValues(mb, zoomv, centerpos);
                PadInternal.ShowInPrintMode = forPrinting;
                if (forPrinting) { PadInternal.Unselect(); }
                PadInternal.Items.DrawCreativePadToBitmap(_tmpBmp, States.Standard, zoomv, slidervalues.X, slidervalues.Y, Seite);
                if (_tmpBmp != null) {
                    foreach (var thisA in Eingebettete_Ansichten) {
                        ChildPadItem? pad = null;
                        foreach (var it in Parent) {
                            if (it is ChildPadItem cp) {
                                if (string.Equals(cp.Name, thisA, StringComparison.OrdinalIgnoreCase)) {
                                    pad = cp;
                                    break;
                                }
                            }
                        }
                        if (pad?.PadInternal?.Items != null) {
                            var mb2 = pad.PadInternal.Items.MaxBounds(pad.ZoomItems);
                            mb2.Inflate(-1, -1);
                            var tmpG = Graphics.FromImage(_tmpBmp);
                            Pen p = new(pad.Randfarbe, (float)(8.7d * zoom));
                            Pen p2 = new(Color.White, (float)(8.7d * zoom) + 2f);
                            p.DashPattern = [5, 1, 1, 1];
                            var dc2 = mb2.ZoomAndMoveRect(zoomv, slidervalues.X, slidervalues.Y, false);
                            tmpG.DrawRectangle(p2, dc2);
                            tmpG.DrawRectangle(p, dc2);
                            if (pad.Textlage != (Alignment)(-1)) {
                                var s = tmpG.MeasureString(pad.Name, font);
                                tmpG.FillRectangle(Brushes.White, new RectangleF(dc2.Left, dc2.Top - s.Height - (9f * zoom), s.Width, s.Height));
                                BlueFont.DrawString(tmpG, pad.Name, font, new SolidBrush(pad.Randfarbe), dc2.Left, dc2.Top - s.Height - (9f * zoom));
                            }
                        }
                    }
                    gr.DrawImage(_tmpBmp, new Rectangle((int)-positionModified.Width / 2, (int)-positionModified.Height / 2, (int)positionModified.Width, (int)positionModified.Height));
                }
            }
            gr.TranslateTransform(-trp.X, -trp.Y);
            gr.ResetTransform();
            if (!forPrinting) {
                BlueFont.DrawString(gr, Name, font, Brushes.Gray, positionModified.Left, positionModified.Top);
            }
            if (Textlage != (Alignment)(-1)) {
                Pen p = new(Randfarbe, (float)(8.7d * zoom)) {
                    DashPattern = [10, 2, 1, 2]
                };
                gr.DrawRectangle(p, positionModified);
                var s = gr.MeasureString(Name, font);
                BlueFont.DrawString(gr, Name, font, new SolidBrush(Randfarbe), positionModified.Left, positionModified.Top - s.Height - (9f * zoom));
            }
        } catch { }
        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
    }

    private void _Pad_DoInvalidate(object sender, System.EventArgs e) => OnPropertyChanged();

    private void RemovePic() {
        if (_tmpBmp != null) {
            _tmpBmp?.Dispose();
            _tmpBmp = null;
        }
    }

    private MouseEventArgs? ZoomMouse(MouseEventArgs e, float zoom, float shiftX, float shiftY) {
        if (PadInternal?.Items is not { Count: not 0 }) { return null; }
        var l1 = UsedArea.ZoomAndMoveRect(zoom, shiftX, shiftY, false);
        var l2 = PadInternal.Items.MaxBounds(ZoomItems);
        if (l1.Width <= 0 || l2.Height <= 0) { return null; }
        float tZo = 1;
        if (l2 is { Width: > 0, Height: > 0 }) { tZo = Math.Min(l1.Width / l2.Width, l1.Height / l2.Height); }
        PadInternal.Zoom = 1f;

        // Coordinaten auf Maßstab 1/1 scalieren
        var x = (e.X - l1.X) / tZo;
        var y = (e.Y - l1.Y) / tZo;

        // Nullpunkt verschiebung laut Maxbounds
        x += l2.X;
        y += l2.Y;

        // Und noch berücksichtigen, daß das Bild in den Rahmen eingepasst wurde
        x += (l2.Width - (l1.Width / tZo)) / 2;
        y += (l2.Height - (l1.Height / tZo)) / 2;
        x = Math.Min(x, int.MaxValue / 2f);
        y = Math.Min(y, int.MaxValue / 2f);
        x = Math.Max(x, int.MinValue / 2f);
        y = Math.Max(y, int.MinValue / 2f);
        return new MouseEventArgs(e.Button, e.Clicks, (int)x, (int)y, e.Delta);
    }

    #endregion
}