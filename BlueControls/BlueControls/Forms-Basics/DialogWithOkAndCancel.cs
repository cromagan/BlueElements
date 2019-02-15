using BlueBasics;
using BlueControls.Controls;
using System;
using static BlueBasics.Develop;

namespace BlueControls.DialogBoxes
{
    public partial class DialogWithOkAndCancel : Forms.Form
    {




        protected bool CancelPressed = false;


        public DialogWithOkAndCancel()
        {
            InitializeComponent();
            SetTopLevel(true);
        }


        public void Setup(int MinWidth, int BottomOfLowestControl, bool CancelPossible, bool Sizeable)
        {

            Text = Develop.AppName();

            MinWidth = Math.Max(this.Width, MinWidth);


            Size = new System.Drawing.Size(MinWidth, BottomOfLowestControl + butOK.Height + BorderHeight);

            if (CancelPossible)
            {
                butAbbrechen.Left = MinWidth - GenericControl.Skin.Padding - butAbbrechen.Width - BorderWidth;
                butOK.Left = butAbbrechen.Left - GenericControl.Skin.Padding - butOK.Width;
            }
            else
            {
                butAbbrechen.Visible = false;
                butAbbrechen.Enabled = false;

                butOK.Left = MinWidth - GenericControl.Skin.Padding - butOK.Width - BorderWidth;
            }




            butOK.Top = BottomOfLowestControl;
            butAbbrechen.Top = BottomOfLowestControl;
            if (Sizeable)
            {
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            }


        }

        public void Setup(string TXT, GenericControl CenterControl, int MinWidth, bool CancelPossible, bool Sizeable)
        {
            var wi = GenericControl.Skin.Padding * 2;
            var he = GenericControl.Skin.Padding * 2;

            if (!string.IsNullOrEmpty(TXT))
            {
                capText.Visible = true;
                capText.Text = TXT;
                capText.Refresh();
                wi = wi + capText.Width;
                he = he + capText.Height;
            }


            if (CenterControl != null)
            {
                CenterControl.Top = he;
                he = he + CenterControl.Height + GenericControl.Skin.Padding;
            }



            wi = Math.Max(wi + BorderWidth, MinWidth);

            Setup(wi, he, CancelPossible, Sizeable);

            if (CenterControl != null)
            {
                CenterControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            }

        }


        protected virtual void SetValue()
        {
            DebugPrint_RoutineMussUeberschriebenWerden();
        }

        protected void Ok()
        {
            CancelPressed = false;
            SetValue();
            Close();
        }

        protected void Cancel()
        {
            CancelPressed = true;
            SetValue();
            Close();
        }

        private void butAbbrechen_Click(object sender, System.EventArgs e)
        {
            Cancel();
        }

        private void butOK_Click(object sender, System.EventArgs e)
        {
            Ok();
        }
    }
}
