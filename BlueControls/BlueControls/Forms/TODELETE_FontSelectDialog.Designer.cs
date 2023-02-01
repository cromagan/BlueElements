using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using BlueControls.Controls;
using BlueControls.Enums;

namespace BlueControls.Forms
    {
        public partial class FontSelectDialog : Form
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
            this.FName = new ListBox();
            this.Caption1 = new Caption();
            this.Größe = new Caption();
            this.FSize = new ListBox();
            this.BlueFrame1 = new GroupBox();
            this.OnlyUpper = new Button();
            this.OnlyLow = new Button();
            this.fOutline = new Button();
            this.fDurchge = new Button();
            this.fKap = new Button();
            this.fUnterstrichen = new Button();
            this.fKursiv = new Button();
            this.fFett = new Button();
            this.cFarbe = new Button();
            this.cRandF = new Button();
            this.Ok = new Button();
            this.BlueFrame2 = new GroupBox();
            this.Sample = new System.Windows.Forms.PictureBox();
            this.ColorDia = new System.Windows.Forms.ColorDialog();
            this.BlueFrame1.SuspendLayout();
            this.BlueFrame2.SuspendLayout();
            ((ISupportInitialize)(this.Sample)).BeginInit();
            this.SuspendLayout();
            // 
            // FName
            // 
            this.FName.AddAllowed = AddType.None;
            this.FName.CheckBehavior = CheckBehavior.AlwaysSingleSelection;
            this.FName.Location = new Point(8, 32);
            this.FName.Name = "FName";
            this.FName.Size = new Size(216, 296);
            this.FName.TabIndex = 0;
            this.FName.Text = "FontF";
            this.FName.ItemCheckedChanged += new  EventHandler(this.FName_Item_CheckedChanged);
            // 
            // Caption1
            // 
            this.Caption1.CausesValidation = false;
            this.Caption1.Location = new Point(8, 8);
            this.Caption1.Name = "Caption1";
            this.Caption1.Size = new Size(64, 18);
            this.Caption1.Text = "Name:";
            // 
            // Größe
            // 
            this.Größe.CausesValidation = false;
            this.Größe.Location = new Point(232, 8);
            this.Größe.Name = "Größe";
            this.Größe.Size = new Size(64, 18);
            this.Größe.Text = "Größe:";
            // 
            // FSize
            // 
            this.FSize.AddAllowed = AddType.Text;
            this.FSize.CheckBehavior = CheckBehavior.AlwaysSingleSelection;
            this.FSize.Location = new Point(232, 32);
            this.FSize.Name = "FSize";
            this.FSize.Size = new Size(88, 296);
            this.FSize.TabIndex = 2;
            this.FSize.Text = "SizeF";
            this.FSize.ItemCheckedChanged += new  EventHandler(this.FName_Item_CheckedChanged);
            // 
            // BlueFrame1
            // 
            this.BlueFrame1.CausesValidation = false;
            this.BlueFrame1.Controls.Add(this.OnlyUpper);
            this.BlueFrame1.Controls.Add(this.OnlyLow);
            this.BlueFrame1.Controls.Add(this.fOutline);
            this.BlueFrame1.Controls.Add(this.fDurchge);
            this.BlueFrame1.Controls.Add(this.fKap);
            this.BlueFrame1.Controls.Add(this.fUnterstrichen);
            this.BlueFrame1.Controls.Add(this.fKursiv);
            this.BlueFrame1.Controls.Add(this.fFett);
            this.BlueFrame1.Location = new Point(328, 24);
            this.BlueFrame1.Name = "BlueFrame1";
            this.BlueFrame1.Size = new Size(160, 224);
            this.BlueFrame1.Text = "Style";
            // 
            // OnlyUpper
            // 
            this.OnlyUpper.ButtonStyle = ButtonStyle.Checkbox_Text;
            this.OnlyUpper.Location = new Point(16, 192);
            this.OnlyUpper.Name = "OnlyUpper";
            this.OnlyUpper.Size = new Size(120, 16);
            this.OnlyUpper.TabIndex = 7;
            this.OnlyUpper.Text = "Großbuchstaben";
            // 
            // OnlyLow
            // 
            this.OnlyLow.ButtonStyle = ButtonStyle.Checkbox_Text;
            this.OnlyLow.Location = new Point(16, 168);
            this.OnlyLow.Name = "OnlyLow";
            this.OnlyLow.Size = new Size(120, 16);
            this.OnlyLow.TabIndex = 6;
            this.OnlyLow.Text = "Kleinbuchstaben";
            // 
            // fOutline
            // 
            this.fOutline.ButtonStyle = ButtonStyle.Checkbox_Text;
            this.fOutline.Location = new Point(16, 144);
            this.fOutline.Name = "fOutline";
            this.fOutline.Size = new Size(120, 16);
            this.fOutline.TabIndex = 5;
            this.fOutline.Text = "Umrandet";
            this.fOutline.CheckedChanged += new  EventHandler(this.fFett_CheckedChanged);
            // 
            // fDurchge
            // 
            this.fDurchge.ButtonStyle = ButtonStyle.Checkbox_Text;
            this.fDurchge.Location = new Point(16, 120);
            this.fDurchge.Name = "fDurchge";
            this.fDurchge.Size = new Size(120, 16);
            this.fDurchge.TabIndex = 4;
            this.fDurchge.Text = "Durchgestrichen";
            this.fDurchge.CheckedChanged += new  EventHandler(this.fFett_CheckedChanged);
            // 
            // fKap
            // 
            this.fKap.ButtonStyle = ButtonStyle.Checkbox_Text;
            this.fKap.Location = new Point(16, 96);
            this.fKap.Name = "fKap";
            this.fKap.Size = new Size(120, 16);
            this.fKap.TabIndex = 3;
            this.fKap.Text = "Kapitälchen";
            this.fKap.CheckedChanged += new  EventHandler(this.fFett_CheckedChanged);
            // 
            // fUnterstrichen
            // 
            this.fUnterstrichen.ButtonStyle = ButtonStyle.Checkbox_Text;
            this.fUnterstrichen.Location = new Point(16, 72);
            this.fUnterstrichen.Name = "fUnterstrichen";
            this.fUnterstrichen.Size = new Size(120, 16);
            this.fUnterstrichen.TabIndex = 2;
            this.fUnterstrichen.Text = "Unterstrichen";
            this.fUnterstrichen.CheckedChanged += new  EventHandler(this.fFett_CheckedChanged);
            // 
            // fKursiv
            // 
            this.fKursiv.ButtonStyle = ButtonStyle.Checkbox_Text;
            this.fKursiv.Location = new Point(16, 48);
            this.fKursiv.Name = "fKursiv";
            this.fKursiv.Size = new Size(120, 16);
            this.fKursiv.TabIndex = 1;
            this.fKursiv.Text = "Kursiv";
            this.fKursiv.CheckedChanged += new  EventHandler(this.fFett_CheckedChanged);
            // 
            // fFett
            // 
            this.fFett.ButtonStyle = ButtonStyle.Checkbox_Text;
            this.fFett.Location = new Point(16, 24);
            this.fFett.Name = "fFett";
            this.fFett.Size = new Size(120, 16);
            this.fFett.TabIndex = 0;
            this.fFett.Text = "Fett";
            this.fFett.CheckedChanged += new  EventHandler(this.fFett_CheckedChanged);
            // 
            // cFarbe
            // 
            this.cFarbe.Location = new Point(328, 256);
            this.cFarbe.Name = "cFarbe";
            this.cFarbe.Size = new Size(160, 32);
            this.cFarbe.TabIndex = 3;
            this.cFarbe.Text = "Schriftfarbe";
            this.cFarbe.Click += new EventHandler(this.cFarbe_Click);
            // 
            // cRandF
            // 
            this.cRandF.Location = new Point(328, 296);
            this.cRandF.Name = "cRandF";
            this.cRandF.Size = new Size(160, 32);
            this.cRandF.TabIndex = 4;
            this.cRandF.Text = "Umrandungs-Farbe";
            this.cRandF.Click += new EventHandler(this.cRandF_Click);
            // 
            // Ok
            // 
            this.Ok.ImageCode = "Häkchen|16";
            this.Ok.Location = new Point(424, 456);
            this.Ok.Name = "Ok";
            this.Ok.Size = new Size(72, 32);
            this.Ok.TabIndex = 5;
            this.Ok.Text = "OK";
            this.Ok.Click += new EventHandler(this.Ok_Click);
            // 
            // BlueFrame2
            // 
            this.BlueFrame2.CausesValidation = false;
            this.BlueFrame2.Controls.Add(this.Sample);
            this.BlueFrame2.Location = new Point(16, 336);
            this.BlueFrame2.Name = "BlueFrame2";
            this.BlueFrame2.Size = new Size(472, 112);
            this.BlueFrame2.Text = "Beispiel";
            // 
            // Sample
            // 
            this.Sample.Location = new Point(8, 16);
            this.Sample.Name = "Sample";
            this.Sample.Size = new Size(456, 88);
            this.Sample.TabIndex = 0;
            this.Sample.TabStop = false;
            // 
            // FontSelectDialog
            // 
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new Size(501, 499);
            this.Controls.Add(this.BlueFrame2);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.cRandF);
            this.Controls.Add(this.cFarbe);
            this.Controls.Add(this.BlueFrame1);
            this.Controls.Add(this.FSize);
            this.Controls.Add(this.Größe);
            this.Controls.Add(this.Caption1);
            this.Controls.Add(this.FName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FontSelectDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Schriftart";
            this.BlueFrame1.ResumeLayout(false);
            this.BlueFrame2.ResumeLayout(false);
            ((ISupportInitialize)(this.Sample)).EndInit();
            this.ResumeLayout(false);
			}
			internal ListBox FName;
			internal Caption Caption1;
			internal Caption Größe;
			internal ListBox FSize;
			internal GroupBox BlueFrame1;
			internal Button fOutline;
			internal Button fDurchge;
			internal Button fKap;
			internal Button fUnterstrichen;
			internal Button fKursiv;
			internal Button fFett;
			internal Button cFarbe;
			internal Button cRandF;
			internal Button Ok;
			internal GroupBox BlueFrame2;
			internal System.Windows.Forms.PictureBox Sample;
			internal System.Windows.Forms.ColorDialog ColorDia;
			internal Button OnlyUpper;
			internal Button OnlyLow;
		}
	}