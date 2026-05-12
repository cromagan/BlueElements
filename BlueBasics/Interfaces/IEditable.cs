// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Interfaces;

/// <summary>
/// Benötigt einen Gegenpart des Typs IIsEditor. EditorEasy würde sich anbieten.
/// IEditable ist optional - Editoren können auch Objekte ohne dieses Interface bearbeiten,
/// dann fehlen jedoch die IsNowEditable-Prüfung und die CaptionForEditor-Anzeige.
/// </summary>
public interface IEditable {

    #region Properties

    string CaptionForEditor { get; }

    #endregion

    #region Methods

    string IsNowEditable();

    #endregion
}