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

    public static readonly FormatHolder Bit = new FormatHolder_Bit();
    public static readonly FormatHolder Color = new FormatHolder_Color();
    public static readonly FormatHolder Date = new FormatHolder_Date();
    public static readonly FormatHolder DateTime = new FormatHolder_DateTime();
    public static readonly FormatHolder DateTimeWithMilliSeconds = new FormatHolder_DateTimeWithMilliSeconds();
    public static readonly FormatHolder Email = new FormatHolder_Email();
    public static readonly FormatHolder Filepath = new FormatHolder_Filepath();
    public static readonly FormatHolder FilepathAndName = new FormatHolder_FilepathAndName();
    public static readonly FormatHolder Float = new FormatHolder_Float();
    public static readonly FormatHolder FloatPositive = new FormatHolder_FloatPositive();
    public static readonly FormatHolder Long = new FormatHolder_Long();
    public static readonly FormatHolder LongPositive = new FormatHolder_LongPositive();
    public static readonly FormatHolder PhoneNumber = new FormatHolder_PhoneNumber();
    public static readonly FormatHolder SystemName = new FormatHolder_SystemName();
    public static readonly FormatHolder Text = new FormatHolder_Text();
    public static readonly FormatHolder TextMitFormatierung = new FormatHolder_TextMitFormatierung();
    public static readonly FormatHolder Url = new FormatHolder_Url();

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

    public string ReadableText() => KeyName;

    public QuickImage SymbolForReadableText() => _image;

    #endregion
}
