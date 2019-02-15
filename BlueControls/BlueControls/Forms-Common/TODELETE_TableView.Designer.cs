using System;
using System.Diagnostics;
using System.Drawing;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;

namespace BlueControls.Forms
    {


        public sealed partial class TableView : Form
        {
			//Das Formular überschreibt den Deletevorgang, um die Komponentenliste zu bereinigen.
			[DebuggerNonUserCode()]
			protected override void Dispose(bool disposing)
			{
				try
				{
					if (disposing )
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
            this.tbl = new Table();
            this.Views = new ComboBox();
            this.SearchT = new TextBox();
            this.FilterAus = new Button();
            this.SearchB = new Button();
            this.TabControl1 = new TabControl();
            this.TabPage1 = new TabPage();
            this.GroupBox2 = new GroupBox();
            this.SpaltAnsichtCap = new Caption();
            this.GroupBox1 = new GroupBox();
            this.OK = new Button();
            this.TabControl1.SuspendLayout();
            this.TabPage1.SuspendLayout();
            this.GroupBox2.SuspendLayout();
            this.GroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbl
            // 
            this.tbl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbl.Location = new Point(0, 110);
            this.tbl.Name = "tbl";
            this.tbl.Size = new Size(772, 426);
            this.tbl.TabIndex = 0;
            this.tbl.RowsSorted += new  EventHandler(this.tbl_RowsSorted);
            // 
            // Views
            // 
            this.Views.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.Views.Format = enDataFormat.Text_Ohne_Kritische_Zeichen;
            this.Views.Location = new Point(112, 2);
            this.Views.Name = "Views";
            this.Views.Size = new Size(152, 22);
            this.Views.TabIndex = 16;
            // 
            // SearchT
            // 
            this.SearchT.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.SearchT.Location = new Point(0, 2);
            this.SearchT.Name = "SearchT";
            this.SearchT.Size = new Size(176, 22);
            this.SearchT.TabIndex = 12;
            this.SearchT.TextChanged += new EventHandler(this.SearchT_TextChange);
            this.SearchT.Enter += new  EventHandler(this.SearchT_Enter);
            // 
            // FilterAus
            // 
            this.FilterAus.ImageCode = "Trichter|||1";
            this.FilterAus.Location = new Point(240, 2);
            this.FilterAus.Name = "FilterAus";
            this.FilterAus.Size = new Size(56, 66);
            this.FilterAus.TabIndex = 14;
            this.FilterAus.Text = "alle Filter aus";
            this.FilterAus.Click += new EventHandler(this.FilterAus_Click);
            // 
            // SearchB
            // 
            this.SearchB.ButtonStyle = enButtonStyle.Pic1_or_Pic2;
            this.SearchB.Enabled = false;
            this.SearchB.ImageCode = "Trichter";
            this.SearchB.ImageCode_Checked = "Trichter||||FF0000";
            this.SearchB.Location = new Point(184, 2);
            this.SearchB.Name = "SearchB";
            this.SearchB.QuickInfo = "Zeigt nur noch Zeilen, in dem der<br>eingegeben Text vorkommt.";
            this.SearchB.Size = new Size(56, 66);
            this.SearchB.TabIndex = 13;
            this.SearchB.Text = "Text filtern";
            this.SearchB.Click += new EventHandler(this.SearchB_Click);
            // 
            // TabControl1
            // 
            this.TabControl1.Controls.Add(this.TabPage1);
            this.TabControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.TabControl1.HotTrack = true;
            this.TabControl1.IsRibbonBar = true;
            this.TabControl1.Location = new Point(0, 0);
            this.TabControl1.Name = "TabControl1";
            this.TabControl1.Size = new Size(772, 110);
            this.TabControl1.TabIndex = 1;
            // 
            // TabPage1
            // 
            this.TabPage1.Controls.Add(this.GroupBox2);
            this.TabPage1.Controls.Add(this.GroupBox1);
            this.TabPage1.Location = new Point(4, 25);
            this.TabPage1.Name = "TabPage1";
            this.TabPage1.Size = new Size(764, 81);
            this.TabPage1.TabIndex = 0;
            this.TabPage1.Text = "Allgemein";
            // 
            // GroupBox2
            // 
            this.GroupBox2.CausesValidation = false;
            this.GroupBox2.Controls.Add(this.SpaltAnsichtCap);
            this.GroupBox2.Controls.Add(this.Views);
            this.GroupBox2.Dock = System.Windows.Forms.DockStyle.Left;
            this.GroupBox2.Location = new Point(328, 0);
            this.GroupBox2.Name = "GroupBox2";
            this.GroupBox2.Size = new Size(272, 81);
            this.GroupBox2.Text = "Ansicht";
            // 
            // SpaltAnsichtCap
            // 
            this.SpaltAnsichtCap.CausesValidation = false;
            this.SpaltAnsichtCap.Location = new Point(8, 2);
            this.SpaltAnsichtCap.Name = "SpaltAnsichtCap";
            this.SpaltAnsichtCap.Size = new Size(98, 22);
            this.SpaltAnsichtCap.Text = "Spalten-Ansicht:";
            this.SpaltAnsichtCap.TextAnzeigeVerhalten = enSteuerelementVerhalten.Scrollen_ohne_Textumbruch;
            // 
            // GroupBox1
            // 
            this.GroupBox1.CausesValidation = false;
            this.GroupBox1.Controls.Add(this.SearchT);
            this.GroupBox1.Controls.Add(this.SearchB);
            this.GroupBox1.Controls.Add(this.FilterAus);
            this.GroupBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.GroupBox1.Location = new Point(0, 0);
            this.GroupBox1.Name = "GroupBox1";
            this.GroupBox1.Size = new Size(328, 81);
            this.GroupBox1.Text = "Filter";
            // 
            // OK
            // 
            this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OK.ImageCode = "Häkchen|16";
            this.OK.Location = new Point(616, 544);
            this.OK.Name = "OK";
            this.OK.Size = new Size(144, 32);
            this.OK.TabIndex = 2;
            this.OK.Text = "Ok";
            this.OK.Click += new EventHandler(this.OK_Click);
            // 
            // TableView
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new Size(772, 582);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.tbl);
            this.Controls.Add(this.TabControl1);
            this.Name = "TableView";
            this.Text = "Tabellen-Bearbeitung";
            this.TabControl1.ResumeLayout(false);
            this.TabPage1.ResumeLayout(false);
            this.GroupBox2.ResumeLayout(false);
            this.GroupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

			}
			private Table tbl;
			private TextBox SearchT;
			private Button FilterAus;
			private Button SearchB;
			private ComboBox Views;
			internal TabControl TabControl1;
			internal TabPage TabPage1;
			internal GroupBox GroupBox2;
			internal GroupBox GroupBox1;
			private Caption SpaltAnsichtCap;
			internal Button OK;
		}
	}