using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using BlueBasics.Enums;


namespace BlueControls.Controls
{
    public partial class CreativePad : GenericControl

    {


        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            ComponentResourceManager resources = new ComponentResourceManager(typeof(CreativePad));
            this.SliderY = new  Slider();
            this.SliderX = new  Slider();
            this.PrintPreviewDialog1 = new System.Windows.Forms.PrintPreviewDialog();
            this.DruckerDokument = new PrintDocument();
            this.PrintDialog1 = new System.Windows.Forms.PrintDialog();
            this.PicsSave = new System.Windows.Forms.SaveFileDialog();
            this.SuspendLayout();
            // 
            // SliderY
            // 
            this.SliderY.CausesValidation = false;
            this.SliderY.Dock = System.Windows.Forms.DockStyle.Right;
            this.SliderY.Location = new Point(480, 0);
            this.SliderY.Name = "SliderY";
            this.SliderY.Orientation = enOrientation.Senkrecht;
            this.SliderY.Size = new Size(18, 362);
            this.SliderY.ValueChanged += new EventHandler(this.SliderY_ValueChanged);
            // 
            // SliderX
            // 
            this.SliderX.CausesValidation = false;
            this.SliderX.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.SliderX.Location = new Point(0, 344);
            this.SliderX.Name = "SliderX";
            this.SliderX.Size = new Size(480, 18);
            this.SliderX.ValueChanged += new EventHandler(this.SliderX_ValueChanged);
            // 
            // PrintPreviewDialog1
            // 
            this.PrintPreviewDialog1.AutoScrollMargin = new Size(0, 0);
            this.PrintPreviewDialog1.AutoScrollMinSize = new Size(0, 0);
            this.PrintPreviewDialog1.ClientSize = new Size(400, 300);
            this.PrintPreviewDialog1.Document = this.DruckerDokument;
            this.PrintPreviewDialog1.Enabled = true;
            this.PrintPreviewDialog1.Icon = ((Icon)(resources.GetObject("PrintPreviewDialog1.Icon")));
            this.PrintPreviewDialog1.Name = "PrintPreviewDialog1";
            this.PrintPreviewDialog1.Visible = false;
            // 
            // DruckerDokument
            // 
            this.DruckerDokument.PrintPage += new PrintPageEventHandler(this.PrintDocument1_PrintPage);
            // 
            // PrintDialog1
            // 
            this.PrintDialog1.Document = this.DruckerDokument;
            // 
            // PicsSave
            // 
            this.PicsSave.Filter = "PNG Portable Network Graphics|*.png|JPG Jpeg Interchange|*.jpg|BMP Windows Bitmap" +
                                   "|*.bmp";
            this.PicsSave.Title = "Bitte neuen Dateinamen der Datei wählen.";
            this.PicsSave.FileOk += new CancelEventHandler(this.PicsSave_FileOk);
            // 
            // CreativePad
            // 
            this.Controls.Add(this.SliderX);
            this.Controls.Add(this.SliderY);
            this.Name = "CreativePad";
            this.Size = new Size(498, 362);
            this.ResumeLayout(false);

        }
        private Slider SliderY;
        private Slider SliderX;
        private System.Windows.Forms.PrintPreviewDialog PrintPreviewDialog1;
        private PrintDocument DruckerDokument;
        private System.Windows.Forms.PrintDialog PrintDialog1;
        public System.Windows.Forms.SaveFileDialog PicsSave;
    }
}
