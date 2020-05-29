using System;
using System.Diagnostics;
using System.Drawing;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;

namespace BlueControls.Forms
    {

        public partial class RelationDiagram : PadEditor
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
            this.Hinzu = new Button();
            this.Ribbon.SuspendLayout();
            this.tabPageControl.SuspendLayout();
            this.tabPageStart.SuspendLayout();
            this.grpKomponenteHinzufügen.SuspendLayout();
            this.Area_Drucken.SuspendLayout();
            this.Area_Design.SuspendLayout();
            this.SuspendLayout();
            // 
            // Pad
            // 
            this.Pad.AutoRelation = enAutoRelationMode.DirektVerbindungen_Erhalten;
            this.Pad.Size = new Size(1290, 528);
            this.Pad.ContextMenuInit += new EventHandler<ContextMenuInitEventArgs>(this.Pad_ContextMenuInit);
            this.Pad.ContextMenuItemClicked += new EventHandler<ContextMenuItemClickedEventArgs>(this.Pad_ContextMenuItemClicked);
            // 
            // Ribbon
            // 
            this.Ribbon.SelectedIndex = 1;
            this.Ribbon.Size = new Size(1290, 110);
            // 
            // tabPageControl
            // 
            this.tabPageControl.Size = new Size(1282, 81);
            // 
            // Area_KomponenteHinzufügen
            // 
            this.grpKomponenteHinzufügen.Controls.Add(this.Hinzu);
            this.grpKomponenteHinzufügen.Size = new Size(256, 81);
            this.grpKomponenteHinzufügen.Controls.SetChildIndex(this.Hinzu, 0);
            // 
            // Area_Design
            // 
            this.Area_Design.Location = new Point(608, 0);
            // 
            // Area_Assistent
            // 
            this.Area_Assistent.Location = new Point(896, 0);
            // 
            // Button_PageSetup
            // 
            this.Button_PageSetup.Visible = false;
            // 
            // Hinzu
            // 
            this.Hinzu.ImageCode = "PlusZeichen";
            this.Hinzu.Location = new Point(176, 2);
            this.Hinzu.Name = "Hinzu";
            this.Hinzu.Size = new Size(64, 66);
            this.Hinzu.TabIndex = 3;
            this.Hinzu.Text = "Eintrag hinzufügen";
            this.Hinzu.Click += new EventHandler(this.Hinzu_Click);
            // 
            // RelationDiagram
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new Size(1290, 638);
            this.Name = "RelationDiagram";
            this.Text = "Beziehungs-Editor";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Ribbon.ResumeLayout(false);
            this.tabPageControl.ResumeLayout(false);
            this.tabPageStart.ResumeLayout(false);
            this.grpKomponenteHinzufügen.ResumeLayout(false);
            this.Area_Drucken.ResumeLayout(false);
            this.Area_Design.ResumeLayout(false);
            this.ResumeLayout(false);

			}
			private Button Hinzu;
		}
	}