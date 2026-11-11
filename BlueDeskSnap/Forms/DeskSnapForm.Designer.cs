// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.ComponentModel;

namespace BlueDeskSnap.Forms
{
    partial class DeskSnapForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components is not null)) {
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
            components = new Container();
            grpNeu = new GroupBox();
            btnNeu = new Button();
            btnMinimizeAll = new Button();
            btnMenu = new Button();
            lstSnaps = new ListBox();
            lstQuality = new ListBox();
            tmrQuality = new System.Windows.Forms.Timer(components);
            grpNeu.SuspendLayout();
            SuspendLayout();
            //
            // grpNeu
            //
            grpNeu.Controls.Add(btnNeu);
            grpNeu.Controls.Add(btnMinimizeAll);
            grpNeu.Controls.Add(btnMenu);
            grpNeu.Dock = System.Windows.Forms.DockStyle.Top;
            grpNeu.Location = new Point(0, 0);
            grpNeu.Name = "grpNeu";
            grpNeu.Size = new Size(181, 42);
            grpNeu.TabIndex = 1;
            grpNeu.TabStop = false;
            //
            // btnNeu
            //
            btnNeu.ImageCode = "PlusZeichen|16";
            btnNeu.Location = new Point(4, 16);
            btnNeu.Name = "btnNeu";
            btnNeu.QuickInfo = "Erfasst die aktuelle Fensteranordnung als Snap";
            btnNeu.Size = new Size(117, 24);
            btnNeu.TabIndex = 0;
            btnNeu.Text = "Snap erfassen";
            btnNeu.Click += BtnNeu_Click;
            //
            // btnMinimizeAll
            //
            btnMinimizeAll.ImageCode = "MinusZeichen|16";
            btnMinimizeAll.Location = new Point(123, 16);
            btnMinimizeAll.Name = "btnMinimizeAll";
            btnMinimizeAll.QuickInfo = "Minimiert alle Fenster außer diesem Werkzeug";
            btnMinimizeAll.Size = new Size(26, 24);
            btnMinimizeAll.TabIndex = 3;
            btnMinimizeAll.Click += BtnMinimizeAll_Click;
            //
            // btnMenu
            //
            btnMenu.ImageCode = "Stern|16";
            btnMenu.Location = new Point(151, 16);
            btnMenu.Name = "btnMenu";
            btnMenu.QuickInfo = "Öffnet das Auswahlmenü";
            btnMenu.Size = new Size(26, 24);
            btnMenu.TabIndex = 4;
            btnMenu.Click += BtnMenu_Click;
            //
            // lstSnaps
            //
            lstSnaps.AddAllowed = AddType.None;
            lstSnaps.AutoSort = false;
            lstSnaps.CheckBehavior = CheckBehavior.UserLockedSingleSelection;
            lstSnaps.Dock = System.Windows.Forms.DockStyle.Fill;
            lstSnaps.Location = new Point(0, 42);
            lstSnaps.MoveAllowed = true;
            lstSnaps.Name = "lstSnaps";
            lstSnaps.RemoveAllowed = true;
            lstSnaps.Size = new Size(181, 258);
            lstSnaps.TabIndex = 0;
            lstSnaps.ItemClicked += LstSnaps_ItemClicked;
            lstSnaps.RemoveClicked += LstSnaps_RemoveClicked;
            lstSnaps.UpDownClicked += LstSnaps_UpDownClicked;
            //
            // lstQuality
            //
            lstQuality.AddAllowed = AddType.None;
            lstQuality.Appearance = ListBoxAppearance.Listbox_Boxes;
            lstQuality.AutoSort = true;
            lstQuality.CheckBehavior = CheckBehavior.MultiSelection;
            lstQuality.Dock = System.Windows.Forms.DockStyle.Bottom;
            lstQuality.Location = new Point(0, 208);
            lstQuality.Name = "lstQuality";
            lstQuality.RemoveAllowed = false;
            lstQuality.Size = new Size(181, 92);
            lstQuality.TabIndex = 2;
            lstQuality.ItemCheckedChanged += LstQuality_CheckedChanged;
            //
            // tmrQuality
            //
            tmrQuality.Enabled = true;
            tmrQuality.Interval = 3000;
            tmrQuality.Tick += TmrQuality_Tick;
            //
            // DeskSnapForm
            //
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            ClientSize = new Size(181, 300);
            Controls.Add(lstSnaps);
            Controls.Add(grpNeu);
            Controls.Add(lstQuality);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            Name = "DeskSnapForm";
            Text = "Desktop-Snaps";
            TopMost = true;
            ResumeLayout(false);
        }

        #endregion

        private BlueControls.Controls.ListBox lstSnaps;
        private BlueControls.Controls.GroupBox grpNeu;
        private BlueControls.Controls.Button btnNeu;
        private BlueControls.Controls.Button btnMinimizeAll;
        private BlueControls.Controls.Button btnMenu;
        private BlueControls.Controls.ListBox lstQuality;
        private System.Windows.Forms.Timer tmrQuality;
    }
}
