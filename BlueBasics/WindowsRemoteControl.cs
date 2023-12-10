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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BlueBasics.Enums;

namespace BlueBasics;

public static class WindowsRemoteControl {

    #region Fields

    private const int KeyeventfExtendedkey = 0x1;

    private const int KeyeventfKeydown = 0x0;

    private const int KeyeventfKeyup = 0x2;

    #endregion

    #region Enums

    private enum InputType {
        INPUT_MOUSE = 0,
        INPUT_KEYBOARD = 1,
        INPUT_HARDWARE = 2
    }

    [Flags]
    private enum Mouseeventf {
        MOVE = 0x0001,  // mouse move
        LEFTDOWN = 0x0002,  // left button down
        LEFTUP = 0x0004,  // left button up
        RIGHTDOWN = 0x0008,  // right button down
        RIGHTUP = 0x0010,  // right button up
        MIDDLEDOWN = 0x0020,  // middle button down
        MIDDLEUP = 0x0040,  // middle button up
        XDOWN = 0x0080,  // x button down
        XUP = 0x0100,  // x button down
        WHEEL = 0x0800,  // wheel button rolled
        VIRTUALDESK = 0x4000,  // map to entire virtual desktop
        ABSOLUTE = 0x8000 // absolute move
    }

    #endregion

    #region Methods

    // Release key
    public static void AltRelease() => keybd_event((byte)KeyCode.VK_MENU, 0, KeyeventfExtendedkey | KeyeventfKeyup, 0);

    public static void FensterMaximieren(IntPtr handle) => ShowWindow(handle, (int)Sw.ShowMaximized);

    public static void FensterMinimieren(IntPtr handle) => ShowWindow(handle, (int)Sw.ShowMinimized);

    public static void FensterPosSetzen(IntPtr handle, int left, int top) {
        Rectangle r = new();
        _ = GetWindowRect(handle, ref r);
        if (r.Width == 0) { return; }
        if (r.Height == 0) { return; }
        _ = SetWindowPos(handle, 0, left, top, r.Width - r.Left, r.Height - r.Top, 0);
    }

    public static void FensterRestore(IntPtr handle) => ShowWindow(handle, (int)Sw.Restore);

    public static IntPtr GetAncestor(IntPtr hWnd) {
        var hw = hWnd;
        IntPtr hParent;
        do {
            hParent = GetParent(hw);
            if (hParent.ToInt32() != 0) { hw = hParent; }
        } while (hParent.ToInt32() != 0);
        return hw;
    }

    /// <summary>
    /// Liest Informationen des Zielfensters aus
    /// </summary>
    /// <param name="wDescr"></param>
    /// <remarks></remarks>
    public static void GetWindowInfo(ref StrProcess wDescr) {
        // If WorkStationISLocked() Then
        //    wDescr = Nothing
        //    Exit Sub
        // End If
        // Dim PridA As Integer
        var prid = 0;
        var hParent = GetAncestor(wDescr.MainWindowHandle);
        _ = GetWindowThreadProcessId(hParent, ref prid);
        wDescr.MainWindowTitle = WinTitle(wDescr.MainWindowHandle);
        wDescr.Klasse = WinClass(wDescr.MainWindowHandle);
        wDescr.ExeName = WinExeName(wDescr.MainWindowHandle);
        wDescr.Prid = prid;
    }

    public static void KeyDown(KeyCode k) => keybd_event((byte)k, 0, KeyeventfKeydown, 0);

    public static void KeyUp(KeyCode k) => keybd_event((byte)k, 0, KeyeventfKeyup, 0);

    public static string LastMouseButton() {
        if (Convert.ToBoolean(GetAsyncKeyState(0x1))) { return "Links"; }
        if (Convert.ToBoolean(GetAsyncKeyState(0x2))) { return "Rechts"; }
        if (Convert.ToBoolean(GetAsyncKeyState(0x4))) { return "Mitte"; }
        return string.Empty;
    }

    public static void LeftAltRelease() => keybd_event((byte)KeyCode.VK_MENU, 0, KeyeventfKeyup, 0);

    /// <summary>
    /// Diese Funktion bewegt den Mauscursor an einen bestimmten Punkt.
    /// </summary>
    /// <param name="x">X Koordinate der Position als absoluter Pixelwert</param>
    /// <param name="y">Y Koordinate der Position als absoluter Pixelwert</param>
    /// <returns>Liefert 1 bei Erfolg und 0, wenn der Eingabestream schon blockiert war zurück.</returns>
    public static uint MoveMouse(int x, int y) {
        // Bildschirm Auflösung
        float screenWidth = Screen.PrimaryScreen.Bounds.Width;
        float screenHeight = Screen.PrimaryScreen.Bounds.Height;
        var input_Move = new Input {
            type = InputType.INPUT_MOUSE
        };
        input_Move.mi.dx = (int)Math.Round(x * (65535 / screenWidth), 0);
        input_Move.mi.dy = (int)Math.Round(y * (65535 / screenHeight), 0);
        input_Move.mi.mouseData = 0;
        input_Move.mi.dwFlags = Mouseeventf.MOVE | Mouseeventf.ABSOLUTE;
        input_Move.mi.time = 0;
        input_Move.mi.dwExtraInfo = GetMessageExtraInfo();
        Input[] input = { input_Move };
        return SendInput(1, input, Marshal.SizeOf(input_Move));
    }

    /// <summary>
    /// Diese Funktion Sucht alle offenen Fenster.
    /// </summary>
    /// <returns></returns>
    /// <remarks></remarks>
    public static List<StrProcess> Prozesse() {
        List<StrProcess> wDescr = new();
        var hh = GetTopWindow((IntPtr)0);
        while (true) {
            hh = GetWindow(hh, 2);
            if (hh.ToInt32() != 0) {
                StrProcess l = new() {
                    MainWindowHandle = hh
                };
                GetWindowInfo(ref l);
                wDescr.Add(l);
            } else {
                return wDescr;
            }
        }
    }

    public static void RebootComputer() {
        MultiUserFile.MultiUserFile.SaveAll(true);
        Develop.TraceLogging_End();

        var psi = new ProcessStartInfo("shutdown.exe", "-r -f -t 0") {
            CreateNoWindow = true,
            UseShellExecute = false
        };
        _ = Process.Start(psi);

        Develop.AbortExe();
    }

    public static void ShiftRelease() => keybd_event((byte)KeyCode.VK_SHIFT, 0, KeyeventfKeyup, 0);

    [DllImport("user32", EntryPoint = "ShowWindow", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
    public static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

    public static void ShutdownComputer() {
        MultiUserFile.MultiUserFile.SaveAll(true);
        Develop.TraceLogging_End();

        var psi = new ProcessStartInfo("shutdown", "/s /t 0") {
            CreateNoWindow = true,
            UseShellExecute = false
        };
        _ = Process.Start(psi);

        Develop.AbortExe();
    }

    /// <summary>
    /// Liest den KlassenNames aus.
    /// </summary>
    /// <param name="handle"></param>
    /// <returns></returns>
    /// <remarks></remarks>
    public static string WinClass(IntPtr handle) {
        var buffer = string.Empty.PadRight(250); // = Space(250)
        var l = GetClassName(handle, buffer, 250);
        // If String.IsNullOrEmpty(buffer) Then Return String.Empty
        return buffer.Substring(0, l);
    }

    /// <summary>
    /// Liest den vollständigen Pfad und den Exe-Dateinamen aus.
    /// </summary>
    /// <param name="handle"></param>
    /// <returns></returns>
    /// <remarks></remarks>
    public static string WinExeName(IntPtr handle) {
        var buffer = string.Empty.PadRight(250);
        var l = GetWindowModuleFileName(handle, buffer, 250);
        if (l > 0) {
            buffer = buffer.Substring(0, l);
            return buffer.Substring(buffer.Length - 1) == "\0" ? buffer.Substring(0, buffer.Length - 1) : buffer;
        }
        return string.Empty;
    }

    /// <summary>
    ///  Liest den Titel der Anwendung aus.
    /// </summary>
    /// <param name="handle"></param>
    /// <returns></returns>
    /// <remarks></remarks>
    public static string WinTitle(IntPtr handle) {
        var l = GetWindowTextLength(handle) + 1;
        var buffer = string.Empty.PadRight(l);
        _ = GetWindowText(handle, buffer, l);
        return buffer.Substring(0, buffer.Length);
    }

    [DllImport("user32", EntryPoint = "GetAsyncKeyState", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32", EntryPoint = "GetClassNameA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern int GetClassName(IntPtr hWnd, string lpClassName, int nMaxCount);

    [DllImport("user32.dll", EntryPoint = "GetMessageExtraInfo", SetLastError = true)]
    private static extern IntPtr GetMessageExtraInfo();

    [DllImport("user32", EntryPoint = "GetParent", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern IntPtr GetParent(IntPtr hWnd);

    [DllImport("user32", EntryPoint = "GetTopWindow", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern IntPtr GetTopWindow(IntPtr hWnd);

    [DllImport("user32", EntryPoint = "GetWindow", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern IntPtr GetWindow(IntPtr hwnd, int wCmd);

    [DllImport("user32", EntryPoint = "GetWindowModuleFileNameA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern int GetWindowModuleFileName(IntPtr hWnd, string pszFileName, int cchFileNameMax);

    [DllImport("user32", EntryPoint = "GetWindowRect", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern int GetWindowRect(IntPtr hWnd, ref Rectangle lpRect);

    [DllImport("user32", EntryPoint = "GetWindowTextA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern int GetWindowText(IntPtr hWnd, string lpstring, int cch);

    [DllImport("user32", EntryPoint = "GetWindowTextLengthA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern int GetWindowTextLength(IntPtr hwnd);

    [DllImport("user32", EntryPoint = "GetWindowThreadProcessId", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern int GetWindowThreadProcessId(IntPtr hWnd, ref int lpdwProcessId);

    // VK_CANCEL = &H3 'Used for control-break processing.
    // '****************
    //    VK_CRSEL = &HF7
    //    VK_EREOF = &HF9
    //    VK_EXECUTE = &H2B
    //    VK_EXSEL = &HF8
    //    VK_NONAME = &HFC
    //    VK_OEM_CLEAR = &HFE
    //    VK_PA1 = &HFD
    //    VK_PROCESSKEY = &HE5
    //    CAPSLOCK_ON = &H80    '  the capslock light is on.
    [DllImport("user32.dll", EntryPoint = "keybd_event", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern int keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

    [DllImport("user32.dll", EntryPoint = "SendInput", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

    /// <summary>
    /// Setzt ein Fenster an eine andere Position
    /// </summary>
    /// <param name="hWnd">Handle des Fensters, das verschoben werden soll.</param>
    /// <param name="hWndInsertAfter">Eine der HWND Konstanten um die neue Position des Fensters festzulegen</param>
    /// <param name="x">Wenn eine neue X-Position festgelegt werden soll, in der Regel wird hier jedoch eine NULL übergeben</param>
    /// <param name="y">Wenn eine neue Y-Position festgelegt werden soll, in der Regel wird hier jedoch eine NULL übergeben</param>
    /// <param name="cx">Die neue Breite des Fensters, inder Regel wird hier jedoch NULL übergeben</param>
    /// <param name="cy">Die neue Höhe des Fensters, in der Regel wird hier jedoch NULL übergeben</param>
    /// <param name="wFlags">einen oder mehrere SWP Konstanten</param>
    /// <returns></returns>
    /// <remarks></remarks>
    [DllImport("user32", EntryPoint = "SetWindowPos", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

    #endregion

    #region Structs

    public struct StrProcess {

        #region Fields

        public string ExeName;
        public string Klasse;
        public IntPtr MainWindowHandle;
        public string MainWindowTitle;
        public int Prid;

        #endregion
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Input {
        public InputType type;
        public Mouseinput mi;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Mouseinput {
        public int dx;
        public int dy;
        public int mouseData;
        public Mouseeventf dwFlags;
        public int time;
        public IntPtr dwExtraInfo;
    }

    #endregion
}