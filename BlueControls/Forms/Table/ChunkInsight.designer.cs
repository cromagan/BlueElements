// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Button = BlueControls.Controls.Button;

namespace BlueControls.BlueTableDialogs {
    public sealed partial class ChunkInsight {
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            tblChunk = new TableViewWithFilters();
            pnlHeader = new Panel();
            capInfo = new Caption();
            btnOeffnen = new Button();
            OpenTab = new OpenFileDialog();
            capZeilen2 = new Caption();
            pnlStatusBar.SuspendLayout();
            pnlHeader.SuspendLayout();
            SuspendLayout();
            // 
            // capStatusBar
            // 
            capStatusBar.Location = new Point(208, 0);
            capStatusBar.Size = new Size(776, 24);
            // 
            // pnlStatusBar
            // 
            pnlStatusBar.Controls.Add(capZeilen2);
            pnlStatusBar.Location = new Point(0, 545);
            pnlStatusBar.Size = new Size(984, 24);
            pnlStatusBar.Controls.SetChildIndex(capZeilen2, 0);
            pnlStatusBar.Controls.SetChildIndex(capStatusBar, 0);
            // 
            // tblChunk
            // 
            tblChunk.Ansichtbearbeitung = false;
            tblChunk.Dock = DockStyle.Fill;
            tblChunk.Location = new Point(0, 31);
            tblChunk.Name = "tblChunk";
            tblChunk.PowerEdit = false;
            tblChunk.Size = new Size(984, 514);
            tblChunk.TabIndex = 1;
            tblChunk.VisibleRowsChanged += Table_VisibleRowsChanged;
            // 
            // pnlHeader
            // 
            pnlHeader.Controls.Add(capInfo);
            pnlHeader.Controls.Add(btnOeffnen);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(984, 31);
            pnlHeader.TabIndex = 0;
            // 
            // capInfo
            // 
            capInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            capInfo.CausesValidation = false;
            capInfo.Location = new Point(136, 0);
            capInfo.Name = "capInfo";
            capInfo.Size = new Size(840, 24);
            capInfo.Text = "Kein Chunk geladen";
            capInfo.Translate = false;
            // 
            // btnOeffnen
            // 
            btnOeffnen.ImageCode = "Ordner|16";
            btnOeffnen.Location = new Point(8, 0);
            btnOeffnen.Name = "btnOeffnen";
            btnOeffnen.QuickInfo = "Chunk-Datei (.bdbc, .cbdb, .bdb, .mbdb, .hbdb, .chk, .cfbdb) auswählen";
            btnOeffnen.Size = new Size(120, 24);
            btnOeffnen.TabIndex = 0;
            btnOeffnen.Text = "Chunk öffnen";
            btnOeffnen.Click += btnOeffnen_Click;
            // 
            // OpenTab
            // 
            OpenTab.Filter = "Chunk-Dateien|*.bdbc;*.cbdb;*.bdb;*.mbdb;*.hbdb;*.chk;*.cfbdb|Alle Dateien|*.*";
            OpenTab.Title = "Chunk-Datei öffnen";
            // 
            // capZeilen2
            // 
            capZeilen2.CausesValidation = false;
            capZeilen2.Dock = DockStyle.Left;
            capZeilen2.Location = new Point(0, 0);
            capZeilen2.Name = "capZeilen2";
            capZeilen2.Size = new Size(208, 24);
            capZeilen2.Translate = false;
            // 
            // ChunkInsight
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(984, 569);
            Controls.Add(tblChunk);
            Controls.Add(pnlHeader);
            GlobalMenuHeight = 0;
            MinimumSize = new Size(600, 400);
            Name = "ChunkInsight";
            StartPosition = FormStartPosition.CenterScreen;
            Tag = "";
            Text = "Chunk-Insight";
            Controls.SetChildIndex(pnlHeader, 0);
            Controls.SetChildIndex(pnlStatusBar, 0);
            Controls.SetChildIndex(tblChunk, 0);
            pnlStatusBar.ResumeLayout(false);
            pnlHeader.ResumeLayout(false);
            ResumeLayout(false);

        }

        private TableViewWithFilters tblChunk;
        private Panel pnlHeader;
        private Button btnOeffnen;
        private Caption capInfo;
        private OpenFileDialog OpenTab;
        private System.ComponentModel.IContainer components;
        private Caption capZeilen2;
    }
}
