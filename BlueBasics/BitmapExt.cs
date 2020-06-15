#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
#endregion


using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static BlueBasics.FileOperations;

namespace BlueBasics
{

    //https://stackoverflow.com/questions/24701703/c-sharp-faster-alternatives-to-setpixel-and-getpixel-for-bitmaps-for-windows-f

  public  class BitmapExt
    {
        public Bitmap Bitmap { get; private set; }
        public int[] Bits { get; private set; } // Int32 = int
        public bool Disposed { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }

        protected GCHandle BitsHandle { get; private set; }


        public BitmapExt(string filename)
        {
            var orig = Image_FromFile(filename);

            Width = orig.Width;
            Height = orig.Height;
            Bits = new int[Width * Height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());

            using (var gr = Graphics.FromImage(Bitmap))
            {
                gr.DrawImage(orig, new Rectangle(0, 0, Width, Height));
            }

            orig.Dispose();
        }

        public BitmapExt(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new int[Width * Height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
        }

        public void SetPixel(int x, int y, Color colour)
        {
            Bits[x + (y * Width)] = colour.ToArgb();
        }

        public Color GetPixel(int x, int y)
        {
            return Color.FromArgb(Bits[x + (y * Width)]);
        }

        public void Dispose()
        {
            if (Disposed) { return; }
            Disposed = true;
            Bitmap.Dispose();
            BitsHandle.Free();
        }




        /// <summary>
        /// Diese Routine ist genau so schnell wie Image.fromFile, setzt aber KEINEN Datei-Lock.
        /// </summary>
        /// <param name="DateiName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static Image Image_FromFile(string DateiName)
        {
            if (string.IsNullOrEmpty(DateiName)) { return null; }
            if (!FileExists(DateiName)) { return null; }

            try
            {
                var fs = new FileStream(DateiName, FileMode.Open, FileAccess.Read, FileShare.Read);
                var IM = Image.FromStream(fs);
                fs.Close();
                fs.Dispose();
                return IM;
            }
            catch (Exception)
            {
                return null;
            }


        }


    }
}