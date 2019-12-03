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
            this.lstActionSelector = new ListBox();
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
            //lstActionSelector
            //
            this.lstActionSelector.AddAllowed = enAddType.UserDef;
            this.lstActionSelector.Anchor = (System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left);
            this.lstActionSelector.CheckBehavior = enCheckBehavior.AlwaysSingleSelection;
            this.lstActionSelector.FilterAllowed = true;
            this.lstActionSelector.Location = new Point(7, 16);
            this.lstActionSelector.Name = "lstActionSelector";
            this.lstActionSelector.QuickInfo = "";
            this.lstActionSelector.RemoveAllowed = true;
            this.lstActionSelector.Size = new Size(472, 216);
            this.lstActionSelector.TabIndex = 5;
            this.lstActionSelector.AddClicked += new EventHandler(ActionSelector_AddClicked);
            this.lstActionSelector.ItemCheckedChanged += new EventHandler(lstActionSelector_ItemCheckedChanged);
            this.lstActionSelector.ItemRemoving += lstActionSelector_ItemRemoving;
            //
            //RuleItem_Editor
            //
            this.Controls.Add(this.RuleActionEditor);
            this.Controls.Add(this.lstActionSelector);
            this.Name = "RuleItem_Editor";
            this.Size = new Size(1023, 243);
            this.ResumeLayout(false);
        }

        internal RuleActionItem_Editor RuleActionEditor;
        private ListBox lstActionSelector;
    }
}

