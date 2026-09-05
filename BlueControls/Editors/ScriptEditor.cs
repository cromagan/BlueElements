// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls;
using BlueControls.ControlStrategies;
using BlueControls.Editoren;
using BlueControls.EventArgs;
using BlueScript.Classes;
using BlueScript.ScriptVariables;
using FastColoredTextBoxNS;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace BlueControls.BlueTableDialogs;

/// <summary>
/// Basis-UserControl für alle Skript-Editoren. Stellt den Code-Editor, die Variablen-Anzeige,
/// den Befehls-Assistenten sowie Speichern/Laden-Logik bereit. Erbt von EditorEasy,
/// kann also über den Default-Aufruf (InputBoxEditor) in einem Dialog gehostet werden.
/// Ist zudem direkt als Editor für ScriptDescription einsetzbar. Abgeleitete Klassen
/// überschreiben Clear, SetValuesToFormula,
/// InitializeComponentDefaultValues und ExecuteScript.
/// </summary>
public partial class ScriptEditor : EditorEasy, IContextMenu, INotifyPropertyChanged {

    #region Fields

    private static Befehlsreferenz? _befehlsReferenz;
    private bool _assistantDone;

    private string _lastVariableContent = string.Empty;
    private string? _lastWord = string.Empty;
    private bool _menuDone;
    private AutocompleteMenu? _popupMenu;

    #endregion

    #region Constructors

    public ScriptEditor() : base() {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
        ScriptChangedByUser = false;
    }

    #endregion

    #region Events

    /// <summary>
    /// Wird unmittelbar vor der Ausführung eines Skripts gefeuert (aus
    /// TesteScript). Ein hostendes Form kann hier die Arbeitskopie
    /// (z.B. EditorForIEnumerable.OutputItem) ins Backend
    /// durchstellen, bevor das Skript läuft — etwa damit der Test gegen den
    /// committeten Stand erfolgt und <c>_item</c> auf die frische Backend-Instanz
    /// aktualisiert wird.
    /// </summary>
    public event EventHandler? Executing;

    /// <summary>
    /// Wird gefeuert, wenn sich eine für das Host relevante Eigenschaft ändert.
    /// Abgeleitete Editoren lösen OnPropertyChanged typischerweise
    /// mit <c>nameof(IIsEditor.OutputItem)</c> aus, sobald die
    /// Oberfläche eine neue Edit-Kopie liefern soll. Ein hostendes
    /// EditorForIEnumerable fragt daraufhin
    /// IIsEditor.OutputItem ab.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Properties

    [DefaultValue(true)]
    public bool ContextMenuDefault { get; set; } = true;

    [DefaultValue(null)]
    public ReadOnlyCollection<ListItem>? CustomContextMenuItems { get; set; }

    public override Type? EditorFor => typeof(ScriptDescription);

    /// <summary>
    /// Delegate für die Test-Ausführung des Skripts. Wird beim Laden einer
    /// ScriptDescription-Items aus dessen
    /// ScriptDescription.ExecuteScript übernommen.
    /// Parameter: Skripttext, Testmodus.
    /// </summary>
    public ExecuteScriptDelegate? ExecuteScript { get; set; }

    public string LastFailedReason { get; set; } = string.Empty;

    public List<ScriptVariable>? LastVariables { get; set; }

    /// <summary>
    /// Der Skript-Editor öffnet maximiert als normales Fenster, da Skripte
    /// viel Platz für Code, Variablen und Assistent benötigen.
    /// </summary>
    public override bool OpenMaximized => true;

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

    /// <summary>
    /// Schlüssel, unter dem Variablen-Sets in EditorVariablesManager abgelegt werden.
    /// Sub-Controls mit eigenem Tabellen- oder Form-Bezug überschreiben dies.
    /// Bei <c>null</c> oder leerem String ist der Speichern-Knopf deaktiviert.
    /// </summary>
    public virtual string? VariablesStorageKey => null;

    protected bool ScriptChangedByUser { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// Setzt die Oberfläche zurück. Abgeleitete Editoren überschreiben dies und base aufrufen.
    /// </summary>
    public override void Clear() {
        tbcScriptEigenschaften.Enabled = false;
        Script = string.Empty;
        LastFailedReason = string.Empty;
        LastVariables = null;
    }

    public List<ListItem>? GetContextMenuItems(object? hotItem) {
        List<ListItem> contextMenu = [];
        if (!string.IsNullOrEmpty(_lastVariableContent)) {
            contextMenu.Add(ItemOf("Variableninhalt kopieren", ImageCode.Clipboard, Contextmenu_CopyVariableContent, true));
        }
        return contextMenu;
    }

    /// <summary>
    /// Liest die Werte aller FlexiControls aus grpInjectVariables,
    /// deren Tag mit "Attribut" beginnt (z.B. "Attribut0", "Attribut1", ...).
    /// Liefert eine Liste für den args-Parameter von Script.Parse,
    /// oder <c>null</c>, wenn keine Attribut-Felder vorhanden sind.
    /// </summary>
    public List<string>? GetParseArgs() {
        var args = new List<string>();
        for (var z = 0; z < 20; z++) {
            var name = "Attribut" + z;
            foreach (var c in grpInjectVariables.Controls) {
                if (c is FlexiControl flx && flx.Tag is string tag && string.Equals(tag, name, StringComparison.OrdinalIgnoreCase)) {
                    args.Add(flx.Value ?? string.Empty);
                    break;
                }
            }
        }
        return args.Count > 0 ? args : null;
    }

    public void TesteScript(bool testmode) {
        UpdateState("Starte Skript", null, false);

        // Dem Host die Möglichkeit geben, die Arbeitskopie ins Backend zu
        // schreiben (z.B. TableScriptEditorForm schreibt OutputItem in
        // Table.EventScript zurück und lädt _item auf die frische Backend-Instanz).
        OnExecuting(System.EventArgs.Empty);

        // Vorab-Prüfung: Variablen-Namen ermitteln und einfache Syntax-Prüfung.
        // Syntax-Fehler verhindern die Ausführung. Protokoll-Nachrichten
        // (z.B. nicht auflösbare Var-Werte) werden angezeigt, blockieren aber nicht.
        var preCheck = ScriptPreCheck.Check(Script);
        if (preCheck.HasSyntaxErrors) {
            WriteCommandsToList();
            var protocolPart = preCheck.Protocol.Count > 0
                ? "\r\n\r\nZusätzliche Hinweise:\r\n" + string.Join("\r\n", preCheck.Protocol)
                : string.Empty;
            UpdateState($"Syntax-Fehler — Ausführung abgebrochen:\r\n{string.Join("\r\n", preCheck.SyntaxErrors)}{protocolPart}", null, false);
            return;
        }

        if (ExecuteScript is not { } exec) {
            UpdateState("Kein Skript-Executor gesetzt — Ausführung abgebrochen.", null, false);
            return;
        }

        var f = exec(Script, testmode);

        WriteCommandsToList();

        if (f.Failed) {
            var protocolPart = preCheck.Protocol.Count > 0
                ? "\r\n\r\nZusätzliche Hinweise:\r\n" + string.Join("\r\n", preCheck.Protocol)
                : string.Empty;
            UpdateState(f.ProtocolText + protocolPart, f.Variables?.ToList(), false);
            return;
        }

        if (!string.IsNullOrEmpty(f.FailedReason)) {
            UpdateState($"NICHT erfolgreich, aber kein Skript Fehler:\r\n{f.FailedReason}", f.Variables?.ToList(), false);
            return;
        }

        var protocolText = preCheck.Protocol.Count > 0
            ? "\r\n\r\nHinweise:\r\n" + string.Join("\r\n", preCheck.Protocol)
            : string.Empty;
        UpdateState("Erfolgreich geprüft." + protocolText, f.Variables?.ToList(), false);
    }

    public void UpdateState(string txt, List<ScriptVariable>? variables, bool updateSpecialFields) {
        txbErrorInfo.Text = "[" + DateTime.UtcNow.ToLongTimeString() + "] " + txt;

        grpVariablen.InputItem = variables is { Count: > 0 } v ? new VariableCollection(v, true) : null;

        if (updateSpecialFields) {
            VariablesToSpecialField(DummyJson(variables), false);
        }
    }

    protected void btnAnzeigen_Click(object? sender, System.EventArgs e) {
        if (string.IsNullOrEmpty(LastFailedReason)) {
            UpdateState("Alles OK - kein Skript-Fehler gespeichert.", null, false);
        } else {
            UpdateState($"Letzter gespeicherter Skript-Fehler:\r\r{LastFailedReason}\r\r\rVariablen während des Auftretens werden angezeigt", LastVariables, true);
        }
    }

    /// <summary>
    /// Überschrieben, damit beim Verwerfen des UserControls Hilfsfenster geschlossen werden.
    /// </summary>
    protected override void Dispose(bool disposing) {
        if (disposing) {
            CloseCommandList();

            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    protected VariableCollection GetEditorVariables() {
        var vc = new VariableCollection();
        foreach (var c in grpInjectVariables.Controls) {
            if (c is FlexiControl flx && flx.Tag is string name && !string.IsNullOrEmpty(name)) {
                vc.Add(new StringScriptVariable(name, flx.Value ?? string.Empty, true, "Editor-Variable"));
            }
        }
        return vc;
    }

    /// <summary>
    /// Basis-Default: tut nichts. Abgeleitete Editoren füllen hier einmalig Dropdowns etc.
    /// </summary>
    protected override void InitializeComponentDefaultValues() { }

    protected virtual void OnExecuting(System.EventArgs e) => Executing?.Invoke(this, e);

    /// <summary>
    /// Löst PropertyChanged aus. Wird von abgeleiteten Editoren
    /// genutzt, um Änderungen — meist <c>nameof(IIsEditor.OutputItem)</c> —
    /// an ein hostendes EditorForIEnumerable zu melden.
    /// </summary>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "unknown") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Lädt das übergebene ScriptDescription-Item in die Oberfläche.
    /// Abgeleitete Skript-Editoren überschreiben dies, um zusätzliche UI-Felder zu befüllen.
    /// </summary>
    protected override bool SetValuesToFormula(object? toEdit) {
        if (toEdit is not ScriptDescription sd) { return true; }

        // ExecuteScriptDelegate übernehmen, damit der Editor das Skript
        // mit dem vom Ersteller vorgesehenen Kontext testen kann.
        // TableScriptEditor setzt ExecuteScript in seinem eigenen
        // SetValuesToFormula auf ExecuteScriptCore und ignoriert diesen Delegate.
        ExecuteScript = sd.ExecuteScript;

        tbcScriptEigenschaften.Enabled = true;
        Script = sd.Script;
        LastFailedReason = sd.FailedReason;
        LastVariables = sd.SavedVariables;
        return true;
    }

    /// <summary>
    /// Erzeugt ein neues JsonObject mit den Basis-Feldern dieses Editors (Injektions-Felder).
    /// Abgeleitete Klassen überschreiben dies, base aufrufen und besondere Felder ergänzen.
    /// </summary>
    protected virtual JsonObject SpecialFieldsToVariables() {
        var data = new JsonObject();
        foreach (var c in grpInjectVariables.Controls) {
            if (c is FlexiControl flx && flx.Tag is string name && !string.IsNullOrEmpty(name)) {
                data[name.ToUpperInvariant()] = flx.Value ?? string.Empty;
            }
        }
        return data;
    }

    /// <summary>
    /// Übernimmt die gespeicherten Feldwerte in die Injektions-Felder
    /// (grpInjectVariables). Ist <paramref name="clearMissing"/> true,
    /// werden Felder, deren Wert in <paramref name="data"/> fehlt, geleert
    /// (z.B. beim Laden eines Variablen-Sets). Bei false bleiben sie unverändert
    /// (z.B. beim Wiederherstellen eines Fehler-Snapshots).
    /// Abgeleitete Klassen überschreiben dies, base aufrufen und besondere Felder befüllen.
    /// </summary>
    protected virtual void VariablesToSpecialField(JsonObject? data, bool clearMissing) {
        foreach (var c in grpInjectVariables.Controls) {
            if (c is FlexiControl flx && flx.Tag is string name && !string.IsNullOrEmpty(name)) {
                if (data is not null
                    && data.TryGetPropertyValue(name.ToUpperInvariant(), out var node)
                    && node is JsonValue v
                    && v.TryGetValue(out string? s)) {
                    flx.Value = s ?? string.Empty;
                } else if (clearMissing) {
                    flx.Value = string.Empty;
                }
            }
        }
    }

    private static void CloseCommandList() {
        if (_befehlsReferenz is { Visible: true }) {
            _befehlsReferenz.Close();
            _befehlsReferenz.Dispose();
            _befehlsReferenz = null;
        }
    }

    private static JsonObject? DummyJson(List<ScriptVariable>? variables) {
        if (variables is null) { return null; }

        var j = new JsonObject();

        foreach (var v in variables) {
            // Wir erstellen explizit ein JsonValue aus dem String,
            // damit node is JsonValue in VariablesToSpecialField sicher wahr ergibt.
            j.TryAdd(v.KeyName, JsonValue.Create(v.ValueForCell));
        }

        return j;
    }

    private void btnAusführen_Click(object sender, System.EventArgs e) => TesteScript(false);

    private void btnBefehlsUebersicht_Click(object sender, System.EventArgs e) {
        CloseCommandList();

        _befehlsReferenz = new Befehlsreferenz();
        _befehlsReferenz.Show();
    }

    private void btnLeeren_Click(object sender, System.EventArgs e) {
        LastFailedReason = string.Empty;
        LastVariables = null;
        OnPropertyChanged("FailedReason");
        btnAnzeigen_Click(null, System.EventArgs.Empty);
    }

    private void btnSave_Click(object sender, System.EventArgs e) {
        btnSaveLoad.Enabled = false;
        FormManager.SaveAllFiles();
        btnSaveLoad.Enabled = true;
    }

    private void btnVariables_Click(object? sender, System.EventArgs e) {
        if (IsDisposed || string.IsNullOrEmpty(VariablesStorageKey)) { return; }

        var storageKey = VariablesStorageKey;
        var savedSets = EditorVariablesManager.GetSets(storageKey);

        var items = new List<ListItem>();

        if (savedSets.Count > 0) {
            items.Add(ItemOf("Gespeicherte Variablen-Sets:", true));
            foreach (var sv in savedSets) {
                items.Add(ItemOf(sv.KeyName, sv.KeyName, ImageCode.Tabelle, VariablesManager_LoadSet, true, string.Empty));
            }
        }

        items.Add(ItemOf("Verwaltung:", true));

        var saveItem = ItemOf("Aktuelle Variablen speichern", "SaveSet", ImageCode.PlusZeichen, VariablesManager_SaveSet, true);
        saveItem.RemoveLocked = true;
        items.Add(saveItem);

        items.Add(Separator());
        var abortItem = ItemOf("Abbruch", ImageCode.TasteESC);
        abortItem.RemoveLocked = true;
        items.Add(abortItem);

        var dropDown = FloatingInputBoxListBoxStyle.Show(items, CheckBehavior.NoSelection, null, this, false, ListBoxAppearance.DropdownSelectbox, Design.Item_ContextMenu, false, savedSets.Count > 0);
        dropDown.ItemRemoved += VariablesManagerDropDown_ItemRemoved;
    }

    private void Contextmenu_CopyVariableContent(object? sender, ContextMenuEventArgs e) {
        if (e.HotItem is not string content) { return; }
        if (CopytoClipboard(content)) {
            QuickNote.Show(NoteSymbols.Ok, "Kopiert");
        } else {
            QuickNote.Show(NoteSymbols.Critical, "Fehlgeschlagen");
        }
    }

    private void CreateVariableFlexiControls() {
        if (DesignMode) { return; }

        grpInjectVariables.SuspendLayout();

        // Nur FlexiControls entfernen, damit vom Designer/Subklassen angelegte Controls (z.B. btnVariables, chkExtendend) erhalten bleiben.
        foreach (var flx in grpInjectVariables.Controls.OfType<FlexiControl>().ToList()) {
            grpInjectVariables.Controls.Remove(flx);
            flx.Dispose();
        }

        var names = VariableDefinitions
            .Split([',', ';', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        const int flxWidth = 200;
        const int flxHeight = 24;
        var addX = flxWidth + Skin.PaddingSmal;

        // Start-X hinter den vorhandenen, nicht-FlexiControl-Controls ermitteln.
        // Y immer oben mit PaddingSmal beginnen, damit die Eingabefelder bündig oben ausgerichtet werden.
        var startX = 0;
        foreach (Control c in grpInjectVariables.Controls) {
            if (c is FlexiControl) { continue; }
            var right = c.Right + Skin.PaddingSmal;
            if (right > startX) { startX = right; }
        }

        var toppos = Skin.Padding;
        var leftpos = startX;

        foreach (var name in names) {
            if (string.IsNullOrEmpty(name)) { continue; }

            var flx = new FlexiControl {
                Caption = name + ":",
                CaptionPosition = CaptionPosition.Links_neben_dem_Feld,
                ControlStrategy = TextBoxControlStrategy.ClassId,
                Tag = name
            };

            flx.SetBounds(leftpos, toppos, flxWidth, flxHeight);
            flx.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            grpInjectVariables.Controls.Add(flx);
            leftpos += addX;
        }

        grpInjectVariables.ResumeLayout();
    }

    private void lstAssistant_ItemClicked(object sender, ListItemEventArgs e) {
        foreach (var thisc in ScriptCommand.AllMethods.Instances) {
            if (thisc is ICommandBuilder ic) {
                if (e.Item.KeyName == ic.KeyName) {
                    var c = ic.GetCode((Forms.Form?)FindForm());

                    if (!string.IsNullOrEmpty(c)) {
                        Script = Script + "\r\n" + c + "\r\n";
                    }

                    return;
                }
            }
        }
    }

    /// <summary>
    /// Speichert das übergebene Variablen-Set und schreibt es in den EditorVariablesManager.
    /// Wird sowohl vom "Speichern unter"- als auch vom "Standard setzen"-Pfad verwendet.
    /// </summary>
    private void SaveCurrentVariableSet(string setName) {
        if (IsDisposed || string.IsNullOrEmpty(VariablesStorageKey)) { return; }
        var data = SpecialFieldsToVariables();
        EditorVariablesManager.SaveSet(VariablesStorageKey, setName, data);
    }

    private void tbcScriptEigenschaften_Selecting(object sender, TabControlCancelEventArgs e) {
        if (e.TabPage == tabAssistent && !_assistantDone) {
            _assistantDone = true;

            foreach (var thisc in ScriptCommand.AllMethods.Instances) {
                if (thisc is ICommandBuilder ic) {
                    var t = new TextListItem(ic.CommandDescription(), ic.KeyName, ic.CommandImage(), false, true, string.Empty, string.Empty);

                    lstAssistant.ItemAdd(t);
                }
            }
        }
    }

    /// <summary>
    /// Lädt ein Variablen-Set per Namen und wendet es auf den Editor an.
    /// Gibt true zurück, wenn das Set gefunden und angewendet wurde.
    /// </summary>
    private bool TryLoadVariableSet(string setName) {
        if (IsDisposed || string.IsNullOrEmpty(VariablesStorageKey)) { return false; }

        var savedSets = EditorVariablesManager.GetSets(VariablesStorageKey);
        var entry = savedSets.Find(v => string.Equals(v.KeyName, setName, StringComparison.OrdinalIgnoreCase));
        if (entry?.JsonData is not { } data) { return false; }

        VariablesToSpecialField(data, true);
        return true;
    }

    private void TxtSkript_MouseUp(object sender, MouseEventArgs e) {
        if (e.Button == MouseButtons.Right) {
            ((IContextMenu)this).ContextMenuShow(_lastWord);
        }
    }

    private void TxtSkript_TextChanged(object sender, TextChangedEventArgs e) {
        ScriptChangedByUser = true;
        if (Mode == EditorMode.EditItem && InputItem is ScriptDescription sd) { sd.Script = Script; }
    }

    private void txtSkript_ToolTipNeeded(object sender, ToolTipNeededEventArgs e) {
        try {
            _lastWord = string.Empty;
            _lastVariableContent = string.Empty;
            foreach (var thisc in ScriptCommand.AllMethods.Instances) {
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

    private void VariablesManager_LoadSet(object? sender, ContextMenuEventArgs e) {
        if (IsDisposed || string.IsNullOrEmpty(VariablesStorageKey)) { return; }

        if (e.Item.KeyName is { } setName && TryLoadVariableSet(setName)) {
            QuickNote.Show(NoteSymbols.Ok, "Geladen");
        }
    }

    private void VariablesManager_SaveSet(object? sender, ContextMenuEventArgs e) {
        if (IsDisposed || string.IsNullOrEmpty(VariablesStorageKey)) { return; }

        var name = InputBox.Show("Variablen speichern unter:", string.Empty, BlueBasics.Classes.Formats.SystemNameFormat.Instance);
        if (string.IsNullOrEmpty(name)) { return; }

        SaveCurrentVariableSet(name);
        QuickNote.Show(NoteSymbols.Ok, "Gespeichert");
    }

    private void VariablesManagerDropDown_ItemRemoved(object? sender, ListItemEventArgs e) {
        if (IsDisposed || string.IsNullOrEmpty(VariablesStorageKey)) { return; }
        if (e.Item.RemoveLocked) { return; }
        if (e.Item is not TextListItem tli) { return; }

        EditorVariablesManager.DeleteSet(VariablesStorageKey, tli.KeyName);
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
            foreach (var thisc in ScriptCommand.AllMethods.Instances) {
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