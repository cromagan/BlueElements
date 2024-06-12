namespace BlueControls.Editoren;

partial class FilterCollectionEditor {
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
            this.capDatabase = new BlueControls.Controls.Caption();
            this.capFilter = new BlueControls.Controls.Caption();
            this.lstFilter = new BlueControls.Controls.ListBox();
            this.SuspendLayout();
            // 
            // capDatabase
            // 
            this.capDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.capDatabase.CausesValidation = false;
            this.capDatabase.Location = new System.Drawing.Point(8, 16);
            this.capDatabase.Name = "capDatabase";
            this.capDatabase.Size = new System.Drawing.Size(400, 32);
            this.capDatabase.Text = "Datenbank: ?";
            // 
            // capFilter
            // 
            this.capFilter.CausesValidation = false;
            this.capFilter.Location = new System.Drawing.Point(8, 56);
            this.capFilter.Name = "capFilter";
            this.capFilter.Size = new System.Drawing.Size(75, 23);
            this.capFilter.Text = "Filter:";
            // 
            // lstFilter
            // 
            this.lstFilter.AddAllowed = BlueControls.Enums.AddType.UserDef;
            this.lstFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstFilter.ItemEditAllowed = true;
            this.lstFilter.Location = new System.Drawing.Point(8, 72);
            this.lstFilter.Name = "lstFilter";
            this.lstFilter.RemoveAllowed = true;
            this.lstFilter.Size = new System.Drawing.Size(400, 296);
            this.lstFilter.TabIndex = 1;
            this.lstFilter.RemoveClicked += new System.EventHandler<BlueControls.EventArgs.AbstractListItemEventArgs>(this.lstFilter_RemoveClicked);
            // 
            // FilterCollectionEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lstFilter);
            this.Controls.Add(this.capFilter);
            this.Controls.Add(this.capDatabase);
            this.Name = "FilterCollectionEditor";
            this.ResumeLayout(false);

    }

    #endregion

    private Controls.Caption capDatabase;
    private Controls.Caption capFilter;
    private Controls.ListBox lstFilter;
}
