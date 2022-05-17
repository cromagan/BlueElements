// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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
using BlueSQLDatabase.Enums;
using System;
using System.Drawing;
using static BlueBasics.Converter;

namespace BlueSQLDatabase {

    public sealed class ColumnViewItem {

        #region Fields

        public readonly ColumnViewCollection Parent;
        public int? OrderTmpSpalteX1;
        public Rectangle TmpAutoFilterLocation;
        public int? TmpDrawWidth;
        public bool TmpReduced;
        public Rectangle TmpReduceLocation;

        private EditTypeFormula _editType;

        /// <summary>
        /// // Koordinaten Angabe in "Spalten"
        /// </summary>
        private int _spalteHeight;

        /// <summary>
        /// Koordinaten Angabe in "Spalten"
        /// </summary>
        private int _spalteWidth;

        /// <summary>
        /// Koordinaten Angabe in "Spalten"
        /// </summary>
        private int _spalteX1;

        private ÜberschriftAnordnung _überschriftAnordnung;
        private ViewType _viewType;

        #endregion

        #region Constructors

        public ColumnViewItem(ColumnItem column, ViewType type, ColumnViewCollection parent) : this(parent) {
            Column = column;
            _viewType = type;
            _editType = Column.CheckFormulaEditType(Column.EditType);
        }

        public ColumnViewItem(ColumnItem column, ÜberschriftAnordnung überschrift, ColumnViewCollection parent) : this(parent) {
            Column = column;

            _viewType = ViewType.Column;
            _überschriftAnordnung = überschrift;
            _editType = Column.CheckFormulaEditType(Column.EditType);
        }

        public ColumnViewItem(Database database, string codeToParse, ColumnViewCollection parent) : this(parent) {
            foreach (var pair in codeToParse.GetAllTags()) {
                switch (pair.Key) {
                    case "column":

                    case "columnname":// Columname wichtg, wegen CopyLayout
                        Column = database.Column[pair.Value];
                        Column?.Repair(); // Alte Formate reparieren
                        _editType = Column.CheckFormulaEditType(Column.EditType);
                        break;

                    case "columnkey":
                        Column = database.Column.SearchByKey(LongParse(pair.Value));
                        Column?.Repair(); // Alte Formate reparieren
                        _editType = Column.CheckFormulaEditType(Column.EditType);
                        break;

                    case "x":
                        _spalteX1 = IntParse(pair.Value);
                        break;

                    case "width":
                        _spalteWidth = IntParse(pair.Value);
                        break;

                    case "height":
                        _spalteHeight = IntParse(pair.Value);
                        break;

                    case "caption":
                        _überschriftAnordnung = (ÜberschriftAnordnung)IntParse(pair.Value);
                        break;

                    case "permanent": // Todo: Alten Code Entfernen, Permanent wird nicht mehr verstringt 06.09.2019
                        _viewType = ViewType.PermanentColumn;
                        break;

                    case "type":
                        _viewType = (ViewType)IntParse(pair.Value);
                        break;

                    case "edittype":
                        _editType = (EditTypeFormula)IntParse(pair.Value);
                        break;

                    //if (_überschriftAnordnung != ÜberschriftAnordnung.Über_dem_Feld) { result = result + ", Caption=" + (int)_überschriftAnordnung; }
                    //result = result + ", EditType=" + (int)_editType;

                    default:
                        Develop.DebugPrint(FehlerArt.Fehler, "Tag unbekannt: " + pair.Key);
                        break;
                }
            }
            if (Column != null && _viewType == ViewType.None) { _viewType = ViewType.Column; }
            //if (Column != null && _viewType != ViewType.None) { Column.CheckFormulaEditType(); }
        }

        private ColumnViewItem(ColumnViewCollection parent) : base() {
            Parent = parent;
            _viewType = ViewType.None;
            Column = null;
            _spalteX1 = 0;
            _spalteWidth = 1;
            _spalteHeight = 1;
            _überschriftAnordnung = ÜberschriftAnordnung.Über_dem_Feld;
            OrderTmpSpalteX1 = null;
            TmpAutoFilterLocation = Rectangle.Empty;
            TmpReduceLocation = Rectangle.Empty;
            TmpDrawWidth = null;
            TmpReduced = false;
            _editType = EditTypeFormula.None;
        }

        #endregion

        #region Events

        public event EventHandler Changed;

        #endregion

        #region Properties

        public ColumnItem? Column { get; private set; }

        public EditTypeFormula EditType {
            get => _editType;
            set {
                if (_editType == value) { return; }
                _editType = value;
                OnChanged();
            }
        }

        public int Height {
            get => _spalteHeight;
            set {
                if (value == _spalteHeight) { return; }
                _spalteHeight = value;
                OnChanged();
            }
        }

        /// <summary>
        /// Für FlexOptions
        /// </summary>
        public bool Permanent {
            get => _viewType == ViewType.PermanentColumn;
            set {
                if (!PermanentPossible() && Permanent) { return; }
                if (!NonPermanentPossible() && !Permanent) { return; }

                if (value == Permanent) { return; }

                if (value) {
                    _viewType = ViewType.PermanentColumn;
                } else {
                    _viewType = ViewType.Column;
                }

                OnChanged();
            }
        }

        /// <summary>
        /// Nur wichtig für Formular
        /// </summary>
        public int Spalte_X1 {
            get => _spalteX1;
            set {
                if (value == _spalteX1) { return; }
                _spalteX1 = value;
                OnChanged();
            }
        }

        public ÜberschriftAnordnung ÜberschriftAnordnung {
            get => _überschriftAnordnung;
            set {
                if (value == _überschriftAnordnung) { return; }
                _überschriftAnordnung = value;
                OnChanged();
            }
        }

        public ViewType ViewType {
            get => _viewType;
            set {
                if (value == _viewType) { return; }
                _viewType = value;
                OnChanged();
            }
        }

        /// <summary>
        /// Nur wichtig für Formular
        /// </summary>
        public int Width {
            get => _spalteWidth;
            set {
                if (value == _spalteWidth) { return; }
                _spalteWidth = value;
                OnChanged();
            }
        }

        #endregion

        #region Methods

        public void Invalidate_DrawWidth() => TmpDrawWidth = null;

        /// <summary>
        /// Info: Es wird keine Änderung ausgelöst
        /// </summary>
        public void KoordÄndern(int modX, int modW, int modH) {
            _spalteX1 += modX;
            _spalteWidth += modW;
            _spalteHeight += modH;
            if (_spalteX1 < 0) { _spalteX1 = 0; }
            if (_spalteX1 > 18) { _spalteX1 = 18; }
            if (_spalteWidth < 1) { _spalteWidth = 1; }
            if (_spalteWidth > 20) { _spalteWidth = 20; }
            if (_spalteHeight < 1) { _spalteHeight = 1; }
            if (_spalteHeight > 30 && modH > 0) {
                //if (MessageBox.Show("Resthöhe benutzen?", ImageCode.Frage, "Ja", "Nein") == 1)
                //{
                //    _Spalte_Height = 30;
                //}
                //else
                //{
                _spalteHeight = 31;
                //}
            }
            OnChanged();
        }

        public ColumnViewItem? NextVisible() => Parent.NextVisible(this);

        public void OnChanged() => Changed?.Invoke(this, System.EventArgs.Empty);

        public ColumnViewItem? PreviewsVisible() => Parent.PreviousVisible(this);

        public override string ToString() {
            var result = "{Type=" + (int)_viewType;
            if (Column != null) { result = result + ", " + Column.ParsableColumnKey(); }
            if (_spalteX1 > 0) { result = result + ", X=" + _spalteX1; }
            if (_spalteWidth > 1) { result = result + ", Width=" + _spalteWidth; }
            if (_spalteHeight > 1) { result = result + ", Height=" + _spalteHeight; }
            if (_überschriftAnordnung != ÜberschriftAnordnung.Über_dem_Feld) { result = result + ", Caption=" + (int)_überschriftAnordnung; }
            if (_editType != EditTypeFormula.None) { result = result + ", EditType=" + (int)_editType; }

            return result + "}";
        }

        internal bool NonPermanentPossible() {
            //if (_arrangementNr < 1) {
            //    return !thisViewItem.Column.IsFirst();
            //}
            var nx = NextVisible();
            return nx == null || Convert.ToBoolean(nx.ViewType != ViewType.PermanentColumn);
        }

        internal bool PermanentPossible() {
            //if (_arrangementNr < 1) {
            //    return thisViewItem.Column.IsFirst();
            //}
            var prev = PreviewsVisible();
            return prev == null || Convert.ToBoolean(prev.ViewType == ViewType.PermanentColumn);
        }

        #endregion
    }
}