// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueControls.Interfaces;

/// <summary>
/// Wird vermendet, wenn das Element sein Aussehen verändern kann - mittels StyleDB
/// </summary>
public interface IStyleable {

    #region Properties

    string SheetStyle { get; }

    #endregion
}