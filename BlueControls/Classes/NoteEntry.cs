// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.Controls;

namespace BlueControls.Classes;

/// <summary>
/// Notiz bearbeiten.
/// </summary>
public sealed class NoteEntry : ISimpleEditor, IReadableText, INotifyPropertyChanged {

    #region Constructors

    public NoteEntry() { }

    #endregion

    #region Events

    public event EventHandler? DoUpdateSideOptionMenu;

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Properties

    /// <summary>
    /// Der Inhalt der Notiz.
    /// </summary>
    public string Note {
        get;
        set {
            if (field != value) {
                field = value;
                OnPropertyChanged(nameof(Note));
            }
        }
    } = string.Empty;

    /// <summary>
    /// Die Art der Notiz.
    /// </summary>
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

    public List<GenericControl> GetProperties(int widthOfControl) => [
        new FlexiControlForProperty<NoteSymbols>(() => Symbol, ItemsOf(typeof(NoteSymbols))),
        new FlexiControlForProperty<string>(() => Note, 10)
    ];

    public string ReadableText() => Note;

    public QuickImage? SymbolForReadableText() => SymbolForReadableText(16);

    public QuickImage? SymbolForReadableText(int size) => QuickImage.Get(ImageCodeFor(Symbol), size);

    private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion
}