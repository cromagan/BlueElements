using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;

namespace BlueControls.Editoren;

partial class RowEditor {
    /// <summary> 
    /// Erforderliche Designervariable.
    /// </summary>
    private IContainer components = null;

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
            this.formular = new ConnectedFormulaView();
            this.SuspendLayout();
            // 
            // formular
            // 
            this.formular.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                     | AnchorStyles.Left) 
                                                    | AnchorStyles.Right)));
            this.formular.GroupBoxStyle = GroupBoxStyle.Nothing;
            this.formular.Location = new Point(0, 0);
            this.formular.Name = "formular";
            this.formular.Size = new Size(523, 612);
            this.formular.TabIndex = 0;
            this.formular.TabStop = false;
            // 
            // RowEditor
            // 
            this.Controls.Add(this.formular);
            this.Name = "RowEditor";
            this.Size = new Size(523, 612);
            this.ResumeLayout(false);

    }

    #endregion

    private ConnectedFormulaView formular;
}
