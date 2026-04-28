// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.ClassesStatic;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using System.Collections.Generic;

namespace BlueBasics.Classes;

public abstract class FormatHolder : IInputFormat, IReadableTextWithKey {

    #region Fields

    private readonly QuickImage _image;

    #endregion

    #region Constructors

    protected FormatHolder(string keyname, QuickImage img) {
        KeyName = keyname;
        _image = img;
    }

    #endregion

    #region Properties

    public static readonly AssemblyAwareCache<FormatHolder> AllFormats = new();

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

    private static FormatHolder GetByKey(string keyName) {
        return AllFormats.GetByKey(keyName) ?? throw new InvalidOperationException($"FormatHolder mit KeyName '{keyName}' nicht gefunden.");
    }

    #endregion
}