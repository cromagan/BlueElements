using System.Diagnostics;
using BlueControls.Controls;

namespace BlueControls.Forms
{

    public partial class PictureView : Form
        {
			//Das Formular überschreibt den Deletevorgang, um die Komponentenliste zu bereinigen.
			[DebuggerNonUserCode()]
			protected override void Dispose(bool disposing)
			{
				if (disposing )
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
            this.grpSeiten = new BlueControls.Controls.GroupBox();
            this.Rechts = new BlueControls.Controls.Button();
            this.Links = new BlueControls.Controls.Button();
            this.ZoomOut = new BlueControls.Controls.Button();
            this.ZoomIn = new BlueControls.Controls.Button();
            this.ZoomFitBut = new BlueControls.Controls.Button();
            this.Pad = new BlueControls.Controls.ZoomPic();
            this.Ribbon = new BlueControls.Controls.TabControl();
            this.Page_Control = new BlueControls.Controls.TabPage();
            this.grpWerkzeuge = new BlueControls.Controls.GroupBox();
            this.Auswahl = new BlueControls.Controls.Button();
            this.grpSeiten.SuspendLayout();
            this.Ribbon.SuspendLayout();
            this.Page_Control.SuspendLayout();
            this.grpWerkzeuge.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpSeiten
            // 
            this.grpSeiten.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
            this.grpSeiten.CausesValidation = false;
            this.grpSeiten.Controls.Add(this.Rechts);
            this.grpSeiten.Controls.Add(this.Links);
            this.grpSeiten.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpSeiten.Location = new System.Drawing.Point(0, 0);
            this.grpSeiten.Name = "grpSeiten";
            this.grpSeiten.Size = new System.Drawing.Size(112, 81);
            this.grpSeiten.Text = "Seiten";
            // 
            // Rechts
            // 
            this.Rechts.ImageCode = "Pfeil_Rechts";
            this.Rechts.Location = new System.Drawing.Point(56, 2);
            this.Rechts.Name = "Rechts";
            this.Rechts.Size = new System.Drawing.Size(48, 66);
            this.Rechts.TabIndex = 6;
            this.Rechts.Text = "vor";
            this.Rechts.Click += new System.EventHandler(this.Rechts_Click);
            // 
            // Links
            // 
            this.Links.ImageCode = "Pfeil_Links";
            this.Links.Location = new System.Drawing.Point(7, 2);
            this.Links.Name = "Links";
            this.Links.Size = new System.Drawing.Size(49, 66);
            this.Links.TabIndex = 5;
            this.Links.Text = "zurück";
            this.Links.Click += new System.EventHandler(this.Links_Click);
            // 
            // ZoomOut
            // 
            this.ZoomOut.ButtonStyle = BlueControls.Enums.enButtonStyle.Optionbox;
            this.ZoomOut.ImageCode = "LupeMinus";
            this.ZoomOut.Location = new System.Drawing.Point(120, 2);
            this.ZoomOut.Name = "ZoomOut";
            this.ZoomOut.Size = new System.Drawing.Size(56, 66);
            this.ZoomOut.TabIndex = 2;
            this.ZoomOut.Text = "kleiner";
            // 
            // ZoomIn
            // 
            this.ZoomIn.ButtonStyle = BlueControls.Enums.enButtonStyle.Optionbox;
            this.ZoomIn.ImageCode = "LupePlus";
            this.ZoomIn.Location = new System.Drawing.Point(176, 2);
            this.ZoomIn.Name = "ZoomIn";
            this.ZoomIn.Size = new System.Drawing.Size(56, 66);
            this.ZoomIn.TabIndex = 1;
            this.ZoomIn.Text = "größer";
            // 
            // ZoomFitBut
            // 
            this.ZoomFitBut.ImageCode = "ZoomFit";
            this.ZoomFitBut.Location = new System.Drawing.Point(8, 2);
            this.ZoomFitBut.Name = "ZoomFitBut";
            this.ZoomFitBut.Size = new System.Drawing.Size(48, 66);
            this.ZoomFitBut.TabIndex = 0;
            this.ZoomFitBut.Text = "ein-passen";
            this.ZoomFitBut.Click += new System.EventHandler(this.ZoomFitBut_Click);
            // 
            // Pad
            // 
            this.Pad.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Pad.Location = new System.Drawing.Point(0, 110);
            this.Pad.Name = "Pad";
            this.Pad.Size = new System.Drawing.Size(1334, 571);
            this.Pad.TabIndex = 0;
            this.Pad.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Pad_MouseUp);
            // 
            // Ribbon
            // 
            this.Ribbon.Controls.Add(this.Page_Control);
            this.Ribbon.Dock = System.Windows.Forms.DockStyle.Top;
            this.Ribbon.HotTrack = true;
            this.Ribbon.IsRibbonBar = true;
            this.Ribbon.Location = new System.Drawing.Point(0, 0);
            this.Ribbon.Name = "Ribbon";
            this.Ribbon.Size = new System.Drawing.Size(1334, 110);
            this.Ribbon.TabIndex = 2;
            // 
            // Page_Control
            // 
            this.Page_Control.Controls.Add(this.grpWerkzeuge);
            this.Page_Control.Controls.Add(this.grpSeiten);
            this.Page_Control.Location = new System.Drawing.Point(4, 25);
            this.Page_Control.Name = "Page_Control";
            this.Page_Control.Size = new System.Drawing.Size(1326, 81);
            this.Page_Control.TabIndex = 0;
            this.Page_Control.Text = "Steuerung";
            // 
            // grpWerkzeuge
            // 
            this.grpWerkzeuge.CausesValidation = false;
            this.grpWerkzeuge.Controls.Add(this.Auswahl);
            this.grpWerkzeuge.Controls.Add(this.ZoomFitBut);
            this.grpWerkzeuge.Controls.Add(this.ZoomOut);
            this.grpWerkzeuge.Controls.Add(this.ZoomIn);
            this.grpWerkzeuge.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpWerkzeuge.Location = new System.Drawing.Point(112, 0);
            this.grpWerkzeuge.Name = "grpWerkzeuge";
            this.grpWerkzeuge.Size = new System.Drawing.Size(240, 81);
            this.grpWerkzeuge.Text = "Werkzeuge";
            // 
            // Auswahl
            // 
            this.Auswahl.ButtonStyle = BlueControls.Enums.enButtonStyle.Optionbox;
            this.Auswahl.Checked = true;
            this.Auswahl.ImageCode = "Mauspfeil";
            this.Auswahl.Location = new System.Drawing.Point(64, 2);
            this.Auswahl.Name = "Auswahl";
            this.Auswahl.Size = new System.Drawing.Size(56, 66);
            this.Auswahl.TabIndex = 3;
            this.Auswahl.Text = "wählen";
            // 
            // PictureView
            // 
            this.ClientSize = new System.Drawing.Size(1334, 681);
            this.Controls.Add(this.Pad);
            this.Controls.Add(this.Ribbon);
            this.Name = "PictureView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "(c) Christian Peter";
            this.TopMost = true;
            this.grpSeiten.ResumeLayout(false);
            this.Ribbon.ResumeLayout(false);
            this.Page_Control.ResumeLayout(false);
            this.grpWerkzeuge.ResumeLayout(false);
            this.ResumeLayout(false);

			}
			private Button Links;
			private Button Rechts;
			private Button ZoomOut;
			private Button Auswahl;
			public ZoomPic Pad;
			protected TabControl Ribbon;
			protected TabPage Page_Control;
			protected GroupBox grpSeiten;
			protected GroupBox grpWerkzeuge;
			private Button ZoomFitBut;
			private Button ZoomIn;
    }
	}