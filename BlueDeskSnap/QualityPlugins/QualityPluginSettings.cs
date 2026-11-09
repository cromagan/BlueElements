// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Text.Json.Nodes;
using static BlueBasics.ClassesStatic.IO;

namespace BlueDeskSnap.QualityPlugins;

/// <summary>
/// Dauerhafte Einstellungen der Quality-Plugins: eine JSON-Datei mit einem Block je Plugin, sortiert nach KeyName.
/// </summary>
public static class QualityPluginSettings {

    #region Fields

    private static readonly string _file = @"%appdocumentpath%\DeskSnaps\QualityPlugins.json".NormalizePath();

    #endregion

    #region Methods

    /// <summary>
    /// Liest den Einstellungs-Block des Plugins; fehlt er, wird ein leeres Objekt geliefert.
    /// </summary>
    public static JsonObject Load(string keyName) => All().GetJson(keyName) as JsonObject ?? new JsonObject();

    /// <summary>
    /// Schreibt den Einstellungs-Block des Plugins zurück und legt Ordner und Datei bei Bedarf neu an.
    /// </summary>
    public static void Save(string keyName, JsonObject settings) {
        var all = All();
        all.Set(keyName, settings);

        if (Path.GetDirectoryName(_file) is { } folder) {
            _ = Directory.CreateDirectory(folder);
        }

        WriteAllText(_file, all.ToJsonString(), Encoding.UTF8, false);
    }

    private static JsonObject All() => FileExists(_file) ? ReadAllText(_file).ParseAsJsonObject() : new JsonObject();

    #endregion
}
