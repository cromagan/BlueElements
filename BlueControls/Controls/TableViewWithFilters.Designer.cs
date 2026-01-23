using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Enums;
using BlueTable.EventArgs;
using Orientation = BlueBasics.Enums.Orientation;

namespace BlueControls.Controls {
    public partial class TableViewWithFilters {

        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            this.btnTextLöschen = new BlueControls.Controls.Button();
            this.txbZeilenFilter = new BlueControls.Controls.TextBox();
            this.btnAlleFilterAus = new BlueControls.Controls.Button();
            this.btnPin = new BlueControls.Controls.Button();
            this.btnPinZurück = new BlueControls.Controls.Button();
            this.btnÄhnliche = new BlueControls.Controls.Button();
            this.TableInternal = new BlueControls.Controls.TableView();
            this.grpBorder = new BlueControls.Controls.GroupBox();
            this.grpBorder.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnTextLöschen
            // 
            this.btnTextLöschen.ImageCode = "Kreuz|16";
            this.btnTextLöschen.Location = new System.Drawing.Point(144, 8);
            this.btnTextLöschen.Name = "btnTextLöschen";
            this.btnTextLöschen.Size = new System.Drawing.Size(24, 24);
            this.btnTextLöschen.TabIndex = 50;
            this.btnTextLöschen.Click += new System.EventHandler(this.btnTextLöschen_Click);
            // 
            // txbZeilenFilter
            // 
            this.txbZeilenFilter.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbZeilenFilter.Location = new System.Drawing.Point(8, 8);
            this.txbZeilenFilter.Margin = new System.Windows.Forms.Padding(4);
            this.txbZeilenFilter.Name = "txbZeilenFilter";
            this.txbZeilenFilter.RaiseChangeDelay = 1;
            this.txbZeilenFilter.Size = new System.Drawing.Size(136, 24);
            this.txbZeilenFilter.TabIndex = 51;
            this.txbZeilenFilter.Enter += new System.EventHandler(this.txbZeilenFilter_Enter);
            this.txbZeilenFilter.TextChanged += new System.EventHandler(this.txbZeilenFilter_TextChanged);
            // 
            // btnAlleFilterAus
            // 
            this.btnAlleFilterAus.ImageCode = "Trichter|16|||||||||Kreuz";
            this.btnAlleFilterAus.Location = new System.Drawing.Point(176, 8);
            this.btnAlleFilterAus.Margin = new System.Windows.Forms.Padding(4);
            this.btnAlleFilterAus.Name = "btnAlleFilterAus";
            this.btnAlleFilterAus.Size = new System.Drawing.Size(128, 24);
            this.btnAlleFilterAus.TabIndex = 52;
            this.btnAlleFilterAus.Text = "alle Filter aus";
            this.btnAlleFilterAus.Click += new System.EventHandler(this.btnAlleFilterAus_Click);
            // 
            // btnPin
            // 
            this.btnPin.ImageCode = "Pinnadel|20";
            this.btnPin.Location = new System.Drawing.Point(312, 8);
            this.btnPin.Name = "btnPin";
            this.btnPin.QuickInfo = "Angezeigte Zeilen anpinnen";
            this.btnPin.Size = new System.Drawing.Size(24, 24);
            this.btnPin.TabIndex = 53;
            this.btnPin.Click += new System.EventHandler(this.btnPin_Click);
            // 
            // btnPinZurück
            // 
            this.btnPinZurück.ImageCode = "Pinnadel|20|||||||||Kreuz";
            this.btnPinZurück.Location = new System.Drawing.Point(336, 8);
            this.btnPinZurück.Name = "btnPinZurück";
            this.btnPinZurück.QuickInfo = "Angepinnte Zeilen zurücksetzen";
            this.btnPinZurück.Size = new System.Drawing.Size(24, 24);
            this.btnPinZurück.TabIndex = 54;
            this.btnPinZurück.Click += new System.EventHandler(this.btnPinZurück_Click);
            // 
            // btnÄhnliche
            // 
            this.btnÄhnliche.ImageCode = "Fernglas|16|||||||||HäkchenDoppelt";
            this.btnÄhnliche.Location = new System.Drawing.Point(8, 40);
            this.btnÄhnliche.Margin = new System.Windows.Forms.Padding(4);
            this.btnÄhnliche.Name = "btnÄhnliche";
            this.btnÄhnliche.Size = new System.Drawing.Size(136, 24);
            this.btnÄhnliche.TabIndex = 55;
            this.btnÄhnliche.Text = "ähnlich";
            this.btnÄhnliche.Visible = false;
            this.btnÄhnliche.Click += new System.EventHandler(this.btnÄhnliche_Click);
            // 
            // TableInternal
            // 
            this.TableInternal.Location = new System.Drawing.Point(8, 80);
            this.TableInternal.Name = "TableInternal";
            this.TableInternal.Size = new System.Drawing.Size(656, 344);
            this.TableInternal.TabIndex = 0;
            // 
            // grpBorder
            // 
            this.grpBorder.Controls.Add(this.TableInternal);
            this.grpBorder.Controls.Add(this.btnÄhnliche);
            this.grpBorder.Controls.Add(this.txbZeilenFilter);
            this.grpBorder.Controls.Add(this.btnPinZurück);
            this.grpBorder.Controls.Add(this.btnAlleFilterAus);
            this.grpBorder.Controls.Add(this.btnPin);
            this.grpBorder.Controls.Add(this.btnTextLöschen);
            this.grpBorder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpBorder.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.Nothing;
            this.grpBorder.Location = new System.Drawing.Point(0, 0);
            this.grpBorder.Name = "grpBorder";
            this.grpBorder.Size = new System.Drawing.Size(684, 455);
            this.grpBorder.TabIndex = 57;
            this.grpBorder.TabStop = false;
            this.grpBorder.Text = "groupBox1";
            // 
            // TableViewWithFilters
            // 
            this.Controls.Add(this.grpBorder);
            this.Name = "TableViewWithFilters";
            this.Size = new System.Drawing.Size(684, 455);
            this.grpBorder.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private Button btnTextLöschen;
        private TextBox txbZeilenFilter;
        private Button btnAlleFilterAus;
        private Button btnPin;
        private Button btnPinZurück;
        private Button btnÄhnliche;
        private TableView TableInternal;
        private GroupBox grpBorder;
    }
}