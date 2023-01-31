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
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using static BlueBasics.Converter;
using static BlueBasics.Extensions;
using static BlueBasics.IO;

namespace BlueDatabase;

public sealed class ExportDefinition : IParseable, IReadableTextWithChanging, IDisposableExtended, ICloneable, IErrorCheckable {

    #region Fields

    /// <summary>
    /// Das maximale Alter der Backups in Tagen, nachdem sie gelöscht werden
    /// </summary>
    private float _autoDelete;

    /// <summary>
    /// Intervall der Backups in Tagen
    /// </summary>
    private float _backupInterval;

    private string _exportFormularId;
    private int _exportSpaltenAnsicht;
    private FilterCollection? _filter;
    private DateTime _lastExportTimeUtc;
    private ExportTyp _typ;
    private string _verzeichnis;

    #endregion

    #region Constructors

    /// <summary>
    ///
    /// </summary>
    /// <param name="database"></param>
    /// <param name="verzeichnis"></param>
    /// <param name="typ"></param>
    /// <param name="backupinterval">Intervall der Backups in Tagen</param>
    /// <param name="autodelete">Das maximale Alter der Backups in Tagen, nachdem sie gelöscht werden</param>
    public ExportDefinition(DatabaseAbstract database, string verzeichnis, ExportTyp typ, float backupinterval, float autodelete) : this(database) {
        _verzeichnis = verzeichnis;
        _typ = typ;
        _backupInterval = backupinterval;
        _autoDelete = autodelete;
    }

    public ExportDefinition(DatabaseAbstract database, string toParse) : this(database) => Parse(toParse);

    public ExportDefinition(DatabaseAbstract database) {
        Database = database;
        Database.Disposing += Database_Disposing;
        _verzeichnis = string.Empty;
        _typ = ExportTyp.DatenbankOriginalFormat;
        _backupInterval = 1;
        _autoDelete = 30;
        _exportFormularId = string.Empty;
        _exportSpaltenAnsicht = 0;
        Filter = new FilterCollection(Database);
        BereitsExportiert = new ListExt<string>();
        BereitsExportiert.Changed += _BereitsExportiert_ListOrItemChanged;
        _lastExportTimeUtc = new DateTime(1900, 1, 1);
    }

    #endregion

    #region Destructors

    // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    ~ExportDefinition() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(false);
    }

    #endregion

    #region Events

    public event EventHandler? Changed;

    #endregion

    #region Properties

    /// <summary>
    /// Das maximale Alter der Backups in Tagen, nachdem sie gelöscht werden
    /// </summary>
    public float AutoDelete {
        get => _autoDelete;
        set {
            if (_autoDelete == value) { return; }
            _autoDelete = value;
            OnChanged();
        }
    }

    /// <summary>
    /// Intervall der Backups in Tagen
    /// </summary>
    public float BackupInterval {
        get => _backupInterval;
        set {
            if (_backupInterval == value) { return; }
            _backupInterval = value;
            OnChanged();
        }
    }

    public ListExt<string> BereitsExportiert { get; private set; }
    public DatabaseAbstract? Database { get; private set; }

    public string ExportFormularId {
        get => _exportFormularId;
        set {
            if (_exportFormularId == value) { return; }
            _exportFormularId = value;
            OnChanged();
        }
    }

    public int ExportSpaltenAnsicht {
        get => _exportSpaltenAnsicht;
        set {
            if (_exportSpaltenAnsicht == value) { return; }
            _exportSpaltenAnsicht = value;
            OnChanged();
        }
    }

    public FilterCollection? Filter {
        get => _filter;
        set {
            if (_filter == value) { return; }
            if (_filter != null) {
                _filter.Changed -= _Filter_Changed;
            }
            _filter = value;
            if (_filter != null) {
                _filter.Changed += _Filter_Changed;
            }
            OnChanged();
        }
    }

    public bool IsDisposed { get; private set; }
    public bool IsParsing { get; private set; }

    public DateTime LastExportTimeUtc {
        get => _lastExportTimeUtc;
        set {
            if (_lastExportTimeUtc == value) { return; }
            _lastExportTimeUtc = value;
            OnChanged();
        }
    }

    public ExportTyp Typ {
        get => _typ;
        set {
            if (_typ == value) { return; }
            _typ = value;
            OnChanged();
        }
    }

    public string Verzeichnis {
        get => _verzeichnis;
        set {
            if (_verzeichnis == value) { return; }
            _verzeichnis = value;
            OnChanged();
        }
    }

    #endregion

    #region Methods

    public object Clone() => new ExportDefinition(Database, ToString());

    public void DeleteAllBackups() {
        for (var n = 0; n < BereitsExportiert.Count; n++) {
            if (!string.IsNullOrEmpty(BereitsExportiert[n])) {
                var x = BereitsExportiert[n].SplitAndCutBy("|");
                if (FileExists(x[0])) {
                    _ = DeleteFile(x[0], false);
                    BereitsExportiert[n] = string.Empty;
                }
            }
        }
        BereitsExportiert.RemoveNullOrEmpty();
    }

    public void Dispose() {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public string ErrorReason() {
        if (Database?.IsDisposed ?? true) { return "Datenbank verworfen"; }

        if (_typ == ExportTyp.DatenbankOriginalFormat) {
            if (Database is not (BlueDatabase.Database or DatabaseMultiUser)) {
                return "Nur von dateibasierten Datenbanken können nur Exporte im Originalformat stattfinden.";
            }
        }

        if (string.IsNullOrEmpty(Database.DefaultBackupPath()) && _typ != ExportTyp.EinzelnMitFormular) {
            return "Nur von Datenbanken, die auch auf der Festplatte gespeichert sind, kann ein Export stattfinden.";
        }

        if (!string.IsNullOrEmpty(Database.GlobalShowPass) && _typ != ExportTyp.DatenbankOriginalFormat) {
            return "Von passwortgeschützten Datenbanken können nur Exporte im Originalformat stattfinden.";
        }

        if (_typ == ExportTyp.EinzelnMitFormular) {
            if (string.IsNullOrEmpty(_exportFormularId)) {
                return "Layout-Vorlage nicht definiert.";
            }
            if (_exportFormularId.StartsWith("#")) {
                var lNo = Database.Layouts.LayoutIdToIndex(_exportFormularId);
                if (lNo < 0) {
                    return "Layout-Vorlage nicht vorhanden.";
                }
            } else {
                if (!FileExists(_exportFormularId)) {
                    return "Layout-Vorlage existiert nicht.";
                }
            }
        } else {
            if (_backupInterval < 0.00099F) // ALT: Auch bei Bild Export. Sonst wird bei jeder änderung der Durchlauf angestoßen und das hindert die Arbeit ungemein
            {
                return "Intervall muss mindestens 0.001 sein.";
            }
            if (_autoDelete is < 0.00099F or > 10000) {
                return "Automatisch löschen muss zwischen 0.01 und 10000 sein.";
            }
            if (_backupInterval * 1000 < _autoDelete) {
                return "Automatisch löschen darf bei diesem Intervall maximal " + (_backupInterval * 1000) + " sein.";
            }
        }
        return !string.IsNullOrEmpty(_verzeichnis) && !DirectoryExists(_verzeichnis)
            ? "Das Zielverzeichnis existiert nicht."
            : !CanWriteInDirectory(_verzeichnis) ? "Sie besitzen im Zielverzeichnis keine Schreibrechte." : string.Empty;
    }

    public bool IsOk() => string.IsNullOrEmpty(ErrorReason());

    public void OnChanged() => Changed?.Invoke(this, System.EventArgs.Empty);

    public void Parse(string toParse) {
        IsParsing = true;
        BereitsExportiert.ThrowEvents = false;
        var shortener = string.Empty;
        foreach (var pair in toParse.GetAllTags()) {
            switch (pair.Key) {
                case "sho":
                    shortener = pair.Value.FromNonCritical();
                    break;

                case "dest":
                case "destination":// ALT, 02.10.2019
                    _verzeichnis = pair.Value.FromNonCritical();
                    break;

                case "typ":
                case "type":// ALT, 02.10.2019
                    _typ = (ExportTyp)IntParse(pair.Value);
                    break;

                case "itv":
                case "interval":// ALT, 02.10.2019
                    _backupInterval = FloatParse(pair.Value.FromNonCritical());
                    break;

                case "aud":
                case "autodelete":// ALT, 02.10.2019
                    _autoDelete = FloatParse(pair.Value.FromNonCritical());
                    break;

                case "exportformula":
                    // _ExportFormular = pair.Value.FromNonCritical(); ALT, 16.07.2019
                    break;

                case "exid":
                    _exportFormularId = pair.Value.FromNonCritical();
                    break;

                case "exc":
                case "exportcolumnorder": // ALT, 02.10.2019
                    _exportSpaltenAnsicht = IntParse(pair.Value);
                    break;

                case "flt":
                case "filter": // ALT, 02.10.2019
                    Filter = new FilterCollection(Database, pair.Value);
                    break;

                case "exported": // ALT, 02.10.2019
                    BereitsExportiert.AddRange(pair.Value.FromNonCritical().SplitAndCutBy("#"));
                    break;

                case "exp":
                    var tmp = pair.Value.FromNonCritical().SplitAndCutBy("#");
                    BereitsExportiert.Clear();
                    foreach (var thise in tmp) {
                        if (thise.StartsWith("@")) {
                            BereitsExportiert.Add(shortener + thise.TrimStart("@"));
                        } else {
                            BereitsExportiert.Add(thise);
                        }
                    }
                    break;

                case "let":
                case "lastexporttime":
                    _lastExportTimeUtc = DateTimeParse(pair.Value);
                    break;

                default:
                    Develop.DebugPrint(FehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                    break;
            }
        }
        BereitsExportiert.ThrowEvents = true;
        IsParsing = false;
    }

    public string ReadableText() {
        var t = ErrorReason();
        if (!string.IsNullOrEmpty(t)) {
            return "Fehler: " + t;
        }
        switch (_typ) {
            case ExportTyp.DatenbankCSVFormat:
                t = "Gesamte Datenbank als CSV-Datei";
                break;

            case ExportTyp.DatenbankHTMLFormat:
                t = "Gesamte Datenbank als HTML-Datei";
                break;

            case ExportTyp.DatenbankOriginalFormat:
                t = "Sicherheitskopie im Originalformat";
                break;

            case ExportTyp.EinzelnMitFormular:
                t = "Einzeleinträge";
                break;

            default:
                Develop.DebugPrint(_typ);
                return "Unbekannte Aktion";
        }

        if (_backupInterval > 0) {
            t = t + ", alle " + _backupInterval + " Tage";
        } else {
            t += ", wenn sich was geändert hat";
        }

        if (_typ == ExportTyp.EinzelnMitFormular) {
            if (!string.IsNullOrEmpty(_exportFormularId)) {
                t += " mit einem gewählten Formular. Einträge werden immer aktualisiert und gelöschte Einträge auch gelöscht.";
            }
        } else {
            if (_exportSpaltenAnsicht > 0) {
                t += " nur bestimmte Spalten.";
            }
        }

        if (_filter.Count > 0) {
            t += " Nur bestimmte Einträge.";
        }
        if (_autoDelete > 0) {
            t += " Automatische Bereinigung.";
        }
        return t;
    }

    public QuickImage? SymbolForReadableText() {
        if (!IsOk()) { return QuickImage.Get(ImageCode.Kritisch); }
        switch (_typ) {
            case ExportTyp.DatenbankCSVFormat:
                return QuickImage.Get(ImageCode.Excel);

            case ExportTyp.DatenbankHTMLFormat:
                return QuickImage.Get(ImageCode.Globus);

            case ExportTyp.DatenbankOriginalFormat:
                return QuickImage.Get(ImageCode.Häkchen);

            case ExportTyp.EinzelnMitFormular:
                return QuickImage.Get(ImageCode.Stern);

            default:
                Develop.DebugPrint(_typ);
                return QuickImage.Get(ImageCode.Kritisch);
        }
    }

    public override string ToString() {
        try {
            var shortener = GetShortener();
            var result = "{";
            result = result + "sho=" + shortener.ToNonCritical() + ", ";
            result = result + "dest=" + _verzeichnis.ToNonCritical() + ", ";
            result = result + "typ=" + (int)_typ + ", ";
            result = result + "let=" + _lastExportTimeUtc.ToString(Constants.Format_Date5) + ", ";
            result = result + "itv=" + _backupInterval.ToString(CultureInfo.InvariantCulture).ToNonCritical() + ", ";
            if (_typ is ExportTyp.DatenbankCSVFormat or ExportTyp.DatenbankHTMLFormat or ExportTyp.DatenbankOriginalFormat) {
                result = result + "aud=" + _autoDelete.ToString(CultureInfo.InvariantCulture).ToNonCritical() + ", ";
                if (_typ != ExportTyp.DatenbankOriginalFormat) {
                    result = result + "exc=" + _exportSpaltenAnsicht + ", ";
                }
            } else {
                result = result + "exid=" + _exportFormularId.ToNonCritical() + ", ";
            }
            if (_filter.Count > 0) {
                result = result + "flt=" + _filter + ", ";
            }
            if (BereitsExportiert.Count > 0) {
                result += "exp=";
                foreach (var thise in BereitsExportiert) {
                    result = !string.IsNullOrEmpty(shortener) && thise.StartsWith(shortener)
                        ? result + "@" + thise.TrimStart(shortener) + "#"
                        : result + thise + "#";
                }
                result = result.TrimEnd("#") + ", ";
            }
            return result.TrimEnd(", ") + "}";
        } catch {
            return ToString();
        }
    }

    internal bool DeleteOutdatedBackUps(BackgroundWorker worker) {
        var did = false;
        if (!IsOk()) { return false; }
        if (_typ is ExportTyp.DatenbankCSVFormat or ExportTyp.DatenbankHTMLFormat or ExportTyp.DatenbankOriginalFormat) {
            for (var n = 0; n < BereitsExportiert.Count; n++) {
                if (worker != null && worker.CancellationPending) { break; }
                if (Database.IsDisposed) { return false; }
                if (!string.IsNullOrEmpty(BereitsExportiert[n])) {
                    var x = BereitsExportiert[n].SplitAndCutBy("|");
                    if ((float)DateTime.Now.Subtract(DateTimeParse(x[1])).TotalDays > _autoDelete) {
                        if (FileExists(x[0])) { _ = DeleteFile(x[0], false); }
                    }
                    if (!FileExists(x[0])) {
                        BereitsExportiert[n] = string.Empty;
                        did = true;
                    }
                }
            }
        } else {
            // Einträge, die noch vorhanden sind aber veraltet, löschen
            // Dabei ist der Filter egall
            foreach (var thisrow in Database.Row) {
                if (worker != null && worker.CancellationPending) { break; }
                if (Database.IsDisposed) { return false; }
                if (thisrow != null) {
                    if (_filter != null && _filter.Count > 0 && !thisrow.MatchesTo(_filter)) {
                        var tmp = DeleteId(thisrow.Key, worker);
                        if (tmp) { did = true; }
                    }
                }
            }
            // Einträge, die noch vorhanden sind aber der Filter NICHT mehr zutrifft, löschen
            foreach (var thisrow in Database.Row) {
                if (worker != null && worker.CancellationPending) { break; }
                if (Database.IsDisposed) { return false; }
                if (thisrow != null) {
                    if (Database.Cell.GetDateTime(Database.Column.SysRowChangeDate, thisrow).Subtract(_lastExportTimeUtc).TotalSeconds > 0) {
                        var tmp = DeleteId(thisrow.Key, worker);
                        if (tmp) { did = true; }
                    }
                }
            }
            // Gelöschte Einträge der Datenbank auch hier löschen
            // Zusätzlich Einträge löschen, die nicht mehr auf der Festplatte sind.
            for (var n = 0; n < BereitsExportiert.Count; n++) {
                if (worker != null && worker.CancellationPending) { break; }
                if (!string.IsNullOrEmpty(BereitsExportiert[n])) {
                    var x = BereitsExportiert[n].SplitAndCutBy("|");
                    if (x.GetUpperBound(0) > 1 && Database.Row.SearchByKey(LongParse(x[2])) == null) {
                        if (FileExists(x[0])) { _ = DeleteFile(x[0], false); }
                    }
                    if (!FileExists(x[0])) {
                        BereitsExportiert[n] = string.Empty;
                        did = true;
                    }
                }
            }
        }
        if (did) {
            BereitsExportiert.RemoveNullOrEmpty();
        }
        return did;
    }

    internal bool DoBackUp(BackgroundWorker worker) {
        if (!IsOk()) { return false; }

        string savePath;

        if (!string.IsNullOrEmpty(_verzeichnis)) {
            savePath = _verzeichnis.CheckPath();
        } else if (!string.IsNullOrEmpty(Database.AdditionalFilesPfad)) {
            savePath = Database.AdditionalFilesPfad + "Backup\\";
        } else if (!string.IsNullOrEmpty(Database.DefaultBackupPath())) {
            savePath = Database.DefaultBackupPath();
        } else {
            savePath = (System.Windows.Forms.Application.StartupPath + "\\Backup\\").CheckPath();
        }

        if (!DirectoryExists(savePath)) { _ = Directory.CreateDirectory(savePath); }

        var singleFileExport = savePath + Database.TableName.FileNameWithoutSuffix().StarkeVereinfachung(" _-+") + "_" + DateTime.Now.ToString(Constants.Format_Date4);

        List<string> added = new();
        var tim2 = DateTime.UtcNow;
        var tim = tim2.ToString(Constants.Format_Date5);
        try {
            switch (_typ) {
                case ExportTyp.DatenbankOriginalFormat:
                    if (_backupInterval > (float)DateTime.UtcNow.Subtract(_lastExportTimeUtc).TotalDays) { return false; }

                    if (Database is Database DBD) {
                        singleFileExport = TempFile(singleFileExport + ".MDB");
                        if (!FileExists(singleFileExport)) { File.Copy(DBD.Filename, singleFileExport); }
                        added.Add(singleFileExport + "|" + tim);
                    }

                    if (Database is DatabaseMultiUser DBDM) {
                        singleFileExport = TempFile(singleFileExport + ".MDB");
                        if (!FileExists(singleFileExport)) { File.Copy(DBDM.Filename, singleFileExport); }
                        added.Add(singleFileExport + "|" + tim);
                    }

                    break;

                case ExportTyp.DatenbankCSVFormat:
                    if (_backupInterval > (float)DateTime.UtcNow.Subtract(_lastExportTimeUtc).TotalDays) { return false; }
                    singleFileExport = TempFile(singleFileExport + ".CSV");
                    if (!FileExists(singleFileExport)) { WriteAllText(singleFileExport, Database.Export_CSV(FirstRow.ColumnInternalName, _exportSpaltenAnsicht, _filter, null), Constants.Win1252, false); }
                    added.Add(singleFileExport + "|" + tim);
                    break;

                case ExportTyp.DatenbankHTMLFormat:
                    if (_backupInterval > (float)DateTime.UtcNow.Subtract(_lastExportTimeUtc).TotalDays) { return false; }
                    singleFileExport = TempFile(singleFileExport + ".HTML");
                    if (!FileExists(singleFileExport)) { Database.Export_HTML(singleFileExport, _exportSpaltenAnsicht, _filter, null); }
                    added.Add(singleFileExport + "|" + tim);
                    break;

                case ExportTyp.EinzelnMitFormular:
                    foreach (var thisrow in Database.Row) {
                        if (thisrow != null) {
                            if (_filter == null || _filter.Count < 1 || thisrow.MatchesTo(_filter)) {
                                var id = thisrow.Key.ToString();
                                var found = BereitsExportiert.Any(thisstring => thisstring.EndsWith("|" + id));
                                if (!found) {
                                    if (_exportFormularId.StartsWith("#")) {
                                        singleFileExport = TempFile(savePath, thisrow.CellFirstString().StarkeVereinfachung(" "), "PNG");
                                        Export.SaveAsBitmap(thisrow, _exportFormularId, singleFileExport);
                                    } else {
                                        singleFileExport = TempFile(savePath, thisrow.CellFirstString().StarkeVereinfachung(" "), _exportFormularId.FileSuffix());
                                        _ = Export.SaveAs(thisrow, _exportFormularId, singleFileExport);
                                    }
                                    added.Add(singleFileExport + "|" + tim + "|" + thisrow.Key);
                                }
                            }
                        }
                        if (Database?.IsDisposed ?? true) { return false; }
                        if (worker != null && worker.CancellationPending) { break; }
                    }
                    break;

                default:
                    Develop.DebugPrint(_typ);
                    return false;
            }
        } catch {
            //Develop.DebugPrint("Backup konnte nicht erstellt werden:<br>" + SingleFileExport + "<br>" + ex.Message + "<br>" + ToString());
            return false;
        }
        var didAndOk = false;
        foreach (var thisString in added) {
            var x = thisString.SplitAndCutBy("|");
            if (FileExists(x[0])) {
                if (!BereitsExportiert.Contains(thisString)) {
                    BereitsExportiert.Add(thisString);
                    didAndOk = true;
                }
            }
        }
        _lastExportTimeUtc = tim2;
        return didAndOk;
    }

    private void _BereitsExportiert_ListOrItemChanged(object sender, System.EventArgs e) => OnChanged();

    private void _Filter_Changed(object sender, System.EventArgs e) => OnChanged();

    private void Database_Disposing(object sender, System.EventArgs e) => Dispose();

    private bool DeleteId(long id, BackgroundWorker worker) {
        var did = false;
        for (var f = 0; f < BereitsExportiert.Count; f++) {
            if (worker.CancellationPending) { break; }
            if (Database?.IsDisposed ?? true) { return false; }
            if (!string.IsNullOrEmpty(BereitsExportiert[f])) {
                if (BereitsExportiert[f].EndsWith("|" + id)) {
                    var x = BereitsExportiert[f].SplitAndCutBy("|");
                    if (FileExists(x[0])) { _ = DeleteFile(x[0], false); }
                    if (!FileExists(x[0])) {
                        BereitsExportiert[f] = string.Empty;
                        did = true;
                    }
                }
            }
        }
        if (did) {
            BereitsExportiert.RemoveNullOrEmpty();
        }
        return did;
    }

    private void Dispose(bool disposing) {
        if (!IsDisposed) {
            if (disposing) {
                // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
            }
            Database.Disposing -= Database_Disposing;
            Database = null;
            Filter.Dispose();

            BereitsExportiert.Changed -= _BereitsExportiert_ListOrItemChanged;
            BereitsExportiert = new ListExt<string>();
            BereitsExportiert.Dispose();
            IsDisposed = true;
        }
    }

    private string GetShortener() {
        if (BereitsExportiert.Count < 2) { return string.Empty; }
        var ze = 1;
        var last = string.Empty;
        do {
            foreach (var thiss in BereitsExportiert) {
                if (ze > thiss.Length - 2) { return thiss.Substring(0, ze - 1); }
                if (!string.IsNullOrEmpty(last)) {
                    if (thiss.Substring(0, ze) != last) { return thiss.Substring(0, ze - 1); }
                } else {
                    last = thiss.Substring(0, ze);
                }
            }
            ze++;
            last = string.Empty;
        }
        while (true);
    }

    #endregion
}