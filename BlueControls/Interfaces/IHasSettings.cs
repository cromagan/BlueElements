// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using BlueBasics;
using BlueBasics.Classes.FileHelpers;
using BlueBasics.ClassesStatic;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using static BlueBasics.ClassesStatic.IO;

namespace BlueControls.Interfaces;

public interface IHasSettings {

    #region Properties

    string Name { get; }
    TextFileHelper Settings { get; }
    bool SettingsLoaded { get; set; }
    string SettingsManualFilename { get; set; }
    bool UsesSettings { get; }

    #endregion
}

public static class HasSettings {

    #region Methods

    public static string GetSettings(this IHasSettings settings, string tagname) {
        if (!settings.UsesSettings) { return string.Empty; }

        settings.LoadSettingsFromDisk(false);

        return settings.Settings.TagGet(tagname).FromNonCritical();
    }

    public static void LoadSettingsFromDisk(this IHasSettings settings, bool loadalways) {
        if (settings.SettingsLoaded && !loadalways) { return; }
        if (!settings.UsesSettings) { return; }

        settings.Settings.Clear();

        var fileName = settings.SettingsFileName();
        if (FileExists(fileName)) {
            var t = ReadAllText(fileName, Encoding.UTF8);
            settings.Settings.ParseContent(t);

            settings.SettingsLoaded = true;
        }
    }

    public static void SaveSettingsToDisk(this IHasSettings settings) {
        if (Develop.AllReadOnly) { return; }
        if (!settings.UsesSettings) { return; }

        var fileName = settings.SettingsFileName();
        var pf = fileName.FilePath().NormalizePath();

        if (!string.IsNullOrEmpty(CanWriteInDirectory(pf.PathParent()))) { return; }
        CreateDirectory(pf);

        if (!string.IsNullOrEmpty(CanWriteInDirectory(pf))) { return; }

        // Nutzt FinishParseable() des jeweiligen Helpers (Ini oder später XML/Json)
        WriteAllText(fileName, settings.Settings.FinishParseable(), Encoding.UTF8, false);
        settings.SettingsLoaded = true;
    }

    public static void SetSetting(this IHasSettings settings, string tagname, string value) {
        if (!settings.UsesSettings) { return; }
        settings.LoadSettingsFromDisk(false);

        var nval = value.ToNonCritical();

        if (settings.GetSettings(tagname) != nval) {
            settings.Settings.TagSet(tagname, nval);
            settings.SaveSettingsToDisk();
        }
    }

    /// <summary>
    /// Fügt dem Settings eine Datei am Ende hinzu.
    /// Falls die Einstellung vorher schon vorhanden ist, wird diese entfernt.
    /// Perfekt für Auflistungen.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="s"></param>
    public static void SettingsAdd(this IHasSettings settings, string s) {
        if (!settings.UsesSettings) { return; }
        settings.LoadSettingsFromDisk(false);

        s = s.Replace("\r", string.Empty).Replace("\n", string.Empty);

        string? existingKey = null;
        var maxKey = -1;

        // Da TextFileHelper nun IEnumerable<XElement> ist:
        foreach (XElement element in settings.Settings) {
            var keyName = element.Name.LocalName;
            if (int.TryParse(keyName, out var k)) {
                if (k > maxKey) { maxKey = k; }
                if (element.Value == s) { existingKey = keyName; }
            }
        }

        // Schon der aktuellste Eintrag → nichts tun
        if (existingKey != null && existingKey == maxKey.ToString()) { return; }

        if (existingKey != null) { settings.Settings.TagRemove(existingKey); }
        settings.Settings.TagSet((maxKey + 1).ToString(), s);

        settings.SaveSettingsToDisk();
    }

    /// <summary>
    /// Gibt die via SettingsAdd gespeicherten Werte als Liste zurück, sortiert vom ältesten zum neuesten.
    /// </summary>
    public static List<string> SettingsList(this IHasSettings settings) {
        var result = new List<(int key, string value)>();

        // Iteration über XElements
        foreach (XElement element in settings.Settings) {
            if (int.TryParse(element.Name.LocalName, out var k)) {
                result.Add((k, element.Value));
            }
        }

        return result.OrderBy(x => x.key).Select(x => x.value).ToList();
    }

    private static string SettingsFileName(this IHasSettings settings) {
        if (!settings.UsesSettings) { return string.Empty; }

        return !string.IsNullOrEmpty(settings.SettingsManualFilename) ?
            settings.SettingsManualFilename.NormalizeFile() :
            $"%homepath%\\{Develop.AppName()}\\{settings.Name}.ini".NormalizeFile();
    }

    #endregion
}