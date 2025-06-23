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
using BlueControls.Controls;
using BlueControls.EventArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using static BlueBasics.Converter;

namespace BlueControls.ItemCollectionPad.Abstract;

public abstract class FixedRectanglePadItem : AbstractPadItem {

    #region Fields

    /// <summary>
    /// Dieser Punkt bestimmt die ganzen Koordinaten. Die anderen werden nur mitgeschleift
    /// </summary>
    protected readonly PointM _pLo;

    /// <summary>
    /// Die fixe Größe in Pixel
    /// </summary>
    protected SizeF _size = SizeF.Empty;

    private readonly PointM _pl;

    private readonly PointM _pLu;

    private readonly PointM _po;

    private readonly PointM _pr;

    private readonly PointM _pRo;

    private readonly PointM _pRu;

    private readonly PointM _pu;

    #endregion

    #region Constructors

    protected FixedRectanglePadItem(string keyName) : base(keyName) {
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
    }

    #endregion

    #region Properties

    [Description("Die Breite des Objekts in mm.")]
    public float Breite {
        get => (float)Math.Round(PixelToMm(_size.Width, ItemCollectionPadItem.Dpi), 2, MidpointRounding.AwayFromZero);
        set {
            if (IsDisposed) { return; }
            if (Math.Abs(Breite - value) < Constants.DefaultTolerance) { return; }
            Size = _size with { Width = MmToPixel(value, ItemCollectionPadItem.Dpi) };
        }
    }

    [Description("Die Höhe des Objekts in mm.")]
    public float Höhe {
        get => (float)Math.Round(PixelToMm(_size.Height, ItemCollectionPadItem.Dpi), 2, MidpointRounding.AwayFromZero);
        set {
            if (IsDisposed) { return; }
            if (Math.Abs(Höhe - value) < Constants.DefaultTolerance) { return; }
            Size = _size with { Height = MmToPixel(value, ItemCollectionPadItem.Dpi) };
        }
    }

    /// <summary>
    /// Die fixe Größe in Pixel
    /// </summary>
    public SizeF Size {
        get => _size;
        set {
            if (Math.Abs(_size.Width - value.Width) < Constants.DefaultTolerance && Math.Abs(_size.Height - value.Height) < Constants.DefaultTolerance) { return; }
            _size = value;
            PointMoved(_pLo, new MoveEventArgs(false));
            OnPropertyChanged();
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

        ];
        return result;
    }

    public override void InitialPosition(int x, int y, int width, int height) {
        var ua = UsedArea;
        SetLeftTopPoint(x - (ua.Width / 2f) + (width / 2f), y - (ua.Height / 2f) + (height / 2f));
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("Size", _size);

        return result;
    }

    public override void ParseFinished(string parsed) {
        base.ParseFinished(parsed);
        SizeChanged();
    }

    public override bool ParseThis(string key, string value) {
        switch (key.ToLowerInvariant()) {
            case "size":
                _size = value.SizeParse();
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
            _pRu.Y = y + _size.Height;
            _po.Y = y;

            _pRu.X = x + _size.Width;
            _pl.X = x;
        }

        if (point == _pRu) {
            _pLo.X = x - _size.Width;
            _pr.X = x;

            _pLo.Y = y - _size.Height;
            _pu.Y = y;
        }

        if (point == _pRo) {
            _po.Y = y;
            _pr.X = x;
        }

        if (point == _pLu) {
            _pl.X = x;
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

        SizeChanged();
        base.PointMoved(sender, e);
    }

    public void SetLeftTopPoint(float x, float y) => _pLo.SetTo(x, y, false);

    public void SizeChanged() {
        // Punkte immer komplett setzen. Um eventuelle Parsing-Fehler auszugleichen
        _pl.SetTo(_pLo.X, _pLo.Y + ((_pLu.Y - _pLo.Y) / 2), false);
        _pr.SetTo(_pRo.X, _pLo.Y + ((_pLu.Y - _pLo.Y) / 2), false);
        _pu.SetTo(_pLo.X + ((_pRo.X - _pLo.X) / 2), _pRu.Y, false);
        _po.SetTo(_pLo.X + ((_pRo.X - _pLo.X) / 2), _pRo.Y, false);
        CalculateJointMiddle(_pl, _pr);
    }

    protected override RectangleF CalculateUsedArea() => new(_pLo.X, _pLo.Y, Size.Width, Size.Height);

    #endregion
}