using System;
using System.Diagnostics;
using System.Drawing;
using BlueBasics.Enums;
using BlueControls.Enums;

namespace BlueControls.Controls
{
    public partial class Table 
    {
        //UserControl überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    Database = null; // Wichtig (nicht _Database) um Events zu lösen
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.BCB = new ComboBox();
            this.BTB = new TextBox();
            this.SliderX = new Slider();
            this.SliderY = new Slider();
            this.ColDia = new System.Windows.Forms.ColorDialog();
            this.SuspendLayout();
            // 
            // BCB
            // 
            this.BCB.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.BCB.Location = new Point(48, 160);
            this.BCB.Name = "BCB";
            this.BCB.Size = new Size(120, 24);
            this.BCB.TabIndex = 8;
            this.BCB.Verhalten = enSteuerelementVerhalten.Steuerelement_Anpassen;
            this.BCB.Visible = false;
            this.BCB.Enter += new EventHandler(this.BB_Enter);
            this.BCB.ESC += new EventHandler(this.BB_ESC);
            this.BCB.TAB += new EventHandler(this.BB_TAB);
            this.BCB.LostFocus += new EventHandler(this.BB_LostFocus);
            // 
            // BTB
            // 
            this.BTB.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.BTB.Location = new Point(48, 136);
            this.BTB.Name = "BTB";
            this.BTB.Size = new Size(120, 24);
            this.BTB.TabIndex = 7;
            this.BTB.Verhalten = enSteuerelementVerhalten.Steuerelement_Anpassen;
            this.BTB.Visible = false;
            this.BTB.Enter += new EventHandler(this.BB_Enter);
            this.BTB.ESC += new EventHandler(this.BB_ESC);
            this.BTB.TAB += new EventHandler(this.BB_TAB);
            this.BTB.LostFocus += new EventHandler(this.BB_LostFocus);
            this.BTB.NeedDatabaseOfAdditinalSpecialChars += BTB_NeedDatabaseOfAdditinalSpecialChars;
            // 
            // SliderX
            // 
            this.SliderX.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.SliderX.Enabled = false;
            this.SliderX.Location = new Point(0, 414);
            this.SliderX.Name = "SliderX";
            this.SliderX.Size = new Size(1107, 18);
            this.SliderX.SmallChange = 16D;
            this.SliderX.ValueChanged += new EventHandler(this.SliderX_ValueChanged);
            // 
            // SliderY
            // 
            this.SliderY.Dock = System.Windows.Forms.DockStyle.Right;
            this.SliderY.Enabled = false;
            this.SliderY.Location = new Point(1107, 0);
            this.SliderY.Name = "SliderY";
            this.SliderY.Orientation = enOrientation.Senkrecht;
            this.SliderY.Size = new Size(18, 432);
            this.SliderY.SmallChange = 16D;
            this.SliderY.ValueChanged += new EventHandler(this.SliderY_ValueChanged);
            // 
            // Table
            // 
            this.Controls.Add(this.BCB);
            this.Controls.Add(this.BTB);
            this.Controls.Add(this.SliderX);
            this.Controls.Add(this.SliderY);
            this.Name = "Table";
            this.Size = new Size(1125, 432);
            this.ResumeLayout(false);
        }
        private ComboBox BCB;
        private TextBox BTB;
        private Slider SliderX;
        private Slider SliderY;
        private System.Windows.Forms.ColorDialog ColDia;
    }
}
