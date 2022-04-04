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

using System.Collections.Generic;
using static BlueBasics.Develop;
using BlueDatabase;
using BlueBasics;
using BlueControls.ItemCollection;
using BlueControls.Controls;
using BlueDatabase.Enums;
using BlueControls.Interfaces;
using BlueScript;

namespace BlueControls.ConnectedFormula {

    internal class RowCloner : System.Windows.Forms.Control, IAcceptRowKey, ICalculateRowsControlLevel {

        #region Fields

        public ItemCollectionPad? ParentCol;

        //private readonly RowClonePadItem _rwf;
        private bool _disposing = false;

        private RowItem? _row = null;

        #endregion

        #region Constructors

        public RowCloner(Database? database, string verbindungsID) {
            Database = database;

            foreach (var thisConnector in FlexiControlRowSelector.AllConnectors) {
                if (thisConnector.VerbindungsId == verbindungsID) {
                    thisConnector.Childs.Add(this);
                }
            }
        }

        #endregion

        #region Properties

        public ListExt<System.Windows.Forms.Control> Childs { get; } = new();
        public Database? Database { get; set; }
        public string DisabledReason { get; set; }

        public long RowKey {
            get {
                if (_row == null) { return -1; }
                return _row.Key;
            }
            set {
                if (_row != null && _row.Key == value) { return; }
                _row = Database.Row.SearchByKey(value);
            }
        }

        public Script? Script { get; set; }

        #endregion

        // Ignorieren, wird eh nie angezeigt

        #region Methods

        public void DeleteValue() {
            RowKey = -1;
        }

        //private string _verbindungsID = string.Empty;
        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);

            if (disposing) {
                _disposing = true;

                //AllConnectors.Remove(this);

                Tag = null;

                _disposing = true;
                Childs.Clear();
                //_parents.Clear();
                _row = null;

                Childs.ItemAdded -= Childs_ItemAdded;
                //_parents.ItemAdded -= Parents_ItemAdded;
                //_parents.ItemRemoving -= Parents_ItemRemoving;
            }
        }

        protected override void OnParentChanged(System.EventArgs e) {
            base.OnParentChanged(e);

            Childs.ItemAdded += Childs_ItemAdded;

            //_rwf_Changed(null, System.EventArgs.Empty);
            FlexiControlRowSelector.DoChilds(this, _row, ParentCol);
        }

        private void Childs_ItemAdded(object sender, BlueBasics.EventArgs.ListEventArgs e) {
            FlexiControlRowSelector.DoChilds(this, _row, ParentCol);
        }

        #endregion
    }
}