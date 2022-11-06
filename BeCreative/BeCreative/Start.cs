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

using BlueBasics.Enums;
using BlueControls;
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Forms;
using static BlueBasics.Develop;

namespace BeCreative;

public partial class Start : Form {

    #region Constructors

    public Start() => InitializeComponent();

    #endregion

    #region Methods

    public static void Ende() {
        DebugPrint(FehlerArt.Info, "Schließe Programm...");

        var P = Progressbar.Show("Programm wird beendet<br><i>Speichern aller Datenbanken");
        BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false); // Sicherheitshalber, falls die Worker zu lange brauchen....

        P?.Update("Programm wird beendet<br><i>Speichern aller Datenbanken");
        BlueBasics.MultiUserFile.MultiUserFile.SaveAll(false); // Fonts und Dictionarys werden noch benötigt

        DebugPrint(FehlerArt.Info, "Schließe Programm, noch ein SaveAll.");
        P?.Update("Programm wird beendet<br><i>Fast geschafft!");
        BlueBasics.MultiUserFile.MultiUserFile.SaveAll(true); // Nun aber

        P?.Close();
        TraceLogging_End();
    }

    internal static System.Windows.Forms.Form? NewForm() => new Start();

    private void btnBildEditor_Click(object sender, System.EventArgs e) => DoForm(new BluePaint.MainWindow(true));

    private void btnDatenbank_Click(object sender, System.EventArgs e) => DoForm(new TableView(null, true, true));

    private void btnFormular_Click(object sender, System.EventArgs e) => DoForm(new ConnectedFormulaEditor());

    private void btnHierachie_Click(object sender, System.EventArgs e) => DoForm(new RelationDiagram(null));

    private void btnLayout_Click(object sender, System.EventArgs e) => DoForm(new LayoutPadEditor(null));

    private void DoForm(Form frm) {
        FormManager.Current.RegisterForm(frm);
        frm.Show();
        Close();
        frm.BringToFront();
    }

    #endregion
}