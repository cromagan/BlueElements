// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueBasics.Interfaces;

/// <summary>
/// Event-Argumente für IFragmentStoreable.FragmentChanged.
/// Beschreibt eine einzelne Fragment-Änderung im Sub-Baum (Wert-Selektor, Content-Typ
/// und Wert). Die Stufen der Kette feuern per WithNavigation um ihren Navigationssatz
/// erweiterte Argumente weiter; das Original bleibt unverändert, das Root leitet
/// daraus per UndoRecords die Undo-Sätze der Änderung ab.
/// </summary>
/// <remarks>
/// Die typisierten Konstruktoren nehmen den Rohtyp des Wertes entgegen und legen
/// Content und ContentType fertig konvertiert fest; der Wert-Selektor wird als
/// klasseneigenes Enum übergeben.
/// </remarks>
public class FragmentChangedEventArgs : System.EventArgs {

    #region Constructors

    /// <summary>
    /// Kern-Konstruktor: legt Content und ContentType fest.
    /// </summary>
    private FragmentChangedEventArgs(object sender, byte typeSelector, FragmentDataType contentType, string content, IReadOnlyList<string>? listContent) {
        SourceKey = (sender as IHasKeyName)?.KeyName ?? sender.GetType().Name;
        TypeSelector = typeSelector;
        ContentType = contentType;
        Content = content;
        ListContent = listContent;
    }

    /// <summary>
    /// Kopier-Konstruktor für WithNavigation: übernimmt die Änderung und stellt den
    /// Navigationssatz der Stufe vor die bereits gesammelten — Journal-Reihenfolge.
    /// </summary>
    private FragmentChangedEventArgs(FragmentChangedEventArgs basis, FragmentRecord navigation) {
        SourceKey = basis.SourceKey;
        TypeSelector = basis.TypeSelector;
        ContentType = basis.ContentType;
        Content = basis.Content;
        ListContent = basis.ListContent;
        _navigations = [navigation, .. basis._navigations];
    }

    /// <summary>
    /// Bool-Wert; Content als "+/-".
    /// </summary>
    public FragmentChangedEventArgs(object sender, Enum typeSelector, bool content) : this(sender, FragmentRecord.EnumByte(typeSelector), FragmentDataType.Bool, content.ToPlusMinus(), null) { }

    /// <summary>
    /// String-Wert.
    /// </summary>
    public FragmentChangedEventArgs(object sender, Enum typeSelector, string content) : this(sender, FragmentRecord.EnumByte(typeSelector), FragmentDataType.String, content, null) { }

    /// <summary>
    /// Int-Wert; Content als ToString1.
    /// </summary>
    public FragmentChangedEventArgs(object sender, Enum typeSelector, int content) : this(sender, FragmentRecord.EnumByte(typeSelector), FragmentDataType.Int, content.ToString1(), null) { }

    /// <summary>
    /// Long-Wert; Content als ToString1.
    /// </summary>
    public FragmentChangedEventArgs(object sender, Enum typeSelector, long content) : this(sender, FragmentRecord.EnumByte(typeSelector), FragmentDataType.Int, content.ToString1(), null) { }

    /// <summary>
    /// Float-Wert; Content als ToString1_5.
    /// </summary>
    public FragmentChangedEventArgs(object sender, Enum typeSelector, float content) : this(sender, FragmentRecord.EnumByte(typeSelector), FragmentDataType.Double, content.ToString1_5(), null) { }

    /// <summary>
    /// DateTime-Wert; Content als ToString5.
    /// </summary>
    public FragmentChangedEventArgs(object sender, Enum typeSelector, DateTime content) : this(sender, FragmentRecord.EnumByte(typeSelector), FragmentDataType.DateTime, content.ToString5(), null) { }

    /// <summary>
    /// Color-Wert; Content als Hex-Code (ToHtmlCode).
    /// </summary>
    public FragmentChangedEventArgs(object sender, Enum typeSelector, Color content) : this(sender, FragmentRecord.EnumByte(typeSelector), FragmentDataType.Color, content.ToHtmlCode(), null) { }

    /// <summary>
    /// JsonObject-Wert; Content als ToJsonString.
    /// </summary>
    public FragmentChangedEventArgs(object sender, Enum typeSelector, JsonObject content) : this(sender, FragmentRecord.EnumByte(typeSelector), FragmentDataType.Json, content.ToJsonString(), null) { }

    /// <summary>
    /// String-Liste als ListSet-Satz: der ListContent bleibt erhalten, der Writer
    /// schreibt ihn als Count + Einträge mit je 2 Längenbytes (kein Join).
    /// </summary>
    public FragmentChangedEventArgs(object sender, Enum typeSelector, IReadOnlyList<string> content) : this(sender, FragmentRecord.EnumByte(typeSelector), FragmentDataType.ListSet, string.Empty, content) { }

    /// <summary>
    /// Ganzes Objekt im Classic-Format; Content als Parseable-String (ParseableItems).
    /// </summary>
    public FragmentChangedEventArgs(object sender, Enum typeSelector, IFragmentStoreable content) : this(sender, typeSelector, content is IStringable s ? s.ParseableItems().FinishParseable() : string.Empty) { }

    #endregion

    #region Properties

    /// <summary>
    /// Navigationssätze der Stufen; die Aufnahme hält sie in Journal-Reihenfolge.
    /// </summary>
    private readonly List<FragmentRecord> _navigations = [];

    /// <summary>
    /// Wert-Selektor im klasseneigenen Enum des Empfängers.
    /// </summary>
    public byte TypeSelector { get; init; }

    /// <summary>
    /// Content-Typ (FragmentDataType).
    /// </summary>
    public FragmentDataType ContentType { get; init; }

    /// <summary>
    /// Der neue Wert.
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Der neue Wert bei ListSet-Inhalten; null bei Skalar-Werten.
    /// </summary>
    public IReadOnlyList<string>? ListContent { get; init; }

    /// <summary>
    /// KeyName des ursprünglichen Auslösers.
    /// </summary>
    public string SourceKey { get; init; } = string.Empty;

    #endregion

    #region Methods

    /// <summary>
    /// Liefert neue Argumente, die um den Navigationssatz dieser Stufe erweitert sind;
    /// tiefere Stufen melden vor der eigenen, das Original bleibt unverändert.
    /// </summary>
    public FragmentChangedEventArgs WithNavigation(Enum navigationType, string key) => new(this, FragmentRecord.CreateNavigation(navigationType, key));

    /// <summary>
    /// Liefert die Undo-Sätze der Änderung in Journal-Reihenfolge: die Navigationssätze
    /// der Stufen, zuletzt der Blatt-Satz mit Meta-Stempel.
    /// </summary>
    public IReadOnlyList<FragmentRecord> UndoRecords() => [.. _navigations, CreateUndoRecord()];

    /// <summary>
    /// Erzeugt den Blatt-Satz der Änderung mit Meta-Stempel (User, Zeitpunkt) als
    /// Undo-Eintrag auf Basis-Tiefe 1.
    /// </summary>
    public FragmentRecord CreateUndoRecord() =>
        FragmentRecord.Create(TypeSelector, ContentType, Content, ListContent, Generic.UserName, DateTime.UtcNow);

    /// <summary>
    /// Liest den Key des auslösenden Kindes frisch beim Ereignis (umbenennungssicher);
    /// Kinder ohne Schlüssel liefern einen leeren Key.
    /// </summary>
    public static string SenderKey(object? sender) => sender is IHasKeyName named ? named.KeyName : string.Empty;

    #endregion
}
