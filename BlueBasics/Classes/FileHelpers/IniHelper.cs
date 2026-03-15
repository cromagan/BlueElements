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

using System.Text;

namespace BlueBasics.Classes.FileHelpers;

public class IniHelper : TextFileHelper {

    #region Methods

    public override string FinishParseable() {
        var sb = new StringBuilder();
        foreach (var kv in _items) {
            sb.AppendLine(kv.Key + "=" + kv.Value.ToNonCritical());
        }
        return sb.ToString().TrimEnd('\r', '\n');
    }

    public override bool ParseContent(string content) {
        Clear();
        _items.Clear();
        if (string.IsNullOrEmpty(content)) { return true; }

        content = content.Replace("\r\n", "\r").Replace("\n", "\r");
        var lines = content.Split('\r');

        foreach (var rawLine in lines) {
            var line = rawLine.Trim();
            if (string.IsNullOrEmpty(line)) { continue; }
            if (line.StartsWith(";", System.StringComparison.Ordinal) ||
                line.StartsWith("#", System.StringComparison.Ordinal)) { continue; }

            var eqIndex = line.IndexOf('=');
            if (eqIndex < 0) { continue; }

            var key = line.Substring(0, eqIndex).Trim();
            var value = line.Substring(eqIndex + 1).Trim().FromNonCritical();

            ParseableAdd(key, value);
        }

        return true;
    }

    #endregion
}