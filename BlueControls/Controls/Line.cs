// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Enums;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.Interfaces;
using System.ComponentModel;
using System.Drawing;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public class Line : GenericControl, IBackgroundNone {

    #region Constructors

    public Line() : base(false, false, false) {
        // Dieser Aufruf ist für den Designer erforderlich.
        //  InitializeComponent()
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        SetNotFocusable();
    }

    #endregion

    #region Properties

    [DefaultValue(Orientation.Waagerecht)]
    public Orientation Orientation {
        get;
        set {
            if (value == field) {
                return;
            }
            field = value;
            CheckSize();
            Invalidate();
        }
    } = Orientation.Waagerecht;

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
        if (Orientation == Orientation.Waagerecht) {
            if (Width < 10) { Width = 10; }
            Height = 2;
        } else {
            Width = 2;
            if (Height < 10) { Height = 10; }
        }
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (IsDisposed) { return; }
        base.DrawControl(gr, state);
        CheckSize();
        var dp = new Pen(SystemColors.ControlDark);
        var lp = new Pen(SystemColors.ControlLight);
        if (Orientation == Orientation.Waagerecht) {
            gr.DrawLine(dp, 0, 0, Width - 1, 0);
            gr.DrawLine(lp, 1, 1, Width, 1);
        } else {
            gr.DrawLine(dp, 0, 0, 0, Height - 1);
            gr.DrawLine(lp, 1, 1, 1, Height);
        }
    }

    #endregion
}