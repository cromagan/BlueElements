// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Editoren;

/// <summary>
/// Standard element, dass das Grundgerüst eines Editors Darstellt.
/// Es müssen die Routinen SetValuesToFormula, Clear und InitializeComponentDefaultValues überschrieben werden.
/// </summary>

public partial class EditorEasy : System.Windows.Forms.UserControl, IIsEditor {

    #region Fields

    private bool _itemChangedWhileHidden;

    #endregion

    #region Constructors

    public EditorEasy() => InitializeComponent();

    #endregion

    #region Properties

    public virtual Type? EditorFor => null;

    public string Error { get; private set; } = "Nicht Initialisiert.";

    [DefaultValue(GroupBoxStyle.Nothing)]
    public GroupBoxStyle GroupBoxStyle {
        get;
        set {
            if (field == value) { return; }
            field = value;
            Invalidate();
        }
    } = GroupBoxStyle.Nothing;

    [DefaultValue(null)]
    public object? InputItem {
        get;
        set {
            if (field == value) { return; }

            if (!Visible || Disposing || IsDisposed) {
                field = value;
                _itemChangedWhileHidden = true;
                return;
            }

            field = null;

            if (!DefaultValuesInitialized) {
                DefaultValuesInitialized = true;
                InitializeComponentDefaultValues();
            }

            Clear();

            Error = string.Empty;
            if (!SetValuesToFormula(value)) {
                Error = "Objekt konnte nicht initialisiert werden.";
            }

            field = value;
            SetEnabledState();
            Invalidate();
        }
    }

    [DefaultValue(EditorMode.OnlyShow)]
    public EditorMode Mode {
        get;
        set {
            if (field == value) { return; }
            field = value;
            SetEnabledState();
        }
    } = EditorMode.OnlyShow;

    /// <inheritdoc/>
    public bool IsInputItemLoaded => !_itemChangedWhileHidden;

    public virtual EditorMode SupportedModes => EditorMode.EditCopy | EditorMode.EditItem;

    /// <summary>
    /// Ob die Standardwerte der Elemente erstellt wurden. Z.B. Komboboxen befüllt
    /// </summary>
    protected bool DefaultValuesInitialized { get; private set; }

    #endregion

    #region Methods

    /// <summary>
    /// Reseted Formulare. Löscht z.B. Texte, Tabellen-Einträge, etc
    /// </summary>
    public virtual void Clear() => Develop.DebugPrint_RoutineMussUeberschriebenWerden(false);

    /// <summary>
    /// Wird vom OutputItem-Getter aufgerufen (bei EditCopy).
    /// Abgeleitete Klassen können diese überschreiben,
    /// um den aktuell bearbeiteten Zustand als neues Objekt zurückzugeben (z.B. aus UI-Controls gelesen).
    /// Gibt null zurück, wenn der Originalwert (_inputItem) verwendet werden soll.
    /// </summary>
    public virtual object? CreateNewItem() => null;

    /// <summary>
    /// Bereitet das Formular vor. ZB. Dropdown Boxen
    /// </summary>
    protected virtual void InitializeComponentDefaultValues() => Develop.DebugPrint_RoutineMussUeberschriebenWerden(false);

    protected bool IsModeSupported() => Mode != EditorMode.OnlyShow && (SupportedModes & Mode) != 0;

    protected override void OnPaint(System.Windows.Forms.PaintEventArgs e) {
        if (IsDisposed) { return; }

        base.OnPaint(e);

        if (GroupBoxStyle == GroupBoxStyle.Nothing) { return; }

        var t = "[?]";

        if (InputItem is IEditable ea) { t = ea.CaptionForEditor; }

        var s = States.Standard;

        if (!Enabled) { s = States.Standard_Disabled; }
        BlueControls.Controls.GroupBox.DrawGroupBox(this, e.Graphics, s, GroupBoxStyle, t);
    }

    protected override void OnVisibleChanged(System.EventArgs e) {
        base.OnVisibleChanged(e);

        if (!Visible || Disposing || IsDisposed) { return; }

        if (DefaultValuesInitialized && !_itemChangedWhileHidden) { return; }

        _itemChangedWhileHidden = false;
        var merk = InputItem;
        InputItem = null; // Keine Steuerelement Änderungen auffangen
        InputItem = merk;
    }

    protected void SetEnabledState() {
        var enabled = InputItem is not null &&
                      IsModeSupported() &&
                      Mode != EditorMode.OnlyShow;

        SetEnabledState(enabled);
    }

    protected virtual void SetEnabledState(bool enabled) => Enabled = enabled;

    protected virtual bool SetValuesToFormula(object? toEdit) {
        Develop.DebugPrint_RoutineMussUeberschriebenWerden(false);
        return false;
    }

    #endregion
}