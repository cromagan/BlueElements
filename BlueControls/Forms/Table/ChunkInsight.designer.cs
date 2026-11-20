// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

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
            fragmentInsight = new FragmentEditor();
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
            // fragmentInsight
            // 
            fragmentInsight.Dock = DockStyle.Fill;
            fragmentInsight.Location = new Point(0, 31);
            fragmentInsight.Name = "fragmentInsight";
            fragmentInsight.Size = new Size(984, 514);
            fragmentInsight.TabIndex = 1;
            fragmentInsight.VisibleRowsChanged += FragmentInsight_VisibleRowsChanged;
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
            capInfo.Text = "Keine Fragment-Daten geladen";
            capInfo.Translate = false;
            // 
            // btnOeffnen
            // 
            btnOeffnen.ImageCode = "Ordner|16";
            btnOeffnen.Location = new Point(8, 0);
            btnOeffnen.Name = "btnOeffnen";
            btnOeffnen.QuickInfo = "Fragment-Datei (.btb, .tbls, .tblf) auswählen";
            btnOeffnen.Size = new Size(120, 24);
            btnOeffnen.TabIndex = 0;
            btnOeffnen.Text = "Öffnen";
            btnOeffnen.Click += btnOeffnen_Click;
            // 
            // OpenTab
            // 
            OpenTab.Filter = "Fragment-Dateien|*.btb;*.tbls;*.tblf|Alle Dateien|*.*";
            OpenTab.Title = "Fragment-Datei öffnen";
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
            Controls.Add(fragmentInsight);
            Controls.Add(pnlHeader);
            GlobalMenuHeight = 0;
            MinimumSize = new Size(600, 400);
            Name = "ChunkInsight";
            StartPosition = FormStartPosition.CenterScreen;
            Tag = "";
            Text = "Fragment-Insight";
            Controls.SetChildIndex(pnlHeader, 0);
            Controls.SetChildIndex(pnlStatusBar, 0);
            Controls.SetChildIndex(fragmentInsight, 0);
            pnlStatusBar.ResumeLayout(false);
            pnlHeader.ResumeLayout(false);
            ResumeLayout(false);

        }

        private FragmentEditor fragmentInsight;
        private Panel pnlHeader;
        private Button btnOeffnen;
        private Caption capInfo;
        private OpenFileDialog OpenTab;
        private System.ComponentModel.IContainer components;
        private Caption capZeilen2;
    }
}
