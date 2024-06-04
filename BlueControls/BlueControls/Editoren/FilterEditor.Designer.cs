namespace BlueControls.Editoren;

partial class FilterEditor {
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
            this.cbxFilterType = new BlueControls.Controls.ComboBox();
            this.cbxColumn = new BlueControls.Controls.ComboBox();
            this.txbFilterText = new BlueControls.Controls.TextBox();
            this.SuspendLayout();
            // 
            // cbxFilterType
            // 
            this.cbxFilterType.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxFilterType.Location = new System.Drawing.Point(8, 8);
            this.cbxFilterType.Name = "cbxFilterType";
            this.cbxFilterType.Size = new System.Drawing.Size(304, 32);
            this.cbxFilterType.TabIndex = 0;
            // 
            // cbxColumn
            // 
            this.cbxColumn.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxColumn.Location = new System.Drawing.Point(8, 48);
            this.cbxColumn.Name = "cbxColumn";
            this.cbxColumn.Size = new System.Drawing.Size(304, 32);
            this.cbxColumn.TabIndex = 1;
            // 
            // txbFilterText
            // 
            this.txbFilterText.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbFilterText.Location = new System.Drawing.Point(8, 88);
            this.txbFilterText.Name = "txbFilterText";
            this.txbFilterText.Size = new System.Drawing.Size(304, 32);
            this.txbFilterText.TabIndex = 2;
            // 
            // FilterEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txbFilterText);
            this.Controls.Add(this.cbxColumn);
            this.Controls.Add(this.cbxFilterType);
            this.Name = "FilterEditor";
            this.Size = new System.Drawing.Size(323, 136);
            this.ResumeLayout(false);

    }

    #endregion

    private Controls.ComboBox cbxFilterType;
    private Controls.ComboBox cbxColumn;
    private Controls.TextBox txbFilterText;
}
