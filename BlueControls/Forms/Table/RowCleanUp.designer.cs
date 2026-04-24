using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;
using ListBox = BlueControls.Controls.ListBox;

namespace BlueControls.BlueTableDialogs
{
    public sealed partial class RowCleanUp {
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            btnExecute = new Button();
            txtInfo = new Caption();
            btnCancel = new Button();
            lstColumns = new ListBox();
            capSpalten = new Caption();
            grpVerhalten = new GroupBox();
            optLöschen = new Button();
            optFülle = new Button();
            pnlStatusBar.SuspendLayout();
            grpVerhalten.SuspendLayout();
            SuspendLayout();
            // 
            // capStatusBar
            // 
            capStatusBar.Size = new Size(731, 24);
            // 
            // pnlStatusBar
            // 
            pnlStatusBar.Location = new Point(0, 289);
            pnlStatusBar.Size = new Size(731, 24);
            // 
            // btnExecute
            // 
            btnExecute.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnExecute.CustomContextMenuItems = null;
            btnExecute.Enabled = false;
            btnExecute.ImageCode = "Abspielen|16";
            btnExecute.Location = new Point(616, 252);
            btnExecute.Name = "btnExecute";
            btnExecute.Size = new Size(104, 32);
            btnExecute.TabIndex = 9;
            btnExecute.Text = "Ausführen";
            btnExecute.Click += Fertig_Click;
            // 
            // txtInfo
            // 
            txtInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtInfo.CausesValidation = false;
            txtInfo.CustomContextMenuItems = null;
            txtInfo.Location = new Point(288, 8);
            txtInfo.Name = "txtInfo";
            txtInfo.Size = new Size(432, 40);
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.CustomContextMenuItems = null;
            btnCancel.ImageCode = "Kreuz|16";
            btnCancel.Location = new Point(512, 252);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(104, 32);
            btnCancel.TabIndex = 11;
            btnCancel.Text = "Abbrechen";
            btnCancel.Click += Cancel_Click;
            // 
            // lstColumns
            // 
            lstColumns.AddAllowed = AddType.None;
            lstColumns.Appearance = ListBoxAppearance.Listbox_Boxes;
            lstColumns.CheckBehavior = CheckBehavior.MultiSelection;
            lstColumns.Location = new Point(8, 64);
            lstColumns.Name = "lstColumns";
            lstColumns.Size = new Size(272, 224);
            lstColumns.TabIndex = 101;
            lstColumns.ItemClicked += lstColumns_ItemClicked;
            // 
            // capSpalten
            // 
            capSpalten.CausesValidation = false;
            capSpalten.CustomContextMenuItems = null;
            capSpalten.Location = new Point(8, 8);
            capSpalten.Name = "capSpalten";
            capSpalten.Size = new Size(272, 56);
            capSpalten.Text = "<b><u>Spalten</b></u>,<br>die verwendet werden, um Zeilen, die mehrfach vorkommen, zu identifizieren:";
            // 
            // grpVerhalten
            // 
            grpVerhalten.BackColor = Color.FromArgb(240, 240, 240);
            grpVerhalten.Controls.Add(optLöschen);
            grpVerhalten.Controls.Add(optFülle);
            grpVerhalten.Location = new Point(288, 56);
            grpVerhalten.Name = "grpVerhalten";
            grpVerhalten.Size = new Size(432, 128);
            grpVerhalten.TabIndex = 102;
            grpVerhalten.TabStop = false;
            grpVerhalten.Text = "Verhalten";
            // 
            // optLöschen
            // 
            optLöschen.ButtonStyle = ButtonStyle.Optionbox_Text;
            optLöschen.CustomContextMenuItems = null;
            optLöschen.Location = new Point(8, 64);
            optLöschen.Name = "optLöschen";
            optLöschen.Size = new Size(416, 56);
            optLöschen.TabIndex = 2;
            optLöschen.Text = "Die jüngste(n) Zeile <b>löschen</b><i><br>Sind die Zeilen gleich alt, wird zufällig eine gewählt<br>Um alle doppelten zu löschen, Befehl mehrfach ausführen.";
            // 
            // optFülle
            // 
            optFülle.ButtonStyle = ButtonStyle.Optionbox_Text;
            optFülle.Checked = true;
            optFülle.CustomContextMenuItems = null;
            optFülle.Location = new Point(8, 24);
            optFülle.Name = "optFülle";
            optFülle.Size = new Size(416, 40);
            optFülle.TabIndex = 1;
            optFülle.Text = "Leere Zellen mit Werten anderer Zeilen <b>befüllen</b><br><i>Dardurch entstehen evtl. doppelte Zeilen.";
            // 
            // RowCleanUp
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(731, 313);
            Controls.Add(grpVerhalten);
            Controls.Add(capSpalten);
            Controls.Add(lstColumns);
            Controls.Add(btnCancel);
            Controls.Add(btnExecute);
            Controls.Add(txtInfo);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            GlobalMenuHeight = 0;
            Name = "RowCleanUp";
            ShowInTaskbar = false;
            Text = "Zeilen aufräumen:";
            Controls.SetChildIndex(txtInfo, 0);
            Controls.SetChildIndex(btnExecute, 0);
            Controls.SetChildIndex(btnCancel, 0);
            Controls.SetChildIndex(pnlStatusBar, 0);
            Controls.SetChildIndex(lstColumns, 0);
            Controls.SetChildIndex(capSpalten, 0);
            Controls.SetChildIndex(grpVerhalten, 0);
            pnlStatusBar.ResumeLayout(false);
            grpVerhalten.ResumeLayout(false);
            ResumeLayout(false);

        }
        private Caption txtInfo;
        private Button btnExecute;
        private Button btnCancel;
        private ListBox lstColumns;
        private Caption capSpalten;
        private GroupBox grpVerhalten;
        private Button optLöschen;
        private Button optFülle;
    }
}
