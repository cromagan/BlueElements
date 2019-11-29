using System;
using System.Diagnostics;
using System.Drawing;
using BlueControls.Controls;
using BlueControls.Enums;

namespace BlueControls.Classes_Editor
{
    internal partial class RuleItem_Editor : AbstractClassEditor
    {
        //UserControl überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {

                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }



        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.RuleActionEditor = new RuleActionItem_Editor();
            this.ActionSelector = new ListBox();
            this.SuspendLayout();
            //
            //RuleActionEditor
            //
            this.RuleActionEditor.Anchor = (System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
            this.RuleActionEditor.CausesValidation = false;
            this.RuleActionEditor.Location = new Point(487, 16);
            this.RuleActionEditor.Name = "RuleActionEditor";
            this.RuleActionEditor.ObjectWithDialog = null;
            this.RuleActionEditor.Size = new Size(528, 216);
            this.RuleActionEditor.Text = "Aktion-Editor";
            this.RuleActionEditor.Changed += new EventHandler(RuleActionEditor_Changed);
            //
            //ActionSelector
            //
            this.ActionSelector.AddAllowed = enAddType.UserDef;
            this.ActionSelector.Anchor = (System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left);
            this.ActionSelector.CheckBehavior = enCheckBehavior.AlwaysSingleSelection;
            this.ActionSelector.FilterAllowed = true;
            this.ActionSelector.Location = new Point(7, 16);
            this.ActionSelector.Name = "ActionSelector";
            this.ActionSelector.QuickInfo = "";
            this.ActionSelector.RemoveAllowed = true;
            this.ActionSelector.Size = new Size(472, 216);
            this.ActionSelector.TabIndex = 5;
            this.ActionSelector.AddClicked += new EventHandler(ActionSelector_AddClicked);
            this.ActionSelector.ItemCheckedChanged += new EventHandler(ActionSelector_Item_CheckedChanged);
            this.ActionSelector.ItemRemoving += ActionSelector_ItemRemoving;
            //
            //RuleItem_Editor
            //
            this.Controls.Add(this.RuleActionEditor);
            this.Controls.Add(this.ActionSelector);
            this.Name = "RuleItem_Editor";
            this.Size = new Size(1023, 243);
            this.ResumeLayout(false);
        }

        internal RuleActionItem_Editor RuleActionEditor;
        private ListBox ActionSelector;
    }
}

