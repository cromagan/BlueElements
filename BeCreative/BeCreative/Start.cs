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

using BlueBasics.Enums;
using BlueBasics.MultiUserFile;
using BlueControls;
using BlueControls.Forms;
using BlueDatabase;
using BluePaint;
using System;
using static BlueBasics.Develop;

namespace BeCreative {

    public partial class Start : Form {

        #region Constructors

        public Start() {
            InitializeComponent();
        }

        #endregion

        #region Methods

        public static void Ende() {
            DebugPrint(FehlerArt.Info, "Schließe Programm...");

            var p = Progressbar.Show("Programm wird beendet<br><i>Speichern aller Datenbanken");
            Database.ForceSaveAll();
            MultiUserFile.SaveAll(false); // Sicherheitshalber, falls die Worker zu lange brauchen....

            p.Update("Programm wird beendet<br><i>Speichern aller Datenbanken");
            Database.ForceSaveAll();
            MultiUserFile.SaveAll(false); // Fonts und Dictionarys werden noch benötigt

            DebugPrint(FehlerArt.Info, "Schließe Programm, noch ein SaveAll.");
            p.Update("Programm wird beendet<br><i>Fast geschafft!");
            MultiUserFile.SaveAll(true); // Nun aber

            p.Close();
            TraceLogging_End();
        }

        private void btnBildEditor_Click(object sender, EventArgs e) {
            DoForm(new MainWindow(true));
        }

        private void btnDatenbank_Click(object sender, EventArgs e) {
            DoForm(new TableView(null, true, true));
        }

        private void btnFormular_Click(object sender, EventArgs e) {
            DoForm(new ConnectedFormulaEditor());
        }

        private void btnFormularAnsicht_Click(object sender, EventArgs e) {
            DoForm(new ConnectedFormulaForm());
        }

        private void btnHierachie_Click(object sender, EventArgs e) {
            DoForm(new RelationDiagram(null));
        }

        private void btnLayout_Click(object sender, EventArgs e) {
            DoForm(new PadEditorWithFileAccess());
        }

        private void DoForm(System.Windows.Forms.Form frm) {
            if (frm.IsDisposed) {
                DebugPrint("Fenster verworfen!");
                return;
            }
            FormManager.RegisterForm(frm);
            frm.Show();
            Close();
            frm.BringToFront();
        }

        #endregion
    }
}