namespace BlueControls.Controls {
    partial class SwapListBox {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
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
            this.splitContainer1.Panel2.Controls.Add(this.Suggest);
            this.splitContainer1.Size = new System.Drawing.Size(345, 371);
            this.splitContainer1.SplitterDistance = 115;
            this.splitContainer1.TabIndex = 0;
            // 
            // Main
            // 
            this.Main.AddAllowed = BlueControls.Enums.enAddType.Text;
            this.Main.CheckBehavior = BlueControls.Enums.enCheckBehavior.NoSelection;
            this.Main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Main.LastFilePath = null;
            this.Main.Location = new System.Drawing.Point(0, 0);
            this.Main.Name = "Main";
            this.Main.Size = new System.Drawing.Size(345, 115);
            this.Main.TabIndex = 0;
            this.Main.ItemAdded += new System.EventHandler<BlueBasics.EventArgs.ListEventArgs>(this.Main_ItemAdded);
            this.Main.ItemRemoving += new System.EventHandler<BlueBasics.EventArgs.ListEventArgs>(this.Main_ItemRemoving);
            this.Main.ItemRemoved += new System.EventHandler(this.Main_ItemRemoved);
            this.Main.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.Main_ItemClicked);
            this.Main.AddClicked += new System.EventHandler(this.Main_AddClicked);
            // 
            // Suggest
            // 
            this.Suggest.AddAllowed = BlueControls.Enums.enAddType.None;
            this.Suggest.CheckBehavior = BlueControls.Enums.enCheckBehavior.NoSelection;
            this.Suggest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Suggest.FilterAllowed = true;
            this.Suggest.LastFilePath = null;
            this.Suggest.Location = new System.Drawing.Point(0, 0);
            this.Suggest.Name = "Suggest";
            this.Suggest.Size = new System.Drawing.Size(345, 252);
            this.Suggest.TabIndex = 1;
            this.Suggest.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.Suggest_ItemClicked);
            // 
            // SwapListBox
            // 
            this.Controls.Add(this.splitContainer1);
            this.Size = new System.Drawing.Size(345, 371);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }


        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private ListBox Main;
        private ListBox Suggest;
    }
}
