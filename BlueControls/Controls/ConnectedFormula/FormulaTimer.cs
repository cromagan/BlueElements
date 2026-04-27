// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueBasics;
using BlueControls.Classes;
using BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular;
using BlueControls.Controls.ConnectedFormula;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueScript.Variables;
using BlueTable.Classes;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;

namespace BlueControls.Controls;

internal partial class FormulaTimer : GenericControlReciver, IBackgroundNone {

    #region Fields

    private int _last;
    private Timer? _main;
    private string _value0 = string.Empty;
    private string _value1 = string.Empty;
    private string _value2 = string.Empty;
    private bool _wasok = true;

    #endregion

    #region Constructors

    public FormulaTimer() : base(false, false, false) {
        InitializeComponent();
        _last = -1;
        _main = new Timer(_ => {
            if (IsHandleCreated) { BeginInvoke(new Action(Main_Tick)); }
        }, null, 1000, 1000);
    }

    #endregion

    #region Properties

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Script {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            Invalidate();
        }
    } = string.Empty;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Seconds { get; set; }

    #endregion

    #region Methods

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        _main?.Change(Timeout.Infinite, Timeout.Infinite);
        Script = string.Empty;
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (IsDisposed) { return; }
        base.DrawControl(gr, state);
        Skin.Draw_Back_Transparent(gr, ClientRectangle, this);
    }

    protected override void HandleChangesNow() {
        base.HandleChangesNow();

        if (IsDisposed) { return; }
        if (RowsInputChangedHandled && FilterInputChangedHandled) { return; }

        DoInputFilter(null, false);
        RowsInputChangedHandled = true;
    }

    private void Main_Tick() {
        if (!_wasok) { return; }

        _last++;
        if (_last < Seconds) { return; }

        capAuslösezeit.Text = DateTime.Now.ToString5();

        if (Parent is ConnectedFormulaView cfv && (cfv.GetConnectedFormula()?.IsEditing() ?? true)) {
            capMessage.Text = "Editor geöffnet.";
            return;
        }

        #region Variablen erstellen

        VariableCollection vars;

        var row = RowSingleOrNull();
        Table? tb = null;

        if (row?.Table is { IsDisposed: false } row_tb) {
            tb = row_tb;
            vars = tb.CreateVariableCollection(row, false, false, true, true, FilterInput);
        } else if (FilterInput?.Table is { IsDisposed: false } fi_tb) {
            tb = fi_tb;
            vars = tb.CreateVariableCollection(null, false, false, true, true, FilterInput);
        } else {
            vars = [];
        }

        if (Parent is IHasFieldVariable hfvp && hfvp.GetFieldVariable() is { } v2) {
            vars.Add(v2);
        }

        foreach (var thisCon in Parent.Controls) {
            if (thisCon is IHasFieldVariable hfv && hfv.GetFieldVariable() is { } v) {
                vars.Add(v);
            }
        }

        vars.Add(new VariableString("Value0", _value0, false, "Diese Variable bleibt im Script erhalten uns steht beim nächsten Durchlauf wieder zur Verfügung."));
        vars.Add(new VariableString("Value1", _value1, false, "Diese Variable bleibt im Script erhalten uns steht beim nächsten Durchlauf wieder zur Verfügung."));
        vars.Add(new VariableString("Value2", _value2, false, "Diese Variable bleibt im Script erhalten uns steht beim nächsten Durchlauf wieder zur Verfügung."));

        #endregion

        var t = TimerPadItem.ExecuteScript(Script, Mode, vars, row, true);

        if (t.Failed) {
            _wasok = false;
            capMessage.Text = "Skript fehlerhaft: " + t.FailedReason;
            return;
        }

        capMessage.Text = t.Variables?.GetString("Feedback") ?? "-";

        _value0 = t.Variables?.GetString("value0") ?? string.Empty;
        _value1 = t.Variables?.GetString("value1") ?? string.Empty;
        _value2 = t.Variables?.GetString("value2") ?? string.Empty;

        #region Variablen zurückschreiben

        foreach (var thisCon in Parent.Controls) {
            if (thisCon is IHasFieldVariable hfv && vars.GetByKey(hfv.FieldName) is { ReadOnly: false } v) {
                hfv.SetValueFromVariable(v);
            }
        }

        tb?.WriteBackVariables(row, vars, false, true, "Timer-Tick", !t.Failed);

        #endregion

        _last = 0;
    }

    #endregion
}