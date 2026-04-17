using BlueControls.Controls;
using System.ComponentModel;
using System.Windows.Forms;

namespace BlueControls.Forms
{
    partial class QuickInfo
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;


        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.capText = new BlueControls.Controls.Caption();
            this.SuspendLayout();
            // 
            // capText
            // 
            this.capText.CausesValidation = false;
            this.capText.Location = new System.Drawing.Point(8, 8);
            this.capText.Name = "capText";
            this.capText.Size = new System.Drawing.Size(16, 16);
            this.capText.Translate = false;
            // 
            // QuickInfo
            // 
            this.ClientSize = new System.Drawing.Size(86, 65);
            this.Controls.Add(this.capText);
            this.Name = "QuickInfo";
            this.Text = "QuickInfo";
            this.ResumeLayout(false);
        }
        #endregion

        private Caption capText;
    }
}