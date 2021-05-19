#region BlueElements - a collection of useful tools, database and controls
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
#endregion

using BlueBasics;
using BlueBasics.Enums;
using BlueDatabase.Enums;
using System;
using System.Drawing;

namespace BlueDatabase {
    public sealed class ColumnViewItem  {
        #region  Variablen-Deklarationen 


        private enViewType _ViewType;

        /// <summary>
        /// Koordinaten Angabe in "Spalten"
        /// </summary>
        private int _Spalte_X1;

        /// <summary>
        /// Koordinaten Angabe in "Spalten"
        /// </summary>
        private int _Spalte_Width;

        /// <summary>
        /// // Koordinaten Angabe in "Spalten"
        /// </summary>
        private int _Spalte_Height;

        private enÜberschriftAnordnung _ÜberschriftAnordnung;

        public int? OrderTMP_Spalte_X1;

        public Rectangle _TMP_AutoFilterLocation;

        public Rectangle _TMP_ReduceLocation;

        public int? _TMP_DrawWidth;

        public bool _TMP_Reduced;



        #endregion


        #region  Event-Deklarationen + Delegaten 
        public event EventHandler Changed;
        #endregion


        #region  Construktor + Initialize 
        /// <summary>
        /// Info: Es wird keine Änderung ausgelöst
        /// </summary>
        private void Initialize() {
            _ViewType = enViewType.None;
            Column = null;
            _Spalte_X1 = 0;
            _Spalte_Width = 1;
            _Spalte_Height = 1;
            _ÜberschriftAnordnung = enÜberschriftAnordnung.Über_dem_Feld;
            OrderTMP_Spalte_X1 = null;
            _TMP_AutoFilterLocation = Rectangle.Empty;
            _TMP_ReduceLocation = Rectangle.Empty;
            _TMP_DrawWidth = null;
            _TMP_Reduced = false;
        }

        /// <summary>
        /// Info: Es wird keine Änderung ausgelöst
        /// </summary>
        public ColumnViewItem(ColumnItem Column, enViewType Type) {
            Initialize();
            this.Column = Column;
            _ViewType = Type;
            this.Column.CheckFormulaEditType();
        }

        /// <summary>
        /// Info: Es wird keine Änderung ausgelöst
        /// </summary>
        public ColumnViewItem(ColumnItem Column, enÜberschriftAnordnung Überschrift) {
            Initialize();
            this.Column = Column;
            _ViewType = enViewType.Column;
            _ÜberschriftAnordnung = Überschrift;
            this.Column.CheckFormulaEditType();
        }



        /// <summary>
        /// Info: Es wird keine Änderung ausgelöst
        /// </summary>
        public ColumnViewItem(Database database, string codeToParse) {

            Initialize();
            foreach (var pair in codeToParse.GetAllTags()) {
                switch (pair.Key) {
                    case "column":
                    case "columnname":// Columname wichtg, wegen CopyLayout
                        Column = database.Column[pair.Value];
                        break;

                    case "columnkey":
                        Column = database.Column.SearchByKey(int.Parse(pair.Value));
                        break;

                    case "x":
                        _Spalte_X1 = int.Parse(pair.Value);
                        break;

                    case "width":
                        _Spalte_Width = int.Parse(pair.Value);
                        break;

                    case "height":
                        _Spalte_Height = int.Parse(pair.Value);
                        break;

                    case "caption": // Todo: Alt 06.09.2019
                        _ÜberschriftAnordnung = (enÜberschriftAnordnung)int.Parse(pair.Value);
                        break;

                    case "permanent": // Todo: Alten Code Entfernen, Permanent wird nicht mehr verstringt 06.09.2019
                        _ViewType = enViewType.PermanentColumn;
                        break;

                    case "type":
                        _ViewType = (enViewType)int.Parse(pair.Value);
                        break;

                    default:
                        Develop.DebugPrint(enFehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                        break;
                }

            }



            if (Column != null && _ViewType == enViewType.None) { _ViewType = enViewType.Column; }
            if (Column != null && _ViewType != enViewType.None) { Column.CheckFormulaEditType(); }


        }

        #endregion


        #region  Properties 


        public enViewType ViewType {
            get => _ViewType;
            set {
                if (value == _ViewType) { return; }
                _ViewType = value;
                OnChanged();
            }
        }

        public enÜberschriftAnordnung ÜberschriftAnordnung {
            get => _ÜberschriftAnordnung;
            set {
                if (value == _ÜberschriftAnordnung) { return; }
                _ÜberschriftAnordnung = value;
                OnChanged();
            }
        }


        /// <summary>
        /// Nur wichtig für Formular
        /// </summary>
        public int Spalte_X1 {
            get => _Spalte_X1;
            set {
                if (value == _Spalte_X1) { return; }
                _Spalte_X1 = value;
                OnChanged();
            }
        }

        /// <summary>
        /// Nur wichtig für Formular
        /// </summary>
        public int Width {
            get => _Spalte_Width;
            set {
                if (value == _Spalte_Width) { return; }
                _Spalte_Width = value;
                OnChanged();
            }
        }

        public int Height {
            get => _Spalte_Height;
            set {
                if (value == _Spalte_Height) { return; }
                _Spalte_Height = value;
                OnChanged();
            }
        }




        public ColumnItem Column { get; private set; }

        #endregion





        /// <summary>
        /// Info: Es wird keine Änderung ausgelöst
        /// </summary>
        public void KoordÄndern(int ModX, int ModW, int ModH) {
            _Spalte_X1 += ModX;
            _Spalte_Width += ModW;
            _Spalte_Height += ModH;


            if (_Spalte_X1 < 0) { _Spalte_X1 = 0; }
            if (_Spalte_X1 > 18) { _Spalte_X1 = 18; }
            if (_Spalte_Width < 1) { _Spalte_Width = 1; }
            if (_Spalte_Width > 20) { _Spalte_Width = 20; }

            if (_Spalte_Height < 1) { _Spalte_Height = 1; }
            if (_Spalte_Height > 30 && ModH > 0) {

                //if (MessageBox.Show("Resthöhe benutzen?", enImageCode.Frage, "Ja", "Nein") == 1)
                //{
                //    _Spalte_Height = 30;
                //}
                //else
                //{
                _Spalte_Height = 31;
                //}

            }
            OnChanged();
        }






        public string ToString() {
            var Result = "{Type=" + (int)(_ViewType);
            if (Column != null) { Result = Result + ", " + Column.ParsableColumnKey(); }
            if (_Spalte_X1 > 0) { Result = Result + ", X=" + _Spalte_X1; }
            if (_Spalte_Width > 1) { Result = Result + ", Width=" + _Spalte_Width; }
            if (_Spalte_Height > 1) { Result = Result + ", Height=" + _Spalte_Height; }
            if (_ÜberschriftAnordnung != enÜberschriftAnordnung.Über_dem_Feld) { Result = Result + ", Caption=" + (int)(_ÜberschriftAnordnung); }
            return Result + "}";
        }



        public ColumnViewItem PreviewsVisible(ColumnViewCollection _Parent) {
            return _Parent?.PreviousVisible(this);
        }

        public ColumnViewItem NextVisible(ColumnViewCollection _Parent) {
            return _Parent?.NextVisible(this);
        }


        public void OnChanged() {
            Changed?.Invoke(this, System.EventArgs.Empty);
        }


    }
}