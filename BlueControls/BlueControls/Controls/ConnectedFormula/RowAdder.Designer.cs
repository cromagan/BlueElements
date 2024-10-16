using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Enums;
using BlueControls.EventArgs;

namespace BlueControls.Controls;

partial class RowAdder {
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
            this.f = new ListBox();
            this.SuspendLayout();
            // 
            // f
            // 
            this.f.AddAllowed = AddType.None;
            this.f.Appearance = ListBoxAppearance.Listbox_Boxes;
            this.f.CheckBehavior = CheckBehavior.MultiSelection;
            this.f.Dock = DockStyle.Fill;
            this.f.Location = new Point(0, 0);
            this.f.Name = "f";
            this.f.Size = new Size(381, 419);
            this.f.TabIndex = 0;
            this.f.ItemClicked += new EventHandler<AbstractListItemEventArgs>(this.F_ItemClicked);
            // 
            // RowAdder
            // 
            this.Controls.Add(this.f);
            this.Name = "RowAdder";
            this.Size = new Size(381, 419);
            this.ResumeLayout(false);

    }



    #endregion

    public ListBox f;

}
