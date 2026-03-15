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
using System.Globalization;
using System.Text;

namespace BlueBasics.Classes.FileHelpers;

/// <summary>
/// Text-Format-Helfer für JSON-Dateien.
/// Implementiert einen einfachen manuellen JSON-Parser ohne NuGet-Pakete.
/// Unterstützt String, Zahl, Bool, null, Array und Object.
/// Reine In-Memory-Verarbeitung ohne Dateisystem.
/// </summary>
public class JsonHelper : TextFileHelper {

    #region Fields

    private object? _root;

    #endregion

    #region Properties

    /// <summary>
    /// Das geparste Root-Element (Dictionary, List, string, double, bool oder null).
    /// </summary>
    public object? Root => _root;

    #endregion

    #region Methods

    public override bool ParseContent(string content) {
        _root = null;

        if (string.IsNullOrWhiteSpace(content)) { return true; }

        try {
            var pos = 0;
            _root = ParseValue(content, ref pos);
            return true;
        } catch {
            return false;
        }
    }

    public override string SerializeContent() {
        var sb = new StringBuilder();
        SerializeValue(_root, sb, 0);
        return sb.ToString();
    }

    /// <summary>
    /// Gibt einen Wert aus dem Root-Object anhand eines Schlüssels zurück.
    /// </summary>
    public object? Get(string key) {
        if (_root is Dictionary<string, object?> dict && dict.TryGetValue(key, out var val)) {
            return val;
        }
        return null;
    }

    /// <summary>
    /// Gibt einen String-Wert aus dem Root-Object zurück.
    /// </summary>
    public string GetString(string key, string defaultValue = "") {
        var val = Get(key);
        return val is string s ? s : (val?.ToString() ?? defaultValue);
    }

    /// <summary>
    /// Setzt einen Wert im Root-Object (erstellt Root-Dict falls nötig).
    /// </summary>
    public void Set(string key, object? value) {
        if (_root is not Dictionary<string, object?> dict) {
            dict = new Dictionary<string, object?>(StringComparer.Ordinal);
            _root = dict;
        }
        dict[key] = value;
    }

    private static void SkipWhitespace(string json, ref int pos) {
        while (pos < json.Length && char.IsWhiteSpace(json[pos])) { pos++; }
    }

    private static object? ParseValue(string json, ref int pos) {
        SkipWhitespace(json, ref pos);

        if (pos >= json.Length) { return null; }

        var c = json[pos];

        if (c == '"') { return ParseString(json, ref pos); }
        if (c == '{') { return ParseObject(json, ref pos); }
        if (c == '[') { return ParseArray(json, ref pos); }
        if (c == 't') { pos += 4; return true; }
        if (c == 'f') { pos += 5; return false; }
        if (c == 'n') { pos += 4; return null; }
        if (c == '-' || char.IsDigit(c)) { return ParseNumber(json, ref pos); }

        throw new FormatException($"Unerwartetes Zeichen '{c}' an Position {pos}");
    }

    private static string ParseString(string json, ref int pos) {
        pos++; // öffnendes "
        var sb = new StringBuilder();

        while (pos < json.Length && json[pos] != '"') {
            if (json[pos] == '\\' && pos + 1 < json.Length) {
                pos++;
                switch (json[pos]) {
                    case '"': sb.Append('"'); break;
                    case '\\': sb.Append('\\'); break;
                    case '/': sb.Append('/'); break;
                    case 'n': sb.Append('\n'); break;
                    case 'r': sb.Append('\r'); break;
                    case 't': sb.Append('\t'); break;
                    case 'u':
                        if (pos + 4 < json.Length) {
                            var hex = json.Substring(pos + 1, 4);
                            sb.Append((char)Convert.ToInt32(hex, 16));
                            pos += 4;
                        }
                        break;
                    default: sb.Append(json[pos]); break;
                }
            } else {
                sb.Append(json[pos]);
            }
            pos++;
        }

        pos++; // schließendes "
        return sb.ToString();
    }

    private static double ParseNumber(string json, ref int pos) {
        var start = pos;
        if (json[pos] == '-') { pos++; }
        while (pos < json.Length && (char.IsDigit(json[pos]) || json[pos] == '.' || json[pos] == 'e' || json[pos] == 'E' || json[pos] == '+' || json[pos] == '-')) {
            pos++;
        }
        var numStr = json.Substring(start, pos - start);
        return double.Parse(numStr, CultureInfo.InvariantCulture);
    }

    private static Dictionary<string, object?> ParseObject(string json, ref int pos) {
        pos++; // {
        var dict = new Dictionary<string, object?>(StringComparer.Ordinal);

        SkipWhitespace(json, ref pos);

        while (pos < json.Length && json[pos] != '}') {
            SkipWhitespace(json, ref pos);

            var key = ParseString(json, ref pos);

            SkipWhitespace(json, ref pos);
            pos++; // :
            SkipWhitespace(json, ref pos);

            var value = ParseValue(json, ref pos);
            dict[key] = value;

            SkipWhitespace(json, ref pos);
            if (pos < json.Length && json[pos] == ',') { pos++; }
            SkipWhitespace(json, ref pos);
        }

        pos++; // }
        return dict;
    }

    private static List<object?> ParseArray(string json, ref int pos) {
        pos++; // [
        var list = new List<object?>();

        SkipWhitespace(json, ref pos);

        while (pos < json.Length && json[pos] != ']') {
            list.Add(ParseValue(json, ref pos));
            SkipWhitespace(json, ref pos);
            if (pos < json.Length && json[pos] == ',') { pos++; }
            SkipWhitespace(json, ref pos);
        }

        pos++; // ]
        return list;
    }

    private static void SerializeValue(object? value, StringBuilder sb, int indent) {
        switch (value) {
            case null:
                sb.Append("null");
                break;
            case bool b:
                sb.Append(b ? "true" : "false");
                break;
            case double d:
                sb.Append(d.ToString(CultureInfo.InvariantCulture));
                break;
            case int i:
                sb.Append(i.ToString(CultureInfo.InvariantCulture));
                break;
            case long l:
                sb.Append(l.ToString(CultureInfo.InvariantCulture));
                break;
            case string s:
                SerializeString(s, sb);
                break;
            case Dictionary<string, object?> dict:
                SerializeObject(dict, sb, indent);
                break;
            case List<object?> list:
                SerializeArray(list, sb, indent);
                break;
            default:
                SerializeString(value.ToString() ?? string.Empty, sb);
                break;
        }
    }

    private static void SerializeString(string s, StringBuilder sb) {
        sb.Append('"');
        foreach (var c in s) {
            switch (c) {
                case '"': sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default:
                    if (c < 0x20) {
                        sb.Append("\\u");
                        sb.Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                    } else {
                        sb.Append(c);
                    }
                    break;
            }
        }
        sb.Append('"');
    }

    private static void SerializeObject(Dictionary<string, object?> dict, StringBuilder sb, int indent) {
        sb.AppendLine("{");
        var pad = new string(' ', (indent + 1) * 2);
        var closePad = new string(' ', indent * 2);
        var first = true;

        foreach (var kv in dict) {
            if (!first) { sb.AppendLine(","); }
            first = false;
            sb.Append(pad);
            SerializeString(kv.Key, sb);
            sb.Append(": ");
            SerializeValue(kv.Value, sb, indent + 1);
        }

        sb.AppendLine();
        sb.Append(closePad);
        sb.Append('}');
    }

    private static void SerializeArray(List<object?> list, StringBuilder sb, int indent) {
        sb.AppendLine("[");
        var pad = new string(' ', (indent + 1) * 2);
        var closePad = new string(' ', indent * 2);
        var first = true;

        foreach (var item in list) {
            if (!first) { sb.AppendLine(","); }
            first = false;
            sb.Append(pad);
            SerializeValue(item, sb, indent + 1);
        }

        sb.AppendLine();
        sb.Append(closePad);
        sb.Append(']');
    }

    #endregion
}
