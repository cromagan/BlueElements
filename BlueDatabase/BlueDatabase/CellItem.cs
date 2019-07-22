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
using BlueDatabase.EventArgs;
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
        RowItem _Row = null;
        ColumnItem _Column = null;

        Database _OldLinkedDatabase = null;


        #region Konstruktor

        internal CellItem(ColumnItem column, RowItem row)
        {
            if (row == null || column == null) { Develop.DebugPrint(enFehlerArt.Fehler, "Ungültige Angaben"); }

            Row = row;
            Column = column;
            GetLinkedData(false);
        }
        #endregion




        #region  Event-Deklarationen + Delegaten 
        public event EventHandler ValueChanged;
        public event EventHandler<CellKeyChangedEventArgs> KeyChanged;
        #endregion



        public Size Size { get; set; }

        public ColumnItem Column
        {
            get
            {
                return _Column;
            }
            private set
            {
                if (_Column == value) { return; }

                if (_Column != null)
                {
                    _Column.KeyChanged -= _ColumnReal_KeyChanged;
                    _Column.LinkDataChanged -= _ColumnReal_LinkDataChanged;

                }

                _Column = value;

                if (_Column != null)
                {
                    _Column.KeyChanged += _ColumnReal_KeyChanged;
                    _Column.LinkDataChanged += _ColumnReal_LinkDataChanged;
                }

            }
        }


        public RowItem Row
        {
            get
            {
                return _Row;
            }
            private set
            {
                if (_Row == value) { return; }

                if (_Row != null)
                {
                    _Row.KeyChanged -= _RowReal_KeyChanged;
                }

                _Row = value;

                if (_Row != null)
                {
                    _Row.KeyChanged += _RowReal_KeyChanged;
                }


            }
        }



        public Database Database
        {
            get
            {
                return _Column.Database;
            }
        }


        public CellItem CellLinked { get; private set; }


        //public Color BackColor { get; set; }
        //public Color FontColor { get; set; }

        //public bool Editable { get; set; }

        //public byte Symbol { get; set; }





        private void _ColumnReal_LinkDataChanged(object sender, System.EventArgs e)
        {
            if (Column.Format != enDataFormat.LinkedCell) { return; }
            GetLinkedData(true);
        }

        private void LinkedDB_Cell_CellRemoved(object sender, CellKeyEventArgs e)
        {
            if (Column.Format != enDataFormat.LinkedCell) { return; }
            if (e.CellKey == GetString())
            {
                GetLinkedData(true);
            }

        }


        private void LinkedDB_Cell_CellAdded(object sender, CellEventArgs e)
        {
            if (Column.Format != enDataFormat.LinkedCell) { return; }

            if (string.IsNullOrEmpty(GetString()))
            {
                GetLinkedData(true);
            }
        }


        #region +++ Get / Set +++

        #region Get/Set String +++ MAIN ROUTINE +++

        /// <summary>
        /// Der korrekt verlinkte Wert
        /// </summary>
        public string GetString()
        {
            if (Column.Format == enDataFormat.LinkedCell)
            {
                if (CellLinked != null) { return CellLinked.GetString(); }
                return string.Empty;
            }

            return _value;
        }
        public void Set(string Value)
        {
            Set(Value, false);
        }

        public void Set(string value, bool freezedMode)
        {

            if (Column.Format == enDataFormat.LinkedCell)
            {
                CellLinked.Set(value);
                return;
            }


            var OldValue = _value;

            value = Column.AutoCorrect(value);

            //    var CellKey = KeyOfCell(Column.Key, Row.Key);
            //    var OldValue = string.Empty;

            //    if (_cells.ContainsKey(CellKey)) { OldValue = _cells[CellKey].String; }

            if (value == OldValue) { return; }

            Database.AddPending(enDatabaseDataType.ce_Value_withoutSizeData, Column.Key, Row.Key, OldValue, value, true, freezedMode);


            GetLinkedData(true);

            Column._UcaseNamesSortedByLenght = null;

            Database.Cell.DoSpecialFormats(Column, Row.Key, OldValue, freezedMode, false);


            if (Database.Column.SysRowChanger.SaveContent) { Database.Cell.Set(Database.Column.SysRowChanger, Row, Database.UserName, freezedMode); }
            if (Database.Column.SysRowChangeDate.SaveContent) { Database.Cell.Set(Database.Column.SysRowChangeDate, Row, DateTime.Now.ToString(), freezedMode); }






            Size = Size.Empty;
            Column.Invalidate_TmpColumnContentWidth();

            OnValueChanged();



            InvalidateSize();
            OnValueChanged();
        }


        #endregion

        #region Get/Set Boolean


        public bool GetBoolean()
        {
            return GetString().FromPlusMinus();
        }

        public void Set(bool Value)
        {
            Set(Value.ToPlusMinus(), false);
        }
        public void Set(bool Value, bool FreezeMode)
        {
            Set(Value.ToPlusMinus(), FreezeMode);
        }

        #endregion

        //#region Get/Set Bitmap

        //public Bitmap GetBitmap()
        //{
        //    return modConverter.StringToBitmap(GetString());
        //}


        //#endregion

        //#region Get/Set Array






        //public string[] GetArray()
        //{
        //    return GetString().SplitByCR();
        //}


        //#endregion

        #region Get/Set List<string>

        public List<string> GetList()
        {
            return GetString().SplitByCRToList();

        }

        public void Set(List<string> Value, bool FreezeMode)
        {
            Set(Value.JoinWithCr(), FreezeMode);
        }
        public void Set(List<string> Value)
        {
            Set(Value.JoinWithCr(), false);
        }


        #endregion

        #region Get/Set DateTime

        public DateTime GetDate()
        {
            var _String = GetString();
            if (string.IsNullOrEmpty(_String)) { return default(DateTime); }
            if (DateTimeTryParse(_String, out var d)) { return d; }
            return default(DateTime);
        }

        public void Set(DateTime Value, bool FreezeMode)
        {
            Set(Value.ToString(Constants.Format_Date5), FreezeMode);
        }


        public void Set(DateTime Value)
        {
            Set(Value.ToString(Constants.Format_Date5), false);
        }


        #endregion

        #region Get/Set Point



        public Point GetPoint()
        {
            var _String = GetString();
            if (string.IsNullOrEmpty(_String)) { return new Point(); }
            return Extensions.PointParse(_String);
        }


        public void Set(Point Value, bool FreezeMode)
        {
            // {X=253,Y=194} MUSS ES SEIN, prüfen
            Set(Value.ToString(), FreezeMode);
        }

        public void Set(Point Value)
        {
            // {X=253,Y=194} MUSS ES SEIN, prüfen
            Set(Value.ToString(), false);
        }


        #endregion

        #region Get/Set Integer

        public void Set(int Value, bool FreezeMode)
        {
            Set(Value.ToString(), FreezeMode);
        }

        public void Set(int Value)
        {
            Set(Value.ToString(), false);
        }
        public int GetInteger()
        {
            var x = GetString();
            if (string.IsNullOrEmpty(x)) { return 0; }
            return int.Parse(x);
        }


        #endregion

        #region Get/Set Double
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


        #endregion

        #region Get/Set Colour

        public void Set(Color Value)
        {
            Set(Value.ToArgb().ToString(), false);
        }

        public void Set(Color Value, bool FreezeMode)
        {
            Set(Value.ToArgb().ToString(), FreezeMode);
        }

        public Color GetColor()
        {
            return Color.FromArgb(GetInteger());
        }

        #region Get/Set ColorBGR

        public int GetColorBGR()
        {
            var c = GetColor();
            int colorBlue = c.B;
            int colorGreen = c.G;
            int colorRed = c.R;
            return (colorBlue << 16) | (colorGreen << 8) | colorRed;
        }

        #endregion

        #endregion

        #region Get/Set Decimal

        public void Set(decimal Value)
        {
            Set(Value.ToString(), false);
        }

        public void Set(decimal Value, bool FreezeMode)
        {
            Set(Value.ToString(), FreezeMode);
        }

        public decimal GetDecimal()
        {
            var x = GetString();
            if (string.IsNullOrEmpty(x)) { return 0; }
            return decimal.Parse(x);
        }


        #endregion


        #endregion


        //public static List<string> ValuesReadable(ColumnItem column, RowItem row, enShortenStyle style)
        //{
        //    if (CellCollection.IsNullOrEmpty(column, row)) { return null; }

        //    //return ValuesReadable(Column, Row, style);

        //    return column.Database.Cell[column, row].ValuesReadable(style);
        //}



        private void GetLinkedData(bool SearchIndex)
        {

            if (CellLinked != null)
            {
                CellLinked.ValueChanged -= CellLinked_ValueChanged;
                CellLinked.KeyChanged -= CellLinked_KeyChanged;
            }
            if (_OldLinkedDatabase != null)
            {
                _OldLinkedDatabase.Cell.CellAdded -= LinkedDB_Cell_CellAdded;
                _OldLinkedDatabase.Cell.CellRemoved -= LinkedDB_Cell_CellRemoved;
            }



            CellLinked = null;
            _OldLinkedDatabase = null;


            if (Column.Format == enDataFormat.LinkedCell)
            {
                _OldLinkedDatabase = Column.LinkedDatabase();
                if (_OldLinkedDatabase == null)
                {
                    return;
                }
                else
                {
                    _OldLinkedDatabase.Cell.CellAdded += LinkedDB_Cell_CellAdded;
                    _OldLinkedDatabase.Cell.CellRemoved += LinkedDB_Cell_CellRemoved;
                }


                if (SearchIndex)
                {

                    var oval = _value;

                    _value = RepairLinkedCellValuex(false);

                    if (!Database.IsParsing())
                    {
                        Database.AddPending(enDatabaseDataType.ce_Value_withoutSizeData, Column.Key, Row.Key, oval, _value, false, false);
                    }
                }


                if (!string.IsNullOrEmpty(_value))
                {
                    CellLinked = _OldLinkedDatabase.Cell[_value];
                }
            }


            if (CellLinked != null)
            {
                CellLinked.ValueChanged += CellLinked_ValueChanged;
                CellLinked.KeyChanged += CellLinked_KeyChanged;
            }

        }

        internal void InvalidateSize()
        {
            Size = Size.Empty;
        }


        /// <summary>
        /// Jede Zeile für sich richtig formatiert.
        /// </summary>
        /// <returns></returns>
        public List<string> ValuesReadable(enShortenStyle Style)
        {

            if (Column.Format == enDataFormat.LinkedCell)
            {
                if (CellLinked != null) { return CellLinked.ValuesReadable(Style); }
                return new List<string>();
            }


            if (GetString() == null) { return new List<string>(); }


            var ret = new List<string>();

            if (!CellLinked.Column.MultiLine)
            {
                ret.Add(ValueReadable(CellLinked.Column, CellLinked.GetString(), Style));
                return ret;
            }

            var x = GetList();
            foreach (var thisstring in x)
            {
                ret.Add(ValueReadable(Column, thisstring, Style));
            }

            if (x.Count == 0)
            {
                var tmp = ValueReadable(Column, string.Empty, Style);
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

            Column = column;
            Row = row;
            // DatabaseReal = row.Database;
            GetLinkedData(false);
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
            return CellCollection.KeyOfCell(Column, Row);
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

        private void CellLinked_ValueChanged(object sender, System.EventArgs e)
        {
            InvalidateSize();
            OnValueChanged();
        }


        private void CellLinked_KeyChanged(object sender, CellKeyChangedEventArgs e)
        {
            if (e.KeyNew != _value) { Develop.DebugPrint(enFehlerArt.Fehler, "Key Inkonsistent"); }
            GetLinkedData(true);
            //            RepairLinkedCellValue(false);
            //_value = e.KeyNew;

            //SetReal(e.KeyNew, false);
        }


        internal void OnValueChanged()
        {
            ValueChanged?.Invoke(this, System.EventArgs.Empty);
            Database.Cell.OnCellValueChanged(new CellEventArgs(this)); // TODO: Über Events korrekt lösen!!!!
        }

        private void _ColumnReal_KeyChanged(object sender, KeyChangedEventArgs e)
        {
            OnKeyChanged(CellCollection.KeyOfCell(e.KeyOld, _Row.Key));
        }
        private void _RowReal_KeyChanged(object sender, KeyChangedEventArgs e)
        {
            OnKeyChanged(CellCollection.KeyOfCell(_Column.Key, e.KeyOld));
        }




        private void OnKeyChanged(string oldkey)
        {
            KeyChanged?.Invoke(this, new CellKeyChangedEventArgs(oldkey, CellKeyReal()));
        }

        internal void SaveToByteList(List<byte> List)
        {

            //internal void SaveToByteList(List<byte> List, KeyValuePair<string, CellItem> vCell)
            //{

            if (string.IsNullOrEmpty(GetString())) { return; }


            if (!Column.SaveContent) { return; }

            var s = GetString();
            var tx = enDatabaseDataType.ce_Value_withSizeData;

            if (Column.Format.NeedUTF8())
            {
                s = modConverter.StringtoUTF8(s);
                tx = enDatabaseDataType.ce_UTF8Value_withSizeData;
            }

            List.Add((byte)enRoutinen.CellFormat_OLD);
            List.Add((byte)tx);
            BlueDatabase.Database.SaveToByteList(List, s.Length, 3);
            BlueDatabase.Database.SaveToByteList(List, Column.Key, 3);
            BlueDatabase.Database.SaveToByteList(List, Row.Key, 3);
            List.AddRange(s.ToByte());
            BlueDatabase.Database.SaveToByteList(List, Size.Width, 2);
            BlueDatabase.Database.SaveToByteList(List, Size.Height, 2);

            //}
            //;
        }

        //private void _TMP_LinkedDatabase_ColumnKeyChanged(object sender, KeyChangedEventArgs e)
        //{



        //    if (_Format != enDataFormat.Columns_für_LinkedCellDropdown)
        //    {
        //        var os = e.KeyOld.ToString();
        //        var ns = e.KeyNew.ToString();
        //        foreach (var ThisRow in Database.Row)
        //        {
        //            if (Database.Cell.GetStringBehindLinkedValue(this, ThisRow) == os)
        //            {
        //                Database.Cell.SetValueBehindLinkedValue(this, ThisRow, ns, false);
        //            }
        //        }
        //    }

        //    if (_Format != enDataFormat.LinkedCell)
        //    {
        //        var os = e.KeyOld.ToString() + "|";
        //        var ns = e.KeyNew.ToString() + "|";
        //        foreach (var ThisRow in Database.Row)
        //        {
        //            var val = Database.Cell.GetStringBehindLinkedValue(this, ThisRow);
        //            if (val.StartsWith(os))
        //            {
        //                Database.Cell.SetValueBehindLinkedValue(this, ThisRow, val.Replace(os, ns), false);
        //            }
        //        }
        //    }
        //}

        //private void _TMP_LinkedDatabase_RowKeyChanged(object sender, KeyChangedEventArgs e)
        //{
        //    if (_Format != enDataFormat.LinkedCell)
        //    {
        //        var os = "|" + e.KeyOld.ToString();
        //        var ns = "|" + e.KeyNew.ToString();
        //        foreach (var ThisRow in Database.Row)
        //        {
        //            var val = Database.Cell.GetStringBehindLinkedValue(this, ThisRow);
        //            if (val.EndsWith(os))
        //            {
        //                Database.Cell.SetValueBehindLinkedValue(this, ThisRow, val.Replace(os, ns), false);
        //            }
        //        }
        //    }
        //}

        internal string RepairLinkedCellValuex(bool FreezeMode)
        {

            if (Column.Format != enDataFormat.LinkedCell) { Develop.DebugPrint(enFehlerArt.Fehler, "Falsches Format! " + Column.Database.Filename + " " + Column.Name); }
            var targetColumnKey = -1;
            var targetRowKey = -1;

            if (Column.LinkedDatabase() == null) { return Ergebnis("Die verlinkte Datenbank existiert nicht"); }
            if (Column.LinkedDatabase() == Database) { return Ergebnis("Die Datenbank ist nicht verlinkt"); }


            ///
            /// Spaltenschlüssel in der Ziel-Datenbank ermitteln
            ///
            if (Column.LinkedCell_ColumnKey >= 0)
            {
                // Fixe angabe
                targetColumnKey = Column.LinkedCell_ColumnKey;
            }
            else
            {
                // Spalte aus einer Spalte lesen
                var LinkedCell_ColumnValueFoundInColumn = Column.Database.Column.SearchByKey(Column.LinkedCell_ColumnValueFoundIn);
                if (LinkedCell_ColumnValueFoundInColumn == null) { return Ergebnis("Die Spalte, aus der der Spaltenschlüssel kommen soll, existiert nicht."); }

                if (!int.TryParse(Row.CellGetString(LinkedCell_ColumnValueFoundInColumn), out var colKey)) { return Ergebnis("Der Text Spalte der Spalte, aus der der Spaltenschlüssel kommen soll, ist fehlerhaft."); }

                if (string.IsNullOrEmpty(Column.LinkedCell_ColumnValueAdd))
                {   // Ohne Vorsatz
                    targetColumnKey = colKey;
                }
                else
                {
                    // Mit Vorsatz
                    var tarCx = Column.LinkedDatabase().Column.SearchByKey(colKey);
                    var tarCxn = Column.LinkedDatabase().Column[Column.LinkedCell_ColumnValueAdd + tarCx.Name];
                    if (tarCxn != null) { targetColumnKey = tarCxn.Key; }
                }
            }

            if (targetColumnKey < 0) { return Ergebnis("Die Spalte ist in der verlinkten Datenbank nicht vorhanden."); }


            ///
            /// Zeilenschlüssel lesen
            ///   
            var LinkedCell_RowColumn = Database.Column.SearchByKey(Column.LinkedCell_RowKey);
            if (LinkedCell_RowColumn == null) { return Ergebnis("Die Spalte, aus der der Zeilenschlüssel kommen soll, existiert nicht."); }

            if (Row.CellIsNullOrEmpty(LinkedCell_RowColumn)) { return Ergebnis("Kein Zeilenschlüssel angegeben."); }

            var tarR = Column.LinkedDatabase().Row[Row.CellGetString(LinkedCell_RowColumn)];

            if (tarR == null && Column.LinkedCell_Behaviour == enFehlendesZiel.ZeileAnlegen)
            {
                tarR = Column.LinkedDatabase().Row.Add(Row.CellGetString(LinkedCell_RowColumn));
            }

            if (tarR == null) { return Ergebnis("Die Zeile ist in der Zieldatenbank nicht vorhanden."); }

            targetRowKey = tarR.Key;

            return Ergebnis(string.Empty);

            /// --------Subroutine---------------------------
            string Ergebnis(string fehler)
            {
                if (string.IsNullOrEmpty(fehler))
                {
                    return CellCollection.KeyOfCell(targetColumnKey, targetRowKey);
                    // SetReal(, FreezeMode);
                }
                else
                {
                    return string.Empty;
                    //SetReal(string.Empty, FreezeMode);
                }

                //  return fehler;
            }

        }



    }
}
