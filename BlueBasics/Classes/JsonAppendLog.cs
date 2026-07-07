// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.IO;
using System.Threading;
using static BlueBasics.ClassesStatic.IO;

namespace BlueBasics.Classes;

/// <summary>
/// Generisches JSON-Append-Log für inkrementelle Speicherung von
/// <see cref="IJsonParseable" />-Objekten. Orientiert sich am Append-Log-Pattern
/// von <c>TableFragments</c>: pro Session eine persönliche Log-Datei im
/// Append-Modus, thread-safe über eine Semaphore.
/// </summary>
public sealed class JsonAppendLog : IDisposableExtended {

    #region Fields

    private readonly string _filename;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private StreamWriter? _writer;

    #endregion

    #region Constructors

    /// <summary>
    /// Erstellt ein neues Append-Log für die angegebene Datei.
    /// Der StreamWriter wird beim ersten <see cref="Append" /> im Append-Modus
    /// geöffnet (<see cref="FileShare.Read" />).
    /// </summary>
    public JsonAppendLog(string filename) {
        _filename = filename;
    }

    #endregion

    #region Properties

    public bool IsDisposed { get; private set; }

    #endregion

    #region Methods

    /// <summary>
    /// Liest alle Änderungen aus der Log-Datei (oder <c>null</c> bei Fehler /
    /// nicht existierender Datei). Andere Log-Dateien im selben Verzeichnis
    /// mit gleichem Basismuster werden ebenfalls gelesen (Multi-User-Support).
    /// </summary>
    public static List<(string Path, JsonElement Value)> ReadAllChanges(string filename) {
        var result = new List<(string Path, JsonElement Value)>();

        if (!FileExists(filename)) { return result; }

        try {
            var content = ReadAllText(filename, Encoding.UTF8);

            foreach (var line in content.SplitAndCutByCr()) {
                if (string.IsNullOrWhiteSpace(line)) { continue; }
                if (line.StartsWith('-')) { continue; }

                try {
                    using var doc = JsonDocument.Parse(line);
                    var root = doc.RootElement;
                    var p = root.GetString("path");
                    if (string.IsNullOrEmpty(p)) { continue; }
                    if (root.TryGetProperty("value", out var v)) {
                        result.Add((p, v.Clone()));
                    }
                } catch {
                    Develop.DebugPrint("JsonAppendLog: ungültige Zeile übersprungen");
                }
            }
        } catch {
            Develop.DebugPrint("JsonAppendLog.ReadAllChanges fehlgeschlagen: " + filename);
        }

        return result;
    }

    /// <summary>
    /// Hängt eine einzelne Property-Änderung an die Log-Datei an.
    /// Format pro Zeile: <c>{"path":"…","value":&lt;json&gt;,"hash":"…"}</c>
    /// </summary>
    /// <param name="path">Key-basierter Pfad, z.B. <c>Items[btnSubmit].Rotation</c>.</param>
    /// <param name="value">Der geänderte Wert als beliebiger JSON-Knoten (Zahl, String, Bool, …).</param>
    public void Append(string path, JsonNode value) {
        if (IsDisposed || string.IsNullOrEmpty(path)) { return; }

        EnsureWriter();

        var line = BuildLine(path, value);

        try {
            _semaphore.Wait();
            try {
                _writer?.WriteLine(line);
                _writer?.Flush();
            } finally {
                _semaphore.Release();
            }
        } catch {
            Develop.DebugPrint("JsonAppendLog.Append fehlgeschlagen: " + _filename);
        }
    }

    /// <summary>
    /// Löscht die Log-Datei nach erfolgreicher Konsolidierung.
    /// Schließt vorher den Writer.
    /// </summary>
    public void Clear() {
        if (IsDisposed) { return; }

        try {
            _semaphore.Wait();
            try {
                CloseWriter();
                DeleteFile(_filename, false);
            } finally {
                _semaphore.Release();
            }
        } catch {
            Develop.DebugPrint("JsonAppendLog.Clear fehlgeschlagen: " + _filename);
        }
    }

    public void Dispose() {
        if (IsDisposed) { return; }
        IsDisposed = true;

        try {
            _semaphore.Wait();
            try {
                CloseWriter();
            } finally {
                _semaphore.Release();
            }
        } catch { }

        try { _semaphore.Dispose(); } catch { }
    }

    private static string BuildLine(string path, JsonNode value) {
        var lineObj = new JsonObject {
            ["path"] = path,
            ["value"] = value.DeepClone(),
            ["hash"] = Generic.GetSHA256HashString(path + value.ToJsonString())
        };
        return lineObj.ToJsonString();
    }

    private void CloseWriter() {
        var w = _writer;
        _writer = null;

        if (w is null) { return; }

        try {
            w.WriteLine("- EOF");
            w.Flush();
        } catch { } finally {
            try { w.Dispose(); } catch { }
        }
    }

    private void EnsureWriter() {
        if (_writer is not null) { return; }
        if (IsDisposed) { return; }

        var dir = _filename.FilePath();
        if (!string.IsNullOrEmpty(dir) && !DirectoryExists(dir)) {
            CreateDirectory(dir);
        }

        try {
            var fs = new FileStream(_filename, FileMode.Append, FileAccess.Write, FileShare.Read);
            _writer = new StreamWriter(fs, Encoding.UTF8) {
                AutoFlush = true
            };

            _writer.WriteLine("- JsonAppendLog v1");
            _writer.WriteLine("- File " + _filename);
            _writer.WriteLine("- User " + UserName);
        } catch {
            _writer?.Dispose();
            _writer = null;
            Develop.DebugPrint("JsonAppendLog.EnsureWriter fehlgeschlagen: " + _filename);
        }
    }

    #endregion
}