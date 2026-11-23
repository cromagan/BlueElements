// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.IO;
using System.Threading;

namespace TableStress;

/// <summary>
/// Text-Protokoll als Datei "Benutzer-Uhrzeit.txt" im Ordner der Tabelle.
/// Jede Zeile beginnt mit dem Protokoll-Dateinamen; Develop-Meldungen ab Warning laufen mit ein.
/// </summary>
public sealed class StressProtocol : IDisposable {

    #region Fields

    private static readonly UTF8Encoding Utf8Bom = new(true);

    private readonly object locker = new();
    private readonly StreamWriter writer;
    private string lastMeldung = string.Empty;
    private int meldungRepeat;

    #endregion

    #region Constructors

    private StressProtocol(string fullPath) {
        FullPath = fullPath;
        writer = new StreamWriter(fullPath, false, Utf8Bom) { AutoFlush = true };
    }

    #endregion

    #region Properties

    public string FullPath { get; }

    public string Name => Path.GetFileName(FullPath);

    #endregion

    #region Methods

    /// <summary>
    /// Hängt eine Watchdog-Zeile an, ohne das Protokoll offen zu halten.
    /// </summary>
    public static void AppendWatchdog(string protocolFile, string detail) {
        var line = $"{Path.GetFileName(protocolFile)}|{DateTime.Now:HH:mm:ss.fff}|WATCHDOG|{detail}";

        for (var attempt = 0; attempt < 5; attempt++) {
            try {
                File.AppendAllText(protocolFile, line + "\r\n", Utf8Bom);
                return;
            } catch (IOException) {
                // Der Stresstest schreibt noch — kurz warten und erneut versuchen.
                Thread.Sleep(250);
            } catch {
                // Ein gesperrtes Protokoll darf den Watchdog nicht abbrechen.
                return;
            }
        }
    }

    /// <summary>
    /// Erstellt das Protokoll im Ordner der Tabelle und leitet die Develop-Meldungen dorthin um.
    /// </summary>
    public static StressProtocol Create(string tablePath, string user) {
        var dir = Path.GetDirectoryName(Path.GetFullPath(tablePath)) ?? AppContext.BaseDirectory;
        var name = $"{user}-{DateTime.Now:HH-mm-ss}";
        var fullPath = Path.Combine(dir, name + ".txt");

        if (File.Exists(fullPath)) { fullPath = Path.Combine(dir, $"{name}-{DateTime.Now:fff}.txt"); }

        var p = new StressProtocol(fullPath);
        Develop.MessageDG += p.OnDevelopMessage;
        return p;
    }

    public void Dispose() {
        Develop.MessageDG -= OnDevelopMessage;
        lock (locker) {
            FlushRepeatedMeldung();
            writer.Dispose();
        }
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Schreibt eine Aktions-Zeile: Protokollname|Uhrzeit|Aktion|Detail. Zeilenumbrüche
    /// im Detail werden ersetzt, damit jede Protokollzeile dem Format entspricht.
    /// </summary>
    public void Write(string aktion, string detail) {
        var line = $"{Name}|{DateTime.Now:HH:mm:ss.fff}|{aktion}|{OneLine(detail)}";

        lock (locker) {
            try {
                writer.WriteLine(line);
            } catch (Exception ex) {
                // Ein volles oder gesperrtes Protokoll darf den Stresstest nicht abbrechen —
                // der Schreibfehler wird aber sichtbar gemacht und per Direktzugriff umgangen.
                try {
                    File.AppendAllText(FullPath, line + "\r\n", Utf8Bom);
                    Console.Error.WriteLine($"Protokoll umgeleitet: {ex.GetType().Name} {ex.Message}");
                } catch (Exception ex2) {
                    Console.Error.WriteLine($"Protokoll-Schreibfehler: {ex.GetType().Name} {ex.Message} / Fallback: {ex2.GetType().Name} {ex2.Message}");
                }
            }
        }

        Console.Out.WriteLine(line);
    }

    /// <summary>
    /// Develop-Meldungen ab Warning mit ins Protokoll aufnehmen. Identische
    /// aufeinanderfolgende Meldungen werden gezählt und nur einmal angemerkt.
    /// </summary>
    private void OnDevelopMessage(ErrorType type, object? reference, string category, ImageCode symbol, string message, int indent) {
        _ = reference;
        _ = symbol;
        _ = indent;
        if (type > ErrorType.Warning) { return; }

        lock (locker) {
            var text = $"{type} {category}: {OneLine(message)}";

            if (text == lastMeldung) {
                meldungRepeat++;
                return;
            }

            FlushRepeatedMeldung();
            lastMeldung = text;
            Write("MELDUNG", text);
        }
    }

    /// <summary>
    /// Schreibt die Anzahl der unterdrückten Wiederholungen der letzten Meldung.
    /// </summary>
    private void FlushRepeatedMeldung() {
        if (meldungRepeat <= 0) { return; }
        Write("MELDUNG", $"{lastMeldung} — weitere identische Meldungen unterdrückt: {meldungRepeat}");
        meldungRepeat = 0;
    }

    /// <summary>
    /// Ersetzt Zeilenumbrüche, damit eine Meldung eine Protokollzeile bleibt.
    /// </summary>
    private static string OneLine(string text) => text.Replace("\r\n", " / ").Replace('\r', ' ').Replace('\n', ' ');

    #endregion
}
