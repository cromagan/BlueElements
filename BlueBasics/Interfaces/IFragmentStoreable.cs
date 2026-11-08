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
    /// Schreibt System-Bytes (User, DateTimeUtc, Machine), den eigenen Block per
    /// SerializeCore und abschließend den EOF-Satz. Nur die Wurzel (Tiefe 1) setzt
    /// System-Bytes und EOF; Sub-Objekte werden mit Serialize(stream, depth + 1)
    /// aufgerufen und schreiben nur ihren Block.
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
            allok &= FragmentRecord.Append(stream, depth, FragmentTagType.DateTimeUtc, DateTime.UtcNow);
            allok &= FragmentRecord.Append(stream, depth, FragmentTagType.Machine, Environment.MachineName);
        }

        // Kommentar-Trenner auf eigener Tiefe: markiert den eigenen Block, ohne
        // fremde Blöcke zu zerschneiden; Deserialize überspringt Steuer-Sätze.
        allok &= FragmentRecord.Append(stream, depth, FragmentTagType.Comment, FragmentRecord.SeparatorComment);
        allok &= SerializeCore(stream, depth);


        // EOF-Satz nur am Root schreiben: Er markiert das saubere Datei-Ende.

        if (isRoot) {
            allok &= FragmentRecord.Append(stream, depth, FragmentTagType.Eof, string.Empty);
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
    /// vom Switch des Aufrufers gelesen. Die Items liegen eine Ebene tiefer; ihr
    /// erster Satz geht an create (baut den Empfänger — bei Schlüssel-Objekten ist
    /// er der Schlüssel) und an DeserializeThis, den Rest liest das Item selbst.
    /// Kommentar-Trenner auf der Item-Tiefe werden übersprungen; zwischen den Items
    /// steht je ein Trenner zwei Ebenen unter der Liste. Fehlt ein Satz der
    /// Item-Tiefe, endet die Liste (der Stream bleibt an dessen Satzanfang stehen).
    /// Liefert null bei echten Lesefehlern.
    /// </summary>
    public static ReadOnlyCollection<T>? DeserializeList<T>(Stream stream, int depth, Func<FragmentRecord, T> create) where T : class, IFragmentStoreable {
        List<T> result = [];

        while (true) {
            if (FragmentRecord.TryReadNext(stream, depth + 1) is not { } first) {
                // Kein Item-Satz mehr; einen hängenden Trenner (leeres Item) dennoch konsumieren
                var position = stream.Position;

                if (FragmentRecord.TryReadNext(stream, depth + 2) is not { } trailing || !trailing.IsTag) {
                    stream.Position = position;
                    break;
                }

                continue;
            }

            if (first.IsTag) { continue; }

            if (create(first) is not { } item || !item.DeserializeThis(stream, first) || !item.Deserialize(stream, depth + 1)) { return null; }

            result.Add(item);

            // Trenner konsumieren; fehlt er, folgt kein weiteres Item
            var separatorPosition = stream.Position;

            if (FragmentRecord.TryReadNext(stream, depth + 2) is not { } separator || !separator.IsTag) {
                stream.Position = separatorPosition;
                break;
            }
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
