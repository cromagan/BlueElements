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
using System.Xml.Linq;
using static BlueBasics.ClassesStatic.Converter;

namespace BlueBasics.Classes.FileHelpers;

public abstract class DataSerializer : IEnumerable<XElement> {

    #region Fields

    protected XDocument _doc = new(new XElement("Root"));

    #endregion

    #region Constructors

    protected DataSerializer() { }

    #endregion

    #region Properties

    public int Count => _doc.Root?.Elements().Count() ?? 0;

    #endregion

    #region Methods

    /// <summary>
    /// Fügt den Inhalt eines anderen Helpers als Unterstruktur (Sektion) hinzu.
    /// </summary>
    public void Add(string tagname, DataSerializer? value) {
        if (value == null || _doc.Root == null) { return; }

        var otherRoot = value.GetRoot();
        if (otherRoot == null) { return; }

        // Wir erstellen ein neues Element für die Sektion/Unterklasse
        var subSection = new XElement(tagname);

        // Wir kopieren alle Kind-Knoten (Elemente, Kommentare, etc.)
        // XElement.Add erstellt automatisch eine Kopie, wenn der Knoten
        // bereits Teil eines anderen XDocument ist.
        foreach (var node in otherRoot.Nodes()) {
            subSection.Add(node);
        }

        _doc.Root.Add(subSection);
    }

    public void Add(string tagname, DateTime? value, string format) {
        if (value == null) { return; }
        Add(tagname, ((DateTime)value).ToString(format, System.Globalization.CultureInfo.InvariantCulture));
    }

    public void Add(string tagname, Bitmap? value) {
        if (value == null) { return; }
        Add(tagname, BitmapToBase64(value, ImageFormat.Png));
    }

    public void Add(string tagname, bool value) => Add(tagname, value.ToPlusMinus());

    public void Add(string tagname, Color value) => Add(tagname, value.ToHtmlCode());

    public void Add(string tagname, DateTime? value) {
        if (value == null) { return; }
        Add(tagname, ((DateTime)value).ToString5());
    }

    public void Add(string tagname, string? value) {
        if (value == null || _doc.Root == null) { return; }
        _doc.Root.Add(new XElement(tagname, value));
    }

    public void Add(string tagname, SizeF value) => Add(tagname, value.ToString().ToNonCritical());

    public void Add(string tagname, float value) => Add(tagname, value.ToString1_5().ToNonCritical());

    public void Add(string tagname, double value) => Add(tagname, value.ToString1_5().ToNonCritical());

    public void Add(string tagname, IHasKeyName? value) {
        if (value is null or IDisposableExtended { IsDisposed: true }) { return; }
        var v = value.KeyName;
        if (string.IsNullOrEmpty(v)) { return; }
        Add(tagname, v.ToNonCritical());
    }

    public void Add(string tagname, IEnumerable<IHasKeyName>? value, bool ignoreEmpty) {
        if (value?.Any() != true) {
            Add(tagname, new List<string>(), ignoreEmpty);
            return;
        }
        Add(tagname, value.ToListOfString(), ignoreEmpty);
    }

    public void Add(string tagname, IEnumerable<IStringable> value) {
        foreach (var thisi in value) {
            Add(tagname, thisi.SerializableContent());
        }
    }

    public void Add(string tagname, ICollection<string>? value, bool ignoreEmpty) {
        if (value is not { Count: not 0 }) {
            if (ignoreEmpty) { return; }
            Add(tagname, string.Empty);
            return;
        }
        var l = new StringBuilder();
        foreach (var thisString in value) {
            l.Append(thisString.ToNonCritical());
            l.Append('|');
        }
        if (l.Length > 0) { l.Remove(l.Length - 1, 1); }
        Add(tagname, l.ToString());
    }

    public void Add<T>(string tagname, T value) where T : Enum {
        var underlyingType = Enum.GetUnderlyingType(typeof(T));
        if (underlyingType == typeof(int)) {
            Add(tagname, (int)(object)value);
            return;
        }
        if (underlyingType == typeof(byte)) {
            Add(tagname, (byte)(object)value);
            return;
        }
        Develop.DebugPrint(ErrorType.Error, "Add: Unbekannter Enum-Typ!");
    }

    /// <summary>
    /// Kopiert alle Einträge (Elemente und Kommentare) eines anderen Helpers
    /// flach in die aktuelle Ebene.
    /// </summary>
    /// <param name="other">Der Helper, dessen Inhalt kopiert werden soll.</param>
    public void AddRange(DataSerializer? other) {
        if (other?.GetRoot() == null || _doc.Root == null) { return; }

        // Wir iterieren über alle Knoten (Elemente, Kommentare, Sektionen)
        // des anderen Helpers und fügen sie unserem Root hinzu.
        foreach (var node in other.GetRoot().Nodes()) {
            // XElement.Add erstellt automatisch eine tiefe Kopie,
            // da der Knoten bereits einem anderen Dokument gehört.
            _doc.Root.Add(node);
        }
    }

    public void Clear() => _doc.Root?.RemoveAll();

    public abstract bool Deserialize(string content);

    public List<string> GetAll(string key) {
        return _doc.Root?.Elements()
            .Where(x => x.Name.LocalName.Equals(key, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Value)
            .ToList() ?? new List<string>();
    }

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

    public IEnumerator<XElement> GetEnumerator() => (_doc.Root?.Elements() ?? Enumerable.Empty<XElement>()).GetEnumerator();

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

    public string GetString(string key, string defaultValue = "") {
        var el = _doc.Root?.Elements().FirstOrDefault(x => x.Name.LocalName.Equals(key, StringComparison.OrdinalIgnoreCase));
        return el?.Value ?? defaultValue;
    }

    public void Remove(string key) {
        _doc.Root?.Elements()
            .Where(x => x.Name.LocalName.Equals(key, StringComparison.OrdinalIgnoreCase))
            .Remove();
    }

    public abstract string Serialize();

    public void Set(string tagname, IStringable? value) {
        Remove(tagname);
        if (value is null or IDisposableExtended { IsDisposed: true }) { return; }
        Add(tagname, value.SerializableContent());
    }

    public void Set(string key, DateTime? value) {
        Remove(key);
        if (value == null) { return; }
        Set(key, ((DateTime)value).ToString5());
    }

    public void Set(string key, float value) => Set(key, value.ToString1_5());

    public void Set(string key, double value) => Set(key, value.ToString1_5());

    public void Set(string key, int value) => Set(key, value.ToString(System.Globalization.CultureInfo.InvariantCulture));

    public void Set(string key, Color value) => Set(key, value.ToHtmlCode());

    public void Set(string key, string value) {
        Remove(key);
        Add(key, value);
    }

    // Hilfsmethode um an den internen Root zu kommen (protected)
    protected XElement? GetRoot() => _doc.Root;

    #endregion
}