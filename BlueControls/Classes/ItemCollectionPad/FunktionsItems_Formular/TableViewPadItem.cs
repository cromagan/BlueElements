// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes.ItemCollectionList;
using BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueControls.Controls;
using System.Windows.Forms;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Classes.ItemCollectionPad.FunktionsItems_Formular;

/// <summary>
/// Dieses Element kann Filter empfangen, und gibt dem Nutzer die Möglichkeit, aus dem daraus resultierenden Zeilen EINE zu wählen.
/// Per Tabellenansicht
/// </summary>

public class TableViewPadItem : ReciverSenderControlPadItem, IItemToControl, IAutosizable {

    #region Constructors

    public TableViewPadItem() : this(string.Empty, null, null) { }

    public TableViewPadItem(string keyName, Controls.ConnectedFormula.ConnectedFormula? cformula) : this(keyName, null, cformula) { }

    public TableViewPadItem(string keyName, Table? tb, Controls.ConnectedFormula.ConnectedFormula? cformula) : base(keyName, cformula, tb) { }

    #endregion

    #region Properties

    public static string ClassId => "FI-TableView";

    public override AllowedInputFilter AllowedInputFilter => AllowedInputFilter.None | AllowedInputFilter.More;

    public bool AutoSizeableHeight => true;

    public override string Description => "Darstellung einer Tabelle als bearbeitbare und filterbare Tabelle.";

    /// <summary>
    /// KeyName eines EventScripts, das beim Doppelklick auf eine Zelle
    /// ausgeführt wird, anstatt die Bearbeitung zu öffnen.
    /// </summary>
    [DefaultValue("")]
    public string Doppelklick_Skript {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public override bool InputMustBeOneRow => false;

    public override bool MustBeInDrawingArea => true;

    [DefaultValue(GroupBoxStyle.Normal)]
    public GroupBoxStyle RahmenStil {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = GroupBoxStyle.Nothing;

    [DefaultValue("")]
    public string Standard_Ansicht {
        get;
        set {
            if (IsDisposed) { return; }
            if (field == value) { return; }
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public override bool TableInputMustMatchOutputTable => true;

    protected override int SaveOrder => 1;

    #endregion

    #region Methods

    public static List<AbstractListItem> AllAvailableColumArrangemengts(Table db) {
        var tcvc = ColumnViewCollection.ParseAll(db);
        var u2 = new List<AbstractListItem>();
        foreach (var thisC in tcvc) {
            u2.Add(ItemOf(thisC as IReadableTextWithKey));
        }
        return u2;
    }

    public static List<AbstractListItem> AllAvailableRowScripts(Table db) {
        var u = new List<AbstractListItem>();
        u.Add(ItemOf("Keins", string.Empty));
        foreach (var thisS in db.EventScript) {
            if (thisS is { IsDisposed: false } && thisS.IsOk() && thisS.NeedRow) {
                u.Add(ItemOf(thisS as IReadableTextWithKey));
            }
        }
        return u;
    }

    public Control CreateControl(ConnectedFormulaView parent, string mode) {
        var con = new TableViewWithFilters();
        con.Table = TableOutput;
        con.DoDefaultSettings(parent, this, mode);
        con.Arrangement = Standard_Ansicht;
        con.DoubleClickScript = Doppelklick_Skript;
        con.EditButton = string.Equals(Generic.UserGroup, Constants.Administrator, StringComparison.OrdinalIgnoreCase);
        con.GroupBoxStyle = RahmenStil;
        return con;
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [
            .. base.GetProperties(widthOfControl),
            new FlexiControl("Einstellungen:", widthOfControl, true),
            new FlexiControlForProperty<GroupBoxStyle>(() => RahmenStil,ItemsOf(typeof(GroupBoxStyle)) )
        ];

        if (TableOutput is { IsDisposed: false } tb) {
            result.Add(new FlexiControlForProperty<string>(() => Standard_Ansicht, AllAvailableColumArrangemengts(tb)));
            result.Add(new FlexiControlForProperty<string>(() => Doppelklick_Skript, AllAvailableRowScripts(tb)));
        }

        //if (TableOutput is { IsDisposed: false }) {
        //    var u = new List<AbstractListItem>();
        //    u.AddRange(ItemsOf(typeof(Filterausgabe)));
        //    result.Add(new FlexiControlForProperty<Filterausgabe>(() => FilterOutputType, u));
        //}
        return result;
    }

    public override List<string> ParseableItems() {
        if (IsDisposed) { return []; }
        List<string> result = [.. base.ParseableItems()];
        result.ParseableAdd("DefaultArrangement", Standard_Ansicht);
        result.ParseableAdd("BorderStyle", RahmenStil);
        if (!string.IsNullOrEmpty(Doppelklick_Skript)) {
            result.ParseableAdd("DoubleClickScript", Doppelklick_Skript);
        }
        return result;
    }

    public override JsonObject ParseableJson() {
        var json = base.ParseableJson();
        json.Set("defaultarrangement", Standard_Ansicht);
        json.Set("borderstyle", (int)RahmenStil);
        json.Set("doubleclickscript", Doppelklick_Skript);
        return json;
    }

    public override void ParseJson(JsonObject json) {
        BeginInit();
        try {
            Standard_Ansicht = json.GetString("defaultarrangement", Standard_Ansicht);
            RahmenStil = json.GetEnum("borderstyle", RahmenStil);
            Doppelklick_Skript = json.GetString("doubleclickscript", Doppelklick_Skript);
            base.ParseJson(json);
        } finally {
            EndInit();
        }
    }

    public override bool ParseThis(string key, string value) {
        switch (key) {
            case "id":
                return true;

            case "defaultarrangement":
                Standard_Ansicht = value.FromNonCritical();
                return true;

            case "borderstyle":
                RahmenStil = (GroupBoxStyle)IntParse(value);
                return true;

            case "doubleclickscript":
                Doppelklick_Skript = value.FromNonCritical();
                return true;
        }
        return base.ParseThis(key, value);
    }

    public override string ReadableText() {
        const string txt = "Bearbeitbare Tabelle: ";

        return txt + TableOutput?.Caption;
    }

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Kreis, 16, Color.Transparent, Skin.IdColor(OutputColorId));

    protected override void DrawExplicit(Graphics gr, Rectangle visibleAreaControl, RectangleF positionControl, float zoom, float offsetX, float offsetY, bool forPrinting) {
        if (!forPrinting) {
            DrawArrowOutput(gr, positionControl, zoom, forPrinting, OutputColorId);
            DrawColorScheme(gr, positionControl, zoom, InputColorId, true, true, false);
        }

        base.DrawExplicit(gr, visibleAreaControl, positionControl, zoom, offsetX, offsetY, forPrinting);
        DrawArrorInput(gr, positionControl, zoom, forPrinting, InputColorId);
    }

    #endregion
}