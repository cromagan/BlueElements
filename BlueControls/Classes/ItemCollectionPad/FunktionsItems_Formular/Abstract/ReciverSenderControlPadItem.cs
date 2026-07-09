// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular.Abstract;

/// <summary>
/// Standard für Objekte, die einen Tabellen/Zeilenbezug haben.
/// Stellt auch alle Methode breit, zum Einrichten der Breite und Benutzer-Sichtbarkeiten.
/// Nur Tabs, die ein solches Objekt haben, werden als anzeigewürdig gewertet.
/// </summary>

public abstract class ReciverSenderControlPadItem : ReciverControlPadItem {

    #region Fields

    private bool _tableOutputLoaded;

    private string _tableOutputName = string.Empty;

    #endregion

    #region Constructors

    protected ReciverSenderControlPadItem(string keyName, Controls.ConnectedFormula.ConnectedFormula? parentFormula, Table? tableOutput) : base(keyName, parentFormula) => TableOutput = tableOutput;

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

    public Table? TableOutput {
        get {
            if (TableInputMustMatchOutputTable && TableInput is { IsDisposed: false }) { return TableInput; }

            if (_tableOutputLoaded) { return field; }

            if (string.IsNullOrEmpty(_tableOutputName)) {
                field = null;
            } else {
                field = Table.Get(_tableOutputName, TableView.Table_NeedPassword);
            }

            _tableOutputLoaded = true;

            return field;
        }
        set {
            if (IsDisposed) { return; }

            if (TableInputMustMatchOutputTable && !AllowedInputFilter.HasFlag(AllowedInputFilter.None)) { return; }

            if (value == TableOutput) { return; }

            field = value;
            _tableOutputName = value?.KeyName ?? string.Empty;
            _tableOutputLoaded = true;
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

    public override void AddedToCollection(ItemCollectionPadItem parent) {
        if (IsDisposed) { return; }
        base.AddedToCollection(parent);

        if (Parent is not null) {
            OutputColorId = -1;
            OutputColorId = GetFreeColorId();
        }
        OnPropertyChanged();
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
        }

        //result.ParseableAdd("SentToChildIds", _childIds, false);

        return result;
    }

    public override JsonObject ParseableJson() {
        var json = base.ParseableJson();

        if (TableInputMustMatchOutputTable && TableInput is { IsDisposed: false } tb) {
            json["outputtable"] = tb.KeyName;
        } else {
            json["outputtable"] = _tableOutputName;
        }

        return json;
    }

    public override void ParseJson(JsonObject json) {
        var name = json.GetString("outputtable");
        if (!string.IsNullOrEmpty(name)) {
            _tableOutputName = name;
            _tableOutputLoaded = false;
        }
        base.ParseJson(json);
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "table":
            case "outputdatabase":
            case "outputtable":
                _tableOutputName = value.FromNonCritical();
                _tableOutputLoaded = false;

                return true;

            case "senttochildids":
                //value = value.Replace("\r", "|");

                //var tmp = value.FromNonCritical().SplitBy("|");
                //_childIds.Clear();
                //foreach (var thiss in tmp) {
                //    _childIds.Add(thiss.FromNonCritical());
                //}
                return true;
        }
        return base.ParseThis(key, value);
    }

    internal int GetFreeColorId() {
        if (Parent is not ItemCollectionPadItem { IsDisposed: false } icpi) { return -1; }

        var usedids = new List<int>();

        foreach (var thisIt in icpi) {
            if (thisIt is ReciverSenderControlPadItem hci) {
                usedids.Add(hci.OutputColorId);
            }
        }

        for (var c = 0; c < 9999; c++) {
            if (!usedids.Contains(c)) { return c; }
        }
        return -1;
    }

    #endregion
}