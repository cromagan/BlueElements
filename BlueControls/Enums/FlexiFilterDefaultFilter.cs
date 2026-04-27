// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueTable.Enums;

namespace BlueControls.Enums;

public enum FlexiFilterDefaultFilter {

    /// <summary>
    ///Wird ein Text eingegeben, wird ein Filter Instr_GroßKleinEgal erzeugt
    /// </summary>
    Textteil = FilterType.Instr_GroßKleinEgal,

    /// <summary>
    /// Wird ein Text eingegeben, wird ein Filter Istgleich_ODER_GroßKleinEgal erzeugt
    /// </summary>
    Istgleich = FilterType.Istgleich_ODER_GroßKleinEgal
}