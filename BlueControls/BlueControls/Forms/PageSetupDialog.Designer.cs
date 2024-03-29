﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using Button = BlueControls.Controls.Button;
using ComboBox = BlueControls.Controls.ComboBox;
using GroupBox = BlueControls.Controls.GroupBox;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls.Forms
    {
        public partial class PageSetupDialog 
        {
			//Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
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
            this.BlueFrame2 = new GroupBox();
            this.Rechts = new TextBox();
            this.Links = new TextBox();
            this.Unten = new TextBox();
            this.Oben = new TextBox();
            this.Caption4 = new Caption();
            this.Caption2 = new Caption();
            this.Caption3 = new Caption();
            this.Caption1 = new Caption();
            this.Sample = new PictureBox();
            this.BlueFrame3 = new GroupBox();
            this.Format = new ComboBox();
            this.Breite = new TextBox();
            this.Caption7 = new Caption();
            this.Caption5 = new Caption();
            this.Höhe = new TextBox();
            this.Caption6 = new Caption();
            this.Hochformat = new Button();
            this.Querformat = new Button();
            this.BlueFrame1 = new GroupBox();
            this.BlueFrame2.SuspendLayout();
            ((ISupportInitialize)(this.Sample)).BeginInit();
            this.BlueFrame3.SuspendLayout();
            this.BlueFrame1.SuspendLayout();
            this.SuspendLayout();
            // 
            // BlueFrame2
            // 
            this.BlueFrame2.CausesValidation = false;
            this.BlueFrame2.Controls.Add(this.Rechts);
            this.BlueFrame2.Controls.Add(this.Links);
            this.BlueFrame2.Controls.Add(this.Unten);
            this.BlueFrame2.Controls.Add(this.Oben);
            this.BlueFrame2.Controls.Add(this.Caption4);
            this.BlueFrame2.Controls.Add(this.Caption2);
            this.BlueFrame2.Controls.Add(this.Caption3);
            this.BlueFrame2.Controls.Add(this.Caption1);
            this.BlueFrame2.Location = new Point(144, 272);
            this.BlueFrame2.Name = "BlueFrame2";
            this.BlueFrame2.Size = new Size(224, 96);
            this.BlueFrame2.TabIndex = 5;
            this.BlueFrame2.TabStop = false;
            this.BlueFrame2.Text = "Seitenränder";
            // 
            // Rechts
            // 
            this.Rechts.Cursor = Cursors.IBeam;
            this.Rechts.Location = new Point(160, 56);
            this.Rechts.Name = "Rechts";
            this.Rechts.Size = new Size(56, 24);
            this.Rechts.Suffix = "mm";
            this.Rechts.TabIndex = 9;
            this.Rechts.TextChanged += new EventHandler(this.Something_TextChanged);
            // 
            // Links
            // 
            this.Links.Cursor = Cursors.IBeam;
            this.Links.Location = new Point(48, 56);
            this.Links.Name = "Links";
            this.Links.Size = new Size(56, 24);
            this.Links.Suffix = "mm";
            this.Links.TabIndex = 8;
            this.Links.TextChanged += new EventHandler(this.Something_TextChanged);
            // 
            // Unten
            // 
            this.Unten.Cursor = Cursors.IBeam;
            this.Unten.Location = new Point(160, 24);
            this.Unten.Name = "Unten";
            this.Unten.Size = new Size(56, 24);
            this.Unten.Suffix = "mm";
            this.Unten.TabIndex = 7;
            this.Unten.TextChanged += new EventHandler(this.Something_TextChanged);
            // 
            // Oben
            // 
            this.Oben.Cursor = Cursors.IBeam;
            this.Oben.Location = new Point(48, 24);
            this.Oben.Name = "Oben";
            this.Oben.Size = new Size(56, 24);
            this.Oben.Suffix = "mm";
            this.Oben.TabIndex = 6;
            this.Oben.TextChanged += new EventHandler(this.Something_TextChanged);
            // 
            // Caption4
            // 
            this.Caption4.CausesValidation = false;
            this.Caption4.Location = new Point(112, 56);
            this.Caption4.Name = "Caption4";
            this.Caption4.Size = new Size(48, 18);
            this.Caption4.Text = "Rechts:";
            // 
            // capBearbeitungsMethode
            // 
            this.Caption2.CausesValidation = false;
            this.Caption2.Location = new Point(8, 56);
            this.Caption2.Name = "capBearbeitungsMethode";
            this.Caption2.Size = new Size(40, 18);
            this.Caption2.Text = "Links:";
            // 
            // Caption3
            // 
            this.Caption3.CausesValidation = false;
            this.Caption3.Location = new Point(112, 24);
            this.Caption3.Name = "Caption3";
            this.Caption3.Size = new Size(48, 18);
            this.Caption3.Text = "Unten:";
            // 
            // Caption1
            // 
            this.Caption1.CausesValidation = false;
            this.Caption1.Location = new Point(8, 24);
            this.Caption1.Name = "Caption1";
            this.Caption1.Size = new Size(40, 18);
            this.Caption1.Text = "Oben:";
            // 
            // Sample
            // 
            this.Sample.BackgroundImageLayout = ImageLayout.None;
            this.Sample.Location = new Point(16, 8);
            this.Sample.Name = "Sample";
            this.Sample.Size = new Size(344, 152);
            this.Sample.SizeMode = PictureBoxSizeMode.CenterImage;
            this.Sample.TabIndex = 0;
            this.Sample.TabStop = false;
            // 
            // BlueFrame3
            // 
            this.BlueFrame3.CausesValidation = false;
            this.BlueFrame3.Controls.Add(this.Format);
            this.BlueFrame3.Controls.Add(this.Breite);
            this.BlueFrame3.Controls.Add(this.Caption7);
            this.BlueFrame3.Controls.Add(this.Caption5);
            this.BlueFrame3.Controls.Add(this.Höhe);
            this.BlueFrame3.Controls.Add(this.Caption6);
            this.BlueFrame3.Location = new Point(8, 168);
            this.BlueFrame3.Name = "BlueFrame3";
            this.BlueFrame3.Size = new Size(360, 96);
            this.BlueFrame3.TabIndex = 4;
            this.BlueFrame3.TabStop = false;
            this.BlueFrame3.Text = "Papier";
            // 
            // Format
            // 
            this.Format.Cursor = Cursors.IBeam;
            this.Format.DropDownStyle = ComboBoxStyle.DropDownList;
            this.Format.Location = new Point(64, 24);
            this.Format.Name = "Format";
            this.Format.Size = new Size(288, 24);
            this.Format.TabIndex = 1;
            this.Format.ItemClicked += new EventHandler<AbstractListItemEventArgs>(this.Format_ItemClicked);
            // 
            // Breite
            // 
            this.Breite.Cursor = Cursors.IBeam;
            this.Breite.Location = new Point(184, 56);
            this.Breite.Name = "Breite";
            this.Breite.Size = new Size(56, 24);
            this.Breite.Suffix = "mm";
            this.Breite.TabIndex = 2;
            this.Breite.TextChanged += new EventHandler(this.Abmasse_TextChanged);
            // 
            // Caption7
            // 
            this.Caption7.CausesValidation = false;
            this.Caption7.Location = new Point(8, 24);
            this.Caption7.Name = "Caption7";
            this.Caption7.Size = new Size(56, 18);
            this.Caption7.Text = "Format:";
            // 
            // Caption5
            // 
            this.Caption5.CausesValidation = false;
            this.Caption5.Location = new Point(134, 56);
            this.Caption5.Name = "Caption5";
            this.Caption5.Size = new Size(56, 18);
            this.Caption5.Text = "Breite:";
            // 
            // Höhe
            // 
            this.Höhe.Cursor = Cursors.IBeam;
            this.Höhe.Location = new Point(296, 56);
            this.Höhe.Name = "Höhe";
            this.Höhe.Size = new Size(56, 24);
            this.Höhe.Suffix = "mm";
            this.Höhe.TabIndex = 3;
            this.Höhe.TextChanged += new EventHandler(this.Abmasse_TextChanged);
            // 
            // Caption6
            // 
            this.Caption6.CausesValidation = false;
            this.Caption6.Location = new Point(248, 56);
            this.Caption6.Name = "Caption6";
            this.Caption6.Size = new Size(40, 18);
            this.Caption6.Text = "Höhe:";
            // 
            // Hochformat
            // 
            this.Hochformat.ButtonStyle = ButtonStyle.Optionbox_Big_Borderless;
            this.Hochformat.ImageCode = "Hochformat";
            this.Hochformat.Location = new Point(8, 16);
            this.Hochformat.Name = "Hochformat";
            this.Hochformat.Size = new Size(56, 72);
            this.Hochformat.TabIndex = 4;
            this.Hochformat.Text = "Hoch-format";
            this.Hochformat.CheckedChanged += new EventHandler(this.HochQuer_CheckedChanged);
            // 
            // Querformat
            // 
            this.Querformat.ButtonStyle = ButtonStyle.Optionbox_Big_Borderless;
            this.Querformat.ImageCode = "Querformat";
            this.Querformat.Location = new Point(64, 16);
            this.Querformat.Name = "Querformat";
            this.Querformat.Size = new Size(56, 72);
            this.Querformat.TabIndex = 5;
            this.Querformat.Text = "Quer-format";
            this.Querformat.CheckedChanged += new EventHandler(this.HochQuer_CheckedChanged);
            // 
            // BlueFrame1
            // 
            this.BlueFrame1.CausesValidation = false;
            this.BlueFrame1.Controls.Add(this.Querformat);
            this.BlueFrame1.Controls.Add(this.Hochformat);
            this.BlueFrame1.Location = new Point(8, 272);
            this.BlueFrame1.Name = "BlueFrame1";
            this.BlueFrame1.Size = new Size(128, 96);
            this.BlueFrame1.TabIndex = 6;
            this.BlueFrame1.TabStop = false;
            this.BlueFrame1.Text = "Ausrichtung";
            // 
            // PageSetupDialog
            // 
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(374, 428);
            this.Controls.Add(this.BlueFrame3);
            this.Controls.Add(this.Sample);
            this.Controls.Add(this.BlueFrame2);
            this.Controls.Add(this.BlueFrame1);
            this.Name = "PageSetupDialog";
            this.Text = "Seite einrichten";
            this.Controls.SetChildIndex(this.BlueFrame1, 0);
            this.Controls.SetChildIndex(this.BlueFrame2, 0);
            this.Controls.SetChildIndex(this.Sample, 0);
            this.Controls.SetChildIndex(this.BlueFrame3, 0);
            this.BlueFrame2.ResumeLayout(false);
            ((ISupportInitialize)(this.Sample)).EndInit();
            this.BlueFrame3.ResumeLayout(false);
            this.BlueFrame1.ResumeLayout(false);
            this.ResumeLayout(false);
			}
			internal GroupBox BlueFrame2;
			internal PictureBox Sample;
			internal GroupBox BlueFrame3;
			internal TextBox Oben;
			internal Caption Caption4;
			internal Caption Caption2;
			internal Caption Caption3;
			internal Caption Caption1;
			internal TextBox Rechts;
			internal TextBox Links;
			internal TextBox Unten;
			internal ComboBox Format;
			internal TextBox Breite;
			internal Caption Caption7;
			internal Caption Caption5;
			internal TextBox Höhe;
			internal Caption Caption6;
			internal Button Hochformat;
			internal Button Querformat;
			internal GroupBox BlueFrame1;
		}
	}