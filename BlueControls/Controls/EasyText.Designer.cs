// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.Enums;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace BlueControls.Controls
{
    public partial class EasyText
    {

        //Wird vom Windows Form-Designer benötigt.
        private IContainer components;
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.EditPanelFrame = new GroupBox();
            this.btnCopy = new Button();
            this.btnPaste = new Button();
            this.txbText = new TextBox();
            this.EditPanelFrame.SuspendLayout();
            this.SuspendLayout();
            //
            // EditPanelFrame
            //
            this.EditPanelFrame.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
                                                          | AnchorStyles.Right)));
            this.EditPanelFrame.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(245)))), ((int)(((byte)(246)))));
            this.EditPanelFrame.CausesValidation = false;
            this.EditPanelFrame.Controls.Add(this.btnPaste);
            this.EditPanelFrame.Controls.Add(this.btnCopy);
            this.EditPanelFrame.GroupBoxStyle = GroupBoxStyle.RoundRect;
            this.EditPanelFrame.Location = new Point(0, 0);
            this.EditPanelFrame.Name = "EditPanelFrame";
            this.EditPanelFrame.Size = new Size(472, 40);
            this.EditPanelFrame.TabIndex = 0;
            this.EditPanelFrame.TabStop = false;
            this.EditPanelFrame.Visible = false;
            //
            // btnCopy
            //
            this.btnCopy.ImageCode = "Kopieren|28";
            this.btnCopy.Location = new Point(0, 2);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.QuickInfo = "Den gesamten Inhalt in die<br>Zwischenablage kopieren.";
            this.btnCopy.Size = new Size(38, 38);
            this.btnCopy.TabIndex = 0;
            this.btnCopy.Click += new EventHandler(this.BtnCopy_Click);
            //
            // btnPaste
            //
            this.btnPaste.ImageCode = "Clipboard|28";
            this.btnPaste.Location = new Point(40, 2);
            this.btnPaste.Name = "btnPaste";
            this.btnPaste.QuickInfo = "Den Inhalt der Zwischenablage einfügen<br>und den bisherigen Text ersetzen.";
            this.btnPaste.Size = new Size(38, 38);
            this.btnPaste.TabIndex = 1;
            this.btnPaste.Click += new EventHandler(this.BtnPaste_Click);
            //
            // txbText
            //
            this.txbText.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.txbText.Cursor = Cursors.IBeam;
            this.txbText.Location = new Point(1, 1);
            this.txbText.Name = "txbText";
            this.txbText.Size = new Size(470, 406);
            this.txbText.TabIndex = 2;
            this.txbText.TextChanged += new EventHandler(this.TxbText_TextChanged);
            //
            // EasyText
            //
            this.Controls.Add(this.txbText);
            this.Controls.Add(this.EditPanelFrame);
            this.Name = "EasyText";
            this.Size = new Size(472, 408);
            this.EditPanelFrame.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private Button btnCopy;
        private Button btnPaste;
        private GroupBox EditPanelFrame;
        private TextBox txbText;
    }
}
