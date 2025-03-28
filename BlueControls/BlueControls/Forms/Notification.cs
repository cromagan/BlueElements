﻿// Authors:
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

using BlueBasics.Enums;
using BlueControls.Enums;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.Forms;

public partial class Notification : FloatingForm {

    #region Fields

    private const double SpeedIn = 250d;

    // Wegen Recheoperation
    private const double SpeedOut = 250d;

    private const bool UserClicked = false;

    // Wegen Recheoperation
    private readonly DateTime _firstTimer = DateTime.UtcNow;

    private readonly int _lowestY;
    private readonly int _screenTime = -999;
    private bool _hiddenNow;
    private bool _isIn;
    private DateTime _outime = new(0);

    #endregion

    #region Constructors

    // Startzeit für UnloadAfterSek
    private Notification() : base(Design.Form_DesktopBenachrichtigung) => InitializeComponent();

    private Notification(string text) : this() {
        capTXT.Text = text;
        var he = Math.Min(capTXT.RequiredTextSize().Height, (int)(Screen.PrimaryScreen.Bounds.Size.Height * 0.7));
        var wi = Math.Min(capTXT.RequiredTextSize().Width, (int)(Screen.PrimaryScreen.Bounds.Size.Width * 0.7));
        Size = new Size(wi + (capTXT.Left * 2), he + (capTXT.Top * 2));
        Location = new Point(-Width - 10, Height - 10);
        _screenTime = Math.Max(3200, text.Length * 100);
        _screenTime = Math.Min(20000, _screenTime);

        //Below müsste in Allboxes ja die letzte sein - außer sich selbst
        foreach (var thisParent in AllBoxes) {
            if (thisParent is Notification nf) {
                if (nf != this && nf is { Visible: true, IsDisposed: false }) {
                    NoteBelow = nf;
                }
            }
        }

        _lowestY = Screen.PrimaryScreen.WorkingArea.Bottom - Height - 1;// - Skin.Padding;
        //var pixelfromLower = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Bottom - lowestY;
        Top = _lowestY;
        Opacity = 0.001;
        //Visible = true; // %$§!% !!!!!!!

        while (Opacity < 1) {
            Timer_Tick(null, System.EventArgs.Empty);
            Refresh();
            //Develop.DoEvents();
            if (_hiddenNow) { return; }
        }

        _firstTimer = DateTime.UtcNow;
        timNote.Enabled = true;
    }

    #endregion

    #region Properties

    public Notification? NoteBelow { get; }

    #endregion

    #region Methods

    public static void Show(string text) {
        if (string.IsNullOrEmpty(text)) { return; }
        Notification x = new(text);
        x.Show();
    }

    public static void Show(string text, ImageCode? img) {
        if (img != null) {
            text = "<ImageCode=" + Enum.GetName(img.GetType(), img) + "|32> <zbx_store><top>" + text;
        }
        Show(text);
    }

    private void capTXT_Click(object sender, System.EventArgs e) { }

    private void Timer_Tick(object? sender, System.EventArgs e) {
        if (_isIn) { return; }
        _isIn = true;

        try {
            var ms = DateTime.UtcNow.Subtract(_firstTimer).TotalMilliseconds;

            #region Anzeige-Status (Richtung, Prozent) bestimmen

            var hasBelow = false;

            var newLeft = Screen.PrimaryScreen.Bounds.Size.Width - Width - 1;
            var newTop = _lowestY;

            if (NoteBelow != null && AllBoxes.Contains(NoteBelow)) {
                newTop = Math.Min(NoteBelow.Top - Height - 1, _lowestY);
                hasBelow = true;
            }

            if (ms < SpeedIn) {
                // Kommt von Rechts reingeflogen
                Opacity = ms / SpeedIn;
                //Left = (int)(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size.Width - (x.Width - (Skin.Padding * 2)) * x.Opacity); // Opacity == Prozent
                newTop = Math.Min(newTop, _lowestY);
            } else if (Top >= Screen.PrimaryScreen.Bounds.Size.Height || Opacity < 0.01) {
                //Lebensdauer überschritten
                _hiddenNow = true;
            } else if (ms > _screenTime - SpeedIn) {
                // War lange genug da, darf wieder raus
                if (!hasBelow) {
                    if (_outime.Ticks == 0) { _outime = DateTime.UtcNow; }

                    var mSo = DateTime.UtcNow.Subtract(_outime).TotalMilliseconds;

                    Opacity = 1 - (mSo / SpeedOut);
                    //Top = (int)(lowestY + pixelfromLower * (MSo / SpeedOut)) + 1;
                    //Left = x.Left + (int)Math.Max(diff / 17, 1);
                } else {
                    Opacity = 1;
                }
            } else {
                //Hauptanzeige ist gerade
                Opacity = 1;
                newTop = Math.Min(newTop, _lowestY);
            }

            #endregion

            if (Left != newLeft || Top != newTop) {
                Left = newLeft;
                //x.Region = new Region(new Rectangle(0, 0, x.Width, (int)Math.Truncate(x.Height * Prozent)));
                Top = newTop;
                //x.Refresh();
                //Develop.DoEvents();
            }

            if (_firstTimer.Subtract(DateTime.UtcNow).TotalMinutes > 2 || UserClicked) { _hiddenNow = true; }
        } catch { }

        if (_hiddenNow) {
            try {
                if (sender is Timer tim) {
                    tim.Enabled = false;
                    tim.Tick -= Timer_Tick;
                }

                Visible = false;
                Close();
                if (!IsDisposed) { Dispose(); }
            } catch { }
        }

        _isIn = false;
    }

    #endregion

    //public static void CloseAll() {
    //    foreach (var ThisForm in AllBoxes) {
    //        if (!ThisForm.IsDisposed && ThisForm is Notification) {
    //            try {
    //                ThisForm.Visible = false;
    //                ThisForm.Close();
    //                CloseAll();
    //                return;
    //            } catch (Exception ex) {
    //                Develop.DebugPrint(ex);
    //            }
    //        }
    //    }
    //}
    //private static void Bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
    //    try {
    //        if (e. is Notification x) {
    //        }

    //            if (x != null && !x.IsDisposed) {
    //            x.Close();
    //            x?.Dispose();
    //        }

    //    }
    //    }
    //public new void Close() {
    //    foreach (var ThisForm in AllBoxes) {
    //        if (!ThisForm.IsDisposed && ThisForm is Notification) {
    //            try {
    //                base.Close();
    //                return;
    //            } catch (Exception ex) {
    //                Develop.DebugPrint(ex);
    //            }
    //        }
    //    }
    //}

    //private void _Timer_Tick(object sender, System.EventArgs e)
    //{
    //    var MS = DateTime.UtcNow.Subtract(_FirstTimer).TotalMilliseconds;
    //    var Going = false;
    //    _Timer_Tick_count++;
    //    if (Tag is System.Windows.Forms.Control tempVar)
    //    {
    //        if (!tempVar.Visible)
    //        {
    //            _Timer_Tick_count = 0;
    //            Close();
    //            return;
    //        }
    //    }
    //    if (FloatInAndOutMilliSek > 0)
    //    {
    //        // Da das System oft ausgelastet ist, erst ein paar Dummy-Leerläufe, daß das Fenster dann angezeigt wird, wenn das System Luft hat
    //        if (_Timer_Tick_count == 6)
    //        {
    //            _Timer_Tick_WasVisible = false;
    //            _FirstTimer = DateTime.UtcNow;
    //        }
    //        if (_Timer_Tick_count < 7) { return; }
    //    }
    //    if (UnloadAfterSek > 0)
    //    {
    //        if (DateTime.UtcNow.Subtract(_FirstTimer).TotalSeconds > UnloadAfterSek)
    //        {
    //            Close();
    //            OnCancel();
    //            return;
    //        }
    //    }
    //    if (FloatInAndOutMilliSek > 0)
    //    {
    //        double _Proz = 0;
    //        if (MS > UpDownSpeed + FloatInAndOutMilliSek * 0.3 && !_Timer_Tick_WasVisible)
    //        {
    //            _FirstTimer = DateTime.UtcNow.Subtract(new TimeSpan(0, 0, 0, 0, UpDownSpeed));
    //            MS = UpDownSpeed;
    //        }
    //        if (MS < UpDownSpeed)
    //        {
    //            _Proz = MS / UpDownSpeed;
    //        }
    //        else if (MS >= UpDownSpeed && MS <= UpDownSpeed + FloatInAndOutMilliSek)
    //        {
    //            _Proz = 1;
    //            _Timer_Tick_WasVisible = true;
    //        }
    //        else if (MS > UpDownSpeed + FloatInAndOutMilliSek && MS < FloatInAndOutMilliSek + UpDownSpeed * 2)
    //        {
    //            _Proz = (FloatInAndOutMilliSek + UpDownSpeed * 2 - MS) / UpDownSpeed;
    //            Going = true;
    //        }
    //        else
    //        {
    //            _Timer_Tick_count = 0;
    //            Close();
    //            return;
    //        }
    //        if (this.Design == enDesign.Form_DesktopBenachrichtigung)
    //        {
    //            Left = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size.Width - Width - Skin.Padding * 2;
    //            Region = new Region(new Rectangle(0, 0, Width, (int)(Math.Truncate(Height * _Proz))));
    //            Top = (int)(Math.Truncate(System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Bottom - Height * _Proz));
    //            Opacity = _Proz;
    //        }
    //        else
    //        {
    //            Opacity = _Proz;
    //            if (Going)
    //            {
    //                Top -= (int)((1 - _Proz) * 5);
    //            }
    //        }
    //    }
    //}
    //public void FadeOut() {
    //    if (FloatInAndOutMilliSek > 300) {
    //        if (DateTime.UtcNow.Subtract(_FirstTimer).TotalMilliseconds > UpDownSpeed && DateTime.UtcNow.Subtract(_FirstTimer).TotalMilliseconds < FloatInAndOutMilliSek + UpDownSpeed) {
    //            _FirstTimer = DateTime.UtcNow.Subtract(new TimeSpan(0, 0, 0, 0, UpDownSpeed + 10));
    //        }
    //        FloatInAndOutMilliSek = 300;
    //    }
    //}
}