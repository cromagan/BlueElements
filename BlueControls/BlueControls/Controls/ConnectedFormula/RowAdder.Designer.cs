namespace BlueControls.Controls;

partial class RowAdder {
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
            OnDisposingEvent();
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
            this.lstTexte = new BlueControls.Controls.ListBox();
            this.SuspendLayout();
            // 
            // lstTexte
            // 
            this.lstTexte.AddAllowed = BlueControls.Enums.AddType.None;
            this.lstTexte.CheckBehavior = BlueControls.Enums.CheckBehavior.MultiSelection;
            this.lstTexte.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstTexte.Location = new System.Drawing.Point(0, 0);
            this.lstTexte.Name = "lstTexte";
            this.lstTexte.Size = new System.Drawing.Size(381, 419);
            this.lstTexte.TabIndex = 0;
            this.lstTexte.ItemClicked += new System.EventHandler<BlueControls.EventArgs.AbstractListItemEventArgs>(this.lstTexte_ItemClicked);
            // 
            // RowAdder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lstTexte);
            this.Name = "RowAdder";
            this.Size = new System.Drawing.Size(381, 419);
            this.ResumeLayout(false);

    }

    #endregion

    private ListBox lstTexte;
}
