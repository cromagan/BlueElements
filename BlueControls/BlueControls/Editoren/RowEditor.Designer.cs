namespace BlueControls.Editoren;

partial class RowEditor {
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
            this.formular = new BlueControls.Controls.ConnectedFormulaView();
            this.SuspendLayout();
            // 
            // formular
            // 
            this.formular.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.formular.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.Nothing;
            this.formular.Location = new System.Drawing.Point(0, 0);
            this.formular.Name = "formular";
            this.formular.Size = new System.Drawing.Size(523, 612);
            this.formular.TabIndex = 0;
            this.formular.TabStop = false;
            // 
            // RowEditor
            // 
            this.Controls.Add(this.formular);
            this.Name = "RowEditor";
            this.Size = new System.Drawing.Size(523, 612);
            this.ResumeLayout(false);

    }

    #endregion

    private Controls.ConnectedFormulaView formular;
}
