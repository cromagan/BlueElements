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

namespace BlueBasics.Classes.FileHelpers;

/// <summary>
/// Abstrakte Basisklasse für Text-Format-Helfer (CSV, INI, JSON, XML).
/// Reine In-Memory-Verarbeitung — kein Dateisystem, kein CachedTextFile.
/// </summary>
public abstract class TextFileHelper {

    #region Methods

    /// <summary>
    /// Parst den übergebenen Text und befüllt die interne Datenstruktur.
    /// </summary>
    /// <param name="content">Der zu parsende Textinhalt.</param>
    /// <returns>true bei Erfolg, false bei Parse-Fehler.</returns>
    public abstract bool ParseContent(string content);

    /// <summary>
    /// Serialisiert die interne Datenstruktur zurück in einen Text.
    /// </summary>
    /// <returns>Den serialisierten Text.</returns>
    public abstract string SerializeContent();

    /// <summary>
    /// Gibt den serialisierten Inhalt zurück.
    /// Generische Funktion — ruft <see cref="SerializeContent"/> auf.
    /// Wird von ParseableAdd genutzt.
    /// </summary>
    public string GetContentAsString() => SerializeContent();

    /// <summary>
    /// Setzt den Inhalt aus einem Text.
    /// Generische Funktion — ruft <see cref="ParseContent"/> auf.
    /// </summary>
    public bool SetContentFromString(string content) => ParseContent(content);

    #endregion
}
