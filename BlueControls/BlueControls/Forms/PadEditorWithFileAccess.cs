﻿// Authors:
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
using BlueControls.EventArgs;
using BlueControls.ItemCollection;
using System.IO;
using static BlueBasics.FileOperations;

namespace BlueControls.Forms {

    public partial class PadEditorWithFileAccess : PadEditor {

        #region Constructors

        public PadEditorWithFileAccess() : base() => InitializeComponent();

        #endregion

        #region Methods

        /// <summary>
        /// löscht den kompletten Inhalt des Pads auch die ID und setzt es auf Disabled
        /// </summary>
        public void DisablePad() {
            Pad.Item = new ItemCollectionPad();
            Pad.Enabled = false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="useThisID">Wenn das Blatt bereits eine Id hat, muss die Id verwendet werden. Wird das Feld leer gelassen, wird die beinhaltete Id benutzt.</param>
        public void LoadFile(string fileName, string useThisID) {
            var t = File.ReadAllText(fileName, Constants.Win1252);
            LoadFromString(t, useThisID);
            btnLastFiles.AddFileName(fileName, fileName.FileNameWithSuffix());
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="useThisID">Wenn das Blatt bereits eine Id hat, muss die Id verwendet werden. Wird das Feld leer gelassen, wird die beinhaltete Id benutzt.</param>
        public void LoadFromString(string data, string useThisID) {
            Pad.Enabled = true;
            Pad.Item = new ItemCollectionPad(data, useThisID);
            ItemChanged();
        }

        private void btnLastFiles_ItemClicked(object sender, BasicListItemEventArgs e) => LoadFile(e.Item.Internal, string.Empty);

        private void btnNeu_Click(object sender, System.EventArgs e) {
            Pad.Item.Clear();
            Pad.ZoomFit();
        }

        private void btnOeffnen_Click(object sender, System.EventArgs e) {
            LoadTab.Tag = sender;
            LoadTab.ShowDialog();
        }

        private void btnSpeichern_Click(object sender, System.EventArgs e) => SaveTab.ShowDialog();

        private void LoadTab_FileOk(object sender, System.ComponentModel.CancelEventArgs e) {
            if (sender == btnOeffnen) {
                LoadFile(LoadTab.FileName, LoadTab.FileName);
            } else {
                // Kann nur der Import-Knopf sein
                LoadFile(LoadTab.FileName, string.Empty);
            }
        }

        private void SaveTab_FileOk(object sender, System.ComponentModel.CancelEventArgs e) {
            var t = Pad.Item.ToString();
            WriteAllText(SaveTab.FileName, t, Constants.Win1252, false);
            btnLastFiles.AddFileName(SaveTab.FileName, string.Empty);
        }

        #endregion
    }
}