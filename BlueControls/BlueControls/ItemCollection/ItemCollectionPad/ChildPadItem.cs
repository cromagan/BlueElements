// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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
using BlueScript.Variables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using static BlueBasics.Converter;

namespace BlueControls.ItemCollection;

public class ChildPadItem : RectanglePadItem, IMouseAndKeyHandle, ICanHaveVariablesItemLevel {

    #region Fields

    public string Seite;
    public List<BasicPadItem>? ZoomItems;
    private string _name;
    private CreativePad? _padInternal;
    private Bitmap? _tmpBmp;

    #endregion

    #region Constructors

    public ChildPadItem() : this(string.Empty) { }

    public ChildPadItem(string internalname) : base(internalname) {
        PadInternal = null; // new CreativePad();
        Seite = string.Empty;
        ZoomItems = null;
        _name = string.Empty;
        Textlage = (Alignment)(-1);
        Randfarbe = Color.Transparent;
        Eingebettete_Ansichten = new List<string>();
    }

    #endregion

    #region Properties

    [Description("Soll eine Umrandung einer anderen Ansicht hier angezeigt werden,<br>muss dessen Name hier eingegeben werden.")]
    public List<string> Eingebettete_Ansichten { get; set; }

    [Description("Name und gleichzeitig eventuelle Beschriftung dieser Ansicht.")]
    public string Name {
        get => _name;
        set {
            if (value == _name) { return; }
            _name = value;
            OnChanged();
        }
    }

    public CreativePad? PadInternal {
        get => _padInternal;
        set {
            if (_padInternal?.Item != null) {
                _padInternal.Item.Changed -= _Pad_DoInvalidate;
            }
            _padInternal = value;
            if (_padInternal?.Item != null) {
                _padInternal.Item.Changed += _Pad_DoInvalidate;
            }
        }
    }

    public Color Randfarbe { get; set; }
    public Alignment Textlage { get; set; }
    protected override int SaveOrder => 1000;

    #endregion

    #region Methods

    public override List<GenericControl> GetStyleOptions() {
        List<GenericControl> l = new()
        {
            new FlexiControlForProperty<string>(() => Name),
            new FlexiControlForProperty<Color>(() => Randfarbe)
        };
        ItemCollectionList.ItemCollectionList lage = new(false)
        {
            { "ohne", "-1" },
            { "Links oben", ((int)Alignment.Top_Left).ToString() }
        };
        l.Add(new FlexiControlForProperty<Alignment>(() => Textlage, lage));
        l.Add(new FlexiControlForProperty<List<string>>(() => Eingebettete_Ansichten, 5));
        l.AddRange(base.GetStyleOptions());
        return l;
    }

    public bool KeyUp(object sender, System.Windows.Forms.KeyEventArgs e, float cZoom, float shiftX, float shiftY) {
        if (PadInternal?.Item == null || PadInternal.Item.Count == 0) { return false; }
        PadInternal.DoKeyUp(e, false);
        return true;
    }

    public bool MouseDown(object sender, System.Windows.Forms.MouseEventArgs e, float cZoom, float shiftX, float shiftY) {
        if (PadInternal?.Item == null || PadInternal.Item.Count == 0) { return false; }
        var l1 = UsedArea.ZoomAndMoveRect(cZoom, shiftX, shiftY, false);
        var l2 = PadInternal.Item.MaxBounds(ZoomItems);
        if (l1.Width <= 0 || l2.Height <= 0) { return false; }
        var tZo = Math.Min(l1.Width / l2.Width, l1.Height / l2.Height);
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
        System.Windows.Forms.MouseEventArgs e2 = new(e.Button, e.Clicks, (int)x, (int)y, e.Delta);
        PadInternal.DoMouseDown(e2);
        return true;
    }

    public bool MouseMove(object sender, System.Windows.Forms.MouseEventArgs e, float zoom, float shiftX, float shiftY) {
        if (PadInternal?.Item == null || PadInternal.Item.Count == 0) { return false; }
        var l1 = UsedArea.ZoomAndMoveRect(zoom, shiftX, shiftY, false);
        var l2 = PadInternal.Item.MaxBounds(ZoomItems);
        if (l1.Width <= 0 || l2.Height <= 0) { return false; }
        float tZo = 1;
        if (l2.Width > 0 && l2.Height > 0) { tZo = Math.Min(l1.Width / l2.Width, l1.Height / l2.Height); }
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
        System.Windows.Forms.MouseEventArgs e2 = new(e.Button, e.Clicks, (int)x, (int)y, e.Delta);
        PadInternal.DoMouseMove(e2);
        return true;
    }

    public bool MouseUp(object sender, System.Windows.Forms.MouseEventArgs e, float zoom, float shiftX, float shiftY) {
        if (PadInternal?.Item == null || PadInternal.Item.Count == 0) { return false; }
        var l1 = UsedArea.ZoomAndMoveRect(zoom, shiftX, shiftY, false);
        var l2 = PadInternal.Item.MaxBounds(ZoomItems);
        if (l1.Width <= 0 || l2.Height <= 0) { return false; }
        var tZo = Math.Min(l1.Width / l2.Width, l1.Height / l2.Height);
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
        System.Windows.Forms.MouseEventArgs e2 = new(e.Button, e.Clicks, (int)x, (int)y, e.Delta);
        PadInternal.DoMouseUp(e2);
        return true;
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }
        switch (tag) {
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
                PadInternal = new CreativePad(new ItemCollectionPad(value, string.Empty));
                return true;

            case "checked":
                return true;

            case "embedded":
                Eingebettete_Ansichten = value.FromNonCritical().SplitAndCutByCrToList();
                return true;

            case "color":
                Randfarbe = value.FromHtmlCode();
                return true;

            case "pos":
                Textlage = (Alignment)IntParse(value);
                return true;
        }
        return false;
    }

    public override void ProcessStyleChange() {
        if (PadInternal?.Item == null || Parent == null) { return; }

        RemovePic();
        PadInternal.Item.SheetStyle = Parent.SheetStyle;
        PadInternal.Item.SheetStyleScale = Parent.SheetStyleScale;
    }

    public bool ReplaceVariable(Variable variable) {
        if (PadInternal?.Item == null) { return false; }
        var b = PadInternal.Item.ParseVariable(variable);
        if (b) { OnChanged(); }
        return b;
    }

    public bool ResetVariables() {
        if (PadInternal?.Item == null) { return false; }
        var b = PadInternal.Item.ResetVariables();
        if (b) { OnChanged(); }
        return b;
    }

    public override string ToString() {
        var result = new List<string>();

        result.ParseableAdd("Name", _name);

        if (Textlage != (Alignment)(-1)) { result.ParseableAdd("Pos", Textlage); }
        result.ParseableAdd("Embedded", Eingebettete_Ansichten);
        result.ParseableAdd("Color", Randfarbe);
        result.ParseableAdd("Data", "Item", PadInternal?.Item);

        return result.Parseable(base.ToString());
    }

    protected override string ClassId() => "CHILDPAD";

    protected override void DrawExplicit(Graphics gr, RectangleF positionModified, float zoom, float shiftX, float shiftY, bool forPrinting) {
        if (PadInternal?.Item == null || Parent == null) { return; }

        try {
            var trp = positionModified.PointOf(Alignment.Horizontal_Vertical_Center);
            gr.TranslateTransform(trp.X, trp.Y);
            gr.RotateTransform(-Drehwinkel);
            Font font = new("Arial", 30 * zoom);
            if (PadInternal?.Item != null) {
                PadInternal.Item.SheetStyle = Parent.SheetStyle;
                PadInternal.Item.SheetStyleScale = Parent.SheetStyleScale;
                if (_tmpBmp != null) {
                    if (_tmpBmp.Width != positionModified.Width || positionModified.Height != _tmpBmp.Height) {
                        _tmpBmp?.Dispose();
                        RemovePic();
                        Generic.CollectGarbage();
                    }
                }
                if (positionModified.Width < 1 || positionModified.Height < 1 || positionModified.Width > 20000 || positionModified.Height > 20000) { return; }
                _tmpBmp ??= new Bitmap((int)Math.Abs(positionModified.Width), (int)Math.Abs(positionModified.Height));
                var mb = PadInternal.Item.MaxBounds(ZoomItems);
                var zoomv = ItemCollectionPad.ZoomFitValue(mb, _tmpBmp.Size);
                var centerpos = ItemCollectionPad.CenterPos(mb, _tmpBmp.Size, zoomv);
                var slidervalues = ItemCollectionPad.SliderValues(mb, zoomv, centerpos);
                PadInternal.ShowInPrintMode = forPrinting;
                if (forPrinting) { PadInternal.Unselect(); }
                PadInternal.Item.DrawCreativePadToBitmap(_tmpBmp, States.Standard, zoomv, slidervalues.X, slidervalues.Y, Seite);
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
                        if (pad?.PadInternal?.Item != null) {
                            var mb2 = pad.PadInternal.Item.MaxBounds(pad.ZoomItems);
                            mb2.Inflate(-1, -1);
                            var tmpG = Graphics.FromImage(_tmpBmp);
                            Pen p = new(pad.Randfarbe, (float)(8.7d * zoom));
                            Pen p2 = new(Color.White, (float)(8.7d * zoom) + 2f);
                            p.DashPattern = new float[] { 5, 1, 1, 1 };
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
                    DashPattern = new float[] { 10, 2, 1, 2 }
                };
                gr.DrawRectangle(p, positionModified);
                var s = gr.MeasureString(Name, font);
                BlueFont.DrawString(gr, Name, font, new SolidBrush(Randfarbe), positionModified.Left, positionModified.Top - s.Height - (9f * zoom));
            }
        } catch {
        }
        base.DrawExplicit(gr, positionModified, zoom, shiftX, shiftY, forPrinting);
    }

    protected override BasicPadItem? TryCreate(string id, string name) {
        if (id.Equals("blueelements.clsitempad", StringComparison.OrdinalIgnoreCase) ||
            id.Equals("blueelements.itempad", StringComparison.OrdinalIgnoreCase) ||
            id.Equals(ClassId(), StringComparison.OrdinalIgnoreCase)) {
            return new ChildPadItem(name);
        }
        return null;
    }

    private void _Pad_DoInvalidate(object sender, System.EventArgs e) {
        if (IsParsing) { return; }
        OnChanged();
    }

    private void RemovePic() {
        if (_tmpBmp != null) {
            _tmpBmp?.Dispose();
            _tmpBmp = null;
        }
    }

    #endregion
}