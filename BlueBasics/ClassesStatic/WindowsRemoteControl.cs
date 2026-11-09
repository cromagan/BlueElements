// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueBasics.Structs;
using System.Diagnostics;
using System.Runtime.InteropServices;
using FormWindowState = System.Windows.Forms.FormWindowState;

namespace BlueBasics.ClassesStatic;

public static class WindowsRemoteControl {

    #region Fields

    private const int DwmwaCloaked = 14;

    private const int GwlExstyle = -20;

    private const int KeyeventfExtendedkey = 0x1;

    private const int KeyeventfKeydown = 0x0;

    private const int KeyeventfKeyup = 0x2;

    private const int SwpNoactivate = 0x0010;

    private const int WsexToolwindow = 0x00000080;

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
    public static void AltRelease() => _ = keybd_event((byte)KeyCode.VK_MENU, 0, KeyeventfExtendedkey | KeyeventfKeyup, 0);

    public static void FensterRestore(IntPtr handle) => _ = ShowWindow(handle, (int)Sw.Restore);

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
        var prid = 0;
        var hParent = GetAncestor(wDescr.MainWindowHandle);
        _ = GetWindowThreadProcessId(hParent, ref prid);
        Develop.Diagnose("WRC", "Handle " + wDescr.MainWindowHandle + ": Titel wird gelesen");
        wDescr.MainWindowTitle = WinTitle(wDescr.MainWindowHandle);
        Develop.Diagnose("WRC", "Handle " + wDescr.MainWindowHandle + ": Klasse wird gelesen");
        wDescr.Klasse = WinClass(wDescr.MainWindowHandle);
        Develop.Diagnose("WRC", "Handle " + wDescr.MainWindowHandle + ": Exe wird gelesen");
        wDescr.ExeName = WinExeName(wDescr.MainWindowHandle);
        wDescr.Prid = prid;
    }

    public static void KeyDown(KeyCode k) => _ = keybd_event((byte)k, 0, KeyeventfKeydown, 0);

    public static void KeyUp(KeyCode k) => _ = keybd_event((byte)k, 0, KeyeventfKeyup, 0);

    public static string LastMouseButton() {
        if (Convert.ToBoolean(GetAsyncKeyState(0x1))) { return "Links"; }
        if (Convert.ToBoolean(GetAsyncKeyState(0x2))) { return "Rechts"; }
        if (Convert.ToBoolean(GetAsyncKeyState(0x4))) { return "Mitte"; }
        return string.Empty;
    }

    public static void LeftAltRelease() => _ = keybd_event((byte)KeyCode.VK_MENU, 0, KeyeventfKeyup, 0);

    public static void MouseAction(MouseEventFlags flags, int x, int y) => mouse_event((uint)flags, x, y, 0, 0);

    /// <summary>
    /// Diese Funktion bewegt den Mauscursor an einen bestimmten Punkt.
    /// </summary>
    /// <param name="x">X Koordinate der Position als absoluter Pixelwert</param>
    /// <param name="y">Y Koordinate der Position als absoluter Pixelwert</param>
    /// <returns>Liefert 1 bei Erfolg und 0, wenn der Eingabestream schon blockiert war zurück.</returns>
    public static uint MoveMouse(int x, int y) {
        // Bildschirm Auflösung
        var screen = System.Windows.Forms.Screen.PrimaryScreen;
        if (screen is null) { return 0; }
        float screenWidth = screen.Bounds.Width;
        float screenHeight = screen.Bounds.Height;
        var input_Move = new Input {
            type = InputType.INPUT_MOUSE
        };
        input_Move.mi.dx = (int)Math.Round(x * (65535 / screenWidth), 0, MidpointRounding.AwayFromZero);
        input_Move.mi.dy = (int)Math.Round(y * (65535 / screenHeight), 0, MidpointRounding.AwayFromZero);
        input_Move.mi.mouseData = 0;
        input_Move.mi.dwFlags = Mouseeventf.MOVE | Mouseeventf.ABSOLUTE;
        input_Move.mi.time = 0;
        input_Move.mi.dwExtraInfo = GetMessageExtraInfo();
        Input[] input = [input_Move];
        return SendInput(1, input, Marshal.SizeOf(input_Move));
    }

    /// <summary>
    /// Diese Funktion Sucht alle offenen Fenster.
    /// </summary>
    /// <returns></returns>
    /// <remarks></remarks>
    public static List<StrProcess> Prozesse() {
        List<StrProcess> wDescr = [];
        var hh = GetTopWindow(0);
        while (true) {
            hh = GetWindow(hh, 2);
            if (hh.ToInt32() != 0) {
                var l = new StrProcess() {
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
        Develop.TraceLogging_End();

        var psi = new ProcessStartInfo("shutdown.exe", "-r -f -t 0") {
            CreateNoWindow = true,
            UseShellExecute = false
        };
        Process.Start(psi);

        Develop.AbortExe(false);
    }

    public static void ShiftRelease() => _ = keybd_event((byte)KeyCode.VK_SHIFT, 0, KeyeventfKeyup, 0);

    public static void ShowWindowNoActivate(IntPtr handle) => _ = ShowWindow(handle, (int)Sw.ShowNoActivate);

    public static void ShutdownComputer() {
        Develop.TraceLogging_End();

        var psi = new ProcessStartInfo("shutdown", "/s /t 0") {
            CreateNoWindow = true,
            UseShellExecute = false
        };
        Process.Start(psi);

        Develop.AbortExe(false);
    }

    /// <summary>
    /// Liefert die Bildschirmkoordinaten (Position und Größe) des Fensters.
    /// </summary>
    /// <param name="handle"></param>
    /// <returns></returns>
    public static Rectangle WinBounds(IntPtr handle) {
        if (GetWindowRect(handle, out var rect) == 0) { return Rectangle.Empty; }

        return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
    }

    /// <summary>
    /// Liest den KlassenNames aus.
    /// </summary>
    /// <param name="handle"></param>
    /// <returns></returns>
    /// <remarks></remarks>
    public static string WinClass(IntPtr handle) {
        var buffer = Marshal.AllocHGlobal(512);
        try {
            var l = GetClassName(handle, buffer, 511);
            return l <= 0 ? string.Empty : Marshal.PtrToStringAnsi(buffer, l) ?? string.Empty;
        } finally {
            Marshal.FreeHGlobal(buffer);
        }
    }

    /// <summary>
    /// Liest den vollständigen Pfad und den Exe-Dateinamen aus.
    /// </summary>
    /// <param name="handle"></param>
    /// <returns></returns>
    /// <remarks></remarks>
    public static string WinExeName(IntPtr handle) {
        var buffer = Marshal.AllocHGlobal(1024);
        try {
            var l = GetWindowModuleFileName(handle, buffer, 1023);
            return l <= 0 ? string.Empty : (Marshal.PtrToStringAnsi(buffer, l) ?? string.Empty).TrimEnd('\0');
        } finally {
            Marshal.FreeHGlobal(buffer);
        }
    }

    /// <summary>
    /// True, wenn das Fenster als Werkzeugfenster (WS_EX_TOOLWINDOW) markiert ist und typischerweise nicht gesichert werden soll.
    /// </summary>
    /// <param name="handle"></param>
    /// <returns></returns>
    public static bool WinIsToolWindow(IntPtr handle) => (GetWindowLong(handle, GwlExstyle) & WsexToolwindow) != 0;

    /// <summary>
    /// True, wenn das Fenster sichtbar und nicht von einer anderen Anwendung "verhüllt" (Cloaked) ist.
    /// </summary>
    /// <param name="handle"></param>
    /// <returns></returns>
    public static bool WinIsVisible(IntPtr handle) => IsWindowVisible(handle) && !WinIsCloaked(handle);

    /// <summary>
    /// Bringt das Fenster auf die gewünschte Position/Größe, stellt den Fensterzustand wieder her und holt es in den Vordergrund.
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="bounds"></param>
    /// <param name="maximized"></param>
    public static void WinPlace(IntPtr handle, Rectangle bounds, bool maximized) {
        Develop.Diagnose("WRC", "WinPlace " + handle + (maximized ? " maximiert" : " normal"));

        if (maximized) {
            _ = ShowWindow(handle, (int)Sw.ShowMaximized);
            Develop.Diagnose("WRC", "WinPlace ShowWindow fertig: " + handle);
            return;
        }

        FensterRestore(handle);
        Develop.Diagnose("WRC", "WinPlace Restore fertig: " + handle);
        _ = SetWindowPos(handle, 0, bounds.X, bounds.Y, bounds.Width, bounds.Height, SwpNoactivate); // 0 = HwndTop
        Develop.Diagnose("WRC", "WinPlace SetWindowPos fertig: " + handle);
    }

    /// <summary>
    /// Minimiert das Fenster.
    /// </summary>
    public static void WinMinimize(IntPtr handle) => _ = ShowWindow(handle, (int)Sw.Minimize);

    /// <summary>
    /// Liefert den Fensterzustand des Fensters.
    /// </summary>
    /// <param name="handle"></param>
    /// <returns></returns>
    public static FormWindowState WinState(IntPtr handle) {
        if (IsIconic(handle)) { return FormWindowState.Minimized; }

        return IsZoomed(handle) ? FormWindowState.Maximized : FormWindowState.Normal;
    }

    /// <summary>
    /// Liefert Position und Größe des Fensters im Normalzustand - auch für minimierte Fenster.
    /// </summary>
    public static Rectangle WinNormalBounds(IntPtr handle) {
        var placement = new WindowPlacement {
            Length = Marshal.SizeOf<WindowPlacement>()
        };

        if (!GetWindowPlacement(handle, ref placement)) { return Rectangle.Empty; }

        var r = placement.NormalPosition;
        return new Rectangle(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top);
    }

    /// <summary>
    ///  Liest den Titel der Anwendung aus.
    /// </summary>
    /// <param name="handle"></param>
    /// <returns></returns>
    /// <remarks></remarks>
    public static string WinTitle(IntPtr handle) {
        var l = GetWindowTextLength(handle) + 1;
        if (l <= 1) { return string.Empty; }

        var buffer = Marshal.AllocHGlobal(l * 2);
        try {
            _ = GetWindowText(handle, buffer, l);
            return (Marshal.PtrToStringUni(buffer) ?? string.Empty).TrimEnd('\0');
        } finally {
            Marshal.FreeHGlobal(buffer);
        }
    }

    [DllImport("user32", EntryPoint = "GetAsyncKeyState", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32", EntryPoint = "GetClassNameA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern int GetClassName(IntPtr hWnd, IntPtr lpClassName, int nMaxCount);

    [DllImport("dwmapi.dll")]
    private static extern int DwmGetWindowAttribute(IntPtr hwnd, int attr, out int attrValue, int attrSize);

    [DllImport("user32.dll", EntryPoint = "GetMessageExtraInfo", SetLastError = true)]
    private static extern IntPtr GetMessageExtraInfo();

    [DllImport("user32", EntryPoint = "GetParent", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern IntPtr GetParent(IntPtr hWnd);

    [DllImport("user32", EntryPoint = "GetTopWindow", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern IntPtr GetTopWindow(IntPtr hWnd);

    [DllImport("user32", EntryPoint = "GetWindow", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern IntPtr GetWindow(IntPtr hwnd, int wCmd);

    [DllImport("user32", EntryPoint = "GetWindowLong", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32", EntryPoint = "GetWindowModuleFileNameA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern int GetWindowModuleFileName(IntPtr hWnd, IntPtr pszFileName, int cchFileNameMax);

    [DllImport("user32", EntryPoint = "GetWindowPlacement", SetLastError = true)]
    private static extern bool GetWindowPlacement(IntPtr hWnd, ref WindowPlacement lpplwnd);

    [DllImport("user32", EntryPoint = "GetWindowRect", SetLastError = true)]
    private static extern int GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32", EntryPoint = "GetWindowTextW", ExactSpelling = true, SetLastError = true)]
    private static extern int GetWindowText(IntPtr hWnd, IntPtr lpstring, int cch);

    [DllImport("user32", EntryPoint = "GetWindowTextLengthA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern int GetWindowTextLength(IntPtr hwnd);

    [DllImport("user32", EntryPoint = "GetWindowThreadProcessId", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern int GetWindowThreadProcessId(IntPtr hWnd, ref int lpdwProcessId);

    [DllImport("user32", EntryPoint = "IsIconic", ExactSpelling = true, SetLastError = true)]
    private static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32", EntryPoint = "IsWindowVisible", ExactSpelling = true, SetLastError = true)]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32", EntryPoint = "IsZoomed", ExactSpelling = true, SetLastError = true)]
    private static extern bool IsZoomed(IntPtr hWnd);

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

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

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

    [DllImport("user32", EntryPoint = "ShowWindow", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

    /// <summary>
    /// True, wenn das Fenster von einer anderen Anwendung "verhüllt" (Cloaked, z.B. UWP im Hintergrund) ist.
    /// </summary>
    /// <param name="handle"></param>
    /// <returns></returns>
    private static bool WinIsCloaked(IntPtr handle) {
        var result = DwmGetWindowAttribute(handle, DwmwaCloaked, out var cloak, sizeof(int));
        return result == 0 && cloak != 0;
    }

    #endregion

    #region Structs

    public struct StrProcess {

        #region Properties

        public string ExeName { get; set; }
        public string Klasse { get; set; }
        public IntPtr MainWindowHandle { get; set; }
        public string MainWindowTitle { get; set; }
        public int Prid { get; set; }

        #endregion
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowPlacement {
        public int Length;
        public int Flags;
        public int ShowCmd;
        public Point MinPosition;
        public Point MaxPosition;
        public RECT NormalPosition;
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