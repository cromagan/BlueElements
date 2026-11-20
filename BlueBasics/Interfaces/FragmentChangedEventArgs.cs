// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueBasics.Interfaces;

/// <summary>
/// Event-Argumente für IFragmentStoreable.FragmentChanged.
/// Beschreibt eine einzelne Fragment-Änderung im Sub-Baum (Wert-Selektor, Content-Typ
/// und fertig kodierter Wert). Die Stufen der Kette feuern per WithNavigation um ihren
/// Navigationssatz erweiterte Argumente weiter; das Original bleibt unverändert, das Root
/// leitet daraus per UndoRecords die Undo-Sätze der Änderung ab.
/// </summary>
/// <remarks>
/// Die typisierten Konstruktoren nehmen den Rohtyp des Wertes entgegen und kodieren den
/// Content sofort nach den Regeln des Fragment-Formats; CreateUndoRecord reicht diesen
/// unverändert an FragmentRecord.Create weiter.
/// </remarks>
public class FragmentChangedEventArgs : System.EventArgs {

    #region Constructors

    /// <summary>
    /// Kern-Konstruktor: legt den fertig kodierten Content und den ContentType fest.
    /// </summary>
    private FragmentChangedEventArgs(object sender, byte typeSelector, FragmentDataType contentType, byte[] content) {
        SourceKey = (sender as IHasKeyName)?.KeyName ?? sender.GetType().Name;
        TypeSelector = typeSelector;
        ContentType = contentType;
        Content = content;
    }

    /// <summary>
    /// Kopier-Konstruktor für WithNavigation: übernimmt die Änderung samt Kommentar und stellt
    /// den Navigationssatz der Stufe vor die bereits gesammelten — Journal-Reihenfolge.
    /// </summary>
    private FragmentChangedEventArgs(FragmentChangedEventArgs basis, FragmentRecord navigation, string comment) {
        SourceKey = basis.SourceKey;
        TypeSelector = basis.TypeSelector;
        ContentType = basis.ContentType;
        Content = basis.Content;
        Comment = comment;
        _navigations = [navigation, .. basis._navigations];
    }

    /// <summary>
    /// Bool-Wert; Content als "+/-".
    /// </summary>
    public FragmentChangedEventArgs(object sender, Enum typeSelector, bool content) : this(sender, FragmentRecord.EnumByte(typeSelector), FragmentDataType.Bool, Encoding.Latin1.GetBytes(content.ToPlusMinus())) { }

    /// <summary>
    /// String-Wert; Content als UTF-8.
    /// </summary>
    public FragmentChangedEventArgs(object sender, Enum typeSelector, string content) : this(sender, FragmentRecord.EnumByte(typeSelector), FragmentDataType.String, Encoding.UTF8.GetBytes(content)) { }

    /// <summary>
    /// Schlüssel-Objekt (z. B. Aufnahme in eine Sammlung); Content als
    /// „KeyName|TypeName“ (FragmentDataType.KeyNameType), damit der Empfänger
    /// beim Einlesen sofort den richtigen Typ erzeugen kann.
    /// </summary>
    public FragmentChangedEventArgs(object sender, Enum typeSelector, IFragmentStoreable content) : this(sender, FragmentRecord.EnumByte(typeSelector), FragmentDataType.KeyNameType, Encoding.UTF8.GetBytes(((content as IHasKeyName)?.KeyName ?? string.Empty) + "|" + content.TypeName)) { }

    /// <summary>
    /// Int-Wert; Content als 4 Little-Endian-Bytes.
    /// </summary>
    public FragmentChangedEventArgs(object sender, Enum typeSelector, int content) : this(sender, FragmentRecord.EnumByte(typeSelector), FragmentDataType.Int, FragmentRecord.IntBytes(content)) { }


    /// <summary>
    /// Long-Wert; Content als 8 Little-Endian-Bytes.
    /// </summary>
    public FragmentChangedEventArgs(object sender, Enum typeSelector, long content) : this(sender, FragmentRecord.EnumByte(typeSelector), FragmentDataType.Long, FragmentRecord.LongBytes(content)) { }

    /// <summary>
    /// Float-Wert; Content als 4 Little-Endian-Bytes.
    /// </summary>
    public FragmentChangedEventArgs(object sender, Enum typeSelector, float content) : this(sender, FragmentRecord.EnumByte(typeSelector), FragmentDataType.Float, FragmentRecord.FloatBytes(content)) { }

    /// <summary>
    /// Double-Wert; Content als 8 Little-Endian-Bytes.
    /// </summary>
    public FragmentChangedEventArgs(object sender, Enum typeSelector, double content) : this(sender, FragmentRecord.EnumByte(typeSelector), FragmentDataType.Double, FragmentRecord.DoubleBytes(content)) { }

    /// <summary>
    /// Decimal-Wert; Content als 16 Little-Endian-Bytes.
    /// </summary>
    public FragmentChangedEventArgs(object sender, Enum typeSelector, decimal content) : this(sender, FragmentRecord.EnumByte(typeSelector), FragmentDataType.Decimal, FragmentRecord.DecimalBytes(content)) { }

    /// <summary>
    /// DateTime-Wert; Content als 8 Tick-Bytes (Little-Endian).
    /// </summary>
    public FragmentChangedEventArgs(object sender, Enum typeSelector, DateTime content) : this(sender, FragmentRecord.EnumByte(typeSelector), FragmentDataType.DateTime, FragmentRecord.LongBytes(content.Ticks)) { }

    /// <summary>
    /// Color-Wert; Content als 4 ARGB-Bytes (Little-Endian).
    /// </summary>
    public FragmentChangedEventArgs(object sender, Enum typeSelector, Color content) : this(sender, FragmentRecord.EnumByte(typeSelector), FragmentDataType.Color, FragmentRecord.IntBytes(content.ToArgb())) { }

    /// <summary>
    /// JsonObject-Wert; Content als ToJsonString-Text (UTF-8).
    /// </summary>
    public FragmentChangedEventArgs(object sender, Enum typeSelector, JsonObject content) : this(sender, FragmentRecord.EnumByte(typeSelector), FragmentDataType.Json, Encoding.UTF8.GetBytes(content.ToJsonString())) { }

    /// <summary>
    /// String-Liste als ListSet-Satz: die Einträge werden je UTF-8-kodiert und sofort
    /// zum Content (je Eintrag 2 Längenbytes, Inhalt) zusammengebaut; der
    /// Writer schreibt diesen Content unverändert (kein Join, kein Sort, kein Trim).
    /// </summary>
    public FragmentChangedEventArgs(object sender, Enum typeSelector, IReadOnlyList<string> content) {
        var payload = FragmentRecord.EncodeListPayload(content) ?? throw Develop.DebugError($"Ein List-Eintrag überschreitet {FragmentRecord.MaxListEntryLength} Bytes.");

        SourceKey = (sender as IHasKeyName)?.KeyName ?? sender.GetType().Name;
        TypeSelector = FragmentRecord.EnumByte(typeSelector);
        ContentType = FragmentDataType.ListSet;
        Content = payload;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Navigationssätze der Stufen; die Aufnahme hält sie in Journal-Reihenfolge.
    /// </summary>
    private readonly List<FragmentRecord> _navigations = [];

    /// <summary>
    /// Wert-Selektor: das Empfänger-Enum-Byte der Änderung — klasseneigen
    /// (unter 200) oder eine standardisierte Sammlungs-Rolle aus FragmentTagType.
    /// </summary>
    public byte TypeSelector { get; init; }

    /// <summary>
    /// Content-Typ (FragmentDataType).
    /// </summary>
    public FragmentDataType ContentType { get; init; }

    /// <summary>
    /// Der neue Wert, fertig kodiert (Kodier-Regeln der Append-Überladungen von FragmentRecord).
    /// </summary>
    public byte[] Content { get; init; } = [];

    /// <summary>
    /// KeyName des ursprünglichen Auslösers.
    /// </summary>
    public string SourceKey { get; init; } = string.Empty;

    /// <summary>
    /// Änderungs-Kommentar des Auslösers; wird durch alle Stufen gereicht und
    /// in den Meta-Stempel des Undo-Eintrags übernommen.
    /// </summary>
    public string Comment { get; init; } = string.Empty;

    #endregion

    #region Methods

    /// <summary>
    /// Liefert neue Argumente, die um den Navigationssatz dieser Stufe erweitert sind;
    /// tiefere Stufen melden vor der eigenen, das Original bleibt unverändert.
    /// </summary>
    public FragmentChangedEventArgs WithNavigation(Enum navigationType, string key) => new(this, FragmentRecord.CreateNavigation(navigationType, key), Comment);

    /// <summary>
    /// Liefert die Undo-Sätze der Änderung in Journal-Reihenfolge: die Navigationssätze
    /// der Stufen, zuletzt der Blatt-Satz mit Meta-Stempel.
    /// </summary>
    public IReadOnlyList<FragmentRecord> UndoRecords() => [.. _navigations, CreateUndoRecord()];

    /// <summary>
    /// Erzeugt den Blatt-Satz der Änderung mit Meta-Stempel (User, Zeitpunkt, Kommentar) als
    /// Undo-Eintrag auf Basis-Tiefe 0.
    /// </summary>
    public FragmentRecord CreateUndoRecord() =>
        FragmentRecord.Create(TypeSelector, ContentType, Content, Generic.UserName, DateTime.UtcNow, Comment);

    /// <summary>
    /// Liest den Key des auslösenden Kindes frisch beim Ereignis (umbenennungssicher);
    /// Kinder ohne Schlüssel liefern einen leeren Key.
    /// </summary>
    public static string SenderKey(object? sender) => sender is IHasKeyName named ? named.KeyName : string.Empty;

    #endregion
}
