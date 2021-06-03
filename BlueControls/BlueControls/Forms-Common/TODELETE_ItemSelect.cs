#region BlueElements - a collection of useful tools, database and controls
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
#endregion

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.EventArgs;
using BlueControls.ItemCollection;
using BlueDatabase;
using System.Collections.Generic;

namespace BlueControls.Forms {
    public sealed partial class ItemSelect {
        public ItemSelect() {
            InitializeComponent();
        }

        private BasicListItem ClickedItem;

        private void List_ItemClicked(object sender, BasicListItemEventArgs e) {
            ClickedItem = e.Item;
            Close();
        }

        public string SelectOne_OfDataSystem(List<string> Files, string FileEncryptionKey) {

            //    Dim l As List(Of String) = DataSystem.GetFiles()

            foreach (var ThisString in Files) {
                if (ThisString.FileType() == enFileFormat.Image) {
                    List.Item.Add(ThisString, ThisString, ThisString.FileNameWithoutSuffix(), FileEncryptionKey);
                }
            }

            ShowDialog();

            return ClickedItem != null ? ClickedItem.Internal : string.Empty;
        }

        public RowItem SelectOne_OfRow(List<RowItem> rows, string textOfNewRow, string layoutID, bool allowClose) {

            List.Item.Clear();

            foreach (var ThisRow in rows) {
                List.Item.Add(ThisRow, layoutID);
            }

            if (!string.IsNullOrEmpty(textOfNewRow)) {
                List.Item.Add(QuickImage.Get("Kreuz|128").BMP, textOfNewRow);
            }

            do {
                ShowDialog();
                if (ClickedItem != null) { break; }
                if (allowClose) { break; }
            } while (true);

            return (ClickedItem as RowFormulaListItem)?.Row;
        }
    }
}