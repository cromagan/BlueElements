using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Enums;

namespace BlueControls.Controls
{
    public partial class Monitor {

        //Wird vom Windows Form-Designer benötigt.
        private IContainer components;
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.capInfo = new Caption();
            this.lstDone = new ListBox();
            this.SuspendLayout();
            // 
            // capInfo
            // 
            this.capInfo.CausesValidation = false;
            this.capInfo.Dock = DockStyle.Top;
            this.capInfo.Location = new Point(0, 0);
            this.capInfo.Name = "capInfo";
            this.capInfo.Size = new Size(401, 24);
            // 
            // lstDone
            // 
            this.lstDone.AddAllowed = AddType.None;
            this.lstDone.AutoSort = true;
            this.lstDone.Dock = DockStyle.Fill;
            this.lstDone.Location = new Point(0, 24);
            this.lstDone.Name = "lstDone";
            this.lstDone.Size = new Size(401, 384);
            this.lstDone.TabIndex = 1;
            // 
            // Monitor
            // 
            this.Controls.Add(this.lstDone);
            this.Controls.Add(this.capInfo);
            this.Name = "Monitor";
            this.Size = new Size(401, 408);
            this.ResumeLayout(false);

        }

        private Caption capInfo;
        private ListBox lstDone;
    }
}
