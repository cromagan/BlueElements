// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueDatabase;
using System.ComponentModel;
using System.Windows.Forms;

namespace BlueControls.Controls;

public class TabControl : AbstractTabControl, IAcceptRowKey {

    #region Fields

    private DatabaseAbstract? _database;

    private long _rowkey = -1;

    #endregion

    #region Constructors

    public TabControl() : base() => BackColor = Skin.Color_Back(Design.TabStrip_Body, States.Standard);

    #endregion

    #region Properties

    [DefaultValue(null)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public DatabaseAbstract? Database {
        get => _database;
        set {
            if (_database != value) {
                _database = value;
                DoDatabaseAction();
            }
        }
    }

    [DefaultValue(-1)]
    public long RowKey {
        get => _rowkey; set {
            if (_rowkey != value) {
                _rowkey = value;
                DoDatabaseAction();
            }
        }
    }

    #endregion

    #region Methods

    protected override void OnControlAdded(ControlEventArgs e) {
        base.OnControlAdded(e);
        if (e.Control is not TabPage tp) {
            return;
        }

        tp.BackColor = Skin.Color_Back(Design.TabStrip_Body, States.Standard);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e) => DrawControl(e, Design.TabStrip_Back);

    private void DoDatabaseAction() {
        foreach (var thisTab in TabPages) {
            if (thisTab is TabPage tp) {
                foreach (var thisControl in tp.Controls) {
                    if (thisControl is IAcceptRowKey iar and not TabControl) {
                        //iar.RowKey = -1;
                        iar.Database = Database;
                        iar.RowKey = RowKey;
                    }
                }
            }
        }
        Invalidate();
    }

    #endregion
}