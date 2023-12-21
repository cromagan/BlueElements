// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Interfaces;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueScript;
using BlueScript.Enums;
using BlueScript.EventArgs;
using BlueScript.Structures;
using FastColoredTextBoxNS;

namespace BlueControls;

public sealed partial class ScriptEditor : GroupBox, IContextMenu, IDisposableExtended, IChangedFeedback //UserControl, IContextMenu//
{
    #region Fields

    private Befehlsreferenz? _befehlsReferenz;
    private string _lastVariableContent = string.Empty;
    private string? _lastWord = string.Empty;
    private bool _menuDone;
    private AutocompleteMenu? _popupMenu;

    #endregion

    #region Constructors

    public ScriptEditor() => InitializeComponent();

    #endregion

    #region Events

    public event EventHandler? Changed;

    public event EventHandler<ContextMenuInitEventArgs>? ContextMenuInit;

    public event EventHandler<ContextMenuItemClickedEventArgs>? ContextMenuItemClicked;

    public event EventHandler<ScriptEventArgs>? ExecuteScript;

    #endregion

    #region Properties

    public string ScriptText {
        get => txtSkript.Text.TrimEnd(" ");
        set => txtSkript.Text = value.TrimEnd(" ") + "    ";
    }

    #endregion

    #region Methods

    public bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e) {
        switch (e.ClickedCommand.ToLower()) {
            case "variableninhalt kopieren":
                _ = Generic.CopytoClipboard(_lastVariableContent);
                return true;
        }

        return false;
    }

    public void GetContextMenuItems(MouseEventArgs? e, ItemCollectionList.ItemCollectionList items, out object? hotItem) {
        if (!string.IsNullOrEmpty(_lastVariableContent)) {
            _ = items.Add("Variableninhalt kopieren");
        }

        hotItem = _lastWord;
    }

    public void Message(string txt) => txbSkriptInfo.Text = "[" + DateTime.UtcNow.ToLongTimeString() + "] " + txt;

    public void OnChanged() => Changed?.Invoke(this, System.EventArgs.Empty);

    public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

    public void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) => ContextMenuItemClicked?.Invoke(this, e);

    /// <summary>
    /// Verwendete Ressourcen bereinigen.
    /// </summary>
    /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
    protected override void Dispose(bool disposing) {
        if (disposing) {
            components?.Dispose();

            if (_befehlsReferenz != null && _befehlsReferenz.Visible) {
                _befehlsReferenz.Close();
                _befehlsReferenz?.Dispose();
                _befehlsReferenz = null;
            }
        }
        base.Dispose(disposing);
    }

    private void btnBefehlsUebersicht_Click(object sender, System.EventArgs e) {
        if (_befehlsReferenz != null && _befehlsReferenz.Visible) {
            _befehlsReferenz.Close();
            _befehlsReferenz?.Dispose();
            _befehlsReferenz = null;
        }

        _befehlsReferenz = new Befehlsreferenz();
        _befehlsReferenz.Show();
    }

    private void btnTest_Click(object sender, System.EventArgs e) {
        Message("Starte Skript");

        grpVariablen.Clear();

        var ex = new ScriptEventArgs();

        OnExecuteScript(ex);

        if (ex.Feedback == null) {
            var scp = new ScriptProperties(MethodType.Standard, false, [], null);
            var s = new Script(null, string.Empty, scp);
            ex.Feedback = s.Parse(0, "Main");
        }

        grpVariablen.WriteVariablesToTable(ex.Feedback.Variables);
        WriteCommandsToList();

        Message(ex.Feedback.AllOk ? "Erfolgreich, wenn auch IF-Routinen nicht geprüft wurden." : ex.Feedback.ProtocolText);
    }

    private void btnZusatzDateien_Click(object sender, System.EventArgs e) => OpenAdditionalFileFolder();

    private void OnExecuteScript(ScriptEventArgs scriptEventArgs) => ExecuteScript?.Invoke(this, scriptEventArgs);

    private void OpenAdditionalFileFolder() { }

    private void TxtSkript_MouseUp(object sender, MouseEventArgs e) {
        if (e.Button == MouseButtons.Right) {
            FloatingInputBoxListBoxStyle.ContextMenuShow(this, e);
        }
    }

    private void TxtSkript_TextChanged(object sender, TextChangedEventArgs e) => OnChanged();

    private void txtSkript_ToolTipNeeded(object sender, ToolTipNeededEventArgs e) {
        if (Script.Commands == null) { return; }

        try {
            _lastWord = string.Empty;
            _lastVariableContent = string.Empty;
            foreach (var thisc in Script.Commands) {
                if (thisc.Command.Equals(e.HoveredWord, StringComparison.OrdinalIgnoreCase)) {
                    e.ToolTipTitle = thisc.Syntax;
                    e.ToolTipText = thisc.HintText();
                    return;
                }
            }

            var hoveredWordnew = new Range(txtSkript, e.Place, e.Place).GetFragment("[A-Za-z0-9_]").Text;
            _lastWord = hoveredWordnew;

            var r = grpVariablen.RowOfVariable(hoveredWordnew);

            //foreach (var r in tableVariablen.Database.Row) {
            if (r != null && !r.IsDisposed) {
                //if (string.Equals(r.CellFirstString(), hoveredWordnew, StringComparison.OrdinalIgnoreCase)) {
                var inh = r.CellGetString("Inhalt");
                _lastVariableContent = inh;
                inh = inh.Replace("\r", ";");
                inh = inh.Replace("\n", ";");
                if (inh.Length > 25) { inh = inh.Substring(0, 20) + "..."; }
                var ro = string.Empty;
                if (r.CellGetBoolean("RO")) { ro = "[ReadOnly] "; }

                e.ToolTipTitle = ro + "(" + r.CellGetString("Typ") + ") " + hoveredWordnew + " = " + inh;
                e.ToolTipText = r.CellGetString("Kommentar") + " ";
            }
        } catch (Exception ex) {
            Develop.DebugPrint("Fehler beim Tooltip generieren", ex);
        }
    }

    private void WriteCommandsToList() {
        if (!_menuDone) {
            _menuDone = true;
            _popupMenu = new AutocompleteMenu(txtSkript) {
                //popupMenu.Items.ImageList = imageList1;
                SearchPattern = @"[\w\.:=!<>]",
                AllowTabKey = true
            };
            List<AutocompleteItem> items = [];
            if (Script.Commands != null) {
                foreach (var thisc in Script.Commands) {
                    items.Add(new SnippetAutocompleteItem(thisc.Syntax + " "));
                    items.Add(new AutocompleteItem(thisc.Command));
                    if (!string.IsNullOrEmpty(thisc.Returns)) {
                        items.Add(new SnippetAutocompleteItem("var " + thisc.Returns + " = " + thisc.Syntax + "; "));
                    }
                }
            }
            //set as autocomplete source
            _popupMenu.Items.SetAutocompleteItems(items);
        }
    }

    #endregion
}