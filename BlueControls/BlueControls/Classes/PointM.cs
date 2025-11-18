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
using BlueBasics.Interfaces;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using static BlueBasics.Constants;
using static BlueBasics.Converter;
using static BlueBasics.Geometry;

namespace BlueControls;

public sealed class PointM : IMoveable, IHasKeyName, IParseable, INotifyPropertyChanged, IMirrorable {

    #region Fields

    /// <summary>
    /// Wert wird nur gespeichert, für evtl. erweiterte Berechnungen
    /// </summary>
    private float _angle;

    /// <summary>
    /// Wert wird nur gespeichert, für evtl. erweiterte Berechnungen
    /// </summary>
    private float _distance;

    private object? _parent;
    private float _x;
    private float _y;

    #endregion

    #region Constructors

    public PointM(object? parent, string name, float startX, float startY, float laenge, float alpha) : this(parent) {
        KeyName = name;
        var tempVar = PolarToCartesian(laenge, alpha);
        _x = startX + tempVar.X;
        _y = startY + tempVar.Y;
        //Tag = string.Empty;
    }

    public PointM(PointM startPoint, float laenge, float alpha) : this(null, string.Empty, startPoint.X, startPoint.Y, laenge, alpha) { }

    public PointM(PointF startPoint, float laenge, float alpha) : this(null, string.Empty, startPoint.X, startPoint.Y, laenge, alpha) { }

    public PointM(object? parent, string toParse) : this(parent) => this.Parse(toParse);

    public PointM(object? parent, string name, float x, float y) {
        _parent = parent;
        _x = x;
        _y = y;
        KeyName = name;
    }

    public PointM() : this(null, string.Empty, 0f, 0f) { }

    public PointM(object? parent) : this(parent, string.Empty, 0f, 0f) { }

    public PointM(string name, float x, float y) : this(null, name, x, y) { }

    public PointM(object? parent, string name, int x, int y) : this(parent, name, x, (float)y) { }

    public PointM(PointF point) : this(null, string.Empty, point.X, point.Y) { }

    public PointM(int x, int y) : this(null, string.Empty, x, (float)y) { }

    public PointM(float x, float y) : this(null, string.Empty, x, y) { }

    public PointM(PointM point) : this(null, string.Empty, point.X, point.Y) { }

    public PointM(object parent, PointM template) : this(parent, template.KeyName, template.X, template.Y) { }

    #endregion

    #region Events

    public event EventHandler<MoveEventArgs>? Moved;

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Properties

    /// <summary>
    /// Wert wird nur gespeichert, für evtl. erweiterte Berechnungen
    /// </summary>
    public float Angle {
        get => _angle;
        set {
            if (Math.Abs(_angle - value) < DefaultTolerance) { return; }
            _angle = value;
        }
    }

    /// <summary>
    /// Wert wird nur gespeichert, für evtl. erweiterte Berechnungen
    /// </summary>
    public float Distance {
        get => _distance;
        set {
            if (Math.Abs(_distance - value) < DefaultTolerance) { return; }
            _distance = value;
        }
    }

    public bool KeyIsCaseSensitive => false;
    public string KeyName { get; set; }
    public float Magnitude => (float)Math.Sqrt((_x * _x) + (_y * _y));
    public bool MoveXByMouse { get; set; } = true;
    public bool MoveYByMouse { get; set; } = true;

    public object? Parent {
        get => _parent;
        set {
            if (_parent == value) { return; }
            _parent = value;
            OnPropertyChanged();
        }
    }

    public float X {
        get => _x;
        set {
            if (Math.Abs(_x - value) < DefaultTolerance) { return; }
            SetTo(value, _y, false);
        }
    }

    public float Y {
        get => _y;
        set {
            if (Math.Abs(_y - value) < DefaultTolerance) { return; }
            SetTo(_x, value, false);
        }
    }

    #endregion

    #region Methods

    public static PointM Empty() => new(0f, 0f);

    public static explicit operator Point(PointM p) => new((int)p.X, (int)p.Y);

    public static implicit operator PointF(PointM p) => new(p.X, p.Y);

    public static PointM operator -(PointM a) => new(-a._x, -a._y);

    public static PointM operator -(PointM a, PointM b) => new(a._x - b._x, a._y - b._y);

    public static PointM operator *(PointM a, float b) => new(a._x * b, a._y * b);

    public static PointM operator +(PointM a, PointM b) => new(a._x + b._x, a._y + b._y);

    public float DistanzZuLinie(PointM p1, PointM p2) => DistanzZuLinie(p1.X, p1.Y, p2.X, p2.Y);

    public float DistanzZuLinie(float x1, float y1, float x2, float y2) => GetLength(this, PointOnLine(this, x1, y1, x2, y2));

    public float DotProduct(PointM vector) => (_x * vector._x) + (_y * vector._y);

    public void Draw(Graphics gr, float zoom, float shiftX, float shiftY, Design type, States state) {
        var t = ZoomAndMove(zoom, shiftX, shiftY);
        Rectangle r = new((int)(t.X - 4), (int)(t.Y - 4), 9, 9);
        Skin.Draw_Back(gr, type, state, r, null, false);
        Skin.Draw_Border(gr, type, state, r);
    }

    public void Mirror(PointM? p, bool vertical, bool horizontal) {
        if (p == null) { return; }

        if (vertical) { X -= (X - p.X) * 2; }
        if (horizontal) { Y -= (Y - p.Y) * 2; }
    }

    public void Move(float x, float y, bool isMouse) => SetTo(_x + x, _y + y, isMouse);

    public void Normalize() {
        var magnitude = Magnitude;
        SetTo(_x / magnitude, _y / magnitude, false);
    }

    public void OnMoved(MoveEventArgs e) => Moved?.Invoke(this, e);

    public List<string> ParseableItems() {
        List<string> result = [];

        if (_parent != null) {
            switch (_parent) {
                case IHasKeyName item:
                    result.ParseableAdd("ParentName", item.KeyName);
                    break;

                //case ItemCollectionPadItem:
                //case CreativePad:
                //    result.ParseableAdd("ParentType", "Main");
                //    break;

                default:
                    result.ParseableAdd("ParentType", _parent.GetType().FullName);
                    break;
            }
        }
        result.ParseableAdd("Name", KeyName);
        result.ParseableAdd("X", _x);
        result.ParseableAdd("Y", _y);
        result.ParseableAdd("Distance", _distance);
        result.ParseableAdd("Angle", _angle);
        //result.ParseableAdd("Tag", _tag);
        return result;
    }

    public void ParseFinished(string parsed) { }

    public bool ParseThis(string key, string value) {
        switch (key) {
            case "parentname":
                return true;

            case "name":
                KeyName = value.FromNonCritical();
                return true;

            case "tag":
                //_tag = value.FromNonCritical(); // TODO: 13.09.2024
                return true;

            case "x":
                _x = FloatParse(value);
                return true;

            case "y":
                _y = FloatParse(value);
                return true;

            case "distance":
                _distance = FloatParse(value);
                return true;

            case "angle":
                _angle = FloatParse(value);
                return true;

            case "fix": // TODO: Entfernt, 24.05.2021
                return true;

            case "moveable": // TODO: Entfernt, 24.05.2021
                return true;

            case "getsnapped": // TODO: Entfernt, 24.05.2021
                return true;

            case "primarygridsnappoint": // TODO: Entfernt, 24.05.2021
                return true;
        }

        return false;
    }

    public void SetTo(float x, float y, bool byMouse) {
        var mx = (float)Math.Round(x - _x, 6);
        var my = (float)Math.Round(y - _y, 6);

        if (mx == 0 && my == 0) { return; }
        _x = x;
        _y = y;
        OnMoved(new MoveEventArgs(byMouse));
        OnPropertyChanged("Position");
    }

    public void SetTo(PointM startPoint, float länge, float alpha, bool byMouse) {
        var tempVar = PolarToCartesian(länge, alpha);
        SetTo(startPoint.X + tempVar.X, Y = startPoint.Y + tempVar.Y, byMouse);
    }

    public void SetTo(PointF point, bool byMouse) => SetTo(point.X, point.Y, byMouse);

    public void SetTo(PointM point, bool byMouse) => SetTo(point.X, point.Y, byMouse);

    public void SetTo(Point point, bool byMouse) => SetTo(point.X, (float)point.Y, byMouse);

    public void SetTo(int x, int y, bool byMouse) => SetTo(x, (float)y, byMouse);

    public override string ToString() => ParseableItems().FinishParseable();

    public PointF ZoomAndMove(AdditionalDrawing e) => ZoomAndMove(e.Zoom, e.ShiftX, e.ShiftY);

    public PointF ZoomAndMove(float zoom, float shiftX, float shiftY) =>
        new((_x * zoom) - shiftX + (zoom / 2), (_y * zoom) - shiftY + (zoom / 2));

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "unknown") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion
}