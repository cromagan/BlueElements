// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

#nullable enable

using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Enums;
using BlueControls.Extended_Text;
using BlueDatabase;
using static BlueBasics.Converter;

namespace BlueControls.Forms;

public partial class MessageBox : Form {

    #region Fields

    private Controls.Button? _pressed;

    #endregion

    //private MessageBox()
    //{
    //    InitializeComponent();
    //    if (Owner == null)
    //    {
    //        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
    //    }
    //}

    #region Constructors

    private MessageBox(string txt, ImageCode? pic, params string[] buttons) : base(Design.Form_MsgBox) {
        InitializeComponent();
        Text = Develop.AppName();
        capText.Text = pic is { } im
            ? "<ImageCode=" + QuickImage.Get(im, 32) + "> <zbx_store><top>" + LanguageTool.DoTranslate(txt, false)
            : LanguageTool.DoTranslate(txt, false);
        Size = new Size((capText.Left * 2) + capText.Width + BorderWidth, (capText.Top * 3) + capText.Height + 35 + BorderHeight);
        if (buttons.Length == 0) { buttons = ["OK"]; }
        var b = Generate_Buttons(buttons);
        foreach (var thisButton in b) {
            thisButton.Click += ThisButton_Click;
            if (thisButton.Left < BorderWidth) {
                Width = Width - thisButton.Left + BorderWidth;
            }
        }
        _pressed = null;
        if (Owner == null) {
            StartPosition = FormStartPosition.CenterScreen;
        }
    }

    #endregion

    #region Properties

    public override sealed string Text {
        get => base.Text;
        set => base.Text = value;
    }

    #endregion

    #region Methods

    public static void Show(string txt) => Show(txt, null, true, "OK");

    public static void Show(string txt, ImageCode? pic, string buttons) => Show(txt, pic, true, buttons);

    public static int Show(string txt, ImageCode? pic, params string[] buttons) => Show(txt, pic, true, buttons);

    public static int Show(string txt, ImageCode? pic, bool dialog, params string[] buttons) {
        MessageBox mb = new(txt, pic, buttons);
        if (dialog) {
            _ = mb.ShowDialog();
        } else {
            mb.Show();
            while (mb._pressed == null) {
                Generic.Pause(0.1, true);
            }
        }

        return mb._pressed == null ? -1 : IntParse(mb._pressed.Name);
    }

    public List<Controls.Button> Generate_Buttons(string[] names) {
        var myX = Width - Skin.Padding - BorderWidth;
        ExtText erT = new(Design.Button, States.Standard);
        List<Controls.Button> buts = [];
        for (var z = names.GetUpperBound(0); z > -1; z--) {
            if (!string.IsNullOrEmpty(names[z])) {
                erT.TextDimensions = Size.Empty;
                erT.PlainText = names[z];
                Controls.Button b = new() {
                    Name = z.ToString(),
                    Text = names[z]
                };
                var w = 2;
                switch (b.Text.ToLowerInvariant()) {
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

                    case "beheben":
                    case "bearbeiten":
                        b.ImageCode = "Stift|16";
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

                    case "fortfahren":
                        b.ImageCode = "Abspielen|16";
                        w = 4;
                        break;

                    default:
                        b.ImageCode = string.Empty;
                        break;
                }
                b.Size = new Size(erT.Width() + (Skin.Padding * w), erT.Height() + (Skin.Padding * 2));
                b.Location = new Point(myX - b.Width, Height - BorderHeight - Skin.Padding - b.Height);
                myX = b.Location.X - Skin.Padding;
                b.ButtonStyle = ButtonStyle.Button;
                b.Visible = true;
                b.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                Controls.Add(b);
                buts.Add(b);
            }
        }
        return buts;
    }

    private void ThisButton_Click(object sender, System.EventArgs e) {
        _pressed = (Controls.Button)sender;
        Close();
    }

    #endregion
}