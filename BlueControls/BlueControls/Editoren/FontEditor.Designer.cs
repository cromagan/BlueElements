using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;
using ListBox = BlueControls.Controls.ListBox;

namespace BlueControls.Forms
    {
        public partial class FontEditor 
        {
			//Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
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
            this.lstName = new BlueControls.Controls.ListBox();
            this.capName = new BlueControls.Controls.Caption();
            this.capSize = new BlueControls.Controls.Caption();
            this.listSize = new BlueControls.Controls.ListBox();
            this.frmStyle = new BlueControls.Controls.GroupBox();
            this.chkOnlyUpper = new BlueControls.Controls.Button();
            this.chkOnlyLow = new BlueControls.Controls.Button();
            this.chkOutline = new BlueControls.Controls.Button();
            this.chkDurchgestrichen = new BlueControls.Controls.Button();
            this.chkKap = new BlueControls.Controls.Button();
            this.chkUnterstrichen = new BlueControls.Controls.Button();
            this.chkKursiv = new BlueControls.Controls.Button();
            this.chkFett = new BlueControls.Controls.Button();
            this.btnFontColor = new BlueControls.Controls.Button();
            this.btnOutlineColor = new BlueControls.Controls.Button();
            this.frmPreview = new BlueControls.Controls.GroupBox();
            this.preview = new System.Windows.Forms.PictureBox();
            this.ColorDia = new System.Windows.Forms.ColorDialog();
            this.frmStyle.SuspendLayout();
            this.frmPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.preview)).BeginInit();
            this.SuspendLayout();
            // 
            // lstName
            // 
            this.lstName.AddAllowed = BlueControls.Enums.AddType.None;
            this.lstName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstName.Location = new System.Drawing.Point(8, 24);
            this.lstName.Name = "lstName";
            this.lstName.Size = new System.Drawing.Size(128, 248);
            this.lstName.TabIndex = 0;
            this.lstName.Text = "FontF";
            this.lstName.ItemCheckedChanged += new System.EventHandler(this.FName_Item_CheckedChanged);
            // 
            // capName
            // 
            this.capName.CausesValidation = false;
            this.capName.Location = new System.Drawing.Point(8, 8);
            this.capName.Name = "capName";
            this.capName.Size = new System.Drawing.Size(64, 18);
            this.capName.Text = "Name:";
            // 
            // capSize
            // 
            this.capSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.capSize.CausesValidation = false;
            this.capSize.Location = new System.Drawing.Point(144, 8);
            this.capSize.Name = "capSize";
            this.capSize.Size = new System.Drawing.Size(58, 18);
            this.capSize.Text = "Größe:";
            // 
            // listSize
            // 
            this.listSize.AddAllowed = BlueControls.Enums.AddType.Text;
            this.listSize.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listSize.Location = new System.Drawing.Point(144, 24);
            this.listSize.Name = "listSize";
            this.listSize.Size = new System.Drawing.Size(58, 248);
            this.listSize.TabIndex = 2;
            this.listSize.Text = "SizeF";
            this.listSize.ItemCheckedChanged += new System.EventHandler(this.FName_Item_CheckedChanged);
            // 
            // frmStyle
            // 
            this.frmStyle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.frmStyle.CausesValidation = false;
            this.frmStyle.Controls.Add(this.chkOnlyUpper);
            this.frmStyle.Controls.Add(this.chkOnlyLow);
            this.frmStyle.Controls.Add(this.chkOutline);
            this.frmStyle.Controls.Add(this.chkDurchgestrichen);
            this.frmStyle.Controls.Add(this.chkKap);
            this.frmStyle.Controls.Add(this.chkUnterstrichen);
            this.frmStyle.Controls.Add(this.chkKursiv);
            this.frmStyle.Controls.Add(this.chkFett);
            this.frmStyle.Location = new System.Drawing.Point(212, 24);
            this.frmStyle.Name = "frmStyle";
            this.frmStyle.Size = new System.Drawing.Size(136, 162);
            this.frmStyle.TabIndex = 5;
            this.frmStyle.TabStop = false;
            this.frmStyle.Text = "Style";
            // 
            // chkOnlyUpper
            // 
            this.chkOnlyUpper.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkOnlyUpper.Location = new System.Drawing.Point(8, 136);
            this.chkOnlyUpper.Name = "chkOnlyUpper";
            this.chkOnlyUpper.Size = new System.Drawing.Size(120, 16);
            this.chkOnlyUpper.TabIndex = 7;
            this.chkOnlyUpper.Text = "Großbuchstaben";
            this.chkOnlyUpper.CheckedChanged += new System.EventHandler(this.style_CheckedChanged);
            // 
            // chkOnlyLow
            // 
            this.chkOnlyLow.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkOnlyLow.Location = new System.Drawing.Point(8, 120);
            this.chkOnlyLow.Name = "chkOnlyLow";
            this.chkOnlyLow.Size = new System.Drawing.Size(120, 16);
            this.chkOnlyLow.TabIndex = 6;
            this.chkOnlyLow.Text = "Kleinbuchstaben";
            this.chkOnlyLow.CheckedChanged += new System.EventHandler(this.style_CheckedChanged);
            // 
            // chkOutline
            // 
            this.chkOutline.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkOutline.Location = new System.Drawing.Point(8, 104);
            this.chkOutline.Name = "chkOutline";
            this.chkOutline.Size = new System.Drawing.Size(120, 16);
            this.chkOutline.TabIndex = 5;
            this.chkOutline.Text = "Umrandet";
            this.chkOutline.CheckedChanged += new System.EventHandler(this.style_CheckedChanged);
            // 
            // chkDurchgestrichen
            // 
            this.chkDurchgestrichen.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkDurchgestrichen.Location = new System.Drawing.Point(8, 88);
            this.chkDurchgestrichen.Name = "chkDurchgestrichen";
            this.chkDurchgestrichen.Size = new System.Drawing.Size(120, 16);
            this.chkDurchgestrichen.TabIndex = 4;
            this.chkDurchgestrichen.Text = "Durchgestrichen";
            this.chkDurchgestrichen.CheckedChanged += new System.EventHandler(this.style_CheckedChanged);
            // 
            // chkKap
            // 
            this.chkKap.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkKap.Location = new System.Drawing.Point(8, 72);
            this.chkKap.Name = "chkKap";
            this.chkKap.Size = new System.Drawing.Size(120, 16);
            this.chkKap.TabIndex = 3;
            this.chkKap.Text = "Kapitälchen";
            this.chkKap.CheckedChanged += new System.EventHandler(this.style_CheckedChanged);
            // 
            // chkUnterstrichen
            // 
            this.chkUnterstrichen.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkUnterstrichen.Location = new System.Drawing.Point(8, 56);
            this.chkUnterstrichen.Name = "chkUnterstrichen";
            this.chkUnterstrichen.Size = new System.Drawing.Size(120, 16);
            this.chkUnterstrichen.TabIndex = 2;
            this.chkUnterstrichen.Text = "Unterstrichen";
            this.chkUnterstrichen.CheckedChanged += new System.EventHandler(this.style_CheckedChanged);
            // 
            // chkKursiv
            // 
            this.chkKursiv.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkKursiv.Location = new System.Drawing.Point(8, 40);
            this.chkKursiv.Name = "chkKursiv";
            this.chkKursiv.Size = new System.Drawing.Size(120, 16);
            this.chkKursiv.TabIndex = 1;
            this.chkKursiv.Text = "Kursiv";
            this.chkKursiv.CheckedChanged += new System.EventHandler(this.style_CheckedChanged);
            // 
            // chkFett
            // 
            this.chkFett.ButtonStyle = BlueControls.Enums.ButtonStyle.Checkbox_Text;
            this.chkFett.Location = new System.Drawing.Point(8, 24);
            this.chkFett.Name = "chkFett";
            this.chkFett.Size = new System.Drawing.Size(120, 16);
            this.chkFett.TabIndex = 0;
            this.chkFett.Text = "Fett";
            this.chkFett.CheckedChanged += new System.EventHandler(this.style_CheckedChanged);
            // 
            // btnFontColor
            // 
            this.btnFontColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFontColor.Location = new System.Drawing.Point(212, 200);
            this.btnFontColor.Name = "btnFontColor";
            this.btnFontColor.Size = new System.Drawing.Size(136, 32);
            this.btnFontColor.TabIndex = 3;
            this.btnFontColor.Text = "Schriftfarbe";
            this.btnFontColor.Click += new System.EventHandler(this.cFarbe_Click);
            // 
            // btnOutlineColor
            // 
            this.btnOutlineColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOutlineColor.Location = new System.Drawing.Point(212, 240);
            this.btnOutlineColor.Name = "btnOutlineColor";
            this.btnOutlineColor.Size = new System.Drawing.Size(136, 32);
            this.btnOutlineColor.TabIndex = 4;
            this.btnOutlineColor.Text = "Umrandungs-Farbe";
            this.btnOutlineColor.Click += new System.EventHandler(this.cRandF_Click);
            // 
            // frmPreview
            // 
            this.frmPreview.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.frmPreview.CausesValidation = false;
            this.frmPreview.Controls.Add(this.preview);
            this.frmPreview.Location = new System.Drawing.Point(8, 274);
            this.frmPreview.Name = "frmPreview";
            this.frmPreview.Size = new System.Drawing.Size(340, 112);
            this.frmPreview.TabIndex = 0;
            this.frmPreview.TabStop = false;
            this.frmPreview.Text = "Vorschau";
            // 
            // preview
            // 
            this.preview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.preview.Location = new System.Drawing.Point(8, 16);
            this.preview.Name = "preview";
            this.preview.Size = new System.Drawing.Size(324, 88);
            this.preview.TabIndex = 0;
            this.preview.TabStop = false;
            // 
            // FontEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lstName);
            this.Controls.Add(this.listSize);
            this.Controls.Add(this.frmPreview);
            this.Controls.Add(this.btnOutlineColor);
            this.Controls.Add(this.btnFontColor);
            this.Controls.Add(this.frmStyle);
            this.Controls.Add(this.capSize);
            this.Controls.Add(this.capName);
            this.Name = "FontEditor";
            this.Size = new System.Drawing.Size(355, 398);
            this.frmStyle.ResumeLayout(false);
            this.frmPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.preview)).EndInit();
            this.ResumeLayout(false);

			}
			internal ListBox lstName;
			internal Caption capName;
			internal Caption capSize;
			internal ListBox listSize;
			internal GroupBox frmStyle;
			internal Button chkOutline;
			internal Button chkDurchgestrichen;
			internal Button chkKap;
			internal Button chkUnterstrichen;
			internal Button chkKursiv;
			internal Button chkFett;
			internal Button btnFontColor;
			internal Button btnOutlineColor;
			internal GroupBox frmPreview;
			internal PictureBox preview;
			internal ColorDialog ColorDia;
			internal Button chkOnlyUpper;
			internal Button chkOnlyLow;
		}
	}