using BlueBasics;
using BlueControls.Controls;
using System;
using System.Collections.Generic;

namespace BlueControls.Forms {
    public partial class EditBoxFlexiControl : DialogWithOkAndCancel {
        private List<string> GiveBack = null;

        #region Konstruktor
        private EditBoxFlexiControl() : base() => InitializeComponent();

        private EditBoxFlexiControl(List<FlexiControl> Flexis) : this() {

            var top = Skin.Padding;
            var we = 300 + (Skin.Padding * 4);

            foreach (var ThisFlexi in Flexis) {
                Controls.Add(ThisFlexi);
                ThisFlexi.DisabledReason = string.Empty;
                ThisFlexi.Left = Skin.Padding;
                ThisFlexi.Top = top;
                ThisFlexi.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
                top = top + Skin.Padding + ThisFlexi.Height;
                we = Math.Max(we, ThisFlexi.Width + (Skin.Padding * 4));

                ThisFlexi.ButtonClicked += FlexiButtonClick;
            }

            we = Math.Min(we, 1500);

            foreach (var ThisFlexi in Flexis) {
                ThisFlexi.Width = Width - (Skin.Padding * 4);
            }

            Setup(we, top, true, false);
        }

        #endregion

        public static List<string> Show(List<FlexiControl> Flexis) {
            var MB = new EditBoxFlexiControl(Flexis);
            MB.ShowDialog();
            return MB.GiveBack;
        }

        protected override void SetValue(bool canceled) {
            GiveBack = new List<string>();

            if (!canceled) {
                foreach (var thisObj in Controls) {

                    if (thisObj is FlexiControl ThisFlexi) {

                        if (!string.IsNullOrEmpty(ThisFlexi.ValueId)) {
                            GiveBack.TagSet(ThisFlexi.ValueId, ThisFlexi.Value.ToNonCritical());
                        }
                    }
                }
            }
        }

        private void FlexiButtonClick(object sender, System.EventArgs e) => Ok();
    }
}
