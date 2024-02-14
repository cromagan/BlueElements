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
            this.splitContainer1.Panel2.Controls.Add(this.Suggest);
            this.splitContainer1.Size = new Size(345, 371);
            this.splitContainer1.SplitterDistance = 115;
            this.splitContainer1.TabIndex = 0;
            // 
            // Main
            // 
            this.Main.AddAllowed = AddType.Text;
            this.Main.AutoSort = true;
            this.Main.CheckBehavior = CheckBehavior.AllSelected;
            this.Main.Dock = DockStyle.Fill;
            this.Main.Location = new Point(0, 0);
            this.Main.Name = "Main";
            this.Main.Size = new Size(345, 115);
            this.Main.TabIndex = 0;
            this.Main.ItemClicked += new EventHandler<AbstractListItemEventArgs>(this.Main_ItemClicked);
            this.Main.ItemCheckedChanged += Main_ItemCheckedChanged;
            // 
            // Suggest
            // 
            this.Suggest.AddAllowed = AddType.None;
            this.Suggest.AutoSort = true;
            this.Suggest.CheckBehavior = CheckBehavior.AllSelected;
            this.Suggest.Dock = DockStyle.Fill;
            this.Suggest.Location = new Point(0, 0);
            this.Suggest.Name = "Suggest";
            this.Suggest.Size = new Size(345, 252);
            this.Suggest.TabIndex = 1;
            this.Suggest.ItemClicked += new EventHandler<AbstractListItemEventArgs>(this.Suggest_ItemClicked);
            // 
            // SwapListBox
            // 
            this.Controls.Add(this.splitContainer1);
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
    }
}
