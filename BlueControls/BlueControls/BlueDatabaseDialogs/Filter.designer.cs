using System;
using System.Diagnostics;
using System.Drawing;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Forms;

namespace BlueControls.BlueDatabaseDialogs
{


    public sealed partial class Filter
    {
        //Das Formular überschreibt den Deletevorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }
            base.Dispose(disposing);
        }



        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.filterleiste = new BlueControls.BlueDatabaseDialogs.Filterleiste();
            this.SuspendLayout();
            // 
            // filterleiste
            // 
            this.filterleiste.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.filterleiste.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.filterleiste.Dock = System.Windows.Forms.DockStyle.Fill;
            this.filterleiste.Filtertypes = ((BlueControls.Enums.enFilterTypesToShow)((BlueControls.Enums.enFilterTypesToShow.SichtbareSpalten_DauerFilter | BlueControls.Enums.enFilterTypesToShow.SichtbareSpalten_AktiveFilter)));
            this.filterleiste.Location = new System.Drawing.Point(0, 0);
            this.filterleiste.Name = "filterleiste";
            this.filterleiste.Orientation = BlueBasics.Enums.enOrientation.Senkrecht;
            this.filterleiste.Size = new System.Drawing.Size(710, 560);
            this.filterleiste.TabIndex = 0;
            this.filterleiste.TabStop = false;
            // 
            // Filter
            // 
            this.ClientSize = new System.Drawing.Size(710, 560);
            this.Controls.Add(this.filterleiste);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "Filter";
            this.ShowInTaskbar = false;
            this.Text = "Filter";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        private Filterleiste filterleiste;
    }
}
