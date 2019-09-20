using System.Diagnostics;
using System.Drawing.Printing;


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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreativePad));
            this.SliderY = new BlueControls.Controls.Slider();
            this.SliderX = new BlueControls.Controls.Slider();
            this.PrintPreviewDialog1 = new System.Windows.Forms.PrintPreviewDialog();
            this.DruckerDokument = new System.Drawing.Printing.PrintDocument();
            this.PrintDialog1 = new System.Windows.Forms.PrintDialog();
            this.PicsSave = new System.Windows.Forms.SaveFileDialog();
            this.SuspendLayout();
            // 
            // SliderY
            // 
            this.SliderY.CausesValidation = false;
            this.SliderY.Dock = System.Windows.Forms.DockStyle.Right;
            this.SliderY.Location = new System.Drawing.Point(480, 0);
            this.SliderY.Name = "SliderY";
            this.SliderY.Orientation = BlueBasics.Enums.enOrientation.Senkrecht;
            this.SliderY.Size = new System.Drawing.Size(18, 362);
            this.SliderY.ValueChanged += new System.EventHandler(this.SliderY_ValueChanged);
            // 
            // SliderX
            // 
            this.SliderX.CausesValidation = false;
            this.SliderX.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.SliderX.Location = new System.Drawing.Point(0, 344);
            this.SliderX.Name = "SliderX";
            this.SliderX.Size = new System.Drawing.Size(480, 18);
            this.SliderX.ValueChanged += new System.EventHandler(this.SliderX_ValueChanged);
            // 
            // PrintPreviewDialog1
            // 
            this.PrintPreviewDialog1.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.PrintPreviewDialog1.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.PrintPreviewDialog1.ClientSize = new System.Drawing.Size(400, 300);
            this.PrintPreviewDialog1.Document = this.DruckerDokument;
            this.PrintPreviewDialog1.Enabled = true;
            this.PrintPreviewDialog1.Icon = ((System.Drawing.Icon)(resources.GetObject("PrintPreviewDialog1.Icon")));
            this.PrintPreviewDialog1.Name = "PrintPreviewDialog1";
            this.PrintPreviewDialog1.Visible = false;
            // 
            // DruckerDokument
            // 
            this.DruckerDokument.BeginPrint += new System.Drawing.Printing.PrintEventHandler(this.DruckerDokument_BeginPrint);
            this.DruckerDokument.EndPrint += new System.Drawing.Printing.PrintEventHandler(this.DruckerDokument_EndPrint);
            this.DruckerDokument.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.DruckerDokument_PrintPage);
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
            this.PicsSave.FileOk += new System.ComponentModel.CancelEventHandler(this.PicsSave_FileOk);
            // 
            // CreativePad
            // 
            this.Controls.Add(this.SliderX);
            this.Controls.Add(this.SliderY);
            this.Size = new System.Drawing.Size(498, 362);
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
