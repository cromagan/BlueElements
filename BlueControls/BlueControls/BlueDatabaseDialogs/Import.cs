// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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
using BlueControls.Forms;
using BlueDatabase;
using static BlueBasics.Extensions;

namespace BlueControls.BlueDatabaseDialogs {

    public sealed partial class Import {

        #region Fields

        private readonly string _originalImportText = string.Empty;

        #endregion

        #region Constructors

        public Import(Database database, string importtext) : base() {
            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();
            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            _originalImportText = importtext.Replace("\r\n", "\r").Trim("\r");
            var Ein = _originalImportText.SplitAndCutByCRToList();
            Eintr.Text = Ein.Count + " zum Importieren bereit.";
            Database = database;
            Database.Disposing += Database_Disposing;
        }

        #endregion

        #region Properties

        public Database Database { get; private set; }

        #endregion

        #region Methods

        private void Cancel_Click(object sender, System.EventArgs e) => Close();

        private void Database_Disposing(object sender, System.EventArgs e) {
            Database.Disposing -= Database_Disposing;
            Close();
        }

        private void Fertig_Click(object sender, System.EventArgs e) {
            var TR = string.Empty;
            if (TabStopp.Checked) {
                TR = "\t";
            } else if (Semikolon.Checked) {
                TR = ";";
            } else if (Komma.Checked) {
                TR = ",";
            } else if (Leerzeichen.Checked) {
                TR = " ";
            } else if (Andere.Checked) {
                TR = aTXT.Text;
            }
            if (string.IsNullOrEmpty(TR)) {
                MessageBox.Show("Bitte Trennzeichen angeben.", enImageCode.Information, "OK");
                return;
            }
            var m = Database.Import(_originalImportText, SpalteZuordnen.Checked, ZeilenZuorden.Checked, TR, Aufa.Checked, AnfTre.Checked, false);

            if (!string.IsNullOrEmpty(m)) {
                MessageBox.Show(m, enImageCode.Information, "OK");
            }
            Close();
        }

        #endregion
    }
}