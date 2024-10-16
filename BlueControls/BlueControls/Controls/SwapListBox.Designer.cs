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
            this.splitContainer1 = new SplitContainer();
            this.Main = new ListBox();
            this.btnFilterDel = new Button();
            this.txbFilter = new TextBox();
            this.Suggest = new ListBox();
            ((ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = DockStyle.Fill;
            this.splitContainer1.Location = new Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = Orientation.Horizontal;
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
            this.splitContainer1.Size = new Size(345, 371);
            this.splitContainer1.SplitterDistance = 115;
            this.splitContainer1.TabIndex = 0;
            // 
            // Main
            // 
            this.Main.AddAllowed = AddType.Text;
            this.Main.CheckBehavior = CheckBehavior.AllSelected;
            this.Main.Dock = DockStyle.Fill;
            this.Main.Location = new Point(0, 0);
            this.Main.Name = "Main";
            this.Main.Size = new Size(345, 115);
            this.Main.TabIndex = 0;
            this.Main.ItemClicked += new EventHandler<AbstractListItemEventArgs>(this.Main_ItemClicked);
            this.Main.ItemAddedByClick += new EventHandler<AbstractListItemEventArgs>(this.Main_ItemAddedByClick);

            // 
            // btnFilterDel
            // 
            this.btnFilterDel.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.btnFilterDel.Enabled = false;
            this.btnFilterDel.ImageCode = "Trichter|16||1";
            this.btnFilterDel.Location = new Point(304, 0);
            this.btnFilterDel.Name = "btnFilterDel";
            this.btnFilterDel.QuickInfo = "Filter löschen";
            this.btnFilterDel.Size = new Size(40, 24);
            this.btnFilterDel.TabIndex = 3;
            this.btnFilterDel.Click += new EventHandler(this.btnFilterDel_Click);
            // 
            // txbFilter
            // 
            this.txbFilter.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                     | AnchorStyles.Right)));
            this.txbFilter.Cursor = Cursors.IBeam;
            this.txbFilter.Location = new Point(0, 0);
            this.txbFilter.Name = "txbFilter";
            this.txbFilter.QuickInfo = "Textfilter";
            this.txbFilter.Size = new Size(304, 24);
            this.txbFilter.TabIndex = 2;
            this.txbFilter.TextChanged += new EventHandler(this.txbFilter_TextChanged);
            // 
            // Suggest
            // 
            this.Suggest.AddAllowed = AddType.None;
            this.Suggest.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                    | AnchorStyles.Left) 
                                                   | AnchorStyles.Right)));
            this.Suggest.CheckBehavior = CheckBehavior.AllSelected;
            this.Suggest.Location = new Point(0, 24);
            this.Suggest.Name = "Suggest";
            this.Suggest.Size = new Size(345, 228);
            this.Suggest.TabIndex = 1;
            this.Suggest.ItemClicked += new EventHandler<AbstractListItemEventArgs>(this.Suggest_ItemClicked);
            // 
            // SwapListBox
            // 
            this.Controls.Add(this.splitContainer1);
            this.Name = "SwapListBox";
            this.Size = new Size(345, 371);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((ISupportInitialize)(this.splitContainer1)).EndInit();
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
