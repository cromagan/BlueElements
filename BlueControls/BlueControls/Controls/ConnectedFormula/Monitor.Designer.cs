using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Orientation = BlueBasics.Enums.Orientation;

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
            this.capInfo = new BlueControls.Controls.Caption();
            this.lstDone = new BlueControls.Controls.ListBox();
            this.SuspendLayout();
            // 
            // capInfo
            // 
            this.capInfo.CausesValidation = false;
            this.capInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.capInfo.Location = new System.Drawing.Point(0, 0);
            this.capInfo.Name = "capInfo";
            this.capInfo.Size = new System.Drawing.Size(401, 24);
            // 
            // lstDone
            // 
            this.lstDone.AddAllowed = BlueControls.Enums.AddType.None;
            this.lstDone.AutoSort = false;
            this.lstDone.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstDone.Location = new System.Drawing.Point(0, 24);
            this.lstDone.Name = "lstDone";
            this.lstDone.Size = new System.Drawing.Size(401, 384);
            this.lstDone.TabIndex = 1;
            // 
            // Monitor
            // 
            this.Controls.Add(this.lstDone);
            this.Controls.Add(this.capInfo);
            this.Name = "Monitor";
            this.Size = new System.Drawing.Size(401, 408);
            this.ResumeLayout(false);

        }

        private Caption capInfo;
        private ListBox lstDone;
    }
}
