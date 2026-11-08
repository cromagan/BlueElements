// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.IO;

namespace BlueBasics.Classes;

/// <summary>
/// Ein einzelner Satz im binären Fragment-Format „Typ + Key“ (BinaryFormat.md).
/// Aufbau: Tabs (Einrücktiefe), SpeicherTyp, len1, [Empfänger-Typ], 0x20, valueId,
/// len2, Content, optional Meta (User/DateTimeUtc/Comment, durch US getrennt), 0x0D.
/// ListSet-Sätze haben kein len2: Count als lesbare Dezimalzahl, dann je Eintrag
/// Tab, 2 Längenbytes, Inhalt und 0x0D.
/// Die Pfad-Navigation wird nicht gespeichert: Jeder Satz trägt nur seine Tiefe;
/// Sub*-Sätze (Empfänger-Typ des Containers, Content = Key) eröffnen den Block des
/// Kind-Objekts eine Ebene tiefer. Ein Block endet am ersten Satz geringerer Tiefe
/// oder am Datei-Ende. Steuer-Sätze (valueId ≥ 231) haben kein Typ-Byte und dürfen
/// an jeder Tiefe stehen (Kommentar-Trenner markieren Blockgrenzen). Listen von
/// Schlüssel-Objekten beginnen mit einem ListKeyObject-Satz; die Items liegen eine
/// Ebene tiefer, zwischen ihnen je ein Kommentar-Trenner zwei Ebenen unter der
/// Liste. Kein Escaping: Eindeutigkeit kommt aus den Längenfeldern; nur die
/// Meta-Felder nutzen den US (0x1F).
/// </summary>
public sealed class FragmentRecord {

    #region Fields

    /// <summary>
    /// SpeicherTyp des Standard-Layouts; derzeit der einzige geschriebene Wert.
    /// </summary>
    public const byte StandardStorageType = 1;

    private const byte TabByte = 0x09;
    private const byte LineFeedByte = 0x0A;
    private const byte CarriageReturnByte = 0x0D;
    private const byte UnitSeparatorByte = 0x1F;
    private const byte SpaceByte = 0x20;
    private const long MaxFieldLength = 256L * 256 * 256 * 256 - 1;

    /// <summary>
    /// Obergrenze für klasseneigene Empfänger-Typen; darüber liegen Steuer-Werte.
    /// </summary>
    public const byte MaxUserTypeValue = 230;

    /// <summary>
    /// Längenbegrenzung eines List-Eintrags (2 Längenbytes).
    /// </summary>
    public const int MaxListEntryLength = 256 * 256 - 1;

    private const string LegacyEofLine = "- EOF";
    private const string LegacyUserLine = "- User ";

    /// <summary>
    /// Text der Kommentar-Trenner zwischen Objekt-Blöcken.
    /// </summary>
    public const string SeparatorComment = "---------------------------------------------------------------";

    private List<string> _listContent = [];

    #endregion

    #region Constructors

    private FragmentRecord() { }

    #endregion

    #region Properties

    /// <summary>
    /// Layout-Kennung des Satzes. Aktuell immer StandardStorageType; unbekannte Werte überspringt der Reader.
    /// </summary>
    public byte StorageType { get; private set; }

    /// <summary>
    /// Einrücktiefe des Satzes (1 = Wurzel-Ebene). Ein Objekt-Block endet am ersten
    /// Satz geringerer Tiefe bzw. am Datei-Ende.
    /// </summary>
    public int Depth { get; private set; }

    /// <summary>
    /// Empfänger-Typ (klasseneigenes Enum des Empfängers). Bei Steuer-Sätzen 0.
    /// </summary>
    public byte LeafType { get; private set; }

    /// <summary>
    /// Content-Typ des Daten-Satzes (FragmentDataType); None bei Steuer-Sätzen.
    /// </summary>
    public FragmentDataType DataType { get; private set; }

    /// <summary>
    /// Standard-Tag des Steuer-Satzes (FragmentTagType); None bei Daten-Sätzen.
    /// Klasseneigene Steuer-Werte ≥ 231 stehen hier als roher FragmentTagType-Wert.
    /// </summary>
    public FragmentTagType TagType { get; private set; }

    /// <summary>
    /// True, wenn der Satz ein Steuer-Satz auf Datei-Ebene ist (valueId ≥ 231).
    /// Solche Sätze haben einen leeren Pfad, nie Meta und werden nie an eine Klasse gereicht.
    /// </summary>
    public bool IsTag => TagType != FragmentTagType.None;

    /// <summary>
    /// Content des Satzes, roh als UTF-8 gespeichert.
    /// </summary>
    public string StringContent { get; private set; } = string.Empty;

    /// <summary>
    /// Content als Bool (Plus/Minus, Pendant zur Append-Überladung).
    /// </summary>
    public bool BoolContent {
        get {
            CheckDataType(FragmentDataType.Bool);
            return StringContent.FromPlusMinus();
        }
    }

    /// <summary>
    /// Content als Int (Pendant zur Append-Überladung).
    /// </summary>
    public int IntContent {
        get {
            CheckDataType(FragmentDataType.Int);
            return Converter.IntParse(StringContent);
        }
    }

    /// <summary>
    /// Content als Long (Pendant zur Append-Überladung).
    /// </summary>
    public long LongContent {
        get {
            CheckDataType(FragmentDataType.Int);
            return Converter.LongParse(StringContent);
        }
    }

    /// <summary>
    /// Content als Float (Pendant zur Append-Überladung mit ToString1_5).
    /// </summary>
    public float FloatContent {
        get {
            CheckDataType(FragmentDataType.Double);
            return Converter.FloatParse(StringContent);
        }
    }

    /// <summary>
    /// Content als DateTime (Pendant zur Append-Überladung).
    /// </summary>
    public DateTime DateTimeContent {
        get {
            CheckDataType(FragmentDataType.DateTime);
            return Converter.DateTimeParse(StringContent);
        }
    }

    /// <summary>
    /// Content als Color (Pendant zur Append-Überladung, Hex-Code).
    /// </summary>
    public Color ColorContent {
        get {
            CheckDataType(FragmentDataType.Color);
            return Converter.ColorParse(StringContent);
        }
    }

    /// <summary>
    /// Content als String-Liste: ListSet-Sätze strikt über Count und Längenfelder
    /// (ohne Sortierung oder Abschneiden), String-Sätze des Altformats CR-getrennt.
    /// </summary>
    public string[] ListContent {
        get {
            if (DataType == FragmentDataType.String) { return StringContent.SplitAndCutByCr(); }

            CheckDataType(FragmentDataType.ListSet);
            return [.. _listContent];
        }
    }

    /// <summary>
    /// Content als JsonNode (Pendant zur Append-Überladung).
    /// Ungültiger oder leerer Content liefert null.
    /// </summary>
    public JsonNode? JsonContent {
        get {
            CheckDataType(FragmentDataType.Json);
            try {
                return StringContent is { Length: > 0 } ? JsonNode.Parse(StringContent) : null;
            } catch {
                return null;
            }
        }
    }

    /// <summary>
    /// True, wenn der Satz Meta (User, DateTimeUtc, Comment) enthält.
    /// </summary>
    public bool HasMeta { get; private set; }

    public string User { get; private set; } = string.Empty;

    public DateTime DateTimeUtc { get; private set; }

    public string Comment { get; private set; } = string.Empty;

    #endregion

    #region Methods

    /// <summary>
    /// Stellt sicher, dass der Satz den geforderten Content-Typ hat, sonst Absturz.
    /// </summary>
    private void CheckDataType(FragmentDataType required) {
        if (DataType != required) { throw Develop.DebugError($"Content-Typ {DataType} passt nicht zum geforderten Typ {required}"); }
    }

    /// <summary>
    /// Schreibt einen Steuer-Satz (kein Empfänger-Typ, nie Meta) mit einem
    /// Standard-Tag in den Stream; die Tiefe markiert den Kontext des Satzes.
    /// Klasseneigene Steuer-Werte werden per Cast auf FragmentTagType übergeben.
    /// Liefert false bei None oder Überschreiten der Längenbegrenzung.
    /// </summary>
    public static bool Append(Stream stream, int depth, FragmentTagType tag, string content) {
        if (tag == FragmentTagType.None) { return false; }

        var contentBytes = Encoding.UTF8.GetBytes(content);

        if (contentBytes.Length > MaxFieldLength) { return false; }

        var len1 = 1 + 1 + 4 + contentBytes.Length + 1;

        if (len1 > MaxFieldLength) { return false; }

        WriteRecord(stream, depth, [SpaceByte], (byte)tag, contentBytes, null, len1);
        return true;
    }

    /// <summary>
    /// Schreibt einen Steuer-Satz mit DateTime-Inhalt (Format ToString9, UTC).
    /// </summary>
    public static bool Append(Stream stream, int depth, FragmentTagType tag, DateTime content) {
        if (tag != FragmentTagType.DateTimeUtc) { return false; }

        return Append(stream, depth, tag, content.ToString9());
    }

    /// <summary>
    /// Schreibt einen Daten-Satz mit Meta (User, DateTimeUtc, Comment) und explizitem
    /// Content-Typ. Die Meta-Sektion kennzeichnet den Satz als Undo-Eintrag.
    /// Liefert false bei US in User/Comment, Steuer-Content, ungültigem Empfänger-Typ
    /// oder Überschreiten der Längenbegrenzung.
    /// </summary>
    public static bool Append(Stream stream, int depth, byte leafType, FragmentDataType contentType, string content, string user, DateTime dateTimeUtc, string comment) {
        if ((byte)contentType > MaxUserTypeValue) { return false; }
        if (user.Contains((char)UnitSeparatorByte) || comment.Contains((char)UnitSeparatorByte)) { return false; }

        var contentBytes = Encoding.UTF8.GetBytes(content);

        return AppendData(stream, depth, leafType, contentType, contentBytes, BuildMetaBytes(user, dateTimeUtc, comment));
    }

    /// <summary>
    /// Schreibt einen ListSet-Satz mit Meta: Count (lesbar) + Einträge mit je
    /// Tab, 2 Längenbytes, Inhalt und 0x0D. Die Meta-Sektion kennzeichnet den
    /// Satz als Undo-Eintrag. Liefert false bei ungültigem Content-Typ, US in
    /// User/Comment oder Überschreiten der Längenbegrenzung.
    /// </summary>
    public static bool Append(Stream stream, int depth, byte leafType, FragmentDataType contentType, IReadOnlyList<string> content, string user, DateTime dateTimeUtc, string comment) {
        if (contentType != FragmentDataType.ListSet) { return false; }
        if (user.Contains((char)UnitSeparatorByte) || comment.Contains((char)UnitSeparatorByte)) { return false; }

        var encodedEntries = EncodeListEntries(content, out var countBytes);

        if (encodedEntries is null) { return false; }

        return AppendListData(stream, depth, leafType, countBytes, encodedEntries, BuildMetaBytes(user, dateTimeUtc, comment));
    }

    /// <summary>
    /// Schreibt einen Daten-Satz mit Meta (User, DateTimeUtc, Comment). Die Meta-Sektion
    /// kennzeichnet den Satz als Undo-Eintrag. Liefert false bei US in User/Comment,
    /// ungültigem Empfänger-Typ oder Überschreiten der Längenbegrenzung.
    /// </summary>
    public static bool Append(Stream stream, int depth, byte leafType, string content, string user, DateTime dateTimeUtc, string comment) {
        if (user.Contains((char)UnitSeparatorByte) || comment.Contains((char)UnitSeparatorByte)) { return false; }

        var contentBytes = Encoding.UTF8.GetBytes(content);

        return AppendData(stream, depth, leafType, FragmentDataType.String, contentBytes, BuildMetaBytes(user, dateTimeUtc, comment));
    }

    /// <summary>
    /// Schreibt einen String-Wert als Daten-Satz.
    /// </summary>
    public static bool Append(Stream stream, int depth, byte leafType, string content) {
        var contentBytes = Encoding.UTF8.GetBytes(content);

        return AppendData(stream, depth, leafType, FragmentDataType.String, contentBytes, null);
    }

    /// <summary>
    /// Schreibt einen Bool-Wert als Daten-Satz (FragmentDataType.Bool, Content "+/-").
    /// </summary>
    public static bool Append(Stream stream, int depth, byte leafType, bool content) => Append(stream, depth, leafType, content.ToPlusMinus());

    /// <summary>
    /// Schreibt einen Int-Wert als Daten-Satz (FragmentDataType.Int, Content ToString1).
    /// </summary>
    public static bool Append(Stream stream, int depth, byte leafType, int content) {
        var contentBytes = Encoding.UTF8.GetBytes(content.ToString1());

        return AppendData(stream, depth, leafType, FragmentDataType.Int, contentBytes, null);
    }

    /// <summary>
    /// Schreibt einen Long-Wert als Daten-Satz (FragmentDataType.Int, Content ToString1).
    /// </summary>
    public static bool Append(Stream stream, int depth, byte leafType, long content) {
        var contentBytes = Encoding.UTF8.GetBytes(content.ToString1());

        return AppendData(stream, depth, leafType, FragmentDataType.Int, contentBytes, null);
    }

    /// <summary>
    /// Schreibt einen DateTime-Wert als Daten-Satz (FragmentDataType.DateTime, Format ToString5).
    /// </summary>
    public static bool Append(Stream stream, int depth, byte leafType, DateTime content) {
        var contentBytes = Encoding.UTF8.GetBytes(content.ToString5());

        return AppendData(stream, depth, leafType, FragmentDataType.DateTime, contentBytes, null);
    }

    /// <summary>
    /// Schreibt einen Color-Wert als Daten-Satz (FragmentDataType.Color, Content ToHtmlCode).
    /// </summary>
    public static bool Append(Stream stream, int depth, byte leafType, Color content) {
        var contentBytes = Encoding.UTF8.GetBytes(content.ToHtmlCode());

        return AppendData(stream, depth, leafType, FragmentDataType.Color, contentBytes, null);
    }

    /// <summary>
    /// Schreibt einen Float-Wert als Daten-Satz (FragmentDataType.Double, Format ToString1_5).
    /// </summary>
    public static bool Append(Stream stream, int depth, byte leafType, float content) {
        var contentBytes = Encoding.UTF8.GetBytes(content.ToString1_5());

        return AppendData(stream, depth, leafType, FragmentDataType.Double, contentBytes, null);
    }

    /// <summary>
    /// Schreibt eine String-Liste als ListSet-Satz: Count (lesbar) + Einträge
    /// mit je Tab, 2 Längenbytes, Inhalt und 0x0D. Kein Join, kein Sort, kein Trim.
    /// </summary>
    public static bool Append(Stream stream, int depth, byte leafType, IReadOnlyList<string> content) {
        var encodedEntries = EncodeListEntries(content, out var countBytes);

        if (encodedEntries is null) { return false; }

        return AppendListData(stream, depth, leafType, countBytes, encodedEntries, null);
    }

    /// <summary>
    /// Schreibt einen JsonNode als Daten-Satz (FragmentDataType.Json, Content ToJsonString).
    /// </summary>
    public static bool Append(Stream stream, int depth, byte leafType, JsonNode content) {
        var contentBytes = Encoding.UTF8.GetBytes(content.ToJsonString());

        return AppendData(stream, depth, leafType, FragmentDataType.Json, contentBytes, null);
    }

    #region Append mit Enum als Empfänger-Typ

    /// <summary>
    /// Schreibt einen String-Wert als Daten-Satz; der Empfänger-Typ wird als
    /// klasseneigenes Enum übergeben.
    /// </summary>
    public static bool Append(Stream stream, int depth, Enum leafType, string content) => Append(stream, depth, EnumByte(leafType), content);

    /// <summary>
    /// Schreibt einen Bool-Wert als Daten-Satz; der Empfänger-Typ wird als
    /// klasseneigenes Enum übergeben.
    /// </summary>
    public static bool Append(Stream stream, int depth, Enum leafType, bool content) => Append(stream, depth, EnumByte(leafType), content);

    /// <summary>
    /// Schreibt einen Int-Wert als Daten-Satz; der Empfänger-Typ wird als
    /// klasseneigenes Enum übergeben.
    /// </summary>
    public static bool Append(Stream stream, int depth, Enum leafType, int content) => Append(stream, depth, EnumByte(leafType), content);

    /// <summary>
    /// Schreibt einen Long-Wert als Daten-Satz; der Empfänger-Typ wird als
    /// klasseneigenes Enum übergeben.
    /// </summary>
    public static bool Append(Stream stream, int depth, Enum leafType, long content) => Append(stream, depth, EnumByte(leafType), content);

    /// <summary>
    /// Schreibt einen DateTime-Wert als Daten-Satz; der Empfänger-Typ wird als
    /// klasseneigenes Enum übergeben.
    /// </summary>
    public static bool Append(Stream stream, int depth, Enum leafType, DateTime content) => Append(stream, depth, EnumByte(leafType), content);

    /// <summary>
    /// Schreibt einen Color-Wert als Daten-Satz; der Empfänger-Typ wird als
    /// klasseneigenes Enum übergeben.
    /// </summary>
    public static bool Append(Stream stream, int depth, Enum leafType, Color content) => Append(stream, depth, EnumByte(leafType), content);

    /// <summary>
    /// Schreibt einen Float-Wert als Daten-Satz; der Empfänger-Typ wird als
    /// klasseneigenes Enum übergeben.
    /// </summary>
    public static bool Append(Stream stream, int depth, Enum leafType, float content) => Append(stream, depth, EnumByte(leafType), content);

    /// <summary>
    /// Schreibt eine String-Liste als ListSet-Satz; der Empfänger-Typ wird als
    /// klasseneigenes Enum übergeben.
    /// </summary>
    public static bool Append(Stream stream, int depth, Enum leafType, IReadOnlyList<string> content) => Append(stream, depth, EnumByte(leafType), content);

    /// <summary>
    /// Schreibt einen Sub*-Satz (Content = KeyName des Objekts) und darunter dessen
    /// Block eine Ebene tiefer; der Empfänger-Typ wird als klasseneigenes Enum
    /// übergeben. Der Key kommt von IHasKeyName.KeyName; ohne Key wird ein leerer
    /// Key geschrieben.
    /// </summary>
    public static bool Append(Stream stream, int depth, Enum leafType, IFragmentStoreable content) => Append(stream, depth, leafType, (content as IHasKeyName)?.KeyName ?? string.Empty, content);

    /// <summary>
    /// Schreibt einen Sub*-Satz mit explizitem Key und darunter den Block des
    /// Objekts eine Ebene tiefer (für Objekte, deren Key nicht der eigene
    /// KeyName ist).
    /// </summary>
    public static bool Append(Stream stream, int depth, Enum leafType, string content, IFragmentStoreable subObject) => Append(stream, depth, leafType, content) && subObject.Serialize(stream, depth + 1);

    /// <summary>
    /// Schreibt einen ListKeyObject-Satz (die Liste beginnt) und darunter die Items
    /// eine Ebene tiefer; zwischen den Items steht je ein Kommentar-Trenner zwei
    /// Ebenen unter der Liste, damit DeserializeList die Blockgrenzen erkennt. Der
    /// Empfänger-Typ wird als klasseneigenes Enum übergeben.
    /// </summary>
    public static bool Append(Stream stream, int depth, Enum leafType, IReadOnlyList<IFragmentStoreable> content) {
        var allok = AppendData(stream, depth, EnumByte(leafType), FragmentDataType.ListKeyObject, [], null);

        for (var i = 0; i < content.Count; i++) {
            allok &= content[i].Serialize(stream, depth + 1);

            if (i < content.Count - 1) {
                allok &= Append(stream, depth + 2, FragmentTagType.Comment, SeparatorComment);
            }
        }

        return allok;
    }

    /// <summary>
    /// Schreibt einen JsonNode als Daten-Satz; der Empfänger-Typ wird als
    /// klasseneigenes Enum übergeben.
    /// </summary>
    public static bool Append(Stream stream, int depth, Enum leafType, JsonNode content) => Append(stream, depth, EnumByte(leafType), content);

    /// <summary>
    /// Schreibt einen Daten-Satz mit Meta; der Empfänger-Typ wird als
    /// klasseneigenes Enum übergeben.
    /// </summary>
    public static bool Append(Stream stream, int depth, Enum leafType, string content, string user, DateTime dateTimeUtc, string comment) => Append(stream, depth, EnumByte(leafType), content, user, dateTimeUtc, comment);

    #endregion

    /// <summary>
    /// Kodiert die List-Einträge: Count-Zeile (lesbare Dezimalzahl) und je Eintrag
    /// Tab, 2 Längenbytes, UTF-8-Inhalt und 0x0D. Liefert null bei zu langen Einträgen.
    /// </summary>
    internal static byte[][]? EncodeListEntries(IReadOnlyList<string> content, out byte[] countBytes) {
        countBytes = Encoding.UTF8.GetBytes(content.Count.ToString1());

        var encodedEntries = new byte[content.Count][];

        for (var i = 0; i < content.Count; i++) {
            var entryBytes = Encoding.UTF8.GetBytes(content[i]);

            if (entryBytes.Length > MaxListEntryLength) { return null; }

            encodedEntries[i] = entryBytes;
        }

        return encodedEntries;
    }

    /// <summary>
    /// Liefert das Byte eines klasseneigenen Empfänger-Typ-Enums, damit Aufrufer
    /// keine (byte)-Casts brauchen.
    /// </summary>
    internal static byte EnumByte(Enum leafType) => Convert.ToByte(leafType, CultureInfo.InvariantCulture);

    /// <summary>
    /// Kern des ListSet-Satz-Schreibens: prüft Tiefe und Empfänger-Typ, berechnet len1
    /// (ohne len2-Feld, inkl. abschließendem 0x0D) und schreibt Count-Zeile, Einträge
    /// und optionale Meta-Sektion.
    /// </summary>
    private static bool AppendListData(Stream stream, int depth, byte leafType, byte[] countBytes, byte[][] encodedEntries, byte[]? metaBytes) {
        if (depth < 1 || leafType > MaxUserTypeValue) { return false; }

        var payloadLength = countBytes.Length + 1;

        foreach (var entryBytes in encodedEntries) {
            payloadLength += 1 + 2 + entryBytes.Length + 1;
        }

        // Empfänger-Typ + 0x20-Trenner + DataType + Payload + Meta + 0x0D
        var len1 = 2 + 1 + payloadLength + (metaBytes?.Length ?? 0) + 1;

        if (len1 > MaxFieldLength) { return false; }

        WriteListRecord(stream, depth, [leafType, SpaceByte], (byte)FragmentDataType.ListSet, countBytes, encodedEntries, metaBytes, len1);
        return true;
    }

    /// <summary>
    /// Kern des Daten-Satz-Schreibens: prüft Tiefe und Empfänger-Typ, berechnet len1
    /// (inkl. abschließendem 0x0D) und schreibt den Satz mit optionaler Meta-Sektion.
    /// </summary>
    private static bool AppendData(Stream stream, int depth, byte leafType, FragmentDataType dataType, byte[] contentBytes, byte[]? metaBytes) {
        if (depth < 1 || leafType > MaxUserTypeValue) { return false; }

        // Empfänger-Typ + 0x20-Trenner + valueId + len2-Feld + Content + Meta + 0x0D
        var len1 = 2 + 1 + 4 + contentBytes.Length + (metaBytes?.Length ?? 0) + 1;

        if (len1 > MaxFieldLength) { return false; }

        WriteRecord(stream, depth, [leafType, SpaceByte], (byte)dataType, contentBytes, metaBytes, len1);
        return true;
    }

    /// <summary>
    /// Liest den nächsten Satz aus dem Stream (bei jeder Einrücktiefe); null bei
    /// Datei-Ende oder Lesefehler.
    /// </summary>
    public static FragmentRecord? TryReadNext(Stream stream) {
        long start = stream.Position;

        if (!TryReadRawRecord(stream, start, out var buffer)) {
            stream.Position = start;
            return null;
        }

        if (!TryParse(buffer, 0, out var parsed, out _)) {
            stream.Position = start;
            return null;
        }

        return parsed;
    }

    /// <summary>
    /// Liest den nächsten Satz und liefert ihn nur, wenn er genau die geforderte
    /// Tiefe hat — das Block-Lese-Prinzip des tiefenbasierten Formats: Stimmt die
    /// Tiefe nicht mehr (geringerer oder tieferer Satz), bleibt der Stream am
    /// Satzanfang stehen und es wird null geliefert — das Zeichen, dass keine
    /// Daten mehr da sind. Datei-Ende und unlesbare Sätze liefern ebenfalls null.
    /// </summary>
    public static FragmentRecord? TryReadNext(Stream stream, int depth) {
        long start = stream.Position;

        if (!TryReadRawRecord(stream, start, out var buffer)) { return null; }

        if (!TryParse(buffer, 0, out var parsed, out _) || parsed is null || parsed.Depth != depth) {
            stream.Position = start;
            return null;
        }

        // Satz der geforderten Tiefe gelesen: der Block geht weiter.
        return parsed;
    }

    /// <summary>
    /// Liest einen kompletten Roh-Satz (Tabs, SpeicherTyp, len1, Rest) aus dem Stream.
    /// Liefert false bei Datei-Ende vor dem Satz oder abgeschnittener Struktur.
    /// </summary>
    private static bool TryReadRawRecord(Stream stream, long start, out byte[] buffer) {
        buffer = [];
        var current = stream.ReadByte();

        if (current < 0) { return false; }

        var tabs = 0;
        while (current == TabByte) {
            tabs++;
            current = stream.ReadByte();
            if (current < 0) { stream.Position = start; return false; }
        }

        var header = new byte[5] { (byte)current, 0, 0, 0, 0 };
        if (stream.Read(header, 1, 4) != 4) { stream.Position = start; return false; }

        var len1 = header[1] | (header[2] << 8) | (header[3] << 16) | (header[4] << 24);
        var rest = new byte[Math.Max(len1, 0)];
        if (len1 < 1 || stream.Read(rest, 0, len1) != len1) { stream.Position = start; return false; }

        buffer = new byte[tabs + header.Length + rest.Length];
        for (var i = 0; i < tabs; i++) { buffer[i] = TabByte; }
        Array.Copy(header, 0, buffer, tabs, header.Length);
        Array.Copy(rest, 0, buffer, tabs + header.Length, rest.Length);

        return true;
    }

    /// <summary>
    /// Parst einen Satz ab startPosition (muss ein Tab sein): Tabs zählen, SpeicherTyp
    /// lesen, len1 lesen, Empfänger-Typ bzw. Steuer-Kennung lesen, len2/Content/Meta
    /// validieren. Bei strukturiert erkennbarem Satzende wird nextPosition über das
    /// Satzende gesetzt, sonst auf das Puffer-Ende. Bei Verletzung der Format-Regeln
    /// liefert die Methode false und record bleibt null.
    /// </summary>
    public static bool TryParse(byte[] buffer, int startPosition, out FragmentRecord? record, out int nextPosition) {
        record = null;
        nextPosition = startPosition;
        var pos = startPosition;

        var tabs = 0;
        while (pos < buffer.Length && buffer[pos] == TabByte) {
            tabs++;
            pos++;
        }

        if (pos >= buffer.Length) { nextPosition = buffer.Length; return false; }

        var storageType = buffer[pos++];

        if (pos + 4 > buffer.Length) { nextPosition = buffer.Length; return false; }

        var len1 = buffer[pos] | (buffer[pos + 1] << 8) | (buffer[pos + 2] << 16) | (buffer[pos + 3] << 24);
        pos += 4;

        var recordEnd = pos + len1;

        if (len1 < 1 || recordEnd > buffer.Length) {
            nextPosition = buffer.Length;
            return false;
        }

        nextPosition = recordEnd;

        // Empfänger-Typ bzw. Steuer-Kennung: Nach len1 folgt entweder direkt das
        // 0x20 (Steuer-Satz, kein Typ-Byte) oder genau ein Typ-Byte ≤ 230, dann 0x20.
        // Blatt-Typ 32 (0x20, ColumnArrangement) kollidiert mit dem Trenner: Die
        // Bytesfolge 0x20 0x20 ist eindeutig Blatt 32 + Trenner — ein echter
        // Steuer-Satz hat als nächstes Byte einen Steuer-Wert (≥ 231), nie 0x20.
        byte leafType;
        var hasLeaf = false;

        if (buffer[pos] == SpaceByte) {
            if (pos + 1 < recordEnd && buffer[pos + 1] == SpaceByte) {
                leafType = SpaceByte;
                hasLeaf = true;
                pos += 2;
            } else {
                leafType = 0;
                pos++;
            }
        } else {
            leafType = buffer[pos++];

            if (leafType > MaxUserTypeValue) { return false; }
            if (pos >= recordEnd || buffer[pos] != SpaceByte) { return false; }

            pos++;
            hasLeaf = true;
        }

        // Jeder Satz braucht mindestens 1 Tab; Steuer-Sätze haben nie einen Empfänger-Typ.
        if (tabs < 1) { return false; }

        if (pos >= recordEnd) { return false; }

        var valueId = buffer[pos];
        var isTag = valueId > MaxUserTypeValue;

        if (!hasLeaf && !isTag) { return false; }

        var dataType = isTag ? FragmentDataType.None : (FragmentDataType)valueId;

        pos++;

        string content;
        List<string> listContent = [];

        if (dataType == FragmentDataType.ListSet) {
            // ListSet-Satz: kein len2 — Count (lesbar) + Einträge mit je 2 Längenbytes
            if (!TryParseListPayload(buffer, recordEnd, ref pos, out listContent)) { return false; }

            content = string.Empty;
        } else {
            if (pos + 4 > recordEnd) { return false; }

            var len2 = buffer[pos] | (buffer[pos + 1] << 8) | (buffer[pos + 2] << 16) | (buffer[pos + 3] << 24);
            pos += 4;

            if (pos + len2 + 1 > recordEnd) { return false; }

            content = Encoding.UTF8.GetString(buffer, pos, len2);
            pos += len2;
        }

        // Rest prüfen (Längen-Arithmetik, kein Marker): Rest = 1 → nur das 0x0D, keine Meta.
        var metaLength = recordEnd - 1 - pos;

        if (metaLength < 0) { return false; }

        var result = new FragmentRecord {
            StorageType = storageType,
            Depth = tabs,
            LeafType = leafType,
            DataType = dataType,
            TagType = isTag ? (FragmentTagType)valueId : FragmentTagType.None,
            StringContent = content
        };

        result._listContent = listContent;

        if (metaLength > 0) {
            if (buffer[recordEnd - 1] != CarriageReturnByte) { return false; }
            if (!result.ParseMeta(buffer, pos, metaLength)) { return false; }
        }

        record = result;
        return true;
    }

    /// <summary>
    /// Prüft byteweise, ob die Datei sauber abgeschlossen ist: Der letzte Satz entscheidet —
    /// 255 (EOF) bzw. beim Altformat die Textzeile „- EOF“ bedeuten abgeschlossen,
    /// alles andere (insbesondere 254 ContinueEOF) bedeutet in Bearbeitung.
    /// Eine unlesbare oder abgeschnittene Datei gilt als nicht abgeschlossen.
    /// </summary>
    public static bool EndsOnEof(Stream stream) {
        if (stream.Length == 0) { return false; }

        var data = ReadAllBytes(stream);
        var pos = 0;
        var ended = false;

        while (pos < data.Length) {
            if (data[pos] == TabByte) {
                if (!TryParse(data, pos, out var record, out pos) || record is null) { return false; }

                ended = record.TagType == FragmentTagType.Eof;
            } else {
                var nl = Array.IndexOf(data, LineFeedByte, pos);
                var end = nl < 0 ? data.Length : nl;
                var line = Encoding.UTF8.GetString(data, pos, end - pos).TrimEnd('\r');
                pos = nl < 0 ? data.Length : nl + 1;

                if (line.Length > 0) { ended = line == LegacyEofLine; }
            }
        }

        return ended;
    }

    /// <summary>
    /// Liefert den Benutzer, der die Datei erstellt hat: den ersten 233-Satz (User)
    /// bzw. beim Altformat den Inhalt der ersten „- User “-Zeile — oder null.
    /// </summary>
    public static string? HeadUser(Stream stream) {
        if (stream.Length == 0) { return null; }

        var data = ReadAllBytes(stream);
        var pos = 0;

        while (pos < data.Length) {
            if (data[pos] == TabByte) {
                if (!TryParse(data, pos, out var record, out pos) || record is null) { return null; }

                if (record.TagType == FragmentTagType.User) { return record.StringContent; }
            } else {
                var nl = Array.IndexOf(data, LineFeedByte, pos);
                var end = nl < 0 ? data.Length : nl;
                var line = Encoding.UTF8.GetString(data, pos, end - pos).TrimEnd('\r');
                pos = nl < 0 ? data.Length : nl + 1;

                if (line.StartsWith(LegacyUserLine, StringComparison.Ordinal)) { return line[LegacyUserLine.Length..].Trim(); }
            }
        }

        return null;
    }

    /// <summary>
    /// Baut die Meta-Bytes (User, DateTimeUtc als ToString9 UTC und Comment, durch US getrennt).
    /// </summary>
    private static byte[] BuildMetaBytes(string user, DateTime dateTimeUtc, string comment) {
        var userBytes = Encoding.UTF8.GetBytes(user);
        var dateBytes = Encoding.UTF8.GetBytes(dateTimeUtc.ToString9());
        var commentBytes = Encoding.UTF8.GetBytes(comment);

        var result = new byte[userBytes.Length + 1 + dateBytes.Length + 1 + commentBytes.Length];
        var pos = 0;

        Array.Copy(userBytes, 0, result, pos, userBytes.Length);
        pos += userBytes.Length;
        result[pos++] = UnitSeparatorByte;
        Array.Copy(dateBytes, 0, result, pos, dateBytes.Length);
        pos += dateBytes.Length;
        result[pos++] = UnitSeparatorByte;
        Array.Copy(commentBytes, 0, result, pos, commentBytes.Length);

        return result;
    }

    /// <summary>
    /// Parst die ListSet-Payload: Count als lesbare Dezimalzahl bis 0x0D, dann je
    /// Eintrag Tab, 2 Längenbytes, UTF-8-Inhalt und 0x0D — strikt nach Count, ohne
    /// Sortierung oder Abschneiden. Liefert false bei verstimmter Struktur.
    /// </summary>
    private static bool TryParseListPayload(byte[] buffer, int recordEnd, ref int pos, out List<string> entries) {
        entries = [];

        var digitsStart = pos;

        while (pos < recordEnd && buffer[pos] is >= (byte)'0' and <= (byte)'9') { pos++; }

        if (pos == digitsStart || pos >= recordEnd || buffer[pos] != CarriageReturnByte) { return false; }

        var count = (int)Math.Min(Converter.LongParse(Encoding.UTF8.GetString(buffer, digitsStart, pos - digitsStart)), int.MaxValue);
        pos++;

        // Jeder Eintrag braucht mindestens Tab + 2 Längenbytes + 0x0D; absurd große
        // Counts werden damit abgelehnt, bevor Speicher reserviert wird.
        if (count > (recordEnd - pos) / 4) { return false; }

        for (var i = 0; i < count; i++) {
            if (pos + 4 > recordEnd || buffer[pos] != TabByte) { return false; }

            pos++;

            var len = buffer[pos] | (buffer[pos + 1] << 8);
            pos += 2;

            if (pos + len + 1 > recordEnd) { return false; }

            entries.Add(Encoding.UTF8.GetString(buffer, pos, len));
            pos += len;

            if (buffer[pos] != CarriageReturnByte) { return false; }

            pos++;
        }

        return true;
    }

    /// <summary>
    /// Parst die Meta-Sektion (genau 3 Felder User, DateTimeUtc, Comment, durch US getrennt).
    /// </summary>
    private bool ParseMeta(byte[] buffer, int start, int length) {
        var first = Array.IndexOf(buffer, UnitSeparatorByte, start, length);
        if (first < 0) { return false; }

        var second = Array.IndexOf(buffer, UnitSeparatorByte, first + 1, start + length - first - 1);
        if (second < 0) { return false; }

        User = Encoding.UTF8.GetString(buffer, start, first - start);
        DateTimeUtc = DateTimeParse(Encoding.UTF8.GetString(buffer, first + 1, second - first - 1));
        Comment = Encoding.UTF8.GetString(buffer, second + 1, start + length - second - 1);
        HasMeta = true;

        return true;
    }

    private static void WriteRecord(Stream stream, int tabCount, byte[] leafBytes, byte dataType, byte[] contentBytes, byte[]? metaBytes, int len1) {
        var record = new byte[tabCount + 1 + 4 + leafBytes.Length + 1 + 4 + contentBytes.Length + (metaBytes?.Length ?? 0) + 1];
        var pos = 0;

        for (var i = 0; i < tabCount; i++) { record[pos++] = TabByte; }

        record[pos++] = StandardStorageType;
        record[pos++] = (byte)(len1 & 0xFF);
        record[pos++] = (byte)((len1 >> 8) & 0xFF);
        record[pos++] = (byte)((len1 >> 16) & 0xFF);
        record[pos++] = (byte)((len1 >> 24) & 0xFF);
        Array.Copy(leafBytes, 0, record, pos, leafBytes.Length);
        pos += leafBytes.Length;
        record[pos++] = dataType;
        record[pos++] = (byte)(contentBytes.Length & 0xFF);
        record[pos++] = (byte)((contentBytes.Length >> 8) & 0xFF);
        record[pos++] = (byte)((contentBytes.Length >> 16) & 0xFF);
        record[pos++] = (byte)((contentBytes.Length >> 24) & 0xFF);
        Array.Copy(contentBytes, 0, record, pos, contentBytes.Length);
        pos += contentBytes.Length;

        if (metaBytes is not null) {
            Array.Copy(metaBytes, 0, record, pos, metaBytes.Length);
            pos += metaBytes.Length;
        }

        record[pos] = CarriageReturnByte;

        stream.Write(record);
    }

    /// <summary>
    /// Schreibt einen ListSet-Satz: Tabs, SpeicherTyp, len1, Empfänger-Typ, 0x20,
    /// DataType, Count-Zeile, Einträge (je Tab, 2 Längenbytes, Inhalt, 0x0D),
    /// optionale Meta-Sektion und abschließendem 0x0D.
    /// </summary>
    private static void WriteListRecord(Stream stream, int tabCount, byte[] leafBytes, byte dataType, byte[] countBytes, byte[][] encodedEntries, byte[]? metaBytes, int len1) {
        var total = tabCount + 1 + 4 + leafBytes.Length + 1 + countBytes.Length + 1 + (metaBytes?.Length ?? 0) + 1;

        foreach (var entryBytes in encodedEntries) { total += 1 + 2 + entryBytes.Length + 1; }

        var record = new byte[total];
        var pos = 0;

        for (var i = 0; i < tabCount; i++) { record[pos++] = TabByte; }

        record[pos++] = StandardStorageType;
        record[pos++] = (byte)(len1 & 0xFF);
        record[pos++] = (byte)((len1 >> 8) & 0xFF);
        record[pos++] = (byte)((len1 >> 16) & 0xFF);
        record[pos++] = (byte)((len1 >> 24) & 0xFF);
        Array.Copy(leafBytes, 0, record, pos, leafBytes.Length);
        pos += leafBytes.Length;
        record[pos++] = dataType;
        Array.Copy(countBytes, 0, record, pos, countBytes.Length);
        pos += countBytes.Length;
        record[pos++] = CarriageReturnByte;

        foreach (var entryBytes in encodedEntries) {
            record[pos++] = TabByte;
            record[pos++] = (byte)(entryBytes.Length & 0xFF);
            record[pos++] = (byte)((entryBytes.Length >> 8) & 0xFF);
            Array.Copy(entryBytes, 0, record, pos, entryBytes.Length);
            pos += entryBytes.Length;
            record[pos++] = CarriageReturnByte;
        }

        if (metaBytes is not null) {
            Array.Copy(metaBytes, 0, record, pos, metaBytes.Length);
            pos += metaBytes.Length;
        }

        record[pos] = CarriageReturnByte;

        stream.Write(record);
    }

    private static byte[] ReadAllBytes(Stream stream) {
        using var target = new MemoryStream();
        stream.CopyTo(target);
        return target.ToArray();
    }

    #endregion
}
