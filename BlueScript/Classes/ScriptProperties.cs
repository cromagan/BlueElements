// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueScript.Classes;

public class ScriptProperties {

    #region Fields

    private Dictionary<string, List<Method>>? _methodLookup;
    private List<string>? _methodsWithReturnSearch;

    #endregion

    #region Constructors

    public ScriptProperties(string scriptname, IEnumerable<Method> allowedMethods, bool produktivphase, List<string> scriptAttributes, object? additionalInfo, string chain, string mainInfo) {
        ScriptName = scriptname;
        AllowedMethods = allowedMethods;
        ProduktivPhase = produktivphase;
        ScriptAttributes = scriptAttributes;
        AdditionalInfo = additionalInfo;
        Stufe = 0;
        Chain = chain;
        MainInfo = mainInfo;
    }

    public ScriptProperties(ScriptProperties scriptProperties, IEnumerable<Method> allowedMethods, int stufe, string chain) : this(scriptProperties.ScriptName, allowedMethods, scriptProperties.ProduktivPhase, scriptProperties.ScriptAttributes, scriptProperties.AdditionalInfo, chain, scriptProperties.MainInfo) => Stufe = stufe;

    #endregion

    #region Properties

    public object? AdditionalInfo { get; }
    public IEnumerable<Method> AllowedMethods { get; }
    public string Chain { get; } = string.Empty;

    public string MainInfo { get; } = string.Empty;

    public Dictionary<string, List<Method>> MethodLookup => _methodLookup ??= BuildMethodLookup();

    public List<string> MethodsWithReturnSearch => _methodsWithReturnSearch ??= BuildMethodsWithReturnSearch();

    public bool ProduktivPhase { get; }

    /// <summary>
    /// Diese Attriute muss das nachfolgende Script mindestens erfüllen
    /// </summary>
    public List<string> ScriptAttributes { get; }

    public string ScriptName { get; }
    public int Stufe { get; }

    #endregion

    #region Methods

    private Dictionary<string, List<Method>> BuildMethodLookup() {
        var lookup = new Dictionary<string, List<Method>>(StringComparer.OrdinalIgnoreCase);
        foreach (var m in AllowedMethods) {
            if (!lookup.TryGetValue(m.Command, out var list)) {
                list = [];
                lookup[m.Command] = list;
            }
            list.Add(m);
        }
        return lookup;
    }

    private List<string> BuildMethodsWithReturnSearch() {
        List<string> list = [];
        foreach (var m in AllowedMethods) {
            if (!string.IsNullOrEmpty(m.Returns)) {
                list.Add(m.Command + m.StartSequence);
            }
        }
        return list;
    }

    #endregion
}