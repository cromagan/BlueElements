using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;

namespace BlueControls.BlueDatabaseDialogs
{
    public sealed partial class UserInfo {
        //Das Formular überschreibt den Deletevorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.tblUndo = new Table();
            this.pnlStatusBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlStatusBar
            // 
            this.pnlStatusBar.Location = new Point(0, 416);
            this.pnlStatusBar.Size = new Size(853, 24);
            // 
            // tblUndo
            // 
            this.tblUndo.Dock = DockStyle.Fill;
            this.tblUndo.Location = new Point(0, 0);
            this.tblUndo.Name = "tblUndo";
            this.tblUndo.Size = new Size(853, 416);
            this.tblUndo.TabIndex = 97;
            // 
            // UserInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new Size(853, 440);
            this.Controls.Add(this.tblUndo);
            this.Name = "UserInfo";
            this.Tag = "";
            this.Text = "Benutzer Info";
            this.Controls.SetChildIndex(this.pnlStatusBar, 0);
            this.Controls.SetChildIndex(this.tblUndo, 0);
            this.pnlStatusBar.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private Table tblUndo;
    }
}
