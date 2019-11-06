using System;
using System.Diagnostics;
using System.Drawing;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;

namespace BlueControls.BlueDatabaseDialogs
{


    public partial class FormulaQuickSelect : Form
    {
        //    Inherits Form
        //Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
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
            this.Caption1 = new Caption();
            this.Für = new Caption();
            this.Caption2 = new Caption();
            this.Such = new TextBox();
            this.Auswahl = new ListBox();
            this.Caption3 = new Caption();
            this.SuspendLayout();
            // 
            // Caption1
            // 
            this.Caption1.CausesValidation = false;
            this.Caption1.Location = new Point(8, 8);
            this.Caption1.Name = "Caption1";
            this.Caption1.Size = new Size(184, 18);
            this.Caption1.Text = "Werte schnell ergänzen für:";
            // 
            // Für
            // 
            this.Für.CausesValidation = false;
            this.Für.Location = new Point(8, 24);
            this.Für.Name = "Für";
            this.Für.Size = new Size(320, 18);
            // 
            // Caption2
            // 
            this.Caption2.CausesValidation = false;
            this.Caption2.Location = new Point(8, 64);
            this.Caption2.Name = "Caption2";
            this.Caption2.Size = new Size(176, 18);
            this.Caption2.Text = "Wert ist bzw. enthält Text:";
            // 
            // Such
            // 
            this.Such.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.Such.Location = new Point(8, 80);
            this.Such.Name = "Such";
            this.Such.Size = new Size(312, 24);
            this.Such.TabIndex = 2;
            this.Such.TextChanged += new EventHandler(this.Such_TextChanged);
            // 
            // Auswahl
            // 
            this.Auswahl.AddAllowed = enAddType.OnlySuggests;
            this.Auswahl.CheckBehavior = enCheckBehavior.MultiSelection;
            this.Auswahl.Location = new Point(8, 136);
            this.Auswahl.Name = "Auswahl";
            this.Auswahl.QuickInfo = "";
            this.Auswahl.Size = new Size(312, 288);
            this.Auswahl.TabIndex = 3;
            this.Auswahl.ItemClicked += new EventHandler<BasicListItemEventArgs>(this.Auswahl_ItemClicked);
            // 
            // Caption3
            // 
            this.Caption3.CausesValidation = false;
            this.Caption3.Location = new Point(8, 120);
            this.Caption3.Name = "Caption3";
            this.Caption3.Size = new Size(144, 18);
            this.Caption3.Text = "Verfügbare Werte:";
            // 
            // FormulaQuickSelect
            // 
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new Size(329, 428);
            this.Controls.Add(this.Für);
            this.Controls.Add(this.Such);
            this.Controls.Add(this.Auswahl);
            this.Controls.Add(this.Caption3);
            this.Controls.Add(this.Caption2);
            this.Controls.Add(this.Caption1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormulaQuickSelect";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Schnelleingabe";
            this.ResumeLayout(false);

        }
        internal Caption Caption1;
        internal Caption Für;
        internal Caption Caption2;
        internal TextBox Such;
        internal ListBox Auswahl;
        internal Caption Caption3;
    }
}
