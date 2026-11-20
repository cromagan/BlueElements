// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.IO;

namespace BlueBasics.Classes;

/// <summary>
/// Ein einzelner Satz im binären Fragment-Format „Typ + Key“.
/// Aufbau: SpeicherTyp, Tiefe (1 Byte), len1, Empfänger-Typ bzw. Tag, Content-Typ, len2, Content,
/// optional Meta (User, DateTimeUtc als 8 Tick-Bytes, Comment; durch US getrennt).
/// Daten-Sätze tragen den Empfänger-Typ direkt als valueId; geschützte Sätze (valueId ≥ 200)
/// tragen ihren Tag als valueId, gefolgt vom Content-Typ.
/// ListSet-Sätze tragen im Content je Eintrag 2 Längenbytes und den Inhalt.
/// Werte-Typen liegen kompakt vor: Int 4 Bytes, Float 4 Bytes, Double 8 Bytes, Long 8 Bytes,
/// DateTime 8 Tick-Bytes, Color 4 ARGB-Bytes, Decimal 16 Bytes, UInt 4 Bytes (Little-Endian); Text-Typen
/// nutzen UTF-8 (String, Json) bzw. Latin1 (Bool).
/// Die Pfad-Navigation wird nicht gespeichert: Jeder Satz trägt nur seine Tiefe;
/// Sub*-Sätze (Empfänger-Typ des Containers, Content = Key) eröffnen den Block des
/// Kind-Objekts eine Ebene tiefer. Ein Block endet am ersten Satz geringerer Tiefe
/// oder am Datei-Ende. Listen von Schlüssel-Objekten beginnen mit einem
/// ListKeyObject-Satz; je Item folgt ein ItemAdd-Kopf („KeyName|TypeName“) eine
/// Ebene tiefer und der Item-Block eine weitere, bis ein Satz auf die Listen-Ebene
/// zurückfällt. Kein Escaping: Eindeutigkeit kommt aus den Längenfeldern; nur die
/// Meta-Felder nutzen den US (0x1F).
/// </summary>
public sealed class FragmentRecord {

    #region Fields

    /// <summary>
    /// SpeicherTyp des Standard-Layouts; derzeit der einzige geschriebene Wert.
    /// </summary>
    public const byte StandardStorageType = 1;

    private const byte UnitSeparatorByte = 0x1F;
    private const long MaxFieldLength = 256L * 256 * 256 * 256 - 1;

    /// <summary>
    /// Obergrenze für klasseneigene Empfänger-Typen; darüber beginnt der geschützte
    /// Bereich des FragmentTagType.
    /// </summary>
    public const byte MaxUserTypeValue = 199;

    /// <summary>
    /// Längenbegrenzung eines List-Eintrags (2 Längenbytes).
    /// </summary>
    public const int MaxListEntryLength = 256 * 256 - 1;

    #endregion

    #region Constructors

    private FragmentRecord() { }

    /// <summary>
    /// Erzeugt im Speicher einen Satz auf Basis-Tiefe 0; der Content kommt fertig kodiert
    /// herein (Kodier-Regeln der Append-Überladungen, bei ListSet die komplette Payload).
    /// Mit Meta-Stempel (User/Zeit/Kommentar) gilt der Satz als Undo-Eintrag; ohne Meta
    /// (DateTimeUtc = MinValue) als Navigationssatz.
    /// </summary>
    public static FragmentRecord Create(byte leafType, FragmentDataType dataType, byte[] content, string user, DateTime dateTimeUtc, string comment) =>
        new FragmentRecord {
            StorageType = StandardStorageType,
            Depth = 0,
            LeafType = leafType,
            DataType = dataType,
            TagType = FragmentTagType.None,
            ByteContent = content,
            User = user,
            DateTimeUtc = dateTimeUtc,
            Comment = comment,
            HasMeta = dateTimeUtc != DateTime.MinValue
        };

    /// <summary>
    /// Erzeugt im Speicher einen Navigationssatz (Navigation einer Stufe, ohne Meta) auf Basis-Tiefe 0.
    /// </summary>
    public static FragmentRecord CreateNavigation(Enum leafType, string key) =>
        Create(EnumByte(leafType), FragmentDataType.String, Encoding.UTF8.GetBytes(key), string.Empty, DateTime.MinValue, string.Empty);

    #endregion

    #region Properties

    /// <summary>
    /// Layout-Kennung des Satzes. Aktuell immer StandardStorageType; unbekannte Werte überspringt der Reader.
    /// </summary>
    public byte StorageType { get; private set; }

    /// <summary>
    /// Einrücktiefe des Satzes (0 = Wurzel-Ebene), gespeichert als Byte.
    /// Ein Objekt-Block endet am ersten Satz geringerer Tiefe bzw. am Datei-Ende.
    /// </summary>
    public int Depth { get; private set; }

    /// <summary>
    /// Empfänger-Typ (klasseneigenes Enum des Empfängers) bzw. bei geschützten
    /// Sätzen (TagType ≥ ItemPath) 0.
    /// </summary>
    public byte LeafType { get; private set; }

    /// <summary>
    /// Content-Typ des Satzes (FragmentDataType) — auch bei geschützten Sätzen.
    /// </summary>
    public FragmentDataType DataType { get; private set; }

    /// <summary>
    /// Tag des Satzes (FragmentTagType) bei valueId ≥ 200; None bei klasseneigenen Daten-Sätzen.
    /// 200–229 sind standardisierte Empfänger-Tags und werden an die Empfänger-Klasse
    /// gereicht; ab 231 liegen die Steuer-Sätze der Datei-Ebene.
    /// </summary>
    public FragmentTagType TagType { get; private set; }

    /// <summary>
    /// True, wenn der Satz im geschützten Bereich liegt (valueId ≥ 200) und damit
    /// per FragmentTagType geroutet wird statt über den klasseneigenen Empfänger-Typ.
    /// </summary>
    public bool IsTag => TagType != FragmentTagType.None;

    /// <summary>
    /// True, wenn der Satz ein Steuer-Satz der Datei-Ebene ist (TagType ≥ Comment).
    /// Solche Sätze haben einen leeren Pfad, nie Meta und werden nie an eine Klasse gereicht.
    /// </summary>
    public bool IsFileTag => TagType >= FragmentTagType.Comment;

    /// <summary>
    /// Das Routing-Byte des Satzes, wie es beim Lesen an der valueId-Position stand:
    /// im geschützten Bereich der FragmentTagType-Wert, sonst der klasseneigene Empfänger-Typ.
    /// </summary>
    public byte ValueId => IsTag ? (byte)TagType : LeafType;

    /// <summary>
    /// Content roh als Byte-Folge; alle Ausgaben reichen den passenden Konverter nach.
    /// </summary>
    public byte[] ByteContent { get; private set; } = [];

    /// <summary>
    /// Content als UTF-8-Text; nur bei FragmentDataType.String, sonst Absturz.
    /// </summary>
    public string StringContent {
        get {
            CheckDataType(FragmentDataType.String);
            return Encoding.UTF8.GetString(ByteContent);
        }
    }

    /// <summary>
    /// KeyName-Anteil des Contents „KeyName|TypeName“ (Text vor dem |); nur bei
    /// FragmentDataType.KeyNameType, sonst Absturz. Ohne Trenner der ganze Text.
    /// </summary>
    public string KeyNameContent {
        get {
            CheckDataType(FragmentDataType.KeyNameType);
            var text = Encoding.UTF8.GetString(ByteContent);
            var pos = text.IndexOf('|');

            return pos < 0 ? text : text[..pos];
        }
    }

    /// <summary>
    /// Typ-Anteil des Contents „KeyName|TypeName“ (Text nach dem |); nur bei
    /// FragmentDataType.KeyNameType, sonst Absturz. Ohne Trenner leer.
    /// </summary>
    public string TypeContent {
        get {
            CheckDataType(FragmentDataType.KeyNameType);
            var text = Encoding.UTF8.GetString(ByteContent);
            var pos = text.IndexOf('|');

            return pos < 0 ? string.Empty : text[(pos + 1)..];
        }
    }

    /// <summary>
    /// Content als Bool (Plus/Minus, Pendant zur Append-Überladung).
    /// </summary>
    public bool BoolContent {
        get {
            CheckDataType(FragmentDataType.Bool);
            return Encoding.Latin1.GetString(ByteContent).FromPlusMinus();
        }
    }

    /// <summary>
    /// Content als Int (Pendant zur Append-Überladung).
    /// </summary>
    public int IntContent {
        get {
            CheckDataType(FragmentDataType.Int);
            return ByteContent is { Length: 4 } bytes ? ToInt(bytes) : 0;
        }
    }

    /// <summary>
    /// Content als UInt (Pendant zur Append-Überladung).
    /// </summary>
    public uint UIntContent {
        get {
            CheckDataType(FragmentDataType.UInt);
            return ByteContent is { Length: 4 } bytes ? ToUInt(bytes) : 0u;
        }
    }

    /// <summary>
    /// Content als Long (Pendant zur Append-Überladung).
    /// </summary>
    public long LongContent {
        get {
            CheckDataType(FragmentDataType.Long);
            return ByteContent is { Length: 8 } bytes ? ToLong(bytes) : 0;
        }
    }

    /// <summary>
    /// Content als Float (Pendant zur Append-Überladung).
    /// </summary>
    public float FloatContent {
        get {
            CheckDataType(FragmentDataType.Float);
            return ByteContent is { Length: 4 } bytes ? ToFloat(bytes) : 0f;
        }
    }

    /// <summary>
    /// Content als Double (Pendant zur Append-Überladung).
    /// </summary>
    public double DoubleContent {
        get {
            CheckDataType(FragmentDataType.Double);
            return ByteContent is { Length: 8 } bytes ? ToDouble(bytes) : 0d;
        }
    }

    /// <summary>
    /// Content als Decimal (Pendant zur Append-Überladung).
    /// </summary>
    public decimal DecimalContent {
        get {
            CheckDataType(FragmentDataType.Decimal);
            return ByteContent is { Length: 16 } bytes ? ToDecimal(bytes) : 0m;
        }
    }

    /// <summary>
    /// Content als DateTime (Pendant zur Append-Überladung).
    /// </summary>
    public DateTime DateTimeContent {
        get {
            CheckDataType(FragmentDataType.DateTime);
            return ByteContent is { Length: 8 } bytes ? new DateTime(ToLong(bytes), DateTimeKind.Utc) : DateTime.MinValue;
        }
    }

    /// <summary>
    /// Content als Color (Pendant zur Append-Überladung, Hex-Code).
    /// </summary>
    public Color ColorContent {
        get {
            CheckDataType(FragmentDataType.Color);
            return ByteContent is { Length: 4 } bytes ? Color.FromArgb(ToInt(bytes)) : Color.Empty;
        }
    }

    /// <summary>
    /// Content als String-Liste: ListSet-Content je Eintrag (2 Längenbytes,
    /// UTF-8-Inhalt) dekodiert, ohne Sortierung oder Abschneiden; String-Sätze
    /// liefern ihre CR-getrennten Teile.
    /// </summary>
    public string[] ListContent {
        get {
            if (DataType == FragmentDataType.String) { return StringContent.SplitAndCutByCr(); }

            CheckDataType(FragmentDataType.ListSet);

            var entries = new List<string>();
            var pos = 0;

            while (pos < ByteContent.Length) {
                // Eintrag: 2 Längenbytes, UTF-8-Inhalt.
                if (pos + 2 > ByteContent.Length) { return []; }

                var len = ByteContent[pos] | (ByteContent[pos + 1] << 8);

                pos += 2;

                if (pos + len > ByteContent.Length) { return []; }

                entries.Add(Encoding.UTF8.GetString(ByteContent, pos, len));

                pos += len;
            }

            return entries.ToArray();
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
                return ByteContent is { Length: > 0 } ? JsonNode.Parse(Encoding.UTF8.GetString(ByteContent)) : null;
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

    /// <summary>
    /// Roh-Bytes des Journal-Eintrags, aus dem der Satz beim Nachladen eines fremden
    /// Journals stammt; null bei Sätzen ohne Journal-Bezug.
    /// </summary>
    public byte[]? Entry { get; private set; }

    /// <summary>
    /// Journal-Datei des Eintrags; leer bei Sätzen ohne Journal-Bezug.
    /// </summary>
    public string Container { get; private set; } = string.Empty;

    /// <summary>
    /// UTC-Zeitstempel des Journal-Eintrags (Meta des ersten Meta-Satzes des Eintrags);
    /// MinValue bei Sätzen ohne Journal-Bezug.
    /// </summary>
    public DateTime EntryDateTimeUtc { get; private set; }

    #endregion

    #region Methods

    /// <summary>
    /// Stellt sicher, dass der Satz den geforderten Content-Typ hat, sonst Absturz.
    /// </summary>
    private void CheckDataType(FragmentDataType required) {
        if (DataType != required) { throw Develop.DebugError($"Content-Typ {DataType} passt nicht zum geforderten Typ {required}."); }
    }

    /// <summary>
    /// Decodiert den Content je Content-Typ als anzeigbaren Text;
    /// defekte Sätze liefern einen Fehlerhinweis statt Absturz.
    /// </summary>
    public string ReadableContent() {
        try {
            switch (DataType) {
                case FragmentDataType.String:
                    return StringContent;

                case FragmentDataType.Bool:
                    return BoolContent ? "+" : "-";

                case FragmentDataType.Int:
                    return IntContent.ToString1();

                case FragmentDataType.Long:
                    return LongContent.ToString1();

                case FragmentDataType.Float:
                    return FloatContent.ToString1_5();

                case FragmentDataType.Double:
                    return DoubleContent.ToString1_5();

                case FragmentDataType.Decimal:
                    return DecimalContent.ToString(CultureInfo.InvariantCulture);

                case FragmentDataType.DateTime:
                    return DateTimeContent == DateTime.MinValue ? string.Empty : DateTimeContent.ToString7();

                case FragmentDataType.Color:
                    return ColorContent.IsEmpty ? string.Empty : ColorContent.ToHtmlCode();

                case FragmentDataType.Json:
                    return JsonContent?.ToJsonString() ?? string.Empty;

                case FragmentDataType.ListSet:
                    return string.Join("\r", ListContent);

                case FragmentDataType.KeyNameType:
                    return TypeContent + ": " + KeyNameContent;

                default:
                    // ListKeyObject: Content leer, die Items folgen als eigene Sätze.
                    return string.Empty;
            }
        } catch (Exception ex) {
            return "<Fehler: " + ex.Message + ">";
        }
    }

    /// <summary>
    /// Schreibt einen Steuer-Satz (kein Empfänger-Typ, nie Meta) mit einem
    /// Standard-Tag und Content-Typ String in den Stream; die Tiefe markiert den
    /// Kontext des Satzes. Klasseneigene Steuer-Werte werden per Cast auf
    /// FragmentTagType übergeben.
    /// Liefert false bei None oder Überschreiten der Längenbegrenzung.
    /// </summary>
    public static bool Append(Stream stream, int depth, FragmentTagType tag, string content) {
        return AppendTag(stream, depth, tag, FragmentDataType.String, Encoding.UTF8.GetBytes(content));
    }

    /// <summary>
    /// Schreibt einen Steuer-Satz mit Content „KeyName|TypeName“ (FragmentDataType.KeyNameType).
    /// Liefert false bei None oder Überschreiten der Längenbegrenzung.
    /// </summary>
    public static bool Append(Stream stream, int depth, FragmentTagType tag, string keyName, string typeName) {
        return AppendTag(stream, depth, tag, FragmentDataType.KeyNameType, Encoding.UTF8.GetBytes(keyName + "|" + typeName));
    }

    /// <summary>
    /// Schreibt einen Steuer-Satz mit Content-Typ DateTime (8 Tick-Bytes).
    /// </summary>
    public static bool Append(Stream stream, int depth, FragmentTagType tag, DateTime content) {
        return AppendTag(stream, depth, tag, FragmentDataType.DateTime, LongBytes(content.Ticks));
    }
    /// <summary>
    /// Schreibt einen String-Wert als Daten-Satz.
    /// </summary>
    public static bool Append(Stream stream, int depth, Enum leafType, string content) {
        var contentBytes = Encoding.UTF8.GetBytes(content);

        return AppendData(stream, depth, EnumByte(leafType), FragmentDataType.String, contentBytes, null);
    }

    /// <summary>
    /// Schreibt einen Bool-Wert als Daten-Satz (FragmentDataType.Bool, Content "+/-").
    /// </summary>
    public static bool Append(Stream stream, int depth, Enum leafType, bool content) => AppendData(stream, depth, EnumByte(leafType), FragmentDataType.Bool, Encoding.Latin1.GetBytes(content.ToPlusMinus()), null);

    /// <summary>
    /// Schreibt einen Int-Wert als Daten-Satz (FragmentDataType.Int, 4 Little-Endian-Bytes).
    /// </summary>
    public static bool Append(Stream stream, int depth, Enum leafType, int content) => AppendData(stream, depth, EnumByte(leafType), FragmentDataType.Int, IntBytes(content), null);

    /// <summary>
    /// Schreibt einen Long-Wert als Daten-Satz (FragmentDataType.Long, 8 Little-Endian-Bytes).
    /// </summary>
    public static bool Append(Stream stream, int depth, Enum leafType, long content) => AppendData(stream, depth, EnumByte(leafType), FragmentDataType.Long, LongBytes(content), null);

    /// <summary>
    /// Schreibt einen DateTime-Wert als Daten-Satz (FragmentDataType.DateTime, 8 Tick-Bytes).
    /// </summary>
    public static bool Append(Stream stream, int depth, Enum leafType, DateTime content) => AppendData(stream, depth, EnumByte(leafType), FragmentDataType.DateTime, LongBytes(content.Ticks), null);

    /// <summary>
    /// Schreibt einen Color-Wert als Daten-Satz (FragmentDataType.Color, 4 ARGB-Bytes).
    /// </summary>
    public static bool Append(Stream stream, int depth, Enum leafType, Color content) => AppendData(stream, depth, EnumByte(leafType), FragmentDataType.Color, IntBytes(content.ToArgb()), null);

    /// <summary>
    /// Schreibt einen Float-Wert als Daten-Satz (FragmentDataType.Float, 4 Little-Endian-Bytes).
    /// </summary>
    public static bool Append(Stream stream, int depth, Enum leafType, float content) => AppendData(stream, depth, EnumByte(leafType), FragmentDataType.Float, FloatBytes(content), null);

    /// <summary>
    /// Schreibt einen Double-Wert als Daten-Satz (FragmentDataType.Double, 8 Little-Endian-Bytes).
    /// </summary>
    public static bool Append(Stream stream, int depth, Enum leafType, double content) => AppendData(stream, depth, EnumByte(leafType), FragmentDataType.Double, DoubleBytes(content), null);

    /// <summary>
    /// Schreibt einen Decimal-Wert als Daten-Satz (FragmentDataType.Decimal, 16 Little-Endian-Bytes).
    /// </summary>
    public static bool Append(Stream stream, int depth, Enum leafType, decimal content) => AppendData(stream, depth, EnumByte(leafType), FragmentDataType.Decimal, DecimalBytes(content), null);

    /// <summary>
    /// Schreibt eine String-Liste als ListSet-Satz: je Eintrag 2 Längenbytes
    /// und Inhalt. Kein Join, kein Sort, kein Trim.
    /// </summary>
    public static bool Append(Stream stream, int depth, Enum leafType, IReadOnlyList<string> content) =>
        EncodeListPayload(content) is { } payload && AppendData(stream, depth, EnumByte(leafType), FragmentDataType.ListSet, payload, null);

    /// <summary>
    /// Schreibt einen JsonNode als Daten-Satz (FragmentDataType.Json, Content ToJsonString).
    /// </summary>
    public static bool Append(Stream stream, int depth, Enum leafType, JsonNode content) {
        var contentBytes = Encoding.UTF8.GetBytes(content.ToJsonString());

        return AppendData(stream, depth, EnumByte(leafType), FragmentDataType.Json, contentBytes, null);
    }

    /// <summary>
    /// Schreibt einen Sub*-Satz (Content = KeyName des Objekts) und darunter dessen
    /// Block eine Ebene tiefer. Der Key kommt von IHasKeyName.KeyName; ohne Key
    /// wird ein leerer Key geschrieben.
    /// </summary>
    public static bool Append(Stream stream, int depth, Enum leafType, IFragmentStoreable content) => Append(stream, depth, leafType, (content as IHasKeyName)?.KeyName ?? string.Empty, content);

    /// <summary>
    /// Schreibt einen Sub*-Satz mit explizitem Key und darunter den Block des
    /// Objekts eine Ebene tiefer (für Objekte, deren Key nicht der eigene
    /// KeyName ist).
    /// </summary>
    public static bool Append(Stream stream, int depth, Enum leafType, string content, IFragmentStoreable subObject) => Append(stream, depth, leafType, content) && subObject.Serialize(stream, depth + 1);

    /// <summary>
    /// Schreibt einen ListKeyObject-Satz (die Liste beginnt) und darunter je Item
    /// einen ItemAdd-Kopf („KeyName|TypeName“) eine Ebene tiefer, gefolgt vom
    /// Item-Block eine weitere Ebene tiefer; das Listen-Ende erkennt der Reader
    /// am Zurückfallen auf die Listen-Ebene.
    /// </summary>
    public static bool Append(Stream stream, int depth, Enum leafType, IReadOnlyList<IFragmentStoreable> content) {
        var allok = AppendData(stream, depth, EnumByte(leafType), FragmentDataType.ListKeyObject, [], null);

        foreach (var item in content) {
            allok &= Append(stream, depth + 1, FragmentTagType.ItemAdd, (item as IHasKeyName)?.KeyName ?? string.Empty, item.TypeName);
            allok &= item.SerializeCore(stream, depth + 2);
        }

        return allok;
    }

    /// <summary>
    /// Kodiert den ListSet-Content: je Eintrag 2 Längenbytes und UTF-8-Inhalt.
    /// Liefert null bei zu langen Einträgen.
    /// </summary>
    internal static byte[]? EncodeListPayload(IReadOnlyList<string> content) {
        var encodedEntries = new byte[content.Count][];
        var length = 0;

        for (var i = 0; i < content.Count; i++) {
            encodedEntries[i] = Encoding.UTF8.GetBytes(content[i]);

            if (encodedEntries[i].Length > MaxListEntryLength) { return null; }

            length += 2 + encodedEntries[i].Length;
        }

        var payload = new byte[length];
        var pos = 0;

        foreach (var entryBytes in encodedEntries) {
            payload[pos++] = (byte)(entryBytes.Length & 0xFF);
            payload[pos++] = (byte)((entryBytes.Length >> 8) & 0xFF);
            Array.Copy(entryBytes, 0, payload, pos, entryBytes.Length);
            pos += entryBytes.Length;
        }

        return payload;
    }

    /// <summary>
    /// Liefert das Byte eines Empfänger-Typ-Enums (klasseneigen oder FragmentTagType),
    /// damit Aufrufer keine (byte)-Casts brauchen.
    /// </summary>
    internal static byte EnumByte(Enum leafType) => Convert.ToByte(leafType, CultureInfo.InvariantCulture);

    /// <summary>
    /// Kodiert Ganzzahlen und ARGB-Farben als Little-Endian-Bytes (gleiches Byte-Muster wie die Längenfelder).
    /// </summary>
    internal static byte[] IntBytes(int value) => [(byte)value, (byte)(value >> 8), (byte)(value >> 16), (byte)(value >> 24)];

    /// <summary>
    /// Kodiert UInt-Werte als 4 Little-Endian-Bytes.
    /// </summary>
    internal static byte[] UIntBytes(uint value) => [(byte)value, (byte)(value >> 8), (byte)(value >> 16), (byte)(value >> 24)];

    /// <summary>
    /// Kodiert Long-Werte (auch DateTime-Ticks) als 8 Little-Endian-Bytes.
    /// </summary>
    internal static byte[] LongBytes(long value) => [
        (byte)value,
        (byte)(value >> 8),
        (byte)(value >> 16),
        (byte)(value >> 24),
        (byte)(value >> 32),
        (byte)(value >> 40),
        (byte)(value >> 48),
        (byte)(value >> 56)
    ];

    /// <summary>
    /// Kodiert Float-Werte als 4 Little-Endian-Bytes (IEEE-754-Bits).
    /// </summary>
    internal static byte[] FloatBytes(float value) => IntBytes(BitConverter.SingleToInt32Bits(value));

    /// <summary>
    /// Kodiert Double-Werte als 8 Little-Endian-Bytes (IEEE-754-Bits).
    /// </summary>
    internal static byte[] DoubleBytes(double value) => LongBytes(BitConverter.DoubleToInt64Bits(value));

    /// <summary>
    /// Kodiert Decimal-Werte als 16 Little-Endian-Bytes (GetBits: Lo, Mid, Hi, Flags).
    /// </summary>
    internal static byte[] DecimalBytes(decimal value) {
        var bits = decimal.GetBits(value);

        return [.. IntBytes(bits[0]), .. IntBytes(bits[1]), .. IntBytes(bits[2]), .. IntBytes(bits[3])];
    }

    private static int ToInt(byte[] bytes) => bytes[0] | bytes[1] << 8 | bytes[2] << 16 | bytes[3] << 24;

    private static uint ToUInt(byte[] bytes) => unchecked((uint)ToInt(bytes));

    private static long ToLong(byte[] bytes) => bytes[0]
        | (long)bytes[1] << 8
        | (long)bytes[2] << 16
        | (long)bytes[3] << 24
        | (long)bytes[4] << 32
        | (long)bytes[5] << 40
        | (long)bytes[6] << 48
        | (long)bytes[7] << 56;

    private static float ToFloat(byte[] bytes) => BitConverter.Int32BitsToSingle(ToInt(bytes));

    private static double ToDouble(byte[] bytes) => BitConverter.Int64BitsToDouble(ToLong(bytes));

    private static decimal ToDecimal(byte[] bytes) => new decimal([ToInt(bytes[0..4]), ToInt(bytes[4..8]), ToInt(bytes[8..12]), ToInt(bytes[12..16])]);

    /// <summary>
    /// Kern des Steuer-Satz-Schreibens: Tag an der valueId-Position, gefolgt vom
    /// Content-Typ, damit der Reader den Content typiert lesen kann.
    /// Liefert false bei None oder Überschreiten der Längenbegrenzung.
    /// </summary>
    private static bool AppendTag(Stream stream, int depth, FragmentTagType tag, FragmentDataType dataType, byte[] contentBytes) =>
        AppendData(stream, depth, (byte)tag, dataType, contentBytes, null);

    /// <summary>
    /// Kern des Daten-Satz-Schreibens: prüft Tiefe und Empfänger-Typ bzw. Tag,
    /// berechnet len1 und schreibt den Satz mit optionaler Meta-Sektion. Der
    /// Empfänger-Typ darf 1–255 sein: Werte ab 200 (FragmentTagType) werden beim
    /// Lesen als Tag geroutet.
    /// </summary>
    private static bool AppendData(Stream stream, int depth, byte leafType, FragmentDataType dataType, byte[] contentBytes, byte[]? metaBytes) {
        if (depth is < 0 or > byte.MaxValue || leafType == 0 || (byte)dataType > MaxUserTypeValue) { return false; }

        // Empfänger-Typ + valueId + len2-Feld + Content + Meta
        var len1 = 2 + 4 + contentBytes.Length + (metaBytes?.Length ?? 0);

        if (len1 > MaxFieldLength) { return false; }

        WriteRecord(stream, depth, [leafType, (byte)dataType], contentBytes, metaBytes, len1);
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
    /// Liest einen kompletten Roh-Satz (SpeicherTyp, Tiefe, len1, Rest) aus dem Stream.
    /// Liefert false bei Datei-Ende vor dem Satz oder abgeschnittener Struktur.
    /// </summary>
    private static bool TryReadRawRecord(Stream stream, long start, out byte[] buffer) {
        buffer = [];
        var header = new byte[6];
        if (stream.Read(header, 0, header.Length) != header.Length) { stream.Position = start; return false; }

        var len1 = header[2] | (header[3] << 8) | (header[4] << 16) | (header[5] << 24);
        var rest = new byte[Math.Max(len1, 0)];
        if (len1 < 1 || stream.Read(rest, 0, len1) != len1) { stream.Position = start; return false; }

        buffer = new byte[header.Length + rest.Length];
        Array.Copy(header, buffer, header.Length);
        Array.Copy(rest, 0, buffer, header.Length, rest.Length);

        return true;
    }

    /// <summary>
    /// Parst einen Satz ab startPosition: SpeicherTyp und Tiefe (1 Byte) lesen, len1 lesen,
    /// Empfänger-Typ bzw. Steuer-Kennung und Content-Typ lesen, len2/Content/Meta validieren.
    /// Bei strukturiert erkennbarem Satzende wird nextPosition über das Satzende gesetzt, sonst
    /// auf das Puffer-Ende. Bei Verletzung der Format-Regeln liefert die Methode false und record
    /// bleibt null.
    /// </summary>
    public static bool TryParse(byte[] buffer, int startPosition, out FragmentRecord? record, out int nextPosition) {
        record = null;
        nextPosition = startPosition;
        var pos = startPosition;

        if (pos + 6 > buffer.Length) { nextPosition = buffer.Length; return false; }

        var storageType = buffer[pos++];
        var depth = buffer[pos++];

        var len1 = buffer[pos] | (buffer[pos + 1] << 8) | (buffer[pos + 2] << 16) | (buffer[pos + 3] << 24);
        pos += 4;

        var recordEnd = pos + len1;

        if (len1 < 1 || recordEnd > buffer.Length) {
            nextPosition = buffer.Length;
            return false;
        }

        nextPosition = recordEnd;

        // Empfänger-Typ bzw. Tag: das erste Byte des Payloads ist bei klasseneigenen
        // Daten-Sätzen der Empfänger-Typ (< 200), im geschützten Bereich (≥ 200) der
        // FragmentTagType-Wert; dahinter folgt je Content-Typ.
        var valueId = buffer[pos++];
        var isTag = valueId > MaxUserTypeValue;

        if (pos >= recordEnd) { return false; }

        if (buffer[pos] > MaxUserTypeValue) { return false; }

        var dataType = (FragmentDataType)buffer[pos];
        pos++;

        byte[] contentBytes;

        if (pos + 4 > recordEnd) { return false; }

        var len2 = buffer[pos] | (buffer[pos + 1] << 8) | (buffer[pos + 2] << 16) | (buffer[pos + 3] << 24);
        pos += 4;

        // len2 kann durch das High-Bit des letzten Längen-Bytes negativ werden; der Vergleich
        // ohne Addition vermeidet den int-Überlauf bei absurden Längen.
        if (len2 < 0 || len2 > recordEnd - pos) { return false; }

        contentBytes = buffer[pos..(pos + len2)];
        pos += len2;

        // Rest ist die optionale Meta-Sektion (Längen-Arithmetik, kein Marker).
        var metaLength = recordEnd - pos;

        var result = new FragmentRecord {
            StorageType = storageType,
            Depth = depth,
            LeafType = isTag ? (byte)0 : valueId,
            DataType = dataType,
            TagType = isTag ? (FragmentTagType)valueId : FragmentTagType.None,
            ByteContent = contentBytes
        };

        if (metaLength > 0) {
            if (!result.ParseMeta(buffer, pos, metaLength)) { return false; }
        }

        record = result;
        return true;
    }

    /// <summary>
    /// Prüft, ob Byte-Daten (ggf. gezippt) einen gültigen EOF-Marker im binären
    /// Fragment-Format enthalten: Der EOF-Satz ist der 12-Byte-Abschluss
    /// (Storage-Type, Tiefe, len1=6, Tag Eof, Content-Typ, len2=0); geprüft wird
    /// der Tag an der valueId-Position.
    /// </summary>
    public static bool HasValidEofMarker(byte[] rawBytes) {
        try {
            if (rawBytes.IsZipped()) {
                if (rawBytes.UnzipIt() is not { } unzipped) { return false; }
                rawBytes = unzipped;
            }

            return rawBytes is { Length: >= 12 }
                && rawBytes[^12] == StandardStorageType
                && rawBytes[^6] == (byte)FragmentTagType.Eof;
        } catch {
            return false;
        }
    }

    /// <summary>
    /// Prüft, ob der Stream sauber abgeschlossen ist: Der letzte lesbare Satz ist
    /// der Eof-Marker (FragmentTagType.Eof). Unlesbare Inhalte und der
    /// Continue-Marker (FragmentTagType.ContinueEof) gelten als nicht abgeschlossen.
    /// </summary>
    public static bool EndsOnEof(Stream stream) {
        var endsOnEof = false;

        while (true) {
            if (TryReadNext(stream) is not { } record) {
                // Müll nach dem letzten gültigen Satz oder Datei-Ende
                return stream.Position >= stream.Length && endsOnEof;
            }

            endsOnEof = record.IsTag && record.TagType == FragmentTagType.Eof;
        }
    }

    /// <summary>
    /// Baut die Meta-Bytes (User, DateTimeUtc als 8 Tick-Bytes und Comment, durch US getrennt).
    /// </summary>
    private static byte[] BuildMetaBytes(string user, DateTime dateTimeUtc, string comment) {
        var userBytes = Encoding.UTF8.GetBytes(user);
        var dateBytes = LongBytes(dateTimeUtc.Ticks);
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
    /// Parst die Meta-Sektion (User, DateTimeUtc als 8 Tick-Bytes, Comment; durch US getrennt).
    /// Das Zeitfeld fester Breite (8 Bytes) wird positionsbasiert gelesen, da Tick-Bytes den
    /// US-Trenner zufällig enthalten können.
    /// </summary>
    private bool ParseMeta(byte[] buffer, int start, int length) {
        var first = Array.IndexOf(buffer, UnitSeparatorByte, start, length);
        if (first < 0) { return false; }

        var dateStart = first + 1;

        if (dateStart + 9 > start + length || buffer[dateStart + 8] != UnitSeparatorByte) { return false; }

        User = Encoding.UTF8.GetString(buffer, start, first - start);
        DateTimeUtc = new DateTime(ToLong(buffer[dateStart..(dateStart + 8)]), DateTimeKind.Utc);
        Comment = Encoding.UTF8.GetString(buffer, dateStart + 9, start + length - dateStart - 9);
        HasMeta = true;

        return true;
    }

    private static void WriteRecord(Stream stream, int depth, byte[] typeBytes, byte[] contentBytes, byte[]? metaBytes, int len1) {
        var record = new byte[2 + 4 + typeBytes.Length + 4 + contentBytes.Length + (metaBytes?.Length ?? 0)];
        var pos = 0;

        record[pos++] = StandardStorageType;
        record[pos++] = (byte)depth;
        record[pos++] = (byte)(len1 & 0xFF);
        record[pos++] = (byte)((len1 >> 8) & 0xFF);
        record[pos++] = (byte)((len1 >> 16) & 0xFF);
        record[pos++] = (byte)((len1 >> 24) & 0xFF);
        Array.Copy(typeBytes, 0, record, pos, typeBytes.Length);
        pos += typeBytes.Length;
        record[pos++] = (byte)(contentBytes.Length & 0xFF);
        record[pos++] = (byte)((contentBytes.Length >> 8) & 0xFF);
        record[pos++] = (byte)((contentBytes.Length >> 16) & 0xFF);
        record[pos++] = (byte)((contentBytes.Length >> 24) & 0xFF);
        Array.Copy(contentBytes, 0, record, pos, contentBytes.Length);
        pos += contentBytes.Length;

        if (metaBytes is not null) {
            Array.Copy(metaBytes, 0, record, pos, metaBytes.Length);
        }

        stream.Write(record);
    }

    /// <summary>
    /// Liefert eine Kopie des Satzes mit verschobener Einrücktiefe.
    /// </summary>
    public FragmentRecord WithDepth(int depth) {
        var result = new FragmentRecord {
            StorageType = StorageType,
            Depth = depth,
            LeafType = LeafType,
            DataType = DataType,
            TagType = TagType,
            ByteContent = ByteContent,
            User = User,
            DateTimeUtc = DateTimeUtc,
            Comment = Comment,
            HasMeta = HasMeta,
            Entry = Entry,
            Container = Container,
            EntryDateTimeUtc = EntryDateTimeUtc
        };

        return result;
    }

    /// <summary>
    /// Liefert eine Kopie des Satzes mit gesetztem Benutzer. Für das Lesen von
    /// Fragment-Journals: Dessen Meta trägt den Benutzer nicht mehr, daher wird
    /// der Journal-Eigentümer aus dem Kopf-Satz injiziert.
    /// </summary>
    public FragmentRecord WithUser(string user) {
        var result = new FragmentRecord {
            StorageType = StorageType,
            Depth = Depth,
            LeafType = LeafType,
            DataType = DataType,
            TagType = TagType,
            ByteContent = ByteContent,
            User = user,
            DateTimeUtc = DateTimeUtc,
            Comment = Comment,
            HasMeta = HasMeta,
            Entry = Entry,
            Container = Container,
            EntryDateTimeUtc = EntryDateTimeUtc
        };

        return result;
    }

    /// <summary>
    /// Liefert eine Kopie des Satzes mit den Daten des Journal-Eintrags, aus dem er
    /// stammt (Roh-Bytes, Container-Datei, Eintrags-Zeitstempel). Für das Nachladen
    /// fremder Fragment-Journale.
    /// </summary>
    public FragmentRecord WithEntry(byte[] entry, string container, DateTime entryDateTimeUtc) {
        var result = new FragmentRecord {
            StorageType = StorageType,
            Depth = Depth,
            LeafType = LeafType,
            DataType = DataType,
            TagType = TagType,
            ByteContent = ByteContent,
            User = User,
            DateTimeUtc = DateTimeUtc,
            Comment = Comment,
            HasMeta = HasMeta,
            Entry = entry,
            Container = container,
            EntryDateTimeUtc = entryDateTimeUtc
        };

        return result;
    }

    /// <summary>
    /// Liefert eine Kopie des Satzes ohne Meta; User, DateTimeUtc und Comment werden geleert.
    /// </summary>
    public FragmentRecord WithoutMeta() {
        var result = new FragmentRecord {
            StorageType = StorageType,
            Depth = Depth,
            LeafType = LeafType,
            DataType = DataType,
            TagType = TagType,
            ByteContent = ByteContent,
            User = string.Empty,
            DateTimeUtc = DateTime.MinValue,
            Comment = string.Empty,
            HasMeta = false,
            Entry = Entry,
            Container = Container,
            EntryDateTimeUtc = EntryDateTimeUtc
        };

        return result;
    }

    /// <summary>
    /// Schreibt den Satz auf der Ziel-Tiefe in den Stream (Pendant zum Einlesen per
    /// TryParse). Die Meta-Sektion enthält bewusst keinen Benutzer: Fragment-Journale
    /// sind benutzerbezogen, der Benutzer steht in den Kopf-Sätzen. Steuer-Sätze der
    /// Datei-Ebene liefern false; außerdem false bei ungültigem Empfänger-Typ bzw.
    /// Tag oder Überschreitung der Längenbegrenzung.
    /// </summary>
    public bool Append(Stream stream, int depth) {
        if (IsFileTag) { return false; }

        var metaBytes = HasMeta ? BuildMetaBytes(string.Empty, DateTimeUtc, Comment) : null;

        return AppendData(stream, depth, ValueId, DataType, ByteContent, metaBytes);
    }

    /// <summary>
    /// Liest alle tieferen Sätze ab der aktuellen Stream-Position als Eintrags-Sätze
    /// und schiebt sie auf Basis-Tiefe zurück (navDepth wird abgezogen). Der Stream
    /// steht danach am nächsten Satz der Nav-Ebene.
    /// </summary>
    public static List<FragmentRecord> ReadEntryRecords(Stream stream, int navDepth) {
        var result = new List<FragmentRecord>();

        while (true) {
            var pos0 = stream.Position;

            if (TryReadNext(stream) is not { } inner) {
                stream.Position = pos0;
                break;
            }

            if (inner.Depth <= navDepth) {
                stream.Position = pos0;
                break;
            }

            result.Add(inner.WithDepth(inner.Depth - navDepth));
        }

        return result;
    }

    #endregion
}
