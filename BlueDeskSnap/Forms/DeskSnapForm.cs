// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueDeskSnap.Forms;

/// <summary>
/// Immer im Vordergrund liegende Liste der Desktop-Snaps: Plus-Knopf = erfassen, Minus-Knopf = alle Fenster minimieren, Stern = Auswahlmenü.
/// Klick auf einen Snap stellt ihn wieder her; solange kein Snap gewählt ist, sind die Quality-Plugins deaktiviert.
/// </summary>
public partial class DeskSnapForm : Form {

    #region Fields

    private int _nextSnapshotNumber = 1;

    /// <summary>
    /// Der Schlüssel des aktuell gewählten Snaps; leer, wenn kein Snap gewählt ist.
    /// </summary>
    private string _selectedSnapId = string.Empty;

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

    /// <summary>
    /// Wählt keinen Snap mehr und deaktiviert alle Quality-Plugins - funktional und sichtbar in der Liste.
    /// </summary>
    private void DisableQualityPlugins() {
        _selectedSnapId = string.Empty;

        foreach (var plugin in QualityPlugin.AllPlugins.Instances) {
            plugin.IsRunning = false;
        }

        FillQualityList();
    }

    private void FillList() {
        lstSnaps.ItemClear();

        var snaps = SnapshotStore.LoadAll();

        foreach (var snap in snaps) {
            var quickInfo = $"{snap.Created.ToString5()} | {snap.Windows.Count} Fenster";
            lstSnaps.ItemAdd(new TextListItem(snap.Name, snap.Id, null, false, true, quickInfo, $"{long.MaxValue - snap.Created.Ticks:d19}"));
        }

        _nextSnapshotNumber = NextSnapshotNumber(snaps);

        // Die getroffene Auswahl über den Neuaufbau der Liste hinweg halten
        if (_selectedSnapId.Length > 0 && lstSnaps[_selectedSnapId] is not null) { lstSnaps.Check(_selectedSnapId); }
    }

    /// <summary>
    /// Füllt die Plugin-Liste; die eingebaute Kästchen-Funktion zeigt den Aktiv-Status, der Klick schaltet um.
    /// </summary>
    private void FillQualityList() {
        lstQuality.ItemClear();

        foreach (var plugin in QualityPlugin.AllPlugins.Instances) {
            lstQuality.ItemAdd(new TextListItem(plugin.Name, plugin.KeyName, null, false, true, plugin.QuickInfo, plugin.Name));
        }

        lstQuality.Check(QualityPlugin.AllPlugins.Instances.Where(p => p.IsRunning).Select(p => p.KeyName), true);
    }

    /// <summary>
    /// Benennt den per Rechtsklick gewählten Snap um; Reihenfolge und Auswahl bleiben unverändert.
    /// </summary>
    private void ContextMenu_RenameSnap(object? sender, ContextMenuEventArgs e) {
        if (e.HotItem is not TextListItem item) { return; }
        if (SnapshotStore.Load(item.KeyName) is not { } snap) { return; }

        var newName = InputBox.Show("Neuer Name für den Snap", snap.Name, BlueBasics.Classes.Formats.TextFormat.Instance);

        if (string.IsNullOrWhiteSpace(newName) || string.Equals(newName, snap.Name, StringComparison.Ordinal)) { return; }

        snap.Name = newName.Trim();
        _selectedSnapId = snap.Id;
        SnapshotStore.Save(snap); // Der Snap behält seinen Platz in der Ablage
        FillList();
    }

    /// <summary>
    /// Erfasst den aktuellen Desktop erneut und ersetzt den Inhalt des gewählten Snaps.
    /// </summary>
    private void ContextMenu_UpdateSnap(object? sender, ContextMenuEventArgs e) {
        if (e.HotItem is not TextListItem item) { return; }
        if (SnapshotStore.Load(item.KeyName) is not { } snap) { return; }

        TakeSnapshot(snap);
    }

    /// <summary>
    /// Legt eine vollständige Kopie des gewählten Snaps an.
    /// </summary>
    private void ContextMenu_CopySnap(object? sender, ContextMenuEventArgs e) {
        if (e.HotItem is not TextListItem item) { return; }
        if (SnapshotStore.Load(item.KeyName) is not { } snap) { return; }

        _selectedSnapId = SnapshotStore.Copy(snap, snap.Name + " (Kopie)").Id;
        FillList();
    }

    /// <summary>
    /// Öffnet den Eigenschaften-Dialog des per Rechtsklick gewählten Quality-Plugins.
    /// </summary>
    private void ContextMenu_EditQualityProperties(object? sender, ContextMenuEventArgs e) {
        if (e.HotItem is not TextListItem it) { return; }
        if (QualityPlugin.AllPlugins.Instances.FirstOrDefault(p => string.Equals(p.KeyName, it.KeyName, StringComparison.OrdinalIgnoreCase)) is not ISimpleEditor plugin) { return; }

        InputBoxEditor.Edit(plugin, true);
    }

    private void LstSnaps_ItemClicked(object? sender, ListItemEventArgs e) {
        if (lstSnaps.Checked.Contains(e.Item.KeyName) && SnapshotStore.Load(e.Item.KeyName) is { } snap) {
            if (_selectedSnapId == snap.Id) { return; } // Bereits gewählt: kein erneutes Restore
            _selectedSnapId = snap.Id;
            DesktopSnapshot.Restore(snap);
            FillQualityList(); // Der Restore schaltet die Quality-Plugins gemäß dem Snapshot um
            return;
        }

        // Kein Snap mehr gewählt: die Quality-Plugins werden deaktiviert
        DisableQualityPlugins();
    }

    private void LstSnaps_RemoveClicked(object? sender, ListItemEventArgs e) {
        SnapshotStore.Delete(e.Item.KeyName);

        if (lstSnaps.Checked.Count == 0) {
            DisableQualityPlugins();
        }
    }

    /// <summary>
    /// Übernimmt die vom User verschobene Reihenfolge dauerhaft in die Ablage.
    /// </summary>
    private void LstSnaps_UpDownClicked(object? sender, SwapEventArgs e) {
        var snaps = SnapshotStore.LoadAll();
        List<WindowSnapshot> ordered = [];

        foreach (var item in lstSnaps.Items) {
            var snap = snaps.FirstOrDefault(s => string.Equals(s.Id, item.KeyName, StringComparison.OrdinalIgnoreCase));
            if (snap is not null) { ordered.Add(snap); }
        }

        SnapshotStore.SaveOrder(ordered);
    }

    private void LstQuality_CheckedChanged(object? sender, System.EventArgs e) {
        if (_selectedSnapId.Length == 0) {
            // Ohne gewählten Snap bleiben die Quality-Plugins deaktiviert
            lstQuality.UncheckAll();
            foreach (var plugin in QualityPlugin.AllPlugins.Instances) {
                plugin.IsRunning = false;
            }
            return;
        }

        foreach (var plugin in QualityPlugin.AllPlugins.Instances) {
            plugin.IsRunning = lstQuality.Checked.Contains(plugin.KeyName);
        }

        // Die geänderten Häkchen dauerhaft im gewählten Snap ablegen
        if (SnapshotStore.Load(_selectedSnapId) is { } snap) {
            snap.ActivePlugins.Clear();
            snap.ActivePlugins.AddRange(QualityPlugin.AllPlugins.Instances.Where(p => p.IsRunning).Select(p => p.KeyName));
            SnapshotStore.Save(snap);
        }
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

    private void BtnNeu_Click(object? sender, System.EventArgs e) => TakeSnapshot(new WindowSnapshot { Name = $"Snap {_nextSnapshotNumber}" });

    /// <summary>
    /// Minimiert alle Fenster außer diesem Werkzeug.
    /// </summary>
    private void BtnMinimizeAll_Click(object? sender, System.EventArgs e) => DesktopSnapshot.MinimizeAll();

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

    /// <summary>
    /// Erfasst den aktuellen Desktop in den gegebenen Snap; dieser wird gewählt und die Liste neu aufgebaut.
    /// </summary>
    private void TakeSnapshot(WindowSnapshot snap) {
        _ = DesktopSnapshot.CaptureInto(snap); // Die eigenen Fenster sind per Handle ausgeschlossen

        _selectedSnapId = snap.Id;
        FillList();
    }

    protected override void OnLoad(System.EventArgs e) {
        base.OnLoad(e);
        FillList();
        DisableQualityPlugins(); // Ohne gewählten Snap sind die Quality-Plugins aus
    }

    #endregion
}
