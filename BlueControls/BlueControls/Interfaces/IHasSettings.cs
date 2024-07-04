// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

#nullable enable

using BlueBasics;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime;
using System.Text;

using BlueBasics;

using BlueControls.Designer_Support;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;

using System.Collections.Generic;

using System.ComponentModel;

using System.Drawing;
using System.IO;
using System.Text;

using System.Windows.Forms;
using static BlueBasics.IO;

namespace BlueControls.Interfaces;

public interface IHasSettings {

    #region Properties

    public string Name { get; }
    public List<string> Settings { get; }
    public bool SettingsLoaded { get; set; }
    public string SettingsManualFilename { get; set; }

    #endregion
}

public static class HasSettings {

    #region Methods

    public static string GetSettings(this IHasSettings settings, string tagname) {
        settings.LoadSettingsFromDisk(false);

        return settings.Settings.TagGet(tagname).FromNonCritical();
    }

    public static void LoadSettingsFromDisk(this IHasSettings settings, bool loadalways) {
        if (settings.SettingsLoaded && !loadalways) { return; }

        settings.Settings.Clear();

        if (FileExists(settings.SettingsFileName())) {
            var t = File.ReadAllText(settings.SettingsFileName(), Encoding.UTF8);
            t = t.RemoveChars("\n");
            settings.Settings.AddRange(t.SplitAndCutByCr());

            settings.SettingsLoaded = true;
        }
    }

    public static void SaveSettingsToDisk(this IHasSettings settings) {
        var pf = settings.SettingsFileName().FilePath().CheckPath();

        if (string.IsNullOrEmpty(pf)) { return; }

        if (!DirectoryExists(pf)) {
            _ = Directory.CreateDirectory(pf);
        }

        if (CanWriteInDirectory(pf)) {
            settings.Settings.WriteAllText(settings.SettingsFileName(), Encoding.UTF8, false);
            settings.SettingsLoaded = true;
        }
    }

    public static void SetSetting(this IHasSettings settings, string tagname, string value) {
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
        settings.LoadSettingsFromDisk(false);

        s = s.Replace("\r", string.Empty).Replace("\n", string.Empty);
        if (settings.Settings.Count > 0) { settings.Settings.RemoveString(s, false); }
        settings.Settings.Add(s);

        settings.SaveSettingsToDisk();
    }

    private static string SettingsFileName(this IHasSettings settings) => !string.IsNullOrEmpty(settings.SettingsManualFilename) ? settings.SettingsManualFilename.CheckFile() : ("%homepath%\\" + Develop.AppName() + "\\" + settings.Name + ".ini").CheckFile();

    #endregion
}