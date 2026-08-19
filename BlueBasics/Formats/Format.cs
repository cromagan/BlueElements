// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Classes;

namespace BlueBasics.Formats;

public abstract class Format : IInputFormat, IReadableTextWithKey {

    #region Fields

    public static readonly AssemblyAwareCache<Format> AllFormats = new();
    private readonly QuickImage _image;

    #endregion

    #region Constructors

    protected Format(string keyname, QuickImage img) {
        KeyName = keyname;
        _image = img;
    }

    #endregion

    #region Properties

    public AdditionalCheck AdditionalFormatCheck { get; set; } = AdditionalCheck.None;
    public string AllowedChars { get; set; } = string.Empty;
    public string ForbiddenChars { get; set; } = string.Empty;
    public string KeyName { get; protected set; }
    public int MaxTextLength { get; set; }
    public int MinTextLength { get; set; }
    public bool MultiLine { get; set; }
    public string QuickInfo { get; set; } = string.Empty;
    public string RegexCheck { get; set; } = string.Empty;

    #endregion

    #region Methods

    public string ReadableText() => KeyName;

    public QuickImage SymbolForReadableText() => _image;

    #endregion
}