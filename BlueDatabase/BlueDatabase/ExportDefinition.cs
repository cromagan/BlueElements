// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using static BlueBasics.Converter;
using static BlueBasics.FileOperations;

namespace BlueDatabase {

    public class ExportDefinition : IParseable, IReadableTextWithChanging, ICompareKey, ICheckable, IDisposable {

        #region Fields

        /// <summary>
        /// Das maximale Alter der Backups in Tagen, nachdem sie gelöscht werden
        /// </summary>
        private float _AutoDelete;

        /// <summary>
        /// Intervall der Backups in Tagen
        /// </summary>
        private float _BackupInterval;

        private string _ExportFormularID;
        private int _ExportSpaltenAnsicht;
        private FilterCollection _Filter;
        private DateTime _LastExportTimeUTC;
        private enExportTyp _Typ;
        private string _Verzeichnis;
        private bool disposedValue;

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
        public ExportDefinition(Database database, string verzeichnis, enExportTyp typ, float backupinterval, float autodelete) : this(database) {
            _Verzeichnis = verzeichnis;
            _Typ = typ;
            _BackupInterval = backupinterval;
            _AutoDelete = autodelete;
        }

        public ExportDefinition(Database database, string toParse) : this(database) => Parse(toParse);

        public ExportDefinition(Database database) {
            Database = database;
            Database.Disposing += Database_Disposing;
            _Verzeichnis = string.Empty;
            _Typ = enExportTyp.DatenbankOriginalFormat;
            _BackupInterval = 1;
            _AutoDelete = 30;
            _ExportFormularID = string.Empty;
            _ExportSpaltenAnsicht = 0;
            Filter = new FilterCollection(Database);
            BereitsExportiert = new ListExt<string>();
            BereitsExportiert.Changed += _BereitsExportiert_ListOrItemChanged;
            _LastExportTimeUTC = new DateTime(1900, 1, 1);
        }

        #endregion

        #region Destructors

        // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
        ~ExportDefinition() {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: false);
        }

        #endregion

        #region Events

        public event EventHandler Changed;

        #endregion

        #region Properties

        /// <summary>
        /// Das maximale Alter der Backups in Tagen, nachdem sie gelöscht werden
        /// </summary>
        public float AutoDelete {
            get => _AutoDelete;
            set {
                if (_AutoDelete == value) { return; }
                _AutoDelete = value;
                OnChanged();
            }
        }

        /// <summary>
        /// Intervall der Backups in Tagen
        /// </summary>
        public float BackupInterval {
            get => _BackupInterval;
            set {
                if (_BackupInterval == value) { return; }
                _BackupInterval = value;
                OnChanged();
            }
        }

        public ListExt<string> BereitsExportiert { get; private set; }
        public Database Database { get; private set; }

        public string ExportFormularID {
            get => _ExportFormularID;
            set {
                if (_ExportFormularID == value) { return; }
                _ExportFormularID = value;
                OnChanged();
            }
        }

        public int ExportSpaltenAnsicht {
            get => _ExportSpaltenAnsicht;
            set {
                if (_ExportSpaltenAnsicht == value) { return; }
                _ExportSpaltenAnsicht = value;
                OnChanged();
            }
        }

        public FilterCollection Filter {
            get => _Filter;
            set {
                if (_Filter == value) { return; }
                if (_Filter != null) {
                    _Filter.Changed -= _Filter_Changed;
                }
                _Filter = value;
                if (_Filter != null) {
                    _Filter.Changed += _Filter_Changed;
                }
                OnChanged();
            }
        }

        public bool IsParsing { get; private set; }

        public DateTime LastExportTimeUTC {
            get => _LastExportTimeUTC;
            set {
                if (_LastExportTimeUTC == value) { return; }
                _LastExportTimeUTC = value;
                OnChanged();
            }
        }

        public enExportTyp Typ {
            get => _Typ;
            set {
                if (_Typ == value) { return; }
                _Typ = value;
                OnChanged();
            }
        }

        public string Verzeichnis {
            get => _Verzeichnis;
            set {
                if (_Verzeichnis == value) { return; }
                _Verzeichnis = value;
                OnChanged();
            }
        }

        #endregion

        #region Methods

        public object Clone() => new ExportDefinition(Database, ToString());

        public string CompareKey() => ((int)_Typ).ToString(Constants.Format_Integer3) + "|" + _Verzeichnis + "|" + _ExportFormularID + "|" + _BackupInterval + "|" + _AutoDelete;

        //#region IDisposable Support
        //// IDisposable
        //protected virtual void Dispose(bool disposing)
        //{
        //    if (!disposedValue)
        //    {
        //        if (disposing)
        //        {
        //            //disposedValue = False
        //            _Verzeichnis = null;
        //            _Typ = 0;
        //            _Intervall = 0;
        //            _AutomatischLöschen = 0;
        //            _ExportFormular = null;
        //            _ExportSpaltenAnsicht = 0;
        //            _Filter.Dispose();
        //            _BereitsExportiert = null;
        //            _LastExportTime = default(DateTime);
        //            // TODO: verwalteten Zustand (verwaltete Objekte) entsorgen.
        //        }
        //        // TODO: nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalize() weiter unten überschreiben.
        //        // TODO: große Felder auf Null setzen.
        //    }
        //    disposedValue = true;
        //}
        //// TODO: Finalize() nur überschreiben, wenn Dispose(disposing As Boolean) weiter oben Code zur Bereinigung nicht verwalteter Ressourcen enthält.
        ////Protected Overrides Sub Finalize()
        ////    ' Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(disposing As Boolean) weiter oben ein.
        ////    Dispose(False)
        ////    MyBase.Finalize()
        ////End Sub
        //// Dieser Code wird von Visual Basic hinzugefügt, um das Dispose-Muster richtig zu implementieren.
        //public void Dispose()
        //{
        //    // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(disposing As Boolean) weiter oben ein.
        //    Dispose(true);
        //    // TODO: Auskommentierung der folgenden Zeile aufheben, wenn Finalize() oben überschrieben wird.
        //    // GC.SuppressFinalize(Me)
        //}
        public void DeleteAllBackups() {
            for (var n = 0; n < BereitsExportiert.Count; n++) {
                if (!string.IsNullOrEmpty(BereitsExportiert[n])) {
                    var x = BereitsExportiert[n].SplitAndCutBy("|");
                    if (FileExists(x[0])) {
                        DeleteFile(x[0], false);
                        BereitsExportiert[n] = string.Empty;
                    }
                }
            }
            BereitsExportiert.RemoveNullOrEmpty();
        }

        public void Dispose() {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public string ErrorReason() {
            if (string.IsNullOrEmpty(Database.Filename)) {
                return "Nur von Datenbanken, die auch auf der Festplatte gespeichert sind, kann ein Export stattfinden.";
            }

            if (!string.IsNullOrEmpty(Database.GlobalShowPass) && _Typ != enExportTyp.DatenbankOriginalFormat) {
                return "Von passwortgeschützten Datenbanken können nur Exporte im Originalformat stattfinden.";
            }

            if (_Typ == enExportTyp.EinzelnMitFormular) {
                if (string.IsNullOrEmpty(_ExportFormularID)) {
                    return "Layout-Vorlage nicht definiert.";
                }
                if (_ExportFormularID.StartsWith("#")) {
                    var LNo = Database.Layouts.LayoutIDToIndex(_ExportFormularID);
                    if (LNo < 0) {
                        return "Layout-Vorlage nicht vorhanden.";
                    }
                } else {
                    if (!FileExists(_ExportFormularID)) {
                        return "Layout-Vorlage existiert nicht.";
                    }
                }
            } else {
                if (_BackupInterval < 0.00099F) // ALT: Auch bei Bild Export. Sonst wird bei jeder änderung der Durchlauf angestoßen und das hindert die Arbeit ungemein
                {
                    return "Intervall muss mindestens 0.001 sein.";
                }
                if (_AutoDelete is < 0.00099F or > 10000) {
                    return "Automatisch löschen muss zwischen 0.01 und 10000 sein.";
                }
                if (_BackupInterval * 1000 < _AutoDelete) {
                    return "Automatisch löschen darf bei diesem Intervall maximal " + (_BackupInterval * 1000) + " sein.";
                }
            }
            return !string.IsNullOrEmpty(_Verzeichnis) && !PathExists(_Verzeichnis)
                ? "Das Zielverzeichnis existiert nicht."
                : !CanWriteInDirectory(_Verzeichnis) ? "Sie besitzen im Zielverzeichnis keine Schreibrechte." : string.Empty;
        }

        public bool IsOk() => string.IsNullOrEmpty(ErrorReason());

        public void OnChanged() => Changed?.Invoke(this, System.EventArgs.Empty);

        public void Parse(string ToParse) {
            IsParsing = true;
            BereitsExportiert.ThrowEvents = false;
            var shortener = string.Empty;
            foreach (var pair in ToParse.GetAllTags()) {
                switch (pair.Key) {
                    case "sho":
                        shortener = pair.Value.FromNonCritical();
                        break;

                    case "dest":
                    case "destination":// ALT, 02.10.2019
                        _Verzeichnis = pair.Value.FromNonCritical();
                        break;

                    case "typ":
                    case "type":// ALT, 02.10.2019
                        _Typ = (enExportTyp)int.Parse(pair.Value);
                        break;

                    case "itv":
                    case "interval":// ALT, 02.10.2019
                        _BackupInterval = float.Parse(pair.Value.FromNonCritical());
                        break;

                    case "aud":
                    case "autodelete":// ALT, 02.10.2019
                        _AutoDelete = float.Parse(pair.Value.FromNonCritical());
                        break;

                    case "exportformula":
                        // _ExportFormular = pair.Value.FromNonCritical(); ALT, 16.07.2019
                        break;

                    case "exid":
                        _ExportFormularID = pair.Value.FromNonCritical();
                        break;

                    case "exc":
                    case "exportcolumnorder": // ALT, 02.10.2019
                        _ExportSpaltenAnsicht = int.Parse(pair.Value);
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
                        _LastExportTimeUTC = DateTimeParse(pair.Value);
                        break;

                    default:
                        Develop.DebugPrint(enFehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
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
            switch (_Typ) {
                case enExportTyp.DatenbankCSVFormat:
                    t = "Gesamte Datenbank als CSV-Datei";
                    break;

                case enExportTyp.DatenbankHTMLFormat:
                    t = "Gesamte Datenbank als HTML-Datei";
                    break;

                case enExportTyp.DatenbankOriginalFormat:
                    t = "Sicherheitskopie im Originalformat";
                    break;

                case enExportTyp.EinzelnMitFormular:
                    t = "Einzeleinträge";
                    break;

                default:
                    Develop.DebugPrint(_Typ);
                    return "Unbekannte Aktion";
            }

            if (_BackupInterval > 0) {
                t = t + ", alle " + _BackupInterval + " Tage";
            } else {
                t += ", wenn sich was geändert hat";
            }

            if (_Typ == enExportTyp.EinzelnMitFormular) {
                if (!string.IsNullOrEmpty(_ExportFormularID)) {
                    t += " mit einem gewählten Formular. Einträge werden immer aktualisiert und gelöschte Einträge auch gelöscht.";
                }
            } else {
                if (_ExportSpaltenAnsicht > 0) {
                    t += " nur bestimmte Spalten.";
                }
            }

            if (_Filter.Count > 0) {
                t += " Nur bestimmte Einträge.";
            }
            if (_AutoDelete > 0) {
                t += " Automatische Bereinigung.";
            }
            return t;
        }

        public QuickImage SymbolForReadableText() {
            if (!IsOk()) { return QuickImage.Get(enImageCode.Kritisch); }
            switch (_Typ) {
                case enExportTyp.DatenbankCSVFormat:
                    return QuickImage.Get(enImageCode.Excel);

                case enExportTyp.DatenbankHTMLFormat:
                    return QuickImage.Get(enImageCode.Globus);

                case enExportTyp.DatenbankOriginalFormat:
                    return QuickImage.Get(enImageCode.Häkchen);

                case enExportTyp.EinzelnMitFormular:
                    return QuickImage.Get(enImageCode.Stern);

                default:
                    Develop.DebugPrint(_Typ);
                    return QuickImage.Get(enImageCode.Kritisch);
            }
        }

        public override string ToString() {
            try {
                var shortener = GetShortener();
                var Result = "{";
                Result = Result + "sho=" + shortener.ToNonCritical() + ", ";
                Result = Result + "dest=" + _Verzeichnis.ToNonCritical() + ", ";
                Result = Result + "typ=" + (int)_Typ + ", ";
                Result = Result + "let=" + _LastExportTimeUTC.ToString(Constants.Format_Date5) + ", ";
                Result = Result + "itv=" + _BackupInterval.ToString().ToNonCritical() + ", ";
                if (_Typ is enExportTyp.DatenbankCSVFormat or enExportTyp.DatenbankHTMLFormat or enExportTyp.DatenbankOriginalFormat) {
                    Result = Result + "aud=" + _AutoDelete.ToString().ToNonCritical() + ", ";
                    if (_Typ != enExportTyp.DatenbankOriginalFormat) {
                        Result = Result + "exc=" + _ExportSpaltenAnsicht + ", ";
                    }
                } else {
                    Result = Result + "exid=" + _ExportFormularID.ToNonCritical() + ", ";
                }
                if (_Filter.Count > 0) {
                    Result = Result + "flt=" + _Filter.ToString() + ", ";
                }
                if (BereitsExportiert.Count > 0) {
                    Result += "exp=";
                    foreach (var thise in BereitsExportiert) {
                        Result = !string.IsNullOrEmpty(shortener) && thise.StartsWith(shortener)
                            ? Result + "@" + thise.TrimStart(shortener) + "#"
                            : Result + thise + "#";
                    }
                    Result = Result.TrimEnd("#") + ", ";
                }
                return Result.TrimEnd(", ") + "}";
            } catch {
                return ToString();
            }
        }

        internal bool DeleteOutdatedBackUps(BackgroundWorker worker) {
            var Did = false;
            if (!IsOk()) { return false; }
            if (_Typ is enExportTyp.DatenbankCSVFormat or enExportTyp.DatenbankHTMLFormat or enExportTyp.DatenbankOriginalFormat) {
                for (var n = 0; n < BereitsExportiert.Count; n++) {
                    if (worker != null && worker.CancellationPending) { break; }
                    if (!string.IsNullOrEmpty(BereitsExportiert[n])) {
                        var x = BereitsExportiert[n].SplitAndCutBy("|");
                        if ((float)DateTime.Now.Subtract(DateTimeParse(x[1])).TotalDays > _AutoDelete) {
                            if (FileExists(x[0])) { DeleteFile(x[0], false); }
                        }
                        if (!FileExists(x[0])) {
                            BereitsExportiert[n] = string.Empty;
                            Did = true;
                        }
                    }
                }
            } else {
                // Einträge, die noch vorhanden sind aber veraltet, löschen
                // Dabei ist der Filter egall
                foreach (var Thisrow in Database.Row) {
                    if (worker != null && worker.CancellationPending) { break; }
                    if (Thisrow != null) {
                        if (_Filter != null && _Filter.Count > 0 && !Thisrow.MatchesTo(_Filter)) {
                            var tmp = DeleteId(Thisrow.Key, worker);
                            if (tmp) { Did = true; }
                        }
                    }
                }
                // Einträge, die noch vorhanden sind aber der Filter NICHT mehr zutrifft, löschen
                foreach (var Thisrow in Database.Row) {
                    if (worker != null && worker.CancellationPending) { break; }
                    if (Thisrow != null) {
                        if (Database.Cell.GetDateTime(Database.Column.SysRowChangeDate, Thisrow).Subtract(_LastExportTimeUTC).TotalSeconds > 0) {
                            var tmp = DeleteId(Thisrow.Key, worker);
                            if (tmp) { Did = true; }
                        }
                    }
                }
                // Gelöschte Einträge der Datenbank auch hier löschen
                // Zusätzlich Einträge löschen, die nicht mehr auf der Festplatte sind.
                for (var n = 0; n < BereitsExportiert.Count; n++) {
                    if (worker != null && worker.CancellationPending) { break; }
                    if (!string.IsNullOrEmpty(BereitsExportiert[n])) {
                        var x = BereitsExportiert[n].SplitAndCutBy("|");
                        if (x.GetUpperBound(0) > 1 && Database.Row.SearchByKey(int.Parse(x[2])) == null) {
                            if (FileExists(x[0])) { DeleteFile(x[0], false); }
                        }
                        if (!FileExists(x[0])) {
                            BereitsExportiert[n] = string.Empty;
                            Did = true;
                        }
                    }
                }
            }
            if (Did) {
                BereitsExportiert.RemoveNullOrEmpty();
            }
            return Did;
        }

        internal bool DoBackUp(BackgroundWorker worker) {
            if (!IsOk()) { return false; }
            string SavePath;
            if (!string.IsNullOrEmpty(_Verzeichnis)) {
                SavePath = _Verzeichnis.CheckPath();
            } else {
                SavePath = !string.IsNullOrEmpty(Database.Filename)
                    ? Database.Filename.FilePath() + "Backup\\"
                    : (System.Windows.Forms.Application.StartupPath + "\\Backup\\").CheckPath();
                if (!PathExists(SavePath)) { Directory.CreateDirectory(SavePath); }
            }
            var SingleFileExport = SavePath + Database.Filename.FileNameWithoutSuffix().StarkeVereinfachung(" _-+") + "_" + DateTime.Now.ToString(Constants.Format_Date4);
            List<string> Added = new();
            var tim2 = DateTime.UtcNow;
            var tim = tim2.ToString(Constants.Format_Date5);
            try {
                switch (_Typ) {
                    case enExportTyp.DatenbankOriginalFormat:
                        if (_BackupInterval > (float)DateTime.UtcNow.Subtract(_LastExportTimeUTC).TotalDays) { return false; }
                        SingleFileExport = TempFile(SingleFileExport + ".MDB");
                        if (!FileExists(SingleFileExport)) { File.Copy(Database.Filename, SingleFileExport); }
                        Added.Add(SingleFileExport + "|" + tim);
                        break;

                    case enExportTyp.DatenbankCSVFormat:
                        if (_BackupInterval > (float)DateTime.UtcNow.Subtract(_LastExportTimeUTC).TotalDays) { return false; }
                        SingleFileExport = TempFile(SingleFileExport + ".CSV");
                        if (!FileExists(SingleFileExport)) { WriteAllText(SingleFileExport, Database.Export_CSV(enFirstRow.ColumnInternalName, _ExportSpaltenAnsicht, _Filter, null), Constants.Win1252, false); }
                        Added.Add(SingleFileExport + "|" + tim);
                        break;

                    case enExportTyp.DatenbankHTMLFormat:
                        if (_BackupInterval > (float)DateTime.UtcNow.Subtract(_LastExportTimeUTC).TotalDays) { return false; }
                        SingleFileExport = TempFile(SingleFileExport + ".HTML");
                        if (!FileExists(SingleFileExport)) { Database.Export_HTML(SingleFileExport, _ExportSpaltenAnsicht, _Filter, null); }
                        Added.Add(SingleFileExport + "|" + tim);
                        break;

                    case enExportTyp.EinzelnMitFormular:
                        foreach (var Thisrow in Database.Row) {
                            if (Thisrow != null) {
                                if (_Filter == null || _Filter.Count < 1 || Thisrow.MatchesTo(_Filter)) {
                                    var Id = Thisrow.Key.ToString();
                                    var Found = false;
                                    foreach (var thisstring in BereitsExportiert) {
                                        if (thisstring.EndsWith("|" + Id)) {
                                            Found = true;
                                            break;
                                        }
                                    }
                                    if (!Found) {
                                        if (_ExportFormularID.StartsWith("#")) {
                                            SingleFileExport = TempFile(SavePath, Thisrow.CellFirstString().StarkeVereinfachung(" "), "PNG");
                                            Export.SaveAsBitmap(Thisrow, _ExportFormularID, SingleFileExport);
                                        } else {
                                            SingleFileExport = TempFile(SavePath, Thisrow.CellFirstString().StarkeVereinfachung(" "), _ExportFormularID.FileSuffix());
                                            Export.SaveAs(Thisrow, _ExportFormularID, SingleFileExport);
                                        }
                                        Added.Add(SingleFileExport + "|" + tim + "|" + Thisrow.Key);
                                    }
                                }
                            }
                            if (worker != null && worker.CancellationPending) { break; }
                        }
                        break;

                    default:
                        Develop.DebugPrint(_Typ);
                        return false;
                }
            } catch {
                //Develop.DebugPrint("Backup konnte nicht erstellt werden:<br>" + SingleFileExport + "<br>" + ex.Message + "<br>" + ToString());
                return false;
            }
            var DidAndOk = false;
            foreach (var ThisString in Added) {
                var x = ThisString.SplitAndCutBy("|");
                if (FileExists(x[0])) {
                    if (!BereitsExportiert.Contains(ThisString)) {
                        BereitsExportiert.Add(ThisString);
                        DidAndOk = true;
                    }
                }
            }
            _LastExportTimeUTC = tim2;
            return DidAndOk;
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
                }
                Database.Disposing -= Database_Disposing;
                Database = null;
                Filter.Dispose();

                BereitsExportiert.Changed -= _BereitsExportiert_ListOrItemChanged;
                BereitsExportiert = new ListExt<string>();
                BereitsExportiert.Dispose();
                disposedValue = true;
            }
        }

        private void _BereitsExportiert_ListOrItemChanged(object sender, System.EventArgs e) => OnChanged();

        private void _Filter_Changed(object sender, System.EventArgs e) => OnChanged();

        private void Database_Disposing(object sender, System.EventArgs e) => Dispose();

        private bool DeleteId(long Id, BackgroundWorker Worker) {
            var Did = false;
            for (var f = 0; f < BereitsExportiert.Count; f++) {
                if (Worker.CancellationPending) { break; }
                if (!string.IsNullOrEmpty(BereitsExportiert[f])) {
                    if (BereitsExportiert[f].EndsWith("|" + Id)) {
                        var x = BereitsExportiert[f].SplitAndCutBy("|");
                        if (FileExists(x[0])) { DeleteFile(x[0], false); }
                        if (!FileExists(x[0])) {
                            BereitsExportiert[f] = string.Empty;
                            Did = true;
                        }
                    }
                }
            }
            if (Did) {
                BereitsExportiert.RemoveNullOrEmpty();
            }
            return Did;
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
}