// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.Editoren;
using BlueControls.EventArgs;
using BlueTable.EventArgs;
using static BlueBasics.ClassesStatic.IO;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.BlueTableDialogs;

/// <summary>
/// Dialog, der die Tabellen-Skript-Verwaltung hostet. Enthält genau zwei Controls:
/// die Liste der Skripte (<see cref="EditorForIEnumerable"/>) und den
/// <see cref="TableScriptEditor"/> zur Bearbeitung des jeweils gewählten Skripts.
/// Wird systemweit eindeutig pro Tabelle geöffnet
/// (<see cref="IUniqueWindowExtension.ShowOrCreate{T}"/>).
/// </summary>
public sealed partial class TableScriptEditorForm : BlueControls.Forms.Form, IUniqueWindow {

    #region Fields

    private Table? _table;

    #endregion

    #region Constructors

    public TableScriptEditorForm() : base() {
        InitializeComponent();
        lstEventScripts.Editor = tableScriptEditor;
        // Vor jedem Test/Ausführen muss die Arbeitskopie (OutputItem) ins Backend
        // durchgestellt werden, damit der Editor mit der frischen Backend-Instanz
        // weiterarbeitet. Das Executing-Event wird in der ScriptEditor-Basis
        // unmittelbar vor der Ausführung gefeuert.
        tableScriptEditor.Executing += TableScriptEditor_Executing;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Die Tabelle, deren Skripte bearbeitet werden. Wird von
    /// <see cref="IUniqueWindowExtension.ShowOrCreate{T}"/> gesetzt.
    /// </summary>
    public object? Object {
        get => IsDisposed ? null : _table;
        set {
            var tb = value as Table;
            if (IsDisposed || (tb?.IsDisposed ?? true)) { tb = null; }

            if (tb is not null && TableViewForm.EditableErrorMessage(tb, null)) {
                // Tabelle ist nicht editierbar - Editor gar nicht erst öffnen.
                if (IsHandleCreated) { BeginInvoke(new Action(Close)); }
                return;
            }

            if (_table == tb) { return; }

            DetachTable(_table);
            _table = tb;
            AttachTable(_table);

            tableScriptEditor.Table = _table;

            Text = _table is null ? "Skript-Editor" : "Skript-Editor: " + _table.Caption;

            // Datenquelle auf die aktuelle Collection setzen — das Setzen löst
            // intern bereits den Neuaufbau der Liste aus (ItemsChanged).
            lstEventScripts.InputItem = _table?.EventScript;
        }
    }

    #endregion

    #region Methods

    protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e) {
        // Gepufferte Eingaben (insb. den Skript-Text) vom Editor anfordern.
        // Table = null löst OnPropertyChanged(OutputItem) aus, wodurch der
        // EditorForIEnumerable sein OutputItem aktualisiert.
        tableScriptEditor.Table = null;

        // Arbeitskopie ins Backend übernehmen. Früher wurde bei jeder Änderung
        // das OutputItem ins Backend geschrieben (ItemsModified-Event) — jetzt
        // geschieht das zentral beim Schließen (und vor jedem Test über das
        // Executing-Event).
        WriteBackEventScripts();

        DetachTable(_table);
        _table = null;
        base.OnFormClosing(e);
    }

    private void AttachTable(Table? tb) {
        if (tb is null) { return; }
        tb.DisposingEvent += Table_Disposing;
        tb.Loaded += Table_Loaded;
        tb.WriteAccessChanged += Table_WriteAccessChanged;
    }

    private void btnVerlauf_Click(object? sender, System.EventArgs e) {
        if (IsDisposed || _table is not { IsDisposed: false } tb) { return; }
        if (lstEventScripts.SelectedItem is not TableScriptDescription item) { return; }

        var l = new List<string>();

        var sortedUndoItems = tb.Undo.Where(it => it.Command is TableDataType.EventScript)
            .OrderByDescending(it => it.DateTimeUtc);

        foreach (var thisUndo in sortedUndoItems) {
            l.Add("############################################################################");
            l.Add("############################################################################");
            l.Add("############################################################################");
            l.Add("############################################################################");
            l.Add("############################################################################");
            l.Add(thisUndo.DateTimeUtc.ToString7() + " " + thisUndo.User);

            l.Add("Art: " + thisUndo.Command);
            var found = false;
            foreach (var t in thisUndo.ChangedTo.SplitAndCutByCr()) {
                var s = new TableScriptDescription(tb, t);
                if (s.KeyName == item.KeyName && item.Script != s.Script) {
                    l.Add(s.Script);
                    found = true;
                    break;
                }
            }
            if (!found) {
                l.Add("    -> Keine Änderung am gewählten Skript");
            }
        }
        l.WriteAllText(TempFile(string.Empty, "Scrip.txt"), Win1252, true);
    }

    private void DetachTable(Table? tb) {
        if (tb is null) { return; }
        tb.DisposingEvent -= Table_Disposing;
        tb.Loaded -= Table_Loaded;
        tb.WriteAccessChanged -= Table_WriteAccessChanged;
    }

    private void LstEventScripts_AddClicked(object? sender, AddItemEventArgs e) {
        // Das Form erzeugt das neue Element und übergibt es über Add direkt
        // an den Sender (den EditorForIEnumerable). Dieser übernimmt das
        // Hinzufügen zur Arbeitskopie, die Aktualisierung der Anzeige und die
        // Selektion. Die Duplikatsprüfung erfolgt gegen die Arbeitskopie, da
        // das Backend erst beim Schließen (bzw. vor einem Skript-Test)
        // aktualisiert wird.
        if (_table is not { IsDisposed: false } tb) { return; }
        if (sender is not EditorForIEnumerable lst) { return; }

        var newItem = new TableScriptDescription(tb);
        if (lstEventScripts.OutputItem?.OfType<TableScriptDescription>().GetByKey(newItem.KeyName) is not null) {
            QuickNote.Show(NoteSymbols.Warning, "Skript existiert bereits");
            return;
        }

        lst.Add(newItem);
    }

    private void TableScriptEditor_Executing(object? sender, System.EventArgs e) => WriteBackEventScripts();

    /// <summary>
    /// Schreibt die Arbeitskopie (<see cref="EditorForIEnumerable.OutputItem"/>)
    /// in das Backend (<see cref="Table.EventScript"/>) zurück. Wird beim
    /// Schließen des Formulars sowie vor jedem Test/Ausführen (über das
    /// <see cref="ScriptEditor.Executing"/>-Event) aufgerufen.
    /// </summary>
    private void WriteBackEventScripts() {
        if (_table is not { IsDisposed: false } tb) { return; }
        if (lstEventScripts.OutputItem is null) { return; }

        tb.EventScript = lstEventScripts.OutputItem
            .Cast<TableScriptDescription>()
            .ToList()
            .AsReadOnly();

        // Backend tauscht die Collection-Referenz aus — Quelle neu setzen löst
        // den Neuaufbau aus (mit Selektionserhalt) und aktualisiert _item im
        // Editor auf die frische Backend-Instanz.
        lstEventScripts.InputItem = tb.EventScript;
    }

    private void LstEventScripts_ListBuilt(object? sender, System.EventArgs e) {
        if (IsDisposed || _table is not { IsDisposed: false } tb) { return; }

        foreach (var thisSet in tb.EventScript) {
            if (thisSet is null) { continue; }

            var cap = thisSet.EventTypes != 0 ? thisSet.EventTypes.ToString() : "Sonstige";

            if (lstEventScripts[thisSet.KeyName] is ReadableListItem rli) {
                rli.UserDefCompareKey = cap + SecondSortChar + thisSet.CompareKey;
            }

            if (lstEventScripts[cap] is null) {
                lstEventScripts.ItemAdd(ItemOf(cap, cap, true, cap + FirstSortChar));
            }
        }
    }

    private void Table_Disposing(object? sender, System.EventArgs e) {
        Object = null;
        if (IsHandleCreated) { BeginInvoke(new Action(Close)); }
    }

    private void Table_Loaded(object? sender, FirstEventArgs e) {
        // Bei externen Aktualisierungen (Server-Sync, Undo/Redo) werden alle
        // TableScriptDescription-Objekte neu erstellt. Quelle neu setzen.
        lstEventScripts.InputItem = _table?.EventScript;
    }

    private void Table_WriteAccessChanged(object? sender, WriteAccessChangedEventArgs e) {
        if (e.IsEditable || IsDisposed) { return; }
        Forms.Notification.Show("Skript-Editor wird geschlossen:<br>Schreibrechte fehlen (" + e.Reason + ")", ImageCode.Warnung);
        if (IsHandleCreated) { BeginInvoke(new Action(Close)); }
    }

    #endregion
}