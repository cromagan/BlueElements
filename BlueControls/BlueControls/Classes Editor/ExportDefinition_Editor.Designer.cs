using System.Diagnostics;
using BlueControls.Controls;

namespace BlueControls.Classes_Editor
{
    internal partial class ExportDefinition_Editor 
    {
        //UserControl überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.BlueFrame3 = new BlueControls.Controls.GroupBox();
            this.filterItemEditor = new BlueControls.Classes_Editor.FilterItem_Editor();
            this.Caption1 = new BlueControls.Controls.Caption();
            this.lbxFilter = new BlueControls.Controls.ListBox();
            this.Caption24 = new BlueControls.Controls.Caption();
            this.ExportIntervall = new BlueControls.Controls.TextBox();
            this.Caption26 = new BlueControls.Controls.Caption();
            this.ExportAutomatischLöschen = new BlueControls.Controls.TextBox();
            this.Caption27 = new BlueControls.Controls.Caption();
            this.BlueFrame2 = new BlueControls.Controls.GroupBox();
            this.ExportSpaltenAnsicht = new BlueControls.Controls.ComboBox();
            this.cbxExportFormularID = new BlueControls.Controls.ComboBox();
            this.ExportOriginalFormat = new BlueControls.Controls.Button();
            this.ExportHTMLFormat = new BlueControls.Controls.Button();
            this.ExportCSVFormat = new BlueControls.Controls.Button();
            this.ExportalsBild = new BlueControls.Controls.Button();
            this.Caption29 = new BlueControls.Controls.Caption();
            this.BlueFrame9 = new BlueControls.Controls.GroupBox();
            this.lsbExportDateien = new BlueControls.Controls.ListBox();
            this.ExportVerzeichnis = new BlueControls.Controls.TextBox();
            this.Caption23 = new BlueControls.Controls.Caption();
            this.BlueFrame3.SuspendLayout();
            this.BlueFrame2.SuspendLayout();
            this.BlueFrame9.SuspendLayout();
            this.SuspendLayout();
            // 
            // BlueFrame3
            // 
            this.BlueFrame3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BlueFrame3.CausesValidation = false;
            this.BlueFrame3.Controls.Add(this.filterItemEditor);
            this.BlueFrame3.Controls.Add(this.Caption1);
            this.BlueFrame3.Controls.Add(this.lbxFilter);
            this.BlueFrame3.Controls.Add(this.Caption24);
            this.BlueFrame3.Controls.Add(this.ExportIntervall);
            this.BlueFrame3.Controls.Add(this.Caption26);
            this.BlueFrame3.Controls.Add(this.ExportAutomatischLöschen);
            this.BlueFrame3.Controls.Add(this.Caption27);
            this.BlueFrame3.Location = new System.Drawing.Point(432, 45);
            this.BlueFrame3.Name = "BlueFrame3";
            this.BlueFrame3.Size = new System.Drawing.Size(584, 227);
            this.BlueFrame3.TabIndex = 0;
            this.BlueFrame3.TabStop = false;
            this.BlueFrame3.Text = "Optionen für den Export:";
            // 
            // filterItemEditor
            // 
            this.filterItemEditor.Location = new System.Drawing.Point(248, 152);
            this.filterItemEditor.Name = "filterItemEditor";
            this.filterItemEditor.Size = new System.Drawing.Size(328, 68);
            this.filterItemEditor.TabIndex = 17;
            this.filterItemEditor.TabStop = false;
            this.filterItemEditor.Text = "Filter bearbeiten:";
            this.filterItemEditor.Changed += new System.EventHandler(this.filterItemEditor_Changed);
            // 
            // Caption1
            // 
            this.Caption1.CausesValidation = false;
            this.Caption1.Location = new System.Drawing.Point(8, 46);
            this.Caption1.Name = "Caption1";
            this.Caption1.Size = new System.Drawing.Size(112, 22);
            this.Caption1.Text = "Automatisch nach";
            // 
            // lbxFilter
            // 
            this.lbxFilter.AddAllowed = BlueControls.Enums.AddType.UserDef;
            this.lbxFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbxFilter.FilterAllowed = true;
            this.lbxFilter.Location = new System.Drawing.Point(248, 32);
            this.lbxFilter.Name = "lbxFilter";
            this.lbxFilter.RemoveAllowed = true;
            this.lbxFilter.Size = new System.Drawing.Size(328, 120);
            this.lbxFilter.TabIndex = 9;
            this.lbxFilter.AddClicked += new System.EventHandler(this.lbxFilter_AddClicked);
            this.lbxFilter.ItemCheckedChanged += new System.EventHandler(this.lbxFilter_ItemCheckedChanged);
            this.lbxFilter.ListOrItemChanged += new System.EventHandler(this.lbxFilter_ListOrItemChanged);
            // 
            // Caption24
            // 
            this.Caption24.CausesValidation = false;
            this.Caption24.Location = new System.Drawing.Point(8, 24);
            this.Caption24.Name = "Caption24";
            this.Caption24.Size = new System.Drawing.Size(104, 22);
            this.Caption24.Text = "Export-Intervall:";
            // 
            // ExportIntervall
            // 
            this.ExportIntervall.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.ExportIntervall.Location = new System.Drawing.Point(120, 24);
            this.ExportIntervall.Name = "ExportIntervall";
            this.ExportIntervall.Size = new System.Drawing.Size(80, 22);
            this.ExportIntervall.Suffix = "Tag(e)";
            this.ExportIntervall.TabIndex = 15;
            this.ExportIntervall.TextChanged += new System.EventHandler(this.ExportVerzeichnis_TextChanged);
            // 
            // Caption26
            // 
            this.Caption26.CausesValidation = false;
            this.Caption26.Location = new System.Drawing.Point(200, 46);
            this.Caption26.Name = "Caption26";
            this.Caption26.Size = new System.Drawing.Size(48, 22);
            this.Caption26.Text = "löschen";
            // 
            // ExportAutomatischLöschen
            // 
            this.ExportAutomatischLöschen.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.ExportAutomatischLöschen.Location = new System.Drawing.Point(120, 46);
            this.ExportAutomatischLöschen.Name = "ExportAutomatischLöschen";
            this.ExportAutomatischLöschen.Size = new System.Drawing.Size(80, 22);
            this.ExportAutomatischLöschen.Suffix = "Tag(en)";
            this.ExportAutomatischLöschen.TabIndex = 16;
            this.ExportAutomatischLöschen.TextChanged += new System.EventHandler(this.ExportVerzeichnis_TextChanged);
            // 
            // Caption27
            // 
            this.Caption27.CausesValidation = false;
            this.Caption27.Location = new System.Drawing.Point(248, 8);
            this.Caption27.Name = "Caption27";
            this.Caption27.Size = new System.Drawing.Size(240, 22);
            this.Caption27.Text = "Nur Einträge, wo diese Filter zutreffen:";
            // 
            // BlueFrame2
            // 
            this.BlueFrame2.CausesValidation = false;
            this.BlueFrame2.Controls.Add(this.ExportSpaltenAnsicht);
            this.BlueFrame2.Controls.Add(this.cbxExportFormularID);
            this.BlueFrame2.Controls.Add(this.ExportOriginalFormat);
            this.BlueFrame2.Controls.Add(this.ExportHTMLFormat);
            this.BlueFrame2.Controls.Add(this.ExportCSVFormat);
            this.BlueFrame2.Controls.Add(this.ExportalsBild);
            this.BlueFrame2.Controls.Add(this.Caption29);
            this.BlueFrame2.Location = new System.Drawing.Point(16, 45);
            this.BlueFrame2.Name = "BlueFrame2";
            this.BlueFrame2.Size = new System.Drawing.Size(416, 227);
            this.BlueFrame2.TabIndex = 1;
            this.BlueFrame2.TabStop = false;
            this.BlueFrame2.Text = "Typ:";
            // 
            // ExportSpaltenAnsicht
            // 
            this.ExportSpaltenAnsicht.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ExportSpaltenAnsicht.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.ExportSpaltenAnsicht.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ExportSpaltenAnsicht.Location = new System.Drawing.Point(264, 68);
            this.ExportSpaltenAnsicht.Name = "ExportSpaltenAnsicht";
            this.ExportSpaltenAnsicht.Size = new System.Drawing.Size(144, 22);
            this.ExportSpaltenAnsicht.TabIndex = 7;
            this.ExportSpaltenAnsicht.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.cbxExportFormularID_ItemClicked);
            // 
            // cbxExportFormularID
            // 
            this.cbxExportFormularID.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxExportFormularID.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxExportFormularID.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxExportFormularID.Location = new System.Drawing.Point(264, 90);
            this.cbxExportFormularID.Name = "cbxExportFormularID";
            this.cbxExportFormularID.Size = new System.Drawing.Size(144, 22);
            this.cbxExportFormularID.TabIndex = 11;
            this.cbxExportFormularID.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.cbxExportFormularID_ItemClicked);
            // 
            // ExportOriginalFormat
            // 
            this.ExportOriginalFormat.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Optionbox | BlueControls.Enums.ButtonStyle.Text)));
            this.ExportOriginalFormat.Location = new System.Drawing.Point(8, 24);
            this.ExportOriginalFormat.Name = "ExportOriginalFormat";
            this.ExportOriginalFormat.Size = new System.Drawing.Size(200, 22);
            this.ExportOriginalFormat.TabIndex = 2;
            this.ExportOriginalFormat.Text = "Datenbank im Original-Format";
            this.ExportOriginalFormat.CheckedChanged += new System.EventHandler(this.ExportOriginalFormat_CheckedChanged);
            // 
            // ExportHTMLFormat
            // 
            this.ExportHTMLFormat.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Optionbox | BlueControls.Enums.ButtonStyle.Text)));
            this.ExportHTMLFormat.Location = new System.Drawing.Point(8, 46);
            this.ExportHTMLFormat.Name = "ExportHTMLFormat";
            this.ExportHTMLFormat.Size = new System.Drawing.Size(200, 22);
            this.ExportHTMLFormat.TabIndex = 3;
            this.ExportHTMLFormat.Text = "Datenbank im HTML-Format";
            this.ExportHTMLFormat.CheckedChanged += new System.EventHandler(this.ExportOriginalFormat_CheckedChanged);
            // 
            // ExportCSVFormat
            // 
            this.ExportCSVFormat.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Optionbox | BlueControls.Enums.ButtonStyle.Text)));
            this.ExportCSVFormat.Location = new System.Drawing.Point(8, 68);
            this.ExportCSVFormat.Name = "ExportCSVFormat";
            this.ExportCSVFormat.Size = new System.Drawing.Size(200, 22);
            this.ExportCSVFormat.TabIndex = 4;
            this.ExportCSVFormat.Text = "Datenbank im CSV-Format";
            this.ExportCSVFormat.CheckedChanged += new System.EventHandler(this.ExportOriginalFormat_CheckedChanged);
            // 
            // ExportalsBild
            // 
            this.ExportalsBild.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Optionbox | BlueControls.Enums.ButtonStyle.Text)));
            this.ExportalsBild.Location = new System.Drawing.Point(8, 90);
            this.ExportalsBild.Name = "ExportalsBild";
            this.ExportalsBild.Size = new System.Drawing.Size(240, 22);
            this.ExportalsBild.TabIndex = 5;
            this.ExportalsBild.Text = "Einzeleinträge, mit Vorlagen-Formular:";
            this.ExportalsBild.CheckedChanged += new System.EventHandler(this.ExportOriginalFormat_CheckedChanged);
            // 
            // Caption29
            // 
            this.Caption29.CausesValidation = false;
            this.Caption29.Location = new System.Drawing.Point(264, 46);
            this.Caption29.Name = "Caption29";
            this.Caption29.Size = new System.Drawing.Size(132, 22);
            this.Caption29.Text = "Export-Spalten-Ansicht:";
            // 
            // BlueFrame9
            // 
            this.BlueFrame9.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BlueFrame9.Controls.Add(this.lsbExportDateien);
            this.BlueFrame9.Location = new System.Drawing.Point(16, 280);
            this.BlueFrame9.Name = "BlueFrame9";
            this.BlueFrame9.Size = new System.Drawing.Size(1000, 165);
            this.BlueFrame9.TabIndex = 2;
            this.BlueFrame9.TabStop = false;
            this.BlueFrame9.Text = "Bereits exportierte Dateien:";
            // 
            // lsbExportDateien
            // 
            this.lsbExportDateien.AddAllowed = BlueControls.Enums.AddType.None;
            this.lsbExportDateien.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lsbExportDateien.CheckBehavior = BlueControls.Enums.CheckBehavior.MultiSelection;
            this.lsbExportDateien.FilterAllowed = true;
            this.lsbExportDateien.Location = new System.Drawing.Point(8, 24);
            this.lsbExportDateien.Name = "lsbExportDateien";
            this.lsbExportDateien.RemoveAllowed = true;
            this.lsbExportDateien.Size = new System.Drawing.Size(984, 133);
            this.lsbExportDateien.TabIndex = 0;
            this.lsbExportDateien.ListOrItemChanged += new System.EventHandler(this.ExportDateien_ListOrItemChanged);
            this.lsbExportDateien.RemoveClicked += new System.EventHandler<BlueControls.EventArgs.ListOfBasicListItemEventArgs>(this.ExportDateien_RemoveClicked);
            // 
            // ExportVerzeichnis
            // 
            this.ExportVerzeichnis.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ExportVerzeichnis.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.ExportVerzeichnis.Location = new System.Drawing.Point(136, 16);
            this.ExportVerzeichnis.Name = "ExportVerzeichnis";
            this.ExportVerzeichnis.Size = new System.Drawing.Size(880, 22);
            this.ExportVerzeichnis.TabIndex = 4;
            this.ExportVerzeichnis.TextChanged += new System.EventHandler(this.ExportVerzeichnis_TextChanged);
            // 
            // Caption23
            // 
            this.Caption23.CausesValidation = false;
            this.Caption23.Location = new System.Drawing.Point(16, 16);
            this.Caption23.Name = "Caption23";
            this.Caption23.Size = new System.Drawing.Size(120, 22);
            this.Caption23.Text = "Export-Verzeichnis:";
            // 
            // ExportDefinition_Editor
            // 
            this.Controls.Add(this.BlueFrame3);
            this.Controls.Add(this.BlueFrame2);
            this.Controls.Add(this.BlueFrame9);
            this.Controls.Add(this.ExportVerzeichnis);
            this.Controls.Add(this.Caption23);
            this.Name = "ExportDefinition_Editor";
            this.Size = new System.Drawing.Size(1031, 459);
            this.BlueFrame3.ResumeLayout(false);
            this.BlueFrame2.ResumeLayout(false);
            this.BlueFrame9.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private GroupBox BlueFrame3;
        private Caption Caption1;
        private ListBox lbxFilter;
        private Caption Caption24;
        private TextBox ExportIntervall;
        private Caption Caption26;
        private TextBox ExportAutomatischLöschen;
        private Caption Caption27;
        private GroupBox BlueFrame2;
        private ComboBox ExportSpaltenAnsicht;
        private ComboBox cbxExportFormularID;
        private Button ExportOriginalFormat;
        private Button ExportHTMLFormat;
        private Button ExportCSVFormat;
        private Button ExportalsBild;
        private Caption Caption29;
        private GroupBox BlueFrame9;
        private ListBox lsbExportDateien;
        private TextBox ExportVerzeichnis;
        private Caption Caption23;
        private FilterItem_Editor filterItemEditor;
    }
}
