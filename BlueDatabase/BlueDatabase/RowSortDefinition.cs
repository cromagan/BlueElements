using System;
using System.Collections.Generic;
using BlueBasics;
using BlueBasics.Interfaces;
using BlueBasics.Enums;

namespace BlueDatabase
    {
        ///Imports Microsoft.VisualBasic

        public sealed class RowSortDefinition : IParseable
        {
            #region  Variablen-Deklarationen 

            public Database Database;
            private readonly ListExt<ColumnItem> _Columns = new ListExt<ColumnItem>();

            #endregion


            #region  Event-Deklarationen + Delegaten 
            public event EventHandler Changed;
            #endregion


            #region  Construktor + Initialize 

            private void Initialize()
            {
                Reverse = false;
                _Columns.Clear();
            }

            public RowSortDefinition(Database cDatabase, string Code)
            {
                Database = cDatabase;
                Parse(Code);
            }

            public RowSortDefinition(Database cDatabase, string ColumnName, bool Reverse)
            {
                Database = cDatabase;
                Initialize();

                this.Reverse = Reverse;
                SetColumn(new[] { ColumnName });
            }

            public RowSortDefinition(Database cDatabase, string[] ColumnNames, bool Reverse)
            {
                Initialize();
                Database = cDatabase;
                this.Reverse = Reverse;
                SetColumn(ColumnNames);
            }

            #endregion


            #region  Properties 
            public bool IsParsing { get; private set; }

            public bool Reverse { get; private set; }

            public List<ColumnItem> Columns
            {
                get
                {
                    return _Columns;
                }
            }

            #endregion




            public override string ToString()
            {

                var Result = "{";

                if (Reverse)
                {
                    Result = Result + "Direction=Z-A";
                }
                else
                {
                    Result = Result + "Direction=A-Z";
                }

                if (_Columns != null)
                {
                    foreach (var ThisColumn in _Columns)
                    {
                        if (ThisColumn != null)
                        {
                            Result = Result + ", " + ThisColumn.ParsableColumnKey();
                        }
                    }
                }

                return Result + "}";
            }


            public void Parse(string StringToParse)
            {
                IsParsing = true;
                Initialize();
                foreach (var pair in StringToParse.GetAllTags())
                {
                    switch (pair.Key)
                    {
                        case "identifier":
                            if (pair.Value != "SortDefinition") { Develop.DebugPrint(enFehlerArt.Fehler, "Identifier fehlerhaft: " + pair.Value); }
                            break;

                        case "direction":
                            Reverse = Convert.ToBoolean(pair.Value == "Z-A");
                            break;

                        case "column":
                        case "columnname": // Columname wichtig wegen CopyLayout
                            _Columns.Add(Database.Column[pair.Value]);
                            break;

                        case "columnkey":
                            _Columns.Add(Database.Column.SearchByKey(int.Parse(pair.Value)));
                            break;

                        default:
                            Develop.DebugPrint(enFehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                            break;
                    }
                }
                IsParsing = false;
            }


            private void SetColumn(string[] name)
            {
                _Columns.Clear();
                for (var z = 0 ; z <= name.GetUpperBound(0) ; z++)
                {
                    if (Database.Column[name[z]] != null)
                    {
                        _Columns.Add(Database.Column[name[z]]);
                    }
                }
            }

            public bool UsedForRowSort(ColumnItem vcolumn)
            {

                if (_Columns.Count == 0) { return false; }

                foreach (var ThisColumn in _Columns)
                {
                    if (ThisColumn == vcolumn) { return true; }
                }
                return false;
            }

            public void OnChanged()
            {
                if (IsParsing) { Develop.DebugPrint(enFehlerArt.Warnung, "Falscher Parsing Zugriff!"); return; }
                Changed?.Invoke(this, System.EventArgs.Empty);
            }


        }
    }