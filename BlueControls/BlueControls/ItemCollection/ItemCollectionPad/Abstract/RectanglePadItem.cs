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
using BlueControls.EventArgs;
using System;
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.Converter;

namespace BlueControls.ItemCollection;

public abstract class RectanglePadItem : BasicPadItem {

    #region Fields

    private readonly PointM? _pl;
    private readonly PointM? _pLo;
    private readonly PointM? _pLu;
    private readonly PointM? _po;
    private readonly PointM? _pr;
    private readonly PointM? _pRo;
    private readonly PointM? _pRu;
    private readonly PointM? _pu;
    private int _drehwinkel;

    #endregion

    #region Constructors

    protected RectanglePadItem(string internalname) : base(internalname) {
        _pLo = new PointM(this, "LO", 0, 0);
        _pRo = new PointM(this, "RO", 0, 0);
        _pRu = new PointM(this, "RU", 0, 0);
        _pLu = new PointM(this, "LU", 0, 0);
        _pl = new PointM(this, "L", 0, 0);
        _pr = new PointM(this, "R", 0, 0);
        _po = new PointM(this, "O", 0, 0);
        _pu = new PointM(this, "U", 0, 0);
        MovablePoint.Add(_pLo);
        MovablePoint.Add(_pRo);
        MovablePoint.Add(_pLu);
        MovablePoint.Add(_pRu);
        MovablePoint.Add(_pl);
        MovablePoint.Add(_pr);
        MovablePoint.Add(_pu);
        MovablePoint.Add(_po);
        PointsForSuccesfullyMove.Add(_pLo);
        PointsForSuccesfullyMove.Add(_pRu);
        Drehwinkel = 0;
    }

    #endregion

    #region Properties

    public int Drehwinkel {
        get => _drehwinkel;
        set {
            if (_drehwinkel == value) { return; }
            _drehwinkel = value;
            OnChanged();
        }
    }

    #endregion

    #region Methods

    public override List<FlexiControl> GetStyleOptions() {
        List<FlexiControl> l = new()
        {
            new FlexiControl(),
            new FlexiControlForProperty<int>(() => Drehwinkel)
        };
        l.AddRange(base.GetStyleOptions());
        return l;
    }

    public override void InitialPosition(int x, int y, int width, int height) {
        SetCoordinates(new RectangleF(x, y, width, height), true);
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }
        switch (tag) {
            case "fixsize": // TODO: Entfernt am 24.05.2021
                //_größe_fixiert = value.FromPlusMinus();
                return true;

            case "rotation":
                _drehwinkel = IntParse(value);
                return true;
        }
        return false;
    }

    public override void PointMoved(object sender, MoveEventArgs e) {
        base.PointMoved(sender, e);
        var x = 0f;
        var y = 0f;

        var point = (PointM)sender;

        if (point != null) {
            x = point.X;
            y = point.Y;
        }

        if (point == _pLo) {
            if (e.Y) { _po.Y = y; }
            if (e.X) { _pl.X = x; }
        }

        if (point == _pRo) {
            if (e.Y) { _po.Y = y; }
            if (e.X) { _pr.X = x; }
        }

        if (point == _pLu) {
            if (e.X) { _pl.X = x; }
            if (e.Y) { _pu.Y = y; }
        }

        if (point == _pRu) {
            if (e.X) { _pr.X = x; }
            if (e.Y) { _pu.Y = y; }
        }

        if (point == _po && e.Y) {
            _pLo.Y = y;
            _pRo.Y = y;
        }

        if (point == _pu && e.Y) {
            _pLu.Y = y;
            _pRu.Y = y;
        }

        if (point == _pl && e.X) {
            _pLo.X = x;
            _pLu.X = x;
        }

        if (point == _pr && e.X) {
            _pRo.X = x;
            _pRu.X = x;
        }

        SizeChanged();
    }

    public void SetCoordinates(RectangleF r, bool overrideFixedSize) {
        if (!overrideFixedSize) {
            var vr = r.PointOf(Alignment.Horizontal_Vertical_Center);
            var ur = UsedArea;
            _pLo.SetTo(vr.X - (ur.Width / 2), vr.Y - (ur.Height / 2));
            _pRu.SetTo(_pLo.X + ur.Width, _pLo.Y + ur.Height);
        } else {
            _pLo.SetTo(r.PointOf(Alignment.Top_Left));
            _pRu.SetTo(r.PointOf(Alignment.Bottom_Right));
        }
    }

    public virtual void SizeChanged() {
        // Punkte immer komplett setzen. Um eventuelle Parsing-Fehler auszugleichen
        _pl.SetTo(_pLo.X, _pLo.Y + ((_pLu.Y - _pLo.Y) / 2));
        _pr.SetTo(_pRo.X, _pLo.Y + ((_pLu.Y - _pLo.Y) / 2));
        _pu.SetTo(_pLo.X + ((_pRo.X - _pLo.X) / 2), _pRu.Y);
        _po.SetTo(_pLo.X + ((_pRo.X - _pLo.X) / 2), _pRo.Y);
    }

    public override string ToString() {
        var t = base.ToString();
        t = t.Substring(0, t.Length - 1) + ", ";
        if (Drehwinkel != 0) { t = t + "Rotation=" + Drehwinkel + ", "; }
        return t.Trim(", ") + "}";
    }

    protected override RectangleF CalculateUsedArea() => _pLo == null || _pRu == null ? RectangleF.Empty
        : new RectangleF(Math.Min(_pLo.X, _pRu.X), Math.Min(_pLo.Y, _pRu.Y), Math.Abs(_pRu.X - _pLo.X), Math.Abs(_pRu.Y - _pLo.Y));

    protected override void ParseFinished() => SizeChanged();

    #endregion
}