// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.Controls;
using BlueControls.EventArgs;
using System.Collections;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Editoren;

/// <summary>
/// Spezialisierte <see cref="ListBox"/>, die eine Collection vom Typ
/// <see cref="IEnumerable{T}"/> verwaltet und ein verknüpftes Editor-Control
/// für das jeweils ausgewählte Element anzeigt.
/// <para />
/// Der Editor ist ein reines Anzeige-Tool: er implementiert
/// <see cref="INotifyPropertyChanged"/> und meldet Änderungen ausschließlich
/// über <see cref="INotifyPropertyChanged.PropertyChanged"/>. Diese ListBox
/// fängt dieses Event ab, aktualisiert ihre eigene Anzeige und übernimmt die
/// geänderte Instanz in ihre Arbeitskopie (<see cref="OutputItem"/>). Das
/// hostende Form übernimmt die gesamte Backend-Arbeit — es liest
/// <see cref="OutputItem"/> beispielsweise beim Schließen aus und schreibt die
/// Collection ans Backend zurück. Wie der Editor verändert, steuert
/// <see cref="IIsEditor.Mode"/> bzw. <see cref="IIsEditor.SupportedModes"/>:
/// <list type="bullet">
/// <item><b>EditItem</b>: der Editor verändert das Element in-place. Die
/// Arbeitskopie enthält die Änderung über die gemeinsame Referenz bereits —
/// die ListBox aktualisiert nur ihre Anzeige.</item>
/// <item><b>EditCopy</b>: der Editor erzeugt über <see cref="IIsEditor.OutputItem"/>
/// eine neue Instanz. Die ListBox ersetzt das alte Element in ihrer Arbeitskopie
/// durch diese neue Instanz.</item>
/// </list>
/// <para />
/// Das Hinzufügen neuer Elemente wird nicht durch den Editor erzeugt,
/// sondern über <see cref="ListBox.AddClicked"/> an das Form durchgereicht.
/// Das Form erzeugt lediglich das neue Element und reicht es über
/// <see cref="EventArgs.AddItemEventArgs.NewItem"/> zurück; diese ListBox
/// übernimmt es dann automatisch in ihre Arbeitskopie (<see cref="OutputItem"/>),
/// aktualisiert die Anzeige und selektiert es. Das Entfernen übernimmt diese
/// ListBox ebenfalls auf ihrer Arbeitskopie. Es wird kein Änderungs-Event
/// gefeuert — der Host liest <see cref="OutputItem"/> aus, wenn er die
/// Arbeitskopie braucht (üblicherweise beim Schließen oder vor einer Aktion
/// wie dem Skript-Test).
/// <para />
/// Ist der Editor puffernd (hält Eingaben in UI-Controls vor, statt sie
/// sofort in das backing Item zu schreiben), wird vor jedem Selektionswechsel,
/// Hinzufügen und Entfernen das aktuelle <see cref="IIsEditor.OutputItem"/>
/// vom Editor angefordert (<see cref="SelectionChanging"/>), damit keine
/// gepufferten Eingaben verloren gehen.
/// <para />
/// Die ListBox wird vom Host über Änderungen der Quell-Collection informiert:
/// dieser setzt <see cref="InputItem"/> neu, woraufhin die Arbeitskopie neu
/// aufgebaut und die Anzeige aktualisiert wird (Tabellenwechsel, Backend-Update,
/// Undo/Redo).
/// </summary>
public partial class EditorForIEnumerable : ListBox {

    #region Events

    /// <summary>
    /// Wird nach jedem Neuaufbau der Liste durch <see cref="UpdateList"/> gefeuert.
    /// Erlaubt dem Aufrufer, gruppenbildende Items, benutzerdefinierte
    /// Sortierschlüssel oder weitere Nachbearbeitungen anzubringen.
    /// </summary>
    public event EventHandler? ListBuilt;

    /// <summary>
    /// Wird gefeuert, bevor die Selektion wechselt, ein Element hinzugefügt oder
    /// entfernt wird. unmittelbar danach fordert die ListBox das aktuelle
    /// <see cref="IIsEditor.OutputItem"/> vom Editor an.
    /// </summary>
    public event EventHandler? SelectionChanging;

    #endregion

    #region Properties

    /// <summary>
    /// Der Detail-Editor, der das aktuell ausgewählte Element anzeigt.
    /// Der zu bearbeitende Element-Typ wird über <see cref="IIsEditor.EditorFor"/>
    /// dieses Editors bestimmt. Der Editor wird vom Aufrufer visuell platziert.
    /// Implementiert der Editor <see cref="INotifyPropertyChanged"/>, wird auf
    /// jede Änderung reagiert (Anzeige aktualisieren und das Form benachrichtigen).
    /// </summary>
    public IIsEditor? Editor {
        get;
        set {
            if (ReferenceEquals(field, value)) { return; }
            DetachPropertyChanged(field);
            field = value;
            AttachPropertyChanged(field);
            RefreshEditor();
        }
    }

    /// <summary>
    /// Die Datenquelle für die zu bearbeitende Collection. Ein beliebiges
    /// <c>IEnumerable</c> — typischerweise die Backend-Collection selbst
    /// (z.B. <c>Table.EventScript</c>). Beim Setzen wird die ListBox neu aufgebaut,
    /// wobei eine bestehende Auswahl über Referenzidentität bzw. den KeyName
    /// (<see cref="IHasKeyName"/>) erhalten bleibt.
    /// <para />
    /// Da das Backend die Collection-Referenz bei jeder Änderung austauscht, muss
    /// der Host <see cref="InputItem"/> nach jeder Backend-Änderung neu auf die
    /// aktuelle Collection setzen — das Setzen löst intern bereits
    /// <see cref="InputItemsChanged"/> aus. Ein separater Aufruf ist nur nötig, wenn
    /// sich der Inhalt derselben Referenz geändert hat.
    /// </summary>
    public IEnumerable? InputItem {
        get;
        set {
            if (IsDisposed || Disposing) { return; }
            if (ReferenceEquals(field, value)) { return; }
            field = value;
            UpdateAddAllowed();

            OutputItem = InputItem is { } items
                ? [.. items.Cast<object>()]
                : [];

            UpdateList();
        }
    }

    /// <summary>
    /// Die aktuelle Arbeitskopie der Elemente. Wird beim Setzen von
    /// <see cref="InputItem"/> neu aus der Quelle aufgebaut. Modifikationen über
    /// Hinzufügen/Entfernen/Ersetzen aktualisieren diese Kopie. Es wird kein
    /// Änderungs-Event gefeuert — der Host liest diese Eigenschaft aus, wenn er
    /// die Arbeitskopie braucht (üblicherweise beim Schließen oder vor einer
    /// Aktion wie dem Skript-Test).
    /// </summary>
    public List<object>? OutputItem { get; private set; }

    /// <summary>
    /// Das aktuell selektierte Element (oder <c>null</c>). Beim Setzen wird das
    /// entsprechende Listen-Item visuell als aktiviert markiert, was über
    /// <see cref="ListBox.OnItemCheckedChanged"/> den Editor aktualisiert.
    /// Überwiegend für das hostende Form gedacht, das z.B. Backend-Aktionen für
    /// das gewählte Element ausführen muss (Verlauf anzeigen, Element löschen).
    /// </summary>
    public object? SelectedItem {
        get;
        private set {
            if (ReferenceEquals(field, value)) { return; }
            field = value;
            if (value is null) { return; }

            for (var i = 0; i < ItemCount; i++) {
                if (this[i] is ReadableListItem rli && ReferenceEquals(rli.Item, value)) {
                    Check(rli);
                    return;
                }
            }
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Wählt das Element mit dem angegebenen Schlüssel und zeigt es im Editor an.
    /// Wird vom hostenden Form typischerweise nach dem Hinzufügen eines neuen
    /// Elements aufgerufen. Hat keine Wirkung, wenn der Schlüssel nicht vorhanden ist.
    /// </summary>
    public void SelectByKey(string? key) {
        if (IsDisposed || Disposing || string.IsNullOrEmpty(key)) { return; }

        var item = FindItem(null, key);
        if (item is null) { return; }

        SelectedItem = item;
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            DetachPropertyChanged(Editor);
        }
        base.Dispose(disposing);
    }

    protected override void OnAddClicked(AddItemEventArgs e) {
        // Aktuellen Editor-Zustand fordern, damit gepufferte Eingaben nicht
        // verloren gehen, bevor das Form ein neues Element hinzufügt.
        OnSelectionChanging();
        DemandEditorOutput();

        // Event durchreichen — das übergeordnete Form erzeugt das neue Element
        // (AddClicked-Handler) und reicht es über AddItemEventArgs.NewItem
        // zurück. Die Basis darf kein Text-Item automatisch einfügen.
        base.OnAddClicked(e);
        e.Cancel = true;

        if (e.NewItem is not { } newItem) { return; }

        // Neues Element in die Arbeitskopie übernehmen, Anzeige aktualisieren
        // und selektieren. Das Backend wird beim Zurückschreiben der
        // Arbeitskopie (üblicherweise beim Schließen des Formulars) aktualisiert.
        OutputItem ??= [];
        OutputItem.Add(newItem);
        UpdateList();
        SelectByKey((newItem as IHasKeyName)?.KeyName);
    }

    protected override void OnEnabledChanged(System.EventArgs e) {
        base.OnEnabledChanged(e);
        RefreshEditor();
    }

    /// <summary>
    /// Reagiert auf Prüfzustandsänderungen der Liste und aktualisiert den
    /// verknüpften Editor mit dem aktuell selektierten Element. Da
    /// <see cref="SelectedItem"/> vor dem visuellen Aktivieren gesetzt wird,
    /// ist die Referenz beim Eintreffen des Events aktuell.
    /// </summary>
    protected override void OnItemCheckedChanged(System.EventArgs e) {
        base.OnItemCheckedChanged(e);
        Editor?.InputItem = SelectedItem;
    }

    protected override void OnItemClicked(AbstractListItemEventArgs e) {
        base.OnItemClicked(e);

        var newSel = (e.Item as ReadableListItem)?.Item;

        OnSelectionChanging();
        DemandEditorOutput();

        // DemandEditorOutput tauscht das selektierte Element ggf. durch eine neue
        // Instanz aus (EditCopy). Damit Selection und Editor konsistent auf der
        // aktuellen Instanz der Arbeitskopie stehen, wird das angeklickte Element
        // über Referenz bzw. KeyName wiederhergestellt — sonst wäre erst ein
        // zweiter Klick nötig.
        var clickedKey = (newSel as IHasKeyName)?.KeyName;
        var current = FindItem(newSel, clickedKey) ?? newSel;

        if (SelectedItem is not null && current is not null && ReferenceEquals(SelectedItem, current)) { return; }

        SelectedItem = current;
        Editor?.InputItem = current;
    }

    protected override void OnRemoveClicked(AbstractListItemEventArgs e) {
        base.OnRemoveClicked(e);
        if (IsDisposed || Disposing) { return; }
        if (e.Item is not ReadableListItem rli || rli.Item is not { } item) { return; }

        // Nur OutputItem fordern, wenn ein anderes als das zu entfernende Item
        // selektiert ist — das entfernte Item selbst hat nichts mehr zu persistieren.
        if (SelectedItem is null || !ReferenceEquals(SelectedItem, item)) {
            OnSelectionChanging();
            DemandEditorOutput();
        }

        OutputItem.RemoveAll(x => ReferenceEquals(x, item));

        if (SelectedItem is not null && ReferenceEquals(SelectedItem, item)) {
            SelectedItem = null;
            Editor?.InputItem = null;
        }
    }

    private static AbstractListItem CreateListItem(object item) {
        if (item is IReadableText rt) {
            var key = item is IHasKeyName hkn && !string.IsNullOrEmpty(hkn.KeyName)
                ? hkn.KeyName
                : rt.ReadableText();
            return ItemOf(rt, key);
        }
        var fallback = item.ToString() ?? item.GetType().Name;
        return ItemOf(fallback, fallback);
    }

    private void AttachPropertyChanged(IIsEditor? editor) {
        if (editor is INotifyPropertyChanged pcn) {
            pcn.PropertyChanged += Editor_PropertyChanged;
        }
    }

    private void DemandEditorOutput() {
        if (SelectedItem is null || Editor is null) { return; }
        if (Editor.OutputItem is not { } edited) { return; }
        if (ReferenceEquals(edited, SelectedItem)) { return; }

        var idx = OutputItem.FindIndex(x => ReferenceEquals(x, SelectedItem));
        if (idx >= 0) {
            OutputItem[idx] = edited;
            SelectedItem = edited;
        }
    }

    private void DetachPropertyChanged(IIsEditor? editor) {
        if (editor is INotifyPropertyChanged pcn) {
            pcn.PropertyChanged -= Editor_PropertyChanged;
        }
    }

    private void Editor_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (IsDisposed || Disposing) { return; }
        if (Editor is null) { return; }

        if (Editor.Mode == EditorMode.EditItem) {
            // In-Place-Änderung: das selektierte Element wurde direkt verändert.
            // Die Arbeitskopie enthält die Änderung über die gemeinsame Referenz
            // bereits — nur die Anzeige aktualisieren und das Form benachrichtigen.
            UpdateList();
            return;
        }

        // EditCopy: Jede Änderung im Editor führt über OutputItem zu einer neuen
        // Instanz. Die Editoren feuern PropertyChanged mit aussagekräftigen Namen
        // (z.B. "Picture", "Permission", "Name", ...), die hier einheitlich
        // behandelt werden — der konkrete Name ist für die Reaktion ohne Belang.
        OnSelectionChanging();
        DemandEditorOutput();
    }

    private object? FindItem(object? byReference, string? byKey) {
        if (OutputItem is not { Count: > 0 } items) { return null; }

        var refOnly = byReference is not null;
        var keyOnly = !string.IsNullOrEmpty(byKey);

        if (!refOnly && !keyOnly) { return null; }

        foreach (var item in items) {
            if (refOnly && ReferenceEquals(item, byReference)) { return item; }
            if (keyOnly && item is IHasKeyName hkn && string.Equals(hkn.KeyName, byKey, StringComparison.Ordinal)) { return item; }
        }
        return null;
    }

    private void OnListBuilt() => ListBuilt?.Invoke(this, System.EventArgs.Empty);

    private void OnSelectionChanging() => SelectionChanging?.Invoke(this, System.EventArgs.Empty);

    private void RefreshEditor() {
        UpdateAddAllowed();
        if (Editor is not { } e) { return; }

        e.Mode = !Enabled
            ? EditorMode.OnlyShow
            : (e.SupportedModes.HasFlag(EditorMode.EditCopy) ? EditorMode.EditCopy : EditorMode.EditItem);

        Editor?.InputItem = SelectedItem;
    }

    private void UpdateAddAllowed() {
        var hasItems = Enabled && InputItem is not null;
        AddAllowed = hasItems ? AddType.Suggestions : AddType.None;
        RemoveAllowed = hasItems;
    }

    /// <summary>
    /// Baut die Liste aus <see cref="InputItem"/> neu auf. Bestehende Selektion
    /// wird bevorzugt über Referenzidentität, als Fallback über den KeyName
    /// (<see cref="IHasKeyName"/>) wiederhergestellt. So bleibt die visuelle
    /// Auswahl auch erhalten, wenn das Backend nach einer Änderung neue
    /// Objekt-Instanzen mit gleichem Schlüssel erzeugt (z.B. <c>TableScriptDescription</c>).
    /// </summary>
    private void UpdateList() {
        if (IsDisposed || Disposing) { return; }

        // SelectedItem während des Neu-Aufbaus null setzen, damit das
        // ausgelöste ItemCheckedChanged nicht fälschlich einen Wechsel annimmt.
        var merkItem = SelectedItem;
        var merkKeyName = (merkItem as IHasKeyName)?.KeyName;
        SelectedItem = null;
        ItemClear();
        if (OutputItem is { Count: > 0 } items) {
            foreach (var item in items) {
                ItemAdd(CreateListItem(item));
            }
        }

        // Selektion wiederherstellen: erst über Referenz, dann über KeyName.
        // Der Setter übernimmt das visuelle Aktivieren; OnItemCheckedChanged
        // aktualisiert den Editor auf die ggf. neue Instanz.
        SelectedItem = FindItem(merkItem, merkKeyName);

        OnListBuilt();
    }

    #endregion
}