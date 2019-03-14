#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2019 Christian Peter
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
#endregion



using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using static BlueDatabase.Database;
using BlueDatabase.Enums;
using static BlueBasics.FileOperations;
using static BlueBasics.modAllgemein;

namespace BlueDatabase
{

    //Der Export wird nur Intern verwaltet und gibt keine Ereignisse aus.
    //Wenn mal ein LAyout geändert wird, sind es gleich 100 und mehr AddPenduings mit imensen Daten.

    public class ExportDefinition : IObjectWithDialog, IParseable, IReadableText, ICompareKey, ICheckable
    {


        public readonly Database Database;

        private string _Verzeichnis;
        private enExportTyp _Typ;
        private float _Intervall;
        private float _AutomatischLöschen;
        private string _ExportFormular;
        private int _ExportSpaltenAnsicht;
        public ListExt<string> _BereitsExportiert;
        private DateTime _LastExportTime;


        #region  Event-Deklarationen + Delegaten 

        public event EventHandler Changed;

        #endregion


        #region  Properties 

        public bool IsParsing { get; private set; }

        public string Verzeichnis
        {
            get
            {
                return _Verzeichnis;
            }
            set
            {
                if (_Verzeichnis == value) { return; }
                _Verzeichnis = value;
                OnChanged();
            }
        }



        public enExportTyp Typ
        {
            get
            {
                return _Typ;
            }
            set
            {
                if (_Typ == value) { return; }
                _Typ = value;
                OnChanged();
            }
        }


        public float Intervall
        {
            get
            {
                return _Intervall;
            }
            set
            {
                if (_Intervall == value) { return; }
                _Intervall = value;
                OnChanged();
            }
        }


        public float AutomatischLöschen
        {
            get
            {
                return _AutomatischLöschen;
            }
            set
            {
                if (_AutomatischLöschen == value) { return; }
                _AutomatischLöschen = value;
                OnChanged();
            }
        }

        public string ExportFormular
        {
            get
            {
                return _ExportFormular;
            }
            set
            {
                if (_ExportFormular == value) { return; }
                _ExportFormular = value;
                OnChanged();
            }
        }

        public int ExportSpaltenAnsicht
        {
            get
            {
                return _ExportSpaltenAnsicht;
            }
            set
            {
                if (_ExportSpaltenAnsicht == value) { return; }
                _ExportSpaltenAnsicht = value;
                OnChanged();
            }
        }

        public FilterCollection Filter { get; private set; }


        public List<string> BereitsExportiert
        {
            get
            {
                return _BereitsExportiert;
            }

        }




        public DateTime LastExportTime
        {
            get
            {
                return _LastExportTime;
            }
            set
            {
                _LastExportTime = value;
                OnChanged();
            }
        }

        #endregion


        #region  Construktor + Initialize 


        private void Initialize()
        {
            _Verzeichnis = string.Empty;
            _Typ = enExportTyp.DatenbankOriginalFormat;
            _Intervall = 1;
            _AutomatischLöschen = 30;
            _ExportFormular = string.Empty;
            _ExportSpaltenAnsicht = 0;
            Filter = new FilterCollection(Database);
            Filter.Changed += _Filter_Changed;
            _BereitsExportiert = new ListExt<string>();
            _BereitsExportiert.ListOrItemChanged += _BereitsExportiert_ListOrItemChanged;

            _LastExportTime = new DateTime(1900, 1, 1);
        }

        private void _Filter_Changed(object sender, System.EventArgs e)
        {
            OnChanged();
        }

        private void _BereitsExportiert_ListOrItemChanged(object sender, System.EventArgs e)
        {
            OnChanged();
        }

        public ExportDefinition(Database DB, string Code)
        {
            Database = DB;
            Parse(Code);
        }

        public ExportDefinition(Database DB, string Code, bool DeleteLastExportInfos)
        {
            Database = DB;
            // Initialize()
            Parse(Code);

            if (DeleteLastExportInfos)
            {
                _BereitsExportiert.Clear();
                _LastExportTime = new DateTime(1900, 1, 1);
            }
        }


        public ExportDefinition(Database DB)
        {
            Database = DB;
            Initialize();
        }

        #endregion



        public void OnChanged()
        {
            if (IsParsing) { Develop.DebugPrint(enFehlerArt.Warnung, "Falscher Parsing Zugriff!"); return; }
            Changed?.Invoke(this, System.EventArgs.Empty);
        }

        public void Parse(string ToParse)
        {

            IsParsing = true;
            Initialize();
            _BereitsExportiert.ThrowEvents = false;


            foreach (var pair in ToParse.GetAllTags())
            {
                switch (pair.Key)
                {
                    case "destination":
                        _Verzeichnis = pair.Value.FromNonCritical();
                        break;
                    case "type":
                        _Typ = (enExportTyp)int.Parse(pair.Value);
                        break;
                    case "interval":
                        _Intervall = float.Parse(pair.Value);
                        break;
                    case "autodelete":
                        _AutomatischLöschen = float.Parse(pair.Value);
                        break;
                    case "exportformula":
                        _ExportFormular = pair.Value.FromNonCritical();
                        break;
                    case "exportcolumnorder":
                        _ExportSpaltenAnsicht = int.Parse(pair.Value);
                        break;
                    case "filter":
                        Filter = new FilterCollection(Database, pair.Value);
                        break;
                    case "exported":
                        _BereitsExportiert.AddRange(pair.Value.FromNonCritical().SplitBy("#"));

                        break;
                    case "lastexporttime":
                        _LastExportTime = DateTimeParse(pair.Value);
                        break;
                    default:
                        Develop.DebugPrint(enFehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                        break;
                }
            }

            _BereitsExportiert.ThrowEvents = true;
            IsParsing = false;
        }



        public string CompareKey()
        {
            return ((int)_Typ).Nummer(3) + "|" + _Verzeichnis + "|" + _ExportFormular + "|" + _Intervall + "|" + _AutomatischLöschen;
        }



        public string ReadableText()
        {
            var t = ErrorReason();


            if (!string.IsNullOrEmpty(t))
            {
                return "Fehler: " + t;
            }

            switch (_Typ)
            {
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
                    //   Case Is = enExportTyp.EinzelnAlsHTML : t = "Einzeleinträge als HTML-Datei"
                    break;
                default:
                    Develop.DebugPrint(_Typ);
                    return "Unbekannte Aktion";
            }


            if (_Intervall > 0)
            {
                t = t + ", alle " + _Intervall + " Tage";
            }
            else
            {
                t = t + ", wenn sich was geändert hat";
            }


            if (_Typ == enExportTyp.EinzelnMitFormular)
            {
                if (!string.IsNullOrEmpty(_ExportFormular))
                {
                    t = t + " mit einem gewählten Formular. Einträge werden immer aktualisiert und gelöschte Einträge auch gelöscht.";
                }
            }
            else
            {
                if (_ExportSpaltenAnsicht > 0)
                {
                    t = t + " nur bestimmte Spalten.";
                }

            }


            if (Filter.Count() > 0)
            {
                t = t + " Nur bestimmte Einträge.";
            }


            if (_AutomatischLöschen > 0)
            {
                t = t + " Automatische Bereinigung.";
            }

            return t;
        }

        public QuickImage SymbolForReadableText()
        {
            if (!IsOk()) { return QuickImage.Get(enImageCode.Kritisch); }


            switch (_Typ)
            {
                case enExportTyp.DatenbankCSVFormat:
                    return QuickImage.Get(enImageCode.Excel);
                case enExportTyp.DatenbankHTMLFormat:
                    return QuickImage.Get(enImageCode.Globus);
                case enExportTyp.DatenbankOriginalFormat:
                    return QuickImage.Get(enImageCode.Häkchen);
                case enExportTyp.EinzelnMitFormular:
                    return QuickImage.Get(enImageCode.Stern);
                //     Case Is = enExportTyp.EinzelnAlsHTML : Return QuickImage.[Get](enImageCode.InternetExplorer, 16, "00FF00", "")
                default:
                    Develop.DebugPrint(_Typ);
                    return QuickImage.Get(enImageCode.Kritisch);
            }
        }

        public override string ToString()
        {
            var Result = "{";

            Result = Result + "Destination=" + _Verzeichnis.ToNonCritical() + ", ";
            Result = Result + "Type=" + Convert.ToInt32(_Typ) + ", ";
            Result = Result + "LastExportTime=" + _LastExportTime + ", ";

            Result = Result + "Interval=" + _Intervall + ", ";

            if (_Typ == enExportTyp.DatenbankCSVFormat || _Typ == enExportTyp.DatenbankHTMLFormat || _Typ == enExportTyp.DatenbankOriginalFormat)
            {
                Result = Result + "AutoDelete=" + _AutomatischLöschen + ", ";


                if (_Typ != enExportTyp.DatenbankOriginalFormat)
                {
                    Result = Result + "ExportColumnOrder=" + _ExportSpaltenAnsicht + ", ";
                }

            }
            else
            {
                Result = Result + "ExportFormula=" + _ExportFormular.ToNonCritical() + ", ";
            }

            if (Filter.Count() > 0)
            {
                Result = Result + "Filter=" + Filter + ", ";
            }

            if (_BereitsExportiert.Count > 0)
            {
                Result = Result + "Exported=" + _BereitsExportiert.JoinWith("#").ToNonCritical() + ", ";
            }


            return Result.TrimEnd(", ") + "}";
        }

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



        public void DeleteAllBackups()
        {
            for (var n = 0 ; n < _BereitsExportiert.Count ; n++)
            {
                if (!string.IsNullOrEmpty(_BereitsExportiert[n]))
                {
                    var x = _BereitsExportiert[n].SplitBy("|");


                    if (FileExists(x[0]))
                    {
                        DeleteFile(x[0], false);
                        _BereitsExportiert[n] = string.Empty;
                    }



                }
            }

            _BereitsExportiert.RemoveNullOrEmpty();

        }

        internal bool DeleteOutdatedBackUps(BackgroundWorker worker)
        {
            var Did = false;

            if (!IsOk()) { return false; }

            if (_Typ == enExportTyp.DatenbankCSVFormat || _Typ == enExportTyp.DatenbankHTMLFormat || _Typ == enExportTyp.DatenbankOriginalFormat)
            {
                for (var n = 0 ; n < _BereitsExportiert.Count ; n++)
                {
                    if (worker != null && worker.CancellationPending) { break; }

                    if (!string.IsNullOrEmpty(_BereitsExportiert[n]))
                    {
                        var x = _BereitsExportiert[n].SplitBy("|");
                        if ((float)DateTime.Now.Subtract(DateTimeParse(x[1])).TotalDays > _AutomatischLöschen)
                        {
                            if (FileExists(x[0])) { DeleteFile(x[0], false); }
                        }
                        if (!FileExists(x[0]))
                        {
                            _BereitsExportiert[n] = string.Empty;
                            Did = true;
                        }
                    }
                }
            }
            else
            {


                // Einträge, die noch vorhanden sind aber veraltet, löschen
                // Dabei ist der Filter egall
                foreach (var Thisrow in Database.Row)
                {
                    if (worker != null && worker.CancellationPending) { break; }
                    if (Thisrow != null)
                    {

                        if (Filter != null && Filter.Count() > 0 && !Thisrow.MatchesTo(Filter))
                        {
                            var tmp = DeleteId(Thisrow.Key, worker);
                            if (tmp) { Did = true; }
                        }

                    }
                }


                // Einträge, die noch vorhanden sind aber der Filter NICHT mehr zutrifft, löschen
                foreach (var Thisrow in Database.Row)
                {
                    if (worker != null && worker.CancellationPending) { break; }
                    if (Thisrow != null)
                    {
                        if (Database.Cell.GetDate(Database.Column.SysRowChangeDate, Thisrow).Subtract(_LastExportTime).TotalSeconds > 0)
                        {
                            var tmp = DeleteId(Thisrow.Key, worker);
                            if (tmp) { Did = true; }
                        }
                    }
                }


                // Gelöschte Einträge der Datenbank auch hier löschen
                // Zusätzlich Einträge löschen, die nicht mehr auf der Festplatte sind.
                for (var n = 0 ; n < _BereitsExportiert.Count ; n++)
                {
                    if (worker != null && worker.CancellationPending) { break; }
                    if (!string.IsNullOrEmpty(_BereitsExportiert[n]))
                    {
                        var x = _BereitsExportiert[n].SplitBy("|");
                        if (Database.Row.SearchByKey(int.Parse(x[2])) == null)
                        {
                            if (FileExists(x[0])) { DeleteFile(x[0], false); }
                        }


                        if (!FileExists(x[0]))
                        {
                            _BereitsExportiert[n] = string.Empty;
                            Did = true;
                        }


                    }
                }

            }

            if (Did) { _BereitsExportiert.RemoveNullOrEmpty(); }
            return Did;
        }


        public bool IsOk()
        {
            return string.IsNullOrEmpty(ErrorReason());
        }

        public string ErrorReason()
        {

            if (string.IsNullOrEmpty(Database.Filename))
            {
                return "Nur von Datenbanken, die auch auf der Festplatte gespeichert sind, kann ein Export stattfinden.";
            }
            if (!string.IsNullOrEmpty(Database.GlobalShowPass) && _Typ != enExportTyp.DatenbankOriginalFormat)
            {
                return "Es können von passwortgeschützten Datenbanken nur Exporte im Originalformat stattfinden.";
            }


            if (_Typ == enExportTyp.EinzelnMitFormular)
            {
                if (string.IsNullOrEmpty(_ExportFormular))
                {
                    return "Layout-Vorlage nicht definiert.";
                }


                if (_ExportFormular.IsNumeral())
                {
                    var c = int.Parse(_ExportFormular);

                    if (c < 0)
                    {
                        return "Layout-Vorlage nicht gewählt.";
                    }
                    if (c > Database.Layouts.Count - 1)
                    {
                        return "Layout-Vorlage nicht vorhanden.";
                    }
                }
                else
                {
                    if (!FileExists(_ExportFormular))
                    {
                        return "Layout-Vorlage existiert nicht.";
                    }
                }

            }
            else
            {
                if (_Intervall < 0.00099F) // ALT: Auch bei Bild Export. Sonst wird bei jeder änderung der Durchlauf angestoßen und das hindert die Arbeit ungemein
                {
                    return "Intervall muss mindestens 0.001 sein.";
                }
                if (_AutomatischLöschen < 0.00099F || _AutomatischLöschen > 10000)
                {
                    return "Automatisch löschen muss zwischen 0.01 und 10000 sein.";
                }
                if (_Intervall * 1000 < _AutomatischLöschen)
                {
                    return "Automatisch löschen darf bei diesem Intervall maximal " + _Intervall * 1000 + " sein.";
                }
            }

            if (!string.IsNullOrEmpty(_Verzeichnis) && !PathExists(_Verzeichnis))
            {
                return "Das Zielverzeichnis existiert nicht.";
            }



            return string.Empty;
        }


        //#endregion

        internal bool DoBackUp(BackgroundWorker worker, GenerateLayout_Internal _GenerateLayout)
        {

            if (!IsOk()) { return false; }


            string SavePath = null;

            if (!string.IsNullOrEmpty(_Verzeichnis))
            {
                SavePath = _Verzeichnis.CheckPath();
            }
            else
            {
                if (!string.IsNullOrEmpty(Database.Filename))
                {
                    SavePath = Database.Filename.FilePath() + "Backup\\";
                }
                else
                {
                    SavePath = (System.Windows.Forms.Application.StartupPath + "\\Backup\\").CheckPath();
                }


                if (!PathExists(SavePath)) { Directory.CreateDirectory(SavePath); }

            }

            var SingleFileExport = SavePath + Database.Filename.FileNameWithoutSuffix() + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");


            var Added = new List<string>();
            var tim = DateTime.Now;

            try
            {
                switch (_Typ)
                {
                    case enExportTyp.DatenbankOriginalFormat:
                        if (_Intervall > (float)DateTime.Now.Subtract(_LastExportTime).TotalDays) { return false; }
                        SingleFileExport = TempFile(SingleFileExport + ".MDB");
                        if (!FileExists(SingleFileExport)) { File.Copy(Database.Filename, SingleFileExport); }
                        Added.Add(SingleFileExport + "|" + tim);

                        break;

                    case enExportTyp.DatenbankCSVFormat:
                        if (_Intervall > (float)DateTime.Now.Subtract(_LastExportTime).TotalDays) { return false; }
                        SingleFileExport = TempFile(SingleFileExport + ".CSV");
                        if (!FileExists(SingleFileExport)) { modAllgemein.SaveToDiskx(SingleFileExport, Database.Export_CSV(enFirstRow.ColumnInternalName, _ExportSpaltenAnsicht, Filter), false); }
                        Added.Add(SingleFileExport + "|" + tim);
                        break;

                    case enExportTyp.DatenbankHTMLFormat:
                        if (_Intervall > (float)DateTime.Now.Subtract(_LastExportTime).TotalDays) { return false; }
                        SingleFileExport = TempFile(SingleFileExport + ".HTML");
                        if (!FileExists(SingleFileExport)) { Database.Export_HTML(SingleFileExport, _ExportSpaltenAnsicht, Filter); }
                        Added.Add(SingleFileExport + "|" + tim);
                        break;

                    case enExportTyp.EinzelnMitFormular:
                        foreach (var Thisrow in Database.Row)
                        {
                            if (Thisrow != null)
                            {
                                if (Filter == null || Filter.Count() < 1 || Thisrow.MatchesTo(Filter))
                                {

                                    var Id = Thisrow.Key.ToString();
                                    var Found = false;
                                    foreach (var thisstring in _BereitsExportiert)
                                    {
                                        if (thisstring.EndsWith("|" + Id))
                                        {
                                            Found = true;
                                            break;
                                        }

                                    }

                                    if (!Found)
                                    {
                                        if (_ExportFormular.IsLong())
                                        {
                                            SingleFileExport = TempFile(SavePath, Thisrow.CellFirstString(), "PNG");
                                            Export.SaveAsBitmap(Thisrow, int.Parse(_ExportFormular), SingleFileExport, _GenerateLayout);
                                        }
                                        else
                                        {
                                            SingleFileExport = TempFile(SavePath, Thisrow.CellFirstString(), _ExportFormular.FileSuffix());
                                            Export.SaveAs(Thisrow, _ExportFormular, SingleFileExport);
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
            }
            catch (Exception ex)
            {
                Develop.DebugPrint("Backup konnte nicht erstellt werden:<br>" + SingleFileExport + "<br>" + ex.Message + "<br>" + ToString());
                return false;
            }


            var DidAndOk = false;


            foreach (var ThisString in Added)
            {
                var x = ThisString.SplitBy("|");

                if (FileExists(x[0]))
                {
                    if (!_BereitsExportiert.Contains(ThisString))
                    {
                        _BereitsExportiert.Add(ThisString);
                        DidAndOk = true;
                    }
                }
                else
                {
                    Develop.DebugPrint("Backup verschwunden:<br>" + ThisString + "<br><br>" + ToString());
                }
            }

            _LastExportTime = tim;
            return DidAndOk;
        }

        private bool DeleteId(long Id, BackgroundWorker Worker)
        {

            var Did = false;

            for (var f = 0 ; f < _BereitsExportiert.Count ; f++)
            {
                if (Worker.CancellationPending) { break; }

                if (!string.IsNullOrEmpty(_BereitsExportiert[f]))
                {
                    if (_BereitsExportiert[f].EndsWith("|" + Id))
                    {
                        var x = _BereitsExportiert[f].SplitBy("|");
                        if (FileExists(x[0])) { DeleteFile(x[0], false); }
                        if (!FileExists(x[0]))
                        {
                            _BereitsExportiert[f] = string.Empty;
                            Did = true;
                        }
                    }
                }
            }


            if (Did) { _BereitsExportiert.RemoveNullOrEmpty(); }
            return Did;
        }


        public object Clone()
        {
            return new ExportDefinition(Database, ToString());
        }





    }
}
