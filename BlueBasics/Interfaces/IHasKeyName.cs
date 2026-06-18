// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueBasics.Interfaces;

public interface IHasKeyName {

    #region Properties

    string KeyName { get; }

    #endregion
}

public static class IHasKeyNameExtension {

    #region Methods

    public static List<T> SortByKeyName<T>(this List<T>? keys) where T : IHasKeyName, IStringable {
        if (keys == null) { return []; }

        //// Prüfen, ob das erste Element IComparable implementiert
        //var firstItem = keys.FirstOrDefault();
        //if (firstItem is IComparable) {
        //    return keys.OrderBy(r => r);
        //}

        // Fallback auf KeyName
        return keys.OrderBy(r => r.KeyName).ToList();
    }

    #endregion
}