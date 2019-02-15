using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueControls.ItemCollection.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;

namespace BlueControls.BlueDatabaseDialogs
{

    public partial class FormulaQuickSelect
    {

        private readonly RowItem Row;

        public FormulaQuickSelect(RowItem RowItem)
        {

            // Dieser Aufruf ist für den Designer erforderlich.
            InitializeComponent();

            // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
            Row = RowItem;
        }


        private void Such_TextChanged(object sender, System.EventArgs e)
        {


            Auswahl.Item.Clear();

            var t = Such.Text;

            if (string.IsNullOrEmpty(t)) { return; }

            t = t.ToLower();


            foreach (var ThisColumn in Row.Database.Column)
            {
                if (ThisColumn != null)
                {
                    if (ThisColumn.EditType == enEditTypeFormula.Listbox_1_Zeile || ThisColumn.EditType == enEditTypeFormula.Listbox_3_Zeilen || ThisColumn.EditType == enEditTypeFormula.Listbox_6_Zeilen || ThisColumn.EditType == enEditTypeFormula.Textfeld_mit_Auswahlknopf)
                    {
                        if (ThisColumn.DropdownBearbeitungErlaubt)
                        {
                            if (Row.Database.Cell.UserEditPossible(ThisColumn, Row, true))
                            {
                                var ThisView = Formula.SearchColumnView(ThisColumn);
                                if (ThisView != null)
                                {
                                    if (Row.Database.PermissionCheck(ThisView.PermissionGroups_Show, null))
                                    {
                                        var dummy = new ItemCollectionList();

                                        dummy.GetItemCollection(ThisColumn, dummy, Row, enShortenStyle.Both);
                                        if (dummy.Count > 0)
                                        {

                                            foreach (var thisItem in dummy)
                                            {

                                                if (thisItem.Internal().ToLower().Contains(t))
                                                {

                                                    var ni = new TextListItem(ThisColumn.Name.ToUpper() + "|" + thisItem.Internal(), ThisColumn.ReadableText() + ": " + thisItem.Internal());
                                                    Auswahl.Item.Add(ni);

                                                    ni.Checked = thisItem.Checked;

                                                }

                                            }

                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        //Private Sub Auswahl_Item_CheckedChanged(sender As Object) Handles Auswahl.Item_CheckedChanged





        //End Sub

        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);
            Init();
        }

        private void Init()
        {


            if (Row == null)
            {
                Close();
                return;
            }



            Für.Text = "<b>" + Row.CellFirstString();





        }





        private void Auswahl_Item_Click(object sender, BasicListItemEventArgs e)
        {

            var x = e.Item.Internal().SplitBy("|");




            if (Row.Database.Column[x[0]].MultiLine)
            {

                var val = Row.CellGetList(Row.Database.Column[x[0]]);
                if (e.Item.Checked)
                {
                    val.AddIfNotExists(x[1]);
                }
                else
                {
                    val.Remove(x[1]);
                }

                Row.CellSet(Row.Database.Column[x[0]], val);
            }
            else
            {


                if (e.Item.Checked)
                {
                    Row.CellSet(Row.Database.Column[x[0]], x[1]);
                }

            }


            //       End If

            Such_TextChanged(null, System.EventArgs.Empty);


        }
    }
}