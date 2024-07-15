using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Enums;
using Orientation = BlueBasics.Enums.Orientation;

namespace BlueControls.Controls
{
    public partial class Table 
    {

        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.BCB = new BlueControls.Controls.ComboBox();
            this.BTB = new BlueControls.Controls.TextBox();
            this.SliderX = new BlueControls.Controls.Slider();
            this.SliderY = new BlueControls.Controls.Slider();
            this.btnEdit = new BlueControls.Controls.Button();
            this.SuspendLayout();
            // 
            // BCB
            // 
            this.BCB.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.BCB.Location = new System.Drawing.Point(48, 120);
            this.BCB.Name = "BCB";
            this.BCB.Size = new System.Drawing.Size(128, 32);
            this.BCB.TabIndex = 8;
            this.BCB.Verhalten = BlueControls.Enums.SteuerelementVerhalten.Steuerelement_Anpassen;
            this.BCB.Visible = false;
            this.BCB.Enter += new System.EventHandler(this.BB_Enter);
            this.BCB.Esc += new System.EventHandler(this.BB_ESC);
            this.BCB.Tab += new System.EventHandler(this.BB_TAB);
            this.BCB.LostFocus += new System.EventHandler(this.BB_LostFocus);
            // 
            // BTB
            // 
            this.BTB.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.BTB.Location = new System.Drawing.Point(48, 88);
            this.BTB.Name = "BTB";
            this.BTB.Size = new System.Drawing.Size(128, 32);
            this.BTB.TabIndex = 7;
            this.BTB.Verhalten = BlueControls.Enums.SteuerelementVerhalten.Steuerelement_Anpassen;
            this.BTB.Visible = false;
            this.BTB.Enter += new System.EventHandler(this.BB_Enter);
            this.BTB.Esc += new System.EventHandler(this.BB_ESC);
            this.BTB.Tab += new System.EventHandler(this.BB_TAB);
            this.BTB.LostFocus += new System.EventHandler(this.BB_LostFocus);
            // 
            // SliderX
            // 
            this.SliderX.CausesValidation = false;
            this.SliderX.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.SliderX.Location = new System.Drawing.Point(0, 370);
            this.SliderX.Name = "SliderX";
            this.SliderX.Size = new System.Drawing.Size(581, 18);
            this.SliderX.SmallChange = 16F;
            this.SliderX.ValueChanged += new System.EventHandler(this.SliderX_ValueChanged);
            // 
            // SliderY
            // 
            this.SliderY.CausesValidation = false;
            this.SliderY.Dock = System.Windows.Forms.DockStyle.Right;
            this.SliderY.Location = new System.Drawing.Point(581, 0);
            this.SliderY.Name = "SliderY";
            this.SliderY.Orientation = BlueBasics.Enums.Orientation.Senkrecht;
            this.SliderY.Size = new System.Drawing.Size(18, 388);
            this.SliderY.SmallChange = 16F;
            this.SliderY.ValueChanged += new System.EventHandler(this.SliderY_ValueChanged);
            // 
            // btnEdit
            // 
            this.btnEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEdit.ImageCode = "Stift|14";
            this.btnEdit.Location = new System.Drawing.Point(550, 338);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(24, 24);
            this.btnEdit.TabIndex = 49;
            this.btnEdit.Visible = false;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // Table
            // 
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.BCB);
            this.Controls.Add(this.BTB);
            this.Controls.Add(this.SliderX);
            this.Controls.Add(this.SliderY);
            this.Name = "Table";
            this.Size = new System.Drawing.Size(599, 388);
            this.ResumeLayout(false);

        }
        private ComboBox BCB;
        private TextBox BTB;
        private Slider SliderX;
        private Slider SliderY;
        private Button btnEdit;
    }
}
