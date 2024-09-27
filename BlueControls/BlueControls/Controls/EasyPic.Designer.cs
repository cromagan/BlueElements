using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Orientation = BlueBasics.Enums.Orientation;

namespace BlueControls.Controls
{
    public partial class EasyPic 
    {

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
            this.btnLoad = new BlueControls.Controls.Button();
            this.btnScreenshot = new BlueControls.Controls.Button();
            this.btnDeleteImage = new BlueControls.Controls.Button();
            this.OpenDia = new System.Windows.Forms.OpenFileDialog();
            this._panelMover = new System.Windows.Forms.Timer(this.components);
            this.EditPanelFrame.SuspendLayout();
            this.SuspendLayout();
            // 
            // EditPanelFrame
            // 
            this.EditPanelFrame.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.EditPanelFrame.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.EditPanelFrame.CausesValidation = false;
            this.EditPanelFrame.Controls.Add(this.btnLoad);
            this.EditPanelFrame.Controls.Add(this.btnScreenshot);
            this.EditPanelFrame.Controls.Add(this.btnDeleteImage);
            this.EditPanelFrame.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.Nothing;
            this.EditPanelFrame.Location = new System.Drawing.Point(0, 0);
            this.EditPanelFrame.Name = "EditPanelFrame";
            this.EditPanelFrame.Size = new System.Drawing.Size(472, 40);
            this.EditPanelFrame.TabIndex = 0;
            this.EditPanelFrame.TabStop = false;
            this.EditPanelFrame.Visible = false;
            // 
            // btnLoad
            // 
            this.btnLoad.ImageCode = "Ordner|28";
            this.btnLoad.Location = new System.Drawing.Point(42, 2);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.QuickInfo = "Ein vorhandenes Bild aus<br>dem Dateisystem wählen.";
            this.btnLoad.Size = new System.Drawing.Size(38, 38);
            this.btnLoad.TabIndex = 6;
            this.btnLoad.Click += new System.EventHandler(this.Lade_Click);
            // 
            // btnScreenshot
            // 
            this.btnScreenshot.ImageCode = "Kamera|28";
            this.btnScreenshot.Location = new System.Drawing.Point(0, 2);
            this.btnScreenshot.Name = "btnScreenshot";
            this.btnScreenshot.QuickInfo = "Einen Bildausschnitt aus einem<br>Bildschirmbereich wählen.";
            this.btnScreenshot.Size = new System.Drawing.Size(40, 38);
            this.btnScreenshot.TabIndex = 2;
            this.btnScreenshot.Click += new System.EventHandler(this.btnScreenshot_Click);
            // 
            // btnDeleteImage
            // 
            this.btnDeleteImage.ImageCode = "Kreuz|28";
            this.btnDeleteImage.Location = new System.Drawing.Point(117, 2);
            this.btnDeleteImage.Name = "btnDeleteImage";
            this.btnDeleteImage.QuickInfo = "Vorhandenes Bild entfernen";
            this.btnDeleteImage.Size = new System.Drawing.Size(43, 38);
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
            // _panelMover
            // 
            this._panelMover.Interval = 5;
            this._panelMover.Tick += new System.EventHandler(this._paneMover_Tick);
            // 
            // EasyPic
            // 
            this.Controls.Add(this.EditPanelFrame);
            this.Name = "EasyPic";
            this.Size = new System.Drawing.Size(472, 408);
            this.EditPanelFrame.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private Button btnDeleteImage;
        private Button btnScreenshot;
        private GroupBox EditPanelFrame;
        private Button btnLoad;
        private OpenFileDialog OpenDia;
        private Timer _panelMover;
    }
}
