// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Extended_Text;
using System.Collections.Generic;
using System.Drawing;

namespace BlueControls.Forms {

    public partial class MessageBox : Form {

        #region Fields

        private Button? Pressed;

        #endregion

        #region Constructors

        //private MessageBox()
        //{
        //    InitializeComponent();
        //    if (Owner == null)
        //    {
        //        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        //    }
        //}
        private MessageBox(string TXT, enImageCode Pic, params string[] Buttons) : base(enDesign.Form_MsgBox) {
            InitializeComponent();
            Text = Develop.AppName();
            capText.Text = Pic != enImageCode.None
                ? "<ImageCode=" + QuickImage.Get(Pic, 32) + "> <zbx_store><top>" + BlueDatabase.LanguageTool.DoTranslate(TXT, false)
                : BlueDatabase.LanguageTool.DoTranslate(TXT, false);
            Size = new Size((capText.Left * 2) + capText.Width + BorderWidth, (capText.Top * 3) + capText.Height + 35 + BorderHeight);
            if (Buttons.Length == 0) { Buttons = new[] { "OK" }; }
            var B = Generate_Buttons(Buttons);
            foreach (var ThisButton in B) {
                ThisButton.Click += ThisButton_Click;
                if (ThisButton.Left < BorderWidth) {
                    Width = Width - ThisButton.Left + BorderWidth;
                }
            }
            Pressed = null;
            if (Owner == null) {
                StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            }
        }

        #endregion

        #region Methods

        public static void Show(string TXT) => Show(TXT, enImageCode.None, true, "OK");

        public static void Show(string TXT, enImageCode Pic, string Buttons) => Show(TXT, Pic, true, Buttons);

        public static int Show(string TXT, enImageCode Pic, params string[] Buttons) => Show(TXT, Pic, true, Buttons);

        public static int Show(string TXT, enImageCode Pic, bool Dialog, params string[] Buttons) {
            MessageBox MB = new(TXT, Pic, Buttons);
            if (Dialog) {
                MB.ShowDialog();
            } else {
                MB.Show();
                while (MB.Pressed == null) {
                    Generic.Pause(0.1, true);
                }
            }
            return int.Parse(MB.Pressed.Name);
        }

        public List<Button> Generate_Buttons(string[] Names) {
            var MyX = Width - Skin.Padding - BorderWidth;
            ExtText erT = new(enDesign.Button, enStates.Standard);
            List<Button> Buts = new();
            for (var Z = Names.GetUpperBound(0); Z > -1; Z--) {
                if (!string.IsNullOrEmpty(Names[Z])) {
                    erT.TextDimensions = Size.Empty;
                    erT.PlainText = Names[Z];
                    Button B = new() {
                        Name = Z.ToString(),
                        Text = Names[Z]
                    };
                    var W = 2;
                    switch (B.Text.ToLower()) {
                        case "ja":
                        case "ok":
                            B.ImageCode = "Häkchen|16";
                            W = 4;
                            break;

                        case "nein":
                        case "abbrechen":
                        case "abbruch":
                            B.ImageCode = "Kreuz|16";
                            W = 4;
                            break;

                        case "verwerfen":
                        case "löschen":
                            B.ImageCode = "Papierkorb|16";
                            W = 4;
                            break;

                        case "speichern":
                        case "sichern":
                            B.ImageCode = "Diskette|16";
                            W = 4;
                            break;

                        case "laden":
                            B.ImageCode = "Ordner|16";
                            W = 4;
                            break;

                        case "anpinnen":
                            B.ImageCode = "Pinnadel|16";
                            W = 4;
                            break;

                        default:
                            B.ImageCode = string.Empty;
                            break;
                    }
                    B.Size = new Size(erT.Width() + (Skin.Padding * W), erT.Height() + (Skin.Padding * 2));
                    B.Location = new Point(MyX - B.Width, Height - BorderHeight - Skin.Padding - B.Height);
                    MyX = B.Location.X - Skin.Padding;
                    B.ButtonStyle = enButtonStyle.Button;
                    B.Visible = true;
                    B.Anchor = System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom;
                    Controls.Add(B);
                    Buts.Add(B);
                }
            }
            return Buts;
        }

        private void ThisButton_Click(object sender, System.EventArgs e) {
            Pressed = (Button)sender;
            Close();
        }

        #endregion
    }
}