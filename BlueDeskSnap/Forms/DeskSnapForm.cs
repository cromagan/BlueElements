// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueDeskSnap.Forms;

/// <summary>
/// Immer im Vordergrund liegende Liste der Desktop-Snaps: Plus-Knopf = erfassen, Klick = wiederherstellen, Rechtsklick = Umbenennen/Aktualisieren/Kopieren, Stern = Auswahlmenü öffnen.
/// </summary>
public partial class DeskSnapForm : Form {

    #region Fields

    private int _nextSnapshotNumber = 1;

    #endregion

    #region Constructors

    public DeskSnapForm() {
        InitializeComponent();

        lstSnaps.CustomContextMenuItems = new([
            ListItemExtension.ItemOf("Umbenennen", ImageCode.Stift, ContextMenu_RenameSnap, true),
            ListItemExtension.ItemOf("Snap aktualisieren", ImageCode.Refresh, ContextMenu_UpdateSnap, true),
            ListItemExtension.ItemOf("Snap kopieren", ImageCode.Kopieren, ContextMenu_CopySnap, true)
        ]);

        lstQuality.CustomContextMenuItems = new([
            ListItemExtension.ItemOf("Eigenschaften bearbeiten", ImageCode.Stift, ContextMenu_EditQualityProperties, true)
        ]);
    }

    #endregion

    #region Methods

    [StandaloneInfo("Desktop-Snaps", ImageCode.Kamera, "Allgemein", "Fensteranordnungen sichern und wiederherstellen", 899)]
    public static System.Windows.Forms.Form Start() => new DeskSnapForm();

    private void FillList() {
        lstSnaps.ItemClear();

        var snaps = SnapshotStore.LoadAll();

        foreach (var snap in snaps) {
            var quickInfo = $"{snap.Created.ToString5()} | {snap.Windows.Count} Fenster";
            lstSnaps.ItemAdd(new TextListItem(snap.Name, snap.Folder, null, false, true, quickInfo, $"{long.MaxValue - snap.Created.Ticks:d19}"));
        }

        _nextSnapshotNumber = NextSnapshotNumber(snaps);
    }

    private void LstSnaps_ItemClicked(object? sender, ListItemEventArgs e) {
        if (SnapshotStore.Load(e.Item.KeyName) is { } snap) {
            DesktopSnapshot.Restore(snap);
            FillQualityList(); // Der Restore schaltet die Quality-Plugins gemäß dem Snapshot um
        }
    }

    private void LstSnaps_RemoveClicked(object? sender, ListItemEventArgs e) => SnapshotStore.DeleteFolder(e.Item.KeyName);

    /// <summary>
    /// Benennt den per Rechtsklick gewählten Snap um.
    /// </summary>
    private void ContextMenu_RenameSnap(object? sender, ContextMenuEventArgs e) {
        if (e.HotItem is not TextListItem item) { return; }
        if (SnapshotStore.Load(item.KeyName) is not { } snap) { return; }

        var newName = InputBox.Show("Neuer Name für den Snap", snap.Name, BlueBasics.Classes.Formats.TextFormat.Instance);

        if (string.IsNullOrWhiteSpace(newName) || string.Equals(newName, snap.Name, StringComparison.Ordinal)) { return; }

        snap.Name = newName.Trim();
        SnapshotStore.Save(snap);
        FillList();
    }

    /// <summary>
    /// Erfasst den aktuellen Desktop erneut und ersetzt den Inhalt des gewählten Snaps.
    /// </summary>
    private void ContextMenu_UpdateSnap(object? sender, ContextMenuEventArgs e) {
        if (e.HotItem is not TextListItem item) { return; }
        if (SnapshotStore.Load(item.KeyName) is not { } snap) { return; }

        TakeSnapshot(snap.Name);
        SnapshotStore.DeleteFolder(snap.Folder);
        FillList();
    }

    /// <summary>
    /// Legt eine vollständige Kopie des gewählten Snaps an.
    /// </summary>
    private void ContextMenu_CopySnap(object? sender, ContextMenuEventArgs e) {
        if (e.HotItem is not TextListItem item) { return; }
        if (SnapshotStore.Load(item.KeyName) is not { } snap) { return; }

        SnapshotStore.Copy(snap, snap.Name + " (Kopie)");
        FillList();
    }

    /// <summary>
    /// Füllt die Plugin-Liste; die eingebaute Kästchen-Funktion zeigt den Aktiv-Status, der Klick schaltet um.
    /// </summary>
    private void FillQualityList() {
        lstQuality.ItemClear();

        lstQuality.ItemAdd(new TextListItem("Quality-Plugins", "QualityPlugins", null, true, true, "Klick auf ein Plugin schaltet es ein oder aus", "0"));

        foreach (var plugin in QualityPlugin.AllPlugins.Instances) {
            lstQuality.ItemAdd(new TextListItem(plugin.Name, plugin.KeyName, null, false, true, plugin.QuickInfo, plugin.Name));
        }

        lstQuality.Check(QualityPlugin.AllPlugins.Instances.Where(p => p.IsRunning).Select(p => p.KeyName), true);
    }

    private void LstQuality_CheckedChanged(object? sender, System.EventArgs e) {
        foreach (var plugin in QualityPlugin.AllPlugins.Instances) {
            plugin.IsRunning = lstQuality.Checked.Contains(plugin.KeyName);
        }
    }

    /// <summary>
    /// Öffnet den Eigenschaften-Dialog des per Rechtsklick gewählten Quality-Plugins.
    /// </summary>
    private void ContextMenu_EditQualityProperties(object? sender, ContextMenuEventArgs e) {
        if (e.HotItem is not TextListItem it) { return; }
        if (QualityPlugin.AllPlugins.Instances.FirstOrDefault(p => string.Equals(p.KeyName, it.KeyName, StringComparison.OrdinalIgnoreCase)) is not ISimpleEditor plugin) { return; }

        InputBoxEditor.Edit(plugin, true);
    }

    private void TmrQuality_Tick(object? sender, System.EventArgs e) {
        foreach (var plugin in QualityPlugin.AllPlugins.Instances) {
            try {
                plugin.Tick();
            } catch (Exception ex) {
                // Ein fehlerhaftes Plugin darf die App nicht destabilisieren
                Develop.DebugPrint("Quality-Plugin-Fehler: " + plugin.KeyName, ex);
            }
        }
    }

    private void BtnNeu_Click(object? sender, System.EventArgs e) => TakeSnapshot($"Snap {_nextSnapshotNumber}");

    /// <summary>
    /// Öffnet das Auswahlmenü, ohne dieses Werkzeugfenster zu schließen.
    /// </summary>
    private void BtnMenu_Click(object? sender, System.EventArgs e) {
        FormManager.OpenLastMenu();
    }

    /// <summary>
    /// Ermittelt die nächste freie Nummer hinter den vorhandenen "Snap &lt;Nr&gt;"-Namen.
    /// </summary>
    private static int NextSnapshotNumber(List<WindowSnapshot> snaps) {
        var max = 0;

        for (var i = 0; i < snaps.Count; i++) {
            var name = snaps[i].Name;
            if (!name.StartsWith("Snap ", StringComparison.OrdinalIgnoreCase)) { continue; }
            max = Math.Max(max, IntParse(name["Snap ".Length..].Trim()));
        }

        return max + 1;
    }

    private void TakeSnapshot(string name) {
        Opacity = 0; // Dieses Fenster blendet sich für die Erfassung aus
        Generic.Pause(0.3, true);
        try {
            _ = DesktopSnapshot.Capture(name);
        } finally {
            Opacity = 1;
        }

        FillList();
    }

    protected override void OnLoad(System.EventArgs e) {
        base.OnLoad(e);
        FillList();
        FillQualityList();
    }

    #endregion
}
