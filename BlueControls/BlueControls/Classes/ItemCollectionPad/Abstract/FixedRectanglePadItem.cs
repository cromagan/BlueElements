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

using BlueControls.EventArgs;
using System.Drawing;

namespace BlueControls.ItemCollectionPad.Abstract;

public abstract class FixedRectanglePadItem : AbstractPadItem {

    #region Fields

    protected Size Size = Size.Empty;

    private readonly PointM _pl;

    /// <summary>
    /// Dieser Punkt bestimmt die ganzen Koordinaten. Die anderen werden nur mitgeschleift
    /// </summary>
    private readonly PointM _pLo;

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

    #region Methods

    public override void InitialPosition(int x, int y, int width, int height) {
        var ua = UsedArea;
        SetLeftTopPoint(x - (ua.Width / 2) + (width / 2), y - (ua.Height / 2) + (height / 2));
    }

    public override void ParseFinished(string parsed) {
        base.ParseFinished(parsed);
        SizeChanged();
    }

    public override void PointMoved(object sender, MoveEventArgs e) {
        var x = 0f;
        var y = 0f;

        var point = (PointM)sender;

        if (point != null) {
            x = point.X;
            y = point.Y;
        }

        if (point == _pLo) {
            _pRu.Y = y + Size.Height;
            _po.Y = y;

            _pRu.X = x + Size.Width;
            _pl.X = x;
        }

        if (point == _pRu) {
            _pLo.X = x - Size.Width;
            _pr.X = x;

            _pLo.Y = y - Size.Height;
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

    public void SetLeftTopPoint(float x, float y) => _pLo.SetTo(x, y);

    public void SizeChanged() {
        // Punkte immer komplett setzen. Um eventuelle Parsing-Fehler auszugleichen
        _pl.SetTo(_pLo.X, _pLo.Y + ((_pLu.Y - _pLo.Y) / 2));
        _pr.SetTo(_pRo.X, _pLo.Y + ((_pLu.Y - _pLo.Y) / 2));
        _pu.SetTo(_pLo.X + ((_pRo.X - _pLo.X) / 2), _pRu.Y);
        _po.SetTo(_pLo.X + ((_pRo.X - _pLo.X) / 2), _pRo.Y);
    }

    protected override RectangleF CalculateUsedArea() {
        if (_pLo == null) { return RectangleF.Empty; }
        return new RectangleF(_pLo.X, _pLo.Y, Size.Width, Size.Height);
    }

    #endregion
}