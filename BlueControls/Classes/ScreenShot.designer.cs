using BlueControls.Controls;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls {
    public sealed partial class ScreenShot  {
        private ZoomPic zoomPic;

        //Das Formular überschreibt den Deletevorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing) {
            if (disposing) {
                zoomPic?.Dispose();
            }
            base.Dispose(disposing);
        }

        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            this.zoomPic = new BlueControls.Controls.ZoomPic();
            this.SuspendLayout();
            // 
            // zoomPic
            // 
            this.zoomPic.Bmp = null;
            this.zoomPic.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zoomPic.Location = new System.Drawing.Point(0, 0);
            this.zoomPic.Name = "zoomPic";
            this.zoomPic.SlideAndZoomAllowed = false;
            this.zoomPic.Size = new System.Drawing.Size(508, 639);
            this.zoomPic.TabIndex = 0;
            this.zoomPic.ImageMouseDown += new System.EventHandler<EventArgs.TrimmedCanvasMouseEventArgs>(this.ZoomPic_ImageMouseDown);
            this.zoomPic.ImageMouseUp += new System.EventHandler<EventArgs.TrimmedCanvasMouseEventArgsDownAndCurrentEventArgs>(this.ZoomPic_ImageMouseUp);
            // 
            // ScreenShot
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new Size(292, 272);
            this.Controls.Add(this.zoomPic);
            this.DoubleBuffered = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Name = "ScreenShot";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);

        }
    }
}