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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BlueBasics.Extensions;
using static BlueBasics.Develop;
using BlueDatabase;
using BlueBasics;

namespace BlueControls.ConnectedFormula {

    public class ConnectedFormula : BlueBasics.MultiUserFile.MultiUserFile {

        #region Fields

        public static readonly string Version = "0.01";

        public readonly ListExt<string> DatabaseFiles = new();
        public readonly List<Database?> Databases = new();
        private bool _saved = true;

        #endregion

        #region Constructors

        public ConnectedFormula(string filename) : base(false, false) {
            Load(filename, true);
            DatabaseFiles.Changed += DatabaseFiles_Changed;
        }

        #endregion

        #region Properties

        public string FilePath { get; set; } = string.Empty;

        #endregion

        #region Methods

        public override void DiscardPendingChanges() => _saved = true;

        public override bool HasPendingChanges() => !_saved;

        public override void RepairAfterParse() { }

        protected override void DoBackGroundWork(BackgroundWorker listenToMyCancel) { }

        protected override void DoWorkAfterSaving() => _saved = true;

        protected override bool IsThereBackgroundWorkToDo() => false;

        protected override void ParseExternal(byte[] bLoaded) {
            var ToParse = bLoaded.ToStringWin1252();
            if (string.IsNullOrEmpty(ToParse)) { return; }

            foreach (var pair in ToParse.GetAllTags()) {
                switch (pair.Key.ToLower()) {
                    case "type":
                        break;

                    case "version":
                        break;

                    case "filepath":
                        FilePath = pair.Value.FromNonCritical();
                        break;

                    case "DatabaseFiles":
                        DatabaseFiles.Clear();
                        DatabaseFiles.AddRange(pair.Value.FromNonCritical().SplitByCrToList());
                        break;

                    default:
                        DebugPrint(enFehlerArt.Warnung, "Tag unbekannt: " + pair.Key);
                        break;
                }
            }
        }

        protected override byte[] ToListOfByte() {
            var t = new List<string>();

            t.TagSet("Type", "ConnectedFormula");
            t.TagSet("Version", Version);
            t.TagSet("FilePath", FilePath.ToNonCritical());
            t.TagSet("DatabaseFiles", DatabaseFiles.JoinWithCr().ToNonCritical());

            return t.JoinWithCr().WIN1252_toByte();
        }

        private void DatabaseFiles_Changed(object sender, System.EventArgs e) {
            Databases.Clear();

            foreach (var thisfile in DatabaseFiles) {
                var mf = Database.GetByFilename(thisfile, false, false);

                if (mf is BlueDatabase.Database db) {
                    Databases.Add(db);
                } else {
                    Databases.Add(null);
                }
            }

            _saved = false;
        }

        #endregion
    }
}