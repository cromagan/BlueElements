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

#nullable enable

using BlueBasics.Enums;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace BlueBasics;

public class WindowsThumbnailProvider {

    #region Fields

    private const string IShellItem2Guid = "7E9FB0D3-919F-4307-AB2E-9B1860310C93";

    #endregion

    #region Enums

    internal enum HResult {
        Ok = 0x0000,
        False = 0x0001,
        InvalidArguments = unchecked((int)0x80070057),
        OutOfMemory = unchecked((int)0x8007000E),
        NoInterface = unchecked((int)0x80004002),
        Fail = unchecked((int)0x80004005),
        ElementNotFound = unchecked((int)0x80070490),
        TypeElementNotFound = unchecked((int)0x8002802B),
        NoObject = unchecked((int)0x800401E5),
        Win32ErrorCanceled = 1223,
        Canceled = unchecked((int)0x800704C7),
        ResourceInUse = unchecked((int)0x800700AA),
        AccessDenied = unchecked((int)0x80030005)
    }

    internal enum SIGDN : uint {
        NORMALDISPLAY = 0,
        PARENTRELATIVEPARSING = 0x80018001,
        PARENTRELATIVEFORADDRESSBAR = 0x8001c001,
        DESKTOPABSOLUTEPARSING = 0x80028000,
        PARENTRELATIVEEDITING = 0x80031001,
        DESKTOPABSOLUTEEDITING = 0x8004c000,
        FILESYSPATH = 0x80058000,
        URL = 0x80068000
    }

    #endregion

    #region Interfaces

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe")]
    internal interface IShellItem {

        void BindToHandler(IntPtr pbc,
            [MarshalAs(UnmanagedType.LPStruct)] Guid bhid,
            [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            out IntPtr ppv);

        void GetParent(out IShellItem ppsi);

        void GetDisplayName(SIGDN sigdnName, out IntPtr ppszName);

        void GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);

        void Compare(IShellItem psi, uint hint, out int piOrder);
    }

    [ComImport]
    [Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IShellItemImageFactory {

        [PreserveSig]
        HResult GetImage(
            [In][MarshalAs(UnmanagedType.Struct)] NativeSize size,
            [In] ThumbnailOptions flags,
            [Out] out IntPtr phbm);
    }

    #endregion

    #region Methods

    public static Bitmap CreateAlphaBitmap(Bitmap srcBitmap, PixelFormat targetPixelFormat) {
        var result = new Bitmap(srcBitmap.Width, srcBitmap.Height, targetPixelFormat);

        var bmpBounds = new Rectangle(0, 0, srcBitmap.Width, srcBitmap.Height);

        var srcData = srcBitmap.LockBits(bmpBounds, ImageLockMode.ReadOnly, srcBitmap.PixelFormat);

        var isAlplaBitmap = false;

        try {
            for (var y = 0; y <= srcData.Height - 1; y++) {
                for (var x = 0; x <= srcData.Width - 1; x++) {
                    var pixelColor = Color.FromArgb(
                        Marshal.ReadInt32(srcData.Scan0, srcData.Stride * y + 4 * x));

                    if ((pixelColor.A > 0) & (pixelColor.A < 255)) {
                        isAlplaBitmap = true;
                    }

                    result.SetPixel(x, y, pixelColor);
                }
            }
        } finally {
            srcBitmap.UnlockBits(srcData);
        }

        if (isAlplaBitmap) {
            return result;
        }

        return srcBitmap;
    }

    public static Bitmap GetBitmapFromHBitmap(IntPtr nativeHBitmap) {
        var bmp = Image.FromHbitmap(nativeHBitmap);

        if (Image.GetPixelFormatSize(bmp.PixelFormat) < 32) { return bmp; }

        return CreateAlphaBitmap(bmp, PixelFormat.Format32bppArgb);
    }

    public static Bitmap? GetThumbnail(string fileName, int width, int height, ThumbnailOptions options) {
        var hBitmap = GetHBitmap(Path.GetFullPath(fileName), width, height, options);

        if (hBitmap == null) { return null; }

        try {
            return GetBitmapFromHBitmap((IntPtr)hBitmap);
        } finally {
            DeleteObject((IntPtr)hBitmap);
        }
    }

    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool DeleteObject(IntPtr hObject);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern int SHCreateItemFromParsingName(
        [MarshalAs(UnmanagedType.LPWStr)] string path,
        // The following parameter is not used - binding context.
        IntPtr pbc,
        ref Guid riid,
        [MarshalAs(UnmanagedType.Interface)] out IShellItem shellItem);

    //[StructLayout(LayoutKind.Sequential)]
    //public struct RGBQUAD
    //{
    //    public byte rgbBlue;
    //    public byte rgbGreen;
    //    public byte rgbRed;
    //    public byte rgbReserved;
    //}
    private static IntPtr? GetHBitmap(string fileName, int width, int height, ThumbnailOptions options) {
        var shellItem2Guid = new Guid(IShellItem2Guid);
        var retCode = SHCreateItemFromParsingName(fileName, IntPtr.Zero, ref shellItem2Guid, out var nativeShellItem);

        if (retCode != 0) { throw Marshal.GetExceptionForHR(retCode); }

        var nativeSize = new NativeSize();
        nativeSize.Width = width;
        nativeSize.Height = height;

        var hr = ((IShellItemImageFactory)nativeShellItem).GetImage(nativeSize, options, out var hBitmap);

        Marshal.ReleaseComObject(nativeShellItem);

        if (hr == HResult.Ok) { return hBitmap; }

        return null;
        //throw Marshal.GetExceptionForHR((int)hr);
    }

    #endregion

    #region Structs

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeSize {
        private int width;
        private int height;

        public int Width { set => width = value; }
        public int Height { set => height = value; }
    }

    #endregion
}