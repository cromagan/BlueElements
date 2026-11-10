// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.IO;

namespace BlueTable.Classes;

/// <summary>
/// Transport-Eintrag für Änderungen im binären Fragment-Format: klassische
/// Wert-Änderung oder roher Sub*-Block samt Roh-Bytes. Dient dem Fragment-Austausch
/// und als Undo-Eintrag für Block-Änderungen im neuen Format.
/// </summary>
public sealed class FragmentChange(string tableName, string container) {

    #region Constructors

    /// <summary>
    /// Klassische Wert-Änderung (Zelle, Spalte, Kopf oder Kommando).
    /// </summary>
    public FragmentChange(string tablename, TableDataType command, string column, string row, string changedTo, string user, DateTime timeutc, string comment, string container) : this(tablename, container) {
        Command = command;
        ColName = column;
        RowKey = row;
        ChangedTo = changedTo;
        User = user;
        DateTimeUtc = timeutc;
        Comment = comment;
    }

    /// <summary>
    /// Roher Sub*-Block (Navigation + Kind-Block); wird als Fragment-Baum angewendet,
    /// nicht über SetValueInternal.
    /// </summary>
    public FragmentChange(string tablename, byte blockType, byte[] block, string user, DateTime timeutc, string container) : this(tablename, container) {
        BlockType = blockType;
        Block = block;
        User = user;
        DateTimeUtc = timeutc;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Roh-Bytes eines Sub*-Fragment-Blocks; nur gesetzt, wenn der Eintrag aus
    /// einem Sub*-Block gelesen wurde.
    /// </summary>
    public byte[]? Block { get; private set; }

    /// <summary>
    /// Navigations-Byte des Blocks (TableFragmentType 92-96); nur bei Blöcken gesetzt.
    /// </summary>
    public byte BlockType { get; private set; }

    public string ChangedTo { get; private set; } = string.Empty;

    public string ColName { get; private set; } = string.Empty;

    public TableDataType Command { get; private set; }

    public string Comment { get; private set; } = string.Empty;

    /// <summary>
    /// Quelldatei des Eintrags; dient dem Schutz vor dem Fragment-Aufräumen.
    /// </summary>
    public string Container { get; } = container;

    public DateTime DateTimeUtc { get; private set; } = DateTime.MinValue;

    public string RowKey { get; private set; } = string.Empty;

    public string TableName { get; } = tableName;

    public string User { get; private set; } = string.Empty;

    #endregion

    #region Methods

    /// <summary>
    /// Berechnet einen deterministischen Hash zur Dedup-Erkennung in TableFragments.
    /// Klassische Einträge über die mit UndoItem geteilte Formel, Blöcke über ihre
    /// kanonischen Roh-Bytes (Basis-Tiefe 1, wie im Fragment-Datei-Layout).
    /// </summary>
    public string Hash() {
        if (Block is { Length: > 0 } block) { return Convert.ToBase64String(block).GetMD5Hash(); }

        return (TableName + "|" + (int)Command + "|" + ColName + "|" + RowKey + "|" +
                DateTimeUtc.ToString9() + "|" + User + "|" + ChangedTo).GetMD5Hash();
    }

    /// <summary>
    /// Pfad-Typen klassischer Undo-Einträge im tiefenbasierten Fragment-Format.
    /// Sub*-Blöcke (TableFragmentType 92-96) werden als Ganzes gelesen und tragen
    /// keinen Pfad-Typ.
    /// </summary>
    private enum UndoNavigationType : byte {

        /// <summary>
        /// Navigation zu einer Spalte bzw. Zelle (Key = ColumnKey).
        /// </summary>
        SubColumn = 90,

        /// <summary>
        /// Navigation zu einer Zeile (Key = RowKey).
        /// </summary>
        RowKey = 210
    }

    /// <summary>
    /// Schreibt den Eintrag als tiefenbasierten Fragment-Block mit Meta: Zell-Werte
    /// über RowKey- und SubColumn-Satz, Spalten-Werte über den SubColumn-Satz,
    /// Kopf-Werte und Kommandos als einzelnen Blatt-Satz — je Navigation eine Ebene.
    /// Liefert false, wenn ein Satz ungültig oder zu groß ist.
    /// </summary>
    public static bool AppendFragment(Stream stream, int depth, FragmentChange item) {
        if (item.Command.IsCellValue()) {
            if (!FragmentRecord.Append(stream, depth, UndoNavigationType.RowKey, item.RowKey)) { return false; }
            if (!FragmentRecord.Append(stream, depth + 1, UndoNavigationType.SubColumn, item.ColName)) { return false; }
            return FragmentRecord.Append(stream, depth + 2, (byte)item.Command, item.ChangedTo, item.User, item.DateTimeUtc, item.Comment);
        }

        if (item.Command.IsColumnTag()) {
            if (!FragmentRecord.Append(stream, depth, UndoNavigationType.SubColumn, item.ColName)) { return false; }
            return FragmentRecord.Append(stream, depth + 1, (byte)item.Command, item.ChangedTo, item.User, item.DateTimeUtc, item.Comment);
        }

        return FragmentRecord.Append(stream, depth, (byte)item.Command, item.ChangedTo, item.User, item.DateTimeUtc, item.Comment);
    }

    /// <summary>
    /// Schreibt den rohen Block eines Sub*-Eintrags auf der Ziel-Tiefe: Je Satz wird
    /// die Basis-Tiefe per Tab-Anzahl angepasst (Pendant zu ReadEntryBlock). Der Inhalt
    /// bleibt byteidentisch, damit der Dedup-Hash beim Einlesen übereinstimmt.
    /// Liefert false bei leerem Block.
    /// </summary>
    public static bool AppendBlock(Stream stream, int depth, FragmentChange item) {
        if (item.Block is not { Length: > 0 } block) { return false; }

        var shifted = ShiftDepth(block, depth - 1);
        stream.Write(shifted, 0, shifted.Length);
        return true;
    }

    /// <summary>
    /// Liest alle tieferen Sätze ab der aktuellen Stream-Position als Eintrags-Block
    /// und schiebt sie auf Basis-Tiefe 1 zurück (Pendant zu AppendBlock). Der Stream
    /// steht danach am nächsten Satz der Nav-Ebene; geliefert werden die kanonischen
    /// Roh-Bytes des Eintrags.
    /// </summary>
    public static byte[] ReadEntryBlock(Stream stream, int navDepth) {
        using var ms = new MemoryStream();

        while (true) {
            var pos0 = stream.Position;

            if (FragmentRecord.TryReadNext(stream) is not { } inner) {
                stream.Position = pos0;
                break;
            }

            var pos1 = stream.Position;

            if (inner.Depth <= navDepth) {
                stream.Position = pos0;
                break;
            }

            var size = (int)(pos1 - pos0);
            var tmp = new byte[size];
            _ = stream.Position = pos0;
            _ = stream.Read(tmp, 0, size);
            ms.Write(tmp, 0, size);
        }

        return ShiftDepth(ms.ToArray(), -navDepth);
    }

    /// <summary>
    /// Verschiebt alle Sätze des Blocks auf eine neue Basis-Tiefe, indem je Satz
    /// Tabs ergänzt (delta &gt; 0) oder entfernt (delta &lt; 0) werden.
    /// </summary>
    private static byte[] ShiftDepth(byte[] block, int delta) {
        if (delta == 0) { return block; }

        using var ms = new MemoryStream();

        var pos = 0;

        while (pos < block.Length) {
            if (block[pos] != 0x09 || !FragmentRecord.TryParse(block, pos, out var record, out var next) || record is null) { break; }

            if (delta > 0) {
                for (var i = 0; i < delta; i++) { ms.WriteByte(0x09); }

                ms.Write(block, pos, next - pos);
            } else {
                var skip = Math.Min(-delta, record.Depth);
                ms.Write(block, pos + skip, next - pos - skip);
            }

            pos = next;
        }

        return ms.ToArray();
    }

    /// <summary>
    /// Liest eine Fragment-Datei als Änderungs-Liste: klassische Änderungen einzeln,
    /// Sub*-Blöcke (TableFragmentType 92-96) als ganze Blöcke; Steuer- und
    /// Altformat-Sätze werden übersprungen.
    /// </summary>
    public static List<FragmentChange> ReadFragments(byte[] data, string tableName, string filename) {
        List<FragmentChange> result = [];
        var invalidCount = 0;
        var rowKey = string.Empty;
        var colName = string.Empty;

        var pos = 0;

        while (pos < data.Length) {
            // Altformat-Zeilen (Text): bis zum Zeilenende überspringen
            if (data[pos] != 0x09) {
                var lineEnd = Array.IndexOf(data, (byte)'\n', pos);
                pos = lineEnd < 0 ? data.Length : lineEnd + 1;
                continue;
            }

            if (!FragmentRecord.TryParse(data, pos, out var record, out var next) || record is null) { invalidCount++; break; }

            // Sub*-Blöcke: Nav-Satz und alle tieferen Sätze als ganzer Block eintragen
            if (record.Depth == 1 && record.LeafType is >= 92 and <= 96) {
                pos = ReadFragmentBlock(data, pos, next, record, tableName, filename, result, ref invalidCount);
                continue;
            }

            pos = next;

            if (record.IsTag) { continue; }

            if (record.HasMeta) {
                if (!Enum.IsDefined(typeof(TableDataType), record.LeafType)) { invalidCount++; continue; }

                var command = (TableDataType)record.LeafType;

                // Listen-Inhalte in die CR-verbundene Domänen-Form bringen (kein Stream-Format)
                var changedTo = record.DataType == FragmentDataType.ListSet ? string.Join('\r', record.ListContent) : record.StringContent;

                result.Add(new FragmentChange(tableName, command, colName, rowKey, changedTo, record.User, record.DateTimeUtc, record.Comment, filename));

                // Kopf-Werte und Kommandos starten einen neuen Block
                if (record.Depth == 1) { rowKey = string.Empty; colName = string.Empty; }

                continue;
            }

            switch ((UndoNavigationType)record.LeafType) {
                case UndoNavigationType.RowKey:
                    rowKey = record.StringContent;
                    colName = string.Empty;
                    break;

                case UndoNavigationType.SubColumn:
                    colName = record.StringContent;
                    if (record.Depth == 1) { rowKey = string.Empty; }
                    break;

                default:
                    invalidCount++;
                    break;
            }
        }

        if (invalidCount > 0) {
            Develop.DebugPrint(ErrorType.Warning, $"Fragment-Datei: {invalidCount} ungültige Sätze verworfen: {filename.FileNameWithSuffix()}");
        }

        return result;
    }

    /// <summary>
    /// Sammelt ab dem Nav-Satz alle tieferen Sätze als Block, trägt einen Block-Eintrag
    /// in result ein und liefert die Leseposition am Block-Ende.
    /// </summary>
    private static int ReadFragmentBlock(byte[] data, int blockStart, int posAfterNavigation, FragmentRecord navigation, string tableName, string filename, List<FragmentChange> result, ref int invalidCount) {
        var pos = posAfterNavigation;
        var hasMeta = false;
        var user = string.Empty;
        var dateTimeUtc = DateTime.MinValue;

        while (pos < data.Length && data[pos] == 0x09) {
            if (!FragmentRecord.TryParse(data, pos, out var inner, out var innerNext) || inner is null) { invalidCount++; break; }

            if (inner.Depth <= navigation.Depth) { break; } // Block-Ende: äußerer Satz folgt

            if (!inner.IsTag && inner.HasMeta && !hasMeta) {
                // Meta des ersten Blatt-Satzes kennzeichnet den Block (Sortierung, Dedup);
                // die schlüssellosen Crumb-Sätze des Blocks tragen bewusst keine Meta.
                user = inner.User;
                dateTimeUtc = inner.DateTimeUtc;
                hasMeta = true;
            }

            pos = innerNext;
        }

        // Ein Block ohne einen einzigen Blatt-Satz mit Meta trägt keine verwertbaren Daten.
        if (!hasMeta) { invalidCount++; }

        result.Add(new FragmentChange(tableName, navigation.LeafType, data[blockStart..pos], user, dateTimeUtc, filename));

        return pos;
    }

    #endregion
}
