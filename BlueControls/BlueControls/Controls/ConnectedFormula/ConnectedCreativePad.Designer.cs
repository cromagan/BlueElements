
using System;
using System.ComponentModel;
using System.Windows.Forms;

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
            this.components = new System.ComponentModel.Container();
            this.pad = new BlueControls.Controls.CreativePad();
            this.EditPanelFrame = new BlueControls.Controls.GroupBox();
            this.btnCopy = new BlueControls.Controls.Button();
            this.btnAktualisieren = new BlueControls.Controls.Button();
            this._panelMover = new System.Windows.Forms.Timer(this.components);
            this._autoRefresh = new System.Windows.Forms.Timer(this.components);
            this.EditPanelFrame.SuspendLayout();
            this.SuspendLayout();
            // 
            // pad
            // 
            this.pad.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pad.Location = new System.Drawing.Point(0, 0);
            this.pad.Name = "pad";
            this.pad.ShiftX = 0F;
            this.pad.ShiftY = 0F;
            this.pad.Size = new System.Drawing.Size(500, 500);
            this.pad.TabIndex = 0;
            this.pad.Zoom = 1F;
            this.pad.MouseEnter += new System.EventHandler(this.pad_MouseEnter);
            this.pad.MouseLeave += new System.EventHandler(this.pad_MouseLeave);
            this.pad.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pad_MouseMove);
            // 
            // EditPanelFrame
            // 
            this.EditPanelFrame.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.EditPanelFrame.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.EditPanelFrame.CausesValidation = false;
            this.EditPanelFrame.Controls.Add(this.btnCopy);
            this.EditPanelFrame.Controls.Add(this.btnAktualisieren);
            this.EditPanelFrame.GroupBoxStyle = BlueControls.Enums.GroupBoxStyle.Nothing;
            this.EditPanelFrame.Location = new System.Drawing.Point(0, 0);
            this.EditPanelFrame.Name = "EditPanelFrame";
            this.EditPanelFrame.Size = new System.Drawing.Size(500, 40);
            this.EditPanelFrame.TabIndex = 1;
            this.EditPanelFrame.TabStop = false;
            this.EditPanelFrame.Visible = false;
            // 
            // btnCopy
            // 
            this.btnCopy.ImageCode = "Clipboard|28";
            this.btnCopy.Location = new System.Drawing.Point(48, 0);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.QuickInfo = "Den Inhalt als Bild\r\nin die Zwischenablage \r\nübernehmen.";
            this.btnCopy.Size = new System.Drawing.Size(38, 38);
            this.btnCopy.TabIndex = 6;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // btnAktualisieren
            // 
            this.btnAktualisieren.ImageCode = "Refresh|28";
            this.btnAktualisieren.Location = new System.Drawing.Point(0, 0);
            this.btnAktualisieren.Name = "btnAktualisieren";
            this.btnAktualisieren.QuickInfo = "Aktualisieren und neuberechnen\r\ndes Inhalts";
            this.btnAktualisieren.Size = new System.Drawing.Size(43, 38);
            this.btnAktualisieren.TabIndex = 3;
            this.btnAktualisieren.Click += new System.EventHandler(this.btnAktualisieren_Click);
            // 
            // _panelMover
            // 
            this._panelMover.Interval = 5;
            this._panelMover.Tick += new System.EventHandler(this._panelMover_Tick);
            // 
            // _autoRefresh
            // 
            this._autoRefresh.Interval = 1000;
            this._autoRefresh.Tick += new System.EventHandler(this._autoRefresh_Tick);
            // 
            // ConnectedCreativePad
            // 
            this.Controls.Add(this.EditPanelFrame);
            this.Controls.Add(this.pad);
            this.Name = "ConnectedCreativePad";
            this.Size = new System.Drawing.Size(500, 500);
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
