// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Text.Json.Nodes;

namespace CodeOptimizer.Classes;

/// <summary>
/// Eine Ersetzungs-Regel: Suchtext, Ersetzung und ob der Suchtext als regulärer Ausdruck behandelt wird.
/// </summary>
public sealed class ReplaceRule(string alt, string neu, bool isRegex) {

    #region Properties

    /// <summary>
    /// Der Suchtext bzw. der reguläre Ausdruck.
    /// </summary>
    public string Alt { get; set; } = alt;

    /// <summary>
    /// Der Ersatz. Bei regulären Ausdrücken können $1-Gruppen verwendet werden.
    /// </summary>
    public string Neu { get; set; } = neu;

    /// <summary>
    /// True, wenn Alt als regulärer Ausdruck interpretiert wird.
    /// </summary>
    public bool IsRegex { get; set; } = isRegex;

    #endregion

    #region Methods

    /// <summary>
    /// Liefert die Regel als JSON.
    /// </summary>
    public JsonObject ToJsonObject() {
        var json = new JsonObject();
        json.Set("Alt", Alt);
        json.Set("Neu", Neu);
        json.Set("Regex", IsRegex);
        return json;
    }

    /// <summary>
    /// Erzeugt eine Regel aus dem JSON; fehlende Werte werden als Leerwerte übernommen.
    /// </summary>
    public static ReplaceRule Parse(JsonObject json) => new(json.GetString("Alt", string.Empty), json.GetString("Neu", string.Empty), json.GetBool("Regex"));

    #endregion
}
