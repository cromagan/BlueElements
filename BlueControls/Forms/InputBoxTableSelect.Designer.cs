using BlueControls.Controls;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.Forms {
    partial class InputBoxTableSelect {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing) {
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            _tableView = new TableView();
            SuspendLayout();
            // 
            // _tableView
            // 
            _tableView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _tableView.Location = new Point(8, 24);
            _tableView.Name = "_tableView";
            _tableView.Size = new Size(680, 320);
            _tableView.TabIndex = 4;
            _tableView.Text = "tableView1";
            _tableView.CellClicked += _tableView_CellClicked;
            // 
            // InputBoxTableSelect
            // 
            ClientSize = new Size(694, 400);
            CloseButtonEnabled = true;
            Controls.Add(_tableView);
            Name = "InputBoxTableSelect";
            Controls.SetChildIndex(_tableView, 0);
            ResumeLayout(false);
        }

        #endregion


        private TableView _tableView;
    }
}
