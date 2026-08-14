// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Editoren;
using BlueControls.EventArgs;
using BlueScript.Classes;
using BlueScript.ScriptVariables;

namespace BlueControls.Forms;

/// <summary>
/// Dialog zur Vorschau eines Formulars mit den Variablen einzelner Zeilen
/// der Referenztabelle. Links wird eine Liste aller Zeilen angezeigt,
/// rechts die Live-Vorschau des Formulars. Unten erscheinen die vom
/// Export-Skript berechneten Variablen (readonly).
/// </summary>
public sealed partial class ReferenceTablePreviewForm : Form {

    #region Fields

    private readonly CollectionPadItem _original;
    private readonly Table _referenceTable;

    #endregion

    #region Constructors

    /// <summary>
    /// Erzeugt einen neuen Vorschau-Dialog für die übergebene Collection
    /// und deren Referenztabelle.
    /// </summary>
    public ReferenceTablePreviewForm(CollectionPadItem original, Table referenceTable) : base() {
        InitializeComponent();

        _original = original;
        _referenceTable = referenceTable;

        Text = "Vorschau: " + original.BestCaption();

        varEditor.Mode = EditorMode.OnlyShow;

        PopulateRowList();

        lstRows.ItemClicked += LstRows_ItemClicked;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Erzeugt einen Klon der Original-Collection über JSON-Serialisierung,
    /// damit das Original durch Variablen-Ersetzung nicht verändert wird.
    /// </summary>
    private static CollectionPadItem? CloneCollection(CollectionPadItem original) {
        var json = original.ParseableJson();
        return ParseableItem.NewByParsingJson<CollectionPadItem>(json);
    }

    private void LstRows_ItemClicked(object sender, ListItemEventArgs e) {
        if (_referenceTable is not { IsDisposed: false } tb) { return; }
        if (tb.Row.GetByKey(e.Item.KeyName) is not { IsDisposed: false } row) { return; }

        UpdatePreview(row);
    }

    private void PopulateRowList() {
        lstRows.ItemClear();

        if (_referenceTable is not { IsDisposed: false } tb) { return; }

        List<ListItem> items = [];
        foreach (var thisRow in tb.Row) {
            if (thisRow is not { IsDisposed: false }) { continue; }
            items.Add(ItemOf(thisRow.CellFirstString(), thisRow.KeyName, ImageCode.Zeile));
        }

        lstRows.ItemAddRange(items);
    }

    /// <summary>
    /// Aktualisiert die Vorschau und den Variablen-Editor für die
    /// übergebene Zeile. Die Collection wird dafür neu geklont, damit
    /// aufeinanderfolgende Vorschauen nicht verfälscht werden.
    /// </summary>
    private void UpdatePreview(RowItem row) {
        var clone = CloneCollection(_original);
        if (clone is null) { return; }

        clone.ResetVariables();
        var feedback = clone.ReplaceVariables(row);

        padVorschau.Items = clone;
        padVorschau.ZoomFit();

        UpdateVariableEditor(feedback);
    }

    private void UpdateVariableEditor(ScriptEndedFeedback feedback) {
        VariableCollection vc;

        if (feedback.Failed) {
            vc = [new StringScriptVariable("Fehler", feedback.ProtocolText, true, "Fehler beim Ausführen des Export-Skripts")];
        } else if (feedback.Variables is { Count: > 0 } vars) {
            vc = new VariableCollection(vars.ToList(), false);
        } else {
            vc = [new StringScriptVariable("Hinweis", "Keine Variablen berechnet.", true, "Export-Skript lieferte keine Variablen")];
        }

        varEditor.InputItem = vc;
    }

    #endregion
}