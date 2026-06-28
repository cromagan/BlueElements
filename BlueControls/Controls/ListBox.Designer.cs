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
            lstBox = new ListBoxCore();
            txtAdd = new TextBox();
            cbxAdd = new ComboBox();
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
            btnPlus.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnPlus.ImageCode = "PlusZeichen|14";
            btnPlus.Location = new Point(411, 56);
            btnPlus.Name = "btnPlus";
            btnPlus.QuickInfo = "Neuen Eintrag hinzufügen";
            btnPlus.Size = new Size(24, 24);
            btnPlus.TabIndex = 48;
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
            // lstBox
            // 
            lstBox.Dock = DockStyle.Fill;
            lstBox.Location = new Point(0, 0);
            lstBox.Name = "lstBox";
            lstBox.Size = new Size(467, 423);
            lstBox.TabIndex = 53;
            lstBox.ButtonUpdate += OnCoreButtonUpdate;
            lstBox.ItemClicked += OnCoreItemClicked;
            lstBox.ItemCheckedChanged += OnCoreItemCheckedChanged;
            // 
            // txtAdd
            // 
            txtAdd.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtAdd.Location = new Point(2, 58);
            txtAdd.Name = "txtAdd";
            txtAdd.Size = new Size(407, 20);
            txtAdd.TabIndex = 54;
            txtAdd.Visible = false;
            txtAdd.TextChanged += AddInput_TextChanged;
            txtAdd.EnterKey += AddInput_EnterKey;
            // 
            // cbxAdd
            // 
            cbxAdd.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            cbxAdd.Location = new Point(2, 58);
            cbxAdd.Name = "cbxAdd";
            cbxAdd.Size = new Size(407, 24);
            cbxAdd.TabIndex = 55;
            cbxAdd.Visible = false;
            cbxAdd.TextChanged += AddInput_TextChanged;
            cbxAdd.ItemAddedByClick += CbxAdd_ItemAddedByClick;
            // 
            // ListBox
            // 
            Controls.Add(btnMinus);
            Controls.Add(btnEdit);
            Controls.Add(btnDown);
            Controls.Add(btnUp);
            Controls.Add(btnPlus);
            Controls.Add(txtAdd);
            Controls.Add(cbxAdd);
            Controls.Add(lstBox);
            Name = "ListBox";
            Size = new Size(467, 423);
            ResumeLayout(false);
            PerformLayout();

        }

        private Button btnEdit;
        private Button btnDown;
        private Button btnUp;
        private Button btnPlus;
        private Button btnMinus;
        private ListBoxCore lstBox;
        private TextBox txtAdd;
        private ComboBox cbxAdd;
    }
}
