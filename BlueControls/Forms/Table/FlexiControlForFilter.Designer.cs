using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueTable.Enums;

namespace BlueControls.Controls{
    partial class FlexiControlForFilter {
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
            this.f.ButtonClicked += new EventHandler(this.F_ButtonClick);
            this.f.ControlAdded += new ControlEventHandler(F_ControlAdded);
            this.f.ControlRemoved += new ControlEventHandler(F_ControlRemoved);
            this.f.ValueChanged += new EventHandler(this.F_ValueChanged);
            // 
            // FlexiControlForFilter
            // 
            this.Controls.Add(this.f);
            this.Name = "FlexiControlForFilter";
            this.ResumeLayout(false);

        }
        #endregion

        private FlexiControl f;
    }
}
