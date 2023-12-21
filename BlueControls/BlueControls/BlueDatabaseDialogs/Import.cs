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

using System.ComponentModel;
using BlueBasics.Enums;
using BlueControls.Forms;
using BlueDatabase;
using BlueDatabase.Interfaces;
using static BlueBasics.Extensions;

namespace BlueControls.BlueDatabaseDialogs;

public sealed partial class Import : FormWithStatusBar, IHasDatabase {

    #region Fields

    private readonly string _originalImportText;

    #endregion

    #region Constructors

    public Import(Database? database, string importtext) : base() {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        _originalImportText = importtext.Replace("\r\n", "\r").Trim("\r");
        var ein = _originalImportText.SplitAndCutByCrToList();
        Eintr.Text = ein.Count + " zum Importieren bereit.";
        Database = database;
        if (Database is not Database db || db.IsDisposed) { return; }

        db.DisposingEvent += Database_DisposingEvent;
    }

    #endregion

    #region Properties

    public Database? Database { get; }

    #endregion

    #region Methods

    protected override void OnClosing(CancelEventArgs e) {
        if (Database != null && !Database.IsDisposed) { Database.DisposingEvent -= Database_DisposingEvent; }
        base.OnClosing(e);
    }

    private void Cancel_Click(object sender, System.EventArgs e) => Close();

    private void Database_DisposingEvent(object sender, System.EventArgs e) => Close();

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
            MessageBox.Show("Bitte Trennzeichen angeben.", ImageCode.Information, "OK");
            return;
        }
        var m = "Datenbank-Fehler";

        if (Database != null && !Database.IsDisposed) {
            m = Database.Import(_originalImportText, SpalteZuordnen.Checked, ZeilenZuorden.Checked, TR, Aufa.Checked, AnfTre.Checked);
        }

        if (!string.IsNullOrEmpty(m)) {
            MessageBox.Show(m, ImageCode.Information, "OK");
        }
        Close();
    }

    #endregion
}