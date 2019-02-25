#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2019 Christian Peter
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

using System.Collections.Generic;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.EventArgs;
using BlueControls.ItemCollection;
using BlueDatabase;


namespace BlueControls.Forms
    {
        public sealed partial class ItemSelect
        {
            public ItemSelect()
            {
                InitializeComponent();
            }

            private BasicListItem ClickedItem;


            private void List_Item_Click(object sender, BasicListItemEventArgs e)
            {
                ClickedItem = e.Item;
                Close();
            }

            public string SelectOne_OfDataSystem(List<string> Files, string FileEncryptionKey)
            {

                //    Dim l As List(Of String) = DataSystem.GetFiles()

                foreach (var ThisString in Files)
                {
                    if (ThisString.FileType() == enFileFormat.Image)
                    {
                        List.Item.Add(new BitmapListItem(ThisString, ThisString.FileNameWithoutSuffix(), ThisString, FileEncryptionKey));
                    }
                }

                ShowDialog();

                if (ClickedItem != null)
                {
                    return ClickedItem.Internal();
                }


                return string.Empty;
            }


            public RowItem SelectOne_OfRow(List<RowItem> Rows, string TextOfNewRow, int LayoutNr, bool AllowClose)
            {

                List.Item.Clear();

                foreach (var ThisRow in Rows)
                {
                    var x = new RowFormulaListItem(ThisRow);
                    x.LayoutNr = LayoutNr;
                    List.Item.Add(x);
                }


                if (!string.IsNullOrEmpty(TextOfNewRow))
                {
                    List.Item.Add(new BitmapListItem(QuickImage.Get("Kreuz|128").BMP, TextOfNewRow));
                }

                do
                {
                    ShowDialog();

                    if (ClickedItem != null)
                    {
                        break;
                    }
                    if (AllowClose)
                    {
                        break;
                    }

                } while (true);

                return (ClickedItem as RowFormulaListItem)?.Row;
            }
        }
    }