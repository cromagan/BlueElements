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

using BlueBasics.Attributes;
using BlueBasics.ClassesStatic;
using System;
using System.Text;
using static BlueBasics.ClassesStatic.IO;

namespace BlueBasics.Classes.FileSystemCaching;

/// <summary>
/// Gecachte Textdatei mit automatischer Encoding-Erkennung.
/// Unterstützt .txt, .ini, .md und .frg Dateien.
/// Das Encoding wird beim ersten Zugriff anhand der BOM erkannt und als Property bereitgestellt.
/// </summary>
[FileSuffix(".txt")]
[FileSuffix(".ini")]
[FileSuffix(".md")]
[FileSuffix(".frg")]
[FileSuffix(".blk")]
[FileSuffix(".csv")]
public sealed class CachedTextFile : CachedFile {

    #region Fields

    /// <summary>
    /// Gecachter Textinhalt — wird bei Invalidate() zurückgesetzt.
    /// </summary>
    private string? _cachedText;

    #endregion

    #region Constructors

    /// <summary>
    /// Erstellt eine neue CachedTextFile-Instanz.
    /// Wird über CachedFileSystem.CreateCachedFile() via Activator aufgerufen.
    /// </summary>
    internal CachedTextFile(string filename) : base(filename) { }

    #endregion

    #region Properties

    /// <summary>
    /// Das erkannte Encoding der Datei.
    /// Wird beim ersten Zugriff auf den Inhalt automatisch anhand der BOM ermittelt.
    /// Fallback ist UTF-8 ohne BOM.
    /// </summary>
    public Encoding DetectedEncoding { get; private set; } = Encoding.UTF8;

    public override bool ExtendedSave => !Filename.FileSuffix().Equals("BLK", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Textdateien werden nicht gezippt gespeichert.
    /// </summary>
    public override bool MustZipped => false;

    #endregion

    #region Methods

    /// <summary>
    /// Gibt den Textinhalt der Datei zurück.
    /// Beim ersten Aufruf wird das Encoding anhand der BOM erkannt.
    /// Das Ergebnis wird gecacht bis Invalidate() aufgerufen wird.
    /// </summary>
    public string GetContentAsString() {
        if (_cachedText != null) { return _cachedText; }

        var content = Content;
        if (content.Length == 0) {
            _cachedText = string.Empty;
            return _cachedText;
        }

        DetectedEncoding = DetectEncoding(content);
        var bomLength = GetBomLength(content);
        _cachedText = DetectedEncoding.GetString(content, bomLength, content.Length - bomLength);
        return _cachedText;
    }

    /// <summary>
    /// Gibt den Textinhalt der Datei mit einem expliziten Encoding zurück.
    /// Überschreibt die automatische Encoding-Erkennung NICHT dauerhaft.
    /// </summary>
    public string GetContentAsString(Encoding encoding) {
        var content = Content;
        if (content.Length == 0) { return string.Empty; }

        var bomLength = GetBomLength(content);
        return encoding.GetString(content, bomLength, content.Length - bomLength);
    }

    /// <summary>
    /// Invalidiert den Cache und setzt den gecachten Text zurück.
    /// </summary>
    public override void Invalidate() {
        _cachedText = null;
        base.Invalidate();
    }

    /// <summary>
    /// Menschenlesbarer Name dieser Datei für Statusmeldungen.
    /// </summary>
    public override string ReadableText() => Filename.FileNameWithoutSuffix();

    /// <summary>
    /// Erkennt das Encoding anhand der Byte-Order-Mark (BOM) am Dateianfang.
    /// Unterstützt UTF-8, UTF-16 LE/BE und UTF-32 LE/BE.
    /// Bei fehlender BOM wird geprüft, ob der Inhalt gültiges UTF-8 ist.
    /// </summary>
    private static Encoding DetectEncoding(byte[] content) {
        if (content.Length >= 3 && content[0] == 0xEF && content[1] == 0xBB && content[2] == 0xBF) {
            return Encoding.UTF8;
        }

        if (content.Length >= 4 && content[0] == 0x00 && content[1] == 0x00 && content[2] == 0xFE && content[3] == 0xFF) {
            return Encoding.GetEncoding(12001); // UTF-32 BE
        }

        if (content.Length >= 4 && content[0] == 0xFF && content[1] == 0xFE && content[2] == 0x00 && content[3] == 0x00) {
            return Encoding.UTF32; // UTF-32 LE
        }

        if (content.Length >= 2 && content[0] == 0xFE && content[1] == 0xFF) {
            return Encoding.BigEndianUnicode; // UTF-16 BE
        }

        if (content.Length >= 2 && content[0] == 0xFF && content[1] == 0xFE) {
            return Encoding.Unicode; // UTF-16 LE
        }

        // Ohne BOM: heuristische UTF-8-Erkennung
        return IsValidUtf8(content) ? Encoding.UTF8 : Encoding.Default;
    }

    /// <summary>
    /// Ermittelt die Länge der BOM in Bytes.
    /// </summary>
    private static int GetBomLength(byte[] content) {
        if (content.Length >= 3 && content[0] == 0xEF && content[1] == 0xBB && content[2] == 0xBF) { return 3; }
        if (content.Length >= 4 && content[0] == 0x00 && content[1] == 0x00 && content[2] == 0xFE && content[3] == 0xFF) { return 4; }
        if (content.Length >= 4 && content[0] == 0xFF && content[1] == 0xFE && content[2] == 0x00 && content[3] == 0x00) { return 4; }
        if (content.Length >= 2 && content[0] == 0xFE && content[1] == 0xFF) { return 2; }
        if (content.Length >= 2 && content[0] == 0xFF && content[1] == 0xFE) { return 2; }
        return 0;
    }

    /// <summary>
    /// Prüft, ob ein Byte-Array gültiges UTF-8 enthält.
    /// Sucht nach Multi-Byte-Sequenzen als Hinweis auf UTF-8.
    /// </summary>
    private static bool IsValidUtf8(byte[] content) {
        var i = 0;

        while (i < content.Length) {
            var b = content[i];

            if (b <= 0x7F) {
                i++;
                continue;
            }

            int expectedBytes;
            if ((b & 0xE0) == 0xC0) { expectedBytes = 1; } else if ((b & 0xF0) == 0xE0) { expectedBytes = 2; } else if ((b & 0xF8) == 0xF0) { expectedBytes = 3; } else { return false; } // Ungültiges UTF-8 Start-Byte

            if (i + expectedBytes >= content.Length) { return false; }

            for (var j = 1; j <= expectedBytes; j++) {
                if ((content[i + j] & 0xC0) != 0x80) { return false; }
            }

            i += expectedBytes + 1;
        }

        return true;
    }

    #endregion
}