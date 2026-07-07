// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.Enums;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Button = BlueControls.Controls.Button;
using GroupBox = BlueControls.Controls.GroupBox;
using TextBox = BlueControls.Controls.TextBox;

namespace BlueControls.BlueTableDialogs {
    public sealed partial class RowAdderScriptEditor  {
        //Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
        //Das Bearbeiten ist mit dem Windows Form-Designer möglich.
        //Das Bearbeiten mit dem Code-Editor ist nicht möglich.
        [DebuggerStepThrough()]
        private void InitializeComponent() {
            var resources = new ComponentResourceManager(typeof(RowAdderScriptEditor));
            btnTabelleKopf = new Button();
            grpScripte = new GroupBox();
            capInfo = new Caption();
            capInfo2 = new Caption();
            capScriptMenu = new Caption();
            btnScriptAfter = new Button();
            btnScriptMenu = new Button();
            btnScriptBefore = new Button();
            txbTestZeile = new TextBox();
            btnTestZeileDropDown = new Button();
            grpRow = new GroupBox();
            txbChunk = new TextBox();
            btnChunkDropDown = new Button();
            caption1 = new Caption();
            capChunk = new Caption();
            tabStart.SuspendLayout();
            pnlStatusBar.SuspendLayout();
            grpScripte.SuspendLayout();
            grpRow.SuspendLayout();
            SuspendLayout();
            // 
            // tbcScriptEigenschaften
            // 
            tbcScriptEigenschaften.Location = new Point(290, 128);
            tbcScriptEigenschaften.Size = new Size(494, 409);
            // 
            // tabStart
            // 
            tabStart.Controls.Add(btnTabelleKopf);
            tabStart.Controls.SetChildIndex(btnSaveLoad, 0);
            tabStart.Controls.SetChildIndex(btnBefehlsUebersicht, 0);
            tabStart.Controls.SetChildIndex(btnAusführen, 0);
            tabStart.Controls.SetChildIndex(btnTabelleKopf, 0);
            // 
            // grpInjectVariables
            // 
            grpInjectVariables.Location = new Point(0, 88);
            // 
            // capStatusBar
            // 
            capStatusBar.Size = new Size(494, 24);
            // 
            // pnlStatusBar
            // 
            pnlStatusBar.Location = new Point(290, 537);
            pnlStatusBar.Size = new Size(494, 24);
            // 
            // btnTabelleKopf
            // 
            btnTabelleKopf.ImageCode = "Tabelle|16||||||||Stift";
            btnTabelleKopf.Location = new Point(368, 8);
            btnTabelleKopf.Name = "btnTabelleKopf";
            btnTabelleKopf.Size = new Size(120, 24);
            btnTabelleKopf.TabIndex = 46;
            btnTabelleKopf.Text = "Tabellen-Kopf";
            btnTabelleKopf.Click += btnTabelleKopf_Click;
            // 
            // grpScripte
            // 
            grpScripte.BackColor = Color.FromArgb(240, 240, 240);
            grpScripte.Controls.Add(capInfo);
            grpScripte.Controls.Add(capInfo2);
            grpScripte.Controls.Add(capScriptMenu);
            grpScripte.Controls.Add(btnScriptAfter);
            grpScripte.Controls.Add(btnScriptMenu);
            grpScripte.Controls.Add(btnScriptBefore);
            grpScripte.Dock = DockStyle.Left;
            grpScripte.Location = new Point(0, 128);
            grpScripte.Name = "grpScripte";
            grpScripte.Size = new Size(290, 433);
            grpScripte.TabIndex = 99;
            grpScripte.TabStop = false;
            grpScripte.Text = "Scripte";
            // 
            // capInfo
            // 
            capInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            capInfo.CausesValidation = false;
            capInfo.Location = new Point(8, 448);
            capInfo.Name = "capInfo";
            capInfo.Size = new Size(274, 64);
            capInfo.Text = "<b>Diese Skript wird ausgeführt, wenn der User einen Eintrag wählt - NACHDEM die Zeile(n) angelegt wurden.";
            // 
            // capInfo2
            // 
            capInfo2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            capInfo2.CausesValidation = false;
            capInfo2.Location = new Point(8, 48);
            capInfo2.Name = "capInfo2";
            capInfo2.Size = new Size(274, 56);
            capInfo2.Text = "<b>Diese Skript wird ausgeführt, wenn der User einen Eintrag wählt - BEVOR die Zeile(n) angelegt werden.";
            // 
            // capScriptMenu
            // 
            capScriptMenu.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            capScriptMenu.CausesValidation = false;
            capScriptMenu.Location = new Point(8, 144);
            capScriptMenu.Name = "capScriptMenu";
            capScriptMenu.Size = new Size(274, 248);
            capScriptMenu.Text = resources.GetString("capScriptMenu.Text");
            // 
            // btnScriptAfter
            // 
            btnScriptAfter.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnScriptAfter.ButtonStyle = ButtonStyle.Optionbox;
            btnScriptAfter.Location = new Point(8, 400);
            btnScriptAfter.Name = "btnScriptAfter";
            btnScriptAfter.Size = new Size(274, 40);
            btnScriptAfter.TabIndex = 2;
            btnScriptAfter.Text = "After";
            btnScriptAfter.Click += btnScriptAfter_Click;
            // 
            // btnScriptMenu
            // 
            btnScriptMenu.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnScriptMenu.ButtonStyle = ButtonStyle.Optionbox;
            btnScriptMenu.Checked = true;
            btnScriptMenu.Location = new Point(8, 112);
            btnScriptMenu.Name = "btnScriptMenu";
            btnScriptMenu.Size = new Size(274, 32);
            btnScriptMenu.TabIndex = 1;
            btnScriptMenu.Text = "Menu Generation";
            btnScriptMenu.Click += btnScriptMenu_Click;
            // 
            // btnScriptBefore
            // 
            btnScriptBefore.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnScriptBefore.ButtonStyle = ButtonStyle.Optionbox;
            btnScriptBefore.Location = new Point(8, 16);
            btnScriptBefore.Name = "btnScriptBefore";
            btnScriptBefore.Size = new Size(274, 32);
            btnScriptBefore.TabIndex = 0;
            btnScriptBefore.Text = "Before";
            btnScriptBefore.Click += btnScriptBefore_Click;
            // 
            // txbTestZeile
            // 
            txbTestZeile.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txbTestZeile.Cursor = Cursors.IBeam;
            txbTestZeile.Location = new Point(128, 16);
            txbTestZeile.Name = "txbTestZeile";
            txbTestZeile.RaiseChangeDelay = 5;
            txbTestZeile.Size = new Size(106, 22);
            txbTestZeile.TabIndex = 0;
            // 
            // btnTestZeileDropDown
            // 
            btnTestZeileDropDown.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnTestZeileDropDown.ImageCode = "Pfeil_Unten_Scrollbar|8|||||0";
            btnTestZeileDropDown.Location = new Point(236, 16);
            btnTestZeileDropDown.Name = "btnTestZeileDropDown";
            btnTestZeileDropDown.QuickInfo = "Zeile aus der Tabelle auswählen.";
            btnTestZeileDropDown.Size = new Size(22, 22);
            btnTestZeileDropDown.TabIndex = 60;
            btnTestZeileDropDown.Click += btnTestZeileDropDown_Click;
            // 
            // grpRow
            // 
            grpRow.BackColor = Color.FromArgb(255, 255, 255);
            grpRow.Controls.Add(txbChunk);
            grpRow.Controls.Add(btnChunkDropDown);
            grpRow.Controls.Add(caption1);
            grpRow.Controls.Add(capChunk);
            grpRow.Controls.Add(txbTestZeile);
            grpRow.Controls.Add(btnTestZeileDropDown);
            grpRow.Dock = DockStyle.Top;
            grpRow.GroupBoxStyle = GroupBoxStyle.RoundRect;
            grpRow.Location = new Point(0, 40);
            grpRow.Name = "grpRow";
            grpRow.Size = new Size(784, 48);
            grpRow.TabIndex = 101;
            grpRow.TabStop = false;
            // 
            // txbChunk
            // 
            txbChunk.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txbChunk.Cursor = Cursors.IBeam;
            txbChunk.Enabled = false;
            txbChunk.Location = new Point(362, 16);
            txbChunk.Name = "txbChunk";
            txbChunk.RaiseChangeDelay = 10;
            txbChunk.Size = new Size(388, 22);
            txbChunk.TabIndex = 58;
            txbChunk.TextChanged += txbChunk_TextChanged;
            // 
            // btnChunkDropDown
            // 
            btnChunkDropDown.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnChunkDropDown.ImageCode = "Pfeil_Unten_Scrollbar|8|||||0";
            btnChunkDropDown.Location = new Point(752, 16);
            btnChunkDropDown.Name = "btnChunkDropDown";
            btnChunkDropDown.QuickInfo = "Chunk-Wert auswählen.";
            btnChunkDropDown.Size = new Size(22, 22);
            btnChunkDropDown.TabIndex = 59;
            btnChunkDropDown.Click += btnChunkDropDown_Click;
            // 
            // caption1
            // 
            caption1.CausesValidation = false;
            caption1.Location = new Point(8, 16);
            caption1.Name = "caption1";
            caption1.Size = new Size(112, 22);
            caption1.Text = "Betreffende Zeile:";
            // 
            // capChunk
            // 
            capChunk.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            capChunk.CausesValidation = false;
            capChunk.Enabled = false;
            capChunk.Location = new Point(274, 16);
            capChunk.Name = "capChunk";
            capChunk.Size = new Size(80, 22);
            capChunk.Text = "Chunk-Wert:";
            // 
            // RowAdderScriptEditor
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(784, 561);
            Controls.Add(grpScripte);
            Controls.Add(grpRow);
            Name = "RowAdderScriptEditor";
            Text = "Tabellen-Eigenschaften";
            VariableDefinitions = "";
            Controls.SetChildIndex(tabStart, 0);
            Controls.SetChildIndex(grpRow, 0);
            Controls.SetChildIndex(grpInjectVariables, 0);
            Controls.SetChildIndex(grpScripte, 0);
            Controls.SetChildIndex(pnlStatusBar, 0);
            Controls.SetChildIndex(tbcScriptEigenschaften, 0);
            tabStart.ResumeLayout(false);
            pnlStatusBar.ResumeLayout(false);
            grpScripte.ResumeLayout(false);
            grpRow.ResumeLayout(false);
            ResumeLayout(false);

        }
        private Button btnTabelleKopf;
        private GroupBox grpScripte;
        private Button btnScriptBefore;
        private Button btnScriptAfter;
        private Button btnScriptMenu;
        private Caption capScriptMenu;
        private Caption capInfo2;
        private Caption capInfo;
        private TextBox txbTestZeile;
        private Button btnTestZeileDropDown;
        private GroupBox grpRow;
        private Caption caption1;
        private TextBox txbChunk;
        private Button btnChunkDropDown;
        private Caption capChunk;
    }
}
