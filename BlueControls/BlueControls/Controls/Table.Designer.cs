using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Enums;
using BlueDatabase.EventArgs;
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
            this.BCB = new ComboBox();
            this.BTB = new TextBox();
            this.SliderX = new Slider();
            this.SliderY = new Slider();
            this.btnEdit = new Button();
            this.SuspendLayout();
            // 
            // BCB
            // 
            this.BCB.Cursor = Cursors.IBeam;
            this.BCB.Location = new Point(48, 120);
            this.BCB.Name = "BCB";
            this.BCB.Size = new Size(128, 32);
            this.BCB.TabIndex = 8;
            this.BCB.Verhalten = SteuerelementVerhalten.Steuerelement_Anpassen;
            this.BCB.Visible = false;
            this.BCB.Enter += new EventHandler(this.BB_Enter);
            this.BCB.Esc += new EventHandler(this.BB_ESC);
            this.BCB.Tab += new EventHandler(this.BB_TAB);
            this.BCB.LostFocus += new EventHandler(this.BB_LostFocus);
            this.BCB.NeedDatabaseOfAdditinalSpecialChars += new EventHandler<DatabaseFileGiveBackEventArgs>(this.BB_NeedDatabaseOfAdditinalSpecialChars);
            // 
            // BTB
            // 
            this.BTB.Cursor = Cursors.IBeam;
            this.BTB.Location = new Point(48, 88);
            this.BTB.Name = "BTB";
            this.BTB.Size = new Size(128, 32);
            this.BTB.TabIndex = 7;
            this.BTB.Verhalten = SteuerelementVerhalten.Steuerelement_Anpassen;
            this.BTB.Visible = false;
            this.BTB.Enter += new EventHandler(this.BB_Enter);
            this.BTB.Esc += new EventHandler(this.BB_ESC);
            this.BTB.Tab += new EventHandler(this.BB_TAB);
            this.BTB.LostFocus += new EventHandler(this.BB_LostFocus);
            this.BTB.NeedDatabaseOfAdditinalSpecialChars += new EventHandler<DatabaseFileGiveBackEventArgs>(this.BB_NeedDatabaseOfAdditinalSpecialChars);
            // 
            // SliderX
            // 
            this.SliderX.CausesValidation = false;
            this.SliderX.Dock = DockStyle.Bottom;
            this.SliderX.Location = new Point(0, 370);
            this.SliderX.Name = "SliderX";
            this.SliderX.Size = new Size(581, 18);
            this.SliderX.SmallChange = 16F;
            this.SliderX.ValueChanged += new EventHandler(this.SliderX_ValueChanged);
            // 
            // SliderY
            // 
            this.SliderY.CausesValidation = false;
            this.SliderY.Dock = DockStyle.Right;
            this.SliderY.Location = new Point(581, 0);
            this.SliderY.Name = "SliderY";
            this.SliderY.Orientation = Orientation.Senkrecht;
            this.SliderY.Size = new Size(18, 388);
            this.SliderY.SmallChange = 16F;
            this.SliderY.ValueChanged += new EventHandler(this.SliderY_ValueChanged);
            // 
            // btnEdit
            // 
            this.btnEdit.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            this.btnEdit.ImageCode = "Stift|14";
            this.btnEdit.Location = new Point(550, 338);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new Size(24, 24);
            this.btnEdit.TabIndex = 49;
            this.btnEdit.Visible = false;
            this.btnEdit.Click += new EventHandler(this.btnEdit_Click);
            // 
            // Table
            // 
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.BCB);
            this.Controls.Add(this.BTB);
            this.Controls.Add(this.SliderX);
            this.Controls.Add(this.SliderY);
            this.Name = "Table";
            this.Size = new Size(599, 388);
            this.ResumeLayout(false);

        }
        private ComboBox BCB;
        private TextBox BTB;
        private Slider SliderX;
        private Slider SliderY;
        private Button btnEdit;
    }
}
