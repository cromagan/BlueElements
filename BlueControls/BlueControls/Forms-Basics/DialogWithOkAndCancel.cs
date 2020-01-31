using BlueBasics;
using BlueControls.Controls;
using System;
using static BlueBasics.Develop;

namespace BlueControls.Forms
{
    public partial class DialogWithOkAndCancel : BlueControls.Forms.Form
    {




        #region Konstruktor


        public DialogWithOkAndCancel() : base()
        {
            InitializeComponent();
            SetTopLevel(true);


            if (Owner == null)
            {
                StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            }

        }

        #endregion

        public void Setup(int MinWidth, int BottomOfLowestControl, bool CancelPossible, bool Sizeable)
        {

            Text = Develop.AppName();

            MinWidth = Math.Max(this.Width, MinWidth);


            Size = new System.Drawing.Size(MinWidth, BottomOfLowestControl + butOK.Height + BorderHeight + Skin.Padding);

            if (CancelPossible)
            {
                butAbbrechen.Left = MinWidth - Skin.Padding - butAbbrechen.Width - BorderWidth;
                butOK.Left = butAbbrechen.Left - Skin.Padding - butOK.Width;
            }
            else
            {
                butAbbrechen.Visible = false;
                butAbbrechen.Enabled = false;

                butOK.Left = MinWidth - Skin.Padding - butOK.Width - BorderWidth;
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
            var wi = Skin.Padding * 2;
            var he = Skin.Padding * 2;

            if (!string.IsNullOrEmpty(TXT))
            {
                capText.Visible = true;
                capText.Translate = false;
                capText.Text = TXT;
                capText.Refresh();
                wi = wi + capText.Width;
                he = he + capText.Height;
            }


            if (CenterControl != null)
            {
                CenterControl.Top = he;
                he = he + CenterControl.Height + Skin.Padding;
            }



            wi = Math.Max(wi + BorderWidth, MinWidth);

            Setup(wi, he, CancelPossible, Sizeable);

            if (CenterControl != null)
            {
                CenterControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            }

        }


        /// <summary>
        /// Diese Routine wird aufgerufen, nachdem OK oder Cancel gedrückt wurde.
        /// </summary>
        protected virtual void SetValue(bool canceled)
        {
            DebugPrint_RoutineMussUeberschriebenWerden();
        }

        protected void Ok()
        {
            SetValue(false);
            Close();
        }

        protected void Cancel()
        {
            SetValue(true);
            Close();
        }

        private void butAbbrechen_Click(object sender, System.EventArgs e)
        {
            Cancel();
        }
        protected bool OK_Enabled
        {
            get
            {
                return butOK.Enabled;
            }
            set
            {
                butOK.Enabled = value;
            }
        }
        private void butOK_Click(object sender, System.EventArgs e)
        {
            Ok();
        }
    }
}
