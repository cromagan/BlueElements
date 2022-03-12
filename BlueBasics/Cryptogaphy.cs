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

using System.Collections.Generic;

namespace BlueBasics {

    public static class Cryptography {

        #region Methods

        public static byte[] SimpleCrypt(byte[] b, string pass, int direction, int start, int end) {
            if (string.IsNullOrEmpty(pass)) { return b; }
            if (end <= start) { return b; }
            for (var z = start; z <= end; z++) {
                var tmp = b[z] + (pass[z % pass.Length] * direction);
                if (tmp < 0) { tmp += 256; }
                if (tmp > 255) { tmp -= 256; }
                b[z] = (byte)tmp;
            }
            return b;
        }

        public static string SimpleCrypt(string content, string pass, int direction) => SimpleCrypt(content.WIN1252_toByte(), pass, direction).ToStringWin1252();

        public static byte[] SimpleCrypt(byte[] b, string pass, int direction) => SimpleCrypt(b, pass, direction, 0, b.GetUpperBound(0));

        public static List<byte> SimpleCrypt(List<byte> b, string pass, int direction, int start, int end) {
            if (string.IsNullOrEmpty(pass)) { return b; }
            if (end <= start) { return b; }
            for (var z = start; z <= end; z++) {
                var TMP = b[z] + (pass[z % pass.Length] * direction);
                if (TMP < 0) { TMP += 256; }
                if (TMP > 255) { TMP -= 256; }
                b[z] = (byte)TMP;
            }
            return b;
        }

        public static List<byte> SimpleCrypt(List<byte> b, string pass, int direction) => SimpleCrypt(b, pass, direction, 0, b.Count - 1);

        #endregion
    }
}