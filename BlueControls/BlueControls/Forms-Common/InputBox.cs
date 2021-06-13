using BlueBasics.Enums;

namespace BlueControls.Forms {
    public partial class InputBox : DialogWithOkAndCancel {
        private string GiveBack = string.Empty;

        #region Konstruktor
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

        public static string Show(string TXT) => Show(TXT, "", enDataFormat.Text, false);
        public static string Show(string TXT, string VorschlagsText, enDataFormat Textformat) => Show(TXT, VorschlagsText, Textformat, false);
        public static string Show(string TXT, string VorschlagsText, enDataFormat Textformat, bool BigMultiLineBox) {
            InputBox MB = new(TXT, VorschlagsText, Textformat, BigMultiLineBox);
            MB.ShowDialog();
            return MB.GiveBack;
        }

        private void txbText_ESC(object sender, System.EventArgs e) => Cancel();
        private void txbText_Enter(object sender, System.EventArgs e) => Ok();
        private void InputBox_Shown(object sender, System.EventArgs e) => txbText.Focus();
        protected override void SetValue(bool canceled) => GiveBack = canceled ? string.Empty : txbText.Text;
    }
}
