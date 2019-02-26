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
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;

namespace BlueDatabase
{
    public sealed class FilterItem : IObjectWithDialog, IParseable, ICompareKey, IReadableText, ICanBeEmpty
    {

        #region  Variablen-Deklarationen 


        /// <summary>
        /// Der Edit-Dialog braucht die Datenbank, um mit Texten die Spalte zu suchen.
        /// </summary>
        public readonly Database Database;

        private ColumnItem _Column;
        private enFilterType _FilterType;

        #endregion


        #region  Event-Deklarationen + Delegaten 
        public event EventHandler Changed;
        #endregion


        #region  Construktor + Initialize 


        private void Initialize()
        {
            SearchValue = new List<string>();
            _FilterType = enFilterType.KeinFilter;
        }


        public FilterItem(Database cDatabase, enFilterType FilterType, string SearchValue)
        {

            Database = cDatabase;
            Initialize();
            _FilterType = FilterType;
            this.SearchValue.Add(SearchValue);
        }

        public FilterItem(Database cDatabase, string FilterCode)
        {
            Database = cDatabase;
            Parse(FilterCode);
        }

        public FilterItem(ColumnItem Column, enFilterType FilterType, string SearchValue)
        {

            if (Column == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Columm == null)"); }

            Database = Column.Database;
            Initialize();
            _Column = Column;
            _FilterType = FilterType;
            this.SearchValue.Add(SearchValue);
        }

        public FilterItem(ColumnItem Column, enFilterType FilterType, List<string> SearchValue)
        {

            Database = Column.Database;
            Initialize();
            _Column = Column;
            _FilterType = FilterType;
            if (SearchValue != null && SearchValue.Count > 0)
            {
                this.SearchValue.AddRange(SearchValue);
            }
        }


        #endregion


        #region  Properties 

        public bool IsParsing { get; private set; }

        public ColumnItem Column
        {
            get
            {
                return _Column;
            }
            set
            {
                _Column = value;
                OnChanged();
            }
        }


        public List<string> SearchValue { get; private set; }

        public enFilterType FilterType
        {
            get
            {
                return _FilterType;
            }
            set
            {
                _FilterType = value;
                OnChanged();
            }
        }


        public void OnChanged()
        {
            if (IsParsing) { Develop.DebugPrint(enFehlerArt.Warnung, "Falscher Parsing Zugriff!"); return; }
            Changed?.Invoke(this, System.EventArgs.Empty);
        }

        #endregion



        public override string ToString()
        {

            if (!IsOk()) { return string.Empty; }


            var Result = "{Type=" + Convert.ToInt32(_FilterType);

            if (_Column != null) { Result = Result + ", " + _Column.ParsableColumnKey(); }


            foreach (var t in SearchValue)
            {
                Result = Result + ", Value=" + t.ToNonCritical();
            }

            return Result + "}";
        }


        public void Parse(string ToParse)
        {
            IsParsing = true;
            Initialize();
            foreach (var pair in ToParse.GetAllTags())
            {
                switch (pair.Key)
                {
                    case "identifier":
                        if (pair.Value != "Filter")
                        {
                            Develop.DebugPrint(enFehlerArt.Fehler, "Identifier fehlerhaft: " + pair.Value);
                        }
                        break;
                    case "type":
                        _FilterType = (enFilterType)int.Parse(pair.Value);
                        break;
                    case "column":
                        _Column = Database.Column[pair.Value];
                        break;
                    case "columnkey":
                        _Column = Database.Column.SearchByKey(int.Parse(pair.Value));
                        break;
                    case "value":
                        SearchValue.Add(pair.Value.FromNonCritical());
                        break;
                    default:
                        Develop.DebugPrint(enFehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                        break;
                }
            }


            if (ToParse.Contains(", Value=}") || ToParse.Contains(", Value=,")) { SearchValue.Add(""); }

            IsParsing = false;

        }





        public string CompareKey()
        {
            return ((int)_FilterType).Nummer(10);
        }

        public string ReadableText()
        {


            if (_Column == null) { return "Spezial-Zeilen-Filter"; }
            if (SearchValue == null || SearchValue.Count != 1) { return "Special-Wert-Filter"; }

            if (_Column == Database.Column.SysCorrect())
            {
                if (Convert.ToBoolean(FilterType & enFilterType.Istgleich) && SearchValue[0].FromPlusMinus()) { return "Muss fehlerfrei sein"; }
                if (Convert.ToBoolean(FilterType & enFilterType.Istgleich) && !SearchValue[0].FromPlusMinus()) { return "Muss Fehler haben"; }
            }


            switch (_FilterType)
            {
                case enFilterType.Istgleich:
                case enFilterType.Istgleich_GroﬂKleinEgal:
                case enFilterType.Istgleich_ODER_GroﬂKleinEgal:
                    return "'" + _Column.ReadableText() + "' = " + SearchValue[0];

                case enFilterType.Ungleich_MultiRowIgnorieren:
                case enFilterType.Ungleich_MultiRowIgnorieren_UND_GroﬂKleinEgal:
                case enFilterType.Ungleich_MultiRowIgnorieren_GroﬂKleinEgal:
                    if (string.IsNullOrEmpty(SearchValue[0])) { return "'" + _Column.ReadableText() + "' muss bef¸llt sein"; }
                    return "'" + _Column.ReadableText() + "' <> " + SearchValue[0];


                case enFilterType.Istgleich_GroﬂKleinEgal_MultiRowIgnorieren:
                case enFilterType.Istgleich_MultiRowIgnorieren:
                    if (SearchValue.Count == 1 && string.IsNullOrEmpty(SearchValue[0])) { return "'" + _Column.ReadableText() + "' muss leer sein"; }
                    return "Spezial-Filter";


                case enFilterType.Instr:
                case enFilterType.Instr_GroﬂKleinEgal:
                    return "'" + _Column.ReadableText() + "' beinhaltet den Text '" + SearchValue[0] + "'";

                default:
                    return "Spezial-Filter";

            }
        }

        public QuickImage SymbolForReadableText()
        {
            return null;
        }

        public bool IsNullOrEmpty()
        {
            if (_FilterType == enFilterType.KeinFilter) { return true; }
            return false;
        }



        public object Clone()
        {
            return new FilterItem(Database, ToString());
        }

        public bool IsOk()
        {
            return string.IsNullOrEmpty(ErrorReason());
        }

        public string ErrorReason()
        {
            if (_FilterType == enFilterType.KeinFilter) { return "'Kein Filter' angegeben"; }
            return string.Empty;
        }


    }
}