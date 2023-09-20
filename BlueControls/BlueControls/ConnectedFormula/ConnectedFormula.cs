// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueBasics.Interfaces;
using BlueBasics.MultiUserFile;
using BlueControls.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueDatabase;
using BlueScript.Variables;
using static BlueBasics.Converter;
using static BlueBasics.Develop;
using static BlueBasics.IO;
using BlueScript.Structures;
using BlueScript.Enums;
using BlueScript;
using static BlueBasics.Generic;

namespace BlueControls.ConnectedFormula;

public class ConnectedFormula : IChangedFeedback, IDisposableExtended, IHasKeyName, ICanDropMessages {

    #region Fields

    public const string Version = "0.30";

    public static readonly ObservableCollection<ConnectedFormula> AllFiles = new();

    public static readonly float StandardHöhe = 1.75f;

    private readonly List<string> _databaseFiles = new();

    private readonly List<FormulaScriptDescription> _eventScript = new();
    private readonly List<string> _notAllowedChilds = new();

    private readonly List<Variable> _variables = new();
    private string _createDate;

    private string _creator;

    private string _eventScriptTmp = string.Empty;
    private int _id = -1;

    private string _loadedVersion = "0.00";

    private MultiUserFile? _muf;

    private ItemCollectionPad.ItemCollectionPad? _padData;

    private bool _saved;

    private bool _saving;

    private string _variableTmp = string.Empty;

    #endregion

    #region Constructors

    public ConnectedFormula() : this(string.Empty) { }

    private ConnectedFormula(string filename) {
        AllFiles.Add(this);
        _muf = new MultiUserFile();

        //_muf.ConnectedControlsStopAllWorking += OnConnectedControlsStopAllWorking;
        _muf.Loaded += OnLoaded;
        _muf.Loading += OnLoading;
        _muf.SavedToDisk += OnSavedToDisk;
        _muf.DiscardPendingChanges += DiscardPendingChanges;
        _muf.HasPendingChanges += HasPendingChanges;
        _muf.ParseExternal += ParseExternal;
        _muf.ToListOfByte += ToListOfByte;
        _muf.Saving += _muf_Saving;
        _createDate = DateTime.UtcNow.ToString(Constants.Format_Date5);
        _creator = UserName;
        PadData = new ItemCollectionPad.ItemCollectionPad();

        if (FileExists(filename)) {
            _muf.Load(filename, true);
        }

        _saved = true;

        if (_padData != null) {
            //_padData.SheetSizeInMm = new SizeF(PixelToMm(500, ItemCollectionPad.Dpi), PixelToMm(850, ItemCollectionPad.Dpi));
            _padData.GridShow = PixelToMm(IAutosizableExtension.GridSize, ItemCollectionPad.ItemCollectionPad.Dpi);
            _padData.GridSnap = PixelToMm(IAutosizableExtension.GridSize, ItemCollectionPad.ItemCollectionPad.Dpi);
        }
    }

    #endregion

    #region Events

    public event EventHandler? Changed;

    public event EventHandler<MessageEventArgs>? DropMessage;

    public event EventHandler? Loaded;

    public event EventHandler? Loading;

    public event EventHandler? NotAllowedChildsChanged;

    public event EventHandler? SavedToDisk;

    #endregion

    #region Properties

    [DefaultValue(true)]
    public bool DropMessages { get; set; } = true;

    public ReadOnlyCollection<FormulaScriptDescription> EventScript {
        get => new(_eventScript);
        set {
            var l = new List<FormulaScriptDescription>();
            l.AddRange(value);
            l.Sort();

            var tmp = l.ToString(true);

            if (_eventScriptTmp == tmp) { return; }

            _eventScriptTmp = tmp;
            EventScript_RemoveAll(true);

            foreach (var t in l) {
                EventScript_Add(t, true);
            }
            _saved = false;
            //CheckScriptError();
        }
    }

    public string Filename => _muf?.Filename ?? string.Empty;

    // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    // ~ConnectedFormula()
    // {
    //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
    //     Dispose(disposing: false);
    // }
    public bool IsDisposed { get; private set; }

    public string KeyName => Filename;

    public ReadOnlyCollection<string> NotAllowedChilds {
        get => new(_notAllowedChilds);
        set {
            var l = new List<string>(value).SortedDistinctList();
            if (_notAllowedChilds.JoinWithCr() == l.JoinWithCr()) { return; }
            _notAllowedChilds.Clear();
            _notAllowedChilds.AddRange(l);
            _saved = false;
            OnNotAllowedChildsChanged();
        }
    }

    public ItemCollectionPad.ItemCollectionPad? PadData {
        get => _padData;
        private set {
            if (_padData == value) { return; }

            if (_padData != null) {
                _padData.Changed -= PadData_Changed;
            }
            _padData = value;
            if (_padData != null) {
                _padData.Changed += PadData_Changed;
            }

            if (_saving || (_muf?.IsLoading ?? false)) { return; }

            _saved = false;
        }
    }

    public VariableCollection Variables {
        get => new(_variables);
        set {
            var l = new List<VariableString>();
            l.AddRange(value.ToListVariableString());
            l.Sort();

            var tmp = l.ToString(true);
            if (_variableTmp == tmp) { return; }

            _variableTmp = tmp;
            Variables_RemoveAll(true);

            foreach (var t in value) {
                if (t is VariableString ts) {
                    ts.ReadOnly = true; // Weil kein onChangedEreigniss vorhanden ist
                    Variables_Add(ts, true);
                }
            }

            _saved = false;
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gibt das Formular zurück.
    /// Zuerst wird geprüft, ob es bereits geladen ist. Falls nicht, wird es geladen.
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static ConnectedFormula? GetByFilename(string filename) {
        if (string.IsNullOrEmpty(filename)) { return null; }

        foreach (var thisFile in AllFiles) {
            if (thisFile != null && string.Equals(thisFile.Filename, filename, StringComparison.OrdinalIgnoreCase)) {
                return thisFile;
            }
        }

        return !FileExists(filename) ? null : new ConnectedFormula(filename);
    }

    public static List<RectangleF> ResizeControls(List<IAutosizable> its, float newWidth, float newHeight, float currentWidth, float currentHeight) {
        var scaleY = newHeight / currentHeight;
        var scaleX = newWidth / currentWidth;

        #region Alle Items an die neue gedachte Y-Position schieben (newY), neue bevorzugte Höhe berechnen (newH), und auch newX und newW

        List<float> newX = new();
        List<float> newW = new();
        List<float> newY = new();
        List<float> newH = new();
        foreach (var thisIt in its) {

            #region  newY

            newY.Add(thisIt.UsedArea.Y * scaleY);

            #endregion

            #region  newX

            newX.Add(thisIt.UsedArea.X * scaleX);

            #endregion

            #region  newH

            var nh = thisIt.UsedArea.Height * scaleY;

            if (thisIt.AutoSizeableHeight) {
                if (!thisIt.CanChangeHeightTo(nh)) {
                    nh = IAutosizableExtension.MinHeigthCapAndBox;
                }
            } else {
                nh = thisIt.UsedArea.Height;
            }

            newH.Add(nh);

            #endregion

            #region  newW

            newW.Add(thisIt.UsedArea.Width * scaleX);

            #endregion
        }

        #endregion

        #region  Alle Items von unten nach oben auf Überlappungen (auch dem Rand) prüfen.

        // Alle prüfen

        for (var tocheck = its.Count - 1; tocheck >= 0; tocheck--) {
            var pos = PositioOf(tocheck);

            #region Unterer Rand

            if (pos.Bottom > newHeight) {
                newY[tocheck] = newHeight - pos.Height;
                pos = PositioOf(tocheck);
            }

            #endregion

            for (var coll = its.Count - 1; coll > tocheck; coll--) {
                var poscoll = PositioOf(coll);
                if (pos.IntersectsWith(poscoll)) {
                    newY[tocheck] = poscoll.Top - pos.Height;
                    pos = PositioOf(tocheck);
                }
            }
        }

        #endregion

        #region  Alle UNveränderlichen Items von oben nach unten auf Überlappungen (auch dem Rand) prüfen.

        // Und von oben nach unten muss sein, weil man ja oben bündig haben will
        // Wichtig, das CanScaleHeightTo nochmal geprüft wird.
        // Nur so kann festgestellt werden, ob es eigentlich veränerlich wäre, aber durch die Mini-Größe doch als unveränderlich gilt

        for (var tocheck = 0; tocheck < its.Count; tocheck++) {
            if (!its[tocheck].CanScaleHeightTo(scaleY)) {
                var pos = PositioOf(tocheck);

                #region Oberer Rand

                if (pos.Y < 0) {
                    newY[tocheck] = 0;
                    pos = PositioOf(tocheck);
                }

                #endregion

                for (var coll = 0; coll < tocheck; coll++) {
                    if (!its[tocheck].CanScaleHeightTo(scaleY)) {
                        var poscoll = PositioOf(coll);
                        if (pos.IntersectsWith(poscoll)) {
                            newY[tocheck] = poscoll.Top + poscoll.Height;
                            pos = PositioOf(tocheck);
                        }
                    }
                }
            }
        }

        #endregion

        #region Alle Items, den Abstand stutzen, wenn der vorgänger unveränderlich ist - nur bei ScaleY >1

        if (scaleY > 1) {
            for (var tocheck = 0; tocheck < its.Count; tocheck++) {
                if (!its[tocheck].CanScaleHeightTo(scaleY)) {
                    //var pos = PositioOf(tocheck);

                    for (var coll = tocheck + 1; coll < its.Count; coll++) {
                        //var poscoll = PositioOf(coll);

                        if (its[coll].UsedArea.Y >= its[tocheck].UsedArea.Bottom && its[coll].UsedArea.IntersectsVericalyWith(its[tocheck].UsedArea)) {
                            newY[coll] = newY[tocheck] + newH[tocheck] + its[coll].UsedArea.Top - its[tocheck].UsedArea.Bottom;
                            //pos = PositioOf(tocheck);
                        }
                    }
                }
            }
        }

        #endregion

        #region  Alle veränderlichen Items von oben nach unten auf Überlappungen (auch dem Rand) prüfen - nur den Y-Wert.

        for (var tocheck = 0; tocheck < its.Count; tocheck++) {
            if (its[tocheck].CanScaleHeightTo(scaleY)) {
                var pos = PositioOf(tocheck);

                #region Oberer Rand

                if (pos.Y < 0) {
                    newY[tocheck] = 0;
                    pos = PositioOf(tocheck);
                }

                #endregion

                for (var coll = 0; coll < tocheck; coll++) {
                    var poscoll = PositioOf(coll);
                    if (pos.IntersectsWith(poscoll)) {
                        newY[tocheck] = poscoll.Top + poscoll.Height;
                        pos = PositioOf(tocheck);
                    }
                }
            }
        }

        #endregion

        #region  Alle veränderlichen Items von oben nach unten auf Überlappungen (auch dem Rand) prüfen - nur den Height-Wert stutzen.

        for (var tocheck = 0; tocheck < its.Count; tocheck++) {
            if (its[tocheck].CanScaleHeightTo(scaleY)) {
                var pos = PositioOf(tocheck);

                #region  Unterer Rand

                if (pos.Bottom > newHeight) {
                    newH[tocheck] = newHeight - pos.Y;
                    pos = PositioOf(tocheck);
                }

                #endregion

                #region  Alle Items stimmen mit dem Y-Wert, also ALLE prüfen, NACH dem Item

                for (var coll = tocheck + 1; coll < its.Count; coll++) {
                    var poscoll = PositioOf(coll);
                    if (pos.IntersectsWith(poscoll)) {
                        newH[tocheck] = poscoll.Top - pos.Top;
                        pos = PositioOf(tocheck);
                    }
                }

                #endregion
            }
        }

        #endregion

        #region Feedback-Liste erstellen (p)

        var p = new List<RectangleF>();
        for (var ite = 0; ite < its.Count; ite++) {
            p.Add(PositioOf(ite));
        }

        #endregion

        return p;

        RectangleF PositioOf(int no) {
            return new RectangleF(newX[no], newY[no], newW[no], newH[no]);
        }
    }

    public static List<(IAutosizable item, RectangleF newpos)> ResizeControls(ItemCollectionPad.ItemCollectionPad padData, float newWidthPixel, float newhHeightPixel, string page) {

        #region Items und Daten in einer sortierene Liste ermitteln, die es betrifft (its)

        List<IAutosizable> its = new();

        foreach (var thisc in padData) {
            if (thisc.IsVisibleOnPage(page) && thisc is IAutosizable aas && aas.IsVisibleForMe()) {
                its.Add(aas);
            }
        }

        its.Sort((it1, it2) => it1.UsedArea.Y.CompareTo(it2.UsedArea.Y));

        #endregion

        var p = ResizeControls(its, newWidthPixel, newhHeightPixel, padData.SheetSizeInPix.Width, padData.SheetSizeInPix.Height);

        var erg = new List<(IAutosizable item, RectangleF newpos)>();

        for (var x = 0; x < its.Count; x++) {
            erg.Add((its[x], p[x]));
        }

        return erg;
    }

    public string CheckScriptError() {
        List<string> names = new();

        foreach (var thissc in _eventScript) {
            if (!thissc.IsOk()) {
                return thissc.Name + ": " + thissc.ErrorReason();
            }

            if (names.Contains(thissc.Name, false)) {
                return "Skriptname '" + thissc.Name + "' mehrfach vorhanden";
            }
        }

        var l = EventScript;
        if (l.Get(ScriptEventTypes.export).Count > 1) {
            return "Skript 'Export' mehrfach vorhanden";
        }

        if (l.Get(ScriptEventTypes.loaded).Count > 1) {
            return "Skript 'Datenank geladen' mehrfach vorhanden";
        }

        if (l.Get(ScriptEventTypes.prepare_formula).Count > 1) {
            return "Skript 'Formular Vorbereitung' mehrfach vorhanden";
        }

        if (l.Get(ScriptEventTypes.value_changed_extra_thread).Count > 1) {
            return "Skript 'Wert geändert Extra Thread' mehrfach vorhanden";
        }

        if (l.Get(ScriptEventTypes.new_row).Count > 1) {
            return "Skript 'Neue Zeile' mehrfach vorhanden";
        }

        if (l.Get(ScriptEventTypes.value_changed).Count > 1) {
            return "Skript 'Wert geändert' mehrfach vorhanden";
        }

        return string.Empty;
    }

    public void DiscardPendingChanges(object sender, System.EventArgs e) => _saved = true;

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public void EventScript_Add(FormulaScriptDescription ev, bool isLoading) {
        _eventScript.Add(ev);
        ev.Changed += EventScript_Changed;

        if (!isLoading) { EventScript_Changed(this, System.EventArgs.Empty); }
    }

    public void EventScript_RemoveAll(bool isLoading) {
        while (_eventScript.Count > 0) {
            var ev = _eventScript[_eventScript.Count - 1];
            ev.Changed -= EventScript_Changed;

            _eventScript.RemoveAt(_eventScript.Count - 1);
        }

        if (!isLoading) { EventScript_Changed(this, System.EventArgs.Empty); }
    }

    public ScriptEndedFeedback ExecuteScript(ScriptEventTypes? eventname, string? scriptname, bool changevalues, RowItem? row) {
        try {
            if (IsDisposed) { return new ScriptEndedFeedback("Formular verworfen", false, false, "Allgemein"); }

            //var m = EditableErrorReason(EditableErrorReasonType.EditCurrently);

            //if (!string.IsNullOrEmpty(m)) { return new ScriptEndedFeedback("Automatische Prozesse nicht möglich: " + m, false, "Allgemein"); }

            #region Script ermitteln

            if (eventname != null && !string.IsNullOrEmpty(scriptname)) {
                DebugPrint(FehlerArt.Fehler, "Event und Skript angekommen!");
                return new ScriptEndedFeedback("Event und Skript angekommen!", false, false, "Allgemein");
            }

            if (eventname == null && string.IsNullOrEmpty(scriptname)) { return new ScriptEndedFeedback("Kein Eventname oder Skript angekommen", false, false, "Allgemein"); }

            if (string.IsNullOrEmpty(scriptname) && eventname != null) {
                var l = EventScript.Get((ScriptEventTypes)eventname);
                if (l.Count == 1) { scriptname = l[0].Name; }
                if (string.IsNullOrEmpty(scriptname)) { return new ScriptEndedFeedback(string.Empty, false, false, string.Empty); }
            }

            if (scriptname == null || string.IsNullOrWhiteSpace(scriptname)) { return new ScriptEndedFeedback("Kein Skriptname angekommen", false, false, "Allgemein"); }

            var script = EventScript.Get(scriptname);

            if (script == null) { return new ScriptEndedFeedback("Skript nicht gefunden.", false, false, scriptname); }

            //if (script.NeedRow && row == null) { return new ScriptEndedFeedback("Zeilenskript aber keine Zeile angekommen.", false, scriptname); }

            //if (!script.NeedRow) { row = null; }

            #endregion

            //if (!script.ChangeValues) { changevalues = false; }

            return ExecuteScript(script);
        } catch {
            CheckStackForOverflow();
            return ExecuteScript(eventname, scriptname, changevalues, row);
        }
    }

    public void HasPendingChanges(object sender, MultiUserFileHasPendingChangesEventArgs e) {
        if (!_saved) { e.HasPendingChanges = true; return; }

        if (IntParse(_loadedVersion.Replace(".", string.Empty)) < IntParse(Version.Replace(".", string.Empty))) {
            e.HasPendingChanges = true;
        }
    }

    public bool IsAdministrator() {
        if (string.Equals(UserGroup, Constants.Administrator, StringComparison.OrdinalIgnoreCase)) { return true; }
        //if (_datenbankAdmin == null || _datenbankAdmin.Count == 0) { return false; }
        //if (_datenbankAdmin.Contains(Constants.Everybody, false)) { return true; }
        //if (!string.IsNullOrEmpty(UserName) && _datenbankAdmin.Contains("#User: " + UserName, false)) { return true; }
        //return !string.IsNullOrEmpty(UserGroup) && _datenbankAdmin.Contains(UserGroup, false);
        return false;
    }

    public void OnChanged() => Changed?.Invoke(this, System.EventArgs.Empty);

    public void OnNotAllowedChildsChanged() => NotAllowedChildsChanged?.Invoke(this, System.EventArgs.Empty);

    public void Repair() {
        // Reparatur-Routine

        PadData ??= new ItemCollectionPad.ItemCollectionPad();

        PadData.BackColor = Skin.Color_Back(Design.Form_Standard, States.Standard);

        foreach (var thisCon in PadData.Connections) {
            thisCon.Bei_Export_sichtbar = false;
        }

        foreach (var thisIt in PadData) {
            if (string.IsNullOrEmpty(thisIt.Page)) {
                thisIt.Page = "Head";
            }

            if (thisIt is IHasConnectedFormula itcf) {
                itcf.CFormula = this;
            }
        }

        var pg = PadData.AllPages();
        pg.AddIfNotExists("Head");

        foreach (var thisP in pg) {
            RowEntryPadItem? found = null;

            foreach (var thisit in PadData) {
                if (thisit is RowEntryPadItem repi) {
                    if (string.Equals(thisP, repi.Page, StringComparison.OrdinalIgnoreCase)) { found = repi; break; }
                }
            }
            if (found == null) {
                found = new RowEntryPadItem(string.Empty);

                PadData.Add(found);
            }

            found.SetCoordinates(new RectangleF((PadData.SheetSizeInPix.Width / 2) - 150, -30, 300, 30), true);
            found.Page = thisP;
            found.Bei_Export_sichtbar = false;
        }
    }

    public void Save() => _muf?.Save(true);

    public void Variables_Add(VariableString va, bool isLoading) {
        _variables.Add(va);
        //ev.Changed += EventScript_Changed;
        if (!isLoading) { Variables_Changed(); }
    }

    public void Variables_RemoveAll(bool isLoading) {
        while (_variables.Count > 0) {
            //var va = _variables[_eventScript.Count - 1];
            //ev.Changed -= EventScript_Changed;

            _variables.RemoveAt(_variables.Count - 1);
        }

        if (!isLoading) { Variables_Changed(); }
    }

    internal ScriptEndedFeedback ExecuteScript(FormulaScriptDescription s) {
        if (IsDisposed) { return new ScriptEndedFeedback("Formular verworfen", false, false, s.Name); }

        var sce = CheckScriptError();
        if (!string.IsNullOrEmpty(sce)) { return new ScriptEndedFeedback("Die Skripte enthalten Fehler: " + sce, false, true, "Allgemein"); }

        try {

            #region Variablen für Skript erstellen

            VariableCollection vars = new();

            foreach (var thisvar in Variables.ToListVariableString()) {
                var v = new VariableString("Formula_" + thisvar.Name, thisvar.ValueString, false, false, "Formular-Kopf-Variable\r\n" + thisvar.Comment);
                vars.Add(v);
            }

            vars.Add(new VariableString("User", UserName, true, false, "ACHTUNG: Keinesfalls dürfen benutzerabhängig Werte verändert werden."));
            vars.Add(new VariableString("Usergroup", UserGroup, true, false, "ACHTUNG: Keinesfalls dürfen gruppenabhängig Werte verändert werden."));
            vars.Add(new VariableBool("Administrator", IsAdministrator(), true, false, "ACHTUNG: Keinesfalls dürfen gruppenabhängig Werte verändert werden.\r\nDiese Variable gibt zurück, ob der Benutzer Admin für diese Datenbank ist."));

            #endregion

            #region  Erlaubte Methoden ermitteln

            var allowedMethods = MethodType.Standard | MethodType.Database;

            //if (row != null && !row.IsDisposed) { allowedMethods |= MethodType.MyDatabaseRow; }
            if (!s.EventTypes.HasFlag(ScriptEventTypes.prepare_formula)) {
                allowedMethods |= MethodType.IO;
                allowedMethods |= MethodType.NeedLongTime;
            }

            if (!s.EventTypes.HasFlag(ScriptEventTypes.value_changed_extra_thread) &&
                !s.EventTypes.HasFlag(ScriptEventTypes.prepare_formula) &&
                !s.EventTypes.HasFlag(ScriptEventTypes.loaded)) {
                allowedMethods |= MethodType.ManipulatesUser;
            }

            allowedMethods |= MethodType.ChangeAnyDatabaseOrRow;

            #endregion

            #region Script ausführen

            var scp = new ScriptProperties(allowedMethods, true, s.Attributes());
            Script sc = new(vars, string.Empty, scp) {
                ScriptText = s.ScriptText
            };
            var scf = sc.Parse(0, s.Name);

            #endregion

            #region Variablen zurückschreiben und Special Rules ausführen

            if (sc.ChangeValues && scf.AllOk) {
                Variables = DatabaseAbstract.WriteBackDbVariables(vars, Variables, "Formula_");
            }

            if (!scf.AllOk) {
                OnDropMessage(FehlerArt.Info, "Das Skript '" + s.Name + "' hat einen Fehler verursacht\r\n" + scf.Protocol[0]);
            }

            #endregion

            return scf;
        } catch {
            CheckStackForOverflow();
            return ExecuteScript(s);
        }
    }

    /// <summary>
    /// Prüft, ob das Formular sichtbare Elemente hat.
    /// Zeilenselectionen werden dabei ignoriert.
    /// </summary>
    /// <param name="page">Wird dieser Wert leer gelassen, wird das komplette Formular geprüft</param>
    /// <returns></returns>
    internal bool HasVisibleItemsForMe(string page) {
        if (_padData == null) { return false; }

        foreach (var thisItem in _padData) {
            if (string.IsNullOrEmpty(page) ||
                string.IsNullOrEmpty(thisItem.Page) ||
                page.Equals(thisItem.Page, StringComparison.OrdinalIgnoreCase)) {
                if (thisItem is FakeControlPadItem cspi) {
                    if (cspi.IsVisibleForMe()) { return true; }
                }
            }
        }

        return false;
    }

    internal void OnDropMessage(FehlerArt type, string message) {
        if (IsDisposed) { return; }
        if (!DropMessages) { return; }
        DropMessage?.Invoke(this, new MessageEventArgs(type, message));
    }

    internal void Resize(float newWidthPixel, float newhHeightPixel, bool changeControls) {
        if (PadData == null) { return; }

        if (changeControls) {
            //var newWidthPixel = MmToPixel(newwidthinmm, ItemCollectionPad.Dpi);
            //var newhHeightPixel = MmToPixel(newheightinmm, ItemCollectionPad.Dpi);

            foreach (var thisPage in PadData.AllPages()) {
                var x = ResizeControls(PadData, newWidthPixel, newhHeightPixel, thisPage);

                #region Die neue Position in die Items schreiben

                foreach (var (item, newpos) in x) {
                    item.SetCoordinates(newpos, true);
                }

                #endregion
            }
        }

        PadData.SheetSizeInMm = new SizeF(PixelToMm(newWidthPixel, ItemCollectionPad.ItemCollectionPad.Dpi), PixelToMm(newhHeightPixel, ItemCollectionPad.ItemCollectionPad.Dpi));
    }

    internal void SaveAsAndChangeTo(string fileName) => _muf?.SaveAsAndChangeTo(fileName);

    protected virtual void Dispose(bool disposing) {
        if (!IsDisposed) {
            _ = AllFiles.Remove(this);
            if (disposing) {
                _ = _muf?.Save(true);
                _muf?.Dispose();
                _muf = null;
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }

            // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
            // TODO: Große Felder auf NULL setzen
            IsDisposed = true;
        }
    }

    protected void ParseExternal(object sender, MultiUserParseEventArgs e) {
        var toParse = e.Data.ToStringWin1252();
        if (string.IsNullOrEmpty(toParse)) { return; }

        foreach (var pair in toParse.GetAllTags()) {
            switch (pair.Key.ToLower()) {
                case "type":
                    break;

                case "version":
                    _loadedVersion = pair.Value;
                    break;

                //case "filepath":
                //    FilePath = pair.Value.FromNonCritical();
                //    break;

                case "databasefiles":
                    _databaseFiles.Clear();
                    _databaseFiles.AddRange(pair.Value.FromNonCritical().SplitByCrToList());
                    DatabaseFiles_Changed();
                    break;

                case "notallowedchilds":
                    _notAllowedChilds.Clear();
                    _notAllowedChilds.AddRange(pair.Value.FromNonCritical().SplitByCrToList());
                    OnNotAllowedChildsChanged();
                    break;

                case "createdate":
                    _createDate = pair.Value.FromNonCritical();
                    break;

                case "createname":
                    _creator = pair.Value.FromNonCritical();
                    break;

                case "paditemdata":
                    PadData = new ItemCollectionPad.ItemCollectionPad(pair.Value.FromNonCritical(), string.Empty);
                    break;

                case "lastusedid":
                    _id = IntParse(pair.Value);
                    break;

                case "events":
                    _eventScriptTmp = pair.Value;
                    EventScript_RemoveAll(true);
                    List<string> ai = new(pair.Value.FromNonCritical().SplitAndCutByCr());
                    foreach (var t in ai) {
                        EventScript_Add(new FormulaScriptDescription(this, t.FromNonCritical()), true);
                    }

                    break;

                case "variables":
                    _variableTmp = pair.Value;
                    Variables_RemoveAll(true);
                    List<string> va = new(pair.Value.FromNonCritical().SplitAndCutByCr());
                    foreach (var t in va) {
                        var l = new VariableString("dummy");
                        l.Parse(t.FromNonCritical());
                        l.ReadOnly = true; // Weil kein onChangedEreigniss vorhanden ist
                        Variables_Add(l, true);
                    }
                    break;

                default:
                    DebugPrint(FehlerArt.Warnung, "Tag unbekannt: " + pair.Key);
                    break;
            }
        }
    }

    private void _muf_Saving(object sender, CancelEventArgs e) {
        if (e.Cancel) { return; }

        e.Cancel = IntParse(_loadedVersion.Replace(".", string.Empty)) > IntParse(Version.Replace(".", string.Empty));

        //return IntParse(LoadedVersion.Replace(".", string.Empty)) > IntParse(DatabaseVersion.Replace(".", string.Empty))
        //    ? "Diese Programm kann nur Datenbanken bis Version " + DatabaseVersion + " speichern."
        //    : string.Empty;
    }

    private void DatabaseFiles_Changed() {
        if (_saving || (_muf?.IsLoading ?? true)) { return; }

        foreach (var thisfile in _databaseFiles) {
            _ = DatabaseAbstract.GetById(new ConnectionInfo(thisfile, null), null);
        }

        _saved = false;
    }

    private void EventScript_Changed(object sender, System.EventArgs e) => EventScript = _eventScript.AsReadOnly();

    //private void NotAllowedChilds_Changed(object sender, System.EventArgs e) {
    //    if (_saving || (_muf?.IsLoading ?? true)) { return; }
    //    _saved = false;
    //}
    private void OnLoaded(object sender, System.EventArgs e) {
        Repair();

        Loaded?.Invoke(this, e);
        _ = ExecuteScript(ScriptEventTypes.loaded, string.Empty, true, null);
    }

    private void OnLoading(object sender, System.EventArgs e) => Loading?.Invoke(this, e);

    private void OnSavedToDisk(object sender, System.EventArgs e) {
        _saved = true;
        _loadedVersion = Version;
        SavedToDisk?.Invoke(this, e);
    }

    private void PadData_Changed(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }
        if (_saving || (_muf?.IsLoading ?? true)) { return; }

        _saved = false;
        OnChanged();
    }

    private void ToListOfByte(object sender, MultiUserToListEventArgs e) {

        #region ein bischen aufräumen zuvor

        _saving = true;
        //PadData.Sort();

        //_id = -1;

        _databaseFiles.Clear();

        //foreach (var thisit in PadData) {
        //    if (thisit is IItemSendRow rwf) {
        //        if (rwf.OutputDatabase != null) {
        //            _ = _databaseFiles.AddIfNotExists(rwf.OutputDatabase.ConnectionData.UniqueID);
        //            _id = Math.Max(_id, rwf.InputColorId);
        //        }
        //    }
        //}
        _saving = false;

        #endregion

        var t = new List<string>();

        t.ParseableAdd("Type", "ConnectedFormula");
        t.ParseableAdd("Version", Version);
        t.ParseableAdd("CreateDate", _createDate);
        t.ParseableAdd("CreateName", _creator);
        t.ParseableAdd("LastUsedID", _id);
        t.ParseableAdd("DatabaseFiles", _databaseFiles);
        t.ParseableAdd("NotAllowedChilds", _notAllowedChilds);

        if (PadData != null) {
            t.ParseableAdd("PadItemData", PadData.ToString());
        }

        if (Variables.Count > 0) {
            t.ParseableAdd("Variables", Variables.ToList().ToString(true));
        }

        if (EventScript.Count > 0) {
            t.ParseableAdd("Events", EventScript.ToString(true));
        }

        e.Data = t.Parseable().WIN1252_toByte();
    }

    private void Variables_Changed() => Variables = new VariableCollection(_variables);

    #endregion
}