// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics;
using BlueBasics.Classes;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.Json;

namespace BlueControls.Classes;

public sealed class NoteEntry : ISimpleEditor, IReadableText {

    #region Fields

    private static readonly Pen PenNoteCritical = new(Color.FromArgb(200, 220, 50, 50)) { Width = 2f };
    private static readonly Pen PenNoteNone = new(Color.FromArgb(200, 150, 150, 150)) { Width = 2f };
    private static readonly Pen PenNoteOk = new(Color.FromArgb(200, 50, 180, 80)) { Width = 2f };
    private static readonly Pen PenNoteWarning = new(Color.FromArgb(200, 230, 180, 30)) { Width = 2f };

    #endregion

    #region Constructors

    public NoteEntry() { }

    #endregion

    #region Events

    public event EventHandler? DoUpdateSideOptionMenu;

    #endregion

    #region Properties

    public string Description => "Notiz bearbeiten";

    public string Note { get; set; } = string.Empty;
    public string Symbol { get; set; } = "Stift";

    #endregion

    #region Methods

    public static Pen PenForSymbol(string symbol) => symbol switch {
        "Kritisch" => PenNoteCritical,
        "Warnung" => PenNoteWarning,
        "Häkchen" => PenNoteOk,
        _ => PenNoteNone
    };

    public List<GenericControl> GetProperties(int widthOfControl) {
        var levels = new List<AbstractListItem> {
            new TextListItem("Neutral", "Stift", QuickImage.Get(ImageCode.Stift, 16), false, true, string.Empty, string.Empty),
            new TextListItem("Ok", "Häkchen", QuickImage.Get(ImageCode.HäkchenDoppelt, 16), false, true, string.Empty, string.Empty),
            new TextListItem("Warnung", "Warnung", QuickImage.Get(ImageCode.Warnung, 16), false, true, string.Empty, string.Empty),
            new TextListItem("Kritisch", "Kritisch", QuickImage.Get(ImageCode.Kritisch, 16), false, true, string.Empty, string.Empty)
        };

        return [
            new FlexiControlForProperty<string>(() => Symbol, levels),
            new FlexiControlForProperty<string>(() => Note, 10)
        ];
    }

    public Pen Pen() => PenForSymbol(Symbol);

    public string ReadableText() => Note;

    public QuickImage? SymbolForReadableText() => SymbolForReadableText(16);

    public QuickImage? SymbolForReadableText(int size) => Symbol switch {
        "Häkchen" => QuickImage.Get(ImageCode.Häkchen, size),
        "Warnung" => QuickImage.Get(ImageCode.Warnung, size),
        "Kritisch" => QuickImage.Get(ImageCode.Kritisch, size),
        "Stift" => QuickImage.Get(ImageCode.Stift, size),
        _ => null
    };

    #endregion
}