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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace BlueBasics.Classes.FileHelpers;

public class IniSerializer : DataSerializer {

    #region Methods

    public override bool Deserialize(string content) {
        Clear();
        if (string.IsNullOrEmpty(content)) { return true; }

        // 1. Altlasten heilen (Rekursives Decoding)
        content = DecodeFully(content.Trim());

        // 2. Den Root-Knoten vorbereiten
        var root = new XElement("Root");
        _doc.Add(root);

        // 3. Rekursiv parsen
        ParseRecursive(content, root);

        return true;
    }

    public override string Serialize() {
        if (_doc.Root == null || !_doc.Root.HasElements)
            return "{}";

        return BuildRecursiveString(_doc.Root.Elements().First());
    }

    private string BuildRecursiveString(XElement element) {
        var sb = new StringBuilder();
        sb.Append("{");

        var children = element.Elements().ToList();
        for (int i = 0; i < children.Count; i++) {
            var child = children[i];
            sb.Append(child.Name.LocalName);
            sb.Append("=");

            if (child.HasElements) {
                sb.Append(BuildRecursiveString(child));
            } else {
                // ToNonCritical wurde hier entfernt, wie gewünscht.
                sb.Append(child.Value);
            }

            if (i < children.Count - 1) {
                sb.Append(",");
            }
        }

        sb.Append("}");
        return sb.ToString();
    }

    private string DecodeFully(string input) {
        string last;
        string current = input;
        do {
            last = current;
            current = FromNonCritical(current);
        } while (current != last);
        return current;
    }

    private int FindTopLevelEquals(string input) {
        int bracketLevel = 0;
        for (int i = 0; i < input.Length; i++) {
            if (input[i] == '{')
                bracketLevel++;
            else if (input[i] == '}')
                bracketLevel--;
            else if (input[i] == '=' && bracketLevel == 0)
                return i;
        }
        return -1;
    }

    private string FromNonCritical(string txt) {
        if (string.IsNullOrEmpty(txt) || txt.Length < 3 || !txt.Contains("["))
            return txt;

        var result = new StringBuilder(txt.Length);
        for (var i = 0; i < txt.Length; i++) {
            if (i <= txt.Length - 3 && txt[i] == '[' && txt[i + 2] == ']') {
                var patternChar = txt[i + 1];
                string? replacement = patternChar switch {
                    'A' => ";",
                    'B' => "<",
                    'C' => ">",
                    'D' => "\r\n",
                    'E' => "\r",
                    'F' => "\n",
                    'G' => "|",
                    'H' => "}",
                    'I' => "{",
                    'J' => "=",
                    'K' => ",",
                    'L' => "&",
                    'M' => "/",
                    'N' => "\"",
                    'Z' => "[",
                    _ => null
                };

                if (replacement != null) {
                    result.Append(replacement);
                    i += 2;
                    continue;
                }
            }
            result.Append(txt[i]);
        }
        return result.ToString();
    }

    private void ParseRecursive(string content, XContainer parent) {
        // Entferne äußere Klammern, falls vorhanden
        content = content.Trim();
        if (content.StartsWith("{") && content.EndsWith("}")) {
            content = content.Substring(1, content.Length - 2).Trim();
        }

        if (string.IsNullOrEmpty(content))
            return;

        int bracketLevel = 0;
        int startPos = 0;
        List<string> parts = new List<string>();

        // Splitte nach Komma, aber beachte Klammer-Ebenen
        for (int i = 0; i < content.Length; i++) {
            char c = content[i];
            if (c == '{')
                bracketLevel++;
            else if (c == '}')
                bracketLevel--;
            else if (c == ',' && bracketLevel == 0) {
                parts.Add(content.Substring(startPos, i - startPos));
                startPos = i + 1;
            }
        }
        parts.Add(content.Substring(startPos));

        foreach (var part in parts) {
            var pair = part.Trim();
            var eqIndex = FindTopLevelEquals(pair);

            if (eqIndex < 0)
                continue;

            var key = pair.Substring(0, eqIndex).Trim();
            var value = pair.Substring(eqIndex + 1).Trim();

            // XML-konforme Namen sicherstellen
            var validKey = string.Concat(key.Where(c => char.IsLetterOrDigit(c) || c == '_'));
            if (string.IsNullOrEmpty(validKey))
                continue;

            var element = new XElement(validKey);
            parent.Add(element);

            if (value.StartsWith("{") && value.EndsWith("}")) {
                ParseRecursive(value, element);
            } else {
                element.Value = value;
            }
        }
    }

    #endregion
}