using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;

namespace BlueControls {
    public sealed partial class ScreenShot  {
        private ZoomPicWithPoints zoomPic;

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
            this.zoomPic = new BlueControls.Controls.ZoomPicWithPoints();
            this.SuspendLayout();
            // 
            // zoomPic
            // 
            this.zoomPic.Bmp = null;
            this.zoomPic.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zoomPic.Location = new System.Drawing.Point(0, 0);
            this.zoomPic.Mittellinie = BlueBasics.Enums.Orientation.Ohne;
            this.zoomPic.Name = "zoomPic";
            this.zoomPic.ScreenshotMode = true;
            this.zoomPic.Size = new System.Drawing.Size(508, 639);
            this.zoomPic.TabIndex = 0;
            this.zoomPic.MouseDown += new System.Windows.Forms.MouseEventHandler(this.zoomPic_MouseDown);
            this.zoomPic.MouseUp += new System.Windows.Forms.MouseEventHandler(this.zoomPic_MouseUp);
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