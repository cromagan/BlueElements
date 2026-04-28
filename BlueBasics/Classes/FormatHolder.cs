// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using System.Collections.Generic;

namespace BlueBasics.Classes;

public abstract class FormatHolder : IInputFormat, IReadableTextWithKey {

    #region Fields

    public static List<FormatHolder> AllFormats {
        get {
            field ??= [.. Generic.GetInstanceOfType<FormatHolder>()];
            return field;
        }
    }

    public static FormatHolder Bit => GetByKey("Bit");
    public static FormatHolder Color => GetByKey("Color");
    public static FormatHolder Date => GetByKey("Date");
    public static FormatHolder DateTime => GetByKey("DateTime");
    public static FormatHolder DateTimeWithMilliSeconds => GetByKey("DateTimeWithMilliSeconds");
    public static FormatHolder Email => GetByKey("EMail");
    public static FormatHolder Filepath => GetByKey("Filepath");
    public static FormatHolder FilepathAndName => GetByKey("FilepathAndName");
    public static FormatHolder Float => GetByKey("Float");
    public static FormatHolder FloatPositive => GetByKey("Float only Positive");
    public static FormatHolder Long => GetByKey("Long");
    public static FormatHolder LongPositive => GetByKey("Long only Positive");
    public static FormatHolder PhoneNumber => GetByKey("Phone Number");
    public static FormatHolder SystemName => GetByKey("Systemname");
    public static FormatHolder Text => GetByKey("Text");
    public static FormatHolder TextMitFormatierung => GetByKey("Text with format");
    public static FormatHolder Url => GetByKey("Url");

    private readonly QuickImage _image;

    #endregion

    #region Constructors

    protected FormatHolder(string keyname, QuickImage img) {
        KeyName = keyname;
        _image = img;
    }

    #endregion

    #region Properties

    public AdditionalCheck AdditionalFormatCheck { get; set; } = AdditionalCheck.None;
    public string AllowedChars { get; set; } = string.Empty;
    public bool KeyIsCaseSensitive => false;
    public string KeyName { get; protected set; }
    public int MaxTextLength { get; set; }
    public bool MultiLine { get; set; }
    public string QuickInfo { get; set; } = string.Empty;
    public string RegexCheck { get; set; } = string.Empty;

    public bool SpellCheckingEnabled { get; set; }

    public bool TextFormatingAllowed { get; set; }

    #endregion

    #region Methods

    private static FormatHolder GetByKey(string keyName) {
        return AllFormats.GetByKey(keyName) ?? throw new InvalidOperationException($"FormatHolder mit KeyName '{keyName}' nicht gefunden.");
    }

    public string ReadableText() => KeyName;

    public QuickImage SymbolForReadableText() => _image;

    #endregion
}
