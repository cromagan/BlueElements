// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using static BlueBasics.ClassesStatic.Generic;

namespace BlueControls.Interfaces;

public interface IHasVersion : IHasKeyName {

    #region Properties

    int Version { get; set; }

    #endregion
}

public static class HasVersionExtensions {

    #region Methods

    public static string DefaultItemToControlName(this IHasVersion item, string? parentName) {
        if (parentName == null) {
            return item.KeyName + "-" + item.Version + "-[UNKNOW]";
        }

        return item.KeyName + "-" + item.Version + "-" + parentName.GetMD5Hash();
    }

    public static void RaiseVersion(this IHasVersion item) {
        if (item.Version == int.MaxValue) { item.Version = 0; }
        item.Version++;
    }

    #endregion
}