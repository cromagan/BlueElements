using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Orientation = BlueBasics.Enums.Orientation;

namespace BlueControls.Controls
{
    partial class ZoomPad
    {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private IContainer components = null;
        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code
        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.SliderX = new BlueControls.Controls.Slider();
            this.SliderY = new BlueControls.Controls.Slider();
            this.SuspendLayout();
            // 
            // SliderX
            // 
            this.SliderX.CausesValidation = false;
            this.SliderX.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.SliderX.Location = new System.Drawing.Point(0, 221);
            this.SliderX.Name = "SliderX";
            this.SliderX.Size = new System.Drawing.Size(365, 18);
            this.SliderX.Visible = false;
            this.SliderX.ValueChanged += new System.EventHandler(this.SliderX_ValueChanged);
            // 
            // SliderY
            // 
            this.SliderY.CausesValidation = false;
            this.SliderY.Dock = System.Windows.Forms.DockStyle.Right;
            this.SliderY.Location = new System.Drawing.Point(365, 0);
            this.SliderY.Name = "SliderY";
            this.SliderY.Orientation = BlueBasics.Enums.Orientation.Senkrecht;
            this.SliderY.Size = new System.Drawing.Size(18, 239);
            this.SliderY.Visible = false;
            this.SliderY.ValueChanged += new System.EventHandler(this.SliderY_ValueChanged);
            // 
            // ZoomPad
            // 
            this.Controls.Add(this.SliderX);
            this.Controls.Add(this.SliderY);
            this.Name = "ZoomPad";
            this.Size = new System.Drawing.Size(383, 239);
            this.ResumeLayout(false);

        }
        #endregion

        private Slider SliderX;
        private Slider SliderY;
    }
}
