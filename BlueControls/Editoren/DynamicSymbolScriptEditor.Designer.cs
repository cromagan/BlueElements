// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.BlueTableDialogs {
    public sealed partial class DynamicSymbolScriptEditor {
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            this.pnlStatusBar.SuspendLayout();
            this.SuspendLayout();
            //
            // DynamicSymbolScriptEditor
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.VariableDefinitions = "Attribut0, Attribut1, Attribut2, Attribut3, Attribut4, Attribut5";
            this.Name = "DynamicSymbolScriptEditor";
            this.Text = "Tabellen-Eigenschaften";
            this.pnlStatusBar.ResumeLayout(false);
            this.ResumeLayout(false);

        }

    }
}
