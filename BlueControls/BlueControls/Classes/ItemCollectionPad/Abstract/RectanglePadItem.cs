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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using static BlueBasics.Converter;

namespace BlueControls.ItemCollectionPad.Abstract;

public abstract class RectanglePadItem : AbstractPadItem, IMirrorable {

    #region Fields

    protected readonly PointM _pl;
    protected readonly PointM _pLo;
    protected readonly PointM _pLu;
    protected readonly PointM _pr;
    protected readonly PointM _pRu;
    private readonly PointM _po;
    private readonly PointM _pRo;
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

        _po.MoveXByMouse = false;
        _pu.MoveXByMouse = false;
        _pl.MoveYByMouse = false;
        _pr.MoveYByMouse = false;

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
        CalculateJointMiddle(_pl, _pr);
        Drehwinkel = 0;
    }

    #endregion

    #region Properties

    [Description("Die Breite des Objekts in mm.")]
    public virtual float Breite {
        get => (float)Math.Round(PixelToMm(UsedArea.Width, ItemCollectionPadItem.Dpi), 2, MidpointRounding.AwayFromZero);
        set {
            if (IsDisposed) { return; }
            if (Math.Abs(Breite - value) < Constants.DefaultTolerance) { return; }
            _pRu.X = _pLo.X + MmToPixel(value, ItemCollectionPadItem.Dpi);
        }
    }

    public int Drehwinkel {
        get => _drehwinkel;
        set {
            if (IsDisposed) { return; }
            if (_drehwinkel == value) { return; }
            _drehwinkel = value;
            OnPropertyChanged();
        }
    }

    [Description("Die Höhe des Objekts in mm.")]
    public virtual float Höhe {
        get => (float)Math.Round(PixelToMm(UsedArea.Height, ItemCollectionPadItem.Dpi), 2, MidpointRounding.AwayFromZero);
        set {
            if (IsDisposed) { return; }
            if (Math.Abs(Höhe - value) < Constants.DefaultTolerance) { return; }
            _pRu.Y = _pLo.Y + MmToPixel(value, ItemCollectionPadItem.Dpi);
        }
    }

    #endregion

    #region Methods

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [   .. base.GetProperties(widthOfControl),
            new FlexiControl(),
            new FlexiControlForProperty<float>(() => Breite),
            new FlexiControlForProperty<float>(() => Höhe),
            new FlexiControlForProperty<int>(() => Drehwinkel),

        ];
        return result;
    }

    public override void InitialPosition(int x, int y, int width, int height) => SetCoordinates(new RectangleF(x, y, width, height));

    public virtual void Mirror(PointM? p, bool vertical, bool horizontal) {
        p ??= new PointM(JointMiddle);

        foreach (var thisP in JointPoints) {
            thisP.Mirror(p, vertical, horizontal);
            DoJointPoint(thisP);
        }

        _pLo.Mirror(p, vertical, horizontal);
        _pRu.Mirror(p, vertical, horizontal);
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("Rotation", Drehwinkel);
        return result;
    }

    public override void ParseFinished(string parsed) {
        base.ParseFinished(parsed);
        CalculateSlavePoints();
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "fixsize": // TODO: Entfernt am 24.05.2021
                //_größe_fixiert = value.FromPlusMinus();
                return true;

            case "rotation":
                _drehwinkel = IntParse(value);
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override void PointMoved(object sender, MoveEventArgs e) {
        if (sender is not PointM point) { return; }
        if (JointPoints.Contains(point)) {
            base.PointMoved(sender, e);
            return;
        }

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

        if (point == _pu) {
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

        CalculateSlavePoints();
        base.PointMoved(sender, e);
    }

    public void SetCoordinates(RectangleF r) {
        _pLo.SetTo(r.PointOf(Alignment.Top_Left), false);
        _pRu.SetTo(r.PointOf(Alignment.Bottom_Right), false);
    }

    protected override RectangleF CalculateUsedArea() => new(Math.Min(_pLo.X, _pRu.X),
                                                               Math.Min(_pLo.Y, _pRu.Y),
                                                               Math.Abs(_pRu.X - _pLo.X),
                                                               Math.Abs(_pRu.Y - _pLo.Y));

    private void CalculateSlavePoints() {
        // Punkte immer komplett setzen. Um eventuelle Parsing-Fehler auszugleichen
        _pl.SetTo(_pLo.X, _pLo.Y + ((_pLu.Y - _pLo.Y) / 2), false);
        _pr.SetTo(_pRo.X, _pLo.Y + ((_pLu.Y - _pLo.Y) / 2), false);
        _pu.SetTo(_pLo.X + ((_pRo.X - _pLo.X) / 2), _pRu.Y, false);
        _po.SetTo(_pLo.X + ((_pRo.X - _pLo.X) / 2), _pRo.Y, false);
        CalculateJointMiddle(_pl, _pr);
    }

    #endregion
}