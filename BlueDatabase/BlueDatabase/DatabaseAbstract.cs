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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.EventArgs;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using BlueScript.Variables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using static BlueBasics.Converter;
using static BlueBasics.IO;
using Timer = System.Threading.Timer;

namespace BlueDatabase;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class DatabaseAbstract : IDisposableExtended {

    #region Fields

    public const string DatabaseVersion = "4.00";

    public static readonly ListExt<DatabaseAbstract> AllFiles = new();
    public readonly ListExt<ColumnViewCollection> _columnArrangements = new();
    public readonly List<string> _datenbankAdmin = new();
    public readonly CellCollection Cell;

    public readonly ColumnCollection Column;
    public readonly RowCollection Row;
    public readonly string TableName = string.Empty;
    public readonly string UserName = Generic.UserName().ToUpper();
    public string UserGroup;
    protected string? _additionalFilesPfadtmp;

    private static List<ConnectionInfo> _allavailabletables = new List<ConnectionInfo>();
    private static DateTime _lastTableCheck = new DateTime(1900, 1, 1);
    private static List<ConnectionInfo> possibletables = new List<ConnectionInfo>();
    private readonly LayoutCollection _layouts = new();
    private readonly List<string> _permissionGroupsNewRow = new();
    private readonly long _startTick = DateTime.UtcNow.Ticks;
    private readonly List<string> _tags = new();
    private string _additionalFilesPfad = string.Empty;
    private BackgroundWorker _backgroundWorker;
    private string _cachePfad = string.Empty;
    private string _caption = string.Empty;
    private Timer _checker;
    private int _checkerTickCount = -5;
    private string _createDate = string.Empty;
    private string _creator = string.Empty;

    /// <summary>
    /// Exporte werden nur internal verwaltet. Wegen zu vieler erzeigter Pendings, z.B. bei LayoutExport.
    /// Der Head-Editor kann und muss (manuelles Löschen) auf die Exporte Zugreifen und kümmert sich auch um die Pendings
    /// </summary>
    private ListExt<ExportDefinition?> _export = new();

    private string _firstColumn;

    private double _globalScale;

    private string _globalShowPass = string.Empty;

    private DateTime _lastUserActionUtc = new(1900, 1, 1);

    private string _rulesScript = string.Empty;

    private RowSortDefinition? _sortDefinition;

    /// <summary>
    /// Die Eingabe des Benutzers. Ist der Pfad gewünscht, muss FormulaFileName benutzt werden.
    /// </summary>
    private string _standardFormulaFile = string.Empty;

    //private string _timeCode = string.Empty;
    private int _undoCount;

    private string _zeilenQuickInfo = string.Empty;

    #endregion

    #region Constructors

    protected DatabaseAbstract(string tablename, bool readOnly) {
        ReadOnly = readOnly;
        TableName = SQLBackAbstract.MakeValidTableName(tablename);
        UserGroup = "#Administrator";
        Cell = new CellCollection(this);

        QuickImage.NeedImage += QuickImage_NeedImage;

        Row = new RowCollection(this);
        Column = new ColumnCollection(this);
    }

    #endregion

    #region Delegates

    public delegate string NeedPassword();

    #endregion

    #region Events

    public event EventHandler? Disposing;

    public event EventHandler<MessageEventArgs>? DropMessage;

    public event CancelEventHandler? Exporting;

    public event EventHandler<GenerateLayoutInternalEventArgs>? GenerateLayoutInternal;

    public event EventHandler? Loaded;

    public event EventHandler? Loading;

    public event EventHandler<ProgressbarEventArgs>? ProgressbarInfo;

    public event EventHandler<RowCancelEventArgs>? ScriptError;

    public event EventHandler? SortParameterChanged;

    public event EventHandler? ViewChanged;

    #endregion

    #region Properties

    [Browsable(false)]
    [Description("In diesem Pfad suchen verschiedene Routinen (Spalten Bilder, Layouts, etc.) nach zusätzlichen Dateien.")]
    public string AdditionalFilesPfad {
        get => _additionalFilesPfad;
        set {
            if (_additionalFilesPfad == value) { return; }
            _additionalFilesPfadtmp = null;
            ChangeData(DatabaseDataType.AdditionalFilesPath, null, null, _additionalFilesPfad, value, string.Empty);
            Cell.InvalidateAllSizes();
        }
    }

    [Browsable(false)]
    public string CachePfad {
        get => _cachePfad;
        set {
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
            ChangeData(DatabaseDataType.Caption, null, null, _caption, value, string.Empty);
        }
    }

    public ListExt<ColumnViewCollection> ColumnArrangements {
        get => _columnArrangements;
        set {
            if (_columnArrangements.ToString() == value.ToString()) { return; }
            ChangeData(DatabaseDataType.ColumnArrangement, null, null, _columnArrangements.ToString(), value.ToString(), string.Empty);
            OnViewChanged();
        }
    }

    public abstract ConnectionInfo ConnectionData { get; }

    [Browsable(false)]
    public string CreateDate {
        get => _createDate;
        set {
            if (_createDate == value) { return; }
            ChangeData(DatabaseDataType.CreateDateUTC, null, null, _createDate, value, string.Empty);
        }
    }

    [Browsable(false)]
    public string Creator {
        get => _creator.Trim();
        set {
            if (_creator == value) { return; }
            ChangeData(DatabaseDataType.Creator, null, null, _creator, value, string.Empty);
        }
    }

    public List<string> DatenbankAdmin {
        get => _datenbankAdmin;
        set {
            if (!_datenbankAdmin.IsDifferentTo(value)) { return; }
            ChangeData(DatabaseDataType.DatabaseAdminGroups, null, null, _datenbankAdmin.JoinWithCr(), value.JoinWithCr(), string.Empty);
        }
    }

    public ListExt<ExportDefinition?> Export {
        get => _export;
        set {
            if (_export.ToString() == value.ToString()) { return; }
            ChangeData(DatabaseDataType.AutoExport, null, null, _export.ToString(true), value.ToString(true), string.Empty);
        }
    }

    [Browsable(false)]
    [Description("Welche Spalte bei Columnfirst zurückgegeben wird")]
    public string FirstColumn {
        get => _firstColumn;
        set {
            if (_firstColumn == value) { return; }
            ChangeData(DatabaseDataType.FirstColumn, null, null, _firstColumn, value, string.Empty);
        }
    }

    [Browsable(false)]
    public double GlobalScale {
        get => _globalScale;
        set {
            if (_globalScale == value) { return; }
            ChangeData(DatabaseDataType.GlobalScale, null, null, _globalScale.ToString(CultureInfo.InvariantCulture), value.ToString(CultureInfo.InvariantCulture), string.Empty);
            Cell.InvalidateAllSizes();
        }
    }

    public string GlobalShowPass {
        get => _globalShowPass;
        set {
            if (_globalShowPass == value) { return; }
            ChangeData(DatabaseDataType.GlobalShowPass, null, null, _globalShowPass, value, string.Empty);
        }
    }

    public bool HasPendingChanges { get; protected set; } = false;

    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Letzter Lade-Stand der Daten. Wird in OnLoaded gesetzt
    /// </summary>
    public DateTime? IsInCache { get; private set; } = null;

    public LayoutCollection Layouts {
        get => _layouts;
        set {
            if (!_layouts.IsDifferentTo(value)) { return; }
            ChangeData(DatabaseDataType.Layouts, null, null, _layouts.JoinWithCr(), value.JoinWithCr(), string.Empty);
        }
    }

    public string LoadedVersion { get; private set; }

    public bool LogUndo { get; set; } = true;

    public List<string>? PermissionGroupsNewRow {
        get => _permissionGroupsNewRow;
        set {
            if (!_permissionGroupsNewRow.IsDifferentTo(value)) { return; }
            ChangeData(DatabaseDataType.PermissionGroupsNewRow, null, null, _permissionGroupsNewRow.JoinWithCr(), value.JoinWithCr(), string.Empty);
        }
    }

    public DateTime PowerEdit { get; set; }

    public bool ReadOnly { get; private set; } = false;

    public string RulesScript {
        get => _rulesScript;
        set {
            if (_rulesScript == value) { return; }
            ChangeData(DatabaseDataType.RulesScript, null, null, _rulesScript, value, string.Empty);
        }
    }

    [Browsable(false)]
    public RowSortDefinition? SortDefinition {
        get => _sortDefinition;
        set {
            var alt = string.Empty;
            var neu = string.Empty;
            if (_sortDefinition != null) { alt = _sortDefinition.ToString(); }
            if (value != null) { neu = value.ToString(); }
            if (alt == neu) { return; }
            ChangeData(DatabaseDataType.SortDefinition, null, null, alt, neu, string.Empty);

            OnSortParameterChanged();
        }
    }

    /// <summary>
    /// Die Eingabe des Benutzers. Ist der Pfad gewünscht, muss FormulaFileName benutzt werden.
    /// </summary>
    [Browsable(false)]
    [Description("Das standardmäßige Formular - dessen Dateiname -, das angezeigt werden soll.")]
    public string StandardFormulaFile {
        get => _standardFormulaFile;
        set {
            if (_standardFormulaFile == value) { return; }
            ChangeData(DatabaseDataType.StandardFormulaFile, null, null, _standardFormulaFile, value, string.Empty);
        }
    }

    public List<string> Tags {
        get => _tags;
        set {
            if (!_tags.IsDifferentTo(value)) { return; }
            ChangeData(DatabaseDataType.Tags, null, null, _tags.JoinWithCr(), value.JoinWithCr(), string.Empty);
        }
    }

    [Browsable(false)]
    public int UndoCount {
        get => _undoCount;
        set {
            if (_undoCount == value) { return; }
            ChangeData(DatabaseDataType.UndoCount, null, null, _undoCount.ToString(), value.ToString(), string.Empty);
        }
    }

    [Browsable(false)]
    public string ZeilenQuickInfo {
        get => _zeilenQuickInfo;
        set {
            if (_zeilenQuickInfo == value) { return; }
            ChangeData(DatabaseDataType.RowQuickInfo, null, null, _zeilenQuickInfo, value, string.Empty);
        }
    }

    #endregion

    #region Methods

    public static List<ConnectionInfo> AllAvailableTables(List<string>? ignorePath) {
        if (DateTime.Now.Subtract(_lastTableCheck).TotalMinutes < 1) {
            return _allavailabletables;
        }

        // Wird benutzt, um z.b. das Dateisystem nicht doppelt und dreifach abzufragen.
        // Wenn eine Datenbank z.B. im gleichen Verzeichnis liegt,
        // reicht es, das Verzeichnis einmal zu prüfen
        var allreadychecked = new List<DatabaseAbstract>();

        var alf = new List<DatabaseAbstract>();// könnte sich ändern, deswegen Zwischenspeichern
        alf.AddRange(AllFiles);

        foreach (var thisDB in alf) {
            var possibletables = thisDB.AllAvailableTables(allreadychecked, ignorePath);

            allreadychecked.Add(thisDB);

            if (possibletables != null) {
                var canadd = true;

                foreach (var thistable in possibletables) {

                    #region prüfen, ob schon voranden, z.b. DatabaseAbstract.AllFiles

                    foreach (var checkme in _allavailabletables) {
                        if (string.Equals(checkme.TableName, thistable.TableName, StringComparison.InvariantCultureIgnoreCase)) {
                            canadd = false;
                            break;
                        }
                    }

                    #endregion

                    if (canadd) { _allavailabletables.Add(thistable); }
                }
            }
        }

        _lastTableCheck = DateTime.Now;
        return _allavailabletables;
    }

    public static void ForceSaveAll() {
        var x = AllFiles.Count;
        foreach (var thisFile in AllFiles) {
            thisFile?.Save();
            if (x != AllFiles.Count) {
                // Die Auflistung wurde verändert! Selten, aber kann passieren!
                ForceSaveAll();
                return;
            }
        }
    }

    /// <summary>
    /// Sucht die Datenbank im Speicher. Wird sie nicht gefunden, wird sie geladen.
    /// </summary>
    public static DatabaseAbstract? GetByID(ConnectionInfo? ci, DatabaseAbstract.NeedPassword? needPassword) {
        if (ci is null) { return null; }

        foreach (var thisFile in AllFiles) {
            var d = thisFile.ConnectionData;

            if (string.Equals(d.UniqueID, ci.UniqueID, StringComparison.OrdinalIgnoreCase)) {
                return thisFile;
            }

            if (d.AdditionalData.ToLower().EndsWith(".mdb")) {
                if (d.AdditionalData.Equals(ci.AdditionalData, StringComparison.OrdinalIgnoreCase)) {
                    return thisFile; // Multiuser - nicht multiuser konflikt
                }
            }
        }

        if (ci.Provider != null) {
            return ci.Provider.GetOtherTable(ci.TableName);
        }

        #region Wenn die Connection einem Dateinamen entspricht, versuchen den zu laden

        if (FileExists(ci.AdditionalData)) {
            if (ci.AdditionalData.FileSuffix().ToLower() == "mdb") {
                return new Database(ci.AdditionalData, false, false, ci.TableName, needPassword);
            }

            //if (connectionID.FileSuffix().ToLower() == "mdf") {
            //    var x = new SQLBackMicrosoftCE(connectionID, false);

            //    var tables = x.Tables();
            //    var tabn = string.Empty;
            //    if (tables.Count >= 1) { tabn = tables[0]; }

            //    if (string.IsNullOrEmpty(tabn)) {
            //        tabn = "MAIN";
            //        x.RepairAll(tabn);
            //    }

            //    return new DatabaseSQL(x, false, tabn);
            //}
        }

        #endregion

        //SQLBackAbstract.GetSQLBacks();

        if (SQLBackAbstract.ConnectedSQLBack != null) {
            foreach (var thisSQL in SQLBackAbstract.ConnectedSQLBack) {
                var h = thisSQL.HandleMe(ci);
                if (h != null) {
                    return new DatabaseSQLLite(h, false, ci.TableName);
                }
            }
        }

        return null;
    }

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
                        pf = System.Windows.Forms.Application.StartupPath + "\\..\\..\\..\\..\\BlueControls\\BlueControls\\Ressourcen\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 1:
                        pf = System.Windows.Forms.Application.StartupPath + "\\..\\..\\..\\..\\..\\..\\BlueControls\\Ressourcen\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 2:
                        pf = System.Windows.Forms.Application.StartupPath + "\\..\\..\\..\\..\\BlueControls\\Ressourcen\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 3:
                        pf = System.Windows.Forms.Application.StartupPath + "\\..\\..\\..\\BlueControls\\Ressourcen\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 4:
                        pf = System.Windows.Forms.Application.StartupPath + "\\" + name;
                        break;

                    case 5:
                        pf = System.Windows.Forms.Application.StartupPath + "\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 6:
                        pf = System.Windows.Forms.Application.StartupPath + "\\..\\..\\..\\..\\..\\Visual Studio Git\\BlueControls\\Ressourcen\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 7:
                        pf = System.Windows.Forms.Application.StartupPath + "\\..\\..\\..\\..\\Visual Studio Git\\BlueControls\\Ressourcen\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 8:
                        // warscheinlich BeCreative, Firma
                        pf = System.Windows.Forms.Application.StartupPath + "\\..\\..\\..\\..\\Visual Studio Git\\BlueElements\\BlueControls\\BlueControls\\Ressourcen\\" + blueBasicsSubDir + "\\" + name;
                        break;

                    case 9:
                        // Bildzeichen-Liste, Firma, 25.10.2021
                        pf = System.Windows.Forms.Application.StartupPath + "\\..\\..\\..\\..\\..\\Visual Studio Git\\BlueElements\\BlueControls\\BlueControls\\Ressourcen\\" + blueBasicsSubDir + "\\" + name;
                        break;
                }
                if (FileExists(pf)) {
                    var ci = new ConnectionInfo(pf);

                    var tmp = GetByID(ci, null);
                    if (tmp != null) { return tmp; }
                    tmp = new DatabaseMultiUser(pf, false, false, pf.FileNameWithoutSuffix());
                    return tmp;
                }
            } while (pf != string.Empty);
        }
        var d = Generic.GetEmmbedResource(assembly, name);
        if (d != null) { return new Database(d, name.ToUpper().TrimEnd(".MDB")); }
        if (fehlerAusgeben) { Develop.DebugPrint(FehlerArt.Fehler, "Ressource konnte nicht initialisiert werden: " + blueBasicsSubDir + " - " + name); }
        return null;
    }

    public static ConnectionInfo? ProviderOf(string tablename) {
        var alf = new List<DatabaseAbstract>();// könnte sich ändern, deswegen Zwischenspeichern
        alf.AddRange(AllFiles);

        foreach (var thisDB in alf) {
            if (thisDB.ConnectionDataOfOtherTable(tablename, true) is ConnectionInfo ci) {
                return ci;
            }
        }

        return null;
    }

    /// <summary>
    /// Der komplette Pfad mit abschließenden \
    /// </summary>
    /// <returns></returns>
    public virtual string AdditionalFilesPfadWhole() {
        if (_additionalFilesPfadtmp != null) { return _additionalFilesPfadtmp; }

        if (!string.IsNullOrEmpty(_additionalFilesPfad)) {
            var t = _additionalFilesPfad.CheckPath();
            if (DirectoryExists(t)) {
                _additionalFilesPfadtmp = t;
                return t;
            }
        }

        _additionalFilesPfadtmp = string.Empty;
        return string.Empty;
    }

    public abstract List<ConnectionInfo>? AllAvailableTables(List<DatabaseAbstract>? allreadychecked, List<string>? ignorePath);

    public void CancelBackGroundWorker() {
        if (_backgroundWorker != null && _backgroundWorker.IsBusy && !_backgroundWorker.CancellationPending) {
            _backgroundWorker?.CancelAsync();
        }
    }

    /// <summary>
    /// Diese Methode setzt einen Wert dauerhaft und kümmert sich um alles, was dahingehend zu tun ist (z.B. Undo).
    /// Der Wert wird intern fest verankert - bei ReadOnly werden aber weitere Schritte ignoriert.
    /// </summary>
    /// <param name="comand"></param>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="previousValue"></param>
    /// <param name="changedTo"></param>
    public string ChangeData(DatabaseDataType comand, long? columnkey, long? rowkey, string previousValue, string changedTo, string comment) {
        var f = SetValueInternal(comand, changedTo, columnkey, rowkey, -1, -1, false);

        if (!string.IsNullOrEmpty(f)) { return f; }

        //if (isLoading) { return f; }

        if (ReadOnly) {
            if (comand == DatabaseDataType.ColumnContentWidth) { return string.Empty; }
            if (comand == DatabaseDataType.FirstColumn) { return string.Empty; }
            if (!string.IsNullOrEmpty(TableName)) {
                Develop.DebugPrint(FehlerArt.Warnung, "Datei ist Readonly, " + comand + ", " + TableName);
            }
            return "Schreibschutz aktiv";
        }

        if (LogUndo) {
            AddUndo(TableName, comand, columnkey, rowkey, previousValue, changedTo, UserName, comment);
        }

        if (comand != DatabaseDataType.AutoExport) { SetUserDidSomething(); } // Ansonsten wir der Export dauernd unterbrochen

        return string.Empty;
    }

    /// <summary>
    /// Einstellungen der Quell-Datenbank auf diese hier übertragen
    /// </summary>
    /// <param name="sourceDatabase"></param>
    /// <param name="cellDataToo"></param>
    /// <param name="tagsToo"></param>
    public void CloneFrom(DatabaseAbstract sourceDatabase, bool cellDataToo, bool tagsToo) {
        sourceDatabase.SetReadOnly();

        Column.CloneFrom(sourceDatabase);

        if (cellDataToo) { Row.CloneFrom(sourceDatabase); }

        FirstColumn = sourceDatabase.FirstColumn;
        AdditionalFilesPfad = sourceDatabase.AdditionalFilesPfad;
        CachePfad = sourceDatabase.CachePfad; // Nicht so wichtig ;-)
        Caption = sourceDatabase.Caption;
        //TimeCode = sourceDatabase.TimeCode;
        CreateDate = sourceDatabase.CreateDate;
        Creator = sourceDatabase.Creator;
        //Filename - nope
        //Tablename - nope
        //TimeCode - nope
        GlobalScale = sourceDatabase.GlobalScale;
        GlobalShowPass = sourceDatabase.GlobalShowPass;
        RulesScript = sourceDatabase.RulesScript;
        if (SortDefinition == null || SortDefinition.ToString() != sourceDatabase.SortDefinition?.ToString()) {
            SortDefinition = new RowSortDefinition(this, sourceDatabase.SortDefinition?.ToString());
        }
        StandardFormulaFile = sourceDatabase.StandardFormulaFile;
        UndoCount = sourceDatabase.UndoCount;
        ZeilenQuickInfo = sourceDatabase.ZeilenQuickInfo;
        if (tagsToo) {
            Tags = sourceDatabase.Tags;
        }
        Layouts = sourceDatabase.Layouts;

        DatenbankAdmin = sourceDatabase.DatenbankAdmin;
        PermissionGroupsNewRow = sourceDatabase.PermissionGroupsNewRow;

        var tcvc = new ListExt<ColumnViewCollection>();
        foreach (var t in sourceDatabase.ColumnArrangements) {
            tcvc.Add(new ColumnViewCollection(this, t.ToString()));
        }
        ColumnArrangements = tcvc;

        Export = sourceDatabase.Export;

        UndoCount = sourceDatabase.UndoCount;
    }

    public string Column_UsedIn(ColumnItem? column) {
        if (column == null) { return string.Empty; }

        var t = "<b><u>Verwendung von " + column.ReadableText() + "</b></u><br>";
        if (column.IsSystemColumn()) {
            t += " - Systemspalte<br>";
        }

        if (SortDefinition.Columns.Contains(column)) { t += " - Sortierung<br>"; }
        //var view = false;
        //foreach (var thisView in OldFormulaViews) {
        //    if (thisView[column] != null) { view = true; }
        //}
        //if (view) { t += " - Formular-Ansichten<br>"; }
        var cola = false;
        var first = true;
        foreach (var thisView in _columnArrangements) {
            if (!first && thisView[column] != null) { cola = true; }
            first = false;
        }
        if (cola) { t += " - Benutzerdefinierte Spalten-Anordnungen<br>"; }
        if (RulesScript.ToUpper().Contains(column.Name.ToUpper())) { t += " - Regeln-Skript<br>"; }
        if (ZeilenQuickInfo.ToUpper().Contains(column.Name.ToUpper())) { t += " - Zeilen-Quick-Info<br>"; }
        if (_tags.JoinWithCr().ToUpper().Contains(column.Name.ToUpper())) { t += " - Datenbank-Tags<br>"; }
        var layout = false;
        foreach (var thisLayout in _layouts) {
            if (thisLayout.Contains(column.Name.ToUpper())) { layout = true; }
        }
        if (layout) { t += " - Layouts<br>"; }
        var l = column.Contents();
        if (l.Count > 0) {
            t += "<br><br><b>Zusatz-Info:</b><br>";
            t = t + " - Befüllt mit " + l.Count + " verschiedenen Werten";
        }
        return t;
    }

    public abstract ConnectionInfo? ConnectionDataOfOtherTable(string tableName, bool checkExists);

    /// <summary>
    /// AdditionalFiles/Datenbankpfad mit Backup und abschließenden \
    /// </summary>
    /// <returns></returns>
    public string DefaultBackupPath() {
        if (!string.IsNullOrEmpty(AdditionalFilesPfadWhole())) { return AdditionalFilesPfadWhole() + "Backup\\"; }
        //if (!string.IsNullOrEmpty(Filename)) { return Filename.FilePath() + "Forms\\"; }
        return string.Empty;
    }

    /// <summary>
    /// AdditionalFiles/Datenbankpfad mit Forms und abschließenden \
    /// </summary>
    /// <returns></returns>
    public string DefaultFormulaPath() {
        if (!string.IsNullOrEmpty(AdditionalFilesPfadWhole())) { return AdditionalFilesPfadWhole() + "Forms\\"; }
        //if (!string.IsNullOrEmpty(Filename)) { return Filename.FilePath() + "Forms\\"; }
        return string.Empty;
    }

    /// <summary>
    /// AdditionalFiles/Datenbankpfad mit Layouts und abschließenden \
    /// </summary>
    public string DefaultLayoutPath() {
        if (!string.IsNullOrEmpty(AdditionalFilesPfadWhole())) { return AdditionalFilesPfadWhole() + "Layouts\\"; }
        //if (!string.IsNullOrEmpty(Filename)) { return Filename.FilePath() + "Layouts\\"; }
        return string.Empty;
    }

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public string ErrorReason(ErrorReason mode) {
        //var f = SpecialErrorReason(mode);

        //if (!string.IsNullOrEmpty(f)) { return f; }
        if (mode == BlueBasics.Enums.ErrorReason.OnlyRead) { return string.Empty; }

        //if (mode.HasFlag(BlueBasics.Enums.ErrorReason.Load)) {
        //    if (_backgroundWorker.IsBusy) { return "Ein Hintergrundprozess verhindert aktuell das Neuladen."; }
        //}

        if (ReadOnly) { return "Datenbank schreibgeschützt!"; }

        if (mode.HasFlag(BlueBasics.Enums.ErrorReason.EditGeneral) || mode.HasFlag(BlueBasics.Enums.ErrorReason.Save)) {
            if (_backgroundWorker.IsBusy) { return "Ein Hintergrundprozess verhindert aktuell die Bearbeitung."; }
        }

        return IntParse(LoadedVersion.Replace(".", "")) > IntParse(DatabaseVersion.Replace(".", ""))
            ? "Diese Programm kann nur Datenbanken bis Version " + DatabaseVersion + " speichern."
            : string.Empty;
    }

    /// <summary>
    /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
    /// </summary>
    /// <param name="firstRow">Ob in der ersten Zeile der Spaltenname ist, oder ob sofort Daten kommen.</param>
    /// <param name="column">Die Spalte, die zurückgegeben wird.</param>
    /// <param name="sortedRows">Die Zeilen, die zurückgegeben werden. NULL gibt alle Zeilen zurück.</param>
    /// <returns></returns>
    public string Export_CSV(FirstRow firstRow, ColumnItem column, List<RowData>? sortedRows) =>
        //Develop.DebugPrint_InvokeRequired(InvokeRequired, false);
        Export_CSV(firstRow, new List<ColumnItem> { column }, sortedRows);

    /// <summary>
    /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
    /// </summary>
    /// <param name="firstRow">Ob in der ersten Zeile der Spaltenname ist, oder ob sofort Daten kommen.</param>
    /// <param name="columnList">Die Spalten, die zurückgegeben werden. NULL gibt alle Spalten zurück.</param>
    /// <param name="sortedRows">Die Zeilen, die zurückgegeben werden. NULL gibt alle ZEilen zurück.</param>
    /// <returns></returns>
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
                        sb.Append(tmp);
                        if (colNr < columnList.Count - 1) { sb.Append(";"); }
                    }
                }
                sb.Append("\r\n");
                break;

            case FirstRow.ColumnInternalName:
                for (var colNr = 0; colNr < columnList.Count; colNr++) {
                    if (columnList[colNr] != null) {
                        sb.Append(columnList[colNr].Name);
                        if (colNr < columnList.Count - 1) { sb.Append(';'); }
                    }
                }
                sb.Append("\r\n");
                break;

            default:
                Develop.DebugPrint(firstRow);
                break;
        }
        foreach (var thisRow in sortedRows) {
            if (thisRow != null) {
                for (var colNr = 0; colNr < columnList.Count; colNr++) {
                    if (columnList[colNr] != null) {
                        var tmp = Cell.GetString(columnList[colNr], thisRow.Row);
                        tmp = tmp.Replace("\r\n", "|");
                        tmp = tmp.Replace("\r", "|");
                        tmp = tmp.Replace("\n", "|");
                        tmp = tmp.Replace(";", "<sk>");
                        sb.Append(tmp);
                        if (colNr < columnList.Count - 1) { sb.Append(';'); }
                    }
                }
                sb.Append("\r\n");
            }
        }
        return sb.ToString().TrimEnd("\r\n");
    }

    /// <summary>
    /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
    /// </summary>
    /// <param name="firstRow">Ob in der ersten Zeile der Spaltenname ist, oder ob sofort Daten kommen.</param>
    /// <param name="arrangement">Die Spalten, die zurückgegeben werden. NULL gibt alle Spalten zurück.</param>
    /// <param name="sortedRows">Die Zeilen, die zurückgegeben werden. NULL gibt alle ZEilen zurück.</param>
    /// <returns></returns>
    public string Export_CSV(FirstRow firstRow, ColumnViewCollection? arrangement, List<RowData>? sortedRows) => Export_CSV(firstRow, arrangement?.ListOfUsedColumn(), sortedRows);

    /// <summary>
    /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
    /// </summary>
    /// <returns></returns>
    public string Export_CSV(FirstRow firstRow, int arrangementNo, FilterCollection? filter, List<RowItem>? pinned) => Export_CSV(firstRow, _columnArrangements[arrangementNo].ListOfUsedColumn(), Row.CalculateSortedRows(filter, SortDefinition, pinned, null));

    /// <summary>
    /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
    /// </summary>
    /// <returns></returns>
    public void Export_HTML(string filename, int arrangementNo, FilterCollection? filter, List<RowItem>? pinned) => Export_HTML(filename, _columnArrangements[arrangementNo].ListOfUsedColumn(), Row.CalculateSortedRows(filter, SortDefinition, pinned, null), false);

    /// <summary>
    /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
    /// </summary>
    /// <returns></returns>
    public void Export_HTML(string filename, List<ColumnItem?>? columnList, List<RowData?>? sortedRows, bool execute) {
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
                //da.GenerateAndAdd("        <th bgcolor=\"#" + ThisColumn.BackColor.ToHTMLCode() + "\"><b>" + ThisColumn.ReadableText().Replace(";", "<br>") + "</b></th>");
            }
        }
        da.RowEnd();
        foreach (var thisRow in sortedRows) {
            if (thisRow != null) {
                da.RowBeginn();
                foreach (var thisColumn in columnList) {
                    if (thisColumn != null) {
                        var lcColumn = thisColumn;
                        var lCrow = thisRow.Row;
                        if (thisColumn.Format is DataFormat.Verknüpfung_zu_anderer_Datenbank) {
                            (lcColumn, lCrow, _) = CellCollection.LinkedCellData(thisColumn, thisRow.Row, false, false);
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

    /// <summary>
    /// TableViews haben eigene Export-Routinen, die hierauf zugreifen
    /// </summary>
    /// <returns></returns>
    public void Export_HTML(string filename, ColumnViewCollection? arrangement, List<RowData?> sortedRows, bool execute) => Export_HTML(filename, arrangement.ListOfUsedColumn(), sortedRows, execute);

    /// <summary>
    /// Testet die Standard-Verzeichnisse und gibt das Formular zurück, falls es existiert
    /// </summary>
    /// <returns></returns>
    public string? FormulaFileName() {
        if (FileExists(_standardFormulaFile)) { return _standardFormulaFile; }
        if (FileExists(AdditionalFilesPfadWhole() + _standardFormulaFile)) { return AdditionalFilesPfadWhole() + _standardFormulaFile; }
        if (FileExists(DefaultFormulaPath() + _standardFormulaFile)) { return DefaultFormulaPath() + _standardFormulaFile; }
        return null;
    }

    public DatabaseAbstract? GetOtherTable(string tablename) {
        //if (string.IsNullOrEmpty(tablename)) { return null; }

        //var newpf = Filename.FilePath() + tablename.FileNameWithoutSuffix() + ".mdb";

        //return GetByID(new ConnectionInfo(tablename, null, DatabaseID, newpf);
        //// KEINE Vorage mitgeben, weil sonst eine Endlosschleife aufgerufen wird!

        //    public override DatabaseAbstract? GetOtherTable(string tablename) {
        //if (!SQLBackAbstract.IsValidTableName(tablename)) {
        //    return null;
        //}

        var x = ConnectionDataOfOtherTable(tablename, true);
        if (x == null) { return null; }

        x.Provider = null;  // KEINE Vorage mitgeben, weil sonst eine Endlosschleife aufgerufen wird!

        return GetByID(x, null);// new DatabaseSQL(_sql, readOnly, tablename);
    }

    public string Import(string importText, bool spalteZuordnen, bool zeileZuordnen, string splitChar, bool eliminateMultipleSplitter, bool eleminateSplitterAtStart, bool dorowautmatic, string script) {
        // Vorbereitung des Textes -----------------------------
        importText = importText.Replace("\r\n", "\r").Trim("\r");

        #region die Zeilen (zeil) vorbereiten

        var ein = importText.SplitAndCutByCr();
        List<string[]> zeil = new();
        var neuZ = 0;
        for (var z = 0; z <= ein.GetUpperBound(0); z++) {

            #region Das Skript berechnen

            if (!string.IsNullOrEmpty(script)) {
                var vars = new List<Variable>();
                vars.Add(new VariableString("Row", ein[z], false, false, "Der Original-Text. Dieser kann (und soll) manipuliert werden."));
                vars.Add(new VariableBool("IsCaption", spalteZuordnen && z == 0, true, false, "Wenn TRUE, ist das die erste Zeile, die Überschriften enthält."));
                vars.Add(new VariableString("Seperator", splitChar, true, false, "Das Trennzeichen"));
                var x = new BlueScript.Script(vars, string.Empty, false);
                x.ScriptText = script;
                if (!x.Parse()) {
                    OnDropMessage(FehlerArt.Warnung, "Skript-Fehler, Import kann nicht ausgeführt werden.");
                    return x.Error;
                }
                ein[z] = vars.GetString("Row");
            }

            #endregion

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
            OnDropMessage(FehlerArt.Warnung, "Import kann nicht ausgeführt werden.");
            return "Import kann nicht ausgeführt werden.";
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
                zeil[0][spaltNo] = SQLBackAbstract.MakeValidColumnName(zeil[0][spaltNo]);

                var col = Column.Exists(zeil[0][spaltNo]);
                if (col == null) {
                    col = Column.GenerateAndAdd(zeil[0][spaltNo]);
                    col.Caption = zeil[0][spaltNo];
                    col.Format = DataFormat.Text;
                }
                columns.Add(col);
            }
        } else {
            columns.AddRange(Column.Where(thisColumn => thisColumn != null && !thisColumn.IsSystemColumn()));
            while (columns.Count < zeil[0].GetUpperBound(0) + 1) {
                var newc = Column.GenerateAndAdd();
                newc.Caption = newc.Name;
                newc.Format = DataFormat.Text;
                newc.MultiLine = true;
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
                if (row != null && dorowautmatic) { row.DoAutomatic(true, true, "import"); }
            }
        }

        OnDropMessage(FehlerArt.Info, "<b>Import abgeschlossen.</b>\r\n" + neuZ + " neue Zeilen erstellt.");
        return string.Empty;
    }

    public string ImportCSV(string filename, string script) {
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

        return Import(importText, true, true, sep, false, false, true, script);
    }

    public void InvalidateExports(string layoutId) {
        if (ReadOnly) { return; }

        var ex = Export.CloneWithClones();

        foreach (var thisExport in ex) {
            if (thisExport != null) {
                if (thisExport.Typ == ExportTyp.EinzelnMitFormular) {
                    if (thisExport.ExportFormularId == layoutId) {
                        thisExport.LastExportTimeUtc = new DateTime(1900, 1, 1);
                    }
                }
            }
        }

        Export = ex;
    }

    public bool IsAdministrator() {
        if (UserGroup.ToUpper() == "#ADMINISTRATOR") { return true; }
        if (_datenbankAdmin == null || _datenbankAdmin.Count == 0) { return false; }
        if (_datenbankAdmin.Contains("#EVERYBODY", false)) { return true; }
        if (!string.IsNullOrEmpty(UserName) && _datenbankAdmin.Contains("#User: " + UserName, false)) { return true; }
        return !string.IsNullOrEmpty(UserGroup) && _datenbankAdmin.Contains(UserGroup, false);
    }

    //public abstract void Load_Reload();

    public void OnScriptError(RowCancelEventArgs e) {
        if (IsDisposed) { return; }
        ScriptError?.Invoke(this, e);
    }

    public void OnViewChanged() {
        if (IsDisposed) { return; }
        ViewChanged?.Invoke(this, System.EventArgs.Empty);
    }

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
        e.Add("#Everybody");
        e.Add("#User: " + UserName);
        if (cellLevel) {
            e.Add("#RowCreator");
        } else {
            e.RemoveString("#RowCreator", false);
        }
        e.RemoveString("#Administrator", false);
        if (!IsAdministrator()) { e.Add(UserGroup); }
        return e.SortedDistinctList();
    }

    public bool PermissionCheck(List<string>? allowed, RowItem? row) {
        try {
            if (IsAdministrator()) { return true; }
            if (PowerEdit.Subtract(DateTime.Now).TotalSeconds > 0) { return true; }
            if (allowed == null || allowed.Count == 0) { return false; }
            if (allowed.Any(thisString => PermissionCheckWithoutAdmin(thisString, row))) {
                return true;
            }
        } catch (Exception ex) {
            Develop.DebugPrint(FehlerArt.Warnung, ex);
        }
        return false;
    }

    public void RefreshColumnsData(ColumnItem column) {
        if (column.IsInCache != null) { return; }
        RefreshColumnsData(new List<ColumnItem>() { column });
    }

    public abstract void RefreshColumnsData(List<ColumnItem>? columns);

    public void RefreshColumnsData(List<FilterItem>? filter) {
        if (filter != null) {
            var c = new ListExt<ColumnItem>();

            foreach (var thisF in filter) {
                if (thisF.Column != null && thisF.Column.IsInCache == null) {
                    c.AddIfNotExists(thisF.Column);
                }
            }
            RefreshColumnsData(c);
        }
    }

    public abstract bool RefreshRowData(List<RowItem> row, bool refreshAlways);

    public bool RefreshRowData(List<long> keys, bool refreshAlways) {
        if (keys.Count == 0) { return false; }

        var r = new List<RowItem>();
        foreach (var thisK in keys) {
            var ro = Row.SearchByKey(thisK);
            if (ro != null) { r.AddIfNotExists(ro); }
        }
        return RefreshRowData(r, refreshAlways);
    }

    public bool RefreshRowData(RowItem row, bool refreshAlways) => RefreshRowData(new List<RowItem>() { row }, refreshAlways);

    public virtual void RepairAfterParse() {
        Column.Repair();
        RepairColumnArrangements();
        RepairViews();
        _layouts.Check();

        IsInCache = DateTime.UtcNow;
    }

    public abstract bool Save();

    public void SetReadOnly() {
        ReadOnly = true;
        DisposeBackgroundWorker();
    }

    public abstract string UndoText(ColumnItem? column, RowItem? row);

    internal void DevelopWarnung(string t) {
        try {
            t += "\r\nColumn-Count: " + Column.Count;
            t += "\r\nRow-Count: " + Row.Count;
            t += "\r\nTable: " + ConnectionData.TableName;
            t += "\r\nID: " + ConnectionData.DatabaseID;
        } catch { }
        Develop.DebugPrint(FehlerArt.Warnung, t);
    }

    //public abstract void WaitEditable();
    internal void OnDropMessage(FehlerArt type, string message) {
        if (IsDisposed) { return; }
        DropMessage?.Invoke(this, new MessageEventArgs(type, message));
    }

    //public abstract void UnlockHard();
    internal void OnGenerateLayoutInternal(GenerateLayoutInternalEventArgs e) {
        if (IsDisposed) { return; }
        GenerateLayoutInternal?.Invoke(this, e);
    }

    internal void OnProgressbarInfo(ProgressbarEventArgs e) {
        if (IsDisposed) { return; }
        ProgressbarInfo?.Invoke(this, e);
    }

    internal void RepairColumnArrangements() {
        //if (ReadOnly) { return; }  // Gibt fehler bei Datenbanken, die nur Temporär erzeugt werden!

        var x = _columnArrangements.CloneWithClones();

        for (var z = 0; z < Math.Max(2, x.Count); z++) {
            if (x.Count < z + 1) { x.Add(new ColumnViewCollection(this, string.Empty)); }
            x[z].Repair(z);
        }

        ColumnArrangements = x;
    }

    internal void RepairViews() {
        var lay = (LayoutCollection)Layouts.Clone();
        lay.Check();
        Layouts = lay;
    }

    /// <summary>
    /// Diese Routine setzt Werte auf den richtigen Speicherplatz und führt Comands aus.
    /// Echtzeitbasierte Syteme sollten diese Routine verwenden, um den Wert ebenfalls fest zu verankern (sofern !IsLoading && !Readonly)
    /// Nur von Laderoutinen aufzurufen, oder von ChangeData, wenn der Wert bereits fest in der Datenbank verankert ist.
    /// Vorsicht beim Überschreiben:
    /// Da in Columns und Cells abgesprungen wird, und diese nicht überschrieben werden können,
    ///
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns>Leer, wenn da Wert setzen erfolgreich war. Andernfalls der Fehlertext.</returns>
    internal virtual string SetValueInternal(DatabaseDataType type, string value, long? columnkey, long? rowkey, int width, int height, bool isLoading) {
        if (type.IsObsolete()) { return string.Empty; }

        if (type.IsColumnTag()) {
            var c = Column.SearchByKey(columnkey);
            if (c == null) {
                Develop.DebugPrint(FehlerArt.Warnung, "Spalte ist null! " + type.ToString());
                return "Wert nicht gesetzt!";
            }
            return c.SetValueInternal(type, value, isLoading);
        }

        if (type.IsCellValue()) {
            //        case DatabaseDataType.Value_withSizeData:
            //case DatabaseDataType.UTF8Value_withSizeData:
            //case DatabaseDataType.Value_withoutSizeData:
            if (type == DatabaseDataType.UTF8Value_withSizeData) {
                //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var enc1252 = Encoding.GetEncoding(1252);
                value = Encoding.UTF8.GetString(enc1252.GetBytes(value));
            }
            return Cell.SetValueInternal((long)columnkey, (long)rowkey, value, width, height, isLoading);
        }

        if (type.IsCommand()) {
            switch (type) {
                case DatabaseDataType.Comand_RemoveColumn:
                case DatabaseDataType.Comand_AddColumn:
                    return Column.SetValueInternal(type, columnkey, isLoading);

                case DatabaseDataType.Comand_AddRow:
                case DatabaseDataType.Comand_RemoveRow:
                    return Row.SetValueInternal(type, rowkey, isLoading);

                default:
                    if (LoadedVersion == DatabaseVersion) {
                        SetReadOnly();
                        if (!ReadOnly) {
                            Develop.DebugPrint(FehlerArt.Fehler, "Laden von Datentyp \'" + type + "\' nicht definiert.<br>Wert: " + value + "<br>Tabelle: " + ConnectionData.ToString);
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

            case DatabaseDataType.FirstColumn:
                _firstColumn = value;
                break;

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

            case DatabaseDataType.AutoExport:
                _export.Clear();
                List<string> ae = new(value.SplitAndCutByCr());
                foreach (var t in ae) {
                    _export.Add(new ExportDefinition(this, t));
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

            case DatabaseDataType.RulesScript:
                _rulesScript = value;
                break;

            case DatabaseDataType.UndoCount:
                _undoCount = IntParse(value);
                break;

            case DatabaseDataType.UndoInOne:
                // Muss eine übergeordnete Routine bei Befarf abfangen
                break;

            case DatabaseDataType.EOF:
                return string.Empty;

            default:
                // Variable type
                if (LoadedVersion == DatabaseVersion) {
                    SetReadOnly();
                    if (!ReadOnly) {
                        Develop.DebugPrint(FehlerArt.Fehler, "Laden von Datentyp \'" + type + "\' nicht definiert.<br>Wert: " + value + "<br>Tabelle: " + ConnectionData.ToString);
                    }
                }

                return "Datentyp unbekannt.";
        }
        return string.Empty;
    }

    protected abstract void AddUndo(string tableName, DatabaseDataType comand, long? columnKey, long? rowKey, string previousValue, string changedTo, string userName, string comment);

    protected void AddUndo(string tableName, DatabaseDataType comand, ColumnItem? column, RowItem? row, string previousValue, string changedTo, string userName, string comment) => AddUndo(tableName, comand, column?.Key ?? -1, row?.Key ?? -1, previousValue, changedTo, userName, comment);

    protected void CreateWatcher() {
        if (!ReadOnly) {
            if (_backgroundWorker != null) { return; }

            _backgroundWorker = new BackgroundWorker {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = true
            };
            _backgroundWorker.DoWork += BackgroundWorker_DoWork;
            _checker = new Timer(Checker_Tick);
            _checker.Change(2000, 2000);
        }
    }

    protected virtual void Dispose(bool disposing) {
        if (IsDisposed) { return; }
        IsDisposed = true;

        OnDisposing();
        AllFiles.Remove(this);

        //base.Dispose(disposing); // speichert und löscht die ganzen Worker. setzt auch disposedValue und ReadOnly auf true
        if (disposing) {
            // TODO: verwalteten Zustand (verwaltete Objekte) entsorgen.
        }
        DisposeBackgroundWorker();

        if (_checker != null) {
            _checker.Dispose();
            _checker = null;
        }

        // TODO: nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer weiter unten überschreiben.
        // TODO: große Felder auf Null setzen.
        //ColumnArrangements.Changed -= ColumnArrangements_ListOrItemChanged;
        //Layouts.Changed -= Layouts_ListOrItemChanged;
        //Layouts.ItemSeted -= Layouts_ItemSeted;
        //PermissionGroupsNewRow.Changed -= PermissionGroups_NewRow_ListOrItemChanged;
        //Tags.Changed -= DatabaseTags_ListOrItemChanged;
        //Export.Changed -= Export_ListOrItemChanged;
        //DatenbankAdmin.Changed -= DatabaseAdmin_ListOrItemChanged;

        //Row.RowRemoving -= Row_RowRemoving;
        ////Row.RowRemoved -= Row_RowRemoved;
        //Row.RowAdded -= Row_RowAdded;

        //Column.ItemRemoving -= Column_ItemRemoving;
        //Column.ItemRemoved -= Column_ItemRemoved;
        //Column.ItemAdded -= Column_ItemAdded;
        Column.Dispose();
        //Cell.Dispose();
        Row.Dispose();

        //_columnArrangements.Dispose();
        //_cags.Dispose();
        //_export.Dispose();
        //_datenbankAdmin.Dispose();
        //_permissionGroupsNewRow.Dispose();
        _layouts.Dispose();
    }

    protected virtual void Initialize() {
        Cell.Initialize();

        _columnArrangements.Clear();
        _layouts.Clear();
        _permissionGroupsNewRow.Clear();
        _tags.Clear();
        _export.Clear();
        _datenbankAdmin.Clear();
        _globalShowPass = string.Empty;
        _creator = UserName;
        _createDate = DateTime.Now.ToString(Constants.Format_Date5);
        _undoCount = 300;
        _caption = string.Empty;
        //_timeCode = string.Empty;
        //_verwaisteDaten = VerwaisteDaten.Ignorieren;
        LoadedVersion = DatabaseVersion;
        _rulesScript = string.Empty;
        _globalScale = 1f;
        _additionalFilesPfad = "AdditionalFiles";
        _firstColumn = string.Empty;
        _zeilenQuickInfo = string.Empty;
        _sortDefinition = null;
    }

    protected void OnExporting(CancelEventArgs e) {
        if (IsDisposed) { return; }
        Exporting?.Invoke(this, e);
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

    protected virtual void SetUserDidSomething() {
        _lastUserActionUtc = DateTime.UtcNow;
    }

    private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e) {
        if (IsDisposed) { return; }
        if (ReadOnly) { return; }
        if (!LogUndo) { return; }

        var tmpe = _export.CloneWithClones();

        foreach (var thisExport in tmpe) {
            if (_backgroundWorker.CancellationPending || IsDisposed) { break; }

            if (thisExport.IsOk()) {
                //var e2 = new MultiUserFileHasPendingChangesEventArgs();
                //HasPendingChanges(null, e2);

                //if (!e2.HasPendingChanges) {
                CancelEventArgs ec = new(false);
                OnExporting(ec);
                if (ec.Cancel) { break; }
                //}

                thisExport.DeleteOutdatedBackUps(_backgroundWorker);
                if (_backgroundWorker.CancellationPending || IsDisposed) { break; }
                thisExport.DoBackUp(_backgroundWorker);
                if (_backgroundWorker.CancellationPending || IsDisposed) { break; }
            }
        }

        Export = tmpe;
    }

    //protected abstract string SpecialErrorReason(ErrorReason mode);
    private void Checker_Tick(object state) {
        if (IsDisposed) { return; }
        if (ReadOnly) { return; }
        if (!LogUndo) { return; }

        _checkerTickCount++;
        if (_checkerTickCount < 0) { return; }

        if (DateTime.UtcNow.Subtract(_lastUserActionUtc).TotalSeconds < 10) { CancelBackGroundWorker(); return; } // Benutzer arbeiten lassen

        var mustBackup = IsThereBackgroundWorkToDo();

        if (!mustBackup) {
            _checkerTickCount = 0;
            return;
        }

        // Zeiten berechnen

        ////if (_checkerTickCount > countSave && mustSave) { CancelBackGroundWorker(); }

        //var mustReload = ReloadNeeded;

        //if (_checkerTickCount > ReloadDelaySecond && mustReload) { CancelBackGroundWorker(); }
        //if (_backgroundWorker.IsBusy) { return; }

        //if (string.IsNullOrEmpty(ErrorReason(Enums.ErrorReason.EditNormaly))) { return; }

        //if (mustReload && mustSave) {
        //    if (!string.IsNullOrEmpty(ErrorReason(BlueBasics.Enums.ErrorReason.Load))) { return; }
        //    // Checker_Tick_count nicht auf 0 setzen, dass der Saver noch stimmt.
        //    Load_Reload();
        //    return;
        //}

        if (HasPendingChanges && _checkerTickCount > 20) {
            if (!string.IsNullOrEmpty(ErrorReason(BlueBasics.Enums.ErrorReason.Save))) { return; }

            Save();
            _checkerTickCount = 0;
            return;
        }

        if (mustBackup && _checkerTickCount >= 30 && string.IsNullOrEmpty(ErrorReason(BlueBasics.Enums.ErrorReason.EditAcut))) {
            var nowsek = (DateTime.UtcNow.Ticks - _startTick) / 30000000;
            if (nowsek % 20 != 0) { return; } // Lasten startabhängig verteilen. Bei Pending changes ist es eh immer true;

            StartBackgroundWorker();
        }

        //// Überhaupt nix besonderes. Ab und zu mal Reloaden
        //if (mustReload && _checkerTickCount > ReloadDelaySecond) {
        //    RepairOldBlockFiles();
        //    if (!string.IsNullOrEmpty(ErrorReason(BlueBasics.Enums.ErrorReason.Load))) { return; }
        //    Load_Reload();
        //    _checkerTickCount = 0;
        //}
    }

    private void DisposeBackgroundWorker() {
        CancelBackGroundWorker();

        if (_backgroundWorker != null) {
            _backgroundWorker.Dispose();
            _backgroundWorker.DoWork -= BackgroundWorker_DoWork;
        }
    }

    //private void Column_ItemAdded(object sender, ListEventArgs e) {
    //    if (IsLoadingx) { return; }
    //    CheckViewsAndArrangements();
    //}

    //private void Column_ItemRemoved(object sender, System.EventArgs e) {
    //    //if (IsLoadingx) { Develop.DebugPrint(FehlerArt.Warnung, "Loading Falsch!"); }
    //    CheckViewsAndArrangements();

    //    var lay = (LayoutCollection)Layouts.Clone();
    //    lay.Check();
    //    Layouts = lay;
    //}

    private bool IsThereBackgroundWorkToDo() {
        if (IsDisposed || ReadOnly || !LogUndo) { return false; }

        CancelEventArgs ec = new(false);
        OnExporting(ec);
        if (ec.Cancel) { return false; }

        foreach (var thisExport in _export) {
            if (thisExport != null) {
                if (thisExport.Typ == ExportTyp.EinzelnMitFormular) { return true; }
                if (DateTime.UtcNow.Subtract(thisExport.LastExportTimeUtc).TotalDays > thisExport.BackupInterval * 50) { return true; }
            }
        }

        return false;
    }

    private void OnDisposing() {
        Disposing?.Invoke(this, System.EventArgs.Empty);
    }

    private void OnSortParameterChanged() {
        if (IsDisposed) { return; }
        SortParameterChanged?.Invoke(this, System.EventArgs.Empty);
    }

    private bool PermissionCheckWithoutAdmin(string allowed, RowItem? row) {
        var tmpName = UserName.ToUpper();
        var tmpGroup = UserGroup.ToUpper();
        if (allowed.ToUpper() == "#EVERYBODY") {
            return true;
        }

        if (allowed.ToUpper() == "#ROWCREATOR") {
            if (row != null && Cell.GetString(Column.SysRowCreator, row).ToUpper() == tmpName) { return true; }
        } else if (allowed.ToUpper() == "#USER: " + tmpName) {
            return true;
        } else if (allowed.ToUpper() == "#USER:" + tmpName) {
            return true;
        } else if (allowed.ToUpper() == tmpGroup) {
            return true;
        }
        return false;
    }

    private void QuickImage_NeedImage(object sender, NeedImageEventArgs e) {
        try {
            if (e.Done) { return; }
            e.Done = true;

            if (string.IsNullOrWhiteSpace(AdditionalFilesPfadWhole())) { return; }

            var name = e.Name.RemoveChars(Constants.Char_DateiSonderZeichen);
            var hashname = name.GetHashString();

            var fullname = AdditionalFilesPfadWhole() + name + ".png";
            var fullhashname = CachePfad.TrimEnd("\\") + "\\" + hashname;

            if (!string.IsNullOrWhiteSpace(CachePfad)) {
                if (FileExists(fullhashname)) {
                    FileInfo f = new(fullhashname);
                    if (DateTime.Now.Subtract(f.CreationTime).TotalDays < 20) {
                        if (f.Length < 5) { return; }
                        e.Bmp = new BitmapExt(fullhashname);
                        return;
                    }
                    DeleteFile(fullhashname, false);
                }
            }

            if (FileExists(fullname)) {
                e.Bmp = new BitmapExt(fullname);
                if (!string.IsNullOrWhiteSpace(CachePfad)) {
                    CopyFile(fullname, fullhashname, false);
                    try {
                        //File.SetLastWriteTime(fullhashname, DateTime.Now);
                        File.SetCreationTime(fullhashname, DateTime.Now);
                        //var x = new FileInfo(fullname);
                        //x.CreationTime = DateTime.Now;
                    } catch { }
                }
                return;
            }

            var l = new List<string>();
            l.Save(fullhashname, Encoding.UTF8, false);
        } catch { }
    }

    //private void Row_RowAdded(object sender, RowEventArgs e) {
    //    //if (IsLoading) {
    //    ChangeData(DatabaseDataType.Comand_RowAdded, null, e.Row, String.Empty, e.Row.Key.ToString());
    //    //}
    //}

    //private void Row_RowRemoving(object sender, RowEventArgs e) {
    //    ChangeData(DatabaseDataType.Comand_RemovingRow, null, e.Row, string.Empty, e.Row.Key.ToString());
    //}

    //private void Row_RowRemoving(object sender, RowEventArgs e) {
    //    Develop.CheckStackForOverflow();
    //    ChangeData(DatabaseDataType.dummyComand_RemoveRow, null, e.Row, string.Empty, e.Row.Key.ToString());

    //}

    private void StartBackgroundWorker() {
        if (IsDisposed) { return; }

        try {
            if (!string.IsNullOrEmpty(ErrorReason(BlueBasics.Enums.ErrorReason.EditNormaly))) { return; }
            if (!_backgroundWorker.IsBusy) { _backgroundWorker.RunWorkerAsync(); }
        } catch {
            StartBackgroundWorker();
        }
    }

    #endregion
}