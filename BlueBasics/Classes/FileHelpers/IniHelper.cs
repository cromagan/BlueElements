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
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace BlueBasics.Classes.FileHelpers;

public class IniHelper : TextFileHelper {

    #region Methods

    public override string FinishParseable() {
        if (_doc.Root == null)
            return string.Empty;
        var sb = new StringBuilder();

        foreach (var node in _doc.Root.Nodes()) {
            if (node is XComment comment) {
                sb.AppendLine($"; {comment.Value}");
            } else if (node is XElement element) {
                // Wenn das Element Unterelemente hat, behandeln wir es als Sektion
                if (element.HasElements) {
                    if (sb.Length > 0)
                        sb.AppendLine();
                    sb.AppendLine($"[{element.Name.LocalName}]");
                    foreach (var subNode in element.Nodes()) {
                        if (subNode is XComment subComment)
                            sb.AppendLine($"; {subComment.Value}");
                        else if (subNode is XElement subElement)
                            sb.AppendLine($"{subElement.Name.LocalName}={subElement.Value.ToNonCritical()}");
                    }
                } else {
                    // Flacher Key-Value Pair
                    sb.AppendLine($"{element.Name.LocalName}={element.Value.ToNonCritical()}");
                }
            }
        }

        return sb.ToString().TrimEnd('\r', '\n');
    }

    public override bool ParseContent(string content) {
        Clear();
        if (string.IsNullOrEmpty(content)) { return true; }

        content = content.Replace("\r\n", "\r").Replace("\n", "\r");
        var lines = content.Split('\r');

        XElement currentContainer = _doc.Root!;
        XComment? lastComment = null;

        foreach (var rawLine in lines) {
            var line = rawLine.Trim();
            if (string.IsNullOrEmpty(line)) { continue; }

            // Sektionen: [Section]
            if (line.StartsWith("[", StringComparison.Ordinal) && line.EndsWith("]", StringComparison.Ordinal)) {
                var sectionName = line.Substring(1, line.Length - 2).Trim();
                var sectionElement = new XElement(sectionName);
                _doc.Root!.Add(sectionElement);
                currentContainer = sectionElement;
                lastComment = null;
                continue;
            }

            // Kommentare: ; oder #
            if (line.StartsWith(";", StringComparison.Ordinal) || line.StartsWith("#", StringComparison.Ordinal)) {
                lastComment = new XComment(line.Substring(1).Trim());
                currentContainer.Add(lastComment);
                continue;
            }

            // Key-Value: Key=Value
            var eqIndex = line.IndexOf('=');
            if (eqIndex < 0)
                continue;

            var key = line.Substring(0, eqIndex).Trim();
            var value = line.Substring(eqIndex + 1).Trim().FromNonCritical();

            // XML-konforme Namen sicherstellen (einfache Validierung)
            key = string.Concat(key.Where(c => char.IsLetterOrDigit(c) || c == '_'));
            if (string.IsNullOrEmpty(key))
                continue;

            var entry = new XElement(key, value);
            currentContainer.Add(entry);
            lastComment = null;
        }

        return true;
    }

    #endregion
}