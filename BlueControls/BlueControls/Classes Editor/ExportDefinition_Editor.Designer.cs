using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using Button = BlueControls.Controls.Button;
using ComboBox = BlueControls.Controls.ComboBox;
using GroupBox = BlueControls.Controls.GroupBox;
using ListBox = BlueControls.Controls.ListBox;
using TextBox = BlueControls.Controls.TextBox;

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
            this.BlueFrame3 = new GroupBox();
            this.filterItemEditor = new FilterItem_Editor();
            this.Caption1 = new Caption();
            this.lbxFilter = new ListBox();
            this.Caption24 = new Caption();
            this.ExportIntervall = new TextBox();
            this.Caption26 = new Caption();
            this.ExportAutomatischLöschen = new TextBox();
            this.Caption27 = new Caption();
            this.BlueFrame2 = new GroupBox();
            this.ExportSpaltenAnsicht = new ComboBox();
            this.cbxExportFormularID = new ComboBox();
            this.ExportOriginalFormat = new Button();
            this.ExportHTMLFormat = new Button();
            this.ExportCSVFormat = new Button();
            this.ExportalsBild = new Button();
            this.Caption29 = new Caption();
            this.BlueFrame9 = new GroupBox();
            this.lsbExportDateien = new ListBox();
            this.ExportVerzeichnis = new TextBox();
            this.Caption23 = new Caption();
            this.BlueFrame3.SuspendLayout();
            this.BlueFrame2.SuspendLayout();
            this.BlueFrame9.SuspendLayout();
            this.SuspendLayout();
            // 
            // BlueFrame3
            // 
            this.BlueFrame3.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                      | AnchorStyles.Right)));
            this.BlueFrame3.CausesValidation = false;
            this.BlueFrame3.Controls.Add(this.filterItemEditor);
            this.BlueFrame3.Controls.Add(this.Caption1);
            this.BlueFrame3.Controls.Add(this.lbxFilter);
            this.BlueFrame3.Controls.Add(this.Caption24);
            this.BlueFrame3.Controls.Add(this.ExportIntervall);
            this.BlueFrame3.Controls.Add(this.Caption26);
            this.BlueFrame3.Controls.Add(this.ExportAutomatischLöschen);
            this.BlueFrame3.Controls.Add(this.Caption27);
            this.BlueFrame3.Location = new Point(432, 45);
            this.BlueFrame3.Name = "BlueFrame3";
            this.BlueFrame3.Size = new Size(584, 227);
            this.BlueFrame3.TabIndex = 0;
            this.BlueFrame3.TabStop = false;
            this.BlueFrame3.Text = "Optionen für den Export:";
            // 
            // filterItemEditor
            // 
            this.filterItemEditor.Location = new Point(248, 152);
            this.filterItemEditor.Name = "filterItemEditor";
            this.filterItemEditor.Size = new Size(328, 68);
            this.filterItemEditor.TabIndex = 17;
            this.filterItemEditor.TabStop = false;
            this.filterItemEditor.Text = "Filter bearbeiten:";
            this.filterItemEditor.Changed += new EventHandler(this.filterItemEditor_Changed);
            // 
            // Caption1
            // 
            this.Caption1.CausesValidation = false;
            this.Caption1.Location = new Point(8, 46);
            this.Caption1.Name = "Caption1";
            this.Caption1.Size = new Size(112, 22);
            this.Caption1.Text = "Automatisch nach";
            // 
            // lbxFilter
            // 
            this.lbxFilter.AddAllowed = AddType.UserDef;
            this.lbxFilter.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                     | AnchorStyles.Right)));
            this.lbxFilter.FilterAllowed = true;
            this.lbxFilter.Location = new Point(248, 32);
            this.lbxFilter.Name = "lbxFilter";
            this.lbxFilter.RemoveAllowed = true;
            this.lbxFilter.Size = new Size(328, 120);
            this.lbxFilter.TabIndex = 9;
            this.lbxFilter.AddClicked += new EventHandler(this.lbxFilter_AddClicked);
            this.lbxFilter.ItemCheckedChanged += new EventHandler(this.lbxFilter_ItemCheckedChanged);
            this.lbxFilter.ListOrItemChanged += new EventHandler(this.lbxFilter_ListOrItemChanged);
            // 
            // Caption24
            // 
            this.Caption24.CausesValidation = false;
            this.Caption24.Location = new Point(8, 24);
            this.Caption24.Name = "Caption24";
            this.Caption24.Size = new Size(104, 22);
            this.Caption24.Text = "Export-Intervall:";
            // 
            // ExportIntervall
            // 
            this.ExportIntervall.Cursor = Cursors.IBeam;
            this.ExportIntervall.Location = new Point(120, 24);
            this.ExportIntervall.Name = "ExportIntervall";
            this.ExportIntervall.Size = new Size(80, 22);
            this.ExportIntervall.Suffix = "Tag(e)";
            this.ExportIntervall.TabIndex = 15;
            this.ExportIntervall.TextChanged += new EventHandler(this.ExportVerzeichnis_TextChanged);
            // 
            // Caption26
            // 
            this.Caption26.CausesValidation = false;
            this.Caption26.Location = new Point(200, 46);
            this.Caption26.Name = "Caption26";
            this.Caption26.Size = new Size(48, 22);
            this.Caption26.Text = "löschen";
            // 
            // ExportAutomatischLöschen
            // 
            this.ExportAutomatischLöschen.Cursor = Cursors.IBeam;
            this.ExportAutomatischLöschen.Location = new Point(120, 46);
            this.ExportAutomatischLöschen.Name = "ExportAutomatischLöschen";
            this.ExportAutomatischLöschen.Size = new Size(80, 22);
            this.ExportAutomatischLöschen.Suffix = "Tag(en)";
            this.ExportAutomatischLöschen.TabIndex = 16;
            this.ExportAutomatischLöschen.TextChanged += new EventHandler(this.ExportVerzeichnis_TextChanged);
            // 
            // Caption27
            // 
            this.Caption27.CausesValidation = false;
            this.Caption27.Location = new Point(248, 8);
            this.Caption27.Name = "Caption27";
            this.Caption27.Size = new Size(240, 22);
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
            this.BlueFrame2.Location = new Point(16, 45);
            this.BlueFrame2.Name = "BlueFrame2";
            this.BlueFrame2.Size = new Size(416, 227);
            this.BlueFrame2.TabIndex = 1;
            this.BlueFrame2.TabStop = false;
            this.BlueFrame2.Text = "Typ:";
            // 
            // ExportSpaltenAnsicht
            // 
            this.ExportSpaltenAnsicht.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                                | AnchorStyles.Right)));
            this.ExportSpaltenAnsicht.Cursor = Cursors.IBeam;
            this.ExportSpaltenAnsicht.DropDownStyle = ComboBoxStyle.DropDownList;
            this.ExportSpaltenAnsicht.Location = new Point(264, 68);
            this.ExportSpaltenAnsicht.Name = "ExportSpaltenAnsicht";
            this.ExportSpaltenAnsicht.Size = new Size(144, 22);
            this.ExportSpaltenAnsicht.TabIndex = 7;
            this.ExportSpaltenAnsicht.ItemClicked += new EventHandler<BasicListItemEventArgs>(this.cbxExportFormularID_ItemClicked);
            // 
            // cbxExportFormularID
            // 
            this.cbxExportFormularID.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                               | AnchorStyles.Right)));
            this.cbxExportFormularID.Cursor = Cursors.IBeam;
            this.cbxExportFormularID.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbxExportFormularID.Location = new Point(264, 90);
            this.cbxExportFormularID.Name = "cbxExportFormularID";
            this.cbxExportFormularID.Size = new Size(144, 22);
            this.cbxExportFormularID.TabIndex = 11;
            this.cbxExportFormularID.ItemClicked += new EventHandler<BasicListItemEventArgs>(this.cbxExportFormularID_ItemClicked);
            // 
            // ExportOriginalFormat
            // 
            this.ExportOriginalFormat.ButtonStyle = ((ButtonStyle)((ButtonStyle.Optionbox | ButtonStyle.Text)));
            this.ExportOriginalFormat.Location = new Point(8, 24);
            this.ExportOriginalFormat.Name = "ExportOriginalFormat";
            this.ExportOriginalFormat.Size = new Size(200, 22);
            this.ExportOriginalFormat.TabIndex = 2;
            this.ExportOriginalFormat.Text = "Datenbank im Original-Format";
            this.ExportOriginalFormat.CheckedChanged += new EventHandler(this.ExportOriginalFormat_CheckedChanged);
            // 
            // ExportHTMLFormat
            // 
            this.ExportHTMLFormat.ButtonStyle = ((ButtonStyle)((ButtonStyle.Optionbox | ButtonStyle.Text)));
            this.ExportHTMLFormat.Location = new Point(8, 46);
            this.ExportHTMLFormat.Name = "ExportHTMLFormat";
            this.ExportHTMLFormat.Size = new Size(200, 22);
            this.ExportHTMLFormat.TabIndex = 3;
            this.ExportHTMLFormat.Text = "Datenbank im HTML-Format";
            this.ExportHTMLFormat.CheckedChanged += new EventHandler(this.ExportOriginalFormat_CheckedChanged);
            // 
            // ExportCSVFormat
            // 
            this.ExportCSVFormat.ButtonStyle = ((ButtonStyle)((ButtonStyle.Optionbox | ButtonStyle.Text)));
            this.ExportCSVFormat.Location = new Point(8, 68);
            this.ExportCSVFormat.Name = "ExportCSVFormat";
            this.ExportCSVFormat.Size = new Size(200, 22);
            this.ExportCSVFormat.TabIndex = 4;
            this.ExportCSVFormat.Text = "Datenbank im CSV-Format";
            this.ExportCSVFormat.CheckedChanged += new EventHandler(this.ExportOriginalFormat_CheckedChanged);
            // 
            // ExportalsBild
            // 
            this.ExportalsBild.ButtonStyle = ((ButtonStyle)((ButtonStyle.Optionbox | ButtonStyle.Text)));
            this.ExportalsBild.Location = new Point(8, 90);
            this.ExportalsBild.Name = "ExportalsBild";
            this.ExportalsBild.Size = new Size(240, 22);
            this.ExportalsBild.TabIndex = 5;
            this.ExportalsBild.Text = "Einzeleinträge, mit Vorlagen-Formular:";
            this.ExportalsBild.CheckedChanged += new EventHandler(this.ExportOriginalFormat_CheckedChanged);
            // 
            // Caption29
            // 
            this.Caption29.CausesValidation = false;
            this.Caption29.Location = new Point(264, 46);
            this.Caption29.Name = "Caption29";
            this.Caption29.Size = new Size(132, 22);
            this.Caption29.Text = "Export-Spalten-Ansicht:";
            // 
            // BlueFrame9
            // 
            this.BlueFrame9.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                       | AnchorStyles.Left) 
                                                      | AnchorStyles.Right)));
            this.BlueFrame9.Controls.Add(this.lsbExportDateien);
            this.BlueFrame9.Location = new Point(16, 280);
            this.BlueFrame9.Name = "BlueFrame9";
            this.BlueFrame9.Size = new Size(1000, 165);
            this.BlueFrame9.TabIndex = 2;
            this.BlueFrame9.TabStop = false;
            this.BlueFrame9.Text = "Bereits exportierte Dateien:";
            // 
            // lsbExportDateien
            // 
            this.lsbExportDateien.AddAllowed = AddType.None;
            this.lsbExportDateien.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                             | AnchorStyles.Left) 
                                                            | AnchorStyles.Right)));
            this.lsbExportDateien.CheckBehavior = CheckBehavior.MultiSelection;
            this.lsbExportDateien.FilterAllowed = true;
            this.lsbExportDateien.Location = new Point(8, 24);
            this.lsbExportDateien.Name = "lsbExportDateien";
            this.lsbExportDateien.RemoveAllowed = false;
            this.lsbExportDateien.Size = new Size(984, 133);
            this.lsbExportDateien.TabIndex = 0;
            this.lsbExportDateien.ListOrItemChanged += new EventHandler(this.ExportDateien_ListOrItemChanged);
            // 
            // ExportVerzeichnis
            // 
            this.ExportVerzeichnis.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                             | AnchorStyles.Right)));
            this.ExportVerzeichnis.Cursor = Cursors.IBeam;
            this.ExportVerzeichnis.Location = new Point(136, 16);
            this.ExportVerzeichnis.Name = "ExportVerzeichnis";
            this.ExportVerzeichnis.Size = new Size(880, 22);
            this.ExportVerzeichnis.TabIndex = 4;
            this.ExportVerzeichnis.TextChanged += new EventHandler(this.ExportVerzeichnis_TextChanged);
            // 
            // Caption23
            // 
            this.Caption23.CausesValidation = false;
            this.Caption23.Location = new Point(16, 16);
            this.Caption23.Name = "Caption23";
            this.Caption23.Size = new Size(120, 22);
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
            this.Size = new Size(1031, 459);
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
