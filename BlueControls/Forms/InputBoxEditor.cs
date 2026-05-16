// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Controls;
using BlueControls.Editoren;

namespace BlueControls.Forms;

public partial class InputBoxEditor : DialogWithOkAndCancel {

    #region Fields

    private static readonly ConcurrentCache<Type, Type?> _editorCache = new(200);
    private static readonly System.Collections.Generic.HashSet<Type> _processedEditors = new();
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
    /// Bearbeitet eine Kopie des übergebenen Items. Das Original bleibt unverändert.
    /// Gibt die bearbeitete Kopie zurück, oder null bei Abbruch.
    /// </summary>
    public static T? EditCopy<T>(T? item) where T : class, new() {
        if (item is null or IDisposableExtended { IsDisposed: true }) { return null; }
        var clone = new T();
        var (ok, result) = InputBoxEditor.ShowWithResult(clone, true, EditorMode.EditCopy);
        return ok ? (T?)result : null;
    }

    /// <summary>
    /// Bearbeitet das übergebene Item direkt im Editor.
    /// Gibt true zurück bei Bestätigung (OK), false bei Abbruch.
    /// </summary>
    public static bool EditItem(object? item) {
        var editorType = ResolveEditorType(item);
        return editorType != null && InputBoxEditor.Show(item, editorType, true, true, false, EditorMode.EditItem, out _);
    }

    /// <summary>
    /// Bearbeitet das übergebene Item mit dem angegebenen Editor-Typ.
    /// Wenn isDialog false ist, wird der Editor nicht-modal angezeigt.
    /// </summary>
    public static void EditItem(object? item, Type editorType, bool isDialog) {
        if (item is null or IDisposableExtended { IsDisposed: true }) { return; }
        InputBoxEditor.Show(item, editorType, true, isDialog, false, EditorMode.EditItem, out _);
    }

    /// <summary>
    /// Bearbeitet das übergebene Item direkt im Editor.
    /// allowInvalid: Wenn true, ist der OK-Button immer aktiv, auch bei Fehlern.
    /// </summary>
    public static bool EditItem(object? item, bool allowInvalid) {
        var editorType = ResolveEditorType(item);
        return editorType != null && InputBoxEditor.Show(item, editorType, true, true, allowInvalid, EditorMode.EditItem, out _);
    }

    /// <summary>
    /// Bearbeitet das übergebene Item direkt im Editor mit Angabe von isDialog und supportsCancel.
    /// </summary>
    public static bool EditItem(object? item, bool isDialog, bool supportsCancel) {
        var editorType = ResolveEditorType(item);
        return editorType != null && InputBoxEditor.Show(item, editorType, isDialog, supportsCancel, false, EditorMode.EditItem, out _);
    }

    /// <summary>
    /// Erstellt ein neues leeres Item des angegebenen Typs und bearbeitet es im Editor.
    /// Gibt das neue Item zurück, oder null bei Abbruch.
    /// </summary>
    public static T? EditNew<T>() where T : class, new() {
        var newItem = new T();
        var (ok, result) = InputBoxEditor.ShowWithResult(newItem, true, EditorMode.EditNew);
        return ok ? (T?)result : null;
    }

    /// <summary>
    /// Bearbeitet ein ISimpleEditor-Objekt in einem generischen Dialog.
    /// </summary>
    public static void EditSimple(ISimpleEditor? toEdit, bool isDialog) {
        if (toEdit is null or IDisposableExtended { IsDisposed: true }) { return; }
        InputBoxEditor.ShowSimple(toEdit, isDialog);
    }

    internal static Type? FindEditorType(Type toEditType) {
        if (_editorCache.TryGetValue(toEditType, out var cached)) { return cached; }

        foreach (var currentEditorType in IIsEditor.AllEditors.Types) {
            var isOriginal = !typeof(IIsEditor).IsAssignableFrom(currentEditorType.BaseType) ||
                              currentEditorType.BaseType == typeof(EditorEasy);

            if (!isOriginal) { continue; }

            lock (_processedEditors) {
                if (_processedEditors.Contains(currentEditorType)) { continue; }
                _processedEditors.Add(currentEditorType);
            }

            if (Activator.CreateInstance(currentEditorType) is not IIsEditor ie) { continue; }

            if (ie.EditorFor != null) { _editorCache[ie.EditorFor] = currentEditorType; }

            if (ie.EditorFor == toEditType || ie.EditorFor?.IsAssignableFrom(toEditType) == true) {
                return currentEditorType;
            }
        }

        return null;
    }

    internal static bool Show(object? toEdit, Type editortype, bool isDialog, bool supportsCancel, bool allowInvalid, EditorMode mode, out object? result) {
        result = null;
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
                if (frm is IIsEditor frmEditor && !frmEditor.SupportedModes.HasFlag(mode)) {
                    mode = EditorMode.OnlyShow;
                }
                mb = frm;
            } else if (myObject is EditorEasy ea) {
                ea.Mode = ea.SupportedModes.HasFlag(mode) ? mode : EditorMode.OnlyShow;
                mb = new InputBoxEditor(ea, supportsCancel, allowInvalid);
            }
        } catch { }

        if (mb == null) { return false; }

        if (mb is IIsEditor ie) {
            ie.InputItem = toEdit;
        } else if (mb is InputBoxEditor ibe) {
            foreach (var c in ibe.Controls) {
                if (c is IIsEditor ie2) {
                    ie2.InputItem = toEdit;
                }
            }
            ibe.UpdateButtons();
        }

        var ok = true;

        if (isDialog) {
            mb.ShowDialog();

            if (toEdit is IErrorCheckable iec && !iec.IsOk()) { ok = false; }

            if (mb is DialogWithOkAndCancel { Canceled: true }) { ok = false; }

            result = GetEditedItem(mb);
            mb.Dispose();
            return ok;
        }

        mb.Show();
        return true;
    }

    internal static void ShowSimple(ISimpleEditor? toEdit, bool isDialog) {
        if (toEdit is null or IDisposableExtended { IsDisposed: true }) { return; }

        var mb = new InputBoxEditor(toEdit.GetControl(400), false, false);

        if (isDialog) {
            mb.ShowDialog();
            mb.Dispose();
        } else {
            mb.Show();
        }
    }

    internal static (bool ok, object? result) ShowWithResult(object? toEdit, bool supportsCancel, EditorMode mode) {
        if (toEdit is null or IDisposableExtended { IsDisposed: true }) { return (false, null); }

        if (toEdit is IEditable editable) {
            var m = editable.IsNowEditable();
            if (!string.IsNullOrEmpty(m)) {
                MessageBox.Show($"<b>Bearbeitung aktuell nicht möglich:</b><br>{m}", ImageCode.Information, "Ok");
                return (false, null);
            }
        }

        var editorType = FindEditorType(toEdit.GetType());

        if (editorType == null) {
            MessageBox.Show($"<b>Bearbeitung aktuell nicht möglich:</b><br>Kein passender Editor gefunden", ImageCode.Information, "Ok");
            return (false, null);
        }

        var ok = Show(toEdit, editorType, true, supportsCancel, false, mode, out var res);
        return (ok, res);
    }

    internal void UpdateButtons() {
        foreach (var thisc in Controls) {
            if (thisc is EditorEasy ee) {
                // Wenn kein IErrorCheckable -> OK immer aktiv, keine Caption nötig
                if (((IIsEditor)ee).OutputItem is not IErrorCheckable ec) {
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
            if (thisc is EditorEasy ee && ((IIsEditor)ee).OutputItem is IErrorCheckable ec) {
                if (ec.IsOk()) { return true; }

                var b = MessageBox.Show($"<b><u>Es sind noch Fehler vorhanden:</u></b>\r\n\r\n{ec.ErrorReason()}\r\n\r\nMöchten sie diese beheben?", ImageCode.Warnung, "Beheben", "Verwerfen");

                if (b == 0) { return false; }
                Canceled = true;
            }
        }

        return true;
    }

    private static object? GetEditedItem(Form? mb) {
        if (mb is InputBoxEditor ibe) {
            foreach (var c in ibe.Controls) {
                if (c is EditorEasy ee) { return ((IIsEditor)ee).OutputItem; }
            }
        }

        return null;
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

        EditorEasy.EditItem(toEdit, true, false);
    }

    /// <summary>
    /// Routine für allgemeine Elemente, wenn nicht bekannt ist, welcher Editor zuständig ist
    /// </summary>
    /// <param name="toEdit"></param>
    /// <param name="isDialog"></param>
    /// <param name="supportsCancel"></param>
    /// <returns>True, wenn die Bearbeitung gültig ist (z.B. kein Cancel gedrückt wurde)</returns>
    public static bool Edit(this object? toEdit, bool isDialog, bool supportsCancel) => toEdit != null && EditorEasy.EditItem(toEdit, isDialog, supportsCancel);

    /// <summary>
    /// Routine mit speziellen Editor öffnen, wenn ein Typ mehrere Editoren besitzt. Z.B. TableHead und Scripte
    /// </summary>
    /// <param name="toEdit"></param>
    /// <param name="type"></param>
    /// <param name="isDialog"></param>
    public static void Edit(this object? toEdit, Type? type, bool isDialog) {
        if (toEdit == null || type == null) { return; }

        EditorEasy.EditItem(toEdit, type, isDialog);
    }

    #endregion
}