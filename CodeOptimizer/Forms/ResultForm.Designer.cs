// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace CodeOptimizer.Forms;

public partial class ResultForm {

    #region Methods

    /// <summary>
    /// Erforderliche Methode für den Designer. Der Inhalt kann mit dem Code-Editor nicht geändert werden.
    /// </summary>
    [DebuggerStepThrough]
    private void InitializeComponent() {
        tvwResult = new TableView();
        SuspendLayout();
        //
        // tvwResult
        //
        tvwResult.Dock = DockStyle.Fill;
        tvwResult.Location = new Point(0, 0);
        tvwResult.Name = "tvwResult";
        tvwResult.Size = new Size(784, 561);
        tvwResult.TabIndex = 0;
        //
        // ResultForm
        //
        ClientSize = new Size(784, 561);
        Controls.Add(tvwResult);
        MinimumSize = new Size(500, 300);
        Name = "ResultForm";
        Text = "Ergebnis";
        ResumeLayout(false);
    }

    #endregion

    private TableView tvwResult;
}
