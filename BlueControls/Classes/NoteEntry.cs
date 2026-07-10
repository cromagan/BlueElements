// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.Controls;

namespace BlueControls.Classes;

public sealed class NoteEntry : ISimpleEditor, IReadableText, INotifyPropertyChanged {

    #region Constructors

    public NoteEntry() { }

    #endregion

    #region Events

    public event EventHandler? DoUpdateSideOptionMenu;

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Properties

    public string Description => "Notiz bearbeiten";

    public string Note {
        get;
        set {
            if (field != value) {
                field = value;
                OnPropertyChanged(nameof(Note));
            }
        }
    } = string.Empty;

    public NoteSymbols Symbol {
        get;
        set {
            if (field != value) {
                field = value;
                OnPropertyChanged(nameof(Symbol));
            }
        }
    } = NoteSymbols.Pencil;

    #endregion

    #region Methods

    public static Design DesignFor(NoteSymbols symbol) => symbol switch {
        NoteSymbols.Ok => Design.Note_OK,
        NoteSymbols.Warning => Design.Note_Warning,
        NoteSymbols.Critical => Design.Note_Error,
        _ => Design.Note_Default
    };

    public static QuickImage? GetQuickImage(NoteSymbols symbol, int size) => QuickImage.Get(ImageCodeFor(symbol), size);

    public static ImageCode ImageCodeFor(NoteSymbols symbol) => symbol switch {
        NoteSymbols.Critical => ImageCode.Kritisch,
        NoteSymbols.Warning => ImageCode.Warnung,
        NoteSymbols.Ok => ImageCode.Häkchen,
        _ => ImageCode.Stift
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

    public QuickImage? SymbolForReadableText(int size) => QuickImage.Get(ImageCodeFor(Symbol), size);

    private void OnPropertyChanged(string propertyName) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}