﻿// Authors:
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
            //var l = Stopwatch.StartNew();

            //const ImageCode tempVar = (ImageCode)9999;
            //for (ImageCode z = 0; z <= tempVar; z++) {
            //    var w = Enum.GetName(z.GetType(), z);
            //    if (!string.IsNullOrEmpty(w)) {
            //        var x = QuickImage.Get(w + "|64||128");
            //        var x2 = QuickImage.Get(w + "|60");
            //        var x3 = QuickImage.Get(w + "|32|40||||50");
            //    }
            //}

            //l.Stop();
            //var l2 = l.ElapsedMilliseconds;

            DoForm(new TableView(null, true, true));
        }

        private void btnFormular_Click(object sender, EventArgs e) {
            DoForm(new ConnectedFormulaEditor());
        }

        private void btnFormularAnsicht_Click(object sender, EventArgs e) {
            DoForm(new ConnectedFormulaView());
        }

        private void btnHierachie_Click(object sender, EventArgs e) {
            DoForm(new RelationDiagram(null));
        }

        private void btnLayout_Click(object sender, EventArgs e) {
            DoForm(new PadEditorWithFileAccess());
        }

        private void DoForm(System.Windows.Forms.Form frm) {
            FormManager.RegisterForm(frm);
            frm.Show();
            Close();
            frm.BringToFront();
        }

        #endregion
    }
}