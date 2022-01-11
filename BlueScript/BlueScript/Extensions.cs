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

using BlueBasics.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BlueScript {

    public static class Extensions {

        #region Fields

        public static readonly string ImageKennung = ((char)9001).ToString();
        public static readonly string ObjectKennung = ((char)9002).ToString();
        internal static readonly string BackSlashEscaped = ((char)9003).ToString();
        internal static readonly string GänsefüßchenReplace = ((char)9000).ToString();

        #endregion

        #region Methods

        public static string RemoveCriticalVariableChars(this string txt) {
            txt = txt.Replace("\\", BackSlashEscaped);
            txt = txt.Replace("\"", GänsefüßchenReplace);
            return txt;
        }

        public static string RemoveEscape(this string txt) {
            txt = txt.Replace("\\\\", BackSlashEscaped);
            txt = txt.Replace("\\\"", GänsefüßchenReplace);
            return txt;
        }

        public static string RestoreCriticalVariableChars(this string txt) {
            txt = txt.Replace(BackSlashEscaped, "\\");
            txt = txt.Replace(GänsefüßchenReplace, "\"");
            return txt;
        }

        public static string RestoreEscape(this string txt) {
            txt = txt.Replace(BackSlashEscaped, "\\\\");
            txt = txt.Replace(GänsefüßchenReplace, "\\\"");
            return txt;
        }

        #endregion
    }
}