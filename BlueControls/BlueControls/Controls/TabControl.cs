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

#nullable enable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueBasics;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueDatabase;

namespace BlueControls.Controls;

public class TabControl : AbstractTabControl, IControlAcceptRow {

    #region Fields

    private IControlSendRow? _getRowFrom;

    #endregion

    #region Constructors

    public TabControl() : base() => BackColor = Skin.Color_Back(Design.TabStrip_Body, States.Standard);

    #endregion

    #region Properties

    public override sealed Color BackColor {
        get => base.BackColor;
        set => base.BackColor = value;
    }

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public DatabaseAbstract? Database { get; private set; }

    public IControlSendRow? GetRowFrom {
        get => _getRowFrom;
        set {
            if (_getRowFrom == value) { return; }
            if (_getRowFrom != null) {
                Develop.DebugPrint(BlueBasics.Enums.FehlerArt.Fehler, "Änderung nicht erlaubt");
            }

            _getRowFrom = value;
            if (_getRowFrom != null) { _getRowFrom.ChildAdd(this); }
        }
    }

    public RowItem? LastInputRow { get; }

    [DefaultValue("")]
    public string RowKey { get; private set; } = string.Empty;

    #endregion

    #region Methods

    public void SetData(DatabaseAbstract? database, string rowkey) {
        if (database != Database && rowkey == RowKey) { return; }
        Database = database;
        RowKey = rowkey ?? string.Empty;
        DoTabChilds();
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

    private void DoTabChilds() {
        // Dieses DoChilds unterscheidet sich von IControlSend:
        // Da das TabControl kein Send - Control an sich ist, aber trotzdem die Tabs befüllen muss
        foreach (var thisTab in TabPages) {
            if (thisTab is TabPage tp) {
                foreach (var thisControl in tp.Controls) {
                    if (thisControl is ConnectedFormulaView cfw) {
                        cfw.GenerateView();
                        foreach (var thisControl2 in cfw.Controls) {
                            if (thisControl2 is IControlRowInput iar) {
                                iar.SetData(Database, RowKey);
                            }
                        }
                    }
                }
            }
        }
        Invalidate();
    }

    #endregion
}