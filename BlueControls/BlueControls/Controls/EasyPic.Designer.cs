using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using BlueBasics.Enums;
using BlueControls.Enums;

namespace BlueControls.Controls
{
    public partial class EasyPic 
    {
        //Inherits Windows.Forms.UserControl
        //UserControl überschreibt den Deletevorgang, um die Komponentenliste zu bereinigen.
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
            this.components = new Container();
            this.EditPanelFrame = new GroupBox();
            this.BlueLine1 = new Line();
            this.Lade = new Button();
            this.MakePic = new Button();
            this.DelP = new Button();
            this.OpenDia = new System.Windows.Forms.OpenFileDialog();
            this._PanelMover = new System.Windows.Forms.Timer(this.components);
            this.EditPanelFrame.SuspendLayout();
            this.SuspendLayout();
            // 
            // EditPanelFrame
            // 
            this.EditPanelFrame.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                                                          | System.Windows.Forms.AnchorStyles.Right)));
            this.EditPanelFrame.CausesValidation = false;
            this.EditPanelFrame.Controls.Add(this.BlueLine1);
            this.EditPanelFrame.Controls.Add(this.Lade);
            this.EditPanelFrame.Controls.Add(this.MakePic);
            this.EditPanelFrame.Controls.Add(this.DelP);
            this.EditPanelFrame.Location = new Point(0, 0);
            this.EditPanelFrame.Name = "EditPanelFrame";
            this.EditPanelFrame.Size = new Size(472, 32);
            this.EditPanelFrame.Visible = false;
            // 
            // BlueLine1
            // 
            this.BlueLine1.CausesValidation = false;
            this.BlueLine1.Location = new Point(112, 3);
            this.BlueLine1.Name = "BlueLine1";
            this.BlueLine1.Orientation = Orientation.Senkrecht;
            this.BlueLine1.Size = new Size(2, 26);
            // 
            // Lade
            // 
            this.Lade.ImageCode = "Ordner|16";
            this.Lade.Location = new Point(42, 3);
            this.Lade.Name = "Lade";
            this.Lade.QuickInfo = "Ein vorhandenes Bild aus<br>dem Dateisystem wählen.";
            this.Lade.Size = new Size(32, 26);
            this.Lade.TabIndex = 6;
            this.Lade.Click += new EventHandler(this.Lade_Click);
            // 
            // MakePic
            // 
            this.MakePic.ImageCode = "Kamera|16";
            this.MakePic.Location = new Point(7, 3);
            this.MakePic.Name = "MakePic";
            this.MakePic.QuickInfo = "Einen Bildausschnitt aus einem<br>Bildschirmbereich wählen.";
            this.MakePic.Size = new Size(32, 26);
            this.MakePic.TabIndex = 2;
            this.MakePic.Click += new EventHandler(this.MakePic_Click);
            // 
            // DelP
            // 
            this.DelP.ImageCode = "Kreuz|16";
            this.DelP.Location = new Point(117, 3);
            this.DelP.Name = "DelP";
            this.DelP.QuickInfo = "Vorhandenes Bild entfernen";
            this.DelP.Size = new Size(32, 26);
            this.DelP.TabIndex = 3;
            this.DelP.Click += new EventHandler(this.DelP_Click);
            // 
            // OpenDia
            // 
            this.OpenDia.DefaultExt = "jpg";
            this.OpenDia.Filter = "Bild-Dateien|*.jpg;*.jpeg;*.png;*.bmp|Alle Dateien|*.*";
            this.OpenDia.Title = "Wählen sie ein Bild, das sie verwenden wollen:";
            this.OpenDia.FileOk += new CancelEventHandler(this.OpenDia_FileOk);
            // 
            // _PanelMover
            // 
            this._PanelMover.Interval = 5;
            this._PanelMover.Tick += new EventHandler(this.EditPanel_Tick);
            // 
            // EasyPic
            // 
            this.Controls.Add(this.EditPanelFrame);
            this.Name = "EasyPic";
            this.Size = new Size(472, 408);
            this.EditPanelFrame.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        private Button DelP;
        private Button MakePic;
        private GroupBox EditPanelFrame;
        private Line BlueLine1;
        private Button Lade;
        private System.Windows.Forms.OpenFileDialog OpenDia;
        private System.Windows.Forms.Timer _PanelMover;
    }
}
