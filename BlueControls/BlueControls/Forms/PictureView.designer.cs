using System.Diagnostics;
using BlueControls.Controls;

namespace BlueControls.Forms
{
    public partial class PictureView 
        {
			//Das Formular überschreibt den Deletevorgang, um die Komponentenliste zu bereinigen.
			[DebuggerNonUserCode()]
			protected override void Dispose(bool disposing)
			{
				if (disposing )
				{
                FileList.Changed -= FileList_Changed;
                FileList.Dispose();

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
            this.btnVor = new BlueControls.Controls.Button();
            this.btnZurueck = new BlueControls.Controls.Button();
            this.btnZoomOut = new BlueControls.Controls.Button();
            this.btnZoomIn = new BlueControls.Controls.Button();
            this.btnZoomFit = new BlueControls.Controls.Button();
            this.Pad = new BlueControls.Controls.ZoomPic();
            this.Ribbon = new BlueControls.Controls.RibbonBar();
            this.tabBearbeiten = new System.Windows.Forms.TabPage();
            this.grpWerkzeuge = new BlueControls.Controls.GroupBox();
            this.btnChoose = new BlueControls.Controls.Button();
            this.grpSeiten.SuspendLayout();
            this.Ribbon.SuspendLayout();
            this.tabBearbeiten.SuspendLayout();
            this.grpWerkzeuge.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpSeiten
            // 
            this.grpSeiten.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpSeiten.CausesValidation = false;
            this.grpSeiten.Controls.Add(this.btnVor);
            this.grpSeiten.Controls.Add(this.btnZurueck);
            this.grpSeiten.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpSeiten.Location = new System.Drawing.Point(0, 0);
            this.grpSeiten.Name = "grpSeiten";
            this.grpSeiten.Size = new System.Drawing.Size(112, 81);
            this.grpSeiten.TabIndex = 1;
            this.grpSeiten.TabStop = false;
            this.grpSeiten.Text = "Seiten";
            // 
            // btnVor
            // 
            this.btnVor.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnVor.ImageCode = "Pfeil_Rechts";
            this.btnVor.Location = new System.Drawing.Point(56, 2);
            this.btnVor.Name = "btnVor";
            this.btnVor.Size = new System.Drawing.Size(48, 66);
            this.btnVor.TabIndex = 6;
            this.btnVor.Text = "vor";
            this.btnVor.Click += new System.EventHandler(this.btnVor_Click);
            // 
            // btnZurueck
            // 
            this.btnZurueck.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnZurueck.ImageCode = "Pfeil_Links";
            this.btnZurueck.Location = new System.Drawing.Point(7, 2);
            this.btnZurueck.Name = "btnZurueck";
            this.btnZurueck.Size = new System.Drawing.Size(49, 66);
            this.btnZurueck.TabIndex = 5;
            this.btnZurueck.Text = "zurück";
            this.btnZurueck.Click += new System.EventHandler(this.btnZurueck_Click);
            // 
            // btnZoomOut
            // 
            this.btnZoomOut.ButtonStyle = ((BlueControls.Enums.ButtonStyle)(((BlueControls.Enums.ButtonStyle.Optionbox | BlueControls.Enums.ButtonStyle.Button_Big) 
            | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnZoomOut.ImageCode = "LupeMinus";
            this.btnZoomOut.Location = new System.Drawing.Point(120, 2);
            this.btnZoomOut.Name = "btnZoomOut";
            this.btnZoomOut.Size = new System.Drawing.Size(56, 66);
            this.btnZoomOut.TabIndex = 2;
            this.btnZoomOut.Text = "kleiner";
            // 
            // btnZoomIn
            // 
            this.btnZoomIn.ButtonStyle = ((BlueControls.Enums.ButtonStyle)(((BlueControls.Enums.ButtonStyle.Optionbox | BlueControls.Enums.ButtonStyle.Button_Big) 
            | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnZoomIn.ImageCode = "LupePlus";
            this.btnZoomIn.Location = new System.Drawing.Point(176, 2);
            this.btnZoomIn.Name = "btnZoomIn";
            this.btnZoomIn.Size = new System.Drawing.Size(56, 66);
            this.btnZoomIn.TabIndex = 1;
            this.btnZoomIn.Text = "größer";
            // 
            // btnZoomFit
            // 
            this.btnZoomFit.ButtonStyle = ((BlueControls.Enums.ButtonStyle)((BlueControls.Enums.ButtonStyle.Button_Big | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnZoomFit.ImageCode = "ZoomFit";
            this.btnZoomFit.Location = new System.Drawing.Point(8, 2);
            this.btnZoomFit.Name = "btnZoomFit";
            this.btnZoomFit.Size = new System.Drawing.Size(48, 66);
            this.btnZoomFit.TabIndex = 0;
            this.btnZoomFit.Text = "ein-passen";
            this.btnZoomFit.Click += new System.EventHandler(this.btnZoomFit_Click);
            // 
            // Pad
            // 
            this.Pad.AlwaysSmooth = true;
            this.Pad.Bmp = null;
            this.Pad.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Pad.Location = new System.Drawing.Point(0, 110);
            this.Pad.Name = "Pad";
            this.Pad.Size = new System.Drawing.Size(1334, 571);
            this.Pad.TabIndex = 0;
            this.Pad.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Pad_MouseUp);
            // 
            // Ribbon
            // 
            this.Ribbon.Controls.Add(this.tabBearbeiten);
            this.Ribbon.Dock = System.Windows.Forms.DockStyle.Top;
            this.Ribbon.HotTrack = true;
            this.Ribbon.Location = new System.Drawing.Point(0, 0);
            this.Ribbon.Name = "Ribbon";
            this.Ribbon.SelectedIndex = 0;
            this.Ribbon.Size = new System.Drawing.Size(1334, 110);
            this.Ribbon.TabIndex = 2;
            // 
            // tabBearbeiten
            // 
            this.tabBearbeiten.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabBearbeiten.Controls.Add(this.grpWerkzeuge);
            this.tabBearbeiten.Controls.Add(this.grpSeiten);
            this.tabBearbeiten.Location = new System.Drawing.Point(4, 25);
            this.tabBearbeiten.Name = "tabBearbeiten";
            this.tabBearbeiten.Size = new System.Drawing.Size(1326, 81);
            this.tabBearbeiten.TabIndex = 0;
            this.tabBearbeiten.Text = "Steuerung";
            // 
            // grpWerkzeuge
            // 
            this.grpWerkzeuge.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpWerkzeuge.CausesValidation = false;
            this.grpWerkzeuge.Controls.Add(this.btnChoose);
            this.grpWerkzeuge.Controls.Add(this.btnZoomFit);
            this.grpWerkzeuge.Controls.Add(this.btnZoomOut);
            this.grpWerkzeuge.Controls.Add(this.btnZoomIn);
            this.grpWerkzeuge.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpWerkzeuge.Location = new System.Drawing.Point(112, 0);
            this.grpWerkzeuge.Name = "grpWerkzeuge";
            this.grpWerkzeuge.Size = new System.Drawing.Size(240, 81);
            this.grpWerkzeuge.TabIndex = 0;
            this.grpWerkzeuge.TabStop = false;
            this.grpWerkzeuge.Text = "Werkzeuge";
            // 
            // btnChoose
            // 
            this.btnChoose.ButtonStyle = ((BlueControls.Enums.ButtonStyle)(((BlueControls.Enums.ButtonStyle.Optionbox | BlueControls.Enums.ButtonStyle.Button_Big) 
            | BlueControls.Enums.ButtonStyle.Borderless)));
            this.btnChoose.Checked = true;
            this.btnChoose.ImageCode = "Mauspfeil";
            this.btnChoose.Location = new System.Drawing.Point(64, 2);
            this.btnChoose.Name = "btnChoose";
            this.btnChoose.Size = new System.Drawing.Size(56, 66);
            this.btnChoose.TabIndex = 3;
            this.btnChoose.Text = "wählen";
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
            this.tabBearbeiten.ResumeLayout(false);
            this.grpWerkzeuge.ResumeLayout(false);
            this.ResumeLayout(false);

			}
			private Button btnZurueck;
			private Button btnVor;
			private Button btnZoomOut;
			private Button btnChoose;
			public ZoomPic Pad;
			protected RibbonBar Ribbon;
			protected System.Windows.Forms.TabPage tabBearbeiten;
			protected GroupBox grpSeiten;
			protected GroupBox grpWerkzeuge;
			private Button btnZoomFit;
			private Button btnZoomIn;
    }
	}