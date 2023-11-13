﻿// Authors:
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

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueDatabase;
using System;

namespace BlueControls.Controls;

public class TabControl : AbstractTabControl, IControlAcceptSomething, IControlSendSomething {

    #region Constructors

    public TabControl() : base() => BackColor = Skin.Color_Back(Design.TabStrip_Body, States.Standard);

    #endregion

    #region Events

    public event EventHandler? DisposingEvent;

    #endregion

    #region Properties

    public override sealed Color BackColor {
        get => base.BackColor;
        set => base.BackColor = value;
    }

    public List<IControlAcceptSomething> Childs { get; } = new();

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public DatabaseAbstract? DatabaseOutput { get; set; }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection? FilterInput { get; set; } = null;

    public FilterCollection? FilterOutput { get; set; } = null;

    public List<IControlSendSomething> Parents { get; } = new();

    #endregion

    #region Methods

    public void FilterInput_Changed(object sender, System.EventArgs e) {
        FilterInput = this.FilterOfSender();

        //// Dieses DoChilds unterscheidet sich von IControlSend:
        //// Da das TabControl kein Send - Control an sich ist, aber trotzdem die Tabs befüllen muss
        //foreach (var thisTab in TabPages) {
        //    if (thisTab is TabPage tp) {
        //        foreach (var thisControl in tp.Controls) {
        //            if (thisControl is ConnectedFormulaView cfw) {
        //                cfw.GenerateView();
        //                foreach (var thisControl2 in cfw.Controls) {
        //                    if (thisControl2 is IControlAcceptSomething iar) {
        //                        iar.FilterOutput_Changed();
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
        Invalidate();
    }

    public void FilterInput_Changing(object sender, System.EventArgs e) { }

    public void OnDisposingEvent() => DisposingEvent?.Invoke(this, System.EventArgs.Empty);

    protected override void Dispose(bool disposing) {
        OnDisposingEvent();

        base.Dispose(disposing);
        if (disposing) {
            _bitmapOfControl?.Dispose();
            _bitmapOfControl = null;
        }
    }

    protected override void Dispose(bool disposing) {
        OnDisposingEvent();

        base.Dispose(disposing);
        //if (disposing) {
        //    _bitmapOfControl?.Dispose();
        //    _bitmapOfControl = null;
        //}
    }

    protected override void OnControlAdded(ControlEventArgs e) {
        base.OnControlAdded(e);
        if (e.Control is not TabPage tp) {
            return;
        }

        tp.BackColor = Skin.Color_Back(Design.TabStrip_Body, States.Standard);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e) => DrawControl(e, Design.TabStrip_Back);

    #endregion
}