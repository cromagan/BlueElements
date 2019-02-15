using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace BlueBasics
{

    // TODO: Structuren und Enums auslagern

    public static class modFernsteuerung
    {


        public enum MOUSEEVENT
        {
            LEFTDOWN = 0x2,
            LEFTUP = 0x4,
            MOVE = 0x1,
            RIGHTDOWN = 0x8,
            RIGHTUP = 0x10,
            MIDDLEDOWN = 0x20,
            MIDDLEUP = 0x40,
            ABSOLUTE = 0x8000
        }



        [DllImport("user32", EntryPoint = "GetClassNameA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern int GetClassName(IntPtr hWnd, string lpClassName, int nMaxCount);
        [DllImport("user32", EntryPoint = "GetWindowTextA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, string lpstring, int cch);
        [DllImport("user32", EntryPoint = "GetWindowTextLengthA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern int GetWindowTextLength(IntPtr hwnd);
        [DllImport("user32", EntryPoint = "GetWindowThreadProcessId", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, ref int lpdwProcessId);
        [DllImport("user32", EntryPoint = "GetParent", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr GetParent(IntPtr hWnd);
        [DllImport("user32", EntryPoint = "GetTopWindow", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr GetTopWindow(IntPtr hWnd);
        [DllImport("user32", EntryPoint = "GetWindowModuleFileNameA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern int GetWindowModuleFileName(IntPtr hWnd, string pszFileName, int cchFileNameMax);


        [DllImport("user32", EntryPoint = "ShowWindow", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

        public enum enSW
        {
            Hide = 0,
            Normal = 1,
            ShowMinimized = 2,
            ShowMaximized = 3,
            ShowNoActivate = 4,
            Show = 5,
            Minimize = 6,
            ShowMinNoActive = 7,
            ShowNA = 8,
            Restore = 9,
            ShowDefault = 10,
            ForceMinimize = 11,
            Max = 11

        }



        [DllImport("user32", EntryPoint = "GetWindow", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr GetWindow(IntPtr hwnd, int wCmd);







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




        [DllImport("user32", EntryPoint = "GetWindowRect", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern int GetWindowRect(IntPtr hWnd, ref Rectangle lpRect);
        //Declare Function ShowWindow Lib "user32" (ByVal hWnd As IntPtr, ByVal nCmdShow As Integer) As Integer


        public struct exProcess
        {
            public IntPtr MainWindowHandle;
            public int prid;
            public string MainWindowTitle;
            public string Klasse;
            public string ExeName;
        }


        /// <summary>
        ///  Liest den Titel der Anwendung aus.
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        /// <remarks></remarks> 
        public static string WinTitle(IntPtr handle)
        {
            var l = GetWindowTextLength(handle) + 1;
            var buffer = "".PadRight(l);
            l = GetWindowText(handle, buffer, l);

            return buffer.Substring(0, buffer.Length);
        }

        /// <summary>
        /// Liest den KlassenNames aus.
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string WinClass(IntPtr handle)
        {
            var buffer = "".PadRight(250); //= Space(250)
            var l = GetClassName(handle, buffer, 250);

            //   If String.IsNullOrEmpty(buffer) Then Return String.Empty


            return buffer.Substring(0, l);
        }

        /// <summary>
        /// Liest den vollständigen Pfad und den Exe-Dateinamen aus.
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string WinExeName(IntPtr handle)
        {
            var l = 0;
            var buffer = "".PadRight(250);
            l = GetWindowModuleFileName(handle, buffer, 250);

            if (l > 0)
            {
                buffer = buffer.Substring(0, l);
                if (buffer.Substring(buffer.Length - 1) == "\0")
                {
                    return buffer.Substring(0, buffer.Length - 1);
                }

                return buffer;
            }

            return string.Empty;
        }

        /// <summary>
        /// Liest Informationen des Zielfensters aus
        /// </summary>
        /// <param name="wDescr"></param>
        /// <remarks></remarks>
        public static void GetWindowInfo(ref exProcess wDescr)
        {
            //If WorkStationISLocked() Then
            //    wDescr = Nothing
            //    Exit Sub
            //End If


            var prid = 0;
            var hParent = IntPtr.Zero;
            //  Dim PridA As Integer

            prid = 0;
            hParent = GetAncestor(wDescr.MainWindowHandle);

            GetWindowThreadProcessId(hParent, ref prid);

            wDescr.MainWindowTitle = WinTitle(wDescr.MainWindowHandle);
            wDescr.Klasse = WinClass(wDescr.MainWindowHandle);
            wDescr.ExeName = WinExeName(wDescr.MainWindowHandle);
            wDescr.prid = prid;


        }

        /// <summary>
        /// Diese Funktion Sucht alle offenen Fenster.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public static List<exProcess> exProzesse()
        {
            var wDescr = new List<exProcess>();
            var hh = GetTopWindow((IntPtr)0);


            do
            {
                hh = GetWindow(hh, 2);
                if (hh.ToInt32() != 0)
                {
                    var l = new exProcess();
                    l.MainWindowHandle = hh;
                    GetWindowInfo(ref l);
                    wDescr.Add(l);
                }
                else
                {
                    return wDescr;
                }
            } while (true);

        }


        public static IntPtr GetAncestor(IntPtr hWnd)
        {

            var hParent = IntPtr.Zero;
            var hw = hWnd;
            do
            {
                hParent = GetParent(hw);
                if (hParent.ToInt32() != 0) { hw = hParent; }
            } while (hParent.ToInt32() != 0);
            return hw;

        }



        public static void FensterPosSetzen(IntPtr Handle, int Left, int Top)
        {
            var r = new Rectangle();
            GetWindowRect(Handle, ref r);

            if (r.Width == 0) { return; }
            if (r.Height == 0) { return; }

            SetWindowPos(Handle, 0, Left, Top, r.Width - r.Left, r.Height - r.Top, 0);
        }



        public static void FensterMinimieren(IntPtr Handle)
        {
            ShowWindow(Handle, (int)enSW.ShowMinimized);
        }

        public static void FensterMaximieren(IntPtr Handle)
        {
            ShowWindow(Handle, (int)enSW.ShowMaximized);
        }

        public static void FensterRestore(IntPtr Handle)
        {
            ShowWindow(Handle, (int)enSW.Restore);
        }

    }
}