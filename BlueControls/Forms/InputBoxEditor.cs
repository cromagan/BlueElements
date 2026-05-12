// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Controls;
using BlueControls.Editoren;

namespace BlueControls.Forms;

public partial class InputBoxEditor : DialogWithOkAndCancel {

    #region Fields

    private readonly bool _allowInvalid;
    private Caption? _capError;
    private System.Threading.Timer? _errorCheckTimer;

    #endregion

    #region Constructors

    private InputBoxEditor() : this(null, false, false) { }

    private InputBoxEditor(System.Windows.Forms.Control? centerControl, bool supportsCancel, bool allowInvalid) : base(supportsCancel, true) {
        _allowInvalid = allowInvalid;
        InitializeComponent();

        //if (toEdit == null) { return; }
        if (centerControl == null) { return; }

        Controls.Add(centerControl);

        Setup(string.Empty, centerControl, centerControl.Width + (Skin.Padding * 2));

        // Caption und Timer werden erst in UpdateButtons() erzeugt,
        // wenn tatsächlich ein IErrorCheckable vorliegt.
        if (centerControl is EditorEasy) {
            _errorCheckTimer = new System.Threading.Timer(CheckErrorState, null, 250, 250);
        }
    }

    #endregion

    #region Methods

    /// <summary>
    ///
    /// </summary>
    /// <param name="toEdit"></param>
    /// <param name="isDialog"></param>
    public static void Show(ISimpleEditor? toEdit, bool isDialog) {
        if (toEdit is null or IDisposableExtended { IsDisposed: true }) { return; }

        var mb = new InputBoxEditor(toEdit.GetControl(400), false, false);

        if (isDialog) {
            mb.ShowDialog();
            mb.Dispose();
        } else {
            mb.Show();
        }
    }

    /// <summary>
    /// Zeigt einen Editor für das angegebene Objekt an.
    /// Sucht automatisch nach einem passenden Editor basierend auf dem Typ.
    /// Wenn das Objekt IEditable implementiert, werden IsNowEditable und CaptionForEditor genutzt.
    /// </summary>
    /// <param name="toEdit">Das zu bearbeitende Objekt</param>
    /// <param name="isDialog">Gibt an, ob der Editor als Dialog angezeigt werden soll</param>
    /// <param name="supportsCancel">Gibt an, ob der Dialog abbrechbar sein soll</param>
    /// <returns>True, wenn die Bearbeitung gültig ist (z.B. kein Cancel gedrückt wurde)</returns>
    public static bool Show(object? toEdit, bool isDialog, bool supportsCancel) => Show(toEdit, isDialog, supportsCancel, false);

    /// <summary>
    /// Zeigt einen Editor für das angegebene Objekt an.
    /// Sucht automatisch nach einem passenden Editor basierend auf dem Typ.
    /// Wenn das Objekt IEditable implementiert, werden IsNowEditable und CaptionForEditor genutzt.
    /// </summary>
    /// <param name="toEdit">Das zu bearbeitende Objekt</param>
    /// <param name="isDialog">Gibt an, ob der Editor als Dialog angezeigt werden soll</param>
    /// <param name="supportsCancel">Gibt an, ob der Dialog abbrechbar sein soll</param>
    /// <param name="allowInvalid">Wenn true, ist der OK-Button immer aktiv, auch bei Fehlern</param>
    /// <returns>True, wenn die Bearbeitung gültig ist (z.B. kein Cancel gedrückt wurde)</returns>
    public static bool Show(object? toEdit, bool isDialog, bool supportsCancel, bool allowInvalid) {
        if (toEdit is null or IDisposableExtended { IsDisposed: true }) { return false; }

        if (toEdit is IEditable editable) {
            var m = editable.IsNowEditable();
            if (!string.IsNullOrEmpty(m)) {
                MessageBox.Show($"<b>Bearbeitung aktuell nicht möglich:</b><br>{m}", ImageCode.Information, "Ok");
                return false;
            }
        }

        // Suche nach passendem Editor: Zuerst exakter Treffer, dann abgeleitete Typen
        var toEditType = toEdit.GetType();
        Type? editorType = null;

        foreach (var ie in IIsEditor.AllEditors) {
            var currentEditorType = ie.GetType();

            // Wir prüfen, ob die Basisklasse des Editors NICHT IIsEditor implementiert.
            // Wenn die Basisklasse es bereits implementiert, ist 'currentEditorType' eine Ableitung.
            var isOriginal = !typeof(IIsEditor).IsAssignableFrom(currentEditorType.BaseType) ||
                              currentEditorType.BaseType == typeof(EditorEasy);

            if (isOriginal && (ie.EditorFor == toEditType || ie.EditorFor?.IsAssignableFrom(toEditType) == true)) {
                editorType = currentEditorType;
                break;
            }
        }

        if (editorType == null) {
            MessageBox.Show($"<b>Bearbeitung aktuell nicht möglich:</b><br>Kein passender Editor gefunden", ImageCode.Information, "Ok");
            return false;
        }

        return Show(toEdit, editorType, isDialog, supportsCancel, allowInvalid);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="toEdit"></param>
    /// <param name="editortype"></param>
    /// <param name="supportsCancel"></param>
    /// <returns>True, wenn die Bearbeitung gültig ist (z.B. kein Cancel gedrückt wurde)</returns>
    public static bool Show(object? toEdit, Type editortype, bool supportsCancel) => Show(toEdit, editortype, true, supportsCancel, false);

    /// <summary>
    ///
    /// </summary>
    /// <param name="toEdit"></param>
    /// <param name="editortype"></param>
    /// <param name="isDialog"></param>
    /// <param name="supportsCancel"></param>
    /// <returns>True, wenn die Bearbeitung gültig ist (z.B. kein Cancel gedrückt wurde)</returns>
    public static bool Show(object? toEdit, Type editortype, bool isDialog, bool supportsCancel) => Show(toEdit, editortype, isDialog, supportsCancel, false);

    /// <summary>
    ///
    /// </summary>
    /// <param name="toEdit"></param>
    /// <param name="editortype"></param>
    /// <param name="isDialog"></param>
    /// <param name="supportsCancel"></param>
    /// <param name="allowInvalid">Wenn true, ist der OK-Button immer aktiv, auch bei Fehlern</param>
    /// <returns>True, wenn die Bearbeitung gültig ist (z.B. kein Cancel gedrückt wurde)</returns>
    public static bool Show(object? toEdit, Type editortype, bool isDialog, bool supportsCancel, bool allowInvalid) {
        if (editortype == null) {
            MessageBox.Show($"<b>Bearbeitung aktuell nicht möglich:</b><br>Interne Bearbeitungsmethode nicht definiert", ImageCode.Information, "Ok");
            return false;
        }

        if (toEdit is null or IDisposableExtended { IsDisposed: true }) { return false; }

        if (toEdit is IEditable editable) {
            var m = editable.IsNowEditable();
            if (!string.IsNullOrEmpty(m)) {
                MessageBox.Show($"<b>Bearbeitung aktuell nicht möglich:</b><br>{m}", ImageCode.Information, "Ok");
                return false;
            }
        }

        if (!isDialog) { supportsCancel = false; }

        Form? mb = null;

        try {
            var myObject = Activator.CreateInstance(editortype);

            if (myObject is Form frm) {
                mb = frm;
            } else if (myObject is EditorEasy ea) {
                mb = new InputBoxEditor(ea, supportsCancel, allowInvalid);
            }
        } catch { }

        if (mb == null) { return false; }

        if (mb is IIsEditor ie) {
            ie.ToEdit = toEdit;
        } else if (mb is InputBoxEditor ibe) {
            foreach (var c in ibe.Controls) {
                if (c is IIsEditor ie2) {
                    ie2.ToEdit = toEdit;
                }
            }
            ibe.UpdateButtons();
        }

        var ok = true;

        if (isDialog) {
            mb.ShowDialog();

            if (toEdit is IErrorCheckable iec && !iec.IsOk()) { ok = false; }

            if (mb is DialogWithOkAndCancel { Canceled: true }) { ok = false; }

            mb.Dispose();
            return ok;
        }

        mb.Show();
        return true;
    }

    internal void UpdateButtons() {
        foreach (var thisc in Controls) {
            if (thisc is EditorEasy ee) {
                // Wenn kein IErrorCheckable -> OK immer aktiv, keine Caption nötig
                if (ee.ToEdit is not IErrorCheckable ec) {
                    butOK.Enabled = true;
                    if (_capError != null) {
                        _capError.Visible = false;
                    }
                    return;
                }

                var error = ec.ErrorReason();
                var hasError = !string.IsNullOrEmpty(error);
                butOK.Enabled = _allowInvalid || !hasError;

                // Caption nur jetzt (lazy) erzeugen, da IErrorCheckable bestätigt ist
                EnsureErrorCaption();

                if (_capError != null) {
                    _capError.Text = hasError ? $"<imagecode=Kreuz|16> {error}" : string.Empty;
                    _capError.Visible = hasError;
                    _capError.BringToFront();
                }
                return;
            }
        }

        // Kein EditorEasy -> OK immer aktiv
        butOK.Enabled = true;
    }

    protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e) {
        _errorCheckTimer?.Dispose();
        _errorCheckTimer = null;
        base.OnFormClosing(e);
    }

    protected override bool SetValue() {
        if (Canceled) { return true; }

        foreach (var thisc in Controls) {
            if (thisc is EditorEasy { ToEdit: IErrorCheckable ec } ee) {
                if (ec.IsOk()) { return true; }

                var b = MessageBox.Show($"<b><u>Es sind noch Fehler vorhanden:</u></b>\r\n\r\n{ec.ErrorReason()}\r\n\r\nMöchten sie diese beheben?", ImageCode.Warnung, "Beheben", "Verwerfen");

                if (b == 0) { return false; }
                Canceled = true;
            }
        }

        return true;
    }

    private void CheckErrorState(object? state) {
        if (_errorCheckTimer == null || Disposing || IsDisposed) { return; }

        try {
            BeginInvoke(new Action(UpdateButtons));
        } catch {
            // Form might be closing
        }
    }

    /// <summary>
    /// Erzeugt die Fehler-Caption beim ersten Bedarf und schafft Platz dafür über den Buttons.
    /// </summary>
    private void EnsureErrorCaption() {
        if (_capError != null) { return; }

        const int errorAreaHeight = 18;

        // Platz schaffen: Fenster vergrößern UND das centerControl in der Höhe reduzieren,
        // damit die Caption nicht überdeckt wird.
        Height += errorAreaHeight + Skin.Padding;

        foreach (var c in Controls) {
            if (c is EditorEasy ee) {
                ee.Height -= errorAreaHeight + Skin.Padding;
                break;
            }
        }

        _capError = new Caption {
            Translate = false,
            Visible = false,
            Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right
        };
        Controls.Add(_capError);

        _capError.Left = Skin.Padding;
        _capError.Width = Width - (Skin.Padding * 2) - BorderWidth;
        _capError.Top = butOK.Top - errorAreaHeight - Skin.Padding;
        _capError.Height = errorAreaHeight;
        _capError.BringToFront();
    }

    #endregion
}

public static class InputBoxEditorExtension {

    #region Methods

    /// <summary>
    /// Routine für allgemeine Elemente, wenn nicht bekannt ist, welcher Editor zuständig ist
    /// </summary>
    /// <param name="toEdit"></param>
    public static void Edit(this object? toEdit) {
        if (toEdit == null) { return; }

        InputBoxEditor.Show(toEdit, true, false);
    }

    /// <summary>
    /// Routine für allgemeine Elemente, wenn nicht bekannt ist, welcher Editor zuständig ist
    /// </summary>
    /// <param name="toEdit"></param>
    /// <param name="isDialog"></param>
    /// <param name="supportsCancel"></param>
    /// <returns>True, wenn die Bearbeitung gültig ist (z.B. kein Cancel gedrückt wurde)</returns>
    public static bool Edit(this object? toEdit, bool isDialog, bool supportsCancel) => toEdit != null && InputBoxEditor.Show(toEdit, isDialog, supportsCancel);

    /// <summary>
    /// Routine mit speziellen Editor öffnen, wenn ein Typ mehrere Editoren besitzt. Z.B. TableHead und Scripte
    /// </summary>
    /// <param name="toEdit"></param>
    /// <param name="type"></param>
    /// <param name="isDialog"></param>
    public static void Edit(this object? toEdit, Type? type, bool isDialog) {
        if (toEdit == null || type == null) { return; }

        InputBoxEditor.Show(toEdit, type, isDialog);
    }

    #endregion
}