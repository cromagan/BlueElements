using System;
using System.Diagnostics;
using System.Drawing;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueDatabase;

namespace BlueControls.Classes_Editor
{
    internal partial class RuleActionItem_Editor : AbstractClassEditor<RuleActionItem>
    {
        //UserControl überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {

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
            this.btnHelp = new Button();
            this.cbxRuleAktion = new ComboBox();
            this.BlueLine1 = new Line();
            this.txbRuleActionText = new TextBox();
            this.lstRuleAktionColumns = new ListBox();
            this.capColumns = new Caption();
            this.capText = new Caption();
            this.capAktion = new Caption();
            this.SuspendLayout();
            //
            //Help
            //
            this.btnHelp.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
            this.btnHelp.ImageCode = "Information|20";
            this.btnHelp.Location = new Point(478, 15);
            this.btnHelp.Name = "Help";
            this.btnHelp.Size = new Size(40, 22);
            this.btnHelp.TabIndex = 13;
            this.btnHelp.Click += new EventHandler(btnHelp_Click);
            //
            //cbxRuleAktion
            //
            this.cbxRuleAktion.Anchor = (System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
            this.cbxRuleAktion.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cbxRuleAktion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxRuleAktion.Enabled = false;
            this.cbxRuleAktion.Format = enDataFormat.Text;
            this.cbxRuleAktion.Location = new Point(63, 15);
            this.cbxRuleAktion.Name = "cbxRuleAktion";
            this.cbxRuleAktion.Size = new Size(415, 22);
            this.cbxRuleAktion.TabIndex = 12;
            this.cbxRuleAktion.ItemClicked += new EventHandler<BasicListItemEventArgs>(cbxRuleAktion_ItemClicked);
            this.cbxRuleAktion.TextChanged += new EventHandler(cbxRuleAktion_TextChanged);
            //
            //BlueLine1
            //
            this.BlueLine1.Anchor = (System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
            this.BlueLine1.CausesValidation = false;
            this.BlueLine1.Location = new Point(7, 47);
            this.BlueLine1.Name = "BlueLine1";
            this.BlueLine1.Size = new Size(512, 2);
            this.BlueLine1.Text = "BlueLine1";
            //
            //txbRuleActionText
            //
            this.txbRuleActionText.Anchor = (System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
            this.txbRuleActionText.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txbRuleActionText.Enabled = false;
            this.txbRuleActionText.Location = new Point(231, 79);
            this.txbRuleActionText.MultiLine = true;
            this.txbRuleActionText.Name = "txbRuleActionText";
            this.txbRuleActionText.Size = new Size(287, 129);
            this.txbRuleActionText.TabIndex = 15;
            this.txbRuleActionText.Verhalten = enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            this.txbRuleActionText.TextChanged += new EventHandler(txbRuleActionText_TextChanged);
            //
            //lstRuleAktionColumns
            //
            this.lstRuleAktionColumns.AddAllowed = enAddType.OnlySuggests;
            this.lstRuleAktionColumns.Anchor = (System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left);
            this.lstRuleAktionColumns.CheckBehavior = enCheckBehavior.MultiSelection;
            this.lstRuleAktionColumns.Enabled = false;
            this.lstRuleAktionColumns.FilterAllowed = true;
            this.lstRuleAktionColumns.Location = new Point(7, 79);
            this.lstRuleAktionColumns.Name = "lstRuleAktionColumns";
            this.lstRuleAktionColumns.QuickInfo = "";
            this.lstRuleAktionColumns.Size = new Size(216, 129);
            this.lstRuleAktionColumns.TabIndex = 14;
            this.lstRuleAktionColumns.ItemClicked += new EventHandler<BasicListItemEventArgs>(lstRuleAktionColumns_ItemClicked);
            //
            //capColumns
            //
            this.capColumns.CausesValidation = false;
            this.capColumns.Location = new Point(7, 59);
            this.capColumns.Name = "capColumns";
            this.capColumns.Size = new Size(112, 20);
            this.capColumns.Text = "Betrifft Spalte(n):";
            //
            //capText
            //
            this.capText.CausesValidation = false;
            this.capText.Location = new Point(231, 59);
            this.capText.Name = "capText";
            this.capText.Size = new Size(88, 20);
            this.capText.Text = "Items / Text:";
            //
            //capAktion
            //
            this.capAktion.CausesValidation = false;
            this.capAktion.Location = new Point(7, 15);
            this.capAktion.Name = "Caption15";
            this.capAktion.Size = new Size(56, 16);
            this.capAktion.Text = "Aktion:";
            //
            //RuleActionItem_Editor
            //
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.cbxRuleAktion);
            this.Controls.Add(this.BlueLine1);
            this.Controls.Add(this.txbRuleActionText);
            this.Controls.Add(this.lstRuleAktionColumns);
            this.Controls.Add(this.capColumns);
            this.Controls.Add(this.capText);
            this.Controls.Add(this.capAktion);
            this.Name = "RuleActionItem_Editor";
            this.Size = new Size(527, 220);
            this.ResumeLayout(false);

        }

        private Button btnHelp;
        private ComboBox cbxRuleAktion;
        private Line BlueLine1;
        private TextBox txbRuleActionText;
        private ListBox lstRuleAktionColumns;
        private Caption capColumns;
        private Caption capText;
        private Caption capAktion;
    }
}
