using System;
using System.Diagnostics;
using System.Drawing;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;

namespace BlueControls.Forms
    {

        public partial class RelationDiagram 
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
            this.Hinzu = new BlueControls.Controls.Button();
            this.Ribbon.SuspendLayout();
            this.tabPageControl.SuspendLayout();
            this.tabPageStart.SuspendLayout();
            this.grpKomponenteHinzufügen.SuspendLayout();
            this.Area_Drucken.SuspendLayout();
            this.Area_Design.SuspendLayout();
            this.Area_Assistent.SuspendLayout();
            this.SuspendLayout();
            // 
            // Pad
            // 
            this.Pad.AutoRelation = ((BlueControls.Enums.enAutoRelationMode)((BlueControls.Enums.enAutoRelationMode.DirektVerbindungen | BlueControls.Enums.enAutoRelationMode.NurBeziehungenErhalten)));
            this.Pad.Size = new System.Drawing.Size(1290, 528);
            this.Pad.ContextMenuInit += new System.EventHandler<BlueControls.EventArgs.ContextMenuInitEventArgs>(this.Pad_ContextMenuInit);
            this.Pad.ContextMenuItemClicked += new System.EventHandler<BlueControls.EventArgs.ContextMenuItemClickedEventArgs>(this.Pad_ContextMenuItemClicked);
            // 
            // Ribbon
            // 
            this.Ribbon.Size = new System.Drawing.Size(1290, 110);
            // 
            // tabPageControl
            // 
            this.tabPageControl.Size = new System.Drawing.Size(1282, 81);
            // 
            // grpKomponenteHinzufügen
            // 
            this.grpKomponenteHinzufügen.Controls.Add(this.Hinzu);
            this.grpKomponenteHinzufügen.Size = new System.Drawing.Size(336, 81);
            this.grpKomponenteHinzufügen.Controls.SetChildIndex(this.Hinzu, 0);
            // 
            // Button_PageSetup
            // 
            this.Button_PageSetup.ButtonStyle = BlueControls.Enums.enButtonStyle.Button;
            this.Button_PageSetup.Visible = false;
            // 
            // ArbeitsbreichSetup
            // 
            this.ArbeitsbreichSetup.ButtonStyle = BlueControls.Enums.enButtonStyle.Button;
            // 
            // Hinzu
            // 
            this.Hinzu.ImageCode = "PlusZeichen";
            this.Hinzu.Location = new System.Drawing.Point(264, 2);
            this.Hinzu.Name = "Hinzu";
            this.Hinzu.Size = new System.Drawing.Size(64, 66);
            this.Hinzu.TabIndex = 3;
            this.Hinzu.Text = "Eintrag hinzufügen";
            this.Hinzu.Click += new System.EventHandler(this.Hinzu_Click);
            // 
            // RelationDiagram
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1290, 638);
            this.Name = "RelationDiagram";
            this.Text = "Beziehungs-Editor";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Ribbon.ResumeLayout(false);
            this.tabPageControl.ResumeLayout(false);
            this.tabPageStart.ResumeLayout(false);
            this.grpKomponenteHinzufügen.ResumeLayout(false);
            this.Area_Drucken.ResumeLayout(false);
            this.Area_Design.ResumeLayout(false);
            this.Area_Assistent.ResumeLayout(false);
            this.ResumeLayout(false);

			}
			private Button Hinzu;
		}
	}