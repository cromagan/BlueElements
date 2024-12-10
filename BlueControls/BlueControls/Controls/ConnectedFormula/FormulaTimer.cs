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
using System.ComponentModel;
using System.Drawing;
using BlueBasics;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;

namespace BlueControls.Controls;

internal partial class FormulaTimer : GenericControl, IBackgroundNone //System.Windows.Forms.UserControl  /// Usercontrol
{
    #region Fields

    private int _last = -5;

    private bool _wasok = true;

    #endregion

    #region Constructors

    public FormulaTimer() : base(false, false, false) => InitializeComponent();

    #endregion

    #region Properties

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public virtual string Mode { get; set; } = string.Empty;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Script { get; internal set; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Seconds { get; set; }

    #endregion

    #region Methods

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        main.Enabled = false;
        Script = string.Empty;
    }

    protected override void DrawControl(Graphics gr, States state) => Skin.Draw_Back_Transparent(gr, ClientRectangle, this);//Intiall();

    private void main_Tick(object sender, System.EventArgs e) {
        if (!_wasok) { return; }

        _last++;
        if (_last < Seconds) { return; }

        capAuslösezeit.Text = DateTime.Now.ToString5();

        var t = TimerPadItem.ExecuteScript(Script, Mode);

        if (!t.Successful || !t.AllOk) {
            _wasok = false;
            capMessage.Text = "Skript fehlerhaft. " + t.ProtocolText;
            return;
        }

        capMessage.Text = t.Variables?.GetString("Feedback") ?? "-";

        _last = 0;
    }

    #endregion
}