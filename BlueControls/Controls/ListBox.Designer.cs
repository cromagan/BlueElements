// Licensed under AGPL-3.0; see License.md for disclaimer and details.

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
        private void InitializeComponent() {
            btnDown = new Button();
            btnEdit = new Button();
            btnUp = new Button();
            btnPlus = new Button();
            btnMinus = new Button();
            grpBox = new GroupBox();
            lstBox = new ListBoxCore();
            btnPlus2 = new Button();
            grpBox.SuspendLayout();
            SuspendLayout();
            // 
            // btnDown
            // 
            btnDown.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDown.ImageCode = "Pfeil_Unten_Scrollbar|8";
            btnDown.Location = new Point(48, 56);
            btnDown.Name = "btnDown";
            btnDown.Size = new Size(24, 24);
            btnDown.TabIndex = 51;
            btnDown.Visible = false;
            btnDown.Click += btnDown_Click;
            // 
            // btnEdit
            // 
            btnEdit.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnEdit.ImageCode = "Stift|12";
            btnEdit.Location = new Point(88, 56);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(24, 24);
            btnEdit.TabIndex = 51;
            btnEdit.Visible = false;
            btnEdit.Click += btnEdit_Click;
            // 
            // btnUp
            // 
            btnUp.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnUp.ImageCode = "Pfeil_Oben_Scrollbar|8";
            btnUp.Location = new Point(16, 56);
            btnUp.Name = "btnUp";
            btnUp.Size = new Size(24, 24);
            btnUp.TabIndex = 50;
            btnUp.Visible = false;
            btnUp.Click += btnUp_Click;
            // 
            // btnPlus
            // 
            btnPlus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnPlus.ImageCode = "PlusZeichen|14";
            btnPlus.Location = new Point(160, 56);
            btnPlus.Name = "btnPlus";
            btnPlus.QuickInfo = "Neuen Eintrag hinzufügen";
            btnPlus.Size = new Size(64, 24);
            btnPlus.TabIndex = 48;
            btnPlus.Text = "hinzu";
            btnPlus.Visible = false;
            btnPlus.Click += btnPlus_Click;
            // 
            // btnMinus
            // 
            btnMinus.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnMinus.ImageCode = "MinusZeichen|14";
            btnMinus.Location = new Point(128, 56);
            btnMinus.Name = "btnMinus";
            btnMinus.Size = new Size(24, 24);
            btnMinus.TabIndex = 47;
            btnMinus.TabStop = false;
            btnMinus.Visible = false;
            btnMinus.Click += btnMinus_Click;
            // 
            // grp
            // 
            grpBox.Controls.Add(btnPlus2);
            grpBox.Dock = DockStyle.Top;
            grpBox.GroupBoxStyle = GroupBoxStyle.RoundRect;
            grpBox.Location = new Point(0, 0);
            grpBox.Name = "grpBox";
            grpBox.Size = new Size(467, 40);
            grpBox.TabIndex = 52;
            grpBox.TabStop = false;
            grpBox.Visible = false;
            // 
            // lstBox
            // 
            lstBox.Dock = DockStyle.Fill;
            lstBox.Location = new Point(0, 40);
            lstBox.Name = "lstBox";
            lstBox.Size = new Size(467, 383);
            lstBox.TabIndex = 53;
            lstBox.ButtonUpdate += OnCoreButtonUpdate;
            // 
            // btnPlus2
            // 
            btnPlus2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnPlus2.ImageCode = "PlusZeichen|14";
            btnPlus2.Location = new Point(8, 8);
            btnPlus2.Name = "btnPlus2";
            btnPlus2.QuickInfo = "Neuen Eintrag hinzufügen";
            btnPlus2.Size = new Size(87, 24);
            btnPlus2.TabIndex = 49;
            btnPlus2.Text = "hinzu";
            btnPlus2.Visible = false;
            btnPlus2.Click += btnPlus_Click;
            // 
            // ListBox
            // 
            Controls.Add(btnMinus);
            Controls.Add(btnEdit);
            Controls.Add(btnDown);
            Controls.Add(btnUp);
            Controls.Add(btnPlus);
            Controls.Add(lstBox);
            Controls.Add(grpBox);
            Name = "ListBox";
            Size = new Size(467, 423);
            grpBox.ResumeLayout(false);
            ResumeLayout(false);

        }

        private Button btnEdit;
        private Button btnDown;
        private Button btnUp;
        private Button btnPlus;
        private Button btnMinus;
        private GroupBox grpBox;
        private ListBoxCore lstBox;
        private Button btnPlus2;
    }
}
