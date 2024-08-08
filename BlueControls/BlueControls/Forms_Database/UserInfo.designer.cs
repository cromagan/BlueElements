using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;
using TextBox = BlueControls.Controls.TextBox;

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
            this.tblUndo = new BlueControls.Controls.Table();
            this.pnlStatusBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // capStatusBar
            // 
            this.capStatusBar.Size = new System.Drawing.Size(853, 24);
            this.capStatusBar.Text = "<imagecode=Häkchen|16> Nix besonderes zu berichten...";
            // 
            // pnlStatusBar
            // 
            this.pnlStatusBar.Location = new System.Drawing.Point(0, 416);
            this.pnlStatusBar.Size = new System.Drawing.Size(853, 24);
            // 
            // tblUndo
            // 
            this.tblUndo.Arrangement = "";
            this.tblUndo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblUndo.Item = null;
            this.tblUndo.Location = new System.Drawing.Point(0, 0);
            this.tblUndo.Mode = "";
            this.tblUndo.Name = "tblUndo";
            this.tblUndo.Size = new System.Drawing.Size(853, 416);
            this.tblUndo.TabIndex = 97;
            // 
            // UserInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(853, 440);
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
