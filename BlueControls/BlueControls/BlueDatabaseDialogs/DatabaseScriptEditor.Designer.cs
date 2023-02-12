using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using BlueControls.Classes_Editor;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.EventArgs;
using BlueControls.Forms;
using Button = BlueControls.Controls.Button;
using Form = BlueControls.Forms.Form;
using GroupBox = BlueControls.Controls.GroupBox;
using ListBox = BlueControls.Controls.ListBox;
using TabControl = BlueControls.Controls.TabControl;
using TabPage = System.Windows.Forms.TabPage;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls.BlueDatabaseDialogs
{
    public sealed partial class DatabaseScriptEditor : FormWithStatusBar
    {
        //Das Formular überschreibt den Deletevorgang, um die Komponentenliste zu bereinigen.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DatabaseScriptEditor));
            this.btnOk = new BlueControls.Controls.Button();
            this.GlobalTab = new BlueControls.Controls.TabControl();
            this.tabVariablen = new System.Windows.Forms.TabPage();
            this.variableEditor = new BlueControls.VariableEditor();
            this.tabScripts = new System.Windows.Forms.TabPage();
            this.eventScriptEditor = new BlueControls.Classes_Editor.EventScript_Editor();
            this.grpVerfügbareSkripte = new BlueControls.Controls.GroupBox();
            this.lstEventScripts = new BlueControls.Controls.ListBox();
            this.btnSave = new BlueControls.Controls.Button();
            this.GlobalTab.SuspendLayout();
            this.tabVariablen.SuspendLayout();
            this.tabScripts.SuspendLayout();
            this.grpVerfügbareSkripte.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(973, 651);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(72, 24);
            this.btnOk.TabIndex = 11;
            this.btnOk.Text = "OK";
            this.btnOk.Click += new System.EventHandler(this.OkBut_Click);
            // 
            // GlobalTab
            // 
            this.GlobalTab.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GlobalTab.Controls.Add(this.tabScripts);
            this.GlobalTab.Controls.Add(this.tabVariablen);
            this.GlobalTab.HotTrack = true;
            this.GlobalTab.Location = new System.Drawing.Point(0, 0);
            this.GlobalTab.Name = "GlobalTab";
            this.GlobalTab.RowKey = ((long)(-1));
            this.GlobalTab.SelectedIndex = 0;
            this.GlobalTab.Size = new System.Drawing.Size(1047, 645);
            this.GlobalTab.TabDefault = null;
            this.GlobalTab.TabDefaultOrder = null;
            this.GlobalTab.TabIndex = 21;
            this.GlobalTab.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.GlobalTab_Selecting);
            // 
            // tabVariablen
            // 
            this.tabVariablen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabVariablen.Controls.Add(this.variableEditor);
            this.tabVariablen.Location = new System.Drawing.Point(4, 25);
            this.tabVariablen.Name = "tabVariablen";
            this.tabVariablen.Padding = new System.Windows.Forms.Padding(3);
            this.tabVariablen.Size = new System.Drawing.Size(1039, 616);
            this.tabVariablen.TabIndex = 3;
            this.tabVariablen.Text = "Variablen";
            // 
            // variableEditor
            // 
            this.variableEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.variableEditor.Editabe = true;
            this.variableEditor.Location = new System.Drawing.Point(3, 3);
            this.variableEditor.Name = "variableEditor";
            this.variableEditor.Size = new System.Drawing.Size(1033, 610);
            this.variableEditor.TabIndex = 0;
            // 
            // tabScripts
            // 
            this.tabScripts.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabScripts.Controls.Add(this.eventScriptEditor);
            this.tabScripts.Controls.Add(this.grpVerfügbareSkripte);
            this.tabScripts.Location = new System.Drawing.Point(4, 25);
            this.tabScripts.Name = "tabScripts";
            this.tabScripts.Padding = new System.Windows.Forms.Padding(3);
            this.tabScripts.Size = new System.Drawing.Size(1039, 616);
            this.tabScripts.TabIndex = 7;
            this.tabScripts.Text = "Skripte";
            // 
            // eventScriptEditor
            // 
            this.eventScriptEditor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.eventScriptEditor.Database = null;
            this.eventScriptEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.eventScriptEditor.Location = new System.Drawing.Point(240, 3);
            this.eventScriptEditor.Name = "eventScriptEditor";
            this.eventScriptEditor.Size = new System.Drawing.Size(796, 610);
            this.eventScriptEditor.TabIndex = 3;
            this.eventScriptEditor.TabStop = false;
            this.eventScriptEditor.Text = "Skript-Editor";
            // 
            // grpVerfügbareSkripte
            // 
            this.grpVerfügbareSkripte.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.grpVerfügbareSkripte.CausesValidation = false;
            this.grpVerfügbareSkripte.Controls.Add(this.lstEventScripts);
            this.grpVerfügbareSkripte.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpVerfügbareSkripte.Location = new System.Drawing.Point(3, 3);
            this.grpVerfügbareSkripte.Name = "grpVerfügbareSkripte";
            this.grpVerfügbareSkripte.Size = new System.Drawing.Size(237, 610);
            this.grpVerfügbareSkripte.TabIndex = 2;
            this.grpVerfügbareSkripte.TabStop = false;
            this.grpVerfügbareSkripte.Text = "Verfügbare Skripte:";
            // 
            // lstEventScripts
            // 
            this.lstEventScripts.AddAllowed = BlueControls.Enums.AddType.UserDef;
            this.lstEventScripts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstEventScripts.FilterAllowed = true;
            this.lstEventScripts.Location = new System.Drawing.Point(8, 24);
            this.lstEventScripts.Name = "lstEventScripts";
            this.lstEventScripts.RemoveAllowed = true;
            this.lstEventScripts.Size = new System.Drawing.Size(222, 578);
            this.lstEventScripts.TabIndex = 0;
            this.lstEventScripts.AddClicked += new System.EventHandler(this.lstEventScripts_AddClicked);
            this.lstEventScripts.ItemCheckedChanged += new System.EventHandler(this.lstEventScripts_ItemCheckedChanged);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.ImageCode = "Diskette|16";
            this.btnSave.Location = new System.Drawing.Point(840, 651);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(112, 24);
            this.btnSave.TabIndex = 22;
            this.btnSave.Text = "Speichern";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // DatabaseScriptEditor
            // 
            this.ClientSize = new System.Drawing.Size(1050, 677);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.GlobalTab);
            this.Controls.Add(this.btnOk);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimizeBox = false;
            this.Name = "DatabaseScriptEditor";
            this.ShowInTaskbar = false;
            this.Text = "Datenbank-Eigenschaften";
            this.GlobalTab.ResumeLayout(false);
            this.tabVariablen.ResumeLayout(false);
            this.tabScripts.ResumeLayout(false);
            this.grpVerfügbareSkripte.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private Button btnOk;
        private  TabPage tabVariablen;
        private TabControl GlobalTab;
        private Button btnSave;
        private TabPage tabScripts;
        private GroupBox grpVerfügbareSkripte;
        private ListBox lstEventScripts;
        private EventScript_Editor eventScriptEditor;
        private VariableEditor variableEditor;
    }
}
