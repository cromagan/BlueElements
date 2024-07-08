﻿
    using System.ComponentModel;
    using System.Windows.Forms;

    namespace BlueControls.Controls
{
    partial class FlexiControlForFilter
    {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private IContainer components = null;

        #region Vom Komponenten-Designer generierter Code
        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.f = new FlexiControl();
            this.SuspendLayout();
            // 
            // f
            // 
            this.f.Dock = DockStyle.Fill;
            this.f.ValueChanged += F_ValueChanged;
            this.f.ButtonClicked += F_ButtonClick;
            // 
            // FlexiControlForCell
            // 
            this.Name = "FlexiControlForCell";
            this.ResumeLayout(false);
        }
        #endregion
        private FlexiControl f;
    }
}
