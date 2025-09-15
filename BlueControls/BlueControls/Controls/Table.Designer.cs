using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Enums;
using BlueTable.EventArgs;
using Orientation = BlueBasics.Enums.Orientation;

namespace BlueControls.Controls {
    public partial class TableView {

        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            this.BCB = new ComboBox();
            this.BTB = new TextBox();
            this.SliderX = new Slider();
            this.SliderY = new Slider();
            this.btnEdit = new Button();
            this.btnTextLöschen = new Button();
            this.txbZeilenFilter = new TextBox();
            this.btnAlleFilterAus = new Button();
            this.btnPin = new Button();
            this.btnPinZurück = new Button();
            this.btnÄhnliche = new Button();
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
            this.BCB.NeedTableOfAdditinalSpecialChars += new EventHandler<TableFileGiveBackEventArgs>(this.BB_NeedTableOfAdditinalSpecialChars);
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
            this.BTB.NeedTableOfAdditinalSpecialChars += new EventHandler<TableFileGiveBackEventArgs>(this.BB_NeedTableOfAdditinalSpecialChars);
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
            // btnTextLöschen
            // 
            this.btnTextLöschen.ImageCode = "Kreuz|16";
            this.btnTextLöschen.Location = new Point(144, 8);
            this.btnTextLöschen.Name = "btnTextLöschen";
            this.btnTextLöschen.Size = new Size(24, 24);
            this.btnTextLöschen.TabIndex = 50;
            this.btnTextLöschen.Click += new EventHandler(this.btnTextLöschen_Click);
            // 
            // txbZeilenFilter
            // 
            this.txbZeilenFilter.Cursor = Cursors.IBeam;
            this.txbZeilenFilter.RaiseChangeDelay = 1;
            this.txbZeilenFilter.Location = new Point(8, 8);
            this.txbZeilenFilter.Margin = new Padding(4);
            this.txbZeilenFilter.Name = "txbZeilenFilter";
            this.txbZeilenFilter.Size = new Size(136, 24);
            this.txbZeilenFilter.TabIndex = 51;
            this.txbZeilenFilter.TextChanged += new EventHandler(this.txbZeilenFilter_TextChanged);
            this.txbZeilenFilter.Enter += new EventHandler(this.txbZeilenFilter_Enter);
            // 
            // btnAlleFilterAus
            // 
            this.btnAlleFilterAus.ImageCode = "Trichter|16|||||||||Kreuz";
            this.btnAlleFilterAus.Location = new Point(176, 8);
            this.btnAlleFilterAus.Margin = new Padding(4);
            this.btnAlleFilterAus.Name = "btnAlleFilterAus";
            this.btnAlleFilterAus.Size = new Size(128, 24);
            this.btnAlleFilterAus.TabIndex = 52;
            this.btnAlleFilterAus.Text = "alle Filter aus";
            this.btnAlleFilterAus.Click += new EventHandler(this.btnAlleFilterAus_Click);
            // 
            // btnPin
            // 
            this.btnPin.ImageCode = "Pinnadel|20";
            this.btnPin.Location = new Point(312, 8);
            this.btnPin.Name = "btnPin";
            this.btnPin.QuickInfo = "Angezeigte Zeilen anpinnen";
            this.btnPin.Size = new Size(24, 24);
            this.btnPin.TabIndex = 53;
            this.btnPin.Click += new EventHandler(this.btnPin_Click);
            // 
            // btnPinZurück
            // 
            this.btnPinZurück.ImageCode = "Pinnadel|20|||||||||Kreuz";
            this.btnPinZurück.Location = new Point(336, 8);
            this.btnPinZurück.Name = "btnPinZurück";
            this.btnPinZurück.QuickInfo = "Angepinnte Zeilen zurücksetzen";
            this.btnPinZurück.Size = new Size(24, 24);
            this.btnPinZurück.TabIndex = 54;
            this.btnPinZurück.Click += new EventHandler(this.btnPinZurück_Click);
            // 
            // btnÄhnliche
            // 
            this.btnÄhnliche.ImageCode = "Fernglas|16|||||||||HäkchenDoppelt";
            this.btnÄhnliche.Location = new Point(8, 40);
            this.btnÄhnliche.Margin = new Padding(4);
            this.btnÄhnliche.Name = "btnÄhnliche";
            this.btnÄhnliche.Size = new Size(136, 24);
            this.btnÄhnliche.TabIndex = 55;
            this.btnÄhnliche.Text = "ähnlich";
            this.btnÄhnliche.Visible = false;
            this.btnÄhnliche.Click += new EventHandler(this.btnÄhnliche_Click);
            // 
            // Table
            // 
            this.Controls.Add(this.btnÄhnliche);
            this.Controls.Add(this.btnPinZurück);
            this.Controls.Add(this.btnPin);
            this.Controls.Add(this.btnTextLöschen);
            this.Controls.Add(this.txbZeilenFilter);
            this.Controls.Add(this.btnAlleFilterAus);
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
        private Button btnTextLöschen;
        private TextBox txbZeilenFilter;
        private Button btnAlleFilterAus;
        private Button btnPin;
        private Button btnPinZurück;
        private Button btnÄhnliche;
    }
}