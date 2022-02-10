using BlueScript;
using System.Windows.Forms;

namespace BlueControls {

    public partial class frmBefehlsreferenz : Form {

        #region Constructors

        public frmBefehlsreferenz() {
            InitializeComponent();

            WriteComandsToList();
        }

        #endregion

        #region Methods

        protected void WriteComandsToList() {
            lstComands.Item.Clear();

            foreach (var thisc in Script.Comands) {
                lstComands.Item.Add(thisc, thisc.Syntax.ToLower());
            }

            lstComands.Item.Sort();
        }

        private void lstComands_ItemClicked(object sender, EventArgs.BasicListItemEventArgs e) {
            var co = string.Empty;
            if (e.Item.Tag is Method thisc) {
                co += thisc.HintText();
            }
            txbComms.Text = co;
        }

        #endregion
    }
}