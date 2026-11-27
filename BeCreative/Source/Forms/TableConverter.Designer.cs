// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.Controls;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Button = BlueControls.Controls.Button;
using TextBox = BlueControls.Controls.TextBox;

namespace BeCreative {
    public sealed partial class TableConverter {
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            components = new System.ComponentModel.Container();
            pnlTop = new Panel();
            txtVerzeichnis = new TextBox();
            btnVerzeichnis = new Button();
            btnStart = new Button();
            folderBrowserDialog = new FolderBrowserDialog();
            capStatus = new Caption();
            txtLog = new TextBox();
            components.Add(folderBrowserDialog);
            pnlTop.SuspendLayout();
            SuspendLayout();
            //
            // pnlTop
            //
            pnlTop.Controls.Add(txtVerzeichnis);
            pnlTop.Controls.Add(btnVerzeichnis);
            pnlTop.Controls.Add(btnStart);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(0, 0);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(800, 31);
            pnlTop.TabIndex = 0;
            //
            // txtVerzeichnis
            //
            txtVerzeichnis.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtVerzeichnis.CausesValidation = false;
            txtVerzeichnis.Location = new Point(8, 4);
            txtVerzeichnis.Name = "txtVerzeichnis";
            txtVerzeichnis.Size = new Size(560, 24);
            txtVerzeichnis.TabIndex = 0;
            //
            // btnVerzeichnis
            //
            btnVerzeichnis.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnVerzeichnis.ImageCode = "Ordner|16";
            btnVerzeichnis.Location = new Point(576, 4);
            btnVerzeichnis.Name = "btnVerzeichnis";
            btnVerzeichnis.QuickInfo = "Verzeichnis mit den zu konvertierenden Tabellen wählen";
            btnVerzeichnis.Size = new Size(110, 24);
            btnVerzeichnis.TabIndex = 1;
            btnVerzeichnis.Text = "Verzeichnis...";
            btnVerzeichnis.Click += BtnVerzeichnis_Click;
            //
            // btnStart
            //
            btnStart.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnStart.ImageCode = "Pfeil_Rechts|16";
            btnStart.Location = new Point(694, 4);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(98, 24);
            btnStart.TabIndex = 2;
            btnStart.Text = "Konvertieren";
            btnStart.Click += BtnStart_Click;
            //
            // folderBrowserDialog
            //
            folderBrowserDialog.Description = "Verzeichnis mit den zu konvertierenden Tabellen wählen";
            folderBrowserDialog.ShowNewFolderButton = false;
            //
            // capStatus
            //
            capStatus.CausesValidation = false;
            capStatus.Dock = DockStyle.Top;
            capStatus.Location = new Point(0, 31);
            capStatus.Name = "capStatus";
            capStatus.Size = new Size(800, 24);
            capStatus.TabIndex = 1;
            capStatus.Text = "Keine Tabelle konvertiert";
            capStatus.Translate = false;
            //
            // txtLog
            //
            txtLog.CausesValidation = false;
            txtLog.Dock = DockStyle.Fill;
            txtLog.Location = new Point(0, 55);
            txtLog.MultiLine = true;
            txtLog.Name = "txtLog";
            txtLog.Size = new Size(800, 445);
            txtLog.TabIndex = 2;
            //
            // TableConverter
            //
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(800, 500);
            Controls.Add(txtLog);
            Controls.Add(capStatus);
            Controls.Add(pnlTop);
            MinimumSize = new Size(600, 380);
            Name = "TableConverter";
            StartPosition = FormStartPosition.CenterScreen;
            Tag = "";
            Text = "Tabellen-Konverter";
            Controls.SetChildIndex(pnlTop, 0);
            Controls.SetChildIndex(capStatus, 1);
            Controls.SetChildIndex(txtLog, 2);
            pnlTop.ResumeLayout(false);
            ResumeLayout(false);

        }

        private Panel pnlTop;
        private TextBox txtVerzeichnis;
        private Button btnVerzeichnis;
        private Button btnStart;
        private Caption capStatus;
        private TextBox txtLog;
        private FolderBrowserDialog folderBrowserDialog;
        private System.ComponentModel.IContainer components;
    }
}
