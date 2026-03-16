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

using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using static BlueBasics.ClassesStatic.Converter;

namespace BlueBasics.Classes.FileHelpers;

public abstract class TextFileHelper : IEnumerable<string> {

    #region Fields

    protected readonly Dictionary<string, string> _items = new(StringComparer.OrdinalIgnoreCase);

    #endregion

    #region Constructors

    protected TextFileHelper() { }

    #endregion

    #region Properties

    public int Count => _items.Count;

    #endregion

    #region Methods

    public void Clear() => _items.Clear();

    public abstract string FinishParseable();

    public bool GetBool(string key, bool defaultValue = false) {
        var val = GetString(key);
        if (string.IsNullOrEmpty(val)) { return defaultValue; }
        return val.FromPlusMinus();
    }

    public Color GetColor(string key, Color defaultValue = default) {
        var val = GetString(key);
        if (string.IsNullOrEmpty(val)) { return defaultValue; }
        return ColorTryParse(val, out var c) ? c : defaultValue;
    }

    public DateTime? GetDateTime(string key) {
        var val = GetString(key);
        if (string.IsNullOrEmpty(val)) { return null; }
        return DateTimeTryParse(val, out var dt) ? dt : (DateTime?)null;
    }

    public double GetDouble(string key, double defaultValue = 0d) {
        var val = GetString(key);
        if (string.IsNullOrEmpty(val)) { return defaultValue; }
        return DoubleTryParse(val, out var d) ? d : defaultValue;
    }

    public IEnumerator<string> GetEnumerator() => _items.Keys.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public float GetFloat(string key, float defaultValue = 0f) {
        var val = GetString(key);
        if (string.IsNullOrEmpty(val)) { return defaultValue; }
        return float.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var f) ? f : defaultValue;
    }

    public int GetInt(string key, int defaultValue = 0) {
        var val = GetString(key);
        if (string.IsNullOrEmpty(val)) { return defaultValue; }
        return int.TryParse(val, out var i) ? i : defaultValue;
    }

    public string GetString(string key, string defaultValue = "")
        => _items.TryGetValue(key, out var v) ? v : defaultValue;

    public void ParseableAdd(string tagname, string? value) {
        if (value == null) { return; }
        _items[tagname] = value;
    }

    public void ParseableAdd(string tagname, DateTime? value) {
        if (value == null) { return; }
        ParseableAdd(tagname, ((DateTime)value).ToString5());
    }

    public void ParseableAdd(string tagname, DateTime? value, string format) {
        if (value == null) { return; }
        ParseableAdd(tagname, ((DateTime)value).ToString(format, System.Globalization.CultureInfo.InvariantCulture));
    }

    public void ParseableAdd(string tagname, Color value) => ParseableAdd(tagname, value.ToHtmlCode());

    public void ParseableAdd(string tagname, Bitmap? value) {
        if (value == null) { return; }
        ParseableAdd(tagname, BitmapToBase64(value, ImageFormat.Png));
    }

    public void ParseableAdd(string tagname, SizeF value) => ParseableAdd(tagname, value.ToString().ToNonCritical());

    public void ParseableAdd(string tagname, float value) => ParseableAdd(tagname, value.ToString1_5().ToNonCritical());

    public void ParseableAdd(string tagname, double value) => ParseableAdd(tagname, value.ToString1_5().ToNonCritical());

    public void ParseableAdd(string tagname, IHasKeyName? value) {
        if (value is null or IDisposableExtended { IsDisposed: true }) { return; }
        var v = value.KeyName;
        if (string.IsNullOrEmpty(v)) { return; }
        ParseableAdd(tagname, v.ToNonCritical());
    }

    public void ParseableAdd(string tagname, IEnumerable<IHasKeyName>? value, bool ignoreEmpty) {
        if (value?.Any() != true) {
            ParseableAdd(tagname, new List<string>(), ignoreEmpty);
            return;
        }
        ParseableAdd(tagname, value.ToListOfString(), ignoreEmpty);
    }

    public void ParseableAdd(string tagname, IEnumerable<IStringable> value) {
        foreach (var thisi in value) {
            ParseableAdd(tagname, thisi);
        }
    }

    public void ParseableAdd(string tagname, ICollection<string>? value, bool ignoreEmpty) {
        if (value is not { Count: not 0 }) {
            if (ignoreEmpty) { return; }
            ParseableAdd(tagname, string.Empty);
            return;
        }
        var l = new StringBuilder();
        foreach (var thisString in value) {
            l.Append(thisString.ToNonCritical());
            l.Append('|');
        }
        if (l.Length > 0) { l.Remove(l.Length - 1, 1); }
        ParseableAdd(tagname, l.ToString());
    }

    public void ParseableAdd<T>(string tagname, T value) where T : Enum {
        var underlyingType = Enum.GetUnderlyingType(typeof(T));
        if (underlyingType == typeof(int)) {
            ParseableAdd(tagname, (int)(object)value);
            return;
        }
        if (underlyingType == typeof(byte)) {
            ParseableAdd(tagname, (byte)(object)value);
            return;
        }
        Develop.DebugPrint(ErrorType.Error, "ParseableAdd: Unbekannter Enum-Typ!");
    }

    public void ParseableAdd(string tagname, IStringable? value) {
        if (value is null or IDisposableExtended { IsDisposed: true }) { return; }
        ParseableAdd(tagname, value.ParseableItems().FinishParseable().ToNonCritical());
    }

    public void ParseableAdd(string tagname, bool value) => ParseableAdd(tagname, value.ToPlusMinus());

    public abstract bool ParseContent(string content);

    public string TagGet(string key) => _items.TryGetValue(key, out var v) ? v : string.Empty;

    public List<string> TagGetAll(string key) {
        if (_items.TryGetValue(key, out var v)) { return [v]; }
        return [];
    }

    public void TagRemove(string key) => _items.Remove(key);

    public void TagSet(string key, DateTime? value) {
        if (value == null) { return; }
        TagSet(key, ((DateTime)value).ToString5());
    }

    public void TagSet(string key, float value) => TagSet(key, value.ToString1_5());

    public void TagSet(string key, double value) => TagSet(key, value.ToString1_5());

    public void TagSet(string key, int value) => TagSet(key, value.ToString(System.Globalization.CultureInfo.InvariantCulture));

    public void TagSet(string key, Color value) => TagSet(key, value.ToHtmlCode());

    public void TagSet(string key, string value) {
        TagRemove(key);
        ParseableAdd(key, value);
    }

    #endregion
}