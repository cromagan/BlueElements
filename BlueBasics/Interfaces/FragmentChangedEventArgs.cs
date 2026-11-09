// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueBasics.Interfaces;

/// <summary>
/// Event-Argumente für IFragmentStoreable.FragmentChanged.
/// Beschreibt eine einzelne Fragment-Änderung im Sub-Baum; RelativePath trägt die
/// Crumb-Kette der Container als Paare aus Typ-Nummer und Key, getrennt durch US.
/// </summary>
/// <remarks>
/// Die typisierten Konstruktoren nehmen den Rohtyp des Wertes entgegen und legen
/// Content und ContentType fertig konvertiert fest; der Wert-Selektor wird als
/// klasseneigenes Enum übergeben.
/// </remarks>
/// <param name="sender">Der ursprüngliche Auslöser der Änderung.</param>
/// <param name="relativePath">Relativer Pfad vom Auslöser zum Abonnenten: Paare aus Typ und Key.</param>
/// <param name="typeSelector">Wert-Selektor als Byte des klasseneigenen Enums des Auslösers.</param>
/// <param name="contentType">Content-Typ (FragmentDataType).</param>
/// <param name="content">Der neue Wert, fertig konvertiert für das Fragment-Format.</param>
public class FragmentChangedEventArgs(object sender, string relativePath, byte typeSelector, FragmentDataType contentType, string content) : System.EventArgs {

    #region Fields

    /// <summary>
    /// Trennzeichen der Crumb-Pfade (Unit Separator); nur diese Klasse baut und liest sie.
    /// </summary>
    public const char PathSeparator = (char)0x1F;

    #endregion

    #region Constructors

    /// <summary>
    /// Bool-Wert; Content als "+/-".
    /// </summary>
    public FragmentChangedEventArgs(object sender, string relativePath, Enum typeSelector, bool content) : this(sender, relativePath, typeSelector, FragmentDataType.Bool, content.ToPlusMinus()) { }

    /// <summary>
    /// String-Wert.
    /// </summary>
    public FragmentChangedEventArgs(object sender, string relativePath, Enum typeSelector, string content) : this(sender, relativePath, typeSelector, FragmentDataType.String, content) { }

    /// <summary>
    /// Int-Wert; Content als ToString1.
    /// </summary>
    public FragmentChangedEventArgs(object sender, string relativePath, Enum typeSelector, int content) : this(sender, relativePath, typeSelector, FragmentDataType.Int, content.ToString1()) { }

    /// <summary>
    /// Long-Wert; Content als ToString1.
    /// </summary>
    public FragmentChangedEventArgs(object sender, string relativePath, Enum typeSelector, long content) : this(sender, relativePath, typeSelector, FragmentDataType.Int, content.ToString1()) { }

    /// <summary>
    /// Float-Wert; Content als ToString1_5.
    /// </summary>
    public FragmentChangedEventArgs(object sender, string relativePath, Enum typeSelector, float content) : this(sender, relativePath, typeSelector, FragmentDataType.Double, content.ToString1_5()) { }

    /// <summary>
    /// DateTime-Wert; Content als ToString5.
    /// </summary>
    public FragmentChangedEventArgs(object sender, string relativePath, Enum typeSelector, DateTime content) : this(sender, relativePath, typeSelector, FragmentDataType.DateTime, content.ToString5()) { }

    /// <summary>
    /// Color-Wert; Content als Hex-Code (ToHtmlCode).
    /// </summary>
    public FragmentChangedEventArgs(object sender, string relativePath, Enum typeSelector, Color content) : this(sender, relativePath, typeSelector, FragmentDataType.Color, content.ToHtmlCode()) { }

    /// <summary>
    /// String-Liste als ListSet-Satz: der ListContent bleibt erhalten, der Writer
    /// schreibt ihn als Count + Einträge mit je 2 Längenbytes (kein Join).
    /// </summary>
    public FragmentChangedEventArgs(object sender, string relativePath, Enum typeSelector, IReadOnlyList<string> content) : this(sender, relativePath, typeSelector, FragmentDataType.ListSet, string.Empty) {
        ListContent = content;
    }

    /// <summary>
    /// Ganzes Objekt im Classic-Format; Content als Parseable-String (ParseableItems).
    /// </summary>
    public FragmentChangedEventArgs(object sender, string relativePath, Enum typeSelector, IFragmentStoreable content) : this(sender, relativePath, typeSelector, content is IStringable s ? s.ParseableItems().FinishParseable() : string.Empty) { }

    /// <summary>
    /// Brücke für den Enum-Selektor: übernimmt die fertige Konvertierung zum Byte.
    /// </summary>
    private FragmentChangedEventArgs(object sender, string relativePath, Enum typeSelector, FragmentDataType contentType, string content) : this(sender, relativePath, FragmentRecord.EnumByte(typeSelector), contentType, content) { }

    #endregion

    #region Properties

    /// <summary>
    /// Relativer Pfad vom Auslöser zum Abonnenten: Paare aus Typ und Key.
    /// </summary>
    public string RelativePath { get; init; } = relativePath;

    /// <summary>
    /// Wert-Selektor im klasseneigenen Enum des Empfängers.
    /// </summary>
    public byte TypeSelector { get; init; } = typeSelector;

    /// <summary>
    /// Content-Typ (FragmentDataType).
    /// </summary>
    public FragmentDataType ContentType { get; init; } = contentType;

    /// <summary>
    /// Der neue Wert.
    /// </summary>
    public string Content { get; init; } = content;

    /// <summary>
    /// Der neue Wert bei ListSet-Inhalten; null bei Skalar-Werten.
    /// </summary>
    public IReadOnlyList<string>? ListContent { get; init; }

    /// <summary>
    /// KeyName des ursprünglichen Auslösers.
    /// </summary>
    public string SourceKey { get; init; } = (sender as IHasKeyName)?.KeyName ?? sender.GetType().Name;

    #endregion

    #region Methods

    /// <summary>
    /// Baut eine Crumb-Kette: stellt das eigene Paar (Typ, Key) vor den Pfad des Kindes.
    /// </summary>
    public static string Crumbs(byte crumbType, string crumbKey, string childPath) {
        if (childPath is not { Length: > 0 }) { return crumbType.ToString1() + PathSeparator + crumbKey + PathSeparator; }

        return crumbType.ToString1() + PathSeparator + crumbKey + PathSeparator + childPath;
    }

    /// <summary>
    /// Baut eine Crumb-Kette; der Typ wird als klasseneigenes Enum übergeben.
    /// </summary>
    public static string Crumbs(Enum crumbType, string crumbKey, string childPath) => Crumbs(FragmentRecord.EnumByte(crumbType), crumbKey, childPath);

    /// <summary>
    /// Liest den Key des auslösenden Kindes frisch beim Ereignis (umbenennungssicher);
    /// Kinder ohne Schlüssel liefern einen leeren Key.
    /// </summary>
    public static string CrumbKey(object? sender) => sender is IHasKeyName named ? named.KeyName : string.Empty;

    /// <summary>
    /// Liefert die Paare (Typ, Key) der Crumb-Kette in Reihenfolge.
    /// </summary>
    public List<(byte CrumbType, string CrumbKey)> CrumbPath() {
        List<(byte CrumbType, string CrumbKey)> result = [];

        var segments = RelativePath.Split(PathSeparator);

        for (var i = 0; i + 1 < segments.Length; i += 2) {
            if (byte.TryParse(segments[i], out var crumbType)) { result.Add((crumbType, segments[i + 1])); }
        }

        return result;
    }

    #endregion
}
