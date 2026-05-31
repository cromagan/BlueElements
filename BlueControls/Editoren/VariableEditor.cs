// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Editoren;
using BlueScript.Variables;
using BlueTable.EventArgs;

namespace BlueControls;

public partial class VariableEditor : EditorEasy {

    #region Constructors

    public VariableEditor() : base() => InitializeComponent();

    #endregion

    #region Properties

    public override Type? EditorFor => typeof(VariableCollection);

    #endregion

    #region Methods

    public override void Clear() => tableVariablen.Table?.Row.Clear("Variablen gelöscht");

    public override object? CreateNewItem() {
        if (InputItem is null || IsDisposed || !IsModeSupported()) { return null; }

        var list = new VariableCollection();
        if (tableVariablen.Table is not { IsDisposed: false } tb) { return list; }

        foreach (var thisr in tb.Row) {
            var v = new VariableString(thisr.CellGetString("Name"), thisr.CellGetString("Inhalt"), false, thisr.CellGetString("Kommentar"));
            list.Add(v);
        }

        return list;
    }

    public RowItem? RowOfVariable(string variable) => tableVariablen?.Table is not { IsDisposed: false } tb ? null : tb.Row[variable];

    public RowItem? RowOfVariable(Variable variable) => IsDisposed || tableVariablen?.Table is not { IsDisposed: false } tb ? null : tb.Row[variable.KeyName];

    protected override void SetEnabledState(bool enabled) {
        // Niemals das gesamte Control disabled setzen - Filter müssen bedienbar bleiben.
        // Die Zellbearbeitung ist im OnlyShow-Modus bereits durch DisableAllEditing() gesperrt.
    }

    protected override void InitializeComponentDefaultValues() {
        var tb = Table.Get();
        var na = tb.Column.GenerateAndAdd("Name", "N", ColumnFormatHolder_Systemname.Instance, "Variablenname");
        na.IsFirst = true;
        tb.Column.GenerateAndAdd("Typ", "T", ColumnFormatHolder_TextOneLine.Instance, "Variablentyp");
        tb.Column.GenerateAndAdd("RO", "R", ColumnFormatHolder_Bit.Instance, "Readonly, Schreibgeschützt");
        var inh = tb.Column.GenerateAndAdd("Inhalt", "I", ColumnFormatHolder_TextOneLine.Instance, "Inhalt");
        var kom = tb.Column.GenerateAndAdd("Kommentar", "K", ColumnFormatHolder_TextOneLine.Instance, "Kommentar");

        tb.Column.DisableAllEditing();

        tb.Column.GenerateAndAddSystem();

        if (Mode != EditorMode.OnlyShow) {
            List<ColumnItem?> l = [na, inh, kom];

            foreach (var thisColumn2 in l) {
                if (thisColumn2 != null) {
                    thisColumn2.EditableWithTextInput = true;
                    thisColumn2.MultiLine = false;
                    thisColumn2.PermissionGroupsChangeCell = new([Constants.Everybody]);
                }
            }

            na.Caption = "Variablen-\rName";
            inh.Caption = "Inhalt";
            kom.Caption = "Kommentar";

            tb.PermissionGroupsNewRow = new([Constants.Everybody]);
        }

        var tcvc = ColumnViewCollection.ParseAll(tb);

        //if (car != null) {
        if (Mode != EditorMode.OnlyShow) {
            tcvc[0].ShowColumns("Name", "Inhalt", "Kommentar");
            tcvc[1].ShowColumns("Name", "Inhalt", "Kommentar");
        } else {
            tcvc[0].ShowColumns("Name", "Typ", "RO", "System", "Inhalt", "Kommentar");
            tcvc[1].ShowColumns("Name", "Typ", "RO", "System", "Inhalt", "Kommentar");
        }

        tb.RepairAfterParse();
        tb.ColumnArrangements = tcvc.AsReadOnly();

        tb.SortDefinition = new RowSortDefinition(tb, na, true);

        tableVariablen.Table = tb;

        tb.Cell.CellValueChanged += TableVariablen_CellValueChanged;
    }

    protected override bool SetValuesToFormula(object? toEdit) {
        if (IsDisposed || tableVariablen?.Table is not { IsDisposed: false } tb) { return false; }
        if (toEdit is not VariableCollection vc) { return false; }

        foreach (var thisv in vc) {
            var ro = RowOfVariable(thisv) ?? tb.Row.GenerateAndAdd(thisv.KeyName, "Neue Variable");

            if (ro != null) {
                ro.CellSet("typ", thisv.MyClassId, string.Empty);
                ro.CellSet("RO", thisv.ReadOnly, string.Empty);

                var tmpi = thisv.ReadableText;
                if (Mode == EditorMode.OnlyShow && tmpi.Length > 3990) {
                    tmpi = tmpi[..3990] + "...";
                }

                ro.CellSet("Inhalt", tmpi, string.Empty);
                ro.CellSet("Kommentar", thisv.Comment, string.Empty);
            }
        }

        return true;
    }

    private void TableVariablen_CellValueChanged(object? sender, CellEventArgs e) {
        var c = tableVariablen.Table?.Column.First;
        if (e.Column == c && string.IsNullOrEmpty(e.Row.CellGetString(c))) {
            RowCollection.Remove(e.Row, "Variable gelöscht");
        }
    }

    #endregion
}