// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.Controls;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.Forms {
    public sealed partial class DependencyOverviewForm {
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            Pad = new CreativePad();
            capBestand = new Caption();
            pnlStatusBar.SuspendLayout();
            SuspendLayout();
            //
            // capStatusBar
            //
            capStatusBar.Location = new Point(360, 0);
            capStatusBar.Size = new Size(840, 24);
            //
            // pnlStatusBar
            //
            pnlStatusBar.Controls.Add(capBestand);
            pnlStatusBar.Location = new Point(0, 576);
            pnlStatusBar.Size = new Size(1200, 24);
            pnlStatusBar.Controls.SetChildIndex(capBestand, 0);
            pnlStatusBar.Controls.SetChildIndex(capStatusBar, 0);
            //
            // capBestand
            //
            capBestand.CausesValidation = false;
            capBestand.Dock = DockStyle.Left;
            capBestand.Location = new Point(0, 0);
            capBestand.Name = "capBestand";
            capBestand.Size = new Size(360, 24);
            capBestand.Translate = false;
            //
            // Pad
            //
            Pad.Dock = DockStyle.Fill;
            Pad.Location = new Point(0, 0);
            Pad.Name = "Pad";
            Pad.Size = new Size(1200, 576);
            Pad.TabIndex = 1;
            //
            // DependencyOverviewForm
            //
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1200, 600);
            Controls.Add(Pad);
            GlobalMenuHeight = 0;
            MinimumSize = new Size(600, 400);
            Name = "DependencyOverviewForm";
            StartPosition = FormStartPosition.CenterScreen;
            Tag = "";
            Text = "Abhängigkeits-Übersicht";
            Controls.SetChildIndex(pnlStatusBar, 0);
            Controls.SetChildIndex(Pad, 0);
            pnlStatusBar.ResumeLayout(false);
            ResumeLayout(false);

        }

        private CreativePad Pad;
        private Caption capBestand;
    }
}
