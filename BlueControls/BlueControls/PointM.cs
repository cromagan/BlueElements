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
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using static BlueBasics.Converter;
using System;
using System.Drawing;
using static BlueBasics.Geometry;

namespace BlueControls {

    public sealed class PointM : IMoveable {

        #region Fields

        private float _x;
        private float _y;

        #endregion

        #region Constructors

        public PointM(object? parent, string name, float startX, float startY, float laenge, float alpha) : this(parent) {
            Name = name;
            var tempVar = PolarToCartesian(laenge, alpha);
            _x = startX + tempVar.X;
            _y = startY + tempVar.Y;
            Tag = string.Empty;
        }

        public PointM(PointM startPoint, float laenge, float alpha) : this(null, string.Empty, startPoint.X, startPoint.Y, laenge, alpha) { }

        public PointM(PointF startPoint, float laenge, float alpha) : this(null, string.Empty, startPoint.X, startPoint.Y, laenge, alpha) { }

        public PointM(object? parent, string codeToParse) : this(parent) => Parse(codeToParse);

        public PointM(object? parent, string name, float x, float y, string tag) {
            Parent = parent;
            _x = x;
            _y = y;
            Name = name;
            Tag = tag;
        }

        public PointM() : this(null, string.Empty, 0f, 0f, string.Empty) { }

        public PointM(object? parent) : this(parent, string.Empty, 0f, 0f, string.Empty) { }

        public PointM(string name, float x, float y) : this(null, name, x, y, string.Empty) { }

        public PointM(object? parent, string name, int x, int y) : this(parent, name, x, y, string.Empty) { }

        public PointM(object parent, string name, float x, float y) : this(parent, name, x, y, string.Empty) { }

        public PointM(PointF point) : this(null, string.Empty, point.X, point.Y, string.Empty) { }

        public PointM(int x, int y) : this(null, string.Empty, x, y, string.Empty) { }

        public PointM(float x, float y) : this(null, string.Empty, x, y, string.Empty) { }

        public PointM(PointM point) : this(null, string.Empty, point.X, point.Y, string.Empty) { }

        public PointM(object parent, PointM template) : this(parent, template.Name, template.X, template.Y, template.Tag) { }

        #endregion

        #region Events

        public event EventHandler<MoveEventArgs> Moved;

        public event EventHandler<MoveEventArgs> Moving;

        #endregion

        #region Properties

        public float Magnitude => (float)Math.Sqrt((_x * _x) + (_y * _y));
        public string Name { get; private set; }
        public object? Parent { get; set; }

        public string Tag { get; set; }

        public float X {
            get => _x;
            set {
                if (_x == value) { return; }
                SetTo(value, _y);
            }
        }

        public float Y {
            get => _y;
            set {
                if (_y == value) { return; }
                SetTo(_x, value);
            }
        }

        #endregion

        #region Methods

        public static PointM Empty() => new(0f, 0f);

        public static implicit operator Point(PointM p) => new((int)p.X, (int)p.Y);

        public static implicit operator PointF(PointM p) => new(p.X, p.Y);

        public static PointM operator -(PointM? a) => new(-a._x, -a._y);

        public static PointM operator -(PointM? a, PointM? b) => new(a._x - b._x, a._y - b._y);

        public static PointM operator *(PointM? a, float b) => new(a._x * b, a._y * b);

        public static PointM operator +(PointM a, PointM b) => new(a._x + b._x, a._y + b._y);

        public float DistanzZuLinie(PointM p1, PointM p2) => DistanzZuLinie(p1.X, p1.Y, p2.X, p2.Y);

        public float DistanzZuLinie(float x1, float y1, float x2, float y2) => Länge(this, PointOnLine(this, x1, y1, x2, y2));

        public float DotProduct(PointM vector) => (_x * vector._x) + (_y * vector._y);

        public void Draw(Graphics gr, float zoom, float shiftX, float shiftY, enDesign type, enStates state) {
            var tx = (_x * zoom) - shiftX + (zoom / 2);
            var ty = (_y * zoom) - shiftY + (zoom / 2);
            Rectangle r = new((int)(tx - 4), (int)(ty - 4), 9, 9);
            //if (!_UserSelectable) {
            //    type = enDesign.Button_EckpunktSchieber_Phantom;
            //    state = enStates.Standard;
            //}
            Skin.Draw_Back(gr, type, state, r, null, false);
            Skin.Draw_Border(gr, type, state, r);
            //if (!string.IsNullOrEmpty(textToDraw)) {
            //    for (var x = -1; x < 2; x++) {
            //        for (var y = -1; y < 2; y++) {
            //            BlueFont.DrawString(GR,textToDraw, SimpleArial, Brushes.White, (float)tx + x, (float)ty + y - 16);
            //        }
            //    }
            //    BlueFont.DrawString(GR,textToDraw, SimpleArial, Brushes.Black, (float)tx, (float)ty - 16);
            //}
        }

        public void Move(float x, float y) => SetTo(_x + x, _y + y);

        public void Normalize() {
            var magnitude = Magnitude;
            SetTo(_x / magnitude, _y / magnitude);
        }

        public void OnMoved(MoveEventArgs e) => Moved?.Invoke(this, e);

        public void OnMoving(MoveEventArgs e) => Moving?.Invoke(this, e);

        public void Parse(string codeToParse) {
            foreach (var pair in codeToParse.GetAllTags()) {
                switch (pair.Key) {
                    case "parentname":
                        break;

                    case "name":
                        Name = pair.Value.FromNonCritical();
                        break;

                    case "tag":
                        Tag = pair.Value.FromNonCritical();
                        break;

                    case "x":
                        _x = FloatParse(pair.Value);
                        break;

                    case "y":
                        _y = FloatParse(pair.Value);
                        break;

                    case "fix": // TODO: Entfernt, 24.05.2021
                        break;

                    case "moveable": // TODO: Entfernt, 24.05.2021
                        break;

                    case "getsnapped": // TODO: Entfernt, 24.05.2021
                        break;

                    case "primarygridsnappoint": // TODO: Entfernt, 24.05.2021
                        break;

                    default:
                        Develop.DebugPrint(enFehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                        break;
                }
            }
        }

        public void SetTo(float x, float y) {
            var mx = x != _x;
            var my = y != _y;

            if (!mx && !my) { return; }
            OnMoving(new MoveEventArgs(mx, my));
            _x = x;
            _y = y;
            OnMoved(new MoveEventArgs(mx, my));
        }

        public void SetTo(PointM? startPoint, float länge, float alpha) {
            var tempVar = PolarToCartesian(länge, alpha);
            SetTo(startPoint.X + tempVar.X, Y = startPoint.Y + tempVar.Y);
        }

        public void SetTo(PointF point) => SetTo(point.X, point.Y);

        public void SetTo(PointM point) => SetTo(point.X, point.Y);

        public void SetTo(Point point) => SetTo(point.X, (float)point.Y);

        public void SetTo(int x, int y) => SetTo(x, (float)y);

        public override string ToString() {
            var t = "{";
            if (Parent != null) {
                switch (Parent) {
                    case BasicPadItem item:
                        t = t + "ParentName=" + item.Internal.ToNonCritical() + ", ";
                        break;

                    case CreativePad:
                        t += "ParentType=Main, ";
                        break;

                    case ItemCollectionPad:
                        t += "ParentType=Main, ";
                        break;

                    default:
                        t = t + "ParentType=" + Parent.GetType().FullName + ", ";
                        break;
                }
            }
            t = t + "Name=" + Name.ToNonCritical() + ", ";
            t = t + "X=" + _x + ", ";
            t = t + "Y=" + _y + ", ";
            if (!string.IsNullOrEmpty(Tag)) { t = t + "Tag=" + Tag.ToNonCritical() + ", "; }
            return t.Trim(", ") + "}";
        }

        public PointF ZoomAndMove(AdditionalDrawing e) => ZoomAndMove(e.Zoom, e.ShiftX, e.ShiftY);

        public PointF ZoomAndMove(float zoom, float shiftX, float shiftY) =>
            new((_x * zoom) - shiftX + (zoom / 2), (_y * zoom) - shiftY + (zoom / 2));

        #endregion
    }
}