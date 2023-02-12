using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using Button = BlueControls.Controls.Button;
using ListBox = BlueControls.Controls.ListBox;
using TextBox = BlueControls.Controls.TextBox;
using Timer = System.Windows.Forms.Timer;

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
                    components?.Dispose();
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
            this.components = new Container();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(AutoFilter));
            this.txbEingabe = new TextBox();
            this.Timer1x = new Timer(this.components);
            this.Line = new Line();
            this.lsbFilterItems = new ListBox();
            this.lsbStandardFilter = new ListBox();
            this.capWas = new Caption();
            this.butFertig = new Button();
            this.capInfo = new Caption();
            this.SuspendLayout();
            // 
            // txbEingabe
            // 
            this.txbEingabe.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                      | AnchorStyles.Right)));
            this.txbEingabe.Cursor = Cursors.IBeam;
            this.txbEingabe.Location = new Point(8, 100);
            this.txbEingabe.Name = "txbEingabe";
            this.txbEingabe.Size = new Size(150, 24);
            this.txbEingabe.TabIndex = 2;
            this.txbEingabe.Enter += new EventHandler(this.TXTBox_Enter);
            this.txbEingabe.LostFocus += new EventHandler(this.Something_LostFocus);
            // 
            // Timer1x
            // 
            this.Timer1x.Enabled = true;
            this.Timer1x.Tick += new EventHandler(this.Timer1_Tick);
            // 
            // Line
            // 
            this.Line.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                | AnchorStyles.Right)));
            this.Line.CausesValidation = false;
            this.Line.Location = new Point(8, 132);
            this.Line.Name = "Line";
            this.Line.Size = new Size(152, 2);
            this.Line.Text = "BlueLine1";
            // 
            // lsbFilterItems
            // 
            this.lsbFilterItems.AddAllowed = AddType.None;
            this.lsbFilterItems.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                                                           | AnchorStyles.Left) 
                                                          | AnchorStyles.Right)));
            this.lsbFilterItems.Appearance = BlueListBoxAppearance.Autofilter;
            this.lsbFilterItems.Location = new Point(8, 140);
            this.lsbFilterItems.Name = "lsbFilterItems";
            this.lsbFilterItems.Size = new Size(150, 12);
            this.lsbFilterItems.TabIndex = 5;
            this.lsbFilterItems.ItemClicked += new EventHandler<BasicListItemEventArgs>(this.FiltItems_ItemClicked);
            this.lsbFilterItems.LostFocus += new EventHandler(this.Something_LostFocus);
            // 
            // lsbStandardFilter
            // 
            this.lsbStandardFilter.AddAllowed = AddType.None;
            this.lsbStandardFilter.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                             | AnchorStyles.Right)));
            this.lsbStandardFilter.Appearance = BlueListBoxAppearance.Autofilter;
            this.lsbStandardFilter.Location = new Point(8, 8);
            this.lsbStandardFilter.Name = "lsbStandardFilter";
            this.lsbStandardFilter.Size = new Size(150, 72);
            this.lsbStandardFilter.TabIndex = 1;
            this.lsbStandardFilter.Text = "Standard";
            this.lsbStandardFilter.ItemClicked += new EventHandler<BasicListItemEventArgs>(this.sFilter_ItemClicked);
            this.lsbStandardFilter.LostFocus += new EventHandler(this.Something_LostFocus);
            // 
            // capWas
            // 
            this.capWas.CausesValidation = false;
            this.capWas.Location = new Point(8, 84);
            this.capWas.Name = "capWas";
            this.capWas.Size = new Size(112, 16);
            this.capWas.Text = "...oder Text:";
            // 
            // butFertig
            // 
            this.butFertig.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.butFertig.ImageCode = "Häkchen|16";
            this.butFertig.Location = new Point(72, 96);
            this.butFertig.Name = "butFertig";
            this.butFertig.Size = new Size(88, 32);
            this.butFertig.TabIndex = 6;
            this.butFertig.Text = "fertig";
            this.butFertig.Visible = false;
            this.butFertig.Click += new EventHandler(this.butFertig_Click);
            // 
            // capInfo
            // 
            this.capInfo.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                   | AnchorStyles.Right)));
            this.capInfo.CausesValidation = false;
            this.capInfo.Location = new Point(8, 8);
            this.capInfo.Name = "capInfo";
            this.capInfo.Size = new Size(152, 72);
            this.capInfo.TextAnzeigeVerhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
            this.capInfo.Visible = false;
            // 
            // AutoFilter
            // 
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(165, 159);
            this.Controls.Add(this.capInfo);
            this.Controls.Add(this.butFertig);
            this.Controls.Add(this.capWas);
            this.Controls.Add(this.lsbStandardFilter);
            this.Controls.Add(this.lsbFilterItems);
            this.Controls.Add(this.Line);
            this.Controls.Add(this.txbEingabe);
            this.Name = "AutoFilter";
            this.ResumeLayout(false);

        }
        internal TextBox txbEingabe;
        internal Timer Timer1x;
        internal Line Line;
        internal ListBox lsbFilterItems;
        internal ListBox lsbStandardFilter;
        internal Caption capWas;
        private Button butFertig;
        private Caption capInfo;
    }
}
