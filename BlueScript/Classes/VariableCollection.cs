// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Collections.ObjectModel;
using System.IO;

namespace BlueScript.ScriptVariables;

public class VariableCollection : KeyedItemCollection<ScriptVariable>, IEditable, IParseable {

    #region Enums

    /// <summary>
    /// Pfad-Typen der VariableCollection im binären Fragment-Format. Nur der
    /// Collection selbst bekannt. Die Werte liegen bewusst oberhalb des
    /// Variablen-Kreises (VariableFragmentType 1-5), damit der erste Satz eines
    /// SubVariable-Blocks nie mit einem Variablen-Blatt verwechselt wird.
    /// </summary>
    private enum VariableCollectionNavigationType : byte {

        /// <summary>
        /// Navigation in eine Variable (Key = KeyName der Variablen);
        /// Journal-Einzelsätze.
        /// </summary>
        SubVariable = 94,

        /// <summary>
        /// Blatt-Satz: übernimmt eine ganze neue Variable im Classic-Format
        /// (Content = Parseable-String der Variablen).
        /// </summary>
        VariableAdd = 99,

        /// <summary>
        /// Blatt-Satz: entfernt die Variable des Journal-Blocks (Nav-Schlüssel).
        /// </summary>
        VariableRemove = 100,

        /// <summary>
        /// ListKeyObject-Satz: die Variablen folgen als Liste eine Ebene tiefer,
        /// zwischen ihnen je ein Trenner zwei Ebenen unter der Liste.
        /// </summary>
        VariableList = 91
    }

    #endregion

    #region Fields

    private List<string>? _cachedStringableNames;

    #endregion

    #region Constructors

    public VariableCollection() { }

    /// <summary>
    /// Erstellt eine schreibgeschützte Sammlung mit den gegebenen Variablen.
    /// </summary>
    public VariableCollection(List<ScriptVariable> v) : this(v, true) { }

    /// <summary>
    /// Übernimmt die Variablen und startet ggf. schreibgeschützt.
    /// </summary>
    public VariableCollection(List<ScriptVariable> v, bool readOnly) : base(v, readOnly) { }

    #endregion

    #region Properties

    public string CaptionForEditor => "Variablen";

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

    /// <summary>
    /// Invalidiert den Namens-Cache; Verdrahtung und Journal-Meldung macht die Basis.
    /// </summary>
    protected override void OnItemAdded(ScriptVariable variable) {
        base.OnItemAdded(variable);
        InvalidateCache();
    }

    //    foreach (var thisvar in vaa) {
    //        var v = newVars.Get(newVarsPrefix + thisvar.KeyName);
    public List<string> AllStringableNames() {
        // Used: Hot-Loop in ScriptCommand.ReplaceVariable. Gibt die gecachte Liste direkt zurück,
        // da der einzige Aufrufer (NextText) nur liest. Add/Remove/Set invalidieren den Cache.
        if (_cachedStringableNames is not null) { return _cachedStringableNames; }

        _cachedStringableNames = Items()
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
        var v = GetByKey(name);

        if (v is not BoolScriptVariable vb) {
            //Develop.DebugPrint("Falscher Datentyp");
            return null;
        }

        return vb.ValueBool;
    }

    //public object Clone() => new Collection(ToList(), ReadOnly);

    /// <summary>
    /// Falls es die Variable gibt, wird dessen Wert ausgegeben. Ansonsten eine leere Liste
    /// </summary>
    /// <param name="name"></param>
    public List<string> GetList(string name) {
        var v = GetByKey(name);
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
        var v = GetByKey(name);

        if (v is not StringScriptVariable vf) {
            //Develop.DebugPrint("Falscher Datentyp");
            return string.Empty;
        }

        return vf.ValueString;
    }

    public List<string> ParseableItems() {
        List<string> result = [];

        result.ParseableAdd("Variable", Items());
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

    /// <summary>
    /// Löst die Verdrahtung der Variable und invalidiert den Namens-Cache; eine Entfernung
    /// erzeugt bewusst keinen Journal-Eintrag.
    /// </summary>
    protected override void OnItemRemoved(ScriptVariable variable) {
        base.OnItemRemoved(variable);
        InvalidateCache();
    }

    public void RemoveWithComment(string comment) {
        if (ReadOnly) { return; }

        foreach (var thisvar in Items()) {
            if (!thisvar.Comment.Contains(comment)) { continue; }

            _ = Remove(thisvar.KeyName);
        }
    }

    public string ReplaceInText(string originalText) {
        foreach (var thisvar in Items()) {
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
            if (GetByKey(v.KeyName) is not { } existing) {
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

        foreach (var keyName in Keys()) {
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
        var v = GetByKey(name);
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
        var v = GetByKey(name);
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
        var v = GetByKey(name);
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
        var v = GetByKey(name);
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

    /// <summary>
    /// Gibt alle Variablen, die 'ToStringPossible' sind, sortiert nach KeyName zurück.
    /// </summary>
    public List<ScriptVariable> ToStringableListVariable() {
        var l = new List<ScriptVariable>();

        foreach (var thiss in ToList()) {
            if (thiss.ToStringPossible) {
                l.Add(thiss);
            }
        }

        return l;
    }

    private void InvalidateCache() => _cachedStringableNames = null;

    #endregion

    #region IFragmentStoreable

    /// <summary>
    /// Schreibt alle Variablen mit Textdarstellung als Liste: ein VariableList-Satz,
    /// darunter je Variable der Variablen-Block eine Ebene tiefer (Type-Satz zuerst).
    /// </summary>
    public override bool SerializeCore(Stream stream, int depth) {
        if (IsDisposed) { return true; }

        return FragmentRecord.Append(stream, depth, VariableCollectionNavigationType.VariableList, ToStringableListVariable());
    }

    /// <summary>
    /// Pfad-Typen: die Liste unter dem VariableList-Satz, Aufnahmen über VariableAdd.
    /// </summary>
    protected override Enum ListType => VariableCollectionNavigationType.VariableList;

    /// <summary>
    /// Aufnahmen eines ganzen Classic-Variable-Strings.
    /// </summary>
    protected override Enum AddItemType => VariableCollectionNavigationType.VariableAdd;

    /// <summary>
    /// Entfernungen per Journal-Block.
    /// </summary>
    protected override Enum RemoveItemType => VariableCollectionNavigationType.VariableRemove;

    /// <summary>
    /// Pfad-Segment des Bubbling.
    /// </summary>
    protected override Enum ItemSubType => VariableCollectionNavigationType.SubVariable;

    /// <summary>
    /// Erzeugt die Variable zum Type-Satz des Segment-Anfangs; der konkrete Typ
    /// liegt im Content. Konstruktoren erzeugen schreibgeschützt — zum Einlesen
    /// wird freigegeben (wie im Classic-Pfad).
    /// </summary>
    protected override ScriptVariable? CreateItem(FragmentRecord first) {
        if (ParseableItem.NewByTypeName<ScriptVariable>(first.StringContent) is not { } newVar) { return null; }

        newVar.ReadOnly = false;
        return newVar;
    }

    /// <summary>
    /// Übernimmt die gelesenen Variablen einzeln; bestehende Instanzen behalten ihre Identität.
    /// </summary>
    protected override bool ApplyLoadedItems(ReadOnlyCollection<ScriptVariable> items) {
        foreach (var thisvar in items) {
            if (!ApplyVariable(thisvar.KeyName, thisvar)) { return false; }
        }

        return true;
    }

    /// <summary>
    /// Ganze Variable im Classic-Format: fehlt sie, wird sie zugefügt,
    /// sonst behält die bestehende Instanz ihre Identität (das Journal
    /// kann VariableAdd-Sätze wiederholen).
    /// </summary>
    protected override bool ApplyItemAdd(string toParse, string keyName) {
        if (ParseableItem.NewByParsing<ScriptVariable>(toParse) is not { } newVariable) { return false; }

        return ApplyVariable(newVariable.KeyName, newVariable);
    }

    /// <summary>
    /// Wendet SubVariable-Journal-Blöcke auf die benannte Variable an: Add- und
    /// Remove-Blätter buchen die Variable, die übrigen Blöcke folgen dem
    /// Variablen-Format (Type-Satz festlegt den konkreten Typ).
    /// </summary>
    public override bool DeserializeThis(Stream stream, FragmentRecord record) {
        if (IsDisposed) { return false; }

        if (record.LeafType == (byte)VariableCollectionNavigationType.SubVariable && record.DataType == BlueBasics.Enums.FragmentDataType.String) { return DeserializeVariable(stream, record); }

        return base.DeserializeThis(stream, record);
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

        if (first.LeafType == (byte)VariableCollectionNavigationType.VariableRemove) { return ApplyItemRemove(record.StringContent); }

        if (first.LeafType == (byte)VariableCollectionNavigationType.VariableAdd) { return ApplyItemAdd(first.StringContent, record.StringContent); }

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

    #endregion
}
