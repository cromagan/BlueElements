using BlueBasics;
using BlueControls.BlueDatabaseDialogs;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueDatabase;
using BlueDatabase.Enums;
using System.Drawing;
using static BlueBasics.Converter;

namespace BlueControls.Classes_Editor {

    internal sealed partial class FilterItem_Editor : AbstractClassEditor<FilterItem> //System.Windows.Forms.UserControl // :
    {
        #region Fields

        private AutoFilter _autofilter;

        #endregion

        #region Constructors

        public FilterItem_Editor() : base() => InitializeComponent();

        #endregion

        #region Methods

        protected override void DisableAndClearFormula() {
            Enabled = false;
            cbxColumns.Text = string.Empty;
        }

        protected override void EnabledAndFillFormula() {
            Enabled = true;
            if (Item?.Column == null) {
                cbxColumns.Text = string.Empty;
                return;
            }
            cbxColumns.Text = Item.Column.Key.ToString();
        }

        protected override void PrepaireFormula() => cbxColumns.Item.AddRange(Item.Database.Column, true);

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

        private void btnFilterWahl_Click(object sender, System.EventArgs e) {
            var c = Item.Database.Column.SearchByKey(LongParse(cbxColumns.Text));
            if (c == null || !c.AutoFilterSymbolPossible()) { return; }
            FilterCollection tmpfc = new(Item.Database);
            if (Item.FilterType != enFilterType.KeinFilter) { tmpfc.Add(Item); }
            _autofilter = new AutoFilter(c, tmpfc, null);
            var p = btnFilterWahl.PointToScreen(Point.Empty);
            _autofilter.Position_LocateToPosition(new Point(p.X, p.Y + btnFilterWahl.Height));
            _autofilter.Show();
            _autofilter.FilterComand += AutoFilter_FilterComand;
            Develop.Debugprint_BackgroundThread();
        }

        private void cbxColumns_TextChanged(object sender, System.EventArgs e) {
            if (IsFilling) { return; }
            var c = Item.Database.Column.SearchByKey(LongParse(cbxColumns.Text));
            btnFilterWahl.Enabled = c == null || c.AutoFilterSymbolPossible() || true;
            Item.Column = c;
            Item.FilterType = enFilterType.KeinFilter;
            Item.SearchValue.Clear();
            OnChanged(Item);
        }

        #endregion
    }
}