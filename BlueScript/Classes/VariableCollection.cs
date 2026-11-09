// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Collections;
using System.Collections.Concurrent;
using System.IO;

namespace BlueScript.ScriptVariables;

public class VariableCollection : IEnumerable<ScriptVariable>, IEditable, IParseable, IFragmentStoreable {

    #region Enums

    /// <summary>
    /// Pfad-Typen der VariableCollection im binären Fragment-Format. Nur der
    /// Collection selbst bekannt.
    /// </summary>
    private enum VariableCollectionNavigationType : byte {

        /// <summary>
        /// Navigation in eine Variable (Key = KeyName der Variablen);
        /// Journal-Einzelsätze.
        /// </summary>
        SubVariable = 1,

        /// <summary>
        /// Blatt-Satz: übernimmt eine ganze neue Variable im Classic-Format
        /// (Content = Parseable-String der Variablen).
        /// </summary>
        VariableAdd = 2,

        /// <summary>
        /// ListKeyObject-Satz: die Variablen folgen als Liste eine Ebene tiefer,
        /// zwischen ihnen je ein Trenner zwei Ebenen unter der Liste.
        /// </summary>
        VariableList = 3
    }

    #endregion

    #region Fields

    private readonly ConcurrentDictionary<string, ScriptVariable> _internal = new(StringComparer.OrdinalIgnoreCase);
    private List<string>? _cachedStringableNames;

    #endregion

    #region Constructors

    public VariableCollection() { }

    /// <summary>
    /// Erstellt eine neue Liste aus Variablen, die ReadOnly ist
    /// </summary>
    /// <param name="v"></param>
    public VariableCollection(List<ScriptVariable> v) : this(v, true) { }

    public VariableCollection(List<ScriptVariable> v, bool readOnly) {
        foreach (var thisV in v) {
            Add(thisV);
        }
        ReadOnly = readOnly;
    }

    #endregion

    #region Properties

    public string CaptionForEditor => "Variablen";

    public int Count => _internal.Count;
    public bool ReadOnly { get; private set; }

    #endregion

    #region Methods

    /// <summary>
    /// Erstellt eine neue Collection, die alle Variablen aus ExistingVars enthält.
    /// Überschreibt die Werte in den existingVars mit den Werten aus newValues. Die Werte aus NewValues können dabei einen besonderen Prefix haben.
    /// Beispiel: ExistingVars die Variable Test
    /// NewValues behinhaltet XX_Test.
    /// mit dem Prefix XX_ wird der Wert aus XX_Test nach Test geschrieben.
    /// </summary>
    /// <param name="existingVars"></param>
    /// <param name="newValues"></param>
    /// <param name="newValsPrefix"></param>
    /// <returns></returns>
    public static VariableCollection Combine(VariableCollection existingVars, VariableCollection newValues, string newValsPrefix) {
        var vaa = existingVars.ToList();

        foreach (var thisvar in vaa) {
            var v = newValues.GetByKey(newValsPrefix + thisvar.KeyName);
            if (v is null) { continue; }

            thisvar.ReadOnly = false; // weil kein OnPropertyChanged vorhanden ist
            var error = thisvar.GetValueFrom(v);
            thisvar.ReadOnly = true; // weil kein OnPropertyChanged vorhanden ist

            if (error is { Length: > 0 }) { Develop.DebugPrint(error); }
        }

        return [.. vaa];
    }

    public static List<ScriptVariable> ParseVariable(string toParse, bool setReadOnly) {
        List<ScriptVariable> result = [];

        foreach (var t in toParse.SplitAndCutByCr()) {
            if (ParseableItem.NewByParsing<ScriptVariable>(t) is not { } l) { continue; }

            if (setReadOnly) {
                l.ReadOnly = true; // Weil kein OnPropertyChangedEreigniss vorhanden ist
            }

            result.Add(l);
        }

        result.Sort();
        return result;
    }

    public bool Add(ScriptVariable? variable) {
        if (ReadOnly) { return false; }
        if (variable is null) { return false; }

        if (!_internal.TryAdd(variable.KeyName, variable)) { return false; }

        // Das Kind-Event für das FragmentChanged-Bubbling verdrahten und die neue
        // Variable als ganze (Classic-Format) ans Root melden.
        this.Register(variable);

        OnFragmentChanged(new FragmentChangedEventArgs(this, string.Empty, VariableCollectionNavigationType.VariableAdd, variable));

        InvalidateCache();
        return true;
    }

    /// <summary>
    /// True, wenn ALLE Variablen erfolgreich hinzugefügt wurden.
    /// </summary>
    /// <param name="vars"></param>
    /// <returns></returns>
    public bool AddRange(IEnumerable<ScriptVariable> vars) {
        if (ReadOnly) { return false; }

        var f = true;
        foreach (var thisv in vars) {
            if (!Add(thisv)) { f = false; }
        }

        return f;
    }

    //    foreach (var thisvar in vaa) {
    //        var v = newVars.Get(newVarsPrefix + thisvar.KeyName);
    public List<string> AllStringableNames() {
        // Used: Hot-Loop in ScriptCommand.ReplaceVariable. Gibt die gecachte Liste direkt zurück,
        // da der einzige Aufrufer (NextText) nur liest. Add/Remove/Set invalidieren den Cache.
        if (_cachedStringableNames is not null) { return _cachedStringableNames; }

        _cachedStringableNames = _internal.Values
            .Where(thisvar => thisvar.ToStringPossible)
            .Select(thisvar => thisvar.KeyName)
            .ToList();

        return _cachedStringableNames;
    }

    /// <summary>
    /// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten null
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool? GetBoolean(string name) {
        _internal.TryGetValue(name, out var v);

        if (v is not BoolScriptVariable vb) {
            //Develop.DebugPrint("Falscher Datentyp");
            return null;
        }

        return vb.ValueBool;
    }

    public ScriptVariable? GetByKey(string keyname) => _internal.TryGetValue(keyname, out var v) ? v : null;

    //public object Clone() => new Collection(ToList(), ReadOnly);
    public IEnumerator<ScriptVariable> GetEnumerator() => _internal.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => IEnumerable_GetEnumerator();

    /// <summary>
    /// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten eine leere Liste
    /// </summary>
    /// <param name="name"></param>
    public List<string> GetList(string name) {
        _internal.TryGetValue(name, out var v);
        if (v is null) { return []; }

        if (v is not ListOfStringsScriptVariable vf) {
            Develop.DebugPrint("Falscher Datentyp");
            return [];
        }

        return vf.ValueList;
    }

    /// <summary>
    /// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten string.Empty
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public string GetString(string name) {
        _internal.TryGetValue(name, out var v);

        if (v is not StringScriptVariable vf) {
            //Develop.DebugPrint("Falscher Datentyp");
            return string.Empty;
        }

        return vf.ValueString;
    }

    public string IsNowEditable() => string.Empty;

    public List<string> ParseableItems() {
        List<string> result = [];

        result.ParseableAdd("Variable", _internal.Values);
        result.ParseableAdd("ReadOnly", ReadOnly);

        return result;
    }

    public bool ParseFinished() => true;

    public bool ParseThis(string key, string value) {
        switch (key) {
            case "variable":
                var v = ParseableItem.NewByParsing<ScriptVariable>(value);
                if (v is not null) { Add(v); }
                return true;

            case "readonly":
                ReadOnly = value.FromPlusMinus();
                return true;
        }

        return false;
    }

    public bool Remove(string keyName) {
        if (ReadOnly) { return false; }

        if (!_internal.TryRemove(keyName, out var variable)) { return false; }
        this.Unregister(variable);
        InvalidateCache();
        return true;
    }

    public void RemoveWithComment(string comment) {
        if (ReadOnly) { return; }

        var changed = false;
        foreach (var kvp in _internal) {
            if (kvp.Value.Comment.Contains(comment)) {
                this.Unregister(kvp.Value);
                _internal.TryRemove(kvp.Key, out _);
                changed = true;
            }
        }
        if (changed) { InvalidateCache(); }
    }

    public string ReplaceInText(string originalText) {
        foreach (var thisvar in _internal.Values) {
            originalText = thisvar.ReplaceInText(originalText);
        }
        return originalText;
    }

    /// <summary>
    /// Ersetzt den kompletten Inhalt durch die übergebenen Variablen.
    /// Die Collection-Instanz bleibt erhalten.
    /// Gleichnamige Variablen behalten, wenn möglich, ihre Instanz
    /// (Wert und Kommentar werden übernommen), damit bestehende Referenzen gültig bleiben.
    /// </summary>
    public void ReplaceWith(IEnumerable<ScriptVariable>? vars) {
        var wasReadOnly = ReadOnly;
        ReadOnly = false;

        List<ScriptVariable> neu = vars is null ? [] : [.. vars.Where(v => v is not null)];
        var neueKeys = new HashSet<string>(neu.Select(v => v.KeyName), StringComparer.OrdinalIgnoreCase);

        foreach (var v in neu) {
            if (!_internal.TryGetValue(v.KeyName, out var existing)) {
                Add(v);
                continue;
            }

            if (ReferenceEquals(existing, v)) { continue; }

            // Wert in die bestehende Instanz übernehmen, um die Instanzidentität zu erhalten
            var existingReadOnly = existing.ReadOnly;
            existing.ReadOnly = false;
            var error = existing.GetValueFrom(v);
            existing.ReadOnly = existingReadOnly;

            if (error is { Length: > 0 }) {
                // Typ passt nicht → bestehende Instanz doch austauschen
                Remove(v.KeyName);
                Add(v);
                continue;
            }

            existing.Comment = v.Comment;
        }

        foreach (var keyName in _internal.Keys.ToList()) {
            if (!neueKeys.Contains(keyName)) { Remove(keyName); }
        }

        ReadOnly = wasReadOnly;
    }

    /// <summary>
    /// Erstellt bei Bedarf eine neue Variable und setzt den Wert und auch ReadOnly
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public bool Set(string name, string value) {
        if (ReadOnly) { return false; }

        var n = name.ToUpperInvariant();
        _internal.TryGetValue(name, out var v);
        if (v is null) {
            v = new StringScriptVariable(n, string.Empty, false, string.Empty);
            if (!Add(v)) { return false; }
        }

        if (v is not StringScriptVariable vf) {
            Develop.DebugPrint("Variablentyp falsch");
            return false;
        }

        vf.ReadOnly = false; // sonst werden keine Daten geschrieben
        vf.ValueString = value;
        vf.ReadOnly = true;
        return true;
    }

    /// <summary>
    /// Erstellt bei Bedarf eine neue Variable und setzt den Wert und auch ReadOnly
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public bool Set(string name, double value) {
        if (ReadOnly) { return false; }

        var n = name.ToUpperInvariant();
        _internal.TryGetValue(name, out var v);
        if (v is null) {
            v = new DoubleScriptVariable(n);
            if (!Add(v)) { return false; }
        }

        if (v is not DoubleScriptVariable vf) {
            Develop.DebugPrint("Variablentyp falsch");
            return false;
        }

        vf.ReadOnly = false; // sonst werden keine Daten geschrieben
        vf.ValueNum = value;
        vf.ReadOnly = true;
        return true;
    }

    /// <summary>
    /// Erstellt bei Bedarf eine neue Variable und setzt den Wert und auch ReadOnly
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public bool Set(string name, List<string> value) {
        if (ReadOnly) { return false; }

        var n = name.ToUpperInvariant();
        _internal.TryGetValue(name, out var v);
        if (v is null) {
            v = new ListOfStringsScriptVariable(n);
            if (!Add(v)) { return false; }
        }

        if (v is not ListOfStringsScriptVariable vf) {
            Develop.DebugPrint("Variablentyp falsch");
            return false;
        }

        vf.ReadOnly = false; // sonst werden keine Daten geschrieben
        vf.ValueList = value;
        vf.ReadOnly = true;
        return true;
    }

    /// <summary>
    /// Erstellt bei Bedarf eine neue Variable und setzt den Wert und auch ReadOnly
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public bool Set(string name, bool value) {
        if (ReadOnly) { return false; }

        var n = name.ToUpperInvariant();
        _internal.TryGetValue(name, out var v);
        if (v is null) {
            v = new BoolScriptVariable(n);
            if (!Add(v)) { return false; }
        }

        if (v is not BoolScriptVariable vf) {
            Develop.DebugPrint("Variablentyp falsch");
            return false;
        }

        vf.ReadOnly = false; // sonst werden keine Daten geschrieben
        vf.ValueBool = value;
        vf.ReadOnly = true;
        return true;
    }

    public List<ScriptVariable> ToList() {
        var l = new List<ScriptVariable>();
        l.AddRange(_internal.Values);
        return l.OrderBy(r => r.KeyName).ToList();
    }

    /// <summary>
    /// Gibt alle Variabelen Sortiern nach Keyname zurück, die 'ToStringPossible' sind.
    /// </summary>
    /// <returns></returns>
    public List<ScriptVariable> ToStringableListVariable() {
        var l = new List<ScriptVariable>();

        foreach (var thiss in _internal.Values.OrderBy(r => r.KeyName)) {
            if (thiss.ToStringPossible) {
                l.Add(thiss);
            }
        }

        return l;
    }

    /// <summary>
    /// Entfernt alle Variablen aus der Collection.
    /// </summary>
    public void Clear() {
        if (ReadOnly) { return; }

        this.Unregister(_internal.Values);
        _internal.Clear();
        InvalidateCache();
    }

    private IEnumerator IEnumerable_GetEnumerator() => _internal.Values.GetEnumerator();

    private void InvalidateCache() => _cachedStringableNames = null;

    #endregion

    #region IFragmentStoreable

    /// <summary>
    /// Bubbling-Event für Fragment-Änderungen (Pendant zu OnPropertyChangedExt).
    /// </summary>
    public event EventHandler<FragmentChangedEventArgs>? FragmentChanged;

    /// <summary>
    /// Schreibt alle Variablen mit Textdarstellung als Liste: ein VariableList-Satz,
    /// darunter je Variable der Variablen-Block eine Ebene tiefer (Type-Satz zuerst).
    /// </summary>
    public bool SerializeCore(Stream stream, int depth) => FragmentRecord.Append(stream, depth, VariableCollectionNavigationType.VariableList, ToStringableListVariable());

    /// <summary>
    /// Verarbeitet den Variablen-Block: Der VariableList-Satz liest die Variablen
    /// als Liste, der SubVariable-Satz wendet einen Journal-Einzelsatz an, der
    /// VariableAdd-Satz übernimmt eine ganze neue Variable im Classic-Format.
    /// </summary>
    public bool DeserializeThis(Stream stream, FragmentRecord record) {
        switch ((VariableCollectionNavigationType)record.LeafType) {
            case VariableCollectionNavigationType.VariableList:
                // Je Segment eine Variable (Type-Satz zuerst); bestehende Instanzen behalten ihre Identität
                if (FragmentStoreableExtension.DeserializeList(stream, record.Depth, VariableCreate) is not { } vars) { return false; }

                foreach (var thisvar in vars) {
                    if (!ApplyVariable(thisvar.KeyName, thisvar)) { return false; }
                }

                return true;

            case VariableCollectionNavigationType.SubVariable:
                return DeserializeVariable(stream, record);

            case VariableCollectionNavigationType.VariableAdd:
                // Ganze Variable im Classic-Format: fehlt sie, wird sie zugefügt,
                // sonst behält die bestehende Instanz ihre Identität (das Journal
                // kann VariableAdd-Sätze wiederholen).
                if (ParseableItem.NewByParsing<ScriptVariable>(record.StringContent) is not { } newVariable) { return false; }

                return ApplyVariable(newVariable.KeyName, newVariable);

            default:
                return false;
        }
    }

    /// <summary>
    /// Erzeugt die Variable zum Type-Satz des Segment-Anfangs; der konkrete Typ
    /// liegt im Content. Konstruktoren erzeugen schreibgeschützt — zum Einlesen
    /// wird freigegeben (wie im Classic-Pfad).
    /// </summary>
    private ScriptVariable? VariableCreate(FragmentRecord first) {
        if (ParseableItem.NewByTypeName<ScriptVariable>(first.StringContent) is not { } newVar) { return null; }

        newVar.ReadOnly = false;
        return newVar;
    }

    /// <summary>
    /// Verarbeitet den Block einer einzelnen Variable.
    /// </summary>
    private bool DeserializeVariable(Stream stream, FragmentRecord record) {
        // Führende Steuer-Sätze (Kommentar-Trenner) überspringen
        FragmentRecord? first;

        do {
            first = FragmentRecord.TryReadNext(stream, record.Depth + 1);
        } while (first is { IsTag: true });

        if (first is null) { return false; }

        if (first.LeafType != (byte)ScriptVariable.VariableFragmentType.Type) {
            // Bestehende Variable: Wert-/Kommentar-Sätze direkt auf die Instanz anwenden.
            // Fehlt die Variable (Satz trifft vor dem Anlegen ein), Satz überspringen.
            if (GetByKey(record.StringContent) is not { } existing) { return true; }

            return existing.DeserializeThis(stream, first) && existing.Deserialize(stream, record.Depth + 1);
        }

        // Der Type-Satz (erster Satz des Variablen-Blocks) legt den konkreten Typ fest.
        if (ParseableItem.NewByTypeName<ScriptVariable>(first.StringContent) is not { } newVar) { return false; }

        // Die Konstruktoren erzeugen Variablen schreibgeschützt — zum Einlesen freigeben
        // (der Classic-Pfad via ParseVariable macht dies ebenso).
        newVar.ReadOnly = false;
        newVar.KeyName = record.StringContent;

        if (!newVar.Deserialize(stream, record.Depth + 1)) { return false; }

        // Gespeicherte Variablen sind immer schreibgeschützt (wie im Classic-Pfad)
        newVar.ReadOnly = true;

        return ApplyVariable(record.StringContent, newVar);
    }

    /// <summary>
    /// Übernimmt die frisch gelesene Variable: fehlt sie, wird sie zugefügt; sonst
    /// behält die bestehende Instanz ihre Identität (Wert und Kommentar übernommen).
    /// </summary>
    private bool ApplyVariable(string keyName, ScriptVariable newVar) {
        if (GetByKey(keyName) is not { } existing) { return Add(newVar); }

        // Bestehende Instanz behalten — Wert und Kommentar in die Instanz übernehmen
        var wasReadOnly = existing.ReadOnly;
        existing.ReadOnly = false;
        var error = existing.GetValueFrom(newVar);
        existing.ReadOnly = wasReadOnly;
        existing.Comment = newVar.Comment;

        return error is not { Length: > 0 };
    }

    /// <summary>
    /// Reicht eine Variablen-Änderung mit dem eigenen Pfad-Segment (SubVariable, Key der Variable) eine Ebene höher.
    /// </summary>
    public void ChildFragmentChanged(object? sender, FragmentChangedEventArgs e) {
        OnFragmentChanged(new FragmentChangedEventArgs(this, FragmentChangedEventArgs.Crumbs(VariableCollectionNavigationType.SubVariable, FragmentChangedEventArgs.CrumbKey(sender), e.RelativePath), e.TypeSelector, e.ContentType, e.Content) { ListContent = e.ListContent });
    }

    /// <summary>
    /// Löst das FragmentChanged-Event mit den fertigen Argumenten aus (Pendant zu OnPropertyChangedExt).
    /// </summary>
    public void OnFragmentChanged(FragmentChangedEventArgs e) => FragmentChanged?.Invoke(this, e);

    #endregion
}