﻿// Authors:
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

using BlueControls.BlueDatabaseDialogs;
using BlueDatabase;

namespace BeCreative;

public partial class Start : BlueControls.Forms.Form {

    #region Constructors

    public Start() {
        InitializeComponent();
    }

    #endregion

    #region Methods

    private void btnBildEditor_Click(object sender, System.EventArgs e) {
        var x = new BluePaint.MainWindow(true);

        x.ShowDialog();
        x.Dispose();
    }

    private void btnDatenbank_Click(object sender, System.EventArgs e) {
        var x = new BlueControls.Forms.TableView(null, true, true);

        x.ShowDialog();
        x.Dispose();
    }

    private void btnFormular_Click(object sender, System.EventArgs e) {
        var x = new BlueControls.Forms.ConnectedFormulaEditor();

        x.ShowDialog();
        x.Dispose();
    }

    private void btnHierachie_Click(object sender, System.EventArgs e) {
        var x = new BlueControls.Forms.RelationDiagram(null);

        x.ShowDialog();
        x.Dispose();
    }

    private void btnLayout_Click(object sender, System.EventArgs e) {
        var x = new LayoutPadEditor((Database)null);

        x.ShowDialog();
        x.Dispose();
    }

    private void btnTextEditor_Click(object sender, System.EventArgs e) {
    }

    #endregion
}