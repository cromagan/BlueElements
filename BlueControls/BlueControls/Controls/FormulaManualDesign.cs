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
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollection;
using BlueDatabase;
using BlueDatabase.EventArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using static BlueBasics.Converter;

namespace BlueControls.Controls {

    public partial class FormulaManualDesign : System.Windows.Forms.Panel {

        #region Fields

        private Database _Database;

        private long _savedRowKey = long.MinValue;

        private long _ShowingRowKey = -1;

        private RowItem _tmpShowingRow = null;

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
        public Database Database {
            get => _Database;
            set {
                if (_Database == value) { return; }

                ShowingRowKey = -1; // Wichtig, dass ordenlich Showing-Row to Nothing gesetzt wird, weil dann alle Fokuse durch Enabled elemeniert werden und nachträglich nix mehr ausgelöst wird.

                if (_Database != null) {
                    _Database.Loading -= _Database_StoreView;
                    _Database.Loaded -= _DatabaseLoaded;
                    //_Database.RowKeyChanged -= _Database_RowKeyChanged;
                    _Database.Disposing -= _Database_Disposing;
                    _Database.Save(false); // Datenbank nicht reseten, weil sie ja anderweitig noch benutzt werden kann
                }

                _Database = value;

                if (_Database != null) {
                    _Database.Loading += _Database_StoreView;
                    _Database.Loaded += _DatabaseLoaded;
                    //_Database.RowKeyChanged += _Database_RowKeyChanged;
                    _Database.Disposing += _Database_Disposing;
                }
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RowItem ShowingRow {
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
                return _ShowingRowKey;
            }
            set {
                Develop.DebugPrint_Disposed(IsDisposed);
                if (value < 0) { value = -1; }
                if (value == _ShowingRowKey) { return; }
                if (value > -1 && _Database == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Database is nothing"); }

                _ShowingRowKey = value;
                _tmpShowingRow = _Database?.Row.SearchByKey(_ShowingRowKey);

                foreach (var thisFlex in Controls) {
                    if (thisFlex is FlexiControlForCell flx && !flx.IsDisposed) {
                        flx.Database = _Database;
                        flx.RowKey = _ShowingRowKey;
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
                    _ShowingRowKey = -1;
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