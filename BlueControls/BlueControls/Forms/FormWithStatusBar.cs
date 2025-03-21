// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BlueControls.Forms;

public partial class FormWithStatusBar : Form {

    #region Fields

    /// <summary>
    /// FormManager kennt manche Forms gar nicht, z.b. Splash Screen. Deswegen eigene Collection
    /// </summary>
    private static readonly List<FormWithStatusBar> _formsWithStatusBar = [];

    private DateTime _lastMessage = DateTime.UtcNow;

    #endregion

    #region Constructors

    public FormWithStatusBar() : base() {
        InitializeComponent();
        _ = _formsWithStatusBar.AddIfNotExists(this);
    }

    #endregion

    #region Properties

    public bool DropMessages { get; set; } = true;

    public int MessageSeconds { get; set; } = 10;

    #endregion

    #region Methods

    public static void UpdateStatusBar(ErrorType type, string text, bool addtime) {
        switch (type) {
            case ErrorType.Error:
                Develop.MonitorMessage?.Invoke("Status-Bar, Fehler", "Kritisch", text, 0);
                break;

            case ErrorType.Warning:
                Develop.MonitorMessage?.Invoke("Status-Bar, Warnung", "Warnung", text, 0);
                break;

            default:
                Develop.MonitorMessage?.Invoke("Status-Bar, Info", "Information", text, 0);
                break;
        }

        if (addtime && !string.IsNullOrEmpty(text)) {
            text = DateTime.Now.ToString("HH:mm:ss") + " " + text;
        }

        var did = false;
        try {
            foreach (var thisf in _formsWithStatusBar) {
                if (thisf is { Visible: true }) {
                    var x = thisf.UpdateStatus(type, text, did);
                    if (x) { did = true; }
                }
            }
        } catch { }
    }

    public bool UpdateStatus(ErrorType type, string message, bool didAlreadyMessagebox) {
        try {
            if (IsDisposed) { return false; }
            if (DesignMode) { return false; }

            if (InvokeRequired) {
                return (bool)Invoke(new Func<bool>(() => UpdateStatus(type, message, didAlreadyMessagebox)));
            }

            if (capStatusBar.InvokeRequired) { return false; }

            _lastMessage = DateTime.UtcNow;
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

            if (type == ErrorType.Warning) { imagecode = ImageCode.Warnung; }
            if (type == ErrorType.Error) { imagecode = ImageCode.Kritisch; }

            if (type is ErrorType.Info || !DropMessages || didAlreadyMessagebox) {
                capStatusBar.Text = "<imagecode=" + QuickImage.Get(imagecode, 16) + "> " + message;
                capStatusBar.Refresh();
                return false;
            }

            if (DropMessages) {
                Develop.DebugPrint(ErrorType.Warning, message);

                if (type == ErrorType.Error) {
                    Notification.Show(message, imagecode);
                    //MessageBox.Show(message, imagecode, "Ok");
                    return true;
                }
            }

            return false;
        } catch {
            return false;
        }
    }

    internal static void GotMessageDropMessage(object sender, MessageEventArgs e) => UpdateStatusBar(e.Type, e.Message, true);

    protected override void OnFormClosed(FormClosedEventArgs e) {
        _ = _formsWithStatusBar.Remove(this);
        base.OnFormClosed(e);
    }

    private void timMessageClearer_Tick(object sender, System.EventArgs e) {
        if (InvokeRequired) {
            _ = Invoke(new Action(() => timMessageClearer_Tick(sender, e)));
            return;
        }

        if (IsDisposed) {
            timMessageClearer.Enabled = false;
            return;
        }
        if (capStatusBar.InvokeRequired) { return; }

        if (DateTime.UtcNow.Subtract(_lastMessage).TotalSeconds >= MessageSeconds) {
            timMessageClearer.Enabled = false;
            capStatusBar.Text = "<imagecode=" + QuickImage.Get(ImageCode.Häkchen, 16) + "> Nix besonderes zu berichten...";
            capStatusBar.Refresh();
        }
    }

    #endregion
}