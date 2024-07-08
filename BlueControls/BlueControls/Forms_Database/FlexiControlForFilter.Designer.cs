
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
        private void InitializeComponent() {
            this.f = new BlueControls.Controls.FlexiControl();
            this.SuspendLayout();
            // 
            // f
            // 
            this.f.Dock = System.Windows.Forms.DockStyle.Fill;
            this.f.EditType = BlueDatabase.Enums.EditTypeFormula.Line;
            this.f.Location = new System.Drawing.Point(0, 0);
            this.f.Name = "f";
            this.f.Size = new System.Drawing.Size(150, 150);
            this.f.TabIndex = 0;
            this.f.ButtonClicked += new System.EventHandler(this.F_ButtonClick);
            this.f.ValueChanged += new System.EventHandler(this.F_ValueChanged);
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
