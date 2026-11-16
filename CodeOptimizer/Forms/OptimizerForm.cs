// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Forms;
using Form = BlueControls.Forms.Form;
using MessageBox = BlueControls.Forms.MessageBox;

namespace CodeOptimizer.Forms;

public sealed partial class OptimizerForm : Form {

    #region Fields

    /// <summary>
    /// Ordner, in dem die JSON-Einstellungsdateien liegen.
    /// </summary>
    private static readonly string SettingsFolder = Develop.AppPath() + "Settings\\";

    /// <summary>
    /// Datei, die den Pfad der zuletzt benutzten Einstellungsdatei enthält.
    /// </summary>
    private static readonly string LastUsedFile = SettingsFolder + "Zuletzt.json";

    /// <summary>
    /// Die zuletzt gespeicherte bzw. geladene Einstellungsdatei; leer, wenn noch nie gespeichert wurde.
    /// </summary>
    private string _lastFile = string.Empty;

    /// <summary>
    /// Der JSON-Stand beim letzten Speichern bzw. Laden; Basis der Prüfung auf ungespeicherte Änderungen.
    /// </summary>
    private string _savedJson = string.Empty;

    /// <summary>
    /// Das Plugin, dessen Optionen gerade angezeigt werden.
    /// </summary>
    private CodeOptimizerPlugin? _currentPlugin;

    #endregion

    #region Constructors

    public OptimizerForm() {
        InitializeComponent();
        FillPluginList();
        LoadLastUsed();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Bietet beim Schließen an, ungespeicherte Änderungen zu sichern.
    /// </summary>
    protected override void OnFormClosing(FormClosingEventArgs e) {
        base.OnFormClosing(e);
        if (e.Cancel) { return; }
        if (!HasUnsavedChanges()) { return; }

        var pressed = MessageBox.Show("Sollen die Änderungen gespeichert werden?", ImageCode.Warnung, "Ja", "Nein", "Abbrechen");
        if (pressed == 2) {
            e.Cancel = true;
            return;
        }

        if (pressed == 0 && !SaveProfile()) {
            e.Cancel = true;
        }
    }

    private void BtnStart_Click(object? sender, EventArgs e) {
        if (txbPath.Text is not { Length: > 0 } rootPath || !Directory.Exists(rootPath)) {
            MessageBox.Show("Bitte einen gültigen Ordner angeben.", ImageCode.Warnung, "Hinweis");
            return;
        }

        var plugins = CheckedPlugins();
        if (plugins.Count == 0) {
            MessageBox.Show("Kein Modul angewählt.", ImageCode.Warnung, "Hinweis");
            return;
        }

        var report = new StringBuilder();

        foreach (var plugin in plugins) {
            report.AppendLine();
            try {
                report.AppendLine(plugin.Execute(rootPath));
            } catch (Exception ex) {
                report.AppendLine("Fehler in Modul " + plugin.Name + ": " + ex.GetType().Name + ": " + ex.Message);
                report.AppendLine(ex.StackTrace);
            }
        }

        MessageBox.Show(report.ToString(), ImageCode.Information, "Fertig");
    }

    private void BtnLaden_Click(object? sender, EventArgs e) {
        if (!ConfirmUnsavedChanges()) { return; }

        using var dlg = new OpenFileDialog {
            Filter = "CodeOptimizer-Einstellungen (*.json)|*.json",
            InitialDirectory = SettingsFolder
        };

        if (dlg.ShowDialog(this) != DialogResult.OK) { return; }

        LoadFromFile(dlg.FileName);
    }

    private void BtnNeu_Click(object? sender, EventArgs e) {
        if (!ConfirmUnsavedChanges()) { return; }

        _lastFile = string.Empty;
        IO.DeleteFile(LastUsedFile, false);
        SetDefaults();
    }

    private void BtnSpeichern_Click(object? sender, EventArgs e) {
        SaveProfile();
    }

    private List<CodeOptimizerPlugin> CheckedPlugins() {
        var result = new List<CodeOptimizerPlugin>();

        foreach (var item in lstPlugins.CheckedItems) {
            if (item is ReadableListItem { Item: CodeOptimizerPlugin plugin }) { result.Add(plugin); }
        }

        result.Sort();
        return result;
    }

    private void FillPluginList() {
        var plugins = CodeOptimizerPlugin.AllPlugins.Instances.ToList();
        plugins.Sort();

        foreach (var plugin in plugins) {
            lstPlugins.ItemAdd(new ReadableListItem(plugin, true, plugin.KeyName));
            plugin.OptionsChanged += Plugin_OptionsChanged;
        }
    }

    private void Plugin_OptionsChanged(object? sender, EventArgs e) {
        if (_currentPlugin is null) { return; }
        if (!ReferenceEquals(sender, _currentPlugin)) { return; }

        ShowOptions(_currentPlugin);
    }

    private void LstPlugins_ItemClicked(object? sender, ListItemEventArgs e) {
        if (e.Item is not ReadableListItem { Item: CodeOptimizerPlugin plugin }) { return; }

        ShowOptions(plugin);
    }

    private void ShowOptions(CodeOptimizerPlugin plugin) {
        _currentPlugin = plugin;

        for (var i = pnlOptions.Controls.Count - 1; i >= 0; i--) {
            var control = pnlOptions.Controls[i];
            pnlOptions.Controls.RemoveAt(i);
            control.Dispose();
        }

        var options = plugin.GetOptions(pnlOptions.Width);

        if (options.Count == 0) {
            options.Add(new FlexiControl("Beschreibung:", pnlOptions.Width, true));
            options.Add(new FlexiControl(plugin.QuickInfo, pnlOptions.Width, false));
        }

        pnlOptions.ChildLayout = ChildLayout.StackVertical | ChildLayout.FullWidth | ChildLayout.Slider;

        foreach (var option in options) {
            if (option is { IsDisposed: false }) {
                pnlOptions.Controls.Add(option);
            }
        }
    }

    /// <summary>
    /// Baut die Optionsanzeige des aktuell angezeigten Plugins neu auf.
    /// </summary>
    private void RefreshOptions() {
        if (_currentPlugin is null) { return; }

        ShowOptions(_currentPlugin);
    }

    /// <summary>
    /// Lädt die zuletzt benutzte Einstellungsdatei; ohne Pfadangabe gelten die Standardwerte.
    /// </summary>
    private void LoadLastUsed() {
        var file = string.Empty;

        if (IO.FileExists(LastUsedFile)) {
            file = IO.ReadAllText(LastUsedFile).ParseAsJsonObject().GetString("File", string.Empty);
        }

        if (file is { Length: > 0 } && IO.FileExists(file)) {
            LoadFromFile(file);
            return;
        }

        SetDefaults();
    }

    private void LoadFromFile(string filename) {
        var json = IO.ReadAllText(filename).ParseAsJsonObject();

        ApplySettings(json);
        _lastFile = filename;
        WriteLastUsed(filename);
        _savedJson = CollectSettings().ToJsonString();
    }

    /// <summary>
    /// Setzt Zielordner, Plugin-Anwahl und Plugin-Einstellungen auf die Standardwerte.
    /// </summary>
    private void SetDefaults() {
        txbPath.Text = Environment.CurrentDirectory;
        lstPlugins.UncheckAll();

        foreach (var plugin in CodeOptimizerPlugin.AllPlugins.Instances) {
            plugin.LoadSettings(new JsonObject());
            lstPlugins.Check(plugin.KeyName);
        }

        RefreshOptions();
        _savedJson = CollectSettings().ToJsonString();
    }

    /// <summary>
    /// Übernimmt den kompletten Stand aus dem JSON und baut die Anzeige neu auf.
    /// </summary>
    private void ApplySettings(JsonObject settings) {
        if (settings.Count == 0) {
            SetDefaults();
            return;
        }

        txbPath.Text = settings.GetString("Path", Environment.CurrentDirectory);
        lstPlugins.UncheckAll();
        lstPlugins.Check(settings.GetStringList("Checked"), false);
        ApplyPluginSettings(settings);
        RefreshOptions();
    }

    private static void ApplyPluginSettings(JsonObject settings) {
        if (settings["Plugins"] is not JsonObject plugins) { return; }

        foreach (var plugin in CodeOptimizerPlugin.AllPlugins.Instances) {
            if (plugins[plugin.KeyName] is JsonObject json) {
                plugin.LoadSettings(json);
            }
        }
    }

    /// <summary>
    /// Liefert den kompletten aktuellen Stand als JSON.
    /// </summary>
    private JsonObject CollectSettings() {
        var checkedNames = new List<string>();
        foreach (var plugin in CheckedPlugins()) {
            checkedNames.Add(plugin.KeyName);
        }

        var allPlugins = CodeOptimizerPlugin.AllPlugins.Instances.ToList();
        allPlugins.Sort();

        var plugins = new JsonObject();
        foreach (var plugin in allPlugins) {
            plugins.Set(plugin.KeyName, plugin.SaveSettings());
        }

        var json = new JsonObject();
        json.Set("Path", txbPath.Text);
        json.Set("Checked", checkedNames);
        json.Set("Plugins", plugins);
        return json;
    }

    private bool HasUnsavedChanges() => CollectSettings().ToJsonString() != _savedJson;

    /// <summary>
    /// Bietet bei ungespeicherten Änderungen Sichern, Verwerfen oder Abbrechen an.
    /// False, wenn abgebrochen bzw. nicht gesichert werden konnte.
    /// </summary>
    private bool ConfirmUnsavedChanges() {
        if (!HasUnsavedChanges()) { return true; }

        var pressed = MessageBox.Show("Sollen die Änderungen gespeichert werden?", ImageCode.Warnung, "Ja", "Nein", "Abbrechen");

        if (pressed == 0) { return SaveProfile(); }
        return pressed == 1;
    }

    /// <summary>
    /// Speichert in die zuletzt benutzte Datei, sonst wird ein Speicherpfad abgefragt. False, wenn nicht gespeichert wurde.
    /// </summary>
    private bool SaveProfile() {
        if (_lastFile is { Length: > 0 }) { return WriteSettings(_lastFile); }
        return SaveProfileAs();
    }

    private bool SaveProfileAs() {
        using var dlg = new SaveFileDialog {
            Filter = "CodeOptimizer-Einstellungen (*.json)|*.json",
            InitialDirectory = SettingsFolder,
            FileName = _lastFile is { Length: > 0 } ? IO.FileNameWithSuffix(_lastFile) : "Einstellungen.json"
        };

        if (dlg.ShowDialog(this) != DialogResult.OK) { return false; }

        return WriteSettings(dlg.FileName);
    }

    private bool WriteSettings(string filename) {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var saved = IO.WriteAllText(filename, CollectSettings().ToJsonString(options), new UTF8Encoding(true), false);

        if (!saved.IsSuccessful) {
            MessageBox.Show("Speichern fehlgeschlagen.", ImageCode.Kritisch, "Ok");
            return false;
        }

        _lastFile = filename;
        WriteLastUsed(filename);
        _savedJson = CollectSettings().ToJsonString();
        return true;
    }

    private static void WriteLastUsed(string filename) {
        var json = new JsonObject();
        json.Set("File", filename);

        IO.CreateDirectory(SettingsFolder);
        IO.WriteAllText(LastUsedFile, json.ToJsonString(), new UTF8Encoding(true), false);
    }

    #endregion
}
