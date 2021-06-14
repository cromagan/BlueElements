using BlueBasics;
using BlueControls.BlueDatabaseDialogs;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueDatabase;
using BlueDatabase.Enums;
using System.Drawing;

namespace BlueControls.Classes_Editor {

    internal sealed partial class FilterItem_Editor : AbstractClassEditor<FilterItem> //System.Windows.Forms.UserControl // :
    {
        #region Fields

        private AutoFilter autofilter;

        #endregion

        #region Constructors

        public FilterItem_Editor() : base() => InitializeComponent();

        #endregion

        #region Methods

        protected override void DisableAndClearFormula() {
            Enabled = false;
            Col.Text = string.Empty;
        }

        protected override void EnabledAndFillFormula() {
            Enabled = true;
            if (Item?.Column == null) {
                Col.Text = string.Empty;
                return;
            }
            Col.Text = Item.Column.Name;
        }

        protected override void PrepaireFormula() => Col.Item.AddRange(Item.Database.Column, false, false, true);

        private void AutoFilter_FilterComand(object sender, FilterComandEventArgs e) {
            if (IsFilling) { return; }
            if (e.Comand != "Filter") {
                Notification.Show("Diese Funktion wird nicht unterstützt,<br>abbruch.");
                return;
            }
            Item.FilterType = e.Filter.FilterType;
            Item.SearchValue.Clear();
            Item.SearchValue.AddRange(e.Filter.SearchValue);
            OnChanged(Item);
        }

        private void Col_TextChanged(object sender, System.EventArgs e) {
            if (IsFilling) { return; }
            var c = Item.Database.Column[Col.Text];
            FiltWahl.Enabled = c == null || c.AutoFilterSymbolPossible() || true;
            Item.Column = c;
            Item.FilterType = enFilterType.KeinFilter;
            Item.SearchValue.Clear();
            OnChanged(Item);
        }

        private void FiltWahl_Click(object sender, System.EventArgs e) {
            var c = Item.Database.Column[Col.Text];
            if (c == null || !c.AutoFilterSymbolPossible()) { return; }
            FilterCollection tmpfc = new(Item.Database);
            if (Item.FilterType != enFilterType.KeinFilter) { tmpfc.Add(Item); }
            autofilter = new AutoFilter(c, tmpfc, null);
            var p = FiltWahl.PointToScreen(Point.Empty);
            autofilter.Position_LocateToPosition(new Point(p.X, p.Y + FiltWahl.Height));
            autofilter.Show();
            autofilter.FilterComand += AutoFilter_FilterComand;
            Develop.Debugprint_BackgroundThread();
        }

        #endregion
    }
}