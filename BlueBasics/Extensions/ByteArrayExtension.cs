#region BlueElements - a collection of useful tools, database and controls

// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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

#endregion BlueElements - a collection of useful tools, database and controls

using System.Collections.Generic;
using System.Text;

namespace BlueBasics {

    public static partial class Extensions {

        // static Encoding enc1252 = CodePagesEncodingProvider.Instance.GetEncoding(1252);
        public static string ToStringWIN1252(this byte[] b) =>
            // https://stackoverflow.com/questions/37870084/net-core-doesnt-know-about-windows-1252-how-to-fix
            // Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            // var enc1252 = Encoding.GetEncoding(1252);
            Encoding.GetEncoding(1252).GetString(b);

        public static string ToStringUTF8(this byte[] b) => Encoding.UTF8.GetString(b);

        public static string ToStringUTF8(this List<byte> b) => Encoding.UTF8.GetString(b.ToArray());
    }
}