using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Orientation = BlueBasics.Enums.Orientation;

namespace BlueControls.Controls
{
    public sealed partial class ListBox 
    {

        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.btnDown = new Button();
            this.btnEdit = new Button();
            this.btnUp = new Button();
            this.btnPlus = new Button();
            this.btnMinus = new Button();
            this.SuspendLayout();
            // 
            // btnDown
            // 
            this.btnDown.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
            this.btnDown.ImageCode = "Pfeil_Unten_Scrollbar|8";
            this.btnDown.Location = new Point(24, 144);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new Size(24, 24);
            this.btnDown.TabIndex = 51;
            this.btnDown.Visible = false;
            this.btnDown.Click += new EventHandler(this.btnDown_Click);
            // 
            // btnEdit
            // 
            this.btnEdit.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
            this.btnEdit.ImageCode = "Stift|12";
            this.btnEdit.Location = new Point(24, 144);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new Size(24, 24);
            this.btnEdit.TabIndex = 51;
            this.btnEdit.Visible = false;
            this.btnEdit.Click += new EventHandler(this.btnEdit_Click);

            // 
            // btnUp
            // 
            this.btnUp.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
            this.btnUp.ImageCode = "Pfeil_Oben_Scrollbar|8";
            this.btnUp.Location = new Point(0, 144);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new Size(24, 24);
            this.btnUp.TabIndex = 50;
            this.btnUp.Visible = false;
            this.btnUp.Click += new EventHandler(this.btnUp_Click);
            // 
            // btnPlus
            // 
            this.btnPlus.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
            this.btnPlus.ImageCode = "PlusZeichen|14";
            this.btnPlus.Location = new Point(152, 144);
            this.btnPlus.Name = "btnPlus";
            this.btnPlus.Size = new Size(64, 24);
            this.btnPlus.Text = "hinzu";
            this.btnPlus.TabIndex = 48;
            this.btnPlus.Visible = false;
            this.btnPlus.QuickInfo = "Neuen Eintrag hinzufügen";
            this.btnPlus.Click += new EventHandler(this.btnPlus_Click);
            // 
            // btnMinus
            // 
            this.btnMinus.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            this.btnMinus.ImageCode = "MinusZeichen|14";
            this.btnMinus.Location = new Point(128, 144);
            this.btnMinus.Name = "btnMinus";
            this.btnMinus.Size = new Size(24, 24);
            this.btnMinus.TabIndex = 47;
            this.btnMinus.TabStop = false;
            this.btnMinus.Visible = false;
            this.btnMinus.Click += new EventHandler(this.btnMinus_Click);
            // 
            // ListBox
            // 
            this.Controls.Add(this.btnMinus);
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.btnDown);
            this.Controls.Add(this.btnUp);
            this.Controls.Add(this.btnPlus);
            this.Size = new Size(177, 168);
            this.AutoCenter = false;
            this.ResumeLayout(false);

        }

        private Button btnEdit;
        private Button btnDown;
        private Button btnUp;
        private Button btnPlus;
        private Button btnMinus;
    }
}
