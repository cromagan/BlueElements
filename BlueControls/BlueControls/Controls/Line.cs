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

using System.ComponentModel;
using System.Drawing;
using BlueBasics.Enums;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.Interfaces;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public class Line : GenericControl, IBackgroundNone {

    #region Fields

    private Orientation _orientation = Orientation.Waagerecht;

    #endregion

    #region Constructors

    public Line() : base(false, false) {
        // Dieser Aufruf ist für den Designer erforderlich.
        //  InitializeComponent()
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        SetNotFocusable();
        MouseHighlight = false;
    }

    #endregion

    #region Properties

    [DefaultValue(Orientation.Waagerecht)]
    public Orientation Orientation {
        get => _orientation;
        set {
            if (value == _orientation) {
                return;
            }
            _orientation = value;
            CheckSize();
            Invalidate();
        }
    }

    [DefaultValue(0)]
    public new int TabIndex {
        get => 0;
        set => base.TabIndex = 0;
    }

    [DefaultValue(false)]
    public new bool TabStop {
        get => false;
        set => base.TabStop = false;
    }

    #endregion

    #region Methods

    public void CheckSize() {
        if (_orientation == Orientation.Waagerecht) {
            if (Width < 10) { Width = 10; }
            Height = 2;
        } else {
            Width = 2;
            if (Height < 10) { Height = 10; }
        }
    }

    protected override void DrawControl(Graphics gr, States state) {
        CheckSize();
        Pen dp = new(SystemColors.ControlDark);
        Pen lp = new(SystemColors.ControlLight);
        if (_orientation == Orientation.Waagerecht) {
            gr.DrawLine(dp, 0, 0, Width - 1, 0);
            gr.DrawLine(lp, 1, 1, Width, 1);
        } else {
            gr.DrawLine(dp, 0, 0, 0, Height - 1);
            gr.DrawLine(lp, 1, 1, 1, Height);
        }
    }

    #endregion
}