// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Text;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.Designer_Support;
using BlueControls.EventArgs;
using static BlueBasics.ClassesStatic.IO;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Controls;

/// <summary>
/// Ein ComboBox-Control zur Anzeige und Verwaltung der zuletzt verwendeten Dateien.
/// </summary>
[Designer(typeof(BasicDesigner))]
[DefaultEvent(nameof(ItemClicked))]
public sealed class LastFilesCombo : ComboBox, IHasSettings {

    #region Constructors

    public LastFilesCombo() : base() {
        SetLastFilesStyle();
        RemoveAllowed = true;

        CustomContextMenuItems = new List<AbstractListItem> {
            ItemOf("Dateipfad öffnen", QuickImage.Get(BlueBasics.Enums.ImageCode.Ordner), ContextMenu_OpenPath, true, string.Empty)
        }.AsReadOnly();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Maximale Anzahl der anzuzeigenden Einträge.
    /// </summary>
    [DefaultValue(20)]
    public int MaxCount {
        get;
        set {
            if (field == value) { return; }
            field = value;
            GenerateMenu();
        }
    } = 20;

    /// <summary>
    /// Gibt an, ob die Datei physisch existieren muss, um angezeigt zu werden.
    /// </summary>
    [DefaultValue(true)]
    public bool MustExist {
        get;
        set {
            if (field == value) { return; }
            field = value;
            GenerateMenu();
        }
    } = true;

    public List<string> Settings { get; } = [];

    public bool SettingsLoaded { get; set; }

    /// <summary>
    /// Pfad zur Datei, in der die Historie gespeichert wird.
    /// </summary>
    [DefaultValue("")]
    public string SettingsManualFilename {
        get;
        set {
            if (field == value) { return; }
            field = value;
            this.LoadSettingsFromDisk(true);
            GenerateMenu();
        }
    } = string.Empty;

    public bool UsesSettings => true;

    #endregion

    #region Methods

    /// <summary>
    /// Fügt einen neuen Dateinamen zur Liste hinzu.
    /// </summary>
    public void AddFileName(string? fileName, string additionalText) {
        if (fileName is null) { return; }

        if (!MustExist || FileExists(fileName)) {
            this.SettingsAdd($"{fileName}|{additionalText}");
        }

        GenerateMenu();
    }

    protected override void DrawControl(Graphics gr, States state) {
        if (IsDisposed) { return; }
        base.DrawControl(gr, state);
    }

    protected override void OnCreateControl() {
        base.OnCreateControl();
        this.LoadSettingsFromDisk(false);
        GenerateMenu();
    }

    protected override void OnItemClicked(AbstractListItemEventArgs e) {
        base.OnItemClicked(e);
        if (e.Item?.KeyName is not { } keyName) { return; }

        var entry = Settings
            .Select(s => s.SplitAndCutBy("|"))
            .FirstOrDefault(x => x.Length > 0 && x[0] == keyName);

        if (entry is not null) {
            var additional = entry.Length > 1 && !string.IsNullOrEmpty(entry[1]) ? entry[1] : string.Empty;
            AddFileName(keyName, additional);
        }

        Text = string.Empty;
    }

    protected override void OnItemRemoved(AbstractListItemEventArgs e) {
        base.OnItemRemoved(e);
        if (e.Item?.KeyName is null) { return; }

        this.SettingsRemoveByKey(e.Item.KeyName, "|");
        GenerateMenu();
    }

    private void ContextMenu_OpenPath(object? sender, ContextMenuEventArgs e) {
        if (e.HotItem is AbstractListItem ali) {
            ExecuteFile(ali.KeyName.FilePath());
        }
    }

    /// <summary>
    /// Generiert die Menüeinträge basierend auf den aktuellen Einstellungen.
    /// </summary>
    private void GenerateMenu() {
        ItemClear();
        var validEntries = Settings
            .AsEnumerable()
            .Reverse()
            .Select(s => s.SplitAndCutBy("|"))
            .Where(x => x.Length > 0 && !string.IsNullOrEmpty(x[0]) && base[x[0]] is null)
            .Where(x => !MustExist || FileExists(x[0]))
            .Take(MaxCount)
            .ToList();

        for (var i = 0; i < validEntries.Count; i++) {
            var x = validEntries[i];
            var sb = new StringBuilder();

            sb.Append((i + 1).ToString3()).Append(": ");
            sb.Append(MustExist ? x[0].FileNameWithSuffix() : x[0]);

            if (x.Length > 1 && !string.IsNullOrEmpty(x[1])) {
                sb.Append(" - ").Append(x[1]);
            }

            ItemAdd(new TextListItem(sb.ToString(), x[0], null, false, true, string.Empty, i.ToString3()));
        }

        Enabled = validEntries.Count > 0;
    }

    private void SetLastFilesStyle() {
        if (DrawStyle == ComboboxStyle.TextBox) { DrawStyle = ComboboxStyle.Button; }
        if (string.IsNullOrEmpty(ImageCode)) { ImageCode = "Ordner"; }
        if (string.IsNullOrEmpty(Text)) { Text = "zuletzt geöffnete Dateien"; }
    }

    #endregion
}