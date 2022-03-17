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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueDatabase;
using BlueDatabase.EventArgs;
using System;
using System.ComponentModel;

namespace BlueControls.Controls {

    [Obsolete("Wird zukünftig entfernt werden", false)]
    public partial class FormulaManualDesign : System.Windows.Forms.Panel {

        #region Fields

        private Database? _database;

        private long _savedRowKey = long.MinValue;

        private long _showingRowKey = -1;

        private RowItem? _tmpShowingRow;

        #endregion

        #region Constructors

        public FormulaManualDesign() : base() => InitializeComponent();

        #endregion

        #region Events

        public event EventHandler<RowEventArgs> ShowingRowChanged;

        #endregion

        #region Properties

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Database? Database {
            get => _database;
            set {
                if (_database == value) { return; }

                ShowingRowKey = -1; // Wichtig, dass ordenlich Showing-Row to Nothing gesetzt wird, weil dann alle Fokuse durch Enabled elemeniert werden und nachträglich nix mehr ausgelöst wird.

                if (_database != null) {
                    _database.Loading -= _Database_StoreView;
                    _database.Loaded -= _DatabaseLoaded;
                    //_Database.RowKeyChanged -= _Database_RowKeyChanged;
                    _database.Disposing -= _Database_Disposing;
                    _database.Save(false); // Datenbank nicht reseten, weil sie ja anderweitig noch benutzt werden kann
                }

                _database = value;

                if (_database == null) {
                    return;
                }

                _database.Loading += _Database_StoreView;
                _database.Loaded += _DatabaseLoaded;
                //_Database.RowKeyChanged += _Database_RowKeyChanged;
                _database.Disposing += _Database_Disposing;
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RowItem? ShowingRow {
            get {
                Develop.DebugPrint_Disposed(IsDisposed);
                return _tmpShowingRow;
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public long ShowingRowKey {
            get {
                Develop.DebugPrint_Disposed(IsDisposed);
                return _showingRowKey;
            }
            set {
                Develop.DebugPrint_Disposed(IsDisposed);
                if (value < 0) { value = -1; }
                if (value == _showingRowKey) { return; }
                if (value > -1 && _database == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Database is nothing"); }

                _showingRowKey = value;
                _tmpShowingRow = _database?.Row.SearchByKey(_showingRowKey);

                foreach (var thisFlex in Controls) {
                    if (thisFlex is FlexiControlForCell flx && !flx.IsDisposed) {
                        flx.Database = _database;
                        flx.RowKey = _showingRowKey;
                        flx.CheckEnabledState();
                    }
                }

                OnShowingRowChanged(new RowEventArgs(ShowingRow));
                ShowingRow?.DoAutomatic(false, false, "to be sure");
            }
        }

        #endregion

        #region Methods

        protected override void Dispose(bool disposing) {
            try {
                if (disposing) {
                    _showingRowKey = -1;
                    _tmpShowingRow = null;
                    Database = null; // Wichtig,  (nicht _Database) um events zu lösen.
                    //components?.Dispose();
                }
            } finally {
                base.Dispose(disposing);
            }
        }

        private void _Database_Disposing(object sender, System.EventArgs e) => Database = null;

        //private void _Database_RowChecked(object sender, RowCheckedEventArgs e) {
        //if (e.Row.Key != _ShowingRowKey) { return; }
        //var nr = int.MaxValue;
        //var ColNr = int.MaxValue;
        //for (var cc = 0; cc < e.ColumnsWithErrors.Count; cc++) {
        //    var p = e.ColumnsWithErrors[cc].Split('|');
        //    foreach (var ThisColumnItem in e.Row.Database.Column) {
        //        if (ThisColumnItem != null) {
        //            if (p[0].ToUpper() == ThisColumnItem.Name.ToUpper()) {
        //                //Bitte jeden Fehler anzeigen..... Es verursacht mehr Rätsel, wenn die Zeile einfach Fehlerhaft ist und überhaut kein Hinweis kommt
        //                var CD = SearchViewItem(ThisColumnItem);
        //                var View = SearchColumnView(ThisColumnItem);
        //                var tmp = CD == null
        //                    ? ThisColumnItem.Index() + 200000
        //                    : View == CurrentView() ? ThisColumnItem.Index() : ThisColumnItem.Index() + 100000;
        //                if (tmp < ColNr) {
        //                    ColNr = tmp;
        //                    nr = cc;
        //                }
        //            }
        //        }
        //    }
        //}
        //if (nr < int.MaxValue) {
        //    _ = e.ColumnsWithErrors[nr].Split('|');
        //}
        //}

        //private void _Database_RowKeyChanged(object sender, KeyChangedEventArgs e) {
        //    // Ist aktuell nur möglich,wenn Pending Changes eine neue Zeile machen
        //    // Jedes FlexControl beachtet für sich die Änderung
        //    if (e.KeyOld == _savedRowKey) { _savedRowKey = e.KeyNew; }
        //}

        private void _Database_StoreView(object sender, LoadingEventArgs e) {
            if (e.OnlyReload) { return; }
            _savedRowKey = ShowingRowKey;
        }

        private void _DatabaseLoaded(object sender, LoadedEventArgs e) {
            if (InvokeRequired) {
                Invoke(new Action(() => _DatabaseLoaded(sender, e)));
                return;
            }
            if (IsDisposed) { return; }
            ShowingRowKey = _savedRowKey;
        }

        private void OnShowingRowChanged(RowEventArgs e) => ShowingRowChanged?.Invoke(this, e);

        #endregion
    }
}