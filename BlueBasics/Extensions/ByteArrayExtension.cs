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

using System;

using System.IO.Compression;
using System.Text;

namespace BlueBasics;

public static partial class Extensions {

    #region Methods

    public static bool IsZipped(this byte[] data) => data.Length > 4 && BitConverter.ToInt32(data, 0) == 67324752;

    public static string ToStringUtf8(this byte[] b) => Encoding.UTF8.GetString(b);

    //public static string ToStringUtf8(this List<byte> b) => Encoding.UTF8.GetString(b.ToArray());

    // static Encoding enc1252 = CodePagesEncodingProvider.Instance.GetEncoding(1252);
    //public static string ToStringWin1252(this byte[] b) =>
    //    // https://stackoverflow.com/questions/37870084/net-core-doesnt-know-about-windows-1252-how-to-fix
    //    // Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    //    // var enc1252 = Encoding.GetEncoding(1252);
    //    Encoding.GetEncoding(1252).GetString(b);

    public static byte[]? UnzipIt(this byte[] data) {
        try {
            using var originalFileStream = new System.IO.MemoryStream(data);
            using ZipArchive zipArchive = new(originalFileStream);
            var entry = zipArchive.GetEntry("Main.bin");
            if (entry != null) {
                using var stream = entry.Open();
                using var ms = new System.IO.MemoryStream();
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        } catch { }

        return null;
    }

    /// <summary>
    /// Gibt die Bytes aus Main.bin zurück
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static byte[]? ZipIt(this byte[] data) {
        try {
            // https://stackoverflow.com/questions/17217077/create-zip-file-from-byte
            using var compressedFileStream = new System.IO.MemoryStream();
            // Create an archive and store the stream in memory.
            using (ZipArchive zipArchive = new(compressedFileStream, ZipArchiveMode.Create, false)) {
                // Create a zip entry for each attachment
                var zipEntry = zipArchive.CreateEntry("Main.bin");
                // Get the stream of the attachment
                using var originalFileStream = new System.IO.MemoryStream(data);
                using var zipEntryStream = zipEntry.Open();
                // Copy the attachment stream to the zip entry stream
                originalFileStream.CopyTo(zipEntryStream);
            }
            return compressedFileStream.ToArray();
        } catch { }
        return null;
    }

    #endregion
}