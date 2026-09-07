// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

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