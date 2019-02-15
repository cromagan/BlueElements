using BlueBasics;
using BlueControls.Controls;
using System;
using System.Collections.Generic;

namespace BlueControls.DialogBoxes
{
    public partial class EditBoxFlexiControl : DialogBoxes.DialogWithOkAndCancel
    {

        public EditBoxFlexiControl()
        {
            InitializeComponent();
        }

        public EditBoxFlexiControl(List<FlexiControl> Flexis)
        {
            InitializeComponent();

            var top = GenericControl.Skin.Padding;
            var we = GenericControl.Skin.Padding * 2;

            foreach (var ThisFlexi in Flexis)
            {
                Controls.Add(ThisFlexi);
                ThisFlexi.Left = GenericControl.Skin.Padding;
                ThisFlexi.Top = top;
                ThisFlexi.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
                top = top + GenericControl.Skin.Padding + ThisFlexi.Height;
                we = Math.Max(we, ThisFlexi.Width + GenericControl.Skin.Padding * 2);

                ThisFlexi.ButtonClicked += FlexiButtonClick;
            }

            Setup(300, top, true, false);
        }


        public static List<string> Show(List<FlexiControl> Flexis)
        {
            var MB = new EditBoxFlexiControl(Flexis);
            MB.ShowDialog();


            var l = new List<string>();


            if (!MB.CancelPressed)
            {
                foreach (var ThisFlexi in Flexis)
                {
                    if (!string.IsNullOrEmpty(ThisFlexi.ValueId))
                    {
                        l.TagSet(ThisFlexi.ValueId, ThisFlexi.Value.ToNonCritical());
                    }
                }

            }

            return l;





        }





        protected override void SetValue()
        {
            // Ausnahme: Wird in Show-Dialog abgehandelt

        }

        private void FlexiButtonClick(object sender, System.EventArgs e)
        {
            Ok();
        }
    }
}
