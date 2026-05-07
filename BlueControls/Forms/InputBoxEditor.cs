// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;
using BlueControls.Editoren;

namespace BlueControls.Forms;

public partial class InputBoxEditor : DialogWithOkAndCancel {

    #region Constructors

    private InputBoxEditor() : this(null, false) { }

    private InputBoxEditor(System.Windows.Forms.Control? centerControl, bool supportsCancel) : base(supportsCancel, true) {
        InitializeComponent();

        //if (toEdit == null) { return; }
        if (centerControl == null) { return; }

        Controls.Add(centerControl);

        Setup(string.Empty, centerControl, centerControl.Width + (Skin.Padding * 2));
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

        var mb = new InputBoxEditor(toEdit.GetControl(400), false);

        if (isDialog) {
            mb.ShowDialog();
            mb.Dispose();
        } else {
            mb.Show();
        }
    }

    /// <summary>
    /// Zeigt einen Editor für das angegebene IEditable-Objekt an.
    /// Sucht automatisch nach einem passenden Editor basierend auf dem Typ.
    /// </summary>
    /// <param name="toEdit">Das zu bearbeitende Objekt</param>
    /// <param name="isDialog">Gibt an, ob der Editor als Dialog angezeigt werden soll</param>
    /// <param name="supportsCancel">Gibt an, ob der Dialog abbrechbar sein soll</param>
    /// <returns>True, wenn die Bearbeitung gültig ist (z.B. kein Cancel gedrückt wurde)</returns>
    public static bool Show(IEditable? toEdit, bool isDialog, bool supportsCancel) {
        if (toEdit is null or IDisposableExtended { IsDisposed: true }) { return false; }

        var m = toEdit.IsNowEditable();
        if (!string.IsNullOrEmpty(m)) {
            MessageBox.Show($"<b>Bearbeitung aktuell nicht möglich:</b><br>{m}", ImageCode.Information, "Ok");
            return false;
        }

        // Suche nach passendem Editor: Zuerst exakter Treffer, dann abgeleitete Typen
        var toEditType = toEdit.GetType();
        Type? editorType = null;

        foreach (var ie in IIsEditor.AllEditors) {
            Type currentEditorType = ie.GetType();

            // Wir prüfen, ob die Basisklasse des Editors NICHT IIsEditor implementiert.
            // Wenn die Basisklasse es bereits implementiert, ist 'currentEditorType' eine Ableitung.
            bool isOriginal = !typeof(IIsEditor).IsAssignableFrom(currentEditorType.BaseType);

            if (isOriginal && (ie.EditorFor == toEditType || ie.EditorFor?.IsAssignableFrom(toEditType) == true)) {
                editorType = currentEditorType;
                break;
            }
        }

        if (editorType == null) {
            MessageBox.Show($"<b>Bearbeitung aktuell nicht möglich:</b><br>Kein passender Editor gefunden", ImageCode.Information, "Ok");
            return false;
        }

        return Show(toEdit, editorType, isDialog, supportsCancel);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="toEdit"></param>
    /// <param name="editortype"></param>
    /// <param name="supportsCancel"></param>
    /// <returns>True, wenn die Bearbeitung gültig ist (z.B. kein Cancel gedrückt wurde)</returns>
    public static bool Show(IEditable? toEdit, Type editortype, bool supportsCancel) => Show(toEdit, editortype, true, supportsCancel);

    /// <summary>
    ///
    /// </summary>
    /// <param name="toEdit"></param>
    /// <param name="editortype"></param>
    /// <param name="isDialog"></param>
    /// <param name="supportsCancel"></param>
    /// <returns>True, wenn die Bearbeitung gültig ist (z.B. kein Cancel gedrückt wurde)</returns>
    public static bool Show(IEditable? toEdit, Type editortype, bool isDialog, bool supportsCancel) {
        if (editortype == null) {
            MessageBox.Show($"<b>Bearbeitung aktuell nicht möglich:</b><br>Interne Bearbeitungsmethode nicht definiert", ImageCode.Information, "Ok");
            return false;
        }

        if (toEdit is null or IDisposableExtended { IsDisposed: true }) { return false; }

        var m = toEdit.IsNowEditable();
        if (!string.IsNullOrEmpty(m)) {
            MessageBox.Show($"<b>Bearbeitung aktuell nicht möglich:</b><br>{m}", ImageCode.Information, "Ok");
            return false;
        }

        if (!isDialog) { supportsCancel = false; }

        Form? mb = null;

        try {
            var myObject = Activator.CreateInstance(editortype);

            if (myObject is IIsEditor ie) {
                ie.ToEdit = toEdit;
                if (ie is EditorEasy ea) {
                    ea.ToEdit = toEdit;
                    mb = new InputBoxEditor(ea, supportsCancel);
                } else if (ie is Form frm) {
                    mb = frm;
                }
            }
        } catch { }

        if (mb == null) { return false; }

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

    protected override bool SetValue() {
        if (Canceled) { return true; }

        foreach (var thisc in Controls) {
            if (thisc is EditorEasy { ToEdit: IErrorCheckable ec } ee) {
                if (ec.IsOk()) { return true; }

                var b = MessageBox.Show($"<b><u>{ee.ToEdit.CaptionForEditor} enthält noch Fehler:</u></b>\r\n\r\n{ec.ErrorReason()}\r\n\r\nMöchten sie diese beheben?", ImageCode.Warnung, "Beheben", "Verwerfen");

                if (b == 0) { return false; }
                Canceled = true;
            }
        }

        return true;
    }

    #endregion
}

public static class InputBoxEditorExtension {

    #region Methods

    /// <summary>
    /// Routine für allgemeine Elemente, wenn nicht bekannt ist, welcher Editor zuständig ist
    /// </summary>
    /// <param name="toEdit"></param>
    public static void Edit(this IEditable? toEdit) {
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
    public static bool Edit(this IEditable? toEdit, bool isDialog, bool supportsCancel) => toEdit != null && InputBoxEditor.Show(toEdit, isDialog, supportsCancel);

    /// <summary>
    /// Routine mit speziellen Editor öffnen, wenn ein Typ mehrere Editoren besitzt. Z.B. TableHead und Scripte
    /// </summary>
    /// <param name="toEdit"></param>
    /// <param name="type"></param>
    /// <param name="isDialog"></param>
    public static void Edit(this IEditable? toEdit, Type? type, bool isDialog) {
        if (toEdit == null || type == null) { return; }

        InputBoxEditor.Show(toEdit, type, isDialog);
    }

    #endregion
}