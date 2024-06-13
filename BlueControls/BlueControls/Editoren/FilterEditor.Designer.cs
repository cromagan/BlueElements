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
            this.capFilterType = new BlueControls.Controls.Caption();
            this.capSpalte = new BlueControls.Controls.Caption();
            this.capSuch = new BlueControls.Controls.Caption();
            this.SuspendLayout();
            // 
            // cbxFilterType
            // 
            this.cbxFilterType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxFilterType.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxFilterType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxFilterType.Location = new System.Drawing.Point(96, 24);
            this.cbxFilterType.Name = "cbxFilterType";
            this.cbxFilterType.Size = new System.Drawing.Size(216, 32);
            this.cbxFilterType.TabIndex = 0;
            this.cbxFilterType.TextChanged += new System.EventHandler(this.cbxFilterType_TextChanged);
            // 
            // cbxColumn
            // 
            this.cbxColumn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxColumn.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxColumn.Location = new System.Drawing.Point(96, 64);
            this.cbxColumn.Name = "cbxColumn";
            this.cbxColumn.Size = new System.Drawing.Size(216, 32);
            this.cbxColumn.TabIndex = 1;
            this.cbxColumn.TextChanged += new System.EventHandler(this.cbxColumn_TextChanged);
            // 
            // txbFilterText
            // 
            this.txbFilterText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbFilterText.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbFilterText.Location = new System.Drawing.Point(96, 104);
            this.txbFilterText.Name = "txbFilterText";
            this.txbFilterText.Size = new System.Drawing.Size(216, 112);
            this.txbFilterText.TabIndex = 2;
            this.txbFilterText.TextChanged += new System.EventHandler(this.txbFilterText_TextChanged);
            // 
            // capFilterType
            // 
            this.capFilterType.CausesValidation = false;
            this.capFilterType.Location = new System.Drawing.Point(8, 24);
            this.capFilterType.Name = "capFilterType";
            this.capFilterType.Size = new System.Drawing.Size(64, 24);
            this.capFilterType.Text = "Filter:";
            // 
            // capSpalte
            // 
            this.capSpalte.CausesValidation = false;
            this.capSpalte.Location = new System.Drawing.Point(8, 64);
            this.capSpalte.Name = "capSpalte";
            this.capSpalte.Size = new System.Drawing.Size(64, 24);
            this.capSpalte.Text = "Spalte:";
            // 
            // capSuch
            // 
            this.capSuch.CausesValidation = false;
            this.capSuch.Location = new System.Drawing.Point(8, 104);
            this.capSuch.Name = "capSuch";
            this.capSuch.Size = new System.Drawing.Size(80, 24);
            this.capSuch.Text = "Filter-Wert:";
            // 
            // FilterEditor
            // 
            this.Controls.Add(this.capSuch);
            this.Controls.Add(this.capSpalte);
            this.Controls.Add(this.capFilterType);
            this.Controls.Add(this.txbFilterText);
            this.Controls.Add(this.cbxColumn);
            this.Controls.Add(this.cbxFilterType);
            this.Name = "FilterEditor";
            this.Size = new System.Drawing.Size(323, 224);
            this.ResumeLayout(false);

    }

    #endregion

    private Controls.ComboBox cbxFilterType;
    private Controls.ComboBox cbxColumn;
    private Controls.TextBox txbFilterText;
    private Controls.Caption capFilterType;
    private Controls.Caption capSpalte;
    private Controls.Caption capSuch;
}
