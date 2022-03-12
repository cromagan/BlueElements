using BlueScript;
using System.Windows.Forms;
using BlueScript.Methods;

namespace BlueControls {

    public partial class Befehlsreferenz : Form {

        #region Constructors

        public Befehlsreferenz() {
            InitializeComponent();

            WriteComandsToList();
        }

        #endregion

        #region Methods

        protected void WriteComandsToList() {
            lstComands.Item.Clear();

            if (Script.Comands == null) { return; }

            foreach (var thisc in Script.Comands) {
                lstComands.Item.Add(thisc, thisc.Syntax.ToLower());
            }

            lstComands.Item.Sort();
        }

        private void lstComands_ItemClicked(object sender, EventArgs.BasicListItemEventArgs e) {
            var co = string.Empty;
            if (e.Item != null && e.Item.Tag is Method thisc) {
                co += thisc.HintText();
            }
            txbComms.Text = co;
        }

        #endregion
    }
}