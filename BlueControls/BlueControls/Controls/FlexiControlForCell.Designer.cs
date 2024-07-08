
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
            this.Marker = new BackgroundWorker();
            this.f = new FlexiControl();
            this.SuspendLayout();
            // 
            // f
            // 
            this.f.Dock = DockStyle.Fill;
            this.f.ControlAdded += F_ControlAdded;
            this.f.ControlRemoved += F_ControlRemoved;
            this.f.ValueChanged += F_ValueChanged;
            this.f.EnabledChanged += F_EnabledChanged;
            this.f.VisibleChanged += F_VisibleChanged;
            // 
            // Marker
            // 
            this.Marker.WorkerReportsProgress = true;
            this.Marker.WorkerSupportsCancellation = true;
            this.Marker.DoWork += new DoWorkEventHandler(this.Marker_DoWork);
            this.Marker.ProgressChanged += new ProgressChangedEventHandler(this.Marker_ProgressChanged);
            this.Marker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.Marker_RunWorkerCompleted);
            this.ResumeLayout(false);
        }



       #endregion

        private BackgroundWorker Marker;
        private FlexiControl f;
    }
}
