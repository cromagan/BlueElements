using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(CreativePad));
            this.PrintPreviewDialog1 = new PrintPreviewDialog();
            this.DruckerDokument = new PrintDocument();
            this.PrintDialog1 = new PrintDialog();
            this.PicsSave = new SaveFileDialog();
            this.SuspendLayout();
            // 
            // PrintPreviewDialog1
            // 
            this.PrintPreviewDialog1.AutoScrollMargin = new Size(0, 0);
            this.PrintPreviewDialog1.AutoScrollMinSize = new Size(0, 0);
            this.PrintPreviewDialog1.ClientSize = new Size(400, 300);
            this.PrintPreviewDialog1.Document = this.DruckerDokument;
            this.PrintPreviewDialog1.Enabled = true;
            this.PrintPreviewDialog1.Name = "PrintPreviewDialog1";
            this.PrintPreviewDialog1.Visible = false;
            // 
            // DruckerDokument
            // 
            this.DruckerDokument.BeginPrint += new PrintEventHandler(this.DruckerDokument_BeginPrint);
            this.DruckerDokument.PrintPage += new PrintPageEventHandler(this.DruckerDokument_PrintPage);
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
            this.PicsSave.FileOk += new CancelEventHandler(this.PicsSave_FileOk);
            // 
            // CreativePad
            // 
            this.Size = new Size(498, 362);
            this.ResumeLayout(false);
        }
        private PrintPreviewDialog PrintPreviewDialog1;
        private PrintDocument DruckerDokument;
        private PrintDialog PrintDialog1;
        public SaveFileDialog PicsSave;
    }
}
