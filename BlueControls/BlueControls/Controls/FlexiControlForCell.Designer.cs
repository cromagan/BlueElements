
using System.ComponentModel;

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
            this.SuspendLayout();
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
    }
}
