// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.BlueTableDialogs;
using BlueControls.Editoren;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Button = BlueControls.Controls.Button;

namespace BlueControls.BlueTableDialogs {
    public sealed partial class TableScriptEditorForm {
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            tableScriptEditor = new TableScriptEditor();
            lstEventScripts = new EditorForIEnumerable();
            btnVerlauf = new Button();
            SuspendLayout();
            // 
            // tableScriptEditor
            // 
            tableScriptEditor.Dock = DockStyle.Fill;
            tableScriptEditor.LastFailedReason = "";
            tableScriptEditor.LastVariables = null;
            tableScriptEditor.Location = new Point(237, 0);
            tableScriptEditor.Name = "tableScriptEditor";
            tableScriptEditor.Script = "";
            tableScriptEditor.Size = new Size(1015, 610);
            tableScriptEditor.StoppedTimeCount = 0;
            tableScriptEditor.TabIndex = 0;
            tableScriptEditor.Table = null;
            tableScriptEditor.VariableDefinitions = "Attribut0, Attribut1, Attribut2, Attribut3, Attribut4, Attribut5";
            // 
            // lstEventScripts
            // 
            lstEventScripts.Dock = DockStyle.Left;
            lstEventScripts.Editor = null;
            lstEventScripts.InputItem = null;
            lstEventScripts.Location = new Point(0, 0);
            lstEventScripts.Name = "lstEventScripts";
            lstEventScripts.Size = new Size(237, 610);
            lstEventScripts.TabIndex = 1;
            lstEventScripts.AddClicked += LstEventScripts_AddClicked;
            lstEventScripts.ListBuilt += LstEventScripts_ListBuilt;
            // 
            // btnVerlauf
            // 
            btnVerlauf.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnVerlauf.ImageCode = "Undo|16";
            btnVerlauf.Location = new Point(1148, 8);
            btnVerlauf.Name = "btnVerlauf";
            btnVerlauf.QuickInfo = "Zeigt den Verlauf in einem separatem Fenster an";
            btnVerlauf.Size = new Size(100, 24);
            btnVerlauf.TabIndex = 2;
            btnVerlauf.Text = "Verlauf";
            btnVerlauf.Click += btnVerlauf_Click;
            // 
            // TableScriptEditorForm
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1252, 610);
            Controls.Add(btnVerlauf);
            Controls.Add(tableScriptEditor);
            Controls.Add(lstEventScripts);
            MinimumSize = new Size(600, 400);
            Name = "TableScriptEditorForm";
            Text = "Skript-Editor";
            WindowState = FormWindowState.Maximized;
            ResumeLayout(false);

        }

        private TableScriptEditor tableScriptEditor;
        private EditorForIEnumerable lstEventScripts;
        private Button btnVerlauf;
    }
}
