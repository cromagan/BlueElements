// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.Editoren;
using System.Collections.ObjectModel;

namespace BlueControls.BlueTableDialogs;

/// <summary>
/// Einfacher Editor (ISimpleEditor) für die Kopf-Eigenschaften
/// einer ColumnViewCollection. Liefert die Eigenschaften über
/// GetProperties als generische Steuerelemente; die Anzeige
/// übernimmt der generische Dialog (InputBoxEditor).
/// </summary>
public sealed class ColumnArrangementEditor : IIsEditor, ISimpleEditor, IHasQuickInfo {

    #region Fields

    private static readonly ReadOnlyCollection<string> _emptyStrings = new List<string>(0).AsReadOnly();

    private ColumnViewCollection? _cvc;

    #endregion

    #region Constructors

    public ColumnArrangementEditor() { }

    public ColumnArrangementEditor(ColumnViewCollection cvc) => _cvc = cvc;

    #endregion

    #region Events

    public event EventHandler? DoUpdateSideOptionMenu;

    #endregion

    #region Properties

    /// <summary>
    /// Skripte, die der Benutzer über diese Ansicht starten darf.
    /// </summary>
    public ReadOnlyCollection<string> Ausführbare_Skripte {
        get => _cvc?.Ausführbare_Skripte ?? _emptyStrings;
        set { if (_cvc is { } cvc) { cvc.Ausführbare_Skripte = value; WriteBack(); } }
    }

    /// <summary>
    /// Spalte, nach der die Tabelle in Abschnitte mit Überschriften gegliedert wird.
    /// </summary>
    public string ChapterColumn {
        get => _cvc?.ColumnForChapter?.KeyName ?? "#ohne";
        set { if (_cvc is { } cvc) { cvc.ColumnForChapter = value == "#ohne" ? null : cvc.Table?.Column[value]; WriteBack(); } }
    }

    /// <summary>
    /// Legt fest, ob und wie die Spaltenüberschriften angezeigt werden.
    /// </summary>
    public ColumnHeaderMode ColumnHeaderMode {
        get => _cvc?.ColumnHeaderMode ?? default;
        set { if (_cvc is { } cvc) { cvc.ColumnHeaderMode = value; WriteBack(); } }
    }

    public string Description => string.Empty;

    public Type? EditorFor => typeof(ColumnViewCollection);

    /// <summary>
    /// Spalten, deren Filterzeile immer sichtbar ist.
    /// </summary>
    public ReadOnlyCollection<string> Filter_immer_Anzeigen {
        get => _cvc?.Filter_immer_Anzeigen ?? _emptyStrings;
        set { if (_cvc is { } cvc) { cvc.Filter_immer_Anzeigen = value; WriteBack(); } }
    }

    /// <summary>
    /// Anzahl der zusätzlichen Zeilen, in denen gefiltert werden kann.
    /// </summary>
    public int FilterRows {
        get => _cvc?.FilterRows ?? 0;
        set { if (_cvc is { } cvc) { cvc.FilterRows = value; WriteBack(); } }
    }

    public object? InputItem {
        get => _cvc;
        set {
            if (value is ColumnViewCollection cvc) { _cvc = cvc; }
        }
    }

    /// <summary>
    /// Ersetzt das Rechtsklick-Menü der Tabelle durch die gewählten Skripte.
    /// </summary>
    public ReadOnlyCollection<string> Kontextmenu_Skripte {
        get => _cvc?.Kontextmenu_Skripte ?? _emptyStrings;
        set { if (_cvc is { } cvc) { cvc.Kontextmenu_Skripte = value; WriteBack(); } }
    }

    public EditorMode Mode { get; set; } = EditorMode.EditItem;

    /// <summary>
    /// Gruppen, die diese Ansicht sehen dürfen.
    /// </summary>
    public ReadOnlyCollection<string> PermissionGroups_Show {
        get => _cvc?.PermissionGroups_Show ?? _emptyStrings;
        set { if (_cvc is { } cvc) { cvc.PermissionGroups_Show = value; WriteBack(); } }
    }

    /// <summary>
    /// Hilfetext, der beim Berühren mit dem Mauszeiger erscheint.
    /// </summary>
    public string QuickInfo {
        get => _cvc?.QuickInfo ?? string.Empty;
        set { if (_cvc is { } cvc) { cvc.QuickInfo = value; WriteBack(); } }
    }

    /// <summary>
    /// Passt die Tabelle automatisch an den verfügbaren Platz an.
    /// </summary>
    public ScaleToFitMode ScaleToFit {
        get => _cvc?.ScaleToFit ?? ScaleToFitMode.Normal;
        set { if (_cvc is { } cvc) { cvc.ScaleToFit = value; WriteBack(); } }
    }

    /// <summary>
    /// Wenn gewählt, werden die Spaltenüberschriften der Tabelle angezeigt.
    /// </summary>
    public bool ShowHead {
        get => _cvc?.ShowHead ?? false;
        set { if (_cvc is { } cvc) { cvc.ShowHead = value; WriteBack(); } }
    }

    /// <summary>
    /// Wenn gewählt, sind die Gruppen der Ansicht beim Öffnen zugeklappt.
    /// </summary>
    public bool StartCollapsed {
        get => _cvc?.StartCollapsed ?? false;
        set { if (_cvc is { } cvc) { cvc.StartCollapsed = value; WriteBack(); } }
    }

    public EditorMode SupportedModes => EditorMode.EditItem;

    #endregion

    #region Methods

    public object? CreateNewItem() => null;

    public List<GenericControl> GetProperties(int widthOfControl) {
        if (_cvc is not { Table: { IsDisposed: false } tb }) { return []; }

        var chapterColumns = ItemsOf(tb.Column);
        chapterColumns.Add(ItemOf("Keine Überschriften", "#ohne", ImageCode.Kreuz, true, "!!!"));

        var filterColumns = ItemsOf(tb.Column);

        var scriptAll = new List<ListItem>();
        var scriptRow = new List<ListItem>();

        foreach (var thisScript in tb.EventScript.Where(s => s.UserGroups.Count > 0)) {
            scriptAll.Add(ItemOf(thisScript));

            if (thisScript.NeedRow) {
                scriptRow.Add(ItemOf(thisScript));
            }
        }

        var permissionItems = ItemsOf(TableView.Permission_AllUsed(false));

        var filterCtrl = new FlexiControlForProperty<ReadOnlyCollection<string>>(
            () => Filter_immer_Anzeigen, "Filter immer anzeigen von", 6, filterColumns,
            CheckBehavior.AllSelected, AddType.Suggestions, false);
        filterCtrl.RemoveAllowed = true;
        filterCtrl.MoveAllowed = true;

        var scriptCtrl = new FlexiControlForProperty<ReadOnlyCollection<string>>(
            () => Ausführbare_Skripte, "Ausführbare Skripte", 6, scriptAll,
            CheckBehavior.AllSelected, AddType.Suggestions, false);
        scriptCtrl.RemoveAllowed = true;
        scriptCtrl.MoveAllowed = true;

        var contextCtrl = new FlexiControlForProperty<ReadOnlyCollection<string>>(
            () => Kontextmenu_Skripte, "Kontextmenü ersetzen mit", 6, scriptRow,
            CheckBehavior.AllSelected, AddType.Suggestions, false);
        contextCtrl.RemoveAllowed = true;
        contextCtrl.MoveAllowed = true;

        var permissionCtrl = new FlexiControlForProperty<ReadOnlyCollection<string>>(
            () => PermissionGroups_Show, "Anzeigeberechtigung", 6, permissionItems,
            CheckBehavior.AllSelected, AddType.Suggestions, false);
        permissionCtrl.RemoveAllowed = true;
        permissionCtrl.MoveAllowed = true;

        return [
            new FlexiControlForProperty<bool>(() => ShowHead),
            new FlexiControlForProperty<ColumnHeaderMode>(() => ColumnHeaderMode, ItemsOf(typeof(ColumnHeaderMode))),
            new FlexiControlForProperty<ScaleToFitMode>(() => ScaleToFit, ItemsOf(typeof(ScaleToFitMode))),
            new FlexiControlForProperty<int>(() => FilterRows),
            new FlexiControlForProperty<string>(() => ChapterColumn, chapterColumns),
            new FlexiControlForProperty<bool>(() => StartCollapsed),
            new FlexiControlForProperty<string>(() => QuickInfo, 3),
            filterCtrl,
            scriptCtrl,
            contextCtrl,
            permissionCtrl,
        ];
    }

    /// <summary>
    /// Schreibt die bearbeitete Ansicht in Table.ColumnArrangements
    /// zurück. Ansichten werden als serialisierte Daten verwaltet und beim
    /// Editieren über ColumnViewCollection.ParseAll als Arbeitskopie
    /// erzeugt. Ohne diesen Rückgriff wären alle Änderungen verloren.
    /// </summary>
    private void WriteBack() {
        if (_cvc is not { Table: { IsDisposed: false } tb } cvc) { return; }

        var tcvc = ColumnViewCollection.ParseAll(tb);
        var idx = tcvc.FindIndex(c => string.Equals(c.KeyName, cvc.KeyName, StringComparison.OrdinalIgnoreCase));
        if (idx < 0) { return; }

        tcvc[idx] = cvc;
        tb.ColumnArrangements = tcvc.AsReadOnly();
    }

    #endregion
}