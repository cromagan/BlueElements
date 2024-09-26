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

#nullable enable

using BlueBasics;
using BlueBasics.Interfaces;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueScript;
using BlueScript.Enums;
using BlueScript.EventArgs;
using BlueScript.Methods;
using BlueScript.Structures;
using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls;

//#if DEBUG

public sealed partial class ScriptEditor : UserControl, IContextMenu, IDisposableExtended, IPropertyChangedFeedback //UserControl, IContextMenu//
//#else
//   public sealed partial class ScriptEditor : GroupBox, IContextMenu, IDisposableExtended, IPropertyChangedFeedback //UserControl, IContextMenu//
//#endif
{
    #region Fields

    private string _lastVariableContent = string.Empty;
    private string? _lastWord = string.Empty;
    private bool _menuDone;
    private AutocompleteMenu? _popupMenu;

    #endregion

    #region Constructors

    public ScriptEditor() => InitializeComponent();

    #endregion

    #region Events

    public event EventHandler<ContextMenuInitEventArgs>? ContextMenuInit;

    public event EventHandler<ContextMenuItemClickedEventArgs>? ContextMenuItemClicked;

    public event EventHandler<ScriptEventArgs>? ExecuteScript;

    public event EventHandler? PropertyChanged;

    #endregion

    #region Properties

    public string Script {
        get => txtSkript.Text.TrimEnd(" ");
        set => txtSkript.Text = value.TrimEnd(" ") + "    ";
    }

    #endregion

    #region Methods

    public void DoContextMenuItemClick(ContextMenuItemClickedEventArgs e) {
        switch (e.Item.KeyName.ToLowerInvariant()) {
            case "variableninhalt kopieren":
                _ = Generic.CopytoClipboard(_lastVariableContent);
                return;
        }

        OnContextMenuItemClicked(e);
    }

    public void GetContextMenuItems(ContextMenuInitEventArgs e) {
        if (!string.IsNullOrEmpty(_lastVariableContent)) {
            e.ContextMenu.Add(ItemOf("Variableninhalt kopieren"));
        }

        OnContextMenuInit(e);
    }

    public void Message(string txt) => txbSkriptInfo.Text = "[" + DateTime.UtcNow.ToLongTimeString() + "] " + txt;

    public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

    public void OnPropertyChanged() => PropertyChanged?.Invoke(this, System.EventArgs.Empty);

    public void TesteScript(string scriptname) {
        Message("Starte Skript");

        grpVariablen.Clear();

        var ex = new ScriptEventArgs();

        OnExecuteScript(ex);

        if (ex.Feedback == null) {
            var m = Method.GetMethods(MethodType.Standard | MethodType.MyDatabaseRow | MethodType.Math | MethodType.DrawOnBitmap);

            var scp = new ScriptProperties("Skript-Editor: " + scriptname, m, false, [], null, 0);
            var s = new Script(null, scp);
            ex.Feedback = s.Parse(0, "Main", null);
        }

        grpVariablen.ToEdit = ex.Feedback.Variables;
        WriteCommandsToList();

        Message(ex.Feedback.AllOk ? "Erfolgreich, wenn auch IF-Routinen nicht geprüft wurden." : ex.Feedback.ProtocolText);
    }

    /// <summary>
    /// Verwendete Ressourcen bereinigen.
    /// </summary>
    /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
    protected override void Dispose(bool disposing) {
        if (disposing) {
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) => ContextMenuItemClicked?.Invoke(this, e);

    private void OnExecuteScript(ScriptEventArgs scriptEventArgs) => ExecuteScript?.Invoke(this, scriptEventArgs);

    private void TxtSkript_MouseUp(object sender, MouseEventArgs e) {
        if (e.Button == MouseButtons.Right) {
            FloatingInputBoxListBoxStyle.ContextMenuShow(this, _lastWord, e);
        }
    }

    private void TxtSkript_TextChanged(object sender, TextChangedEventArgs e) => OnPropertyChanged();

    private void txtSkript_ToolTipNeeded(object sender, ToolTipNeededEventArgs e) {
        try {
            _lastWord = string.Empty;
            _lastVariableContent = string.Empty;
            foreach (var thisc in Method.AllMethods) {
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
            if (r is { IsDisposed: false }) {
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
            foreach (var thisc in Method.AllMethods) {
                items.Add(new SnippetAutocompleteItem(thisc.Syntax + " "));
                items.Add(new AutocompleteItem(thisc.Command));
                if (!string.IsNullOrEmpty(thisc.Returns)) {
                    items.Add(new SnippetAutocompleteItem("var " + thisc.Returns + " = " + thisc.Syntax + "; "));
                }
            }
            //set as autocomplete source
            _popupMenu.Items.SetAutocompleteItems(items);
        }
    }

    #endregion
}