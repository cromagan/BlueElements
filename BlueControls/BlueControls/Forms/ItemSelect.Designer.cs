using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Enums;
using ListBox = BlueControls.Controls.ListBox;

namespace BlueControls.Forms
    {
        public partial class ItemSelect 
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
            this.List.AddAllowed = AddType.None;
            this.List.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                 | AnchorStyles.Left) 
                                                | AnchorStyles.Right)));
            this.List.Appearance = ListBoxAppearance.Gallery;
            this.List.Location = new Point(8, 8);
            this.List.Name = "List";
            this.List.Size = new Size(677, 427);
            this.List.TabIndex = 0;
            // 
            // ItemSelect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new Size(692, 492);
            this.Controls.Add(this.List);
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.Name = "ItemSelect";
            this.Text = "Wähle:";
            this.Controls.SetChildIndex(this.List, 0);
            this.ResumeLayout(false);

			}
			internal ListBox List;
		}
	}