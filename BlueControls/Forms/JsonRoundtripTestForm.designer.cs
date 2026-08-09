// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Button = BlueControls.Controls.Button;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls.Forms;

public sealed partial class JsonRoundtripTestForm {

    #region Fields

    private IContainer components = null;

    #endregion

    #region Methods

    protected override void Dispose(bool disposing) {
        if (disposing && components is not null) {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    [DebuggerStepThrough()]
    private void InitializeComponent() {
        btnSelectFile = new Button();
        txbProtocol = new TextBox();
        dlgOpen = new OpenFileDialog();
        SuspendLayout();
        //
        // btnSelectFile
        //
        btnSelectFile.Dock = DockStyle.Top;
        btnSelectFile.ImageCode = "Ordner|16";
        btnSelectFile.Location = new Point(0, 0);
        btnSelectFile.Name = "btnSelectFile";
        btnSelectFile.Size = new Size(900, 32);
        btnSelectFile.TabIndex = 0;
        btnSelectFile.Text = "Datei auswählen (.cfo, .bcr, .bdb) ...";
        btnSelectFile.Click += btnSelectFile_Click;
        //
        // txbProtocol
        //
        txbProtocol.BackColor = Color.White;
        txbProtocol.Cursor = Cursors.IBeam;
        txbProtocol.Dock = DockStyle.Fill;
        txbProtocol.Location = new Point(0, 32);
        txbProtocol.MultiLine = true;
        txbProtocol.Name = "txbProtocol";
        txbProtocol.Size = new Size(900, 468);
        txbProtocol.TabIndex = 1;
        txbProtocol.Text = "Bitte eine .cfo-, .bcr- oder .bdb-Datei auswählen.\r\n\r\nAblauf:\r\n- CFO/BCR: alt laden → JSON → alt laden → alt speichern → vergleichen\r\n- BDB: laden → als .tblj (JSON) speichern → laden → als .bdb speichern → vergleichen";
        txbProtocol.Verhalten = SteuerelementVerhalten.Scrollen_mit_Textumbruch;
        //
        // dlgOpen
        //
        dlgOpen.Filter = "Unterstützte Dateien|*.cfo;*.bcr;*.bdb|Alle Dateien|*.*";
        dlgOpen.Title = "Datei für Roundtrip-Test auswählen";
        //
        // JsonRoundtripTestForm
        //
        AutoScaleMode = AutoScaleMode.None;
        ClientSize = new Size(900, 500);
        Controls.Add(txbProtocol);
        Controls.Add(btnSelectFile);
        MinimumSize = new Size(500, 300);
        Name = "JsonRoundtripTestForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "JSON-Roundtrip-Test";
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    #region Fields

    private Button btnSelectFile;
    private TextBox txbProtocol;
    private OpenFileDialog dlgOpen;

    #endregion
}
