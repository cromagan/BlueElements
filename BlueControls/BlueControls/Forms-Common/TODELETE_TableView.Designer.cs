using System;
using System.Diagnostics;
using System.Drawing;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;

namespace BlueControls.Forms
    {


        public sealed partial class frmTableView : Form
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
            this.TableView = new BlueControls.Controls.Table();
            this.cbxColumnArr = new BlueControls.Controls.ComboBox();
            this.SearchT = new BlueControls.Controls.TextBox();
            this.FilterAus = new BlueControls.Controls.Button();
            this.SearchB = new BlueControls.Controls.Button();
            this.TabControl1 = new BlueControls.Controls.TabControl();
            this.TabPage1 = new BlueControls.Controls.TabPage();
            this.GroupBox2 = new BlueControls.Controls.GroupBox();
            this.SpaltAnsichtCap = new BlueControls.Controls.Caption();
            this.GroupBox1 = new BlueControls.Controls.GroupBox();
            this.OK = new BlueControls.Controls.Button();
            this.TabControl1.SuspendLayout();
            this.TabPage1.SuspendLayout();
            this.GroupBox2.SuspendLayout();
            this.GroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // TableView
            // 
            this.TableView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TableView.Location = new System.Drawing.Point(0, 110);
            this.TableView.Name = "tbl";
            this.TableView.Size = new System.Drawing.Size(772, 426);
            this.TableView.TabIndex = 0;
            this.TableView.ColumnArrangementChanged += new System.EventHandler(this.TableView_ColumnArrangementChanged);
            this.TableView.ViewChanged += new System.EventHandler(this.TableView_ViewChanged);
            this.TableView.RowsSorted += new System.EventHandler(this.TableView_RowsSorted);
            // 
            // cbxColumnArr
            // 
            this.cbxColumnArr.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxColumnArr.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxColumnArr.Location = new System.Drawing.Point(112, 2);
            this.cbxColumnArr.Name = "cbxColumnArr";
            this.cbxColumnArr.Size = new System.Drawing.Size(152, 22);
            this.cbxColumnArr.TabIndex = 16;
            this.cbxColumnArr.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.cbxColumnArr_ItemClicked);
            // 
            // SearchT
            // 
            this.SearchT.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.SearchT.Location = new System.Drawing.Point(0, 2);
            this.SearchT.Name = "SearchT";
            this.SearchT.Size = new System.Drawing.Size(176, 22);
            this.SearchT.TabIndex = 12;
            this.SearchT.TextChanged += new System.EventHandler(this.SearchT_TextChange);
            this.SearchT.Enter += new System.EventHandler(this.SearchT_Enter);
            // 
            // FilterAus
            // 
            this.FilterAus.ImageCode = "Trichter|||1";
            this.FilterAus.Location = new System.Drawing.Point(240, 2);
            this.FilterAus.Name = "FilterAus";
            this.FilterAus.Size = new System.Drawing.Size(56, 66);
            this.FilterAus.TabIndex = 14;
            this.FilterAus.Text = "alle Filter aus";
            this.FilterAus.Click += new System.EventHandler(this.FilterAus_Click);
            // 
            // SearchB
            // 
            this.SearchB.ButtonStyle = BlueControls.Enums.enButtonStyle.Pic1_or_Pic2;
            this.SearchB.Enabled = false;
            this.SearchB.ImageCode = "Trichter";
            this.SearchB.ImageCode_Checked = "Trichter||||FF0000";
            this.SearchB.Location = new System.Drawing.Point(184, 2);
            this.SearchB.Name = "SearchB";
            this.SearchB.QuickInfo = "Zeigt nur noch Zeilen, in dem der<br>eingegeben Text vorkommt.";
            this.SearchB.Size = new System.Drawing.Size(56, 66);
            this.SearchB.TabIndex = 13;
            this.SearchB.Text = "Text filtern";
            this.SearchB.Click += new System.EventHandler(this.SearchB_Click);
            // 
            // TabControl1
            // 
            this.TabControl1.Controls.Add(this.TabPage1);
            this.TabControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.TabControl1.HotTrack = true;
            this.TabControl1.IsRibbonBar = true;
            this.TabControl1.Location = new System.Drawing.Point(0, 0);
            this.TabControl1.Name = "TabControl1";
            this.TabControl1.Size = new System.Drawing.Size(772, 110);
            this.TabControl1.TabIndex = 1;
            // 
            // TabPage1
            // 
            this.TabPage1.Controls.Add(this.GroupBox2);
            this.TabPage1.Controls.Add(this.GroupBox1);
            this.TabPage1.Location = new System.Drawing.Point(4, 25);
            this.TabPage1.Name = "TabPage1";
            this.TabPage1.Size = new System.Drawing.Size(764, 81);
            this.TabPage1.TabIndex = 0;
            this.TabPage1.Text = "Allgemein";
            // 
            // GroupBox2
            // 
            this.GroupBox2.CausesValidation = false;
            this.GroupBox2.Controls.Add(this.SpaltAnsichtCap);
            this.GroupBox2.Controls.Add(this.cbxColumnArr);
            this.GroupBox2.Dock = System.Windows.Forms.DockStyle.Left;
            this.GroupBox2.Location = new System.Drawing.Point(328, 0);
            this.GroupBox2.Name = "GroupBox2";
            this.GroupBox2.Size = new System.Drawing.Size(272, 81);
            this.GroupBox2.Text = "Ansicht";
            // 
            // SpaltAnsichtCap
            // 
            this.SpaltAnsichtCap.CausesValidation = false;
            this.SpaltAnsichtCap.Location = new System.Drawing.Point(8, 2);
            this.SpaltAnsichtCap.Name = "SpaltAnsichtCap";
            this.SpaltAnsichtCap.Size = new System.Drawing.Size(98, 22);
            this.SpaltAnsichtCap.Text = "Spalten-Ansicht:";
            this.SpaltAnsichtCap.TextAnzeigeVerhalten = BlueControls.Enums.enSteuerelementVerhalten.Scrollen_ohne_Textumbruch;
            // 
            // GroupBox1
            // 
            this.GroupBox1.CausesValidation = false;
            this.GroupBox1.Controls.Add(this.SearchT);
            this.GroupBox1.Controls.Add(this.SearchB);
            this.GroupBox1.Controls.Add(this.FilterAus);
            this.GroupBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.GroupBox1.Location = new System.Drawing.Point(0, 0);
            this.GroupBox1.Name = "GroupBox1";
            this.GroupBox1.Size = new System.Drawing.Size(328, 81);
            this.GroupBox1.Text = "Filter";
            // 
            // OK
            // 
            this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OK.ImageCode = "Häkchen|16";
            this.OK.Location = new System.Drawing.Point(616, 544);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(144, 32);
            this.OK.TabIndex = 2;
            this.OK.Text = "Ok";
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // TableView
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(772, 582);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.TableView);
            this.Controls.Add(this.TabControl1);
            this.Name = "TableView";
            this.Text = "Tabellen-Bearbeitung";
            this.TabControl1.ResumeLayout(false);
            this.TabPage1.ResumeLayout(false);
            this.GroupBox2.ResumeLayout(false);
            this.GroupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

			}
			private Table TableView;
			private TextBox SearchT;
			private Button FilterAus;
			private Button SearchB;
			private ComboBox cbxColumnArr;
			internal TabControl TabControl1;
			internal TabPage TabPage1;
			internal GroupBox GroupBox2;
			internal GroupBox GroupBox1;
			private Caption SpaltAnsichtCap;
			internal Button OK;
		}
	}