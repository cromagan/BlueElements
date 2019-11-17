﻿using BlueBasics;
using BlueControls.Controls;
using System;
using System.Collections.Generic;

namespace BlueControls.Forms
{
    public partial class EditBoxFlexiControl : Forms.DialogWithOkAndCancel
    {

        private EditBoxFlexiControl()
        {
            InitializeComponent();
        }

        private EditBoxFlexiControl(List<FlexiControl> Flexis) : this()
        {

            var top = Skin.Padding;
            var we = 300 + Skin.Padding * 4;

            foreach (var ThisFlexi in Flexis)
            {
                Controls.Add(ThisFlexi);
                ThisFlexi.Enabled = true;
                ThisFlexi.Left = Skin.Padding;
                ThisFlexi.Top = top;
                ThisFlexi.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
                top = top + Skin.Padding + ThisFlexi.Height;
                we = Math.Max(we, ThisFlexi.Width + Skin.Padding * 4);

                ThisFlexi.ButtonClicked += FlexiButtonClick;
            }

            we = Math.Min(we, 1500);

            foreach (var ThisFlexi in Flexis)
            {
                ThisFlexi.Width = Width - Skin.Padding * 4;
            }


            Setup(we, top, true, false);
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
