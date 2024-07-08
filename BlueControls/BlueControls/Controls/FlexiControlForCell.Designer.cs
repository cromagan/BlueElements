
using System.ComponentModel;
using System.Windows.Forms;

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
            this.Marker = new System.ComponentModel.BackgroundWorker();
            this.f = new BlueControls.Controls.FlexiControl();
            this.SuspendLayout();
            // 
            // Marker
            // 
            this.Marker.WorkerReportsProgress = true;
            this.Marker.WorkerSupportsCancellation = true;
            this.Marker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.Marker_DoWork);
            this.Marker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.Marker_ProgressChanged);
            this.Marker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.Marker_RunWorkerCompleted);
            // 
            // f
            // 
            this.f.Dock = System.Windows.Forms.DockStyle.Fill;
            this.f.EditType = BlueDatabase.Enums.EditTypeFormula.Line;
            this.f.Location = new System.Drawing.Point(0, 0);
            this.f.Name = "f";
            this.f.Size = new System.Drawing.Size(150, 150);
            this.f.TabIndex = 0;
            this.f.ControlAdded += new ControlEventHandler(this.F_ControlAdded);
            this.f.ControlRemoved += new ControlEventHandler(this.F_ControlRemoved);
            this.f.EnabledChanged += new System.EventHandler(this.F_EnabledChanged);
            this.f.VisibleChanged += new System.EventHandler(this.F_VisibleChanged);
            this.f.ValueChanged += new System.EventHandler(this.F_ValueChanged);
            // 
            // FlexiControlForCell
            // 
            this.Controls.Add(this.f);
            this.Name = "FlexiControlForCell";
            this.ResumeLayout(false);

        }



       #endregion

        private BackgroundWorker Marker;
        private FlexiControl f;
    }
}
