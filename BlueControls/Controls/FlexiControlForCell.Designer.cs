using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueTable.Enums;

namespace BlueControls.Controls {
    partial class FlexiControlForCell {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private IContainer components = null;


        #region Vom Komponenten-Designer generierter Code
        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent() {
            this.f = new FlexiControl();
            this.SuspendLayout();
            // 
            // f
            // 
            this.f.Dock = DockStyle.Fill;
            this.f.EditType = EditTypeFormula.Line;
            this.f.Location = new Point(0, 0);
            this.f.Name = "f";
            this.f.Size = new Size(150, 150);
            this.f.TabIndex = 0;
            this.f.ControlAdded += new ControlEventHandler(this.F_ControlAdded);
            this.f.ControlRemoved += new ControlEventHandler(this.F_ControlRemoved);
            this.f.EnabledChanged += new EventHandler(this.F_EnabledChanged);
            this.f.VisibleChanged += new EventHandler(this.F_VisibleChanged);
            this.f.ValueChanged += new EventHandler(this.F_ValueChanged);
            // 
            // FlexiControlForCell
            // 
            this.Controls.Add(this.f);
            this.Name = "FlexiControlForCell";
            this.ResumeLayout(false);

        }



       #endregion

        private FlexiControl f;
    }
}
