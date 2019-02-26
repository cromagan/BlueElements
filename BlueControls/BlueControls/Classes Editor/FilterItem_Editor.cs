using System.Drawing;
using BlueBasics;
using BlueBasics.Interfaces;
using BlueControls.BlueDatabaseDialogs;
using BlueControls.DialogBoxes;
using BlueControls.EventArgs;
using BlueDatabase;
using BlueDatabase.Enums;

namespace BlueControls.Classes_Editor
{
    internal sealed partial class FilterItem_Editor
    {
        public FilterItem_Editor()
        {
            InitializeComponent();
        }

        private AutoFilter autofilter;

        private FilterItem tmp;


        protected override void ConvertObject(IObjectWithDialog ThisObject)
        {
            tmp = (FilterItem)ThisObject;
        }

        protected override void PrepaireFormula()
        {
            Col.Item.AddRange(tmp.Database.Column, false, false);
        }

        protected override void EnabledAndFillFormula()
        {
            Enabled = true;

            if (tmp?.Column == null)
            {
                Col.Text = string.Empty;
                return;
            }

            Col.Text = tmp.Column.Name;

        }

        protected override void DisableAndClearFormula()
        {
            Enabled = false;
            Col.Text = "";
        }






        private void FiltWahl_Click(object sender, System.EventArgs e)
        {

            var c = tmp.Database.Column[Col.Text];


            if (c == null || !c.AutoFilter_möglich()) { return; }

            var tmpfc = new FilterCollection(tmp.Database);
            if (tmp.FilterType != enFilterType.KeinFilter) { tmpfc.Add(tmp); }

            autofilter = new AutoFilter(c, tmpfc);

            var p = FiltWahl.PointToScreen(Point.Empty);

            autofilter.Position_LocateToPosition(new Point(p.X, p.Y + FiltWahl.Height));

            autofilter.Show();

            autofilter.FilterComand += AutoFilter_FilterComand;
            Develop.Debugprint_BackgroundThread();


        }


        private void AutoFilter_FilterComand(object sender, FilterComandEventArgs e)
        {
            if (IsFilling()) { return; }

            if (e.Comand != "Filter")
            {
                Notification.Show("Diese Funktion wird nicht unterstützt,<br>abbruch.");
                return;
            }


            tmp.FilterType = e.Filter.FilterType;
            tmp.SearchValue.Clear();
            tmp.SearchValue.AddRange(e.Filter.SearchValue);

            OnChanged(tmp);
        }

        private void Col_TextChanged(object sender, System.EventArgs e)
        {
            if (IsFilling()) { return; }

            var c = tmp.Database.Column[Col.Text];


            if (c == null || c.AutoFilter_möglich())
            {
                FiltWahl.Enabled = true;
            }
            else
            {
                FiltWahl.Enabled = true;
            }


            tmp.Column = c;
            tmp.FilterType = enFilterType.KeinFilter;
            tmp.SearchValue.Clear();


            OnChanged(tmp);
        }
    }
}
