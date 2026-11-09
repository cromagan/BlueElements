// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;
using ListBox = BlueControls.Controls.ListBox;
using TextBox = BlueControls.Controls.TextBox;

namespace BluePaint
{
    public partial class StampTool : Tool
    {
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.grpSymbol = new GroupBox();
            this.txbSuche = new TextBox();
            this.btnFilterDel = new Button();
            this.lstSymbole = new ListBox();
            this.grpGröße = new GroupBox();
            this.capSize = new Caption();
            this.sldSize = new Slider();
            this.grpSymbol.SuspendLayout();
            this.grpGröße.SuspendLayout();
            this.SuspendLayout();
            //
            // grpSymbol
            //
            this.grpSymbol.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
                | AnchorStyles.Right)));
            this.grpSymbol.Controls.Add(this.txbSuche);
            this.grpSymbol.Controls.Add(this.btnFilterDel);
            this.grpSymbol.Controls.Add(this.lstSymbole);
            this.grpSymbol.Location = new Point(8, 8);
            this.grpSymbol.Name = "grpSymbol";
            this.grpSymbol.Size = new Size(252, 148);
            this.grpSymbol.TabIndex = 0;
            this.grpSymbol.TabStop = false;
            this.grpSymbol.Text = "Symbol";
            //
            // txbSuche
            //
            this.txbSuche.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
                | AnchorStyles.Right)));
            this.txbSuche.Location = new Point(8, 16);
            this.txbSuche.Name = "txbSuche";
            this.txbSuche.Size = new Size(196, 24);
            this.txbSuche.TabIndex = 0;
            this.txbSuche.TextChanged += new EventHandler(this.TxbSuche_TextChanged);
            //
            // btnFilterDel
            //
            this.btnFilterDel.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.btnFilterDel.Enabled = false;
            this.btnFilterDel.ImageCode = "Trichter|16||1";
            this.btnFilterDel.Location = new Point(204, 16);
            this.btnFilterDel.Name = "btnFilterDel";
            this.btnFilterDel.QuickInfo = "Filter löschen";
            this.btnFilterDel.Size = new Size(40, 24);
            this.btnFilterDel.TabIndex = 2;
            this.btnFilterDel.Click += new EventHandler(this.BtnFilterDel_Click);
            //
            // lstSymbole
            //
            this.lstSymbole.AddAllowed = AddType.None;
            this.lstSymbole.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
                | AnchorStyles.Right)));
            this.lstSymbole.AutoSort = true;
            this.lstSymbole.Location = new Point(8, 44);
            this.lstSymbole.Name = "lstSymbole";
            this.lstSymbole.Size = new Size(236, 92);
            this.lstSymbole.TabIndex = 1;
            this.lstSymbole.ItemClicked += new EventHandler<ListItemEventArgs>(this.LstSymbole_ItemClicked);
            //
            // grpGröße
            //
            this.grpGröße.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
                | AnchorStyles.Right)));
            this.grpGröße.Controls.Add(this.capSize);
            this.grpGröße.Controls.Add(this.sldSize);
            this.grpGröße.Location = new Point(8, 160);
            this.grpGröße.Name = "grpGröße";
            this.grpGröße.Size = new Size(252, 56);
            this.grpGröße.TabIndex = 1;
            this.grpGröße.TabStop = false;
            this.grpGröße.Text = "Größe";
            //
            // capSize
            //
            this.capSize.Location = new Point(196, 20);
            this.capSize.Name = "capSize";
            this.capSize.Size = new Size(48, 24);
            this.capSize.TabIndex = 1;
            this.capSize.Text = "32";
            //
            // sldSize
            //
            this.sldSize.LargeChange = 5f;
            this.sldSize.Location = new Point(8, 20);
            this.sldSize.Maximum = 128f;
            this.sldSize.Minimum = 8f;
            this.sldSize.MouseChange = 1f;
            this.sldSize.Name = "sldSize";
            this.sldSize.Size = new Size(180, 24);
            this.sldSize.SmallChange = 1f;
            this.sldSize.TabIndex = 0;
            this.sldSize.Value = 32f;
            this.sldSize.ValueChanged += new EventHandler(this.SldSize_ValueChanged);
            //
            // StampTool
            //
            this.Controls.Add(this.grpGröße);
            this.Controls.Add(this.grpSymbol);
            this.Name = "StampTool";
            this.Size = new Size(268, 224);
            this.grpSymbol.ResumeLayout(false);
            this.grpGröße.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        private GroupBox grpSymbol;
        private TextBox txbSuche;
        private Button btnFilterDel;
        private ListBox lstSymbole;
        private GroupBox grpGröße;
        private Caption capSize;
        private Slider sldSize;
    }
}
