using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using ListBox = BlueControls.Controls.ListBox;

namespace BlueControls.Editoren;

partial class FilterCollectionEditor {
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
            this.capDatabase = new Caption();
            this.capFilter = new Caption();
            this.lstFilter = new ListBox();
            this.SuspendLayout();
            // 
            // capDatabase
            // 
            this.capDatabase.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                       | AnchorStyles.Right)));
            this.capDatabase.CausesValidation = false;
            this.capDatabase.Location = new Point(8, 16);
            this.capDatabase.Name = "capDatabase";
            this.capDatabase.Size = new Size(400, 32);
            this.capDatabase.Text = "Datenbank: ?";
            // 
            // capFilter
            // 
            this.capFilter.CausesValidation = false;
            this.capFilter.Location = new Point(8, 56);
            this.capFilter.Name = "capFilter";
            this.capFilter.Size = new Size(75, 23);
            this.capFilter.Text = "Filter:";
            // 
            // lstFilter
            // 
            this.lstFilter.AddAllowed = AddType.UserDef;
            this.lstFilter.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                      | AnchorStyles.Left) 
                                                     | AnchorStyles.Right)));
            this.lstFilter.ItemEditAllowed = true;
            this.lstFilter.Location = new Point(8, 72);
            this.lstFilter.Name = "lstFilter";
            this.lstFilter.RemoveAllowed = true;
            this.lstFilter.Size = new Size(400, 296);
            this.lstFilter.TabIndex = 1;
            this.lstFilter.RemoveClicked += new EventHandler<AbstractListItemEventArgs>(this.lstFilter_RemoveClicked);
            // 
            // FilterCollectionEditor
            // 
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this.lstFilter);
            this.Controls.Add(this.capFilter);
            this.Controls.Add(this.capDatabase);
            this.Name = "FilterCollectionEditor";
            this.ResumeLayout(false);

    }

    #endregion

    private Caption capDatabase;
    private Caption capFilter;
    private ListBox lstFilter;
}
