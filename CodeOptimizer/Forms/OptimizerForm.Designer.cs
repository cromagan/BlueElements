// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.Enums;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Button = BlueControls.Controls.Button;
using ListBox = BlueControls.Controls.ListBox;
using ScrollPanel = BlueControls.Controls.ScrollPanel;
using TextBox = BlueControls.Controls.TextBox;

namespace CodeOptimizer.Forms;

public partial class OptimizerForm {

    #region Methods

    /// <summary>
    /// Erforderliche Methode für den Designer. Der Inhalt kann mit dem Code-Editor nicht geändert werden.
    /// </summary>
    [DebuggerStepThrough]
    private void InitializeComponent() {
        txbPath = new TextBox();
        lstPlugins = new ListBox();
        pnlOptions = new ScrollPanel();
        btnStart = new Button();
        SuspendLayout();
        // 
        // txbPath
        // 
        txbPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txbPath.Location = new Point(8, 8);
        txbPath.Name = "txbPath";
        txbPath.Size = new Size(580, 27);
        txbPath.TabIndex = 0;
        // 
        // lstPlugins
        // 
        lstPlugins.AddAllowed = AddType.None;
        lstPlugins.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
        lstPlugins.Appearance = ListBoxAppearance.Listbox_Boxes;
        lstPlugins.CheckBehavior = CheckBehavior.MultiSelection;
        lstPlugins.Location = new Point(8, 43);
        lstPlugins.Name = "lstPlugins";
        lstPlugins.Size = new Size(290, 330);
        lstPlugins.TabIndex = 1;
        lstPlugins.ItemClicked += LstPlugins_ItemClicked;
        // 
        // pnlOptions
        // 
        pnlOptions.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        pnlOptions.Location = new Point(306, 43);
        pnlOptions.Name = "pnlOptions";
        pnlOptions.Size = new Size(282, 330);
        pnlOptions.TabIndex = 2;
        // 
        // btnStart
        // 
        btnStart.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        btnStart.ImageCode = "Abspielen|16";
        btnStart.Location = new Point(8, 381);
        btnStart.Name = "btnStart";
        btnStart.Size = new Size(580, 40);
        btnStart.TabIndex = 3;
        btnStart.Text = "Start";
        btnStart.Click += BtnStart_Click;
        // 
        // OptimizerForm
        // 
        AutoScaleMode = AutoScaleMode.None;
        ClientSize = new Size(596, 429);
        Controls.Add(txbPath);
        Controls.Add(lstPlugins);
        Controls.Add(pnlOptions);
        Controls.Add(btnStart);
        MinimumSize = new Size(420, 320);
        Name = "OptimizerForm";
        Text = "CodeOptimizer";
        ResumeLayout(false);
    }

    #endregion

    #region Fields

    internal TextBox txbPath;
    internal ListBox lstPlugins;
    internal ScrollPanel pnlOptions;
    internal Button btnStart;

    #endregion
}
