// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.EventArgs;
using BlueTable.Enums;
using System;
using System.ComponentModel;
using System.Drawing;

namespace BlueControls.Controls{
    partial class FlexiControlForFilter {
        #region Vom Komponenten-Designer generierter Code
        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent() {
            this.f = new FlexiControl();
            this.SuspendLayout();
            // 
            // f
            // 
            this.f.Dock = System.Windows.Forms.DockStyle.Fill;
            this.f.EditType = EditTypeFormula.Line;
            this.f.Location = new System.Drawing.Point(0, 0);
            this.f.Name = "f";
            this.f.Size = new System.Drawing.Size(150, 150);
            this.f.TabIndex = 0;
            this.f.DropDownShowing += new EventHandler(this.Cbx_DropDownShowing);
            this.f.ItemRemoved += new System.EventHandler<AbstractListItemEventArgs>(this.Cbx_ItemRemoved);
            this.f.ExecuteComand += new EventHandler(this.F_ExecuteComand);
            this.f.ValueChanged += new EventHandler(this.F_ValueChanged);
            this.f.NavigateToNext += new System.EventHandler<BlueControls.EventArgs.NavigationDirectionEventArgs>(this.F_NavigateToNext);            
            // 
            // FlexiControlForFilter
            // 
            this.Controls.Add(this.f);
            this.Name = "FlexiControlForFilter";
            this.ResumeLayout(false);

        }
        #endregion

        private FlexiControl f;
    }
}
