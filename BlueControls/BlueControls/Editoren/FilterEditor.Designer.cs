using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using ComboBox = BlueControls.Controls.ComboBox;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls.Editoren;

partial class FilterEditor {
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
            this.cbxFilterType = new ComboBox();
            this.cbxColumn = new ComboBox();
            this.txbFilterText = new TextBox();
            this.capFilterType = new Caption();
            this.capSpalte = new Caption();
            this.capSuch = new Caption();
            this.SuspendLayout();
            // 
            // cbxFilterType
            // 
            this.cbxFilterType.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                         | AnchorStyles.Right)));
            this.cbxFilterType.Cursor = Cursors.IBeam;
            this.cbxFilterType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbxFilterType.Location = new Point(96, 24);
            this.cbxFilterType.Name = "cbxFilterType";
            this.cbxFilterType.Size = new Size(216, 32);
            this.cbxFilterType.TabIndex = 0;
            this.cbxFilterType.TextChanged += new EventHandler(this.cbxFilterType_TextChanged);
            // 
            // cbxColumn
            // 
            this.cbxColumn.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                     | AnchorStyles.Right)));
            this.cbxColumn.Cursor = Cursors.IBeam;
            this.cbxColumn.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbxColumn.Location = new Point(96, 64);
            this.cbxColumn.Name = "cbxColumn";
            this.cbxColumn.Size = new Size(216, 32);
            this.cbxColumn.TabIndex = 1;
            this.cbxColumn.TextChanged += new EventHandler(this.cbxColumn_TextChanged);
            // 
            // txbFilterText
            // 
            this.txbFilterText.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                          | AnchorStyles.Left) 
                                                         | AnchorStyles.Right)));
            this.txbFilterText.Cursor = Cursors.IBeam;
            this.txbFilterText.Location = new Point(96, 104);
            this.txbFilterText.Name = "txbFilterText";
            this.txbFilterText.Size = new Size(216, 112);
            this.txbFilterText.TabIndex = 2;
            this.txbFilterText.TextChanged += new EventHandler(this.txbFilterText_TextChanged);
            // 
            // capFilterType
            // 
            this.capFilterType.CausesValidation = false;
            this.capFilterType.Location = new Point(8, 24);
            this.capFilterType.Name = "capFilterType";
            this.capFilterType.Size = new Size(64, 24);
            this.capFilterType.Text = "Filter:";
            // 
            // capSpalte
            // 
            this.capSpalte.CausesValidation = false;
            this.capSpalte.Location = new Point(8, 64);
            this.capSpalte.Name = "capSpalte";
            this.capSpalte.Size = new Size(64, 24);
            this.capSpalte.Text = "Spalte:";
            // 
            // capSuch
            // 
            this.capSuch.CausesValidation = false;
            this.capSuch.Location = new Point(8, 104);
            this.capSuch.Name = "capSuch";
            this.capSuch.Size = new Size(80, 24);
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
            this.Size = new Size(323, 224);
            this.ResumeLayout(false);

    }

    #endregion

    private ComboBox cbxFilterType;
    private ComboBox cbxColumn;
    private TextBox txbFilterText;
    private Caption capFilterType;
    private Caption capSpalte;
    private Caption capSuch;
}
