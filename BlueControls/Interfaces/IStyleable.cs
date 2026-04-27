// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Interfaces;

/// <summary>
/// Wird vermendet, wenn das Element sein Aussehen verändern kann - mittels StyleDB
/// </summary>
public interface IStyleable {

    #region Properties

    string SheetStyle { get; }

    #endregion
}