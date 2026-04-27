// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Enums;

public enum FlexiFilterDefaultOutput {

    /// <summary>
    /// Wir nichts eingegeben, wird kein Filter zurück gegeben.
    /// Sozusagen ist das das ein AlwaysTrue Filter
    /// </summary>
    Alles_Anzeigen = 0,

    /// <summary>
    /// Wird nichts eingegeben wird keine Zeile gezeigt. Es muss also was eingegeben werden
    /// </summary>
    Nichts_Anzeigen = FilterType.AlwaysFalse
}