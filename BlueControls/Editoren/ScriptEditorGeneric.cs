// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Classes.FileSystemCaching;
using BlueControls.Classes.ItemCollectionList;
using BlueControls.Controls;
using BlueControls.EventArgs;
using BlueScript.Classes;
using BlueScript.Variables;
using FastColoredTextBoxNS;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows.Forms;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.BlueTableDialogs;

public partial class ScriptEditorGeneric : FormWithStatusBar, IUniqueWindow, IContextMenu {

    #region Fields

    private static Befehlsreferenz? _befehlsReferenz;
    private bool _assistantDone;
    private string _lastVariableContent = string.Empty;
    private string? _lastWord = string.Empty;
    private bool _menuDone;
    private AutocompleteMenu? _popupMenu;

    #endregion

    #region Constructors

    protected ScriptEditorGeneric() : base() {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
        ScriptChangedByUser = false;
    }

    #endregion

    #region Properties

    [DefaultValue(true)]
    public bool ContextMenuDefault { get; set; } = true;

    [DefaultValue(null)]
    public ReadOnlyCollection<AbstractListItem>? CustomContextMenuItems { get; set; } = null;

    public string LastFailedReason { get; set; } = string.Empty;
    public virtual object? Object { get; set; }

    public string Script {
        get => txtSkript.Text.TrimEnd(' ').Replace("\r\n", "\r");
        set {
            var normalizedText = value.TrimEnd(' ') + "    ";

            if (normalizedText == txtSkript.Text.Replace("\r\n", "\r")) { return; }

            txtSkript.Text = normalizedText;
            ScriptChangedByUser = false;
        }
    }

    public int StoppedTimeCount { get; set; }

    [Category("Skript"), DefaultValue("")]
    [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
    public string VariableDefinitions {
        get;
        set {
            field = value ?? string.Empty;
            CreateVariableFlexiControls();
        }
    }

    protected bool ScriptChangedByUser { get; set; }

    #endregion

    #region Methods

    public virtual ScriptEndedFeedback ExecuteScript(bool testmode) => new("Fehler", false, false, "Unbekannt");

    public List<AbstractListItem>? GetContextMenuItems(object? hotItem) {
        List<AbstractListItem> contextMenu = [];
        if (!string.IsNullOrEmpty(_lastVariableContent)) {
            contextMenu.Add(ItemOf("Variableninhalt kopieren", ImageCode.Clipboard, Contextmenu_CopyVariableContent, true));
        }
        return contextMenu;
    }

    public void Message(string txt) => txbErrorInfo.Text = "[" + DateTime.UtcNow.ToLongTimeString() + "] " + txt;

    public void TesteScript(bool testmode) {
        Message("Starte Skript");

        grpVariablen.Clear();

        var f = ExecuteScript(testmode);

        grpVariablen.InputItem = f.Variables;
        WriteCommandsToList();

        if (f.Failed) {
            Message(f.ProtocolText);
            return;
        }

        if (!string.IsNullOrEmpty(f.FailedReason)) {
            Message("NICHT erfolgreich, aber kein Skript Fehler: " + f.FailedReason);
            return;
        }

        Message("Erfolgreich geprüft.");
    }

    public virtual void WriteInfosBack() { }

    protected void btnAnzeigen_Click(object? sender, System.EventArgs e) {
        if (string.IsNullOrEmpty(LastFailedReason)) {
            txbErrorInfo.Text = "Alles OK - kein Skript-Fehler gespeichert.";
        } else {
            txbErrorInfo.Text = "Letzter gespeicherter Skript-Fehler:\r\r" + LastFailedReason;
        }
    }

    protected VariableCollection GetEditorVariables() {
        var vc = new VariableCollection();
        foreach (var c in grpVariables.Controls) {
            if (c is FlexiControl flx && flx.Tag is string name && !string.IsNullOrEmpty(name)) {
                vc.Add(new VariableString(name, flx.Value ?? string.Empty, true, "Editor-Variable"));
            }
        }
        return vc;
    }

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
        LastFailedReason = string.Empty;
        WriteInfosBack();
    }

    private void btnSave_Click(object sender, System.EventArgs e) {
        btnSaveLoad.Enabled = false;

        WriteInfosBack();
        Table.SaveAll();
        CachedFileSystem.SaveAll(false);
        btnSaveLoad.Enabled = true;
    }

    private void Contextmenu_CopyVariableContent(object? sender, ContextMenuEventArgs e) {
        if (e.HotItem is not string content) { return; }
        if (Generic.CopytoClipboard(content)) {
            QuickNote.Show(NoteSymbols.Ok, "Kopiert");
        } else {
            QuickNote.Show(NoteSymbols.Critical, "Fehlgeschlagen");
        }
    }

    private void CreateVariableFlexiControls() {
        grpVariables.SuspendLayout();
        grpVariables.Controls.Clear();
        var names = VariableDefinitions
            .Split([',', ';', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var name in names) {
            if (string.IsNullOrEmpty(name)) { continue; }
            var flx = new FlexiControl {
                Caption = name,
                CaptionPosition = CaptionPosition.Links_neben_dem_Feld,
                EditType = EditTypeFormula.Textfeld,
                Tag = name
            };
            grpVariables.Controls.Add(flx);
        }
        grpVariables.ResumeLayout();
    }

    private void lstAssistant_ItemClicked(object sender, AbstractListItemEventArgs e) {
        foreach (var thisc in Method.AllMethods.Instances) {
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

            foreach (var thisc in Method.AllMethods.Instances) {
                if (thisc is IComandBuilder ic) {
                    var t = new TextListItem(ic.ComandDescription(), ic.KeyName, ic.ComandImage(), false, true, string.Empty, string.Empty);

                    lstAssistant.ItemAdd(t);
                }
            }
        }
    }

    private void TxtSkript_MouseUp(object sender, MouseEventArgs e) {
        if (e.Button == MouseButtons.Right) {
            ((IContextMenu)this).ContextMenuShow(_lastWord);
        }
    }

    private void TxtSkript_TextChanged(object sender, TextChangedEventArgs e) => ScriptChangedByUser = true;

    private void txtSkript_ToolTipNeeded(object sender, ToolTipNeededEventArgs e) {
        try {
            _lastWord = string.Empty;
            _lastVariableContent = string.Empty;
            foreach (var thisc in Method.AllMethods.Instances) {
                if (thisc.Command.Equals(e.HoveredWord, StringComparison.OrdinalIgnoreCase)) {
                    e.ToolTipTitle = thisc.Syntax;
                    e.ToolTipText = thisc.HintText();
                    return;
                }
            }

            var hoveredWordnew = new FastColoredTextBoxNS.Range(txtSkript, e.Place, e.Place).GetFragment("[A-Za-z0-9_]").Text;
            _lastWord = hoveredWordnew;

            var r = grpVariablen.RowOfVariable(hoveredWordnew);

            //foreach (var r in tableVariablen.Table.Row) {
            if (r is { IsDisposed: false }) {
                //if (string.Equals(r.CellFirstString(), hoveredWordnew, StringComparison.OrdinalIgnoreCase)) {
                var inh = r.CellGetString("Inhalt");
                _lastVariableContent = inh;
                inh = inh.Replace('\r', ';');
                inh = inh.Replace('\n', ';');
                if (inh.Length > 25) { inh = inh[..20] + "..."; }
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
            foreach (var thisc in Method.AllMethods.Instances) {
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