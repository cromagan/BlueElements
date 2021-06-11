﻿using BlueControls.Controls;
using BlueControls.Enums;
using System;
using static BlueBasics.Develop;
namespace BlueControls.Forms {
    public partial class DialogWithOkAndCancel : Form {
        private bool _cancelPossible = false;
        #region Konstruktor
        public DialogWithOkAndCancel() : this(enDesign.Form_MsgBox) {
        }
        public DialogWithOkAndCancel(enDesign design) : base(design) {
            InitializeComponent();
            SetTopLevel(true);
            if (Owner == null) {
                StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            }
        }
        #endregion
        public void Setup(int MinWidth, int BottomOfLowestControl, bool CancelPossible, bool Sizeable) {
            Text = AppName();
            _cancelPossible = CancelPossible;
            MinWidth = Math.Max(Width, MinWidth);
            Size = new System.Drawing.Size(MinWidth, BottomOfLowestControl + butOK.Height + BorderHeight + Skin.Padding);
            if (CancelPossible) {
                butAbbrechen.Left = MinWidth - Skin.Padding - butAbbrechen.Width - BorderWidth;
                butOK.Left = butAbbrechen.Left - Skin.Padding - butOK.Width;
            } else {
                butAbbrechen.Visible = false;
                butAbbrechen.Enabled = false;
                butOK.Left = MinWidth - Skin.Padding - butOK.Width - BorderWidth;
            }
            butOK.Top = BottomOfLowestControl;
            butAbbrechen.Top = BottomOfLowestControl;
            if (Sizeable) {
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            }
        }
        public void Setup(string TXT, GenericControl CenterControl, int MinWidth, bool CancelPossible, bool Sizeable) {
            var wi = Skin.Padding * 2;
            var he = Skin.Padding * 2;
            _cancelPossible = CancelPossible;
            if (!string.IsNullOrEmpty(TXT)) {
                capText.Visible = true;
                capText.Translate = false;
                capText.Text = TXT;
                capText.Refresh();
                wi += capText.Width;
                he += capText.Height;
            }
            if (CenterControl != null) {
                CenterControl.Top = he;
                he = he + CenterControl.Height + Skin.Padding;
            }
            wi = Math.Max(wi + BorderWidth, MinWidth);
            Setup(wi, he, CancelPossible, Sizeable);
            if (CenterControl != null) {
                CenterControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            }
        }
        /// <summary>
        /// Diese Routine wird aufgerufen, nachdem OK oder Cancel gedrückt wurde.
        /// </summary>
        protected virtual void SetValue(bool canceled) => DebugPrint_RoutineMussUeberschriebenWerden();
        protected void Ok() {
            SetValue(false);
            Close();
        }
        protected void Cancel() {
            SetValue(true);
            Close();
        }
        private void butAbbrechen_Click(object sender, System.EventArgs e) => Cancel();
        protected bool OK_Enabled {
            get => butOK.Enabled;
            set => butOK.Enabled = value;
        }
        private void butOK_Click(object sender, System.EventArgs e) => Ok();
        /// <summary>
        /// Must handle some layout operations manually because Visual Studio 
        /// 2005 arbitrarily changes some properties of inherited controls.
        /// </summary>
        /// <param name="e">Data for event.</param>
        protected override void OnResize(System.EventArgs e) {
            base.OnResize(e);
            // https://stackoverflow.com/questions/4971768/incorrect-behavior-of-panel-on-inherited-windows-form
            if (butOK != null) {
                if (_cancelPossible) {
                    butOK.Top = Height - 87;
                    butOK.Left = Width - 193;
                    butAbbrechen.Top = butOK.Top;
                    butAbbrechen.Left = butOK.Right + Skin.Padding;
                } else {
                    butOK.Top = Height - 87;
                    butOK.Left = Width - 193 + butAbbrechen.Width + Skin.Padding;
                    butAbbrechen.Visible = false;
                }
            }
        }
    }
}
