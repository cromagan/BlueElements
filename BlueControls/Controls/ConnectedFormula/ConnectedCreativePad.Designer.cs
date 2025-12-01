using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Enums;

namespace BlueControls.Controls {
    partial class ConnectedCreativePad {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private IContainer components = null;



        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent() {
            this.components = new Container();
            this.pad = new CreativePad();
            this.EditPanelFrame = new GroupBox();
            this.btnCopy = new Button();
            this.btnAktualisieren = new Button();
            this._panelMover = new Timer(this.components);
            this._autoRefresh = new Timer(this.components);
            this.EditPanelFrame.SuspendLayout();
            this.SuspendLayout();
            // 
            // pad
            // 
            this.pad.Dock = DockStyle.Fill;
            this.pad.Location = new Point(0, 0);
            this.pad.Name = "pad";
            this.pad.Size = new Size(500, 500);
            this.pad.TabIndex = 0;
            this.pad.MouseEnter += new EventHandler(this.pad_MouseEnter);
            this.pad.MouseLeave += new EventHandler(this.pad_MouseLeave);
            this.pad.MouseMove += new MouseEventHandler(this.pad_MouseMove);
            // 
            // EditPanelFrame
            // 
            this.EditPanelFrame.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
                                                          | AnchorStyles.Right)));
            this.EditPanelFrame.BackColor = Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.EditPanelFrame.CausesValidation = false;
            this.EditPanelFrame.Controls.Add(this.btnCopy);
            this.EditPanelFrame.Controls.Add(this.btnAktualisieren);
            this.EditPanelFrame.GroupBoxStyle = GroupBoxStyle.Nothing;
            this.EditPanelFrame.Location = new Point(0, 0);
            this.EditPanelFrame.Name = "EditPanelFrame";
            this.EditPanelFrame.Size = new Size(500, 40);
            this.EditPanelFrame.TabIndex = 1;
            this.EditPanelFrame.TabStop = false;
            this.EditPanelFrame.Visible = false;
            // 
            // btnCopy
            // 
            this.btnCopy.ImageCode = "Clipboard|28";
            this.btnCopy.Location = new Point(48, 0);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.QuickInfo = "Den Inhalt als Bild\r\nin die Zwischenablage \r\nübernehmen.";
            this.btnCopy.Size = new Size(38, 38);
            this.btnCopy.TabIndex = 6;
            this.btnCopy.Click += new EventHandler(this.btnCopy_Click);
            // 
            // btnAktualisieren
            // 
            this.btnAktualisieren.ImageCode = "Refresh|28";
            this.btnAktualisieren.Location = new Point(0, 0);
            this.btnAktualisieren.Name = "btnAktualisieren";
            this.btnAktualisieren.QuickInfo = "Aktualisieren und neuberechnen\r\ndes Inhalts";
            this.btnAktualisieren.Size = new Size(43, 38);
            this.btnAktualisieren.TabIndex = 3;
            this.btnAktualisieren.Click += new EventHandler(this.btnAktualisieren_Click);
            // 
            // _panelMover
            // 
            this._panelMover.Interval = 5;
            this._panelMover.Tick += new EventHandler(this._panelMover_Tick);
            // 
            // _autoRefresh
            // 
            this._autoRefresh.Interval = 1000;
            this._autoRefresh.Tick += new EventHandler(this._autoRefresh_Tick);
            // 
            // ConnectedCreativePad
            // 
            this.Controls.Add(this.EditPanelFrame);
            this.Controls.Add(this.pad);
            this.Name = "ConnectedCreativePad";
            this.Size = new Size(500, 500);
            this.EditPanelFrame.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion


        private CreativePad pad;
        private GroupBox EditPanelFrame;
        private Button btnCopy;
        private Button btnAktualisieren;
        private Timer _panelMover;
        private Timer _autoRefresh;
    }
}
