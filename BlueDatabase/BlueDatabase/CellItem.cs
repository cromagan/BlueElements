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

using BlueBasics;
using BlueBasics.Enums;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.modAllgemein;

namespace BlueDatabase
{


    /// <summary>
    /// Diese Klasse enthält nur das Aussehen und gibt keinerlei Events ab. Deswegen INTERNAL!
    /// </summary>
    public class CellItem
    {
        string _value = string.Empty;


        #region Konstruktor

        internal CellItem() { }
        #endregion


        #region Properties

        public Size Size { get; set; }

        public ColumnItem ColumnReal { get; private set; }
        public RowItem RowReal { get; private set; }
        public Database DatabaseReal { get; private set; }


        //public ColumnItem ColumnLinked { get; private set; }


        /// <summary>
        /// Enthält die verlinkte Zelle. Falls das Form keine Verlinkung ist, wird 'this' zurückgegeben.
        /// </summary>
        private CellItem CellLinked { get; set; }


        //public Color BackColor { get; set; }
        //public Color FontColor { get; set; }

        //public bool Editable { get; set; }

        //public byte Symbol { get; set; }


        /// <summary>
        /// Gibt bevorzugt die verlinkte Spalte zurück. Falls es keine verlinkte Zelle ist, die unverlinkte Spalte.
        /// </summary>
        public ColumnItem Column
        {
            get
            {
                if (CellLinked == null) { return null; ; }
                return CellLinked.ColumnReal;
            }

        }

        /// <summary>
        /// Gibt bevorzugt die verlinkte Zeile zurück. Falls es keine verlinkte Zelle ist, die unverlinkte Zeile.
        /// </summary>
        public RowItem Row
        {
            get
            {
                if (CellLinked == null) { return null; ; }
                return CellLinked.RowReal;
            }

        }

        /// <summary>
        /// Gibt bevorzugt die verlinkte Datenbank zurück. Falls es keine verlinkte Zelle ist, die unverlinkte Datenbank.
        /// </summary>
        public Database Database
        {
            get
            {
                if (CellLinked == null) { return null; }
                return CellLinked.DatabaseReal;
            }

        }





        /// <summary>
        /// Wird nur im Ausnahmefall benutzt, wenn z.B: die Koordinaten der verlinkung gespeichert werden müssen.
        /// </summary>
        public string GetStringReal()
        {

            return _value;
        }

        public void SetReal(string value, bool freezeMode)
        {
            if (_value == value) { return; }
            Database.AddPending(enDatabaseDataType.ce_Value_withoutSizeData, ColumnReal.Key, RowReal.Key, _value, value, false, freezeMode);
            _value = value;
            GetLinkedData();
        }


        /// <summary>
        /// Der Korrekt verlinkte Wert
        /// </summary>
        public string GetString()
        {
            if (CellLinked == null) { return string.Empty; }
            return CellLinked.GetStringReal();
        }
        public void Set(string Value)
        {
            Set(Value, false);
        }

        public void Set(string value, bool Freezed)
        {
            Develop.DebugPrint_NichtImplementiert();


            //    Value = Column.AutoCorrect(Value);

            //    var CellKey = KeyOfCell(Column.Key, Row.Key);
            //    var OldValue = string.Empty;

            //    if (_cells.ContainsKey(CellKey)) { OldValue = _cells[CellKey].String; }

            //    if (Value == OldValue) { return; }

            //    Database.AddPending(enDatabaseDataType.ce_Value_withoutSizeData, Column.Key, Row.Key, OldValue, Value, true, FreezeMode);

            //    Column._UcaseNamesSortedByLenght = null;

            //    DoSpecialFormats(Column, Row.Key, OldValue, FreezeMode, false);


            //    Set(Database.Column.SysRowChanger, Row, Database.UserName, FreezeMode);
            //    Set(Database.Column.SysRowChangeDate, Row, DateTime.Now.ToString(), FreezeMode);


            //    Invalidate_CellContentSize(Column, Row);
            //    Column.Invalidate_TmpColumnContentWidth();

            //    OnCellValueChanged(new CellEventArgs(Column, Row));
            if (CellLinked == null)
            {
                Develop.DebugPrint(enFehlerArt.Warnung, "Keine verlinkte Zelle");
                return;
            }

            if (value == CellLinked.StringReal) { return; }
            CellLinked.StringReal = value;
            InvalidateSize();
        }



        public bool GetBoolean()
        {
            return GetString().FromPlusMinus();
        }

        public Bitmap GetBitmap()
        {
            return modConverter.StringToBitmap(GetString());
        }


        internal string CompareKey()
        {
            return DataFormat.CompareKey(GetString(), Column.Format);
        }












        public string[] GetArray()
        {
            return GetString().SplitByCR();
        }


        public List<string> GetList()
        {
            return new List<string>(GetArray());

        }

        public List<string> ValuesReadable(ColumnItem Column, RowItem Row, enShortenStyle style)
        {
            return ValuesReadable(Column, Row, style);
        }


        //public Size GetSizeOfCellContent()
        //{
        //    var CellKey = KeyOfCell(Column, Row);
        //    if (_cells.ContainsKey(CellKey))
        //    {
        //        return Column.Database.Cell._cells[CellKey].Size;
        //    }
        //    return Size.Empty;
        //}

        public DateTime GetDate()
        {
            var _String = GetString();
            if (string.IsNullOrEmpty(_String)) { return default(DateTime); }
            if (DateTimeTryParse(_String, out var d)) { return d; }
            return default(DateTime);
        }


        public void Set(List<string> Value, bool FreezeMode)
        {
            Set(Value.JoinWithCr(), FreezeMode);
        }
        public void Set(List<string> Value)
        {
            Set(Value.JoinWithCr(), false);
        }

        public void Set(bool Value)
        {
            Set(Value.ToPlusMinus(), false);
        }
        public void Set(bool Value, bool FreezeMode)
        {
            Set(Value.ToPlusMinus(), FreezeMode);
        }


        public Point GetPoint()
        {
            var _String = GetString();
            if (string.IsNullOrEmpty(_String)) { return new Point(); }
            return Extensions.PointParse(_String);
        }

        public void Set(DateTime Value)
        {
            Set(Value.ToString(Constants.Format_Date5), false);
        }

        public void Set(Point Value)
        {
            // {X=253,Y=194} MUSS ES SEIN, prüfen
            Set(Value.ToString(), false);
        }

        public void Set(int Value, bool FreezeMode)
        {
            Set(FreezeMode);
        }

        public void Set(int Value)
        {
            Set(Value.ToString(), false);
        }

        public void Set(double Value)
        {
            Set(Value.ToString(), false);
        }

        public void Set(double Value, bool FreezeMode)
        {
            Set(Value.ToString(), FreezeMode);
        }




        public double GetDouble()
        {
            var x = GetString();
            if (string.IsNullOrEmpty(x)) { return 0; }
            return double.Parse(x);
        }

        public Color GetColor()
        {
            return Color.FromArgb(GetInteger());
        }

        public int GetInteger()
        {
            var x = GetString();
            if (string.IsNullOrEmpty(x)) { return 0; }
            return int.Parse(x);
        }

        public decimal GetDecimal()
        {
            var x = GetString();
            if (string.IsNullOrEmpty(x)) { return 0; }
            return decimal.Parse(x);
        }

        public int GetColorBGR()
        {
            var c = GetColor();
            int colorBlue = c.B;
            int colorGreen = c.G;
            int colorRed = c.R;
            return (colorBlue << 16) | (colorGreen << 8) | colorRed;
        }


        #endregion






        private void GetLinkedData()
        {
            CellLinked = this;


            if (ColumnReal.Format != enDataFormat.LinkedCell) { return; }

            var LinkedDatabase = ColumnReal.LinkedDatabase();
            if (LinkedDatabase == null)
            {
                CellLinked = null;
                return;
            }

            CellLinked = LinkedDatabase.Cell[_value];
        }



















        internal void InvalidateSize()
        {
            Size = Size.Empty;
        }


        /// <summary>
        /// Jede Zeile für sich richtig formatiert.
        /// </summary>
        /// <returns></returns>
        public static List<string> ValuesReadable(CellItem cell, enShortenStyle Style)
        {
            if (cell.GetString() == null) { return new List<string>(); }


            var ret = new List<string>();

            if (!cell.CellLinked.ColumnReal.MultiLine)
            {
                ret.Add(ValueReadable(cell.CellLinked.ColumnReal, cell.CellLinked.StringReal, Style));
                return ret;
            }

            var x = cell.GetList();
            foreach (var thisstring in x)
            {
                ret.Add(ValueReadable(cell.Column, thisstring, Style));
            }

            if (x.Count == 0)
            {
                var tmp = ValueReadable(cell.Column, string.Empty, Style);
                if (!string.IsNullOrEmpty(tmp)) { ret.Add(tmp); }
            }


            return ret;
        }

        internal void Load_310(ColumnItem column, RowItem row, string value, int width, int height)
        {
            _value = value; // Auf jeden Fall setzen. Auch falls es nachher entfernt wird, so ist es sicher leer

            if (width > 0)
            {
                Size = new Size(width, height);
            }
            else
            {
                Size = Size.Empty;
            }

            ColumnReal = column;
            RowReal = row;
            DatabaseReal = row.Database;
            GetLinkedData();
        }



        /// <summary>
        /// Gibt eine einzelne Zeile richtig formatiert zurück. Zeilenumbrüche werden eleminiert.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="Txt"></param>
        /// <param name="Style"></param>
        /// <returns></returns>
        public static string ValueReadable(ColumnItem column, string Txt, enShortenStyle Style)
        {

            switch (column.Format)
            {
                case enDataFormat.Text:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.Datum_und_Uhrzeit:
                case enDataFormat.Link_To_Filesystem:
                case enDataFormat.Gleitkommazahl:
                case enDataFormat.Ganzzahl:
                case enDataFormat.Values_für_LinkedCellDropdown:
                case enDataFormat.RelationText:
                case enDataFormat.LinkedCell:  // Bei LinkedCell kommt direkt der Text der verlinkten Zelle an
                                               //if (Txt == null || string.IsNullOrEmpty(Txt)) { return string.Empty; }
                    Txt = LanguageTool.ColumnReplace(Txt, column, Style);
                    Txt = Txt.Replace("\r\n", " ");
                    break;


                case enDataFormat.BildCode:
                    if (column.CompactView && Style != enShortenStyle.HTML) { return string.Empty; }
                    Txt = LanguageTool.ColumnReplace(Txt, column, Style);
                    break;


                case enDataFormat.Bit:
                    if (column.CompactView && Style != enShortenStyle.HTML) { return string.Empty; }
                    if (Txt == true.ToPlusMinus())
                    {
                        Txt = "Ja";
                        if (column == column.Database.Column.SysCorrect) { return "Ok"; }
                        if (column == column.Database.Column.SysLocked) { return "gesperrt"; }

                    }
                    else if (Txt == false.ToPlusMinus())
                    {
                        Txt = "Nein";
                        if (column == column.Database.Column.SysCorrect) { return "fehlerhaft"; }
                        if (column == column.Database.Column.SysLocked) { return "bearbeitbar"; }
                    }
                    else if (Txt == "o" || Txt == "O")
                    {
                        Txt = "Neutral";
                    }
                    else if (Txt == "?")
                    {
                        Txt = "Unbekannt";
                    }
                    Txt = LanguageTool.ColumnReplace(Txt, column, Style);
                    break;


                case enDataFormat.Farbcode:
                    if (Style == enShortenStyle.HTML) { break; }
                    if (column.CompactView) { return string.Empty; }
                    if (!string.IsNullOrEmpty(Txt) && Txt.IsFormat(enDataFormat.Farbcode))
                    {
                        var col = Color.FromArgb(int.Parse(Txt));
                        Txt = col.ColorName();
                    }
                    Txt = LanguageTool.ColumnReplace(Txt, column, Style);
                    break;


                case enDataFormat.Schrift:
                    if (Style == enShortenStyle.HTML) { break; }
                    //    Develop.DebugPrint_NichtImplementiert();
                    //if (string.IsNullOrEmpty(Txt) || Txt.Substring(0, 1) != "{") { return Txt; }

                    //if (Column.CompactView) { return string.Empty; }
                    //return BlueFont.Get(Txt).ReadableText();
                    return Txt;

                case enDataFormat.Columns_für_LinkedCellDropdown:
                    // Hier kommt die Spalten-ID  an
                    if (string.IsNullOrEmpty(Txt)) { return string.Empty; }
                    if (!int.TryParse(Txt, out var ColKey)) { return "Columkey kann nicht geparsed werden"; }
                    var LinkedDatabase = column.LinkedDatabase();
                    if (LinkedDatabase == null) { return "Datenbankverknüpfung fehlt"; }
                    var C = LinkedDatabase.Column.SearchByKey(ColKey);
                    if (C == null) { return "Columnkey nicht gefunden"; }

                    Txt = LanguageTool.ColumnReplace(C.ReadableText(), column, Style);
                    break;

                default:
                    Develop.DebugPrint(column.Format);
                    break;
            }

            if (Style != enShortenStyle.HTML) { return Txt; }


            if (Txt.Contains("\r")) { Develop.DebugPrint(enFehlerArt.Warnung, "\\r enthalten:" + Txt); }
            if (Txt.Contains("\n")) { Develop.DebugPrint(enFehlerArt.Warnung, "\\n enthalten:" + Txt); }


            while (Txt.StartsWith(" ") || Txt.StartsWith("<br>") || Txt.EndsWith(" ") || Txt.EndsWith("<br>"))
            {
                Txt = Txt.Trim();
                Txt = Txt.Trim("<br>");
            }

            return Txt;
        }



        public static QuickImage StandardImage(ColumnItem column, string Txt, QuickImage defaultImage)
        {

            switch (column.Format)
            {
                case enDataFormat.Text:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.RelationText:
                    return defaultImage; // z.B. KontextMenu

                case enDataFormat.Bit:
                    if (Txt == true.ToPlusMinus())
                    {
                        if (column == column.Database.Column.SysCorrect) { return QuickImage.Get("Häkchen|16||||||||80"); }
                        //if (column == column.Database.Column.SysLocked) { return QuickImage.Get(enImageCode.Schloss, 16,"00AA00",string.Empty); }
                        return QuickImage.Get(enImageCode.Häkchen, 16);
                    }
                    else if (Txt == false.ToPlusMinus())
                    {
                        if (column == column.Database.Column.SysCorrect) { return QuickImage.Get(enImageCode.Warnung, 16); }
                        //if (column == column.Database.Column.SysLocked) { return QuickImage.Get(enImageCode.Schlüssel, 16, "FFBB00", string.Empty); }
                        return QuickImage.Get(enImageCode.Kreuz, 16);
                    }
                    else if (Txt == "o" || Txt == "O")
                    {
                        return QuickImage.Get(enImageCode.Kreis2, 16);
                    }
                    else if (Txt == "?")
                    {
                        return QuickImage.Get(enImageCode.Fragezeichen, 16);
                    }
                    else if (string.IsNullOrEmpty(Txt))
                    {
                        return null;
                    }
                    else
                    {
                        return QuickImage.Get(enImageCode.Kritisch, 16);
                    }


                case enDataFormat.BildCode:
                    if (defaultImage != null || column == null) { return defaultImage; }// z.B. Dropdownmenu-Textfeld mit bereits definierten Icon
                    if (string.IsNullOrEmpty(Txt)) { return null; }

                    var code = column.Prefix + Txt + column.Suffix;
                    if (column.BildCode_ConstantHeight > 0) { code = code + "|" + column.BildCode_ConstantHeight; }
                    defaultImage = QuickImage.Get(code);

                    if (defaultImage != null && !defaultImage.IsError) { return defaultImage; }

                    if (column.BildCode_ImageNotFound != enImageNotFound.ShowErrorPic) { return null; }

                    if (column.BildCode_ConstantHeight > 0)
                    {
                        return QuickImage.Get("Fragezeichen|" + column.BildCode_ConstantHeight + "|||||200|||80");
                    }
                    else
                    {
                        return QuickImage.Get("Fragezeichen||||||200|||80");
                    }

                case enDataFormat.Farbcode:

                    if (!string.IsNullOrEmpty(Txt) && Txt.IsFormat(enDataFormat.Farbcode))
                    {
                        var col = Color.FromArgb(int.Parse(Txt));
                        return QuickImage.Get(enImageCode.Kreis, 16, "", col.ToHTMLCode());
                    }
                    return null;


                //case enDataFormat.Relation:
                //    if (ImageCode != null) { return ImageCode; }
                //    if (!string.IsNullOrEmpty(Txt)) { return new clsRelation(Column, null, Txt).SymbolForReadableText(); }
                //    return null;



                case enDataFormat.Link_To_Filesystem:
                    if (defaultImage != null) { return defaultImage; }
                    if (Txt.FileType() == enFileFormat.Unknown) { return null; }
                    return QuickImage.Get(Txt.FileType(), 48);

                case enDataFormat.Schrift:
                    //  Develop.DebugPrint_NichtImplementiert();
                    //if (string.IsNullOrEmpty(Txt) || Txt.Substring(0, 1) != "{") { return defaultImage; }
                    // return Skin.BlueFont.Get(Txt).SymbolForReadableText();
                    return defaultImage;

                case enDataFormat.LinkedCell:
                case enDataFormat.Columns_für_LinkedCellDropdown:
                case enDataFormat.Values_für_LinkedCellDropdown:
                    return null;

                default:
                    return null;

            }




        }



        public static enAlignment StandardAlignment(ColumnItem column)
        {


            switch (column.Align)
            {
                case enAlignmentHorizontal.Links: return enAlignment.Top_Left;
                case enAlignmentHorizontal.Rechts: return enAlignment.Top_Right;
                case enAlignmentHorizontal.Zentriert: return enAlignment.HorizontalCenter;
            }


            switch (column.Format)
            {
                case enDataFormat.Ganzzahl:
                case enDataFormat.Gleitkommazahl:
                    return enAlignment.Top_Right;

                case enDataFormat.Bit:
                    if (column.CompactView) { return enAlignment.Top_HorizontalCenter; }
                    return enAlignment.Top_Left;

                default:
                    return enAlignment.Top_Left;
            }

        }

        public string CellKeyReal()
        {
            return CellCollection.KeyOfCell(ColumnReal.Key, RowReal.Key);
        }

        //public string GetString()
        //{
        //    if (column == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Spalte ungültig!<br>" + Database.Filename); }
        //    if (row == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Zeile ungültig!<br>" + Database.Filename); }


        //    var CellKey = KeyOfCell(column, row);

        //    if (!_cells.ContainsKey(CellKey)) { return string.Empty; }

        //    return _cells[CellKey].String;
        //}

        //public string GetStringBehindLinkedValue()
        //{
        //    if (Column == null || Row == null) { return string.Empty; }
        //    var CellKey = KeyOfCell(Column, Row);
        //    if (!_cells.ContainsKey(CellKey)) { return string.Empty; }
        //    return _cells[CellKey].String;
        //}

        //public static string KeyOfCell()
        //{
        //    // Alte verweise eleminieren.
        //    if (Column != null) { Column = Column.Database.Column.SearchByKey(Column.Key); }
        //    if (Row != null) { Row = Row.Database.Row.SearchByKey(Row.Key); }

        //    if (Column == null && Row == null) { return string.Empty; }

        //    if (Column == null) { return KeyOfCell(-1, Row.Key); }
        //    if (Row == null) { return KeyOfCell(Column.Key, -1); }
        //    return KeyOfCell(Column.Key, Row.Key);
        //}




        /// <summary>
        ///  Gibt zurück, ob die Zelle bearbeitet werden kann.
        ///  Optional zusätzlich mit den Dateirechten.
        /// </summary>
        /// <param name="Column"></param>
        /// <param name="Row"></param>
        /// <param name="DateiRechtePrüfen"></param>
        /// <returns></returns>
        public bool UserEditPossible(bool DateiRechtePrüfen)
        {

            return string.IsNullOrEmpty(UserEditErrorReason(DateiRechtePrüfen));
        }

        /// <summary>
        /// Gibt einen Fehlergrund zurück, ob die Zelle bearbeitet werden kann.
        /// Optional zusätzlich mit den Dateirechten.
        /// </summary>
        /// <param name="Row"></param>
        /// <param name="DateiRechtePrüfen"></param>
        /// <param name="Column"></param>
        /// <returns></returns>
        public string UserEditErrorReason(bool DateiRechtePrüfen)
        {
            if (DatabaseReal.ReadOnly) { return LanguageTool.DoTranslate("Datenbank wurde schreibgeschützt geöffnet", true); }
            if (Database.ReadOnly) { return LanguageTool.DoTranslate("Die verlinkte Datenbank wurde schreibgeschützt geöffnet", true); }
            if (Column == null) { return LanguageTool.DoTranslate("Es ist keine Spalte ausgewählt.", true); }
            if (Column.Database != Database) { return LanguageTool.DoTranslate("Interner Fehler: Bezug der Datenbank zur Spalte ist fehlerhaft.", true); }

            if (!Column.SaveContent) { return LanguageTool.DoTranslate("Der Spalteninhalt wird nicht gespeichert.", true); }


            if (CellLinked == null) { return LanguageTool.DoTranslate("Die Spalte ist in der Quell-Datenbank nicht vorhanden.", true); }

            //if (Column.Format == enDataFormat.LinkedCell)
            //{
            //    var LCcell = CellCollection.LinkedCellData(column, row);
            //    if (LCcell != null)
            //    {
            //        var tmp = LCrow.Database.Cell.UserEditErrorReason(LCColumn, LCrow, DateiRechtePrüfen);
            //        if (!string.IsNullOrEmpty(tmp)) { return LanguageTool.DoTranslate("Die verlinkte Zelle kann nicht bearbeitet werden: ", true) + tmp; }
            //        return string.Empty;
            //    }
            //    if (LCColumn == null) { return LanguageTool.DoTranslate("Die Spalte ist in der Quell-Datenbank nicht vorhanden.", true); }
            //    if (LCrow == null) { return LanguageTool.DoTranslate("Neue Zeilen können bei verlinkten Zellen nicht erstellt werden.", true); }


            //    return LanguageTool.DoTranslate("Die Zeile ist in der Quell-Datenbank nicht vorhanden.", true);
            //}

            if (Row != null)
            {
                if (Column.Database != Database) { return LanguageTool.DoTranslate("Interner Fehler: Bezug der Datenbank zur Zeile ist fehlerhaft.", true); }
                if (Column != Database.Column.SysLocked && Row.CellGetBoolean(Database.Column.SysLocked) && !Column.EditTrotzSperreErlaubt) { return LanguageTool.DoTranslate("Da die Zeile als abgeschlossen markiert ist, kann die Zelle nicht bearbeitet werden.", true); }
                if (ColumnReal != DatabaseReal.Column.SysLocked && RowReal.CellGetBoolean(DatabaseReal.Column.SysLocked) && !ColumnReal.EditTrotzSperreErlaubt) { return LanguageTool.DoTranslate("Da die Zeile als abgeschlossen markiert ist, kann die Zelle nicht bearbeitet werden.", true); }
            }
            else
            {
                //Auf neue Zeile wird geprüft
                if (!Column.IsFirst()) { return LanguageTool.DoTranslate("Neue Zeilen müssen mit der ersten Spalte beginnen.", true); }
            }





            if (!Column.TextBearbeitungErlaubt && !Column.DropdownBearbeitungErlaubt)
            {
                return LanguageTool.DoTranslate("Die Inhalte dieser Spalte können nicht manuell bearbeitet werden, da keine Bearbeitungsmethode erlaubt ist.", true);
            }

            if (ColumnItem.UserEditDialogTypeInTable(Column.Format, false, true, Column.MultiLine) == enEditTypeTable.None)
            {
                return "Interner Programm-Fehler: Es ist keine Bearbeitungsmethode für den Typ des Spalteninhalts '" + Column.Format + "' definiert.";
            }


            foreach (var ThisRule in Database.Rules)
            {
                if (ThisRule != null)
                {
                    if (ThisRule.WillAlwaysCellOverride(Column)) { return LanguageTool.DoTranslate("Diese Zelle wird von automatischen Regeln befüllt.", true); }
                    if (ThisRule.BlockEditing(Column, Row)) { return LanguageTool.DoTranslate("Eine Regel sperrt diese Zelle.", true); }
                }
            }

            if (!Database.PermissionCheck(Column.PermissionGroups_ChangeCell, Row)) { return LanguageTool.DoTranslate("Sie haben nicht die nötigen Rechte, um diesen Wert zu ändern.", true); }
            if (!DatabaseReal.PermissionCheck(ColumnReal.PermissionGroups_ChangeCell, RowReal)) { return LanguageTool.DoTranslate("Sie haben nicht die nötigen Rechte, um diesen Wert zu ändern.", true); }
            return string.Empty;
        }





        internal Size ContentSizeToSave(KeyValuePair<string, CellItem> vCell, ColumnItem Column)
        {
            if (Column.Format.SaveSizeData())
            {

                if (vCell.Value.Size.Height > 4 &&
                    vCell.Value.Size.Height < 65025 &&
                    vCell.Value.Size.Width > 4 &&
                    vCell.Value.Size.Width < 65025) { return vCell.Value.Size; }
            }
            return Size.Empty;
        }






        //public static string ValueForHTMLExport(ColumnItem Column, string Einstiegstext)
        //{
        //    switch (Column.Format)
        //    {

        //        case enDataFormat.Text:
        //        case enDataFormat.Text_Ohne_Kritische_Zeichen:
        //        case enDataFormat.Text_mit_Formatierung:
        //        case enDataFormat.Datum_und_Uhrzeit:
        //        case enDataFormat.Ganzzahl:
        //        case enDataFormat.Telefonnummer:
        //        case enDataFormat.Email:
        //        case enDataFormat.InternetAdresse:
        //        case enDataFormat.BildCode:
        //        case enDataFormat.Link_To_Filesystem:
        //        case enDataFormat.Values_für_LinkedCellDropdown:
        //        case enDataFormat.RelationText:
        //            // hier nix.
        //            break;


        //        case enDataFormat.Bit:
        //            if (Einstiegstext == true.ToPlusMinus())
        //            {
        //                return "Ja";
        //            }
        //            else if (Einstiegstext == false.ToPlusMinus())
        //            {
        //                return "Nein";
        //            }
        //            else if (Einstiegstext == "o" || Einstiegstext == "O")
        //            {
        //                return "Neutral";
        //            }
        //            else if (Einstiegstext == "?")
        //            {
        //                return "Unbekannt";
        //            }
        //            else
        //            {
        //                return Einstiegstext;
        //            }


        //        case enDataFormat.Gleitkommazahl:
        //            Einstiegstext = Einstiegstext.Replace(".", ",");
        //            break;

        //        case enDataFormat.Binärdaten_Bild:
        //        case enDataFormat.Binärdaten:
        //        case enDataFormat.Farbcode:
        //            Einstiegstext = "?";
        //            break;


        //        case enDataFormat.LinkedCell:
        //            Develop.DebugPrint("Fremdzelle dürfte hier nicht ankommen");
        //            break;


        //        //case enDataFormat.Relation:
        //        //    var tmp = new clsRelation(Column, null, Einstiegstext);
        //        //    return tmp.ReadableText();


        //        case enDataFormat.Columns_für_LinkedCellDropdown:
        //            return Draw_FormatedText_TextOf(Einstiegstext, null, Column, enShortenStyle.Unreplaced);

        //        default:
        //            Develop.DebugPrint(Column.Format);
        //            return "???";
        //    }




        //    Einstiegstext = Einstiegstext.Replace("\r\n", "<br>");
        //    Einstiegstext = Einstiegstext.Replace("\r", "<br>");
        //    Einstiegstext = Einstiegstext.Replace("\n", "<br>");
        //    Einstiegstext = Einstiegstext.Trim();
        //    Einstiegstext = Einstiegstext.Trim("<br>");
        //    Einstiegstext = Einstiegstext.Trim();
        //    Einstiegstext = Einstiegstext.Trim("<br>");

        //    return Einstiegstext;

        //}





        //public static CellItem DataOfCellKey(Database Database, string CellKey)
        //{


        //    if (string.IsNullOrEmpty(CellKey)) { return null; }

        //    var cd = CellKey.SplitBy("|");
        //    if (cd.GetUpperBound(0) != 1) { Develop.DebugPrint(enFehlerArt.Fehler, "Falscher CellKey übergeben: " + CellKey); }



        //    Column = Database.Column.SearchByKey(int.Parse(cd[0]));
        //    Row = Database.Row.SearchByKey(int.Parse(cd[1]));
        //}



    }
}
