// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.Interfaces;

public interface IHasScript {

    #region Methods

    /// <summary>
    /// Liefert Name (KeyName) und Text aller Skripte dieses Objekts.
    /// </summary>
    IEnumerable<ScriptDescription> GetAllScripts();

    #endregion
}