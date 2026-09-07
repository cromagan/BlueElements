// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

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