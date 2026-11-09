// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Collections.ObjectModel;
using System.IO;

namespace BlueBasics.Interfaces;

/// <summary>
/// Pendant zu IJsonParseable für das binäre Fragment-Format „Typ + Key“.
/// Implementierer schreiben ihren Zustand als tiefenbasierte Fragment-Sätze
/// (Serialize) und lesen ihn blockweise zurück (Deserialize): Skalar-Sätze setzt
/// der klasseneigene Switch, Sub*-Sätze (Content = Key) eröffnen den Block des
/// Kind-Objekts, das der Empfänger sucht, notfalls erzeugt, und eine Ebene tiefer
/// selbst per Deserialize einliest — die Einrücktiefe ersetzt den gespeicherten Pfad.
/// </summary>
public interface IFragmentStoreable {

    #region Events

    /// <summary>
    /// Bubbling-Event: Container abonnieren ihre Kinder und reichen die Änderung
    /// mit dem eigenen Pfad-Segment als Präfix weiter zum Root.
    /// </summary>
    event EventHandler<FragmentChangedEventArgs>? FragmentChanged;

    #endregion

    #region Methods

    /// <summary>
    /// Schreibt System-Bytes (User, Machine), den eigenen Block per
    /// SerializeCore und abschließend den EOF-Satz. Nur die Wurzel (Tiefe 1) setzt
    /// System-Bytes, Kommentar-Trenner und EOF; Sub-Objekte werden mit
    /// Serialize(stream, depth + 1) aufgerufen und schreiben nur ihren Block.
    /// Bewusst kein Zeitstempel-Satz: Die Speicherzeit steht im Dateisystem
    /// (LastWriteTimeUtc), ein UtcNow-Satz würde identische Stände nicht mehr
    /// byteidentisch speichern. Bewusst kein Kommentar-Trenner bei Sub-Objekten:
    /// Er hätte Item-Tiefe und würde vom Deserialize hinter die Blockgrenze
    /// genommen — die Grenzen zwischen Listen-Items setzt der Listen-Writer
    /// zwei Ebenen unter der Liste.
    /// </summary>
    bool Serialize(Stream stream, int depth, string type) {
        var allok = true;
        var isRoot = depth == 1;

        if (isRoot) {
            // Anfangs bytes nur im Root schreiben, weil sie durchgehend gleich sind
            allok &= FragmentRecord.Append(stream, depth, FragmentTagType.SerializerVersion, "1.00");
            allok &= FragmentRecord.Append(stream, depth, FragmentTagType.CopyrightInfo, "(c) by Christian Peter - see License for details.");
            allok &= FragmentRecord.Append(stream, depth, FragmentTagType.Type,  type);
            allok &= FragmentRecord.Append(stream, depth, FragmentTagType.User, Generic.UserName);
            allok &= FragmentRecord.Append(stream, depth, FragmentTagType.Machine, Environment.MachineName);

            // Kommentar-Trenner markiert das Ende der Root-Kopfzeilen
            allok &= FragmentRecord.Append(stream, depth, FragmentTagType.Comment, FragmentRecord.SeparatorComment);
            allok &= SerializeCore(stream, depth);

            // EOF-Satz nur am Root schreiben: Er markiert das saubere Datei-Ende.
            allok &= FragmentRecord.Append(stream, depth, FragmentTagType.Eof, string.Empty);
        } else {
            allok &= SerializeCore(stream, depth);
        }

        return allok;
    }

    /// <summary>
    /// Schreibt den eigenen Block in den Stream: Blatt-Werte bei der Tiefe depth,
    /// Sub-Objekte als Sub*-Satz (Content = Key) gefolft von deren Block per
    /// Serialize bei depth + 1. Liefert false, wenn ein Satz ungültig oder ein
    /// Sub-Objekt nicht serialisierbar ist.
    /// </summary>
    bool SerializeCore(Stream stream, int depth);

    /// <summary>
    /// Verarbeitet einen einzelnen Satz des eigenen Blocks (Tiefe = record.Depth):
    /// Blatt-Werte im klasseneigenen Switch, Sub*-Sätze suchen bzw. erzeugen den
    /// Empfänger und lassen dessen Block per Deserialize eine Ebene tiefer lesen.
    /// Liefert false bei echten Fehlern.
    /// </summary>
    bool DeserializeThis(Stream stream, FragmentRecord record);

    /// <summary>
    /// Wird von Deserialize nach dem erfolgreichen Einlesen des eigenen Blocks
    /// aufgerufen: hier erfolgen Nacharbeiten wie das Neuaufbauen abgeleiteter
    /// Werte (Pendant zu IParseable.ParseFinished).
    /// </summary>
    bool ParseFinished() => true;

    /// <summary>
    /// Löst das FragmentChanged-Event mit den fertigen Argumenten aus
    /// (Pendant zu OnPropertyChangedExt).
    /// </summary>
    void OnFragmentChanged(FragmentChangedEventArgs e);

    #endregion
}

/// <summary>
/// Statische Erweiterungsmethoden für IFragmentStoreable.
/// </summary>
public static class FragmentStoreableExtension {

    #region Methods

    /// <summary>
    /// Schreibt System-Bytes (User, DateTimeUtc, Machine), den eigenen Block per
    /// SerializeCore und abschließend den EOF-Satz (Details im Interface). Diese
    /// Überladung ermöglicht den Serialize-Aufruf über konkrete Klassentypen,
    /// da Default-Interface-Methoden nur über den Schnittstellentyp sichtbar sind.
    /// </summary>
    public static bool Serialize(this IFragmentStoreable sender, Stream stream, int depth) => sender.Serialize(stream, depth, sender.GetType().Name);

    /// <summary>
    /// Liest den eigenen Block bis zu seinem Ende: Sätze genau der Tiefe depth gehen an
    /// DeserializeThis, ein Satz anderer Tiefe beendet den Block (der Stream bleibt an
    /// dessen Satzanfang stehen). Steuer-Sätze werden übersprungen; Nach dem
    /// erfolgreichen Block-Ende wird ParseFinished aufgerufen. Liefert false bei echten
    /// Lesefehlern.
    /// </summary>
    public static bool Deserialize(this IFragmentStoreable sender, Stream stream, int depth) {
        while (FragmentRecord.TryReadNext(stream, depth) is { } record) {
            if (record.IsTag) {
                // Steuer-Sätze tragen keine Klassen-Daten.
                continue;
            }

            if (!sender.DeserializeThis(stream, record)) { return false; }
        }

        return sender.ParseFinished();
    }

    /// <summary>
    /// Liest eine Liste von Schlüssel-Objekten: Der ListKeyObject-Satz wurde bereits
    /// vom Switch des Aufrufers gelesen. Der komplette Block (alle Sätze tiefer als
    /// die List-Ebene) wird gepuffert, an den Item-Trennern (Tag-Sätze zwei Ebenen
    /// unter der Liste) segmentiert und je Segment exakt ein Item geladen — das
    /// Item-Deserialize kann so seine Blockgrenze nicht verfehlen, auch wenn alle
    /// Items dieselbe Tiefe haben. Der erste Satz jedes Segments geht an create
    /// (baut den Empfänger — bei Schlüssel-Objekten ist er der Schlüssel) und an
    /// DeserializeThis, den Rest liest das Item selbst. Liefert null bei echten
    /// Lesefehlern.
    /// </summary>
    public static ReadOnlyCollection<T>? DeserializeList<T>(Stream stream, int depth, Func<FragmentRecord, T> create) where T : class, IFragmentStoreable {
        // Listen-Block in den Puffer kopieren: alle Sätze tiefer als depth.
        // Der stream steht danach exakt am nächsten Satz der List-Ebene.
        using var blockStream = new MemoryStream();

        while (true) {
            var pos0 = stream.Position;

            if (FragmentRecord.TryReadNext(stream) is not { } record) { stream.Position = pos0; break; }

            var pos1 = stream.Position;

            if (record.Depth <= depth) {
                stream.Position = pos0;
                break;
            }

            var size = (int)(pos1 - pos0);
            var tmp = new byte[size];
            _ = stream.Position = pos0;
            _ = stream.Read(tmp, 0, size);
            blockStream.Write(tmp, 0, size);
        }

        // Segmentgrenzen: Trenner-Sätze (Tiefe depth+2, Tag) beenden ein Segment.
        var buffer = blockStream.ToArray();
        List<long> cuts = [0];
        blockStream.Position = 0;

        while (FragmentRecord.TryReadNext(blockStream) is { } rec) {
            if (rec.Depth == depth + 2 && rec.IsTag) { cuts.Add(blockStream.Position); }
        }

        cuts.Add(blockStream.Length);

        List<T> result = [];

        for (var i = 0; i < cuts.Count - 1; i++) {
            var start = (int)cuts[i];
            var len = (int)(cuts[i + 1] - start);

            if (len <= 0) { continue; }

            using var segment = new MemoryStream(buffer, start, len);

            if (FragmentRecord.TryReadNext(segment) is not { } first) { continue; }

            // Kommentar-Trenner am Segment-Anfang überspringen (Alt-Format)
            if (first.IsTag) {
                if (FragmentRecord.TryReadNext(segment) is not { } next) { continue; }
                first = next;
            }

            if (create(first) is not { } item || !item.DeserializeThis(segment, first) || !item.Deserialize(segment, depth + 1)) { return null; }

            result.Add(item);
        }

        return new ReadOnlyCollection<T>(result);
    }

    /// <summary>
    /// Liest den Block eines einzelnen Sub*-Objekts: Der bereits gelesene
    /// Navigations-Satz benennt den Empfänger; ein bestehendes Objekt (existing)
    /// wird wiederverwendet, sonst per create erzeugt. Liefert null bei Lesefehlern.
    /// </summary>
    public static T? DeserializeOrCreate<T>(Stream stream, int depth, T? existing, Func<T> create) where T : class, IFragmentStoreable {
        var target = existing ?? create();

        return target.Deserialize(stream, depth + 1) ? target : null;
    }

    #endregion
}
