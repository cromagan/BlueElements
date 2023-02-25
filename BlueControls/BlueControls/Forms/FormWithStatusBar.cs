// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

using System;
using System.Text.RegularExpressions;
using BlueBasics;
using BlueBasics.Enums;
using BlueDatabase.EventArgs;

namespace BlueControls.Forms;

public partial class FormWithStatusBar : Form {

    #region Fields

    private DateTime LastMessage = DateTime.UtcNow;

    #endregion

    #region Constructors

    public FormWithStatusBar() : base() => InitializeComponent();

    #endregion

    #region Properties

    public bool DropMessages { get; set; } = true;
    public int MessageSeconds { get; set; } = 10;

    #endregion

    #region Methods

    public static void UpdateStatusBar(string jobname, string text) {
        if (string.IsNullOrEmpty(jobname) && string.IsNullOrEmpty(text)) {
            UpdateStatusBar(FehlerArt.Info, string.Empty, false);
        } else {
            UpdateStatusBar(FehlerArt.Info, "[" + jobname + " " + DateTime.Now.ToString("HH:mm:ss") + "] " + text, false);
        }
    }

    public static void UpdateStatusBar(FehlerArt type, string text, bool addtime) {
        if (addtime && !string.IsNullOrEmpty(text)) {
            text = DateTime.Now.ToString("HH:mm:ss") + " " + text;
        }

        var did = false;
        try {
            foreach (var thisf in FormManager.Forms) {
                if (thisf is FormWithStatusBar fd) {
                    var x = fd.UpdateStatus(type, text, did);
                    if (x) { did = true; }
                }
            }
        } catch { }
    }

    public bool UpdateStatus(FehlerArt type, string message, bool didAlreadyMessagebox) {
        if (IsDisposed) { return false; }

        try {
            if (InvokeRequired) {
                return (bool)Invoke(new Func<bool>(() => UpdateStatus(type, message, didAlreadyMessagebox)));
            }

            LastMessage = DateTime.UtcNow;
            timMessageClearer.Enabled = true;

            var imagecode = ImageCode.Information;

            if (string.IsNullOrEmpty(message)) {
                capStatusBar.Text = string.Empty;
                capStatusBar.Refresh();
                return false;
            }

            message = message.Replace("\r\n", "; ");
            message = message.Replace("\r", "; ");
            message = message.Replace("\n", "; ");
            message = message.Replace("<BR>", "; ", RegexOptions.IgnoreCase);
            message = message.Replace("; ; ", "; ");
            message = message.TrimEnd("; ");

            if (type == FehlerArt.Warnung) { imagecode = ImageCode.Warnung; }
            if (type == FehlerArt.Fehler) { imagecode = ImageCode.Kritisch; }

            if (type == FehlerArt.Info || type == FehlerArt.DevelopInfo || !DropMessages || didAlreadyMessagebox) {
                capStatusBar.Text = "<imagecode=" + QuickImage.Get(imagecode, 16) + "> " + message;
                capStatusBar.Refresh();
                return false;
            }

            if (DropMessages) {
                Develop.DebugPrint(FehlerArt.Warnung, message);
                MessageBox.Show(message, imagecode, "Ok");
                return true;
            }

            return false;
        } catch {
            return false;
        }
    }

    internal static void Db_DropMessage(object sender, MessageEventArgs e) => UpdateStatusBar(e.Type, e.Message, true);

    private void timMessageClearer_Tick(object sender, System.EventArgs e) {
        if (IsDisposed) {
            timMessageClearer.Enabled = false;
            return;
        }

        if (DateTime.UtcNow.Subtract(LastMessage).TotalSeconds >= MessageSeconds) {
            timMessageClearer.Enabled = false;
            capStatusBar.Text = "<imagecode=" + QuickImage.Get(ImageCode.Häkchen, 16) + "> Nix besonderes zu berichten...";
            capStatusBar.Refresh();
        }
    }

    #endregion
}