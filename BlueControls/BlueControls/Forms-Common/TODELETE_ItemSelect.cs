using System.Collections.Generic;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.EventArgs;
using BlueControls.ItemCollection.ItemCollectionList;
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