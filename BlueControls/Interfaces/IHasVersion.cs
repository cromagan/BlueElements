// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Interfaces;

public interface IHasVersion : IHasKeyName {

    #region Properties

    int Version { get; set; }

    #endregion
}

public static class HasVersionExtensions {

    #region Methods

    public static string DefaultItemToControlName(this IHasVersion item, string? parentName) {
        if (parentName is null) {
            return item.KeyName + "-" + item.Version + "-[UNKNOWN]";
        }

        return item.KeyName + "-" + item.Version + "-" + parentName.GetMD5Hash();
    }

    /// <summary>
    /// Erhoeht die Version, damit verknuepfte Controls ihr Layout neu aufbauen.
    /// Waehrend des Parsens / Initialisierens (ParseableItem.IsEventsSuppressed)
    /// ist die Methode eine No-Op: Die Version kommt gerade aus dem Speicher und
    /// ein Hochzaehlen wuerde einen Roundtrip (z.B. JSON-Serialisierung) verfaelschen.
    /// </summary>
    public static void RaiseVersion(this IHasVersion item) {
        if (item is ParseableItem { IsEventsSuppressed: true }) { return; }
        if (item.Version == int.MaxValue) { item.Version = 0; }
        item.Version++;
    }

    #endregion
}