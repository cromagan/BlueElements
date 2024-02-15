using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Enums;
using BlueControls.EventArgs;

namespace BlueControls.Controls {
    partial class SwapListBox {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private IContainer components = null;
        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing) {
            if (disposing) {
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code
        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent() {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.Main = new BlueControls.Controls.ListBox();
            this.btnFilterDel = new BlueControls.Controls.Button();
            this.txbFilter = new BlueControls.Controls.TextBox();
            this.Suggest = new BlueControls.Controls.ListBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.Main);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.btnFilterDel);
            this.splitContainer1.Panel2.Controls.Add(this.txbFilter);
            this.splitContainer1.Panel2.Controls.Add(this.Suggest);
            this.splitContainer1.Size = new System.Drawing.Size(345, 371);
            this.splitContainer1.SplitterDistance = 115;
            this.splitContainer1.TabIndex = 0;
            // 
            // Main
            // 
            this.Main.AddAllowed = BlueControls.Enums.AddType.Text;
            this.Main.CheckBehavior = BlueControls.Enums.CheckBehavior.AllSelected;
            this.Main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Main.Location = new System.Drawing.Point(0, 0);
            this.Main.Name = "Main";
            this.Main.Size = new System.Drawing.Size(345, 115);
            this.Main.TabIndex = 0;
            this.Main.ItemClicked += new System.EventHandler<BlueControls.EventArgs.AbstractListItemEventArgs>(this.Main_ItemClicked);
            // 
            // btnFilterDel
            // 
            this.btnFilterDel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFilterDel.Enabled = false;
            this.btnFilterDel.ImageCode = "Trichter|16||1";
            this.btnFilterDel.Location = new System.Drawing.Point(304, 0);
            this.btnFilterDel.Name = "btnFilterDel";
            this.btnFilterDel.QuickInfo = "Filter löschen";
            this.btnFilterDel.Size = new System.Drawing.Size(40, 24);
            this.btnFilterDel.TabIndex = 3;
            this.btnFilterDel.Click += new System.EventHandler(this.btnFilterDel_Click);
            // 
            // txbFilter
            // 
            this.txbFilter.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbFilter.Location = new System.Drawing.Point(0, 0);
            this.txbFilter.Name = "txbFilter";
            this.txbFilter.QuickInfo = "Textfilter";
            this.txbFilter.Size = new System.Drawing.Size(304, 24);
            this.txbFilter.TabIndex = 2;
            this.txbFilter.TextChanged += new System.EventHandler(this.txbFilter_TextChanged);
            // 
            // Suggest
            // 
            this.Suggest.AddAllowed = BlueControls.Enums.AddType.None;
            this.Suggest.CheckBehavior = BlueControls.Enums.CheckBehavior.AllSelected;
            this.Suggest.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.Suggest.Location = new System.Drawing.Point(0, 24);
            this.Suggest.Name = "Suggest";
            this.Suggest.Size = new System.Drawing.Size(345, 228);
            this.Suggest.TabIndex = 1;
            this.Suggest.ItemClicked += new System.EventHandler<BlueControls.EventArgs.AbstractListItemEventArgs>(this.Suggest_ItemClicked);
            // 
            // SwapListBox
            // 
            this.Controls.Add(this.splitContainer1);
            this.Name = "SwapListBox";
            this.Size = new System.Drawing.Size(345, 371);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }


        #endregion

        private SplitContainer splitContainer1;
        private ListBox Main;
        private ListBox Suggest;
        private Button btnFilterDel;
        private TextBox txbFilter;
    }
}
