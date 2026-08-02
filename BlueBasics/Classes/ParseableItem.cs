// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.ComponentModel;
using System.Threading;

namespace BlueBasics.Classes;

public abstract class ParseableItem : IParseable, ICloneable, INotifyPropertyChanged, IDisposableExtended {

    #region Fields

    private volatile int _isDisposedFlag;

    #endregion

    #region Events

    public event EventHandler? Disposed;

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler<JsonPathChangedEventArgs>? PropertyChangedExt;

    #endregion

    #region Properties

    public bool IsDisposed => _isDisposedFlag == 1;

    public string MyClassId {
        get {
            if ((string?)GetType().GetProperty("ClassId")?.GetValue(null, null) is { } ci) {
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

        if (toParse.GetAllTags() is not { } x) { return null; }

        foreach (var thisIt in x) {
            switch (thisIt.Key) {
                case "type":
                case "classid":
                    typeName = thisIt.Value;
                    break;
            }
        }

        if (NewByTypeName<T>(typeName, args) is not { } ni) { return null; }
        ni.Parse(toParse);
        return ni;
    }

    /// <summary>
    /// JSON-Pendant zu <see cref="NewByParsing{T}" />. Erzeugt anhand des
    /// <c>type</c>- bzw. <c>classid</c>-Feldes im JSON-Objekt über die
    /// <c>ClassId</c>-Registry die passende Instanz und parst anschließend das Objekt.
    /// Gibt <c>null</c> zurück, wenn der Typ nicht ermittelt werden konnte.
    /// </summary>
    public static T? NewByParsingJson<T>(JsonObject element, params object[] args) where T : ParseableItem, IJsonParseable {
        var typeName = string.Empty;

        foreach (var prop in element) {
            switch (prop.Key.ToLowerInvariant()) {
                case "type":
                case "classid":
                    if (prop.Value is JsonValue v && v.TryGetValue(out string? s)) {
                        typeName = s ?? string.Empty;
                    }
                    break;
            }
        }

        if (string.IsNullOrEmpty(typeName)) { return null; }

        if (NewByTypeName<T>(typeName, args) is not { } ni) { return null; }
        ni.ParseJson(element);
        return ni;
    }

    /// <summary>
    /// Erstellt eine neue Instanz anhand des Typnamens. Gesucht wird über
    /// die statische <c>ClassId</c>-Property des Zieltyps; <typeparamref name="T" />
    /// muss daher nur ein Referenztyp mit einer solchen Property sein (ein
    /// Bezug zu <see cref="ParseableItem" /> ist nicht erforderlich).
    /// Die Typsuche ist über <see cref="Generic.GetTypeByClassId{T}" /> gecacht,
    /// sodass der Reflection-Zugriff auf <c>ClassId</c> nur einmal pro Target-Typ
    /// und Assembly-Generation erfolgt.
    /// </summary>
    /// <typeparam name="T">Der Zieltyp (Referenztyp) mit einer statischen ClassId-Property.</typeparam>
    /// <param name="typname">Der ClassId-String des gesuchten Typs.</param>
    /// <param name="args">Konstruktor-Argumente für den Zieltyp.</param>
    /// <returns>Eine neue Instanz oder null, wenn der Typ nicht gefunden wurde.</returns>
    public static T? NewByTypeName<T>(string? typname, params object[] args) where T : class {
        if (string.IsNullOrEmpty(typname)) { return null; }
        if (GetTypeByClassId<T>(typname) is not { } type) { return null; }
        return Activator.CreateInstance(type, args) as T;
    }

    public object Clone() {
        // GetType() liefert den konkreten Typ direkt - kein Umweg nötig über
        // NewByParsing, das sonst über ALLE ParseableItem-Typen iteriert und
        // bei Klassen ohne parameterlosen Konstruktor (z. B. Variable) schon
        // vor dem Finden des richtigen Typs eine MissingMethodException wirft.
        if (Activator.CreateInstance(GetType()) is not ParseableItem clone) {
            throw Develop.DebugError("Clonen fehlgeschlagen");
        }
        clone.Parse(ParseableItems().FinishParseable());
        return clone;
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Default: keine Sub-Items. Container-Klassen überschreiben dies.
    /// </summary>
    public virtual IJsonParseable? GetSubItemByKey(string containerName, string key) => null;

    /// <summary>
    /// Löst das <see cref="PropertyChangedExt" />-Event aus. Wird von Property-Settern
    /// in Subklassen aufgerufen, um eine inkrementelle Speicherung auszulösen.
    /// EventArgs-Erzeugung delegiert an
    /// <see cref="JsonParseableExtension.BuildPartialJson" />, nur der SourceKey
    /// wird hier individuell aufgebaut (ParseableItem ist formal nicht
    /// IJsonParseable, deshalb kein direkter Aufruf der Extension).
    /// </summary>
    public void OnPropertyChangedExt(string relativePath, object? value) {
        if (IsDisposed) { return; }
        var key = (this as IHasKeyName)?.KeyName ?? MyClassId;
        var partial = JsonParseableExtension.BuildPartialJson(relativePath, value);
        PropertyChangedExt?.Invoke(this, new JsonPathChangedEventArgs(relativePath, partial, key));
    }

    public virtual List<string> ParseableItems() {
        List<string> result = [];
        result.ParseableAdd("ClassId", MyClassId);
        return result;
    }

    public virtual void ParseFinished(string parsed) { }

    public abstract bool ParseThis(string key, string value);

    public override string ToString() => ParseableItems().FinishParseable();

    protected virtual void Dispose(bool disposing) {
        if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) != 0) { return; }

        if (disposing) {
            OnDisposed();
            Disposed = null;
            PropertyChangedExt = null;
            PropertyChanged = null;
        }
    }

    protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void OnDisposed() => Disposed?.Invoke(this, System.EventArgs.Empty);

    #endregion
}