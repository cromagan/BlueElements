using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;
using ListBox = BlueControls.Controls.ListBox;

namespace BlueControls.Forms {
    public partial class RowSortDefinitionEditor {
        //Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing) {
            try {
                if (disposing) {
                }
            } finally {
                base.Dispose(disposing);
            }
        }
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            this.lbxSortierSpalten = new BlueControls.Controls.ListBox();
            this.capSortierspalten = new BlueControls.Controls.Caption();
            this.btnSortRichtung = new BlueControls.Controls.Button();
            this.SuspendLayout();
            // 
            // lbxSortierSpalten
            // 
            this.lbxSortierSpalten.AddAllowed = BlueControls.Enums.AddType.OnlySuggests;
            this.lbxSortierSpalten.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbxSortierSpalten.AutoSort = false;
            this.lbxSortierSpalten.CheckBehavior = BlueControls.Enums.CheckBehavior.AllSelected;
            this.lbxSortierSpalten.FilterText = null;
            this.lbxSortierSpalten.Location = new System.Drawing.Point(8, 32);
            this.lbxSortierSpalten.MoveAllowed = true;
            this.lbxSortierSpalten.Name = "lbxSortierSpalten";
            this.lbxSortierSpalten.RemoveAllowed = true;
            this.lbxSortierSpalten.Size = new System.Drawing.Size(336, 352);
            this.lbxSortierSpalten.TabIndex = 8;
            this.lbxSortierSpalten.ItemAddedByClick += new System.EventHandler<BlueControls.EventArgs.AbstractListItemEventArgs>(this.lbxSortierSpalten_ItemAddedByClick);
            this.lbxSortierSpalten.RemoveClicked += new System.EventHandler<BlueControls.EventArgs.AbstractListItemEventArgs>(this.lbxSortierSpalten_RemoveClicked);
            this.lbxSortierSpalten.UpDownClicked += new System.EventHandler(this.lbxSortierSpalten_UpDownClicked);
            // 
            // capSortierspalten
            // 
            this.capSortierspalten.CausesValidation = false;
            this.capSortierspalten.Location = new System.Drawing.Point(8, 8);
            this.capSortierspalten.Name = "capSortierspalten";
            this.capSortierspalten.Size = new System.Drawing.Size(160, 24);
            this.capSortierspalten.Text = "Sortier-Spalten:";
            // 
            // btnSortRichtung
            // 
            this.btnSortRichtung.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSortRichtung.ButtonStyle = BlueControls.Enums.ButtonStyle.Yes_or_No;
            this.btnSortRichtung.Location = new System.Drawing.Point(8, 392);
            this.btnSortRichtung.Name = "btnSortRichtung";
            this.btnSortRichtung.Size = new System.Drawing.Size(336, 40);
            this.btnSortRichtung.TabIndex = 7;
            this.btnSortRichtung.Text = "Umgekehrte Sortierung";
            this.btnSortRichtung.MouseClick += new System.Windows.Forms.MouseEventHandler(this.btnSortRichtung_MouseClick);
            // 
            // RowSortDefinitionEditor
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.lbxSortierSpalten);
            this.Controls.Add(this.capSortierspalten);
            this.Controls.Add(this.btnSortRichtung);
            this.Name = "RowSortDefinitionEditor";
            this.Size = new System.Drawing.Size(355, 442);
            this.ResumeLayout(false);

        }
        private ListBox lbxSortierSpalten;
        private Caption capSortierspalten;
        private Button btnSortRichtung;
    }
}