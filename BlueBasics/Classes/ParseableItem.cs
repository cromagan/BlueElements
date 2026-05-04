// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.ComponentModel;

namespace BlueBasics.Classes;

public abstract class ParseableItem : IParseable, ICloneable, INotifyPropertyChanged {
    //public abstract string MyClassId { get; }

    #region Events

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Properties

    public string MyClassId {
        get {
            var ci = (string?)GetType().GetProperty("ClassId")?.GetValue(null, null);
            if (ci != null) {
                return ci;
            }

            throw Develop.DebugError("ClassID nicht gefunden!");
        }
    }

    #endregion

    #region Methods

    public static T? NewByParsing<T>(string toParse, params object[] args) where T : ParseableItem {
        var typeName = string.Empty;

        if (toParse.StartsWith("[I]", StringComparison.Ordinal)) { toParse = toParse.FromNonCritical(); }

        if (toParse is "{}" or "{ }") { return null; }

        var x = toParse.GetAllTags();

        if (x == null) { return null; }

        foreach (var thisIt in x) {
            switch (thisIt.Key) {
                case "type":
                case "classid":
                    typeName = thisIt.Value;
                    break;
            }
        }

        var ni = NewByTypeName<T>(typeName, args);
        if (ni == null) { return null; }
        ni.Parse(toParse);
        return ni;
    }

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="typname"></param>
    /// <param name="args">Ein Array von Argumenten, das bezüglich Anzahl, Reihenfolge und Typ mit den Parametern
    /// des aufzurufenden Konstruktors übereinstimmt. Wenn args ein leeres Array oder
    /// null ist, wird der Konstruktor aufgerufen, der keine Parameter akzeptiert, d.   h. der Standardkonstruktor.
    ///</param>
    /// <returns></returns>
    public static T? NewByTypeName<T>(string? typname, params object[] args) where T : ParseableItem {
        if (string.IsNullOrEmpty(typname)) { return null; }
        var types = Generic.GetEnumerableOfType<T>();

        if (!types.Any()) { return null; }

        if (string.IsNullOrEmpty(typname)) {
            Develop.DebugError("Typ unbekannt: " + typname);
            return null;
        }

        foreach (var thist in types) {
            if (thist != null) {
                var v = thist.GetProperty("ClassId")?.GetValue(null, null);
                if (v is string tn && tn.Equals(typname, StringComparison.OrdinalIgnoreCase)) {
                    var created = Activator.CreateInstance(thist, args);
                    if (created == null) {
                        return null;
                    }
                    var ni = (T)created;
                    return ni;
                }
            }
        }
        return null;
    }

    public object Clone() {
        if (NewByParsing<ParseableItem>(ParseableItems().FinishParseable()) is { } clone) {
            return clone;
        }
        throw Develop.DebugError("Clonen fehlgeschlagen");
    }

    public virtual List<string> ParseableItems() {
        List<string> result = [];
        result.ParseableAdd("ClassId", MyClassId);
        return result;
    }

    public virtual void ParseFinished(string parsed) { }

    public abstract bool ParseThis(string key, string value);

    public override string ToString() => ParseableItems().FinishParseable();

    protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion
}