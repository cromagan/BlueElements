// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BlueControls.Forms;

public partial class FormWithStatusBar : Form {

    #region Fields

    /// <summary>
    /// FormManager kennt manche Forms gar nicht, z.b. Splash Screen. Deswegen eigene Collection
    /// </summary>
    private static readonly List<FormWithStatusBar> _formsWithStatusBar = [];

    /// <summary>
    /// Lock für Thread-Sicherheit bei Handler-Registrierung
    /// </summary>
    private static readonly object _handlerLock = new();

    /// <summary>
    /// Tracks ob der statische Handler bereits registriert wurde
    /// </summary>
    private static bool _staticHandlerRegistered;

    private DateTime _lastMessage = DateTime.UtcNow;

    #endregion

    #region Constructors

    public FormWithStatusBar() : base() {
        InitializeComponent();
        _formsWithStatusBar.AddIfNotExists(this);

        // Handler nur einmal statisch registrieren
        RegisterStaticHandler();
    }

    #endregion

    #region Properties

    public bool DropMessages { get; set; } = true;

    public int MessageSeconds { get; set; } = 10;

    #endregion

    #region Methods

    /// <summary>
    /// Statischer Handler der an alle FormWithStatusBar-Instanzen weiterleitet
    /// </summary>
    public static void UpdateStatusBar(ErrorType type, object? reference, string category, ImageCode symbol, string message, int indent) {
        message = "[" + DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture) + "] " + message;

        List<FormWithStatusBar> l = [.. _formsWithStatusBar];

        var did = false;

        foreach (var thisf in l) {
            if (thisf is { Visible: true, IsDisposed: false }) {
                try {
                    var x = thisf.UpdateStatus(type, symbol, message, did);
                    if (x) { did = true; }
                } catch { }
            }
        }
    }

    /// <summary>
    /// Aktualisiert die Statusleiste dieser Form-Instanz
    /// </summary>
    public bool UpdateStatus(ErrorType type, ImageCode symbol, string message, bool didAlreadyMessagebox) {
        try {
            if (IsDisposed || Disposing || !IsHandleCreated) { return false; }
            if (DesignMode) { return false; }

            if (type == ErrorType.DevelopInfo) { return false; }

            if (InvokeRequired) {
                BeginInvoke(new Func<bool>(() => UpdateStatus(type, symbol, message, didAlreadyMessagebox)));
                return true;
            }

            if (capStatusBar.InvokeRequired) { return false; }

            _lastMessage = DateTime.UtcNow;
            timMessageClearer.Enabled = true;

            //var imagecode = ImageCode.Information;

            if (string.IsNullOrEmpty(message)) {
                capStatusBar.Text = string.Empty;
                capStatusBar.Refresh();
                return false;
            }

            message = message.Replace("\r\n", "; ");
            message = message.Replace("\r", "; ");
            message = message.Replace("\n", "; ");
            message = message.Replace("<br>", "; ", RegexOptions.IgnoreCase);
            message = message.Replace("; ; ", "; ");
            message = message.TrimEnd("; ");

            //if (type == ErrorType.Warning) { imagecode = ImageCode.Warnung; }
            //if (type == ErrorType.Error) { imagecode = ImageCode.Kritisch; }

            if (type is ErrorType.Info || !DropMessages || didAlreadyMessagebox) {
                capStatusBar.Text = QuickImage.Get(symbol, 16).HTMLCode + " " + message;
                capStatusBar.Refresh();
                return false;
            }

            if (DropMessages) {
                Develop.DebugPrint(ErrorType.Warning, message);

                if (type == ErrorType.Error) {
                    Notification.Show(message, symbol);
                    //MessageBox.Show(message, imagecode, "Ok");
                    return true;
                }
            }

            return false;
        } catch {
            return false;
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e) {
        _formsWithStatusBar.Remove(this);

        // Handler nur entfernen, wenn keine Forms mehr vorhanden sind
        lock (_handlerLock) {
            if (_formsWithStatusBar.Count == 0 && _staticHandlerRegistered) {
                Develop.MessageDG -= UpdateStatusBar;
                _staticHandlerRegistered = false;
            }
        }

        base.OnFormClosed(e);
    }

    /// <summary>
    /// Registriert den statischen Handler nur einmal
    /// </summary>
    private static void RegisterStaticHandler() {
        lock (_handlerLock) {
            if (!_staticHandlerRegistered) {
                Develop.MessageDG += UpdateStatusBar;
                _staticHandlerRegistered = true;
            }
        }
    }

    private void timMessageClearer_Tick(object sender, System.EventArgs e) {
        if (InvokeRequired) {
            Invoke(new Action(() => timMessageClearer_Tick(sender, e)));
            return;
        }

        if (IsDisposed) {
            timMessageClearer.Enabled = false;
            return;
        }
        if (capStatusBar.InvokeRequired) { return; }

        if (DateTime.UtcNow.Subtract(_lastMessage).TotalSeconds >= MessageSeconds) {
            timMessageClearer.Enabled = false;
            capStatusBar.Text = QuickImage.Get(ImageCode.Häkchen, 16).HTMLCode + " Nix besonderes zu berichten...";
            capStatusBar.Refresh();
        }
    }

    #endregion
}