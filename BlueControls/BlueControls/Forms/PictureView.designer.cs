using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Controls;
using BlueControls.Enums;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;

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
                //_fileList.Changed -= FileList_Changed;
                //_fileList?.Dispose();

                }
				base.Dispose(disposing);
			}
			//Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
			//Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
			//Das Bearbeiten mit dem Code-Editor ist nicht möglich.
			[DebuggerStepThrough()]
			private void InitializeComponent()
			{
            this.grpSeiten = new GroupBox();
            this.btnVor = new Button();
            this.btnZurueck = new Button();
            this.btnZoomOut = new Button();
            this.btnZoomIn = new Button();
            this.btnZoomFit = new Button();
            this.Pad = new ZoomPic();
            this.Ribbon = new RibbonBar();
            this.tabBearbeiten = new TabPage();
            this.btnTopMost = new Button();
            this.grpWerkzeuge = new GroupBox();
            this.btnChoose = new Button();
            this.grpSeiten.SuspendLayout();
            this.Ribbon.SuspendLayout();
            this.tabBearbeiten.SuspendLayout();
            this.grpWerkzeuge.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpSeiten
            // 
            this.grpSeiten.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpSeiten.CausesValidation = false;
            this.grpSeiten.Controls.Add(this.btnVor);
            this.grpSeiten.Controls.Add(this.btnZurueck);
            this.grpSeiten.Dock = DockStyle.Left;
            this.grpSeiten.Location = new Point(0, 0);
            this.grpSeiten.Name = "grpSeiten";
            this.grpSeiten.Size = new Size(112, 81);
            this.grpSeiten.TabIndex = 1;
            this.grpSeiten.TabStop = false;
            this.grpSeiten.Text = "Seiten";
            // 
            // btnVor
            // 
            this.btnVor.ButtonStyle = ((ButtonStyle)((ButtonStyle.Button_Big | ButtonStyle.Borderless)));
            this.btnVor.ImageCode = "Pfeil_Rechts";
            this.btnVor.Location = new Point(56, 2);
            this.btnVor.Name = "btnVor";
            this.btnVor.Size = new Size(48, 66);
            this.btnVor.TabIndex = 6;
            this.btnVor.Text = "vor";
            this.btnVor.Click += new EventHandler(this.btnVor_Click);
            // 
            // btnZurueck
            // 
            this.btnZurueck.ButtonStyle = ((ButtonStyle)((ButtonStyle.Button_Big | ButtonStyle.Borderless)));
            this.btnZurueck.ImageCode = "Pfeil_Links";
            this.btnZurueck.Location = new Point(7, 2);
            this.btnZurueck.Name = "btnZurueck";
            this.btnZurueck.Size = new Size(49, 66);
            this.btnZurueck.TabIndex = 5;
            this.btnZurueck.Text = "zurück";
            this.btnZurueck.Click += new EventHandler(this.btnZurueck_Click);
            // 
            // btnZoomOut
            // 
            this.btnZoomOut.ButtonStyle = ((ButtonStyle)(((ButtonStyle.Optionbox | ButtonStyle.Button_Big) 
                                                          | ButtonStyle.Borderless)));
            this.btnZoomOut.ImageCode = "LupeMinus";
            this.btnZoomOut.Location = new Point(120, 2);
            this.btnZoomOut.Name = "btnZoomOut";
            this.btnZoomOut.Size = new Size(56, 66);
            this.btnZoomOut.TabIndex = 2;
            this.btnZoomOut.Text = "kleiner";
            // 
            // btnZoomIn
            // 
            this.btnZoomIn.ButtonStyle = ((ButtonStyle)(((ButtonStyle.Optionbox | ButtonStyle.Button_Big) 
                                                         | ButtonStyle.Borderless)));
            this.btnZoomIn.ImageCode = "LupePlus";
            this.btnZoomIn.Location = new Point(176, 2);
            this.btnZoomIn.Name = "btnZoomIn";
            this.btnZoomIn.Size = new Size(56, 66);
            this.btnZoomIn.TabIndex = 1;
            this.btnZoomIn.Text = "größer";
            // 
            // btnZoomFit
            // 
            this.btnZoomFit.ButtonStyle = ((ButtonStyle)((ButtonStyle.Button_Big | ButtonStyle.Borderless)));
            this.btnZoomFit.ImageCode = "ZoomFit";
            this.btnZoomFit.Location = new Point(8, 2);
            this.btnZoomFit.Name = "btnZoomFit";
            this.btnZoomFit.Size = new Size(48, 66);
            this.btnZoomFit.TabIndex = 0;
            this.btnZoomFit.Text = "ein-passen";
            this.btnZoomFit.Click += new EventHandler(this.btnZoomFit_Click);
            // 
            // Pad
            // 
            this.Pad.AlwaysSmooth = true;
            this.Pad.Dock = DockStyle.Fill;
            this.Pad.Location = new Point(0, 110);
            this.Pad.Name = "Pad";
            this.Pad.ShiftX = -1F;
            this.Pad.ShiftY = -1F;
            this.Pad.Size = new Size(1334, 571);
            this.Pad.TabIndex = 0;
            this.Pad.Zoom = 1F;
            this.Pad.MouseUp += new MouseEventHandler(this.Pad_MouseUp);
            // 
            // Ribbon
            // 
            this.Ribbon.Controls.Add(this.tabBearbeiten);
            this.Ribbon.Dock = DockStyle.Top;
            this.Ribbon.HotTrack = true;
            this.Ribbon.Location = new Point(0, 0);
            this.Ribbon.Name = "Ribbon";
            this.Ribbon.SelectedIndex = 0;
            this.Ribbon.Size = new Size(1334, 110);
            this.Ribbon.TabDefault = null;
            this.Ribbon.TabDefaultOrder = null;
            this.Ribbon.TabIndex = 2;
            // 
            // tabBearbeiten
            // 
            this.tabBearbeiten.BackColor = Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.tabBearbeiten.Controls.Add(this.btnTopMost);
            this.tabBearbeiten.Controls.Add(this.grpWerkzeuge);
            this.tabBearbeiten.Controls.Add(this.grpSeiten);
            this.tabBearbeiten.Location = new Point(4, 25);
            this.tabBearbeiten.Name = "tabBearbeiten";
            this.tabBearbeiten.Size = new Size(1326, 81);
            this.tabBearbeiten.TabIndex = 0;
            this.tabBearbeiten.Text = "Steuerung";
            // 
            // btnTopMost
            // 
            this.btnTopMost.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.btnTopMost.ButtonStyle = ((ButtonStyle)(((ButtonStyle.Checkbox | ButtonStyle.Button_Big) 
                                                          | ButtonStyle.Borderless)));
            this.btnTopMost.ImageCode = "Pinnadel|24";
            this.btnTopMost.Location = new Point(1294, 0);
            this.btnTopMost.Name = "btnTopMost";
            this.btnTopMost.Size = new Size(32, 32);
            this.btnTopMost.TabIndex = 2;
            this.btnTopMost.CheckedChanged += new EventHandler(this.btnTopMost_CheckedChanged);
            // 
            // grpWerkzeuge
            // 
            this.grpWerkzeuge.BackColor = Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpWerkzeuge.CausesValidation = false;
            this.grpWerkzeuge.Controls.Add(this.btnChoose);
            this.grpWerkzeuge.Controls.Add(this.btnZoomFit);
            this.grpWerkzeuge.Controls.Add(this.btnZoomOut);
            this.grpWerkzeuge.Controls.Add(this.btnZoomIn);
            this.grpWerkzeuge.Dock = DockStyle.Left;
            this.grpWerkzeuge.Location = new Point(112, 0);
            this.grpWerkzeuge.Name = "grpWerkzeuge";
            this.grpWerkzeuge.Size = new Size(240, 81);
            this.grpWerkzeuge.TabIndex = 0;
            this.grpWerkzeuge.TabStop = false;
            this.grpWerkzeuge.Text = "Werkzeuge";
            // 
            // btnChoose
            // 
            this.btnChoose.ButtonStyle = ((ButtonStyle)(((ButtonStyle.Optionbox | ButtonStyle.Button_Big) 
                                                         | ButtonStyle.Borderless)));
            this.btnChoose.Checked = true;
            this.btnChoose.ImageCode = "Mauspfeil";
            this.btnChoose.Location = new Point(64, 2);
            this.btnChoose.Name = "btnChoose";
            this.btnChoose.Size = new Size(56, 66);
            this.btnChoose.TabIndex = 3;
            this.btnChoose.Text = "wählen";
            // 
            // PictureView
            // 
            this.ClientSize = new Size(1334, 681);
            this.Controls.Add(this.Pad);
            this.Controls.Add(this.Ribbon);
            this.Name = "PictureView";
            this.StartPosition = FormStartPosition.Manual;
            this.Text = "(c) Christian Peter";
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
			protected TabPage tabBearbeiten;
			protected GroupBox grpSeiten;
			protected GroupBox grpWerkzeuge;
			private Button btnZoomFit;
			private Button btnZoomIn;
        public Button btnTopMost;
    }
	}