// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueControls.Interfaces;

public interface ISupportsTextScale {

    #region Properties

    /// <summary>
    /// Absoluter Skalierfaktor der Vorgängerversion; TextScale ist relativ dazu (1 = Standard).
    /// </summary>
    const float LegacyScale = 3.07f;

    float TextScale { get; set; }

    #endregion
}