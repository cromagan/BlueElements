// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Text.Json;
using System.Text.Json.Nodes;
using Form = BlueControls.Forms.Form;
using MessageBox = BlueControls.Forms.MessageBox;

namespace CodeOptimizer.Forms;

public sealed partial class OptimizerForm : Form {

    #region Fields

    /// <summary>
    /// Ordner, in dem die Einstellungsdatei liegt.
    /// </summary>
    private static readonly string SettingsFolder = Develop.AppPath() + "Settings\\";

    /// <summary>
    /// Die eine Einstellungsdatei, die immer geladen und immer geschrieben wird.
    /// </summary>
    private static readonly string SettingsFile = SettingsFolder + "CodeOptimizer.json";

    /// <summary>
    /// Das Plugin, dessen Optionen gerade angezeigt werden.
    /// </summary>
    private CodeOptimizerPlugin? _currentPlugin;

    /// <summary>
    /// Unterdrückt das automatische Speichern, während Einstellungen geladen werden.
    /// </summary>
    private bool _loading;

    #endregion

    #region Constructors

    public OptimizerForm() {
        InitializeComponent();
        FillPluginList();
        txbPath.TextChanged += TxbPath_TextChanged;
        lstPlugins.ItemCheckedChanged += LstPlugins_ItemCheckedChanged;
        LoadSettings();
    }

    #endregion

    #region Methods

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
                report.AppendLine("=== " + plugin.Name + " ===");
                report.AppendLine("Fehler: " + ex.GetType().Name + ": " + ex.Message);
                report.AppendLine(ex.StackTrace);
            }
        }

        using var result = new ResultForm(report.ToString());
        result.ShowDialog();
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
        SaveSettings();

        if (_currentPlugin is null) { return; }
        if (!ReferenceEquals(sender, _currentPlugin)) { return; }

        ShowOptions(_currentPlugin);
    }

    private void LstPlugins_ItemClicked(object? sender, ListItemEventArgs e) {
        if (e.Item is not ReadableListItem { Item: CodeOptimizerPlugin plugin }) { return; }

        ShowOptions(plugin);
    }

    private void LstPlugins_ItemCheckedChanged(object? sender, EventArgs e) => SaveSettings();

    private void TxbPath_TextChanged(object? sender, EventArgs e) => SaveSettings();

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

        foreach (var option in options.Where(static option => option is { IsDisposed: false })) {
            pnlOptions.Controls.Add(option);
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
    /// Lädt immer die Einstellungsdatei; ohne Datei gelten die Standardwerte.
    /// </summary>
    private void LoadSettings() {
        _loading = true;

        if (IO.FileExists(SettingsFile)) {
            ApplySettings(IO.ReadAllText(SettingsFile).ParseAsJsonObject());
        } else {
            SetDefaults();
        }

        _loading = false;
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
        var checkedNames = CheckedPlugins().Select(static plugin => plugin.KeyName).ToList();

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

    /// <summary>
    /// Schreibt den kompletten aktuellen Stand in die Einstellungsdatei.
    /// </summary>
    private void SaveSettings() {
        if (_loading) { return; }

        var options = new JsonSerializerOptions { WriteIndented = true };
        IO.CreateDirectory(SettingsFolder);
        var saved = IO.WriteAllText(SettingsFile, CollectSettings().ToJsonString(options), new UTF8Encoding(true), false);

        if (!saved.IsSuccessful) {
            MessageBox.Show("Speichern fehlgeschlagen.", ImageCode.Kritisch, "Ok");
        }
    }

    #endregion
}
