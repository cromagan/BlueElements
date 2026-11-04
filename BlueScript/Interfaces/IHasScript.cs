// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.Interfaces;

public interface IHasScript {

    #region Methods

    /// <summary>
    /// Liefert Name (KeyName) und Text aller Skripte dieses Objekts.
    /// </summary>
    IEnumerable<ScriptDescription> GetAllScripts();

    /// <summary>
    /// Setzt den neuen Skripttext für das angegebene Skript und meldet, ob es geändert wurde.
    /// </summary>
    bool SetScript(ScriptDescription script, string newScript);

    #endregion
}

public static class IHasScriptExtension {

    #region Methods

    /// <summary>
    /// Ersetzt in allen Skripten dieses Objekts den Suchtext und liefert die Anzahl geänderter Skripte.
    /// </summary>
    public static int ReplaceInScripts(this IHasScript hasScript, string suchText, string ersetzText) {
        var count = 0;

        foreach (var sd in hasScript.GetAllScripts()) {
            if (sd.Script is not { Length: > 0 } s) { continue; }

            var newScript = s.Replace(suchText, ersetzText);
            if (newScript == s) { continue; }

            if (!hasScript.SetScript(sd, newScript)) { continue; }
            count++;
        }

        return count;
    }

    #endregion
}