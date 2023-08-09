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

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueBasics;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueDatabase;
using System.Collections.Generic;

#nullable enable

namespace BlueControls.Controls;

public partial class TextGenerator : UserControl {

    #region Fields

    private DatabaseAbstract? _textDatabase = null;

    #endregion

    #region Constructors

    public TextGenerator() => InitializeComponent();

    #endregion

    #region Properties

    public string Vorfilter { get; set; } = string.Empty;

    #endregion

    #region Methods

    protected override void OnLoad(System.EventArgs e) {
        base.OnLoad(e);

        if (DesignMode) { return; }

        //_textDatabase = EnsureLoaded("Textkonserven");

        if (_textDatabase == null) { return; }

        var c1 = _textDatabase.Column.Exists("Filter00");
        var c2 = _textDatabase.Column.Exists("Filter01");
        if (c1 == null || c2 == null) { return; }

        var fi = new FilterItem(c1, BlueDatabase.Enums.FilterType.Istgleich_ODER_GroßKleinEgal, Vorfilter);

        var l = c2.Contents(fi, null);

        if (l.Count == 0) {
            cbxModus.Text = string.Empty;
            cbxModus.Enabled = false;
        } else if (l.Count == 1) {
            cbxModus.Text = l[0];
            cbxModus.Enabled = false;
        } else {
            cbxModus.Text = l[0];
            cbxModus.Enabled = true;
            cbxModus.Item.AddRange(l);
        }
    }

    private void cbxModus_TextChanged(object sender, System.EventArgs e) => GenerateItemsAndText();

    private void GenerateItemsAndText() {

        #region Zuerst die ungültigen Einträge löschen, es kann ja was abgewählt worden sein

        var stufe = 0;

        do {

            #region Ermitteln, mit welchen String die items anfangen müssen - die Stufe mit einbezogen (allowed)

            var x = lstAuswahl.Item.Checked().ToListOfString();
            _ = x.AddIfNotExists(Vorfilter);
            _ = x.AddIfNotExists(Vorfilter + ";" + cbxModus.Text);

            var allowed = new List<string>();
            foreach (var thisx in x) {
                if (thisx.CountString(";") == stufe) { allowed.Add(thisx); }
            }

            #endregion

            #region Nun alle löschen, die nicht damit beginnen

            bool again;
            do {
                again = false;
                foreach (var thisit in lstAuswahl.Item) {
                    if (thisit.Internal.CountString(";") >= stufe) {
                        var ok = false;
                        foreach (var thisall in allowed) {
                            if (thisit.Internal.StartsWith(thisall)) { ok = true; }
                        }
                        if (!ok) { lstAuswahl.Item.Remove(thisit); again = true; break; }
                    }
                }
            } while (again);

            #endregion

            stufe += 1;
        } while (_textDatabase.Column.Exists("Filter" + stufe.ToString(Constants.Format_Integer2)) != null);

        #endregion

        #region Gut, nun steht fest, welche Items wirklich erlaubt sind. die gewählten ermitteln (chk)

        var chk = lstAuswahl.Item.Checked().ToListOfString();
        _ = chk.AddIfNotExists(Vorfilter + ";" + cbxModus.Text);

        #endregion

        #region und nun die Datenbank durchforsten und fehlene einträge erzeugen

        var allr = _textDatabase.Row.CalculateSortedRows((List<FilterItem>)null, _textDatabase.SortDefinition, null, null);
        var txt = string.Empty;

        foreach (var thisRow in allr) {
            var r = RowString(thisRow.Row);

            if (lstAuswahl.Item[r] == null) {
                var add = false;
                foreach (var thisChecked in chk) {
                    if (thisChecked.CountString(";") + 1 == r.CountString(";") && r.StartsWith(thisChecked)) { add = true; }
                }
                if (add) {
                    var rvis = string.Empty.PadLeft((r.CountString(";") - 2) * 5) + r.Substring(r.LastIndexOf(";") + 1);

                    _ = lstAuswahl.Item.Add(rvis, r);
                }
            }

            if (chk.Contains(r)) {
                txt = txt + "\r\n" + thisRow.Row.CellGetString("Deutsch");
            }
        }

        //lstAuswahl.Item.Sort();

        #endregion

        textBox1.Text = txt;
    }

    private void lstAuswahl_ItemClicked(object sender, BlueControls.EventArgs.AbstractListItemEventArgs e) => GenerateItemsAndText();

    private string RowString(RowItem thisRow) {
        var stufe = 0;
        var s = string.Empty;

        do {
            var co = _textDatabase.Column.Exists("Filter" + stufe.ToString(Constants.Format_Integer2));
            if (co == null) { return s; }
            if (thisRow.IsNullOrEmpty(co)) { return s; }

            if (!string.IsNullOrEmpty(s)) { s += ";"; }
            s += thisRow.CellGetString(co);
            stufe++;
        } while (true);
    }

    #endregion
}