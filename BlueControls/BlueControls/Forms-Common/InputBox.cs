using BlueBasics.Enums;

namespace BlueControls.Forms {

    public partial class InputBox : DialogWithOkAndCancel {

        #region Fields

        private string GiveBack = string.Empty;

        #endregion

        #region Constructors

        private InputBox() : base() => InitializeComponent();

        private InputBox(string TXT, string VorschlagsText, enDataFormat Textformat, bool BigMultiLineBox) : this() {
            txbText.Text = VorschlagsText;
            txbText.Format = Textformat;
            txbText.MultiLine = BigMultiLineBox;
            if (BigMultiLineBox) { txbText.Height += 200; }
            Setup(TXT, txbText, 250, true, BigMultiLineBox);
            GiveBack = VorschlagsText;
        }

        #endregion

        #region Methods

        public static string Show(string TXT) => Show(TXT, "", enDataFormat.Text, false);

        public static string Show(string TXT, string VorschlagsText, enDataFormat Textformat) => Show(TXT, VorschlagsText, Textformat, false);

        public static string Show(string TXT, string VorschlagsText, enDataFormat Textformat, bool BigMultiLineBox) {
            InputBox MB = new(TXT, VorschlagsText, Textformat, BigMultiLineBox);
            MB.ShowDialog();
            return MB.GiveBack;
        }

        protected override void SetValue(bool canceled) => GiveBack = canceled ? string.Empty : txbText.Text;

        private void InputBox_Shown(object sender, System.EventArgs e) => txbText.Focus();

        private void txbText_Enter(object sender, System.EventArgs e) => Ok();

        private void txbText_ESC(object sender, System.EventArgs e) => Cancel();

        #endregion
    }
}