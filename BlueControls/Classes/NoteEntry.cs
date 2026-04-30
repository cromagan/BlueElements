// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.Controls;
using System.Drawing;

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

    public NoteSymbols Symbol { get; set; } = NoteSymbols.Pencil;

    #endregion

    #region Methods

    public static Color GetBackColor(NoteSymbols symbol) => symbol switch {
        NoteSymbols.Critical => Color.FromArgb(255, 100, 100),
        NoteSymbols.Warning => Color.FromArgb(255, 255, 100),
        NoteSymbols.Ok => Color.FromArgb(100, 255, 100),
        _ => Color.FromArgb(200, 200, 200)
    };

    public static QuickImage? GetQuickImage(NoteSymbols symbol, int size) => symbol switch {
        NoteSymbols.Critical => QuickImage.Get(ImageCode.Kritisch, size),
        NoteSymbols.Warning => QuickImage.Get(ImageCode.Warnung, size),
        NoteSymbols.Ok => QuickImage.Get(ImageCode.Häkchen, size),
        _ => QuickImage.Get(ImageCode.Stift, 16)
    };

    public static Color GetTextColor(NoteSymbols symbol) => symbol switch {
        NoteSymbols.Critical => Color.FromArgb(100, 0, 0),
        NoteSymbols.Warning => Color.FromArgb(100, 100, 0),
        NoteSymbols.Ok => Color.FromArgb(0, 100, 0),
        _ => Color.FromArgb(0, 0, 0)
    };

    public static Pen PenForSymbol(NoteSymbols symbol) => symbol switch {
        NoteSymbols.Critical => PenNoteCritical,
        NoteSymbols.Warning => PenNoteWarning,
        NoteSymbols.Ok => PenNoteOk,
        _ => PenNoteNone
    };

    public List<GenericControl> GetProperties(int widthOfControl) {
        var levels = new List<AbstractListItem> {
            new TextListItem("Neutral", ((int)NoteSymbols.Pencil).ToString1(), QuickImage.Get(ImageCode.Stift, 16), false, true, string.Empty, string.Empty),
            new TextListItem("Ok", ((int)NoteSymbols.Ok).ToString1(), QuickImage.Get(ImageCode.HäkchenDoppelt, 16), false, true, string.Empty, string.Empty),
            new TextListItem("Warnung", ((int)NoteSymbols.Warning).ToString1(), QuickImage.Get(ImageCode.Warnung, 16), false, true, string.Empty, string.Empty),
            new TextListItem("Kritisch", ((int)NoteSymbols.Critical).ToString1(), QuickImage.Get(ImageCode.Kritisch, 16), false, true, string.Empty, string.Empty)
        };

        return [
            new FlexiControlForProperty<NoteSymbols>(() => Symbol, levels),
            new FlexiControlForProperty<string>(() => Note, 10)
        ];
    }

    public string ReadableText() => Note;

    public QuickImage? SymbolForReadableText() => SymbolForReadableText(16);

    public QuickImage? SymbolForReadableText(int size) => Symbol switch {
        NoteSymbols.Ok => QuickImage.Get(ImageCode.Häkchen, size),
        NoteSymbols.Warning => QuickImage.Get(ImageCode.Warnung, size),
        NoteSymbols.Critical => QuickImage.Get(ImageCode.Kritisch, size),
        _ => QuickImage.Get(ImageCode.Stift, size)
    };

    #endregion
}