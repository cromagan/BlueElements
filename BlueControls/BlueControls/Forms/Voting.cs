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

using System.Collections.Generic;
using BlueBasics;
using BlueDatabase;
using System;

namespace BlueControls.Forms;

public partial class Voting : System.Windows.Forms.Form {

    #region Fields

    private readonly ColumnItem _column;

    private readonly List<RowItem> _rows;
    private Dictionary<string, bool> _done = new();
    private string _filename = string.Empty;
    private RowItem? _fr1;
    private RowItem? _fr2;

    #endregion

    #region Constructors

    public Voting(ColumnItem column, List<RowItem> rows) : base() {
        InitializeComponent();
        _column = column;
        _rows = rows;
    }

    #endregion

    #region Methods

    protected override void OnShown(System.EventArgs e) {
        base.OnShown(e);
        ExportDialog.AddLayoutsOff(cbxStil, _column.Database);
        Generate();
    }

    private void Add(RowItem fr1, RowItem fr2, bool xxx) {
        string key;
        if (fr1.KeyName.CompareTo(fr2.KeyName) > 0) {
            key = fr1.KeyName + "|" + fr2.KeyName;
        } else {
            key = fr2.KeyName + "|" + fr1.KeyName;
            xxx = !xxx;
        }
        _done[key] = xxx;
    }

    private void btn1_Click(object? sender, System.EventArgs e) {
        if (_fr1 == null || _fr2 == null) { return; }
        var v1 = _fr1.CellGetDouble(_column) - 7;
        var v2 = _fr2.CellGetDouble(_column) + 10;

        Add(_fr1, _fr2, false);

        _fr1.CellSet(_column, v1, "Downvote");
        _fr2.CellSet(_column, v2, "Upvote");
        Generate();
    }

    private void btn2_Click(object? sender, System.EventArgs e) {
        if (_fr1 == null || _fr2 == null) { return; }
        var v1 = _fr1.CellGetDouble(_column) + 10;
        var v2 = _fr2.CellGetDouble(_column) - 7;

        Add(_fr1, _fr2, true);

        _fr1.CellSet(_column, v1, "Upvote");
        _fr2.CellSet(_column, v2, "DownVote");
        Generate();
    }

    private void cbxStil_TextChanged(object sender, System.EventArgs e) {
        _filename = string.Empty;
        if (cbxStil.Text.FileSuffix().ToUpperInvariant() == "BCR") {
            _filename = cbxStil.Text;
        }

        Generate();
    }

    private void Generate() {
        _fr1 = null;
        _fr2 = null;

        double minab = 6;

        for (var row1 = 0; row1 < _rows.Count - 1; row1++) {
            var sc1 = _rows[row1].CellGetDouble(_column);

            for (int row2 = row1 + 1; row2 < _rows.Count; row2++) {
                var sc2 = _rows[row2].CellGetDouble(_column);

                var istab = Math.Abs(sc1 - sc2);
                //var weight = 20 / (1 + Math.Exp(-Math.Abs(sc1)));
                if (istab < minab) {
                    minab = istab;
                    _fr1 = _rows[row1];
                    _fr2 = _rows[row2];
                }
            }
        }






        if (_fr1 == null || _fr2 == null || string.IsNullOrWhiteSpace(_filename)) {
            Pad1.Item?.Clear();
            btn1.Enabled = false;
            Pad2.Item?.Clear();
            btn2.Enabled = false;
            return;
        }



        string key;
        if (_fr1.KeyName.CompareTo(_fr2.KeyName) > 0) {
            key = _fr1.KeyName + "|" + _fr2.KeyName;
        } else {
            key = _fr2.KeyName + "|" + _fr1.KeyName;
        }

        if (_done.ContainsKey(key)) {
            //Pad1.Refresh();
            //Pad2.Refresh();
            //Database.ForceSaveAll();
            if (_done[key]) {
                btn2_Click(null, System.EventArgs.Empty);
            } else {
                btn1_Click(null, System.EventArgs.Empty);
            }
            return;
        }


        var p1 = new ItemCollectionPad.ItemCollectionPad(_filename);
        p1.ResetVariables();
        p1.ReplaceVariables(_fr1);

        Pad1.Item = p1;
        Pad1.ShowInPrintMode = true;
        Pad1.ZoomFit();

        var p2 = new ItemCollectionPad.ItemCollectionPad(_filename);
        p2.ResetVariables();
        p2.ReplaceVariables(_fr2);

        Pad2.Item = p2;
        Pad2.ShowInPrintMode = true;
        Pad2.ZoomFit();

 



        btn1.Enabled = true;
        btn2.Enabled = true;
    }

    #endregion
}