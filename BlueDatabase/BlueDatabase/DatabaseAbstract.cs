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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using BlueScript;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueBasics.Converter;
using static BlueBasics.IO;
using static BlueBasics.Generic;
using Timer = System.Threading.Timer;

namespace BlueDatabase;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class DatabaseAbstract : IDisposableExtended, IHasKeyName {

    #region Fields

    public const string DatabaseVersion = "4.02";

    public static readonly ObservableCollection<DatabaseAbstract> AllFiles = new();
    public static List<Type>? DatabaseTypes;
    private static DateTime _lastTableCheck = new(1900, 1, 1);
    private readonly List<ColumnViewCollection> _columnArrangements = new();
    private readonly List<string> _datenbankAdmin = new();
    private readonly List<EventScript> _eventScript = new();
    private readonly LayoutCollection _layouts = new();

    private readonly List<string> _permissionGroupsNewRow = new();
    private readonly List<string> _tags = new();
    private readonly List<Variable> _variables = new();
    private string _additionalFilesPfad = string.Empty;
    private string _cachePfad = string.Empty;
    private string _caption = string.Empty;
    private Timer? _checker;
    private int _checkerTickCount = -5;
    private string _createDate = string.Empty;
    private string _creator = string.Empty;
    private string _eventScriptTmp = string.Empty;

    private double _globalScale;
    private string _globalShowPass = string.Empty;
    private RowSortDefinition? _sortDefinition;

    /// <summary>
    /// Die Eingabe des Benutzers. Ist der Pfad gew�nscht, muss FormulaFileName benutzt werden.
    /// </summary>
    private string _standardFormulaFile = string.Empty;

    //private string _timeCode = string.Empty;
    private int _undoCount;

    private string _variableTmp = string.Empty;
    private string _zeilenQuickInfo = string.Empty;

    #endregion

    #region Constructors

    protected DatabaseAbstract(bool readOnly) {
        Develop.StartService();

        ReadOnly = readOnly;

        Cell = new CellCollection(this);

        QuickImage.NeedImage += QuickImage_NeedImage;

        Row = new RowCollection(this);
        Column = new ColumnCollection(this);

        // Muss vor dem Laden der Datan zu Allfiles hinzugf�gt werde, weil das bei OnAdded
        // Die Events registriert werden, um z.B: das Passwort abzufragen
        // Zus�tzlich werden z.B: Filter f�r den Export erstellt - auch der muss die Datenbank finden k�nnen.
        // Zus�tzlich muss der Tablename stimme, dass in Added diesen verwerten kann.
        AllFiles.Add(this);
    }

    #endregion

    #region Delegates

    public delegate string NeedPassword();

    #endregion

    #region Events

    public event EventHandler? Disposing;

    public event EventHandler<MessageEventArgs>? DropMessage;

    public event EventHandler? Loaded;

    public event EventHandler? Loading;

    public event EventHandler<ProgressbarEventArgs>? ProgressbarInfo;

    public event EventHandler<RowCancelEventArgs>? ScriptError;

    public event EventHandler? SortParameterChanged;

    public event EventHandler? ViewChanged;

    #endregion

    #region Properties

    public static List<ConnectionInfo> Allavailabletables { get; } = new();

    [Browsable(false)]
    [Description("In diesem Pfad suchen verschiedene Routinen (Spalten Bilder, Layouts, etc.) nach zus�tzlichen Dateien.")]
    public string AdditionalFilesPfad {
        get => _additionalFilesPfad;
        set {
            if (_additionalFilesPfad == value) { return; }
            AdditionalFilesPfadtmp = null;
            _ = ChangeData(DatabaseDataType.AdditionalFilesPath, null, null, _additionalFilesPfad, value, string.Empty);
            Cell.InvalidateAllSizes();
        }
    }

    [Browsable(false)]
    public string CachePfad {
        get => _cachePfad;
        set {
            // ReSharper disable once RedundantCheckBeforeAssignment
            if (_cachePfad == value) { return; }
            _cachePfad = value;
        }
    }

    [Browsable(false)]
    [Description("Der Name der Datenbank.")]
    public string Caption {
        get => _caption;
        set {
            if (_caption == value) { return; }
            _ = ChangeData(DatabaseDataType.Caption, null, null, _caption, value, string.Empty);
        }
    }

    public CellCollection Cell { get; }
    public ColumnCollection Column { get; }

    public ReadOnlyCollection<ColumnViewCollection> ColumnArrangements {
        get => new(_columnArrangements);
        set {
            if (_columnArrangements.ToString(false) == value.ToString(false)) { return; }
            _ = ChangeData(DatabaseDataType.ColumnArrangement, null, null, _columnArrangements.ToString(false), value.ToString(false), string.Empty);
            OnViewChanged();
        }
    }

    public abstract ConnectionInfo ConnectionData { get; }

    [Browsable(false)]
    public string CreateDate {
        get => _createDate;
        set {
            if (_createDate == value) { return; }
            _ = ChangeData(DatabaseDataType.CreateDateUTC, null, null, _createDate, value, string.Empty);
        }
    }

    [Browsable(false)]
    public string Creator {
        get => _creator.Trim();
        set {
            if (_creator == value) { return; }
            _ = ChangeData(DatabaseDataType.Creator, null, null, _creator, value, string.Empty);
        }
    }

    public ReadOnlyCollection<string> DatenbankAdmin {
        get => new(_datenbankAdmin);
        set {
            if (!_datenbankAdmin.IsDifferentTo(value)) { return; }
            _ = ChangeData(DatabaseDataType.DatabaseAdminGroups, null, null, _datenbankAdmin.JoinWithCr(), value.JoinWithCr(), string.Empty);
        }
    }

    [DefaultValue(true)]
    public bool DropMessages { get; set; } = true;

    public ReadOnlyCollection<EventScript> EventScript {
        get => new(_eventScript);
        set {
            var l = new List<EventScript>();
            l.AddRange(value);
            l.Sort();

            if (_eventScriptTmp == l.ToString(false)) { return; }
            _ = ChangeData(DatabaseDataType.EventScript, null, null, _eventScriptTmp, l.ToString(true), string.Empty);
        }
    }

    [Browsable(false)]
    public double GlobalScale {
        get => _globalScale;
        set {
            if (Math.Abs(_globalScale - value) < 0.0001) { return; }
            _ = ChangeData(DatabaseDataType.GlobalScale, null, null, _globalScale.ToString(CultureInfo.InvariantCulture), value.ToString(CultureInfo.InvariantCulture), string.Empty);
            Cell.InvalidateAllSizes();
        }
    }

    public string GlobalShowPass {
        get => _globalShowPass;
        set {
            if (_globalShowPass == value) { return; }
            _ = ChangeData(DatabaseDataType.GlobalShowPass, null, null, _globalShowPass, value, string.Empty);
        }
    }

    //[Browsable(false)]
    //[Description("Welche Spalte bei Columnfirst zur�ckgegeben wird")]
    //public string FirstColumn {
    //    get => _firstColumn;
    //    set {
    //        if (_firstColumn == value) { return; }
    //        ChangeData(DatabaseDataType.FirstColumn, null, null, _firstColumn, value, string.Empty);
    //    }
    //}
    public bool HasPendingChanges { get; protected set; } = false;

    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Letzter Lade-Stand der Daten. Wird in OnLoaded gesetzt
    /// </summary>
    public DateTime? IsInCache { get; private set; }

    public string KeyName {
        get {
            if (IsDisposed) { return string.Empty; }
            return ConnectionData.UniqueID;
        }
    }

    public LayoutCollection Layouts {
        get => _layouts;
        set {
            if (!_layouts.IsDifferentTo(value)) { return; }
            _ = ChangeData(DatabaseDataType.Layouts, null, null, _layouts.JoinWithCr(), value.JoinWithCr(), string.Empty);
        }
    }

    public string LoadedVersion { get; private set; } = "0.00";
    public bool LogUndo { get; set; } = true;

    public ReadOnlyCollection<string> PermissionGroupsNewRow {
        get => new(_permissionGroupsNewRow);
        set {
            if (!_permissionGroupsNewRow.IsDifferentTo(value)) { return; }
            _ = ChangeData(DatabaseDataType.PermissionGroupsNewRow, null, null, _permissionGroupsNewRow.JoinWithCr(), value.JoinWithCr(), string.Empty);
        }
    }

    public DateTime PowerEdit { get; set; }
    public bool ReadOnly { get; private set; }
    public RowCollection Row { get; }

    [Browsable(false)]
    public RowSortDefinition? SortDefinition {
        get => _sortDefinition;
        set {
            var alt = string.Empty;
            var neu = string.Empty;
            if (_sortDefinition != null) { alt = _sortDefinition.ToString(); }
            if (value != null) { neu = value.ToString(); }
            if (alt == neu) { return; }
            _ = ChangeData(DatabaseDataType.SortDefinition, null, null, alt, neu, string.Empty);

            OnSortParameterChanged();
        }
    }

    /// <summary>
    /// Die Eingabe des Benutzers. Ist der Pfad gew�nscht, muss FormulaFileName benutzt werden.
    /// </summary>
    [Browsable(false)]
    [Description("Das standardm��ige Formular - dessen Dateiname -, das angezeigt werden soll.")]
    public string StandardFormulaFile {
        get => _standardFormulaFile;
        set {
            if (_standardFormulaFile == value) { return; }
            _ = ChangeData(DatabaseDataType.StandardFormulaFile, null, null, _standardFormulaFile, value, string.Empty);
        }
    }

    public abstract string TableName { get; }

    public ReadOnlyCollection<string> Tags {
        get => new(_tags);
        set {
            if (!_tags.IsDifferentTo(value)) { return; }
            _ = ChangeData(DatabaseDataType.Tags, null, null, _tags.JoinWithCr(), value.JoinWithCr(), string.Empty);
        }
    }

    [Browsable(false)]
    public int UndoCount {
        get => _undoCount;
        set {
            if (_undoCount == value) { return; }
            _ = ChangeData(DatabaseDataType.UndoCount, null, null, _undoCount.ToString(), value.ToString(), string.Empty);
        }
    }

    public VariableCollection Variables {
        get => new(_variables);
        set {
            var l = new List<VariableString>();
            l.AddRange(value.ToListVariableString());
            l.Sort();

            if (_variableTmp == l.ToString(true)) { return; }
            _ = ChangeData(DatabaseDataType.DatabaseVariables, null, null, _variableTmp, l.ToString(true), string.Empty);
            //OnViewChanged();
        }
    }

    public int VorhalteZeit { get; set; } = 30;

    [Browsable(false)]
    public string ZeilenQuickInfo {
        get => _zeilenQuickInfo;
        set {
            if (_zeilenQuickInfo == value) { return; }
            _ = ChangeData(DatabaseDataType.RowQuickInfo, null, null, _zeilenQuickInfo, value, string.Empty);
        }
    }

    protected string? AdditionalFilesPfadtmp { get; set; }

    #endregion

    #region Methods

    public static List<ConnectionInfo> AllAvailableTables() {
        if (DateTime.Now.Subtract(_lastTableCheck).TotalMinutes < 1) {
            return Allavailabletables.Clone(); // Als Clone, damit bez�ge gebrochen werden und sich die Auflistung nicht mehr ver�ndern kann
        }

        // Wird benutzt, um z.b. das Dateisystem nicht doppelt und dreifach abzufragen.
        // Wenn eine Datenbank z.B. im gleichen Verzeichnis liegt,
        // reicht es, das Verzeichnis einmal zu pr�fen
        var allreadychecked = new List<DatabaseAbstract>();

        var alf = new List<DatabaseAbstract>();// k�nnte sich �ndern, deswegen Zwischenspeichern
        alf.AddRange(AllFiles);

        foreach (var thisDb in alf) {
            var possibletables = thisDb.AllAvailableTables(allreadychecked);

            allreadychecked.Add(thisDb);

            if (possibletables != null) {
                foreach (var thistable in possibletables) {
                    var canadd = true;

                    #region pr�fen, ob schon voranden, z.b. DatabaseAbstract.AllFiles

                    foreach (var checkme in Allavailabletables) {
                        if (string.Equals(checkme.TableName, thistable.TableName, StringComparison.InvariantCultureIgnoreCase)) {
                            canadd = false;
                            break;
                        }
                    }

                    #endregion

                    if (canadd) { Allavailabletables.Add(thistable); }
                }
            }
        }

        _lastTableCheck = DateTime.Now;
        return Allavailabletables.Clone(); // Als Clone, damit bez�ge gebrochen werden und sich die Auflistung nicht mehr ver�ndern kann
    }

    public static bool CriticalState() {
        foreach (var thisDb in AllFiles) {
            if (!thisDb.IsDisposed) {
                if (!thisDb.LogUndo) { return true; } // Irgend ein heikler Prozess
                if (thisDb.IsInCache == null) { return true; } // Irgend eine Datenbank wird aktuell geladen
            }
        }

        return false;
    }

    public static string EditableErrorReason(DatabaseAbstract? database, EditableErrorReason mode) {
        if (database == null) { return "Keine Datenbank zum bearbeiten."; }
        return database.EditableErrorReason(mode);
    }

    public static void ForceSaveAll() {
        var x = AllFiles.Count;
        foreach (var thisFile in AllFiles) {
            _ = thisFile?.Save();
            if (x != AllFiles.Count) {
                // Die Auflistung wurde ver�ndert! Selten, aber kann passieren!
                ForceSaveAll();
                return;
            }
        }
    }

    public static DatabaseAbstract? GetById(ConnectionInfo? ci, NeedPassword? needPassword) {
        if (ci is null) { return null; }

        #region Schauen, ob die Datenbank bereits geladen ist

        foreach (var thisFile in AllFiles) {
            var d = thisFile.ConnectionData;

            if (string.Equals(d.UniqueID, ci.UniqueID, StringComparison.OrdinalIgnoreCase)) {
                return thisFile;
            }

            if (d.AdditionalData.ToLower().EndsWith(".mdb") || d.AdditionalData.ToLower().EndsWith(".bdb")) {
                if (d.AdditionalData.Equals(ci.AdditionalData, StringComparison.OrdinalIgnoreCase)) {
                    return thisFile; // Multiuser - nicht multiuser konflikt
                }
            }
        }

        #endregion

        #region Schauen, ob der Provider sie herstellen kann

        if (ci.Provider != null) {
            var db = ci.Provider.GetOtherTable(ci.TableName);
            if (db != null) { return db; }
        }

        #endregion

        DatabaseTypes ??= Generic.GetEnumerableOfType<DatabaseAbstract>();

        #region Schauen, ob sie �ber den Typ definiert werden kann

        foreach (var thist in DatabaseTypes) {
            if (thist.Name.Equals(ci.DatabaseID, StringComparison.OrdinalIgnoreCase)) {
                var l = new object?[2];
                l[0] = ci;
                l[1] = needPassword;
                var v = thist.GetMethod("CanProvide")?.Invoke(null, l);

                if (v is DatabaseAbstract db) { return db; }
            }
        }

        #endregion

        #region Wenn die Connection einem Dateinamen entspricht, versuchen den zu laden

        if (FileExists(ci.AdditionalData)) {
            if (ci.AdditionalData.FileSuffix().ToLower() is "mdb" or "bdb") {
                return new Database(ci.AdditionalData, false, false, needPassword);
            }
        }

        #endregion

        if (SqlBackAbstract.ConnectedSqlBack != null) {
            foreach (var thisSql in SqlBackAbstract.ConnectedSqlBack) {
                var h = thisSql.HandleMe(ci);
                if (h != null) {
                    return new DatabaseSqlLite(h, false, ci.TableName);
                }
            }
        }

        #region Zu guter Letzte, den Tablenamen nehmen...

        foreach (var thisFile in AllFiles) {
            var d = thisFile.ConnectionData;

            if (string.Equals(d.TableName, ci.TableName, StringComparison.OrdinalIgnoreCase)) {
                return thisFile;
            }
        }

        #endregion

        return null;
    }

    /// <summary>
    /// Sucht die Datenbank im Speicher. Wird sie nicht gefunden, wird sie geladen.
    /// </summary>
    public static DatabaseAbstract? LoadResource(Assembly assembly, string name, string blueBasicsSubDir, bool fehlerAusgeben, bool mustBeStream) {
        if (Develop.IsHostRunning() && !mustBeStream) {
            var x = -1;
            string? pf;
            do {
                x++;
                pf = string.Empty;
                switch (x) {
                    case 0:
                        // BeCreative, At Home, 31.11.2021
                        pf = Application.StartupPath + "\\..\\..\\..\\..\\BlueControls\\BlueControls\\Ressourcen\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 1:
                        pf = Application.StartupPath + "\\..\\..\\..\\..\\..\\..\\BlueControls\\Ressourcen\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 2:
                        pf = Application.StartupPath + "\\..\\..\\..\\..\\BlueControls\\Ressourcen\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 3:
                        pf = Application.StartupPath + "\\..\\..\\..\\BlueControls\\Ressourcen\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 4:
                        pf = Application.StartupPath + "\\" + name;
                        break;

                    case 5:
                        pf = Application.StartupPath + "\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 6:
                        pf = Application.StartupPath + "\\..\\..\\..\\..\\..\\Visual Studio Git\\BlueControls\\Ressourcen\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 7:
                        pf = Application.StartupPath + "\\..\\..\\..\\..\\Visual Studio Git\\BlueControls\\Ressourcen\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 8:
                        // warscheinlich BeCreative, Firma
                        pf = Application.StartupPath + "\\..\\..\\..\\..\\Visual Studio Git\\BlueElements\\BlueControls\\BlueControls\\Ressourcen\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 9:
                        // Bildzeichen-Liste, Firma, 25.10.2021
                        pf = Application.StartupPath + "\\..\\..\\..\\..\\..\\Visual Studio Git\\BlueElements\\BlueControls\\BlueControls\\Ressourcen\\" + blueBasicsSubDir + "\\" + name;
                        break;
                }
                if (FileExists(pf)) {
                    var ci = new ConnectionInfo(pf, Database.DatabaseId);

                    var tmp = GetById(ci, null);
                    if (tmp != null) { return tmp; }
                    tmp = new Database(pf, false, false, null);
                    return tmp;
                }
            } while (pf != string.Empty);
        }
        var d = Generic.GetEmmbedResource(assembly, name);
        if (d != null) { return new Database(d, name.ToUpper().TrimEnd(".MDB").TrimEnd(".BDB")); }
        if (fehlerAusgeben) { Develop.DebugPrint(FehlerArt.Fehler, "Ressource konnte nicht initialisiert werden: " + blueBasicsSubDir + " - " + name); }
        return null;
    }

    public static ConnectionInfo? ProviderOf(string tablename) {
        var alf = new List<DatabaseAbstract>();// k�nnte sich �ndern, deswegen Zwischenspeichern
        alf.AddRange(AllFiles);

        foreach (var thisDb in alf) {
            if (thisDb.ConnectionDataOfOtherTable(tablename, true) is ConnectionInfo ci) {
                return ci;
            }
        }

        return null;
    }

    public static string UniqueKeyValue() {
        var x = 9999;
        do {
            x += 1;
            if (x > 99999) { Develop.DebugPrint(FehlerArt.Fehler, "Unique ID konnte nicht erzeugt werden"); }

            var unique = ("X" + DateTime.UtcNow.ToString("mm.fff") + x.ToString(Constants.Format_Integer5)).RemoveChars(Constants.Char_DateiSonderZeichen + " _.");
            var ok = true;

            if (SqlBackAbstract.IsValidTableName(unique, false)) {
                foreach (var thisfile in AllFiles) {
                    if (string.Equals(unique, thisfile.TableName)) { ok = false; break; }
                }
            } else {
                ok = false;
            }

            if (ok) { return unique; }
        } while (true);
    }

    /// <summary>
    /// Der komplette Pfad mit abschlie�enden \
    /// </summary>
    /// <returns></returns>
    public virtual string AdditionalFilesPfadWhole() {
        if (AdditionalFilesPfadtmp != null) { return AdditionalFilesPfadtmp; }

        if (!string.IsNullOrEmpty(_additionalFilesPfad)) {
            var t = _additionalFilesPfad.CheckPath();
            if (DirectoryExists(t)) {
                AdditionalFilesPfadtmp = t;
                return t;
            }
        }

        AdditionalFilesPfadtmp = string.Empty;
        return string.Empty;
    }

    public abstract List<ConnectionInfo>? AllAvailableTables(List<DatabaseAbstract>? allreadychecked);

    /// <summary>
    /// Diese Methode setzt einen Wert dauerhaft und k�mmert sich um alles, was dahingehend zu tun ist (z.B. Undo).
    /// Der Wert wird intern fest verankert - bei ReadOnly werden aber weitere Schritte ignoriert.
    /// </summary>
    /// <param name="comand"></param>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="previousValue"></param>
    /// <param name="changedTo"></param>
    /// <param name="comment"></param>
    public string ChangeData(DatabaseDataType comand, ColumnItem? column, RowItem? row, string previousValue, string changedTo, string comment) {
        if (IsDisposed) { return "Datenbank verworfen!"; }

        var f = SetValueInternal(comand, changedTo, column, row, false);

        if (!string.IsNullOrEmpty(f)) { return f; }

        //if (isLoading) { return f; }

        //if (ReadOnly) {
        //    //if (comand == DatabaseDataType.ColumnContentWidth) { return string.Empty; }
        //    ////if (comand == DatabaseDataType.FirstColumn) { return string.Empty; }
        //    //if (!string.IsNullOrEmpty(TableName)) {
        //    //    Develop.DebugPrint(FehlerArt.Warnung, "Datei ist Readonly, " + comand + ", " + TableName);
        //    //}
        //    return "Schreibschutz aktiv";
        //}

        if (LogUndo) {
            AddUndo(TableName, comand, column, row, previousValue, changedTo, UserName, comment);
        }

        //if (comand != DatabaseDataType.AutoExport) { SetUserDidSomething(); } // Ansonsten wir der Export dauernd unterbrochen

        return string.Empty;
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
        if (l.Get(EventTypes.export).Count > 1) {
            return "Skript 'Export' mehrfach vorhanden";
        }

        if (l.Get(EventTypes.database_loaded).Count > 1) {
            return "Skript 'Datenank geladen' mehrfach vorhanden";
        }

        if (l.Get(EventTypes.prepare_formula).Count > 1) {
            return "Skript 'Formular Vorbereitung' mehrfach vorhanden";
        }

        if (l.Get(EventTypes.value_changed_extra_thread).Count > 1) {
            return "Skript 'Wert ge�ndert Extra Thread' mehrfach vorhanden";
        }

        if (l.Get(EventTypes.new_row).Count > 1) {
            return "Skript 'Neue Zeile' mehrfach vorhanden";
        }

        if (l.Get(EventTypes.value_changed).Count > 1) {
            return "Skript 'Wert ge�ndert' mehrfach vorhanden";
        }

        return string.Empty;
    }

    public void CloneFrom(DatabaseAbstract sourceDatabase, bool cellDataToo, bool tagsToo) {
        _ = sourceDatabase.Save();

        Column.CloneFrom(sourceDatabase, cellDataToo);

        if (cellDataToo) { Row.CloneFrom(sourceDatabase); }

        //FirstColumn = sourceDatabase.FirstColumn;
        AdditionalFilesPfad = sourceDatabase.AdditionalFilesPfad;
        CachePfad = sourceDatabase.CachePfad; // Nicht so wichtig ;-)
        VorhalteZeit = sourceDatabase.VorhalteZeit; // Nicht so wichtig ;-)
        Caption = sourceDatabase.Caption;
        //TimeCode = sourceDatabase.TimeCode;
        CreateDate = sourceDatabase.CreateDate;
        Creator = sourceDatabase.Creator;
        //Filename - nope
        //Tablename - nope
        //TimeCode - nope
        GlobalScale = sourceDatabase.GlobalScale;
        GlobalShowPass = sourceDatabase.GlobalShowPass;
        //RulesScript = sourceDatabase.RulesScript;
        if (SortDefinition == null || SortDefinition.ToString() != sourceDatabase.SortDefinition?.ToString()) {
            if (sourceDatabase.SortDefinition != null) {
                SortDefinition = new RowSortDefinition(this, sourceDatabase.SortDefinition.ToString());
            }
        }

        StandardFormulaFile = sourceDatabase.StandardFormulaFile;
        UndoCount = sourceDatabase.UndoCount;
        ZeilenQuickInfo = sourceDatabase.ZeilenQuickInfo;
        if (tagsToo) {
            Tags = new(sourceDatabase.Tags.Clone());
        }
        Layouts = sourceDatabase.Layouts;

        DatenbankAdmin = new(sourceDatabase.DatenbankAdmin.Clone());
        PermissionGroupsNewRow = new(sourceDatabase.PermissionGroupsNewRow.Clone());

        var tcvc = new List<ColumnViewCollection>();
        foreach (var t in sourceDatabase.ColumnArrangements) {
            tcvc.Add(new ColumnViewCollection(this, t.ToString()));
        }
        ColumnArrangements = new(tcvc);

        //Export = sourceDatabase.Export;

        EventScript = sourceDatabase.EventScript;

        if (tagsToo) {
            Variables = sourceDatabase.Variables;
        }

        //Events = sourceDatabase.Events;

        UndoCount = sourceDatabase.UndoCount;
    }

    public abstract ConnectionInfo? ConnectionDataOfOtherTable(string tableName, bool checkExists);

    /// <summary>
    /// AdditionalFiles/Datenbankpfad mit Backup und abschlie�enden \
    /// </summary>
    /// <returns></returns>
    public string DefaultBackupPath() {
        if (!string.IsNullOrEmpty(AdditionalFilesPfadWhole())) { return AdditionalFilesPfadWhole() + "Backup\\"; }
        //if (!string.IsNullOrEmpty(Filename)) { return Filename.FilePath() + "Forms\\"; }
        return string.Empty;
    }

    /// <summary>
    /// AdditionalFiles/Datenbankpfad mit Forms und abschlie�enden \
    /// </summary>
    /// <returns></returns>
    public string DefaultFormulaPath() {
        if (!string.IsNullOrEmpty(AdditionalFilesPfadWhole())) { return AdditionalFilesPfadWhole() + "Forms\\"; }
        //if (!string.IsNullOrEmpty(Filename)) { return Filename.FilePath() + "Forms\\"; }
        return string.Empty;
    }

    /// <summary>
    /// AdditionalFiles/Datenbankpfad mit Layouts und abschlie�enden \
    /// </summary>
    public string DefaultLayoutPath() {
        if (!string.IsNullOrEmpty(AdditionalFilesPfadWhole())) { return AdditionalFilesPfadWhole() + "Layouts\\"; }
        //if (!string.IsNullOrEmpty(Filename)) { return Filename.FilePath() + "Layouts\\"; }
        return string.Empty;
    }

    public void Dispose() {
        // �ndern Sie diesen Code nicht. F�gen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public virtual string EditableErrorReason(EditableErrorReason mode) {
        if (IsDisposed) { return "Datenbank verworfen."; }

        if (mode is BlueBasics.Enums.EditableErrorReason.OnlyRead or BlueBasics.Enums.EditableErrorReason.Load) { return string.Empty; }

        if (ReadOnly && mode.HasFlag(BlueBasics.Enums.EditableErrorReason.Save)) { return "Datenbank schreibgesch�tzt!"; }

        if (mode.HasFlag(BlueBasics.Enums.EditableErrorReason.EditGeneral) || mode.HasFlag(BlueBasics.Enums.EditableErrorReason.Save)) {
            if (Row.HasPendingWorker()) { return "Es m�ssen noch Daten �berpr�ft werden."; }
        }

        return IntParse(LoadedVersion.Replace(".", string.Empty)) > IntParse(DatabaseVersion.Replace(".", string.Empty))
            ? "Diese Programm kann nur Datenbanken bis Version " + DatabaseVersion + " speichern."
            : string.Empty;
    }

    public void EventScript_Add(EventScript ev, bool isLoading) {
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

    public ScriptEndedFeedback ExecuteScript(EventScript s, bool changevalues, RowItem? row) {
        if (IsDisposed) { return new ScriptEndedFeedback("Datenbank verworfen", false, s.Name); }

        var sce = CheckScriptError();
        if (!string.IsNullOrEmpty(sce)) { return new ScriptEndedFeedback("Die Skripte enthalten Fehler: " + sce, false, "Allgemein"); }

        try {

            #region Variablen f�r Skript erstellen

            VariableCollection vars = new();

            if (row != null && !row.IsDisposed) {
                foreach (var thisCol in Column) {
                    var v = RowItem.CellToVariable(thisCol, row);
                    if (v != null) { vars.AddRange(v); }
                }
                vars.Add(new VariableRowItem("RowKey", row, true, true, "Die aktuelle Zeile, die ausgef�hrt wird."));
            }

            foreach (var thisvar in Variables.ToListVariableString()) {
                var v = new VariableString("DB_" + thisvar.Name, thisvar.ValueString, false, false, "Datenbank-Kopf-Variable\r\n" + thisvar.Comment);
                vars.Add(v);
            }

            vars.Add(new VariableString("User", UserName, true, false, "ACHTUNG: Keinesfalls d�rfen benutzerabh�ngig Werte ver�ndert werden."));
            vars.Add(new VariableString("Usergroup", UserGroup, true, false, "ACHTUNG: Keinesfalls d�rfen gruppenabh�ngig Werte ver�ndert werden."));
            vars.Add(new VariableBool("Administrator", IsAdministrator(), true, false, "ACHTUNG: Keinesfalls d�rfen gruppenabh�ngig Werte ver�ndert werden.\r\nDiese Variable gibt zur�ck, ob der Benutzer Admin f�r diese Datenbank ist."));
            vars.Add(new VariableDatabase("Database", this, true, true, "Die Datenbank, die zu dem Skript geh�rt"));
            vars.Add(new VariableString("Tablename", TableName, true, false, "Der aktuelle Tabellenname."));
            vars.Add(new VariableFloat("Rows", Row.Count, true, false, "Die Anzahl der Zeilen in der Datenbank")); // RowCount als Befehl belegt
            vars.Add(new VariableString("NameOfFirstColumn", Column.First()?.Name ?? string.Empty, true, false, "Der Name der ersten Spalte"));
            vars.Add(new VariableBool("SetErrorEnabled", s.EventTypes.HasFlag(EventTypes.prepare_formula), true, true, "Marker, ob der Befehl 'SetError' benutzt werden kann."));
            if (!string.IsNullOrEmpty(AdditionalFilesPfadWhole())) {
                vars.Add(new VariableString("AdditionalFilesPfad", AdditionalFilesPfadWhole(), true, false, "Der Dateipfad der Datenbank, in dem zus�zliche Daten gespeichert werden."));
            }

            #endregion

            #region  Erlaubte Methoden ermitteln

            var allowedMethods = MethodType.Standard | MethodType.Database;

            if (row != null && !row.IsDisposed) { allowedMethods |= MethodType.MyDatabaseRow; }
            if (!s.EventTypes.HasFlag(EventTypes.prepare_formula)) {
                allowedMethods |= MethodType.IO;
                allowedMethods |= MethodType.NeedLongTime;
            }

            if (!s.EventTypes.HasFlag(EventTypes.value_changed_extra_thread) &&
                !s.EventTypes.HasFlag(EventTypes.prepare_formula) &&
                !s.EventTypes.HasFlag(EventTypes.database_loaded)) {
                allowedMethods |= MethodType.ManipulatesUser;
            }

            if (changevalues) { allowedMethods |= MethodType.ChangeAnyDatabaseOrRow; }

            #endregion

            #region Script ausf�hren

            Script sc = new(vars, AdditionalFilesPfadWhole(), changevalues, allowedMethods, s.Attributes()) {
                ScriptText = s.Script
            };
            var scf = sc.Parse(0, s.Name);

            #endregion

            #region Variablen zur�ckschreiben und Special Rules ausf�hren

            if (sc.ChangeValues && changevalues && scf.AllOk) {
                if (row != null && !row.IsDisposed) {
                    foreach (var thisCol in Column) {
                        row.VariableToCell(thisCol, vars);
                    }
                }

                WriteBackDbVariables(vars);
            }

            if (!scf.AllOk) {
                OnDropMessage(FehlerArt.Info, "Das Skript '" + s.Name + "' hat einen Fehler verursacht\r\n" + scf.Protocol[0]);
            }

            #endregion

            return scf;
        } catch {
            Develop.CheckStackForOverflow();
            return ExecuteScript(s, changevalues, row);
        }
    }

    public ScriptEndedFeedback ExecuteScript(EventTypes? eventname, string? scriptname, bool changevalues, RowItem? row) {
        try {
            if (IsDisposed) { return new ScriptEndedFeedback("Datenbank verworfen", false, "Allgemein"); }

            var m = EditableErrorReason(BlueBasics.Enums.EditableErrorReason.EditGeneral);

            if (!string.IsNullOrEmpty(m)) { return new ScriptEndedFeedback("Automatische Prozesse nicht m�glich: " + m, false, "Allgemein"); }

            #region Script ermitteln

            if (eventname != null && !string.IsNullOrEmpty(scriptname)) {
                Develop.DebugPrint(FehlerArt.Fehler, "Event und Skript angekommen!");
                return new ScriptEndedFeedback("Event und Skript angekommen!", false, "Allgemein");
            }

            if (eventname == null && string.IsNullOrEmpty(scriptname)) { return new ScriptEndedFeedback("Kein Eventname oder Skript angekommen", false, "Allgemein"); }

            if (string.IsNullOrEmpty(scriptname) && eventname != null) {
                var l = EventScript.Get((EventTypes)eventname);
                if (l.Count == 1) { scriptname = l[0].Name; }
                if (string.IsNullOrEmpty(scriptname)) { return new ScriptEndedFeedback(string.Empty, false, string.Empty); }
            }

            if (scriptname == null || string.IsNullOrWhiteSpace(scriptname)) { return new ScriptEndedFeedback("Kein Skriptname angekommen", false, "Allgemein"); }

            var script = EventScript.Get(scriptname);

            if (script == null) { return new ScriptEndedFeedback("Skript nicht gefunden.", false, scriptname); }

            if (script.NeedRow && row == null) { return new ScriptEndedFeedback("Zeilenskript aber keine Zeile angekommen.", false, scriptname); }

            if (!script.NeedRow) { row = null; }

            #endregion

            if (!script.ChangeValues) { changevalues = false; }

            return ExecuteScript(script, changevalues, row);
        } catch {
            Develop.CheckStackForOverflow();
            return ExecuteScript(eventname, scriptname, changevalues, row);
        }
    }

    public string Export_CSV(FirstRow firstRow, ColumnItem column, List<RowData>? sortedRows) =>
                //Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
                Export_CSV(firstRow, new List<ColumnItem> { column }, sortedRows);

    public string Export_CSV(FirstRow firstRow, List<ColumnItem>? columnList, List<RowData>? sortedRows) {
        columnList ??= Column.Where(thisColumnItem => thisColumnItem != null).ToList();
        sortedRows ??= Row.AllRows();

        StringBuilder sb = new();
        switch (firstRow) {
            case FirstRow.Without:
                break;

            case FirstRow.ColumnCaption:
                for (var colNr = 0; colNr < columnList.Count; colNr++) {
                    if (columnList[colNr] != null) {
                        var tmp = columnList[colNr].ReadableText();
                        tmp = tmp.Replace(";", "|");
                        tmp = tmp.Replace(" |", "|");
                        tmp = tmp.Replace("| ", "|");
                        _ = sb.Append(tmp);
                        if (colNr < columnList.Count - 1) { _ = sb.Append(";"); }
                    }
                }
                _ = sb.Append("\r\n");
                break;

            case FirstRow.ColumnInternalName:
                for (var colNr = 0; colNr < columnList.Count; colNr++) {
                    if (columnList[colNr] != null) {
                        _ = sb.Append(columnList[colNr].Name);
                        if (colNr < columnList.Count - 1) { _ = sb.Append(';'); }
                    }
                }
                _ = sb.Append("\r\n");
                break;

            default:
                Develop.DebugPrint(firstRow);
                break;
        }

        var (_, errormessage) = RefreshRowData(sortedRows, false);
        if (!string.IsNullOrEmpty(errormessage)) {
            OnDropMessage(FehlerArt.Fehler, errormessage);
        }

        foreach (var thisRow in sortedRows) {
            if (thisRow != null && !thisRow.IsDisposed) {
                for (var colNr = 0; colNr < columnList.Count; colNr++) {
                    if (columnList[colNr] != null) {
                        var tmp = Cell.GetString(columnList[colNr], thisRow?.Row);
                        tmp = tmp.Replace("\r\n", "|");
                        tmp = tmp.Replace("\r", "|");
                        tmp = tmp.Replace("\n", "|");
                        tmp = tmp.Replace(";", "<sk>");
                        _ = sb.Append(tmp);
                        if (colNr < columnList.Count - 1) { _ = sb.Append(';'); }
                    }
                }
                _ = sb.Append("\r\n");
            }
        }
        return sb.ToString().TrimEnd("\r\n");
    }

    public string Export_CSV(FirstRow firstRow, ColumnViewCollection? arrangement, List<RowData>? sortedRows) => Export_CSV(firstRow, arrangement?.ListOfUsedColumn(), sortedRows);

    public string Export_CSV(FirstRow firstRow, int arrangementNo, FilterCollection? filter, List<RowItem>? pinned) => Export_CSV(firstRow, _columnArrangements[arrangementNo].ListOfUsedColumn(), Row.CalculateSortedRows(filter, SortDefinition, pinned, null));

    public void Export_HTML(string filename, int arrangementNo, FilterCollection? filter, List<RowItem>? pinned) => Export_HTML(filename, _columnArrangements[arrangementNo].ListOfUsedColumn(), Row.CalculateSortedRows(filter, SortDefinition, pinned, null), false);

    public void Export_HTML(string filename, List<ColumnItem>? columnList, List<RowData>? sortedRows, bool execute) {
        if (columnList == null || columnList.Count == 0) {
            columnList = Column.Where(thisColumnItem => thisColumnItem != null).ToList();
        }

        sortedRows ??= Row.AllRows();

        if (string.IsNullOrEmpty(filename)) {
            filename = TempFile(string.Empty, "Export", "html");
        }

        Html da = new(TableName.FileNameWithoutSuffix());
        da.AddCaption(_caption);
        da.TableBeginn();
        da.RowBeginn();
        foreach (var thisColumn in columnList) {
            if (thisColumn != null) {
                da.CellAdd(thisColumn.ReadableText().Replace(";", "<br>"), thisColumn.BackColor);
            }
        }

        da.RowEnd();
        var (_, errormessage) = RefreshRowData(sortedRows, false);
        if (!string.IsNullOrEmpty(errormessage)) {
            OnDropMessage(FehlerArt.Fehler, errormessage);
        }

        foreach (var thisRow in sortedRows) {
            if (thisRow != null && !thisRow.IsDisposed) {
                da.RowBeginn();
                foreach (var thisColumn in columnList) {
                    if (thisColumn != null) {
                        var lcColumn = thisColumn;
                        var lCrow = thisRow?.Row;
                        if (thisColumn.Format is DataFormat.Verkn�pfung_zu_anderer_Datenbank) {
                            (lcColumn, lCrow, _) = CellCollection.LinkedCellData(thisColumn, thisRow?.Row, false, false);
                        }

                        if (lCrow != null && lcColumn != null) {
                            da.CellAdd(lCrow.CellGetValuesReadable(lcColumn, ShortenStyle.HTML).JoinWith("<br>"), thisColumn.BackColor);
                        } else {
                            da.CellAdd(" ", thisColumn.BackColor);
                        }
                    }
                }

                da.RowEnd();
            }
        }

        // Summe----
        da.RowBeginn();
        foreach (var thisColumn in columnList) {
            if (thisColumn != null) {
                var s = thisColumn.Summe(sortedRows);
                if (s == null) {
                    da.CellAdd("-", thisColumn.BackColor);
                    //da.GenerateAndAdd("        <th BORDERCOLOR=\"#aaaaaa\" align=\"left\" valign=\"middle\" bgcolor=\"#" + ThisColumn.BackColor.ToHTMLCode() + "\">-</th>");
                } else {
                    da.CellAdd("~sum~ " + s, thisColumn.BackColor);
                    //da.GenerateAndAdd("        <th BORDERCOLOR=\"#aaaaaa\" align=\"left\" valign=\"middle\" bgcolor=\"#" + ThisColumn.BackColor.ToHTMLCode() + "\">&sum; " + s + "</th>");
                }
            }
        }
        da.RowEnd();
        // ----------------------
        da.TableEnd();
        da.AddFoot();
        da.Save(filename, execute);
    }

    public void Export_HTML(string filename, ColumnViewCollection? arrangement, List<RowData> sortedRows, bool execute) => Export_HTML(filename, arrangement?.ListOfUsedColumn(), sortedRows, execute);

    public string? FormulaFileName() {
        if (FileExists(_standardFormulaFile)) { return _standardFormulaFile; }
        if (FileExists(AdditionalFilesPfadWhole() + _standardFormulaFile)) { return AdditionalFilesPfadWhole() + _standardFormulaFile; }
        if (FileExists(DefaultFormulaPath() + _standardFormulaFile)) { return DefaultFormulaPath() + _standardFormulaFile; }
        return null;
    }

    public DatabaseAbstract? GetOtherTable(string tablename) {
        if (!SqlBackAbstract.IsValidTableName(tablename, false)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Ung�ltiger Tabellenname: " + tablename);
            return null;
        }

        var x = ConnectionDataOfOtherTable(tablename, true);
        if (x == null) { return null; }

        x.Provider = null;  // KEINE Vorage mitgeben, weil sonst eine Endlosschleife aufgerufen wird!

        return GetById(x, null);// new DatabaseSQL(_sql, readOnly, tablename);
    }

    public string Import(string importText, bool spalteZuordnen, bool zeileZuordnen, string splitChar, bool eliminateMultipleSplitter, bool eleminateSplitterAtStart, bool dorowautmatic) {
        // Vorbereitung des Textes -----------------------------
        importText = importText.Replace("\r\n", "\r").Trim("\r");

        #region die Zeilen (zeil) vorbereiten

        var ein = importText.SplitAndCutByCr();
        List<string[]> zeil = new();
        var neuZ = 0;
        for (var z = 0; z <= ein.GetUpperBound(0); z++) {
            //#region Das Skript berechnen

            //if (!string.IsNullOrEmpty(scriptnameeverychangedrow)) {
            //    var vars = new VariableCollection {
            //        new VariableString("Row", ein[z], false, false, "Der Original-Text. Dieser kann (und soll) manipuliert werden."),
            //        new VariableBool("IsCaption", spalteZuordnen && z == 0, true, false, "Wenn TRUE, ist das die erste Zeile, die �berschriften enth�lt."),
            //        new VariableString("Seperator", splitChar, true, false, "Das Trennzeichen")
            //    };
            //    var x = new BlueScript.Script(vars, string.Empty, false) {
            //        ScriptText = scriptnameeverychangedrow
            //    };
            //    if (!x.Parse()) {
            //        OnDropMessage(FehlerArt.Warnung, "Skript-Fehler, Import kann nicht ausgef�hrt werden.");
            //        return x.Error;
            //    }
            //    ein[z] = vars.GetString("Row");
            //}

            //#endregion

            if (eliminateMultipleSplitter) {
                ein[z] = ein[z].Replace(splitChar + splitChar, splitChar);
            }
            if (eleminateSplitterAtStart) {
                ein[z] = ein[z].TrimStart(splitChar);
            }
            ein[z] = ein[z].TrimEnd(splitChar);
            zeil.Add(ein[z].SplitAndCutBy(splitChar));
        }

        if (zeil.Count == 0) {
            OnDropMessage(FehlerArt.Warnung, "Import kann nicht ausgef�hrt werden.");
            return "Import kann nicht ausgef�hrt werden.";
        }

        #endregion

        #region Spaltenreihenfolge (columns) ermitteln

        List<ColumnItem?> columns = new();
        var startZ = 0;

        if (spalteZuordnen) {
            startZ = 1;
            for (var spaltNo = 0; spaltNo < zeil[0].GetUpperBound(0) + 1; spaltNo++) {
                if (string.IsNullOrEmpty(zeil[0][spaltNo])) {
                    OnDropMessage(FehlerArt.Warnung, "Abbruch,<br>leerer Spaltenname.");
                    return "Abbruch,<br>leerer Spaltenname.";
                }
                zeil[0][spaltNo] = SqlBackAbstract.MakeValidColumnName(zeil[0][spaltNo]);

                var col = Column.Exists(zeil[0][spaltNo]);
                if (col == null) {
                    if (!ColumnItem.IsValidColumnName(zeil[0][spaltNo])) {
                        OnDropMessage(FehlerArt.Warnung, "Abbruch,<br>ung�ltiger Spaltenname.");
                        return "Abbruch,<br>ung�ltiger Spaltenname.";
                    }

                    col = Column.GenerateAndAdd(zeil[0][spaltNo]);
                    if (col != null) {
                        col.Caption = zeil[0][spaltNo];
                        col.Format = DataFormat.Text;
                    }
                }
                columns.Add(col);
            }
        } else {
            columns.AddRange(Column.Where(thisColumn => thisColumn != null && !thisColumn.IsSystemColumn()));
            while (columns.Count < zeil[0].GetUpperBound(0) + 1) {
                var newc = Column.GenerateAndAdd();
                if (newc != null) {
                    newc.Caption = newc.Name;
                    newc.Format = DataFormat.Text;
                    newc.MultiLine = true;
                }

                columns.Add(newc);
            }
        }

        #endregion

        // -------------------------------------
        // --- Importieren ---
        // -------------------------------------

        for (var zeilNo = startZ; zeilNo < zeil.Count; zeilNo++) {
            var tempVar2 = Math.Min(zeil[zeilNo].GetUpperBound(0) + 1, columns.Count);
            RowItem? row = null;
            for (var spaltNo = 0; spaltNo < tempVar2; spaltNo++) {
                if (spaltNo == 0) {
                    row = null;
                    if (zeileZuordnen && !string.IsNullOrEmpty(zeil[zeilNo][spaltNo])) { row = Row[zeil[zeilNo][spaltNo]]; }
                    if (row == null && !string.IsNullOrEmpty(zeil[zeilNo][spaltNo])) {
                        row = Row.GenerateAndAdd(zeil[zeilNo][spaltNo], "Import, fehlende Zeile");
                        neuZ++;
                    }
                } else {
                    row?.CellSet(columns[spaltNo], zeil[zeilNo][spaltNo].SplitAndCutBy("|").JoinWithCr());
                }
                if (row != null && dorowautmatic) { _ = row.ExecuteScript(null, string.Empty, true, true, true, 0); }
            }
        }

        OnDropMessage(FehlerArt.Info, "<b>Import abgeschlossen.</b>\r\n" + neuZ + " neue Zeilen erstellt.");
        return string.Empty;
    }

    public string ImportCsv(string filename) {
        if (!FileExists(filename)) { return "Datei nicht gefunden"; }
        var importText = File.ReadAllText(filename, Constants.Win1252);

        if (string.IsNullOrEmpty(importText)) { return "Dateiinhalt leer"; }

        var x = importText.SplitAndCutByCrToList();

        if (x.Count < 2) { return "Keine Zeilen zum importieren."; }

        var sep = ",";

        if (x[0].StartsWith("sep=", StringComparison.OrdinalIgnoreCase)) {
            if (x.Count < 3) { return "Keine Zeilen zum importieren."; }
            sep = x[0].Substring(4);
            x.RemoveAt(0);
            importText = x.JoinWithCr();
        }

        return Import(importText, true, true, sep, false, false, true);
    }

    public bool IsAdministrator() {
        if (string.Equals(UserGroup, Constants.Administrator, StringComparison.OrdinalIgnoreCase)) { return true; }
        if (_datenbankAdmin == null || _datenbankAdmin.Count == 0) { return false; }
        if (_datenbankAdmin.Contains(Constants.Everybody, false)) { return true; }
        if (!string.IsNullOrEmpty(UserName) && _datenbankAdmin.Contains("#User: " + UserName, false)) { return true; }
        return !string.IsNullOrEmpty(UserGroup) && _datenbankAdmin.Contains(UserGroup, false);
    }

    //    Export = new(ex);
    //}
    public bool IsFileAllowedToLoad(string fileName) {
        foreach (var thisFile in AllFiles) {
            if (thisFile is Database db) {
                if (string.Equals(db.Filename, fileName, StringComparison.OrdinalIgnoreCase)) {
                    _ = thisFile.Save();
                    Develop.DebugPrint(FehlerArt.Warnung, "Doppletes Laden von " + fileName);
                    return false;
                }
            }
        }

        return true;
    }

    //    foreach (var thisExport in ex) {
    //        if (thisExport != null) {
    //            if (thisExport.Typ == ExportTyp.EinzelnMitFormular) {
    //                if (thisExport.ExportFormularId == layoutId) {
    //                    thisExport.LastExportTimeUtc = new DateTime(1900, 1, 1);
    //                }
    //            }
    //        }
    //    }
    public void OnScriptError(RowCancelEventArgs e) {
        if (IsDisposed) { return; }
        ScriptError?.Invoke(this, e);
    }

    //    var ex = Export.CloneWithClones();
    //public abstract void Load_Reload();
    public void OnViewChanged() {
        if (IsDisposed) { return; }
        ViewChanged?.Invoke(this, System.EventArgs.Empty);
    }

    public void Optimize() {
        foreach (var thisColumn in Column) {
            thisColumn.Optimize();
        }
    }

    //public void InvalidateExports(string layoutId) {
    //    if (ReadOnly) { return; }
    public List<string> Permission_AllUsed(bool cellLevel) {
        List<string> e = new();
        foreach (var thisColumnItem in Column) {
            if (thisColumnItem != null) {
                e.AddRange(thisColumnItem.PermissionGroupsChangeCell);
            }
        }
        e.AddRange(_permissionGroupsNewRow);
        e.AddRange(_datenbankAdmin);
        foreach (var thisArrangement in _columnArrangements) {
            e.AddRange(thisArrangement.PermissionGroups_Show);
        }
        //foreach (var thisArrangement in OldFormulaViews) {
        //    e.AddRange(thisArrangement.PermissionGroups_Show);
        //}
        e.Add(Constants.Everybody);
        e.Add("#User: " + UserName);
        if (cellLevel) {
            e.Add("#RowCreator");
        } else {
            e.RemoveString("#RowCreator", false);
        }
        e.RemoveString(Constants.Administrator, false);
        if (!IsAdministrator()) { e.Add(UserGroup); }
        return e.SortedDistinctList();
    }

    public bool PermissionCheck(IList<string>? allowed, RowItem? row) {
        try {
            if (IsAdministrator()) { return true; }
            if (PowerEdit.Subtract(DateTime.Now).TotalSeconds > 0) { return true; }
            if (allowed == null || allowed.Count == 0) { return false; }
            if (allowed.Any(thisString => PermissionCheckWithoutAdmin(thisString, row))) {
                return true;
            }
        } catch (Exception ex) {
            Develop.DebugPrint(FehlerArt.Warnung, "Fehler beim Rechte-Check", ex);
        }
        return false;
    }

    public void RefreshColumnsData(ColumnItem? column) {
        if (column == null || column.IsInCache != null) { return; }
        RefreshColumnsData(new List<ColumnItem> { column });
    }

    public abstract void RefreshColumnsData(List<ColumnItem> columns);

    public void RefreshColumnsData(IEnumerable<FilterItem>? filter) {
        if (filter != null) {
            var c = new List<ColumnItem>();

            foreach (var thisF in filter) {
                if (thisF.Column != null && thisF.Column.IsInCache == null) {
                    _ = c.AddIfNotExists(thisF.Column);
                }
            }
            RefreshColumnsData(c);
        }
    }

    public (bool didreload, string errormessage) RefreshRowData(List<RowData> rowdata, bool refreshAlways) {
        if (rowdata.Count == 0) { return (false, string.Empty); }

        var r = new List<RowItem>();
        foreach (var thisRow in rowdata) {
            if (thisRow.Row != null) { r.Add(thisRow.Row); }
        }
        return RefreshRowData(r, refreshAlways, null);
    }

    public abstract (bool didreload, string errormessage) RefreshRowData(List<RowItem> row, bool refreshAlways, List<RowItem>? sortedRows);

    public (bool didreload, string errormessage) RefreshRowData(List<long> keys, bool refreshAlways, List<RowItem>? sortedRows) {
        if (keys.Count == 0) { return (false, string.Empty); }

        var r = new List<RowItem>();
        foreach (var thisK in keys) {
            var ro = Row.SearchByKey(thisK);
            if (ro != null) { r.Add(ro); }
        }
        return RefreshRowData(r, refreshAlways, sortedRows);
    }

    public (bool didreload, string errormessage) RefreshRowData(RowItem row, bool refreshAlways, List<RowItem>? sortedRows) => RefreshRowData(new List<RowItem> { row }, refreshAlways, sortedRows);

    public virtual void RepairAfterParse() {
        Column.Repair();
        RepairColumnArrangements();
        RepairViews();
        _layouts.Check();

        IsInCache = DateTime.UtcNow;
    }

    public abstract bool Save();

    public void SetReadOnly() => ReadOnly = true;

    public override string ToString() => base.ToString() + " " + TableName;

    public abstract string UndoText(ColumnItem? column, RowItem? row);

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

    public void WriteBackDbVariables(VariableCollection vars) {
        var vaa = new List<VariableString>();
        vaa.AddRange(Variables.ToListVariableString());

        foreach (var thisvar in vaa) {
            var v = vars.Get("DB_" + thisvar.Name);

            if (v is VariableString vs) {
                thisvar.ReadOnly = false; // weil kein OnChanged vorhanden ist
                thisvar.ValueString = vs.ValueString;
                thisvar.ReadOnly = true; // weil kein OnChanged vorhanden ist
            }
        }
        Variables = new VariableCollection(vaa);
    }

    internal void DevelopWarnung(string t) {
        try {
            t += "\r\nColumn-Count: " + Column.Count;
            t += "\r\nRow-Count: " + Row.Count;
            t += "\r\nTable: " + ConnectionData.TableName;
            t += "\r\nID: " + ConnectionData.DatabaseID;
        } catch { }
        Develop.DebugPrint(FehlerArt.Warnung, t);
    }

    internal void OnDropMessage(FehlerArt type, string message) {
        if (IsDisposed) { return; }
        if (!DropMessages) { return; }
        DropMessage?.Invoke(this, new MessageEventArgs(type, message));
    }

    internal void OnProgressbarInfo(ProgressbarEventArgs e) {
        if (IsDisposed) { return; }
        ProgressbarInfo?.Invoke(this, e);
    }

    //internal void OnGenerateLayoutInternal(GenerateLayoutInternalEventArgs e) {
    //    if (IsDisposed) { return; }
    //    GenerateLayoutInternal?.Invoke(this, e);
    //}
    internal void RepairColumnArrangements() {
        //if (ReadOnly) { return; }  // Gibt fehler bei Datenbanken, die nur Tempor�r erzeugt werden!

        var x = _columnArrangements!.CloneWithClones();

        for (var z = 0; z < Math.Max(2, x.Count); z++) {
            if (x.Count < z + 1) { x.Add(new ColumnViewCollection(this, string.Empty)); }
            ColumnViewCollection.Repair(x[z], z);
        }

        ColumnArrangements = x.AsReadOnly();
    }

    internal void RepairViews() {
        var lay = (LayoutCollection)Layouts.Clone();
        lay.Check();
        Layouts = lay;
    }

    /// <summary>
    /// Diese Routine setzt Werte auf den richtigen Speicherplatz und f�hrt Comands aus.
    /// Echtzeitbasierte Syteme sollten diese Routine verwenden, um den Wert ebenfalls fest zu verankern (sofern !IsLoading && !Readonly)
    /// Nur von Laderoutinen aufzurufen, oder von ChangeData, wenn der Wert bereits fest in der Datenbank verankert ist.
    /// Vorsicht beim �berschreiben:
    /// Da in Columns und Cells abgesprungen wird, und diese nicht �berschrieben werden k�nnen,
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="isLoading"></param>
    /// <returns>Leer, wenn da Wert setzen erfolgreich war. Andernfalls der Fehlertext.</returns>
    internal virtual string SetValueInternal(DatabaseDataType type, string value, ColumnItem? column, RowItem? row, bool isLoading) {
        if (IsDisposed) { return "Datenbank verworfen!"; }
        if (type.IsObsolete()) { return string.Empty; }

        if (type.IsCellValue()) {
            if (column == null || row == null) {
                Develop.DebugPrint(FehlerArt.Warnung, "Spalte/Zeile ist null! " + type);
                return "Wert nicht gesetzt!";
            }

            return Cell.SetValueInternal(column, row, value, isLoading);
        }

        if (type.IsColumnTag()) {
            if (column == null || column.IsDisposed || Column.IsDisposed) {
                Develop.DebugPrint(FehlerArt.Warnung, "Spalte ist null! " + type);
                return "Wert nicht gesetzt!";
            }
            return column.SetValueInternal(type, value, isLoading);
        }

        if (type.IsCommand()) {
            switch (type) {
                case DatabaseDataType.Comand_RemoveColumn:
                    var c = Column.Exists(value);
                    if (c == null) { return string.Empty; }
                    return Column.SetValueInternal(type, isLoading, c.Name);

                case DatabaseDataType.Comand_AddColumnByName:
                    var f2 = Column.SetValueInternal(type, isLoading, value);

                    if (!isLoading) {
                        var thisColumn = Column.Exists(value);
                        if (thisColumn != null) { thisColumn.IsInCache = DateTime.UtcNow; }
                    }

                    return f2;

                case DatabaseDataType.Comand_RemoveRow:
                    return Row.SetValueInternal(type, row?.Key, row, isLoading);

                case DatabaseDataType.Comand_AddRow:
                    var rk = LongParse(value);
                    var f1 = Row.SetValueInternal(type, rk, null, isLoading);

                    if (!isLoading) {
                        var thisRow = Row.SearchByKey(rk);
                        if (thisRow != null && !thisRow.IsDisposed) { thisRow.IsInCache = DateTime.UtcNow; }
                    }

                    return f1;

                default:
                    if (LoadedVersion == DatabaseVersion) {
                        SetReadOnly();
                        if (!ReadOnly) {
                            Develop.DebugPrint(FehlerArt.Fehler, "Laden von Datentyp \'" + type + "\' nicht definiert.<br>Wert: " + value + "<br>Tabelle: " + ConnectionData.ToString());
                        }
                    }
                    return "Befehl unbekannt.";
            }
        }

        switch (type) {
            case DatabaseDataType.Formatkennung:
                break;

            case DatabaseDataType.Version:
                LoadedVersion = value.Trim();
                if (LoadedVersion != DatabaseVersion) {
                    Initialize();
                    LoadedVersion = value.Trim();
                } else {
                    //Cell.RemoveOrphans();
                    Row.RemoveNullOrEmpty();
                    Cell.Clear();
                }
                break;

            case DatabaseDataType.Werbung:
                break;

            //case DatabaseDataType.CryptionState:
            //    break;

            //case DatabaseDataType.CryptionTest:
            //    break;

            case DatabaseDataType.Creator:
                _creator = value;
                break;

            case DatabaseDataType.CreateDateUTC:
                _createDate = value;
                break;

            case DatabaseDataType.DatabaseAdminGroups:
                _datenbankAdmin.SplitAndCutByCr_QuickSortAndRemoveDouble(value);
                break;

            case DatabaseDataType.SortDefinition:
                _sortDefinition = new RowSortDefinition(this, value);
                break;

            case DatabaseDataType.Caption:
                _caption = value;
                break;

            //case DatabaseDataType.TimeCode:
            //    _timeCode = value;
            //    break;

            case DatabaseDataType.GlobalScale:
                _globalScale = DoubleParse(value);
                break;

            case DatabaseDataType.AdditionalFilesPath:
                _additionalFilesPfad = value;
                break;

            //case DatabaseDataType.FirstColumn:
            //    _firstColumn = value;
            //    break;

            case DatabaseDataType.StandardFormulaFile:
                _standardFormulaFile = value;
                break;

            case DatabaseDataType.RowQuickInfo:
                _zeilenQuickInfo = value;
                break;

            case DatabaseDataType.Tags:
                _tags.SplitAndCutByCr(value);
                break;

            //case enDatabaseDataType.BinaryDataInOne:
            //    break;

            case DatabaseDataType.Layouts:
                _layouts.SplitAndCutByCr_QuickSortAndRemoveDouble(value);
                break;

            //case DatabaseDataType.AutoExport:
            //    _export.Clear();
            //    List<string> ae = new(value.SplitAndCutByCr());
            //    foreach (var t in ae) {
            //        _export.Add(new ExportDefinition(this, t));
            //    }
            //    break;

            case DatabaseDataType.EventScript:
                _eventScriptTmp = value;
                EventScript_RemoveAll(true);
                List<string> ai = new(value.SplitAndCutByCr());
                foreach (var t in ai) {
                    EventScript_Add(new EventScript(this, t), true);
                }

                CheckScriptError();
                break;

            case DatabaseDataType.DatabaseVariables:
                _variableTmp = value;
                Variables_RemoveAll(true);
                List<string> va = new(value.SplitAndCutByCr());
                foreach (var t in va) {
                    var l = new VariableString("dummy");
                    l.Parse(t);
                    l.ReadOnly = true; // Weil kein onChangedEreigniss vorhanden ist
                    Variables_Add(l, true);
                }
                break;

            case DatabaseDataType.ColumnArrangement:
                _columnArrangements.Clear();
                List<string> ca = new(value.SplitAndCutByCr());
                foreach (var t in ca) {
                    _columnArrangements.Add(new ColumnViewCollection(this, t));
                }
                break;

            case DatabaseDataType.PermissionGroupsNewRow:
                _permissionGroupsNewRow.SplitAndCutByCr_QuickSortAndRemoveDouble(value);
                break;

            case DatabaseDataType.GlobalShowPass:
                _globalShowPass = value;
                break;

            //case (DatabaseDataType)67: //.RulesScript:
            //    //ConvertRules(value);
            //    //_rulesScript = value;
            //    break;

            case DatabaseDataType.UndoCount:
                _undoCount = IntParse(value);
                break;

            case DatabaseDataType.UndoInOne:
                // Muss eine �bergeordnete Routine bei Befarf abfangen
                break;

            case DatabaseDataType.EOF:
                return string.Empty;

            default:
                // Variable type
                if (LoadedVersion == DatabaseVersion) {
                    SetReadOnly();
                    if (!ReadOnly) {
                        Develop.DebugPrint(FehlerArt.Fehler, "Laden von Datentyp \'" + type + "\' nicht definiert.<br>Wert: " + value + "<br>Tabelle: " + ConnectionData.ToString());
                    }
                }

                return "Datentyp unbekannt.";
        }
        return string.Empty;
    }

    protected abstract void AddUndo(string tableName, DatabaseDataType type, ColumnItem? column, RowItem? row, string previousValue, string changedTo, string userName, string comment);

    protected void CreateWatcher() {
        if (string.IsNullOrEmpty(EditableErrorReason(BlueBasics.Enums.EditableErrorReason.Save))) {
            _checker = new Timer(Checker_Tick);
            _ = _checker.Change(2000, 2000);
        }
    }

    protected virtual void Dispose(bool disposing) {
        if (IsDisposed) { return; }
        IsDisposed = true;

        OnDisposing();
        _ = AllFiles.Remove(this);

        //base.Dispose(disposing); // speichert und l�scht die ganzen Worker. setzt auch disposedValue und ReadOnly auf true
        if (disposing) {
            // TODO: verwalteten Zustand (verwaltete Objekte) entsorgen.
        }

        _checker?.Dispose();

        // TODO: nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer weiter unten �berschreiben.
        // TODO: gro�e Felder auf Null setzen.

        Column.Dispose();
        //Cell?.Dispose();
        Row.Dispose();

        _layouts.Clear();
    }

    protected virtual void Initialize() {
        Cell.Initialize();

        _columnArrangements.Clear();
        _layouts.Clear();
        _permissionGroupsNewRow.Clear();
        _tags.Clear();
        _datenbankAdmin.Clear();
        _globalShowPass = string.Empty;
        _creator = UserName;
        _createDate = DateTime.Now.ToString(Constants.Format_Date5);
        _undoCount = 300;
        _caption = string.Empty;
        LoadedVersion = DatabaseVersion;
        _globalScale = 1f;
        _additionalFilesPfad = "AdditionalFiles";
        _zeilenQuickInfo = string.Empty;
        _sortDefinition = null;
        EventScript_RemoveAll(true);
        Variables_RemoveAll(true);
    }

    protected void OnLoaded() {
        if (IsDisposed) { return; }
        IsInCache = DateTime.UtcNow;
        Loaded?.Invoke(this, System.EventArgs.Empty);
    }

    protected void OnLoading() {
        if (IsDisposed) { return; }
        Loading?.Invoke(this, System.EventArgs.Empty);
    }

    private void Checker_Tick(object state) {
        if (IsDisposed) { return; }

        Row.ExecuteValueChanged();

        if (!string.IsNullOrEmpty(EditableErrorReason(BlueBasics.Enums.EditableErrorReason.Save))) { return; }
        if (!LogUndo) { return; }

        Row.ExecuteExtraThread();

        _checkerTickCount++;
        if (_checkerTickCount < 0) { return; }

        if (!HasPendingChanges) {
            _checkerTickCount = 0;
            return;
        }

        if (HasPendingChanges &&
            ((_checkerTickCount > 20 && DateTime.UtcNow.Subtract(Develop.LastUserActionUtc).TotalSeconds > 20) ||
            _checkerTickCount > 180)) {
            if (!string.IsNullOrEmpty(EditableErrorReason(BlueBasics.Enums.EditableErrorReason.Save))) { return; }

            _ = Save();
            _checkerTickCount = 0;
        }
    }

    private void EventScript_Changed(object sender, System.EventArgs e) => EventScript = _eventScript.AsReadOnly();

    private void OnDisposing() => Disposing?.Invoke(this, System.EventArgs.Empty);

    private void OnSortParameterChanged() {
        if (IsDisposed) { return; }
        SortParameterChanged?.Invoke(this, System.EventArgs.Empty);
    }

    private bool PermissionCheckWithoutAdmin(string allowed, RowItem? row) {
        var tmpName = UserName.ToUpper();
        var tmpGroup = UserGroup.ToUpper();
        if (string.Equals(allowed, Constants.Everybody, StringComparison.OrdinalIgnoreCase)) {
            return true;
        }

        if (string.Equals(allowed, "#ROWCREATOR", StringComparison.OrdinalIgnoreCase)) {
            if (row != null && Cell.GetString(Column.SysRowCreator, row).ToUpper() == tmpName) { return true; }
        } else if (string.Equals(allowed, "#USER: " + tmpName, StringComparison.OrdinalIgnoreCase)) {
            return true;
        } else if (string.Equals(allowed, "#USER:" + tmpName, StringComparison.OrdinalIgnoreCase)) {
            return true;
        } else if (string.Equals(allowed, tmpGroup, StringComparison.OrdinalIgnoreCase)) {
            return true;
        }
        return false;
    }

    private void QuickImage_NeedImage(object sender, NeedImageEventArgs e) {
        try {
            if (e.Done) { return; }
            // Es werden alle Datenbankn abgefragt, also kann nach der ersten nicht schluss sein...

            if (string.IsNullOrWhiteSpace(AdditionalFilesPfadWhole())) {
                //OnDropMessage(FehlerArt.Info, "Bild '" + e.Name + "' nicht gefunden, da das Verzeichniss mit den Zusatzdateien nicht existiert.");
                return;
            }

            var name = e.Name.RemoveChars(Constants.Char_DateiSonderZeichen);
            var hashname = name.GetHashString();

            var fullname = AdditionalFilesPfadWhole() + name + ".png";
            var fullhashname = CachePfad.TrimEnd("\\") + "\\" + hashname;

            if (!string.IsNullOrWhiteSpace(CachePfad)) {
                if (FileExists(fullhashname)) {
                    FileInfo f = new(fullhashname);
                    if (DateTime.Now.Subtract(f.CreationTime).TotalDays < VorhalteZeit && Constants.GlobalRND.Next(0, VorhalteZeit * 20) != 1) {
                        if (f.Length < 5) { return; }
                        e.Bmp = new BitmapExt(fullhashname);
                        return;
                    }
                    _ = DeleteFile(fullhashname, false);
                }
            }

            if (FileExists(fullname)) {
                e.Done = true;
                e.Bmp = new BitmapExt(fullname);
                if (!string.IsNullOrWhiteSpace(CachePfad)) {
                    _ = CopyFile(fullname, fullhashname, false);
                    try {
                        //File.SetLastWriteTime(fullhashname, DateTime.Now);
                        File.SetCreationTime(fullhashname, DateTime.Now);
                        //var x = new FileInfo(fullname);
                        //x.CreationTime = DateTime.Now;
                    } catch { }
                }
                return;
            }
            //OnDropMessage(FehlerArt.Info, "Bild '" + e.Name + "' im Verzeihniss der Zusatzdateien nicht gefunden.");

            #region   Datei nicht vorhanden, dann einen Dummy speichern

            if (!string.IsNullOrWhiteSpace(CachePfad)) {
                var l = new List<string>();
                l.Save(fullhashname, Encoding.UTF8, false);
            }

            #endregion
        } catch { }
    }

    private void Variables_Changed() => Variables = new VariableCollection(_variables);

    #endregion
}