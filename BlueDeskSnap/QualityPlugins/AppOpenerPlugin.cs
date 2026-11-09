// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Collections.ObjectModel;
using System.Text.Json.Nodes;
using static BlueBasics.ClassesStatic.IO;

namespace BlueDeskSnap.QualityPlugins;

/// <summary>
/// Öffnet die eingetragenen Anwendungen und Dateien automatisch, sobald sie noch nicht geöffnet sind.
/// </summary>
public sealed class AppOpenerPlugin : QualityPlugin, ISimpleEditor {

    #region Fields

    /// <summary>
    /// Einträge, die gestartet wurden, seitdem sie zuletzt als offen erkannt wurden. Verhindert, dass noch nicht erkannte Starts im 3-Sekunden-Takt wiederholt werden.
    /// </summary>
    private readonly HashSet<string> _started = new(StringComparer.OrdinalIgnoreCase);

    private List<string> _dateien = [];

    #endregion

    #region Constructors

    public AppOpenerPlugin() => _dateien = DateienOfSettings();

    #endregion

    #region Events

    public event EventHandler? DoUpdateSideOptionMenu;

    #endregion

    #region Properties

    public override string KeyName => "AppOpener";

    public override string Name => "Anwendungen öffnen";

    public override string QuickInfo => "Öffnet die eingetragenen Anwendungen und Dateien automatisch, sobald sie noch nicht geöffnet sind.";

    public override ImageCode Symbol => ImageCode.Anwendung;

    /// <summary>
    /// Die Anwendungen und Dateien, die geöffnet werden, solange sie noch nicht geöffnet sind. Eine pro Zeile.
    /// </summary>
    public ReadOnlyCollection<string> Dateien {
        get => _dateien.AsReadOnly();
        set {
            var neu = CleanOf(value);
            if (neu.IsDifferentTo(_dateien)) {
                _dateien = neu;
                SaveSettings();
            }
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Eigenschaften-Dialog: die Liste der zu öffnenden Dateien als mehrzeiliges Feld, eine Datei pro Zeile.
    /// </summary>
    public List<GenericControl> GetProperties(int widthOfControl) => [
        new FlexiControlForProperty<ReadOnlyCollection<string>>(() => Dateien, "Zu öffnende Dateien (eine pro Zeile)", 8, null, CheckBehavior.SingleSelection, AddType.None, false)
    ];

    protected override void ExecuteTick() {
        foreach (var datei in _dateien) {
            if (IsOpen(datei)) {
                _ = _started.Remove(datei);
                continue;
            }

            // Starten nur, solange dieser Eintrag seit dem letzten "nicht offen" noch nicht versucht wurde
            if (_started.Add(datei)) {
                _ = ExecuteFile(datei);
            }
        }
    }

    private static List<string> CleanOf(IEnumerable<string> dateien) => [.. dateien.Select(d => d.Trim()).Where(d => d.Length > 0)];

    private List<string> DateienOfSettings() => CleanOf(QualityPluginSettings.Load(KeyName).GetStringList("dateien"));

    private void SaveSettings() {
        JsonObject json = new();
        json.Set("dateien", _dateien);
        QualityPluginSettings.Save(KeyName, json);
    }

    /// <summary>
    /// True, wenn die Anwendung als Prozess läuft bzw. die Datei laut Fenstertitel geöffnet ist.
    /// </summary>
    private static bool IsOpen(string datei) {
        if (datei.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) || !datei.Contains('.', StringComparison.Ordinal)) {
            return Process.GetProcessesByName(Path.GetFileNameWithoutExtension(datei)).Length > 0;
        }

        var such = Path.GetFileName(datei);

        foreach (var p in Process.GetProcesses()) {
            try {
                if (p.MainWindowTitle.Contains(such, StringComparison.OrdinalIgnoreCase)) { return true; }
            } catch {
                // Der Prozess kann zwischenzeitlich beendet sein
            }
        }

        return false;
    }

    #endregion
}
