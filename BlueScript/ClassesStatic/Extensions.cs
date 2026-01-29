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

namespace BlueScript.ClassesStatic;

public static class Extensions {
    //public static readonly string ImageKennung = ((char)9001).ToString(false);
    //public static readonly string ObjectKennung = ((char)9002).ToString(false);

    #region Fields

    private static readonly string ReplacerForBackSlash = ((char)9003).ToString();
    private static readonly string ReplacerForN = ((char)9005).ToString();
    private static readonly string ReplacerForQuotes = ((char)9000).ToString();
    private static readonly string ReplacerForR = ((char)9004).ToString();

    #endregion

    #region Methods

    public static string RemoveCriticalVariableChars(this string txt) {
        txt = txt.Replace("\\", ReplacerForBackSlash);
        txt = txt.Replace("\"", ReplacerForQuotes);
        txt = txt.Replace("\r", ReplacerForR);
        txt = txt.Replace("\n", ReplacerForN);
        return txt;
    }

    public static string RemoveEscape(this string txt) {
        txt = txt.Replace("\\\\", ReplacerForBackSlash);
        txt = txt.Replace("\\\"", ReplacerForQuotes);
        txt = txt.Replace("\\r", ReplacerForR);
        txt = txt.Replace("\\n", ReplacerForN);
        return txt;
    }

    public static string RestoreCriticalVariableChars(this string txt) {
        txt = txt.Replace(ReplacerForBackSlash, "\\");
        txt = txt.Replace(ReplacerForQuotes, "\"");
        txt = txt.Replace(ReplacerForR, "\r");
        txt = txt.Replace(ReplacerForN, "\n");
        return txt;
    }

    #endregion
}