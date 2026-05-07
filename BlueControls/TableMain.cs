using BildzeichenListe.Classes;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Forms;
using System;
using static BildzeichenListe.Classes.Starter;
using static BlueBasics.ClassesStatic.Generic;

namespace BildzeichenListe;

public partial class TableMain : TableViewFormBZL {

    #region Constructors

    public TableMain() : base() {
        InitializeComponent();
        Filter_StdFilterSetzen(btnPrio3, null, null); // Wichtig, da der Prio3 button erst jetzt verfübar ist, die Tabellen aber schon im Parent geladen werden

        capInfo.Text = string.Empty;
    }

    #endregion

    #region Methods

    public override void Table_ViewLoading(object? sender, BlueControls.EventArgs.ViewEventArgs e) {
        btnPrio3.Checked = e.ViewData.GetBool("Prio3");
    }

    public override void Table_ViewSaving(object? sender, BlueControls.EventArgs.ViewEventArgs e) {
        e.ViewData.Add("Prio3", btnPrio3.Checked);
    }

    protected override void Filter_StdFilterSetzen(Button? prio3, ComboBox? belegWahl, KommissionsDaten? kommDaten) => base.Filter_StdFilterSetzen(btnPrio3, null, null);

    private void btnAlleTabellenLaden_Click(object sender, EventArgs e) {
        if (!IsAdministrator()) { return; }
        LoadAllTables(false);
    }

    private void btnBildzeichenVerwendungAllerZeilen_Click(object sender, EventArgs e) {
    }

    private void btnClearSAPCache_Click(object sender, EventArgs e) => SAPBZ.SapInfos.Clear();

    private void btnDraftAbleitung_Click(object sender, EventArgs e) {
    }

    private void btnKplot_Click(object sender, EventArgs e) {
    }

    private void btnPreRelease_Click(object sender, EventArgs e) {
    }

    private void btnPrio3_CheckedChanged(object sender, EventArgs e) {
        var tmp = TableView.Table?.Column["PRIO"];
        if (tmp == null) { return; }
        TableView.FilterFix.Remove(tmp);
        Filter_StdFilterSetzen(btnPrio3, null, null);
    }

    private void btnVorschauVerwaltung_Click(object sender, EventArgs e) {
    }

    private void btnWFMassenAenderung_Click(object sender, EventArgs e) {
        if (!IsAdministrator()) { return; }
        Identischprüfung.btnWorkflowMassen_Click(TableView);
    }

    #endregion
}