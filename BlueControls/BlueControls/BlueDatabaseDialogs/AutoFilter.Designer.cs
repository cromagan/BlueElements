using BlueControls.Controls;
using BlueControls.Enums;
using System.ComponentModel;
using System.Diagnostics;

namespace BlueControls.BlueDatabaseDialogs
{


    public partial class AutoFilter
    {
        //Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && components != null)
                {
                    components.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        //Wird vom Windows Form-Designer benötigt.
        private IContainer components;

        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AutoFilter));
            this.txbEingabe = new BlueControls.Controls.TextBox();
            this.Timer1x = new System.Windows.Forms.Timer(this.components);
            this.Line = new BlueControls.Controls.Line();
            this.lsbFilterItems = new BlueControls.Controls.ListBox();
            this.lsbStandardFilter = new BlueControls.Controls.ListBox();
            this.capWas = new BlueControls.Controls.Caption();
            this.butFertig = new BlueControls.Controls.Button();
            this.capInfo = new BlueControls.Controls.Caption();
            this.SuspendLayout();
            // 
            // txbEingabe
            // 
            this.txbEingabe.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txbEingabe.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbEingabe.Location = new System.Drawing.Point(8, 100);
            this.txbEingabe.Name = "txbEingabe";
            this.txbEingabe.Size = new System.Drawing.Size(150, 24);
            this.txbEingabe.TabIndex = 2;
            this.txbEingabe.Enter += new System.EventHandler(this.TXTBox_Enter);
            this.txbEingabe.LostFocus += new System.EventHandler(this.Something_LostFocus);
            // 
            // Timer1x
            // 
            this.Timer1x.Enabled = true;
            this.Timer1x.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // Line
            // 
            this.Line.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Line.CausesValidation = false;
            this.Line.Location = new System.Drawing.Point(8, 132);
            this.Line.Name = "Line";
            this.Line.Size = new System.Drawing.Size(152, 2);
            this.Line.Text = "BlueLine1";
            // 
            // lsbFilterItems
            // 
            this.lsbFilterItems.AddAllowed = BlueControls.Enums.enAddType.None;
            this.lsbFilterItems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lsbFilterItems.Appearance = BlueControls.Enums.enBlueListBoxAppearance.Autofilter;
            this.lsbFilterItems.LastFilePath = null;
            this.lsbFilterItems.Location = new System.Drawing.Point(8, 140);
            this.lsbFilterItems.Name = "lsbFilterItems";
            this.lsbFilterItems.Size = new System.Drawing.Size(150, 12);
            this.lsbFilterItems.TabIndex = 5;
            this.lsbFilterItems.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.FiltItems_ItemClicked);
            this.lsbFilterItems.LostFocus += new System.EventHandler(this.Something_LostFocus);
            // 
            // lsbStandardFilter
            // 
            this.lsbStandardFilter.AddAllowed = BlueControls.Enums.enAddType.None;
            this.lsbStandardFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lsbStandardFilter.Appearance = BlueControls.Enums.enBlueListBoxAppearance.Autofilter;
            this.lsbStandardFilter.LastFilePath = null;
            this.lsbStandardFilter.Location = new System.Drawing.Point(8, 8);
            this.lsbStandardFilter.Name = "lsbStandardFilter";
            this.lsbStandardFilter.Size = new System.Drawing.Size(150, 72);
            this.lsbStandardFilter.TabIndex = 1;
            this.lsbStandardFilter.Text = "Standard";
            this.lsbStandardFilter.ItemClicked += new System.EventHandler<BlueControls.EventArgs.BasicListItemEventArgs>(this.sFilter_ItemClicked);
            this.lsbStandardFilter.LostFocus += new System.EventHandler(this.Something_LostFocus);
            // 
            // capWas
            // 
            this.capWas.CausesValidation = false;
            this.capWas.Location = new System.Drawing.Point(8, 84);
            this.capWas.Name = "capWas";
            this.capWas.Size = new System.Drawing.Size(112, 16);
            this.capWas.Text = "...oder Text:";
            // 
            // butFertig
            // 
            this.butFertig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butFertig.ImageCode = "Häkchen|16";
            this.butFertig.Location = new System.Drawing.Point(72, 96);
            this.butFertig.Name = "butFertig";
            this.butFertig.Size = new System.Drawing.Size(88, 32);
            this.butFertig.TabIndex = 6;
            this.butFertig.Text = "fertig";
            this.butFertig.Visible = false;
            this.butFertig.Click += new System.EventHandler(this.butFertig_Click);
            // 
            // capInfo
            // 
            this.capInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.capInfo.CausesValidation = false;
            this.capInfo.Location = new System.Drawing.Point(8, 8);
            this.capInfo.Name = "capInfo";
            this.capInfo.Size = new System.Drawing.Size(152, 72);
            this.capInfo.TextAnzeigeVerhalten = enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            this.capInfo.Visible = false;
            // 
            // AutoFilter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.Controls.Add(this.capInfo);
            this.Controls.Add(this.butFertig);
            this.Controls.Add(this.capWas);
            this.Controls.Add(this.lsbStandardFilter);
            this.Controls.Add(this.lsbFilterItems);
            this.Controls.Add(this.Line);
            this.Controls.Add(this.txbEingabe);
            this.Name = "AutoFilter";
            this.Size = new System.Drawing.Size(165, 159);
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        internal TextBox txbEingabe;
        internal System.Windows.Forms.Timer Timer1x;
        internal Line Line;
        internal ListBox lsbFilterItems;
        internal ListBox lsbStandardFilter;
        internal Caption capWas;
        private Button butFertig;
        private Caption capInfo;
    }
}
