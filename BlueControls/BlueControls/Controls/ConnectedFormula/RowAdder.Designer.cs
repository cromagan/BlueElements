using System.Windows.Forms;

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
            this.f = new ListBox();
            this.SuspendLayout();
            //
            // f
            //
            this.f.Dock = DockStyle.Fill;   
            this.f.AddAllowed = BlueControls.Enums.AddType.None;
            this.f.Appearance = BlueControls.Enums.ListBoxAppearance.Listbox_Boxes;
            this.f.CheckBehavior = BlueControls.Enums.CheckBehavior.MultiSelection;
            this.f.ItemClicked += F_ItemClicked;
            // 
            // RowAdder
            // 
            this.Name = "RowAdder";
            this.Size = new System.Drawing.Size(381, 419);
            this.Size = new System.Drawing.Size(381, 419);
            this.ResumeLayout(false);
    }



    #endregion

    public ListBox f;

}
