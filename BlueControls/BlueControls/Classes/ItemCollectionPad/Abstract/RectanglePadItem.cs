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
using BlueControls.EventArgs;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.ItemCollectionPad.Abstract;

public abstract class RectanglePadItem : AbstractPadItem {

    #region Fields

    private readonly PointM _pl;
    private readonly PointM _pLo;
    private readonly PointM _pLu;
    private readonly PointM _po;
    private readonly PointM _pr;
    private readonly PointM _pRo;
    private readonly PointM _pRu;
    private readonly PointM _pu;
    private int _drehwinkel;

    #endregion

    #region Constructors

    protected RectanglePadItem(string keyName) : base(keyName) {
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
            if (IsDisposed) { return; }
            if (_drehwinkel == value) { return; }
            _drehwinkel = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [   .. base.GetProperties(widthOfControl),
            new FlexiControl(),
            new FlexiControlForProperty<int>(() => Drehwinkel),

        ];
        return result;
    }

    public override void InitialPosition(int x, int y, int width, int height) => SetCoordinates(new RectangleF(x, y, width, height), true);

    public override void ParseFinished(string parsed) {
        base.ParseFinished(parsed);
        SizeChanged();
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "fixsize": // TODO: Entfernt am 24.05.2021
                //_größe_fixiert = value.FromPlusMinus();
                return true;

            case "rotation":
                _drehwinkel = Converter.IntParse(value);
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override void PointMoved(object sender, MoveEventArgs e) {


        if (sender is not PointM point) { return; }

        var x = point.X;
        var y = point.Y;

        if (point == _pLo) {
            _po.Y = y; 
            _pl.X = x; 
        }

        if (point == _pRo) {
            _po.Y = y; 
             _pr.X = x; 
        }

        if (point == _pLu) {
            _pl.X = x; 
             _pu.Y = y; 
        }

        if (point == _pRu) {
            _pr.X = x; 
            _pu.Y = y; 
        }

        if (point == _po) {
            _pLo.Y = y;
            _pRo.Y = y;
        }

        if (point == _pu ) {
            _pLu.Y = y;
            _pRu.Y = y;
        }

        if (point == _pl) {
            _pLo.X = x;
            _pLu.X = x;
        }

        if (point == _pr) {
            _pRo.X = x;
            _pRu.X = x;
        }

        SizeChanged();
        base.PointMoved(sender, e);
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

    public override string ToParseableString() {
        if (IsDisposed) { return string.Empty; }
        List<string> result = [];
        result.ParseableAdd("Rotation", Drehwinkel);
        return result.Parseable(base.ToParseableString());
    }

    protected override RectangleF CalculateUsedArea() => new(Math.Min(_pLo.X, _pRu.X),
                                                               Math.Min(_pLo.Y, _pRu.Y),
                                                               Math.Abs(_pRu.X - _pLo.X),
                                                               Math.Abs(_pRu.Y - _pLo.Y));

    #endregion
}