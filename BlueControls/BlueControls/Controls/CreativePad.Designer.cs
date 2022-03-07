using System.Diagnostics;
using System.Drawing.Printing;

namespace BlueControls.Controls
{
    public partial class CreativePad 
    {
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreativePad));
            this.PrintPreviewDialog1 = new System.Windows.Forms.PrintPreviewDialog();
            this.DruckerDokument = new System.Drawing.Printing.PrintDocument();
            this.PrintDialog1 = new System.Windows.Forms.PrintDialog();
            this.PicsSave = new System.Windows.Forms.SaveFileDialog();
            this.SuspendLayout();
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
            this.PicsSave.Filter = "PNG Portable Network Graphics|*.png|JPG Jpeg Interchange|*.jpg|Bmp Windows Bitmap" +
    "|*.bmp";
            this.PicsSave.Title = "Bitte neuen Dateinamen der Datei wählen.";
            this.PicsSave.FileOk += new System.ComponentModel.CancelEventHandler(this.PicsSave_FileOk);
            // 
            // CreativePad
            // 
            this.Size = new System.Drawing.Size(498, 362);
            this.ResumeLayout(false);
        }
        private System.Windows.Forms.PrintPreviewDialog PrintPreviewDialog1;
        private PrintDocument DruckerDokument;
        private System.Windows.Forms.PrintDialog PrintDialog1;
        public System.Windows.Forms.SaveFileDialog PicsSave;
    }
}
