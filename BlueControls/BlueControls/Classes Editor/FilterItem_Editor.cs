using System.Drawing;
using BlueBasics;
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Forms;
using BlueControls.EventArgs;
using BlueDatabase;
using BlueDatabase.Enums;

namespace BlueControls.Classes_Editor
{
    internal sealed partial class FilterItem_Editor :  AbstractClassEditor<FilterItem> //System.Windows.Forms.UserControl // :
    {
        public FilterItem_Editor() : base()
        {
            InitializeComponent();
        }

        private AutoFilter autofilter;


        protected override void PrepaireFormula()
        {
            Col.Item.AddRange(Item.Database.Column, false, false,true);
        }

        protected override void EnabledAndFillFormula()
        {
            Enabled = true;

            if (Item?.Column == null)
            {
                Col.Text = string.Empty;
                return;
            }

            Col.Text = Item.Column.Name;

        }

        protected override void DisableAndClearFormula()
        {
            Enabled = false;
            Col.Text = string.Empty;
        }






        private void FiltWahl_Click(object sender, System.EventArgs e)
        {

            var c = Item.Database.Column[Col.Text];


            if (c == null || !c.AutoFilter_möglich()) { return; }

            var tmpfc = new FilterCollection(Item.Database);
            if (Item.FilterType != enFilterType.KeinFilter) { tmpfc.Add(Item); }

            autofilter = new AutoFilter(c, tmpfc);

            var p = FiltWahl.PointToScreen(Point.Empty);

            autofilter.Position_LocateToPosition(new Point(p.X, p.Y + FiltWahl.Height));

            autofilter.Show();

            autofilter.FilterComand += AutoFilter_FilterComand;
            Develop.Debugprint_BackgroundThread();


        }


        private void AutoFilter_FilterComand(object sender, FilterComandEventArgs e)
        {
            if (IsFilling) { return; }

            if (e.Comand != "Filter")
            {
                Notification.Show("Diese Funktion wird nicht unterstützt,<br>abbruch.");
                return;
            }


            Item.FilterType = e.Filter.FilterType;
            Item.SearchValue.Clear();
            Item.SearchValue.AddRange(e.Filter.SearchValue);

            OnChanged(Item);
        }

        private void Col_TextChanged(object sender, System.EventArgs e)
        {
            if (IsFilling) { return; }

            var c = Item.Database.Column[Col.Text];


            if (c == null || c.AutoFilter_möglich())
            {
                FiltWahl.Enabled = true;
            }
            else
            {
                FiltWahl.Enabled = true;
            }


            Item.Column = c;
            Item.FilterType = enFilterType.KeinFilter;
            Item.SearchValue.Clear();


            OnChanged(Item);
        }
    }
}
