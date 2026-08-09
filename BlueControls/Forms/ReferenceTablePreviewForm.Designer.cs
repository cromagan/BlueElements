// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.Editoren;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Classes;
using ListBox = BlueControls.Controls.ListBox;

namespace BlueControls.Forms;

public sealed partial class ReferenceTablePreviewForm {
    /// <summary>
    /// Erforderliche Designervariable.
    /// </summary>
    private IContainer components = null;

    /// <summary>
    /// Verwendete Ressourcen bereinigen.
    /// </summary>
    /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
    protected override void Dispose(bool disposing) {
        if (disposing && components is not null) {
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
        splMain = new SplitContainer();
        splTop = new SplitContainer();
        lstRows = new ListBox();
        padVorschau = new CreativePad();
        varEditor = new VariableEditor();
        ((ISupportInitialize)splMain).BeginInit();
        splMain.Panel1.SuspendLayout();
        splMain.Panel2.SuspendLayout();
        splMain.SuspendLayout();
        ((ISupportInitialize)splTop).BeginInit();
        splTop.Panel1.SuspendLayout();
        splTop.Panel2.SuspendLayout();
        splTop.SuspendLayout();
        SuspendLayout();
        //
        // splMain (horizontal: oben Vorschau, unten Variablen)
        //
        splMain.Dock = DockStyle.Fill;
        splMain.Location = new Point(0, 0);
        splMain.Name = "splMain";
        splMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
        //
        // splMain.Panel1
        //
        splMain.Panel1.Controls.Add(splTop);
        //
        // splMain.Panel2
        //
        splMain.Panel2.Controls.Add(varEditor);
        splMain.Size = new Size(984, 661);
        splMain.SplitterDistance = 450;
        splMain.SplitterWidth = 8;
        splMain.TabIndex = 0;
        //
        // splTop (vertikal: links ListBox, rechts CreativePad)
        //
        splTop.Dock = DockStyle.Fill;
        splTop.Location = new Point(0, 0);
        splTop.Name = "splTop";
        //
        // splTop.Panel1
        //
        splTop.Panel1.Controls.Add(lstRows);
        //
        // splTop.Panel2
        //
        splTop.Panel2.Controls.Add(padVorschau);
        splTop.Size = new Size(984, 450);
        splTop.SplitterDistance = 300;
        splTop.SplitterWidth = 8;
        splTop.TabIndex = 0;
        //
        // lstRows
        //
        lstRows.AddAllowed = AddType.None;
        lstRows.Dock = DockStyle.Fill;
        lstRows.Location = new Point(0, 0);
        lstRows.Name = "lstRows";
        lstRows.Size = new Size(300, 450);
        lstRows.TabIndex = 0;
        //
        // padVorschau
        //
        padVorschau.Dock = DockStyle.Fill;
        padVorschau.Location = new Point(0, 0);
        padVorschau.Name = "padVorschau";
        padVorschau.ShowInPrintMode = true;
        padVorschau.Size = new Size(676, 450);
        padVorschau.TabIndex = 0;
        //
        // varEditor
        //
        varEditor.Dock = DockStyle.Fill;
        varEditor.Location = new Point(0, 0);
        varEditor.Name = "varEditor";
        varEditor.Size = new Size(984, 203);
        varEditor.TabIndex = 0;
        //
        // ReferenceTablePreviewForm
        //
        AutoScaleMode = AutoScaleMode.None;
        ClientSize = new Size(984, 661);
        Controls.Add(splMain);
        FormBorderStyle = FormBorderStyle.SizableToolWindow;
        Name = "ReferenceTablePreviewForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Vorschau";
        splMain.Panel1.ResumeLayout(false);
        splMain.Panel2.ResumeLayout(false);
        ((ISupportInitialize)splMain).EndInit();
        splMain.ResumeLayout(false);
        splTop.Panel1.ResumeLayout(false);
        splTop.Panel2.ResumeLayout(false);
        ((ISupportInitialize)splTop).EndInit();
        splTop.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private SplitContainer splMain;
    private SplitContainer splTop;
    private ListBox lstRows;
    private CreativePad padVorschau;
    private VariableEditor varEditor;
}
