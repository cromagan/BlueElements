// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using BlueBasics.MultiUserFile;
using BlueControls.EventArgs;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueScript.Methods;
using BlueScript.Structures;
using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.BlueDatabaseDialogs;

public partial class ScriptEditorGeneric : FormWithStatusBar, IUniqueWindow, IContextMenuWithInternalHandling {

    #region Fields

    private static Befehlsreferenz? _befehlsReferenz;

    private bool _assistantDone;
    private string _lastVariableContent = string.Empty;

    private string? _lastWord = string.Empty;

    private bool _menuDone;

    private AutocompleteMenu? _popupMenu;

    #endregion

    #region Constructors

    public ScriptEditorGeneric() : base() {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
        tbcScriptEigenschaften.Enabled = false;
    }

    #endregion

    #region Events

    public event EventHandler<ContextMenuInitEventArgs>? ContextMenuInit;

    public event EventHandler<ContextMenuItemClickedEventArgs>? ContextMenuItemClicked;

    #endregion

    #region Properties

    public string LastFailedReason {
        get => txbLastError.Text.TrimEnd(" ");
        set => txbLastError.Text = value.TrimEnd(" ");
    }

    public virtual object? Object { get; set; }

    public string Script {
        get => txtSkript.Text.TrimEnd(" ").Replace("\r\n", "\r");
        set {
            var scr = value.TrimEnd(" ") + "    ";

            if (scr == txtSkript.Text.Replace("\r\n", "\r")) { return; }

            txtSkript.Text = scr;
        }
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

    public virtual ScriptEndedFeedback ExecuteScript(bool testmode) =>
        new ScriptEndedFeedback("Fehler", false, false, "Unbekannt");

    public void GetContextMenuItems(ContextMenuInitEventArgs e) {
        if (!string.IsNullOrEmpty(_lastVariableContent)) {
            e.ContextMenu.Add(ItemOf("Variableninhalt kopieren"));
        }

        OnContextMenuInit(e);
    }

    public void Message(string txt) => txbSkriptInfo.Text = "[" + DateTime.UtcNow.ToLongTimeString() + "] " + txt;

    public void OnContextMenuInit(ContextMenuInitEventArgs e) => ContextMenuInit?.Invoke(this, e);

    public void TesteScript(bool testmode) {
        Message("Starte Skript");

        grpVariablen.Clear();

        var f = ExecuteScript(testmode);

        //if (f == null) {
        //    var m = Method.GetMethods(MethodType.Standard | MethodType.MyDatabaseRow | MethodType.Math | MethodType.DrawOnBitmap);

        //    var scp = new ScriptProperties("Skript-Editor: " + f., m, false, [], null, 0);
        //    var s = new Script(null, scp);
        //    f = s.Parse(0, "Main", null);
        //}

        grpVariablen.ToEdit = f.Variables;
        WriteCommandsToList();

        if (f.Failed) {
            Message(f.ProtocolText);
            return;
        }

        if (!string.IsNullOrEmpty(f.FailedReason)) {
            Message("NICHT erfolgreich, aber kein Skript Fehler: " + f.FailedReason);
            return;
        }

        Message("Erfolgreich, wenn auch IF-Routinen nicht geprüft wurden.");
    }

    public virtual void WriteInfosBack() { }

    protected void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e) => ContextMenuItemClicked?.Invoke(this, e);

    protected override void OnFormClosing(FormClosingEventArgs e) {
        WriteInfosBack();

        if (_befehlsReferenz is { Visible: true }) {
            _befehlsReferenz.Close();
            _befehlsReferenz.Dispose();
            _befehlsReferenz = null;
        }

        base.OnFormClosing(e);
    }

    private void btnAusführen_Click(object sender, System.EventArgs e) => TesteScript(false);

    private void btnBefehlsUebersicht_Click(object sender, System.EventArgs e) {
        if (_befehlsReferenz is { Visible: true }) {
            _befehlsReferenz.Close();
            _befehlsReferenz.Dispose();
            _befehlsReferenz = null;
        }

        _befehlsReferenz = new Befehlsreferenz();
        _befehlsReferenz.Show();
    }

    private void btnLeeren_Click(object sender, System.EventArgs e) {
        txbLastError.Text = string.Empty;
        WriteInfosBack();
    }

    private void btnSave_Click(object sender, System.EventArgs e) {
        btnSaveLoad.Enabled = false;

        WriteInfosBack();
        MultiUserFile.SaveAll(false);

        btnSaveLoad.Enabled = true;
    }

    private void lstAssistant_ItemClicked(object sender, AbstractListItemEventArgs e) {
        foreach (var thisc in Method.AllMethods) {
            if (thisc is IComandBuilder ic) {
                if (e.Item.KeyName == ic.KeyName) {
                    var c = ic.GetCode(this);

                    if (!string.IsNullOrEmpty(c)) {
                        Script = Script + "\r\n" + c + "\r\n";
                    }

                    return;
                }
            }
        }
    }

    private void tbcScriptEigenschaften_Selecting(object sender, TabControlCancelEventArgs e) {
        if (e.TabPage == tabAssistent && !_assistantDone) {
            _assistantDone = true;

            foreach (var thisc in Method.AllMethods) {
                if (thisc is IComandBuilder ic) {
                    var t = new TextListItem(ic.ComandDescription(), ic.KeyName, ic.ComandImage(), false, true, string.Empty);

                    lstAssistant.ItemAdd(t);
                }
            }
        }
    }

    private void TxtSkript_MouseUp(object sender, MouseEventArgs e) {
        if (e.Button == MouseButtons.Right) {
            FloatingInputBoxListBoxStyle.ContextMenuShow(this, _lastWord, e);
        }
    }

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