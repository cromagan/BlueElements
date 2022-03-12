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
using static BlueBasics.Converter;

namespace BlueControls.Forms {

    public partial class MessageBox : Form {

        #region Fields

        private Button? _pressed;

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
        private MessageBox(string txt, enImageCode pic, params string[] buttons) : base(enDesign.Form_MsgBox) {
            InitializeComponent();
            Text = Develop.AppName();
            capText.Text = pic != enImageCode.None
                ? "<ImageCode=" + QuickImage.Get(pic, 32) + "> <zbx_store><top>" + BlueDatabase.LanguageTool.DoTranslate(txt, false)
                : BlueDatabase.LanguageTool.DoTranslate(txt, false);
            Size = new Size((capText.Left * 2) + capText.Width + BorderWidth, (capText.Top * 3) + capText.Height + 35 + BorderHeight);
            if (buttons.Length == 0) { buttons = new[] { "OK" }; }
            var b = Generate_Buttons(buttons);
            foreach (var thisButton in b) {
                thisButton.Click += ThisButton_Click;
                if (thisButton.Left < BorderWidth) {
                    Width = Width - thisButton.Left + BorderWidth;
                }
            }
            _pressed = null;
            if (Owner == null) {
                StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            }
        }

        #endregion

        #region Methods

        public static void Show(string txt) => Show(txt, enImageCode.None, true, "OK");

        public static void Show(string txt, enImageCode pic, string buttons) => Show(txt, pic, true, buttons);

        public static int Show(string txt, enImageCode pic, params string[] buttons) => Show(txt, pic, true, buttons);

        public static int Show(string txt, enImageCode pic, bool dialog, params string[] buttons) {
            MessageBox mb = new(txt, pic, buttons);
            if (dialog) {
                mb.ShowDialog();
            } else {
                mb.Show();
                while (mb._pressed == null) {
                    Generic.Pause(0.1, true);
                }
            }
            return IntParse(mb._pressed.Name);
        }

        public List<Button> Generate_Buttons(string[] names) {
            var myX = Width - Skin.Padding - BorderWidth;
            ExtText erT = new(enDesign.Button, enStates.Standard);
            List<Button> buts = new();
            for (var z = names.GetUpperBound(0); z > -1; z--) {
                if (!string.IsNullOrEmpty(names[z])) {
                    erT.TextDimensions = Size.Empty;
                    erT.PlainText = names[z];
                    Button b = new() {
                        Name = z.ToString(),
                        Text = names[z]
                    };
                    var w = 2;
                    switch (b.Text.ToLower()) {
                        case "ja":
                        case "ok":
                            b.ImageCode = "Häkchen|16";
                            w = 4;
                            break;

                        case "nein":
                        case "abbrechen":
                        case "abbruch":
                            b.ImageCode = "Kreuz|16";
                            w = 4;
                            break;

                        case "verwerfen":
                        case "löschen":
                            b.ImageCode = "Papierkorb|16";
                            w = 4;
                            break;

                        case "speichern":
                        case "sichern":
                            b.ImageCode = "Diskette|16";
                            w = 4;
                            break;

                        case "laden":
                            b.ImageCode = "Ordner|16";
                            w = 4;
                            break;

                        case "anpinnen":
                            b.ImageCode = "Pinnadel|16";
                            w = 4;
                            break;

                        default:
                            b.ImageCode = string.Empty;
                            break;
                    }
                    b.Size = new Size(erT.Width() + (Skin.Padding * w), erT.Height() + (Skin.Padding * 2));
                    b.Location = new Point(myX - b.Width, Height - BorderHeight - Skin.Padding - b.Height);
                    myX = b.Location.X - Skin.Padding;
                    b.ButtonStyle = enButtonStyle.Button;
                    b.Visible = true;
                    b.Anchor = System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom;
                    Controls.Add(b);
                    buts.Add(b);
                }
            }
            return buts;
        }

        private void ThisButton_Click(object sender, System.EventArgs e) {
            _pressed = (Button)sender;
            Close();
        }

        #endregion
    }
}