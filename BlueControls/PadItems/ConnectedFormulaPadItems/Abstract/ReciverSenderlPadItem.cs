// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;

namespace BlueControls.PadItems.FunktionsItems_Formular.Abstract;

/// <summary>
/// Standard für Objekte, die einen Tabellen/Zeilenbezug haben.
/// Stellt auch alle Methode breit, zum Einrichten der Breite und Benutzer-Sichtbarkeiten.
/// Nur Tabs, die ein solches Objekt haben, werden als anzeigewürdig gewertet.
/// </summary>

public abstract class ReciverSenderPadItem : ReciverPadItem {

    #region Fields

    private Table? _tableOutput;
    private string _tableOutputHintPath = string.Empty;
    private string _tableOutputName = string.Empty;
    private bool _tableOutputTried;

    #endregion

    #region Constructors

    protected ReciverSenderPadItem(string keyName, Controls.ConnectedFormula.ConnectedFormula? parentFormula, Table? tableOutput) : base(keyName, parentFormula) {
        BeginInit();

        try {
            Table.Added += Table_Added;
            TableOutput = tableOutput;
        } finally { EndInit(); }
    }

    #endregion

    #region Properties

    public int OutputColorId {
        get;

        private set {
            if (IsDisposed) { return; }

            if (field == value) { return; }

            field = value;
            OnPropertyChanged();
        }
    } = -1;

    /// <summary>
    /// Die Tabelle, in die dieses Element seine Werte schreibt.
    /// </summary>
    public Table? TableOutput {
        get {
            if (TableInputMustMatchOutputTable && TableInput is { IsDisposed: false }) { return TableInput; }

            if (_tableOutputTried) { return _tableOutput; }

            _tableOutputTried = true;
            _tableOutput = Table.Get(_tableOutputName);

            // Fallback: Wenn die Tabelle namentlich nicht gefunden wurde
            // (z. B. weil noch keine andere Tabelle geladen ist und der
            // Name kein Dateipfad ist), versuche es mit dem HintPath.
            if (_tableOutput is null && _tableOutputHintPath is { Length: > 0 }) {
                _tableOutput = Table.Get(_tableOutputHintPath);
            }

            // HintPath aus der gefundenen Tabelle aktualisieren, damit er
            // beim nächsten Speichern aktuell ist — auch wenn die Tabelle
            // über den Namen (ohne HintPath) gefunden wurde.
            if (_tableOutput is TableFile tbf) {
                _tableOutputHintPath = tbf.Filename;
            }

            return _tableOutput;
        }
        set {
            if (IsDisposed) { return; }

            if (TableInputMustMatchOutputTable && !AllowedInputFilter.HasFlag(AllowedInputFilter.None)) { return; }

            if (value == TableOutput) { return; }

            _tableOutput = value;
            _tableOutputName = value?.KeyName ?? string.Empty;
            _tableOutputHintPath = (value as TableFile)?.Filename ?? string.Empty;
            _tableOutputTried = true;
            OnPropertyChanged();
            OnDoUpdateSideOptionMenu();
            OnPropertyChangedExt("outputtable", _tableOutputName);
        }
    }

    #endregion

    //public void AddChild(IHasKeyName add) {
    //    var l = new List<string>();
    //    l.AddRange(ChildIds);
    //    l.Add(add.KeyName);
    //    l = l.SortedDistinctList();

    //    ChildIds = l.AsReadOnly();
    //}

    #region Methods

    public override void AddedToCollection(CollectionPadItem parent) {
        if (IsDisposed) { return; }

        // Befindet sich die umgebende Page im Parse-/Initialisierungs-Modus
        // (IsEventsSuppressed), wird dieses Item gerade aus einem Speicherstand
        // geladen. Die unten folgenden Initialisierungs-Aufrufe (OutputColorId,
        // OnPropertyChanged) duerfen dann keine Versionserhoehung ausloesen -
        // sonst wuerde das die Version bei jedem Roundtrip (altes Format <->
        // JSON) kuenstlich hochzaehlen. Siehe IHasVersion.RaiseVersion.
        // Im interaktiven Fall (Drag & Drop im Editor) ist die Page nicht
        // suppressed und das Verhalten bleibt wie bisher.
        var suppress = parent.IsEventsSuppressed;
        if (suppress) { BeginInit(); }
        try {
            base.AddedToCollection(parent);

            if (Parent is not null) {
                OutputColorId = -1;
                OutputColorId = GetFreeColorId();
            }
            OnPropertyChanged();
        } finally {
            if (suppress) { EndInit(); }
        }
    }

    public override string ErrorReason() {
        if (TableOutput is not { IsDisposed: false }) {
            if (TableInputMustMatchOutputTable) {
                return "Eingehendes Objekt nicht gewählt.";
            }

            return "Ausgehende Tabelle nicht angegeben.";
        }

        return base.ErrorReason();
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result = [.. base.GetProperties(widthOfControl),
                                new FlexiControl("Ausgang:", widthOfControl, true)
        ];

        var enableOutput = true;
        Table? outp = null;

        if (TableInputMustMatchOutputTable) {
            enableOutput = AllowedInputFilter.HasFlag(AllowedInputFilter.None);

            outp = TableInput;
            if (outp is not null) { enableOutput = false; }
        }
        if (!enableOutput) {
            if (outp is not null) {
                result.Add(new FlexiControlForDelegate(outp));
            } else {
                result.Add(new FlexiControl("<imagecode=Information|16> Ausgangstabelle wird über den Eingang gewählt.", widthOfControl, false));
            }
        } else {
            result.Add(new FlexiControlForProperty<Table?>(() => TableOutput, AllAvailableTables()));
            if (TableOutput is { } tbo) {
                result.Add(new FlexiControlForDelegate(tbo));
            }
        }

        return result;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];

        if (TableInputMustMatchOutputTable && TableInput is { IsDisposed: false } tb) {
            result.ParseableAdd("OutputTable", tb.KeyName);
        } else {
            result.ParseableAdd("OutputTable", _tableOutputName);
            result.ParseableAdd("OutputTableHintPath", _tableOutputHintPath);
        }

        //result.ParseableAdd("SentToChildIds", _childIds, false);

        return result;
    }

    public override JsonObject ParseableJson() {
        if (TableOutput is TableFile tf) {
            _tableOutputHintPath = tf.Filename;
        }

        var json = base.ParseableJson();

        if (TableInputMustMatchOutputTable && TableInput is { IsDisposed: false } tb) {
            json.Set("outputtable", tb.KeyName);
        } else {
            json.Set("outputtable", _tableOutputName);
        }

        json.Set("outputtablehintpath", _tableOutputHintPath);

        return json;
    }

    public override void ParseJson(JsonObject json) {
        BeginInit();
        try {
            var name = json.GetString("outputtable");
            if (!string.IsNullOrEmpty(name)) {
                _tableOutputName = name;
                _tableOutputTried = false;
            }

            _tableOutputHintPath = json.GetString("outputtablehintpath", _tableOutputHintPath);

            base.ParseJson(json);
        } finally {
            EndInit();
        }
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "table":
            case "outputdatabase":
            case "outputtable":
                _tableOutputName = value.FromNonCritical();
                _tableOutputTried = false;
                return true;

            case "outputtablehintpath":
                _tableOutputHintPath = value.FromNonCritical();
                return true;

            case "senttochildids":
                return true;
        }
        return base.ParseThis(key, value);
    }

    internal int GetFreeColorId() {
        if (Parent is not CollectionPadItem { IsDisposed: false } icpi) { return -1; }

        var usedids = new List<int>();

        foreach (var thisIt in icpi) {
            if (thisIt is ReciverSenderPadItem hci) {
                usedids.Add(hci.OutputColorId);
            }
        }

        for (var c = 0; c < 9999; c++) {
            if (!usedids.Contains(c)) { return c; }
        }
        return -1;
    }

    protected override void Dispose(bool disposing) {
        if (IsDisposed) { return; }

        if (disposing) {
            Table.Added -= Table_Added;
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// Ereignisgesteuerter Retry: Wurde die Ausgangstabelle beim ersten
    /// Zugriff nicht gefunden (z. B. weil sie noch nicht geladen war),
    /// wird hier auf neu hinzugefügte Tabellen reagiert. Stimmt KeyName
    /// oder Dateipfad überein, wird die Property invalidiert und beim
    /// nächsten Zugriff neu geladen.
    /// </summary>
    private void Table_Added(object? sender, LiveInstanceEventArgs<Table> e) {
        if (IsDisposed) { return; }
        if (_tableOutputTried && _tableOutput is not null) { return; }
        var tb = e.Instance;

        var matches = string.Equals(tb.KeyName, _tableOutputName, StringComparison.OrdinalIgnoreCase);
        if (!matches && tb is TableFile tbf) {
            matches = string.Equals(tbf.Filename, _tableOutputHintPath, StringComparison.OrdinalIgnoreCase);
        }

        if (!matches) { return; }

        _tableOutputTried = false;
        OnDoUpdateSideOptionMenu();
    }

    #endregion
}