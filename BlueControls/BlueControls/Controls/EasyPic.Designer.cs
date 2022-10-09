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
            this.components = new System.ComponentModel.Container();
            this.EditPanelFrame = new BlueControls.Controls.GroupBox();
            this.BlueLine1 = new BlueControls.Controls.Line();
            this.Lade = new BlueControls.Controls.Button();
            this.btnScreenshot = new BlueControls.Controls.Button();
            this.btnDeleteImage = new BlueControls.Controls.Button();
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
            this.EditPanelFrame.Controls.Add(this.btnScreenshot);
            this.EditPanelFrame.Controls.Add(this.btnDeleteImage);
            this.EditPanelFrame.Location = new System.Drawing.Point(0, 0);
            this.EditPanelFrame.Name = "EditPanelFrame";
            this.EditPanelFrame.Size = new System.Drawing.Size(472, 32);
            this.EditPanelFrame.TabIndex = 0;
            this.EditPanelFrame.TabStop = false;
            this.EditPanelFrame.Visible = false;
            // 
            // BlueLine1
            // 
            this.BlueLine1.CausesValidation = false;
            this.BlueLine1.Location = new System.Drawing.Point(112, 3);
            this.BlueLine1.Name = "BlueLine1";
            this.BlueLine1.Orientation = BlueBasics.Enums.Orientation.Senkrecht;
            this.BlueLine1.Size = new System.Drawing.Size(2, 26);
            // 
            // Lade
            // 
            this.Lade.ImageCode = "Ordner|16";
            this.Lade.Location = new System.Drawing.Point(42, 3);
            this.Lade.Name = "Lade";
            this.Lade.QuickInfo = "Ein vorhandenes Bild aus<br>dem Dateisystem wählen.";
            this.Lade.Size = new System.Drawing.Size(32, 26);
            this.Lade.TabIndex = 6;
            this.Lade.Click += new System.EventHandler(this.Lade_Click);
            // 
            // btnScreenshot
            // 
            this.btnScreenshot.ImageCode = "Kamera|16";
            this.btnScreenshot.Location = new System.Drawing.Point(7, 3);
            this.btnScreenshot.Name = "btnScreenshot";
            this.btnScreenshot.QuickInfo = "Einen Bildausschnitt aus einem<br>Bildschirmbereich wählen.";
            this.btnScreenshot.Size = new System.Drawing.Size(32, 26);
            this.btnScreenshot.TabIndex = 2;
            this.btnScreenshot.Click += new System.EventHandler(this.btnScreenshot_Click);
            // 
            // btnDeleteImage
            // 
            this.btnDeleteImage.ImageCode = "Kreuz|16";
            this.btnDeleteImage.Location = new System.Drawing.Point(117, 3);
            this.btnDeleteImage.Name = "btnDeleteImage";
            this.btnDeleteImage.QuickInfo = "Vorhandenes Bild entfernen";
            this.btnDeleteImage.Size = new System.Drawing.Size(32, 26);
            this.btnDeleteImage.TabIndex = 3;
            this.btnDeleteImage.Click += new System.EventHandler(this.DelP_Click);
            // 
            // OpenDia
            // 
            this.OpenDia.DefaultExt = "jpg";
            this.OpenDia.Filter = "Bild-Dateien|*.jpg;*.jpeg;*.png;*.bmp|Alle Dateien|*.*";
            this.OpenDia.Title = "Wählen sie ein Bild, das sie verwenden wollen:";
            this.OpenDia.FileOk += new System.ComponentModel.CancelEventHandler(this.OpenDia_FileOk);
            // 
            // _PanelMover
            // 
            this._PanelMover.Interval = 5;
            this._PanelMover.Tick += new System.EventHandler(this.EditPanel_Tick);
            // 
            // EasyPic
            // 
            this.Controls.Add(this.EditPanelFrame);
            this.Size = new System.Drawing.Size(472, 408);
            this.EditPanelFrame.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private Button btnDeleteImage;
        private Button btnScreenshot;
        private GroupBox EditPanelFrame;
        private Line BlueLine1;
        private Button Lade;
        private System.Windows.Forms.OpenFileDialog OpenDia;
        private System.Windows.Forms.Timer _PanelMover;
    }
}
