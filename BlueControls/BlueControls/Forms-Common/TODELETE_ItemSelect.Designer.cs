using System;
using System.Diagnostics;
using System.Drawing;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;

namespace BlueControls.Forms
    {


        public partial class ItemSelect : Form
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
            this.List = new ListBox();
            this.SuspendLayout();
            // 
            // List
            // 
            this.List.AddAllowed = enAddType.None;
            this.List.Appearance = enBlueListBoxAppearance.Gallery;
            this.List.Dock = System.Windows.Forms.DockStyle.Fill;
            this.List.Location = new Point(0, 0);
            this.List.Name = "List";
            this.List.QuickInfo = "";
            this.List.Size = new Size(943, 673);
            this.List.TabIndex = 0;
            this.List.ItemClick += new EventHandler<BasicListItemEventArgs>(this.List_Item_Click);
            // 
            // ItemSelect
            // 
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new Size(943, 673);
            this.Controls.Add(this.List);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "ItemSelect";
            this.Text = "Bild Auswählen:";
            this.TopMost = true;
            this.ResumeLayout(false);

			}

			internal ListBox List;
		}
	}