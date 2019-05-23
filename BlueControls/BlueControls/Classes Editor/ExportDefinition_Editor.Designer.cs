using System;
using System.Diagnostics;
using System.Drawing;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;

namespace BlueControls.Classes_Editor
{
    internal partial class ExportDefinition_Editor : AbstractClassEditor
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
            this.Caption1 = new Caption();
            this.ExportFilter = new ListBox();
            this.Caption24 = new Caption();
            this.ExportIntervall = new TextBox();
            this.Caption26 = new Caption();
            this.ExportAutomatischLöschen = new TextBox();
            this.Caption27 = new Caption();
            this.BlueFrame2 = new GroupBox();
            this.ExportSpaltenAnsicht = new ComboBox();
            this.ExportFormular = new ComboBox();
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
            //BlueFrame3
            //
            this.BlueFrame3.Anchor = (System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
            this.BlueFrame3.CausesValidation = false;
            this.BlueFrame3.Controls.Add(this.Caption1);
            this.BlueFrame3.Controls.Add(this.ExportFilter);
            this.BlueFrame3.Controls.Add(this.Caption24);
            this.BlueFrame3.Controls.Add(this.ExportIntervall);
            this.BlueFrame3.Controls.Add(this.Caption26);
            this.BlueFrame3.Controls.Add(this.ExportAutomatischLöschen);
            this.BlueFrame3.Controls.Add(this.Caption27);
            this.BlueFrame3.Location = new Point(432, 45);
            this.BlueFrame3.Name = "BlueFrame3";
            this.BlueFrame3.Size = new Size(584, 132);
            this.BlueFrame3.Text = "Optionen für den Export:";
            //
            //Caption1
            //
            this.Caption1.CausesValidation = false;
            this.Caption1.Location = new Point(8, 46);
            this.Caption1.Name = "Caption1";
            this.Caption1.Size = new Size(112, 22);
            this.Caption1.Text = "Automatisch nach";
            //
            //ExportFilter
            //
            this.ExportFilter.AddAllowed = enAddType.UserDef;
            this.ExportFilter.Anchor = (System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
            this.ExportFilter.CheckBehavior = enCheckBehavior.MultiSelection;
            this.ExportFilter.FilterAllowed = true;
            this.ExportFilter.Location = new Point(248, 32);
            this.ExportFilter.Name = "ExportFilter";
            this.ExportFilter.QuickInfo = "";
            this.ExportFilter.RemoveAllowed = true;
            this.ExportFilter.Size = new Size(328, 96);
            this.ExportFilter.TabIndex = 9;
            this.ExportFilter.Translate = false;
            this.ExportFilter.Changed += new EventHandler(ExportFilter_Changed);
            this.ExportFilter.AddClicked += new EventHandler(this.ExportFilter_Add_Clicked);
            //
            //Caption24
            //
            this.Caption24.CausesValidation = false;
            this.Caption24.Location = new Point(8, 24);
            this.Caption24.Name = "Caption24";
            this.Caption24.Size = new Size(104, 22);
            this.Caption24.Text = "Export-Intervall:";
            //
            //ExportIntervall
            //
            this.ExportIntervall.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.ExportIntervall.Format = enDataFormat.Gleitkommazahl;
            this.ExportIntervall.Location = new Point(120, 24);
            this.ExportIntervall.Name = "ExportIntervall";
            this.ExportIntervall.Size = new Size(80, 22);
            this.ExportIntervall.Suffix = "Tag(e)";
            this.ExportIntervall.TabIndex = 15;
            this.ExportIntervall.TextChanged += new EventHandler(ExportVerzeichnis_TextChanged);
            //
            //Caption26
            //
            this.Caption26.CausesValidation = false;
            this.Caption26.Location = new Point(200, 46);
            this.Caption26.Name = "Caption26";
            this.Caption26.Size = new Size(48, 22);
            this.Caption26.Text = "löschen";
            //
            //ExportAutomatischLöschen
            //
            this.ExportAutomatischLöschen.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.ExportAutomatischLöschen.Format = enDataFormat.Gleitkommazahl;
            this.ExportAutomatischLöschen.Location = new Point(120, 46);
            this.ExportAutomatischLöschen.Name = "ExportAutomatischLöschen";
            this.ExportAutomatischLöschen.Size = new Size(80, 22);
            this.ExportAutomatischLöschen.Suffix = "Tag(en)";
            this.ExportAutomatischLöschen.TabIndex = 16;
            this.ExportAutomatischLöschen.TextChanged += new EventHandler(ExportVerzeichnis_TextChanged);
            //
            //Caption27
            //
            this.Caption27.CausesValidation = false;
            this.Caption27.Location = new Point(248, 8);
            this.Caption27.Name = "Caption27";
            this.Caption27.Size = new Size(240, 22);
            this.Caption27.Text = "Nur Einträge, wo diese Filter zutreffen:";
            //
            //BlueFrame2
            //
            this.BlueFrame2.CausesValidation = false;
            this.BlueFrame2.Controls.Add(this.ExportSpaltenAnsicht);
            this.BlueFrame2.Controls.Add(this.ExportFormular);
            this.BlueFrame2.Controls.Add(this.ExportOriginalFormat);
            this.BlueFrame2.Controls.Add(this.ExportHTMLFormat);
            this.BlueFrame2.Controls.Add(this.ExportCSVFormat);
            this.BlueFrame2.Controls.Add(this.ExportalsBild);
            this.BlueFrame2.Controls.Add(this.Caption29);
            this.BlueFrame2.Location = new Point(16, 45);
            this.BlueFrame2.Name = "BlueFrame2";
            this.BlueFrame2.Size = new Size(416, 132);
            this.BlueFrame2.Text = "Typ:";
            //
            //ExportSpaltenAnsicht
            //
            this.ExportSpaltenAnsicht.Anchor = (System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
            this.ExportSpaltenAnsicht.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.ExportSpaltenAnsicht.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ExportSpaltenAnsicht.Location = new Point(264, 68);
            this.ExportSpaltenAnsicht.Name = "ExportSpaltenAnsicht";
            this.ExportSpaltenAnsicht.Size = new Size(144, 22);
            this.ExportSpaltenAnsicht.TabIndex = 7;
            this.ExportSpaltenAnsicht.ItemClicked += new EventHandler<BasicListItemEventArgs>(ExportFormular_Item_Click);
            //
            //ExportFormular
            //
            this.ExportFormular.Anchor = (System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
            this.ExportFormular.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.ExportFormular.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ExportFormular.Location = new Point(264, 90);
            this.ExportFormular.Name = "ExportFormular";
            this.ExportFormular.Size = new Size(144, 22);
            this.ExportFormular.TabIndex = 11;
            this.ExportFormular.ItemClicked += new EventHandler<BasicListItemEventArgs>(ExportFormular_Item_Click);
            //
            //ExportOriginalFormat
            //
            this.ExportOriginalFormat.ButtonStyle = enButtonStyle.Optionbox_Text;
            this.ExportOriginalFormat.Location = new Point(8, 24);
            this.ExportOriginalFormat.Name = "ExportOriginalFormat";
            this.ExportOriginalFormat.Size = new Size(200, 22);
            this.ExportOriginalFormat.TabIndex = 2;
            this.ExportOriginalFormat.Text = "Datenbank im Original-Format";
            this.ExportOriginalFormat.CheckedChanged += new EventHandler(ExportOriginalFormat_CheckedChanged);
            //
            //ExportHTMLFormat
            //
            this.ExportHTMLFormat.ButtonStyle = enButtonStyle.Optionbox_Text;
            this.ExportHTMLFormat.Location = new Point(8, 46);
            this.ExportHTMLFormat.Name = "ExportHTMLFormat";
            this.ExportHTMLFormat.Size = new Size(200, 22);
            this.ExportHTMLFormat.TabIndex = 3;
            this.ExportHTMLFormat.Text = "Datenbank im HTML-Format";
            this.ExportHTMLFormat.CheckedChanged += new EventHandler(ExportOriginalFormat_CheckedChanged);
            //
            //ExportCSVFormat
            //
            this.ExportCSVFormat.ButtonStyle = enButtonStyle.Optionbox_Text;
            this.ExportCSVFormat.Location = new Point(8, 68);
            this.ExportCSVFormat.Name = "ExportCSVFormat";
            this.ExportCSVFormat.Size = new Size(200, 22);
            this.ExportCSVFormat.TabIndex = 4;
            this.ExportCSVFormat.Text = "Datenbank im CSV-Format";
            this.ExportCSVFormat.CheckedChanged += new EventHandler(ExportOriginalFormat_CheckedChanged);
            //
            //ExportalsBild
            //
            this.ExportalsBild.ButtonStyle = enButtonStyle.Optionbox_Text;
            this.ExportalsBild.Location = new Point(8, 90);
            this.ExportalsBild.Name = "ExportalsBild";
            this.ExportalsBild.Size = new Size(240, 22);
            this.ExportalsBild.TabIndex = 5;
            this.ExportalsBild.Text = "Einzeleinträge, mit Vorlagen-Formular:";
            this.ExportalsBild.CheckedChanged += new EventHandler(ExportOriginalFormat_CheckedChanged);
            //
            //Caption29
            //
            this.Caption29.CausesValidation = false;
            this.Caption29.Location = new Point(264, 46);
            this.Caption29.Name = "Caption29";
            this.Caption29.Size = new Size(132, 22);
            this.Caption29.Text = "Export-Spalten-Ansicht:";
            //
            //BlueFrame9
            //
            this.BlueFrame9.Anchor = (System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
            this.BlueFrame9.CausesValidation = false;
            this.BlueFrame9.Controls.Add(this.lsbExportDateien);
            this.BlueFrame9.Location = new Point(16, 181);
            this.BlueFrame9.Name = "BlueFrame9";
            this.BlueFrame9.Size = new Size(1000, 264);
            this.BlueFrame9.Text = "Bereits exportierte Dateien:";
            //
            //lsbExportDateien
            //
            this.lsbExportDateien.AddAllowed = enAddType.None;
            this.lsbExportDateien.Anchor = (System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
            this.lsbExportDateien.CheckBehavior = enCheckBehavior.MultiSelection;
            this.lsbExportDateien.FilterAllowed = true;
            this.lsbExportDateien.Location = new Point(8, 24);
            this.lsbExportDateien.Name = "lsbExportDateien";
            this.lsbExportDateien.QuickInfo = "";
            this.lsbExportDateien.RemoveAllowed = true;
            this.lsbExportDateien.Size = new Size(984, 232);
            this.lsbExportDateien.TabIndex = 0;
            this.lsbExportDateien.RemoveClicked += new EventHandler<ListOfBasicListItemEventArgs>(ExportDateien_RemoveClicked);
            this.lsbExportDateien.Changed += new EventHandler(ExportDateien_Changed);
            //
            //ExportVerzeichnis
            //
            this.ExportVerzeichnis.Anchor = (System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
            this.ExportVerzeichnis.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.ExportVerzeichnis.Location = new Point(136, 16);
            this.ExportVerzeichnis.Name = "ExportVerzeichnis";
            this.ExportVerzeichnis.Size = new Size(880, 22);
            this.ExportVerzeichnis.TabIndex = 4;
            this.ExportVerzeichnis.TextChanged += new EventHandler(ExportVerzeichnis_TextChanged);
            //
            //Caption23
            //
            this.Caption23.CausesValidation = false;
            this.Caption23.Location = new Point(16, 16);
            this.Caption23.Name = "Caption23";
            this.Caption23.Size = new Size(120, 22);
            this.Caption23.Text = "Export-Verzeichnis:";
            //
            //ExportDefinition_Editor
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
        private ListBox ExportFilter;
        private Caption Caption24;
        private TextBox ExportIntervall;
        private Caption Caption26;
        private TextBox ExportAutomatischLöschen;
        private Caption Caption27;
        private GroupBox BlueFrame2;
        private ComboBox ExportSpaltenAnsicht;
        private ComboBox ExportFormular;
        private Button ExportOriginalFormat;
        private Button ExportHTMLFormat;
        private Button ExportCSVFormat;
        private Button ExportalsBild;
        private Caption Caption29;
        private GroupBox BlueFrame9;
        private ListBox lsbExportDateien;
        private TextBox ExportVerzeichnis;
        private Caption Caption23;
    }
}
