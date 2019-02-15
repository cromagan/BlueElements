using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;

namespace BlueControls.DialogBoxes
{
    public partial class MessageBox : Forms.Form
    {

        Button Pressed = null;

        public MessageBox()
        {
            InitializeComponent();
        }

        public MessageBox(string TXT, enImageCode Pic, params string[] Buttons)
        {
            InitializeComponent();


            Text = Develop.AppName();

            if (Pic != enImageCode.None)
            {
                capText.Text = "<ImageCode=" + QuickImage.Get(Pic, 32) + "> <zbx_store><top>" + TXT;
            }
            else
            {
                capText.Text = TXT;
            }

            Size = new System.Drawing.Size(capText.Left * 2 + capText.Width + BorderWidth, capText.Top * 3 + capText.Height + 35 + BorderHeight);


            if (Buttons.Length == 0) { Buttons = new[] { "OK" }; }

            var B = Generate_Buttons(Buttons);

            foreach (var ThisButton in B)
            {
                ThisButton.Click += ThisButton_Click;

                if (ThisButton.Left < BorderWidth)
                {
                    this.Width = (this.Width - ThisButton.Left) + BorderWidth;
                }
            }
            Pressed = null;
        }

        private void ThisButton_Click(object sender, System.EventArgs e)
        {
            Pressed = (Button)sender;
            Close();
        }

        public static int Show(string TXT)
        {
            return Show(TXT, enImageCode.None, true, "OK");
        }


        public static int Show(string TXT, enImageCode Pic, params string[] Buttons)
        {
            return Show(TXT, Pic, true, Buttons);
        }

        public static int Show(string TXT, enImageCode Pic, bool Dialog, params string[] Buttons)
        {

            var MB = new MessageBox(TXT, Pic, Buttons);

            if (Dialog)
            {
                MB.ShowDialog();
            }
            else
            {
                MB.Show();

                while (MB.Pressed == null)
                {
                    modAllgemein.Pause(0.1, true);
                }

            }


            return int.Parse(MB.Pressed.Name);
        }
    }
}
