using System;
using System.Diagnostics;
using System.Drawing;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;

namespace BlueControls.Classes_Editor
{
    internal partial class RuleActionItem_Editor : AbstractClassEditor
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
            this.Help = new  Button();
            this.Rule_Aktion = new  ComboBox();
            this.BlueLine1 = new  Line();
            this.Rule_Action_Text = new  TextBox();
            this.Rule_Aktion_Columns = new  ListBox();
            this.Caption12 = new  Caption();
            this.Caption16 = new  Caption();
            this.Caption15 = new  Caption();
            this.SuspendLayout();
            //
            //Help
            //
            this.Help.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
            this.Help.ImageCode = "Information|20";
            this.Help.Location = new Point(478, 15);
            this.Help.Name = "Help";
            this.Help.Size = new Size(40, 22);
            this.Help.TabIndex = 13;
            this.Help.Click += new EventHandler(Help_Click);
            //
            //Rule_Aktion
            //
            this.Rule_Aktion.Anchor = (System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
            this.Rule_Aktion.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.Rule_Aktion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Rule_Aktion.Enabled = false;
            this.Rule_Aktion.Format = enDataFormat.Text;
            this.Rule_Aktion.Location = new Point(63, 15);
            this.Rule_Aktion.Name = "Rule_Aktion";
            this.Rule_Aktion.Size = new Size(415, 22);
            this.Rule_Aktion.TabIndex = 12;
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
            //Rule_Action_Text
            //
            this.Rule_Action_Text.Anchor = (System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
            this.Rule_Action_Text.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.Rule_Action_Text.Enabled = false;
            this.Rule_Action_Text.Location = new Point(231, 79);
            this.Rule_Action_Text.MultiLine = true;
            this.Rule_Action_Text.Name = "Rule_Action_Text";
            this.Rule_Action_Text.Size = new Size(287, 129);
            this.Rule_Action_Text.TabIndex = 15;
            this.Rule_Action_Text.Verhalten = enSteuerelementVerhalten.Scrollen_mit_Textumbruch;
            this.Rule_Action_Text.TextChanged += new EventHandler(Rule_Action_Text_TextChanged);
            //
            //Rule_Aktion_Columns
            //
            this.Rule_Aktion_Columns.AddAllowed = enAddType.OnlySuggests;
            this.Rule_Aktion_Columns.Anchor = (System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left);
            this.Rule_Aktion_Columns.CheckBehavior = enCheckBehavior.MultiSelection;
            this.Rule_Aktion_Columns.Enabled = false;
            this.Rule_Aktion_Columns.FilterAllowed = true;
            this.Rule_Aktion_Columns.Location = new Point(7, 79);
            this.Rule_Aktion_Columns.Name = "Rule_Aktion_Columns";
            this.Rule_Aktion_Columns.QuickInfo = "";
            this.Rule_Aktion_Columns.Size = new Size(216, 129);
            this.Rule_Aktion_Columns.TabIndex = 14;
            this.Rule_Aktion_Columns.ItemClicked += new EventHandler<BasicListItemEventArgs>(Rule_Aktion_Columns_ItemClicked);
            this.Rule_Aktion.ItemClicked += new EventHandler<BasicListItemEventArgs>(Rule_Aktion_ItemClicked);
            this.Rule_Aktion.TextChanged += new EventHandler(Rule_Aktion_TextChanged);
            //
            //Caption12
            //
            this.Caption12.CausesValidation = false;
            this.Caption12.Location = new Point(7, 59);
            this.Caption12.Name = "Caption12";
            this.Caption12.Size = new Size(112, 20);
            this.Caption12.Text = "Betrifft Spalte(n):";
            //
            //Caption16
            //
            this.Caption16.CausesValidation = false;
            this.Caption16.Location = new Point(231, 59);
            this.Caption16.Name = "Caption16";
            this.Caption16.Size = new Size(88, 20);
            this.Caption16.Text = "Items / Text:";
            //
            //Caption15
            //
            this.Caption15.CausesValidation = false;
            this.Caption15.Location = new Point(7, 15);
            this.Caption15.Name = "Caption15";
            this.Caption15.Size = new Size(56, 16);
            this.Caption15.Text = "Aktion:";
            //
            //RuleActionItem_Editor
            //
            this.Controls.Add(this.Help);
            this.Controls.Add(this.Rule_Aktion);
            this.Controls.Add(this.BlueLine1);
            this.Controls.Add(this.Rule_Action_Text);
            this.Controls.Add(this.Rule_Aktion_Columns);
            this.Controls.Add(this.Caption12);
            this.Controls.Add(this.Caption16);
            this.Controls.Add(this.Caption15);
            this.Name = "RuleActionItem_Editor";
            this.Size = new Size(527, 220);
            this.ResumeLayout(false);

        }

        private Button Help;
        private ComboBox Rule_Aktion;
        private Line BlueLine1;
        private TextBox Rule_Action_Text;
        private ListBox Rule_Aktion_Columns;
        private Caption Caption12;
        private Caption Caption16;
        private Caption Caption15;
    }
}
