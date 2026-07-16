// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.Controls;
using BlueControls.Editoren;
using System.Collections.ObjectModel;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.BlueTableDialogs;

/// <summary>
/// Einfacher Editor (<see cref="ISimpleEditor" />) für die Kopf-Eigenschaften
/// einer <see cref="ColumnViewCollection" />. Liefert die Eigenschaften über
/// <see cref="GetProperties" /> als generische Steuerelemente; die Anzeige
/// übernimmt der generische Dialog (<see cref="InputBoxEditor" />).
/// </summary>
public sealed class ColumnArrangementEditor : IIsEditor, ISimpleEditor {

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

    public ReadOnlyCollection<string> Ausführbare_Skripte {
        get => _cvc?.Ausführbare_Skripte ?? _emptyStrings;
        set { if (_cvc is { } cvc) { cvc.Ausführbare_Skripte = value; WriteBack(); } }
    }

    public string ChapterColumn {
        get => _cvc?.ColumnForChapter?.KeyName ?? "#ohne";
        set { if (_cvc is { } cvc) { cvc.ColumnForChapter = value == "#ohne" ? null : cvc.Table?.Column[value]; WriteBack(); } }
    }

    public ColumnHeaderMode ColumnHeaderMode {
        get => _cvc?.ColumnHeaderMode ?? default;
        set { if (_cvc is { } cvc) { cvc.ColumnHeaderMode = value; WriteBack(); } }
    }

    public string Description => string.Empty;

    public Type? EditorFor => typeof(ColumnViewCollection);

    public ReadOnlyCollection<string> Filter_immer_Anzeigen {
        get => _cvc?.Filter_immer_Anzeigen ?? _emptyStrings;
        set { if (_cvc is { } cvc) { cvc.Filter_immer_Anzeigen = value; WriteBack(); } }
    }

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

    public ReadOnlyCollection<string> Kontextmenu_Skripte {
        get => _cvc?.Kontextmenu_Skripte ?? _emptyStrings;
        set { if (_cvc is { } cvc) { cvc.Kontextmenu_Skripte = value; WriteBack(); } }
    }

    public EditorMode Mode { get; set; } = EditorMode.EditItem;

    public ReadOnlyCollection<string> PermissionGroups_Show {
        get => _cvc?.PermissionGroups_Show ?? _emptyStrings;
        set { if (_cvc is { } cvc) { cvc.PermissionGroups_Show = value; WriteBack(); } }
    }

    public string QuickInfo {
        get => _cvc?.QuickInfo ?? string.Empty;
        set { if (_cvc is { } cvc) { cvc.QuickInfo = value; WriteBack(); } }
    }

    public ScaleToFitMode ScaleToFit {
        get => _cvc?.ScaleToFit ?? ScaleToFitMode.Normal;
        set { if (_cvc is { } cvc) { cvc.ScaleToFit = value; WriteBack(); } }
    }

    public bool ShowHead {
        get => _cvc?.ShowHead ?? false;
        set { if (_cvc is { } cvc) { cvc.ShowHead = value; WriteBack(); } }
    }

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

        var scriptAll = new List<AbstractListItem>();
        var scriptRow = new List<AbstractListItem>();

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
            new FlexiControlForDelegate(tb),
            new FlexiControl(),
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
    /// Schreibt die bearbeitete Ansicht in <see cref="Table.ColumnArrangements" />
    /// zurück. Ansichten werden als serialisierte Daten verwaltet und beim
    /// Editieren über <see cref="ColumnViewCollection.ParseAll" /> als Arbeitskopie
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