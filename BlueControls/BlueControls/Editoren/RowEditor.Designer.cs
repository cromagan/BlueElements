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
            this.formular.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.Nothing;
            this.formular.Location = new System.Drawing.Point(8, 16);
            this.formular.Mode = "";
            this.formular.Name = "formular";
            this.formular.Size = new System.Drawing.Size(448, 400);
            this.formular.TabIndex = 0;
            this.formular.TabStop = false;
            // 
            // RowEditor
            // 
            this.Controls.Add(this.formular);
            this.Name = "RowEditor";
            this.Size = new System.Drawing.Size(468, 427);
            this.ResumeLayout(false);

    }

    #endregion

    private Controls.ConnectedFormulaView formular;
}
