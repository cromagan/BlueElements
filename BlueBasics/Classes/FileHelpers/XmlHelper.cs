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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace BlueBasics.Classes.FileHelpers;

/// <summary>
/// Text-Format-Helfer für XML-Dateien.
/// Verwendet <see cref="XDocument"/> aus System.Xml.Linq (ohne NuGet-Pakete).
/// Reine In-Memory-Verarbeitung ohne Dateisystem.
/// </summary>
public class XmlHelper : TextFileHelper {

    #region Properties

    /// <summary>
    /// Das geparste XML-Dokument.
    /// </summary>
    public XDocument? Document { get; private set; }

    /// <summary>
    /// Das Root-Element des XML-Dokuments.
    /// </summary>
    public XElement? Root => Document?.Root;

    #endregion

    #region Methods

    public override bool ParseContent(string content) {
        Document = null;

        if (string.IsNullOrWhiteSpace(content)) { return true; }

        try {
            Document = XDocument.Load(new StringReader(content));
            return true;
        } catch {
            return false;
        }
    }

    public override string SerializeContent() {
        if (Document == null) { return string.Empty; }

        var sw = new StringWriter();
        Document.Save(sw);
        return sw.ToString();
    }

    /// <summary>
    /// Gibt den Text-Inhalt eines Elements am angegebenen XPath-Pfad zurück.
    /// Unterstützt nur einfache Element-Pfade (z.B. "Root/Child/Element").
    /// </summary>
    public string GetElementValue(string path, string defaultValue = "") {
        if (Root == null || string.IsNullOrEmpty(path)) { return defaultValue; }

        var parts = path.Split('/');
        XElement? current = Root;

        foreach (var part in parts) {
            current = current?.Element(part);
            if (current == null) { return defaultValue; }
        }

        return current?.Value ?? defaultValue;
    }

    /// <summary>
    /// Gibt alle Kindelemente mit dem angegebenen Namen zurück.
    /// </summary>
    public IEnumerable<XElement> GetElements(string name) {
        return Root?.Elements(name) ?? Enumerable.Empty<XElement>();
    }

    /// <summary>
    /// Gibt den Wert eines Attributs eines Elements zurück.
    /// </summary>
    public string GetAttributeValue(XElement element, string attributeName, string defaultValue = "") {
        return element.Attribute(attributeName)?.Value ?? defaultValue;
    }

    #endregion
}
