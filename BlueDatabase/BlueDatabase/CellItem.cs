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
        RowItem _RowReal = null;
        ColumnItem _ColumnReal = null;


        #region Konstruktor

        internal CellItem(ColumnItem column, RowItem row)
        {
            _RowReal = row;
            _ColumnReal = column;
        }
        #endregion




        #region  Event-Deklarationen + Delegaten 
        public event EventHandler ValueChanged;
        public event EventHandler<CellKeyChangedEventArgs> KeyChanged;
        #endregion



        public Size Size { get; set; }

        public ColumnItem ColumnReal
        {
            get
            {
                return _ColumnReal;
            }
            private set
            {
                if (_ColumnReal == value) { return; }

                if (_ColumnReal != null)
                {
                    _ColumnReal.KeyChanged -= _ColumnReal_KeyChanged;
                }

                _ColumnReal = value;

                if (_ColumnReal != null)
                {
                    _ColumnReal.KeyChanged += _ColumnReal_KeyChanged;
                }

            }
        }



        public RowItem RowReal
        {
            get
            {
                return _RowReal;
            }
            private set
            {
                if (_RowReal == value) { return; }

                if (_RowReal != null)
                {
                    _RowReal.KeyChanged -= _RowReal_KeyChanged;
                }

                _RowReal = value;

                if (_RowReal != null)
                {
                    _RowReal.KeyChanged += _RowReal_KeyChanged;
                }


            }
        }
        public Database DatabaseReal
        {
            get
            {
                return _ColumnReal.Database;
            }
        }

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
            OnValueChanged();
        }




        #region +++ Get / Set +++

        #region Get/Set String +++ MAIN ROUTINE +++

        /// <summary>
        /// Der korrekt verlinkte Wert
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

        public void Set(string value, bool freezedMode)
        {

            var OldValue = GetStringReal();

            value = ColumnReal.AutoCorrect(value);

            //    var CellKey = KeyOfCell(Column.Key, Row.Key);
            //    var OldValue = string.Empty;

            //    if (_cells.ContainsKey(CellKey)) { OldValue = _cells[CellKey].String; }

            if (value == OldValue) { return; }

            DatabaseReal.AddPending(enDatabaseDataType.ce_Value_withoutSizeData, ColumnReal.Key, RowReal.Key, OldValue, value, true, freezedMode);
            GetLinkedData();

            ColumnReal._UcaseNamesSortedByLenght = null;

            DatabaseReal.Cell.DoSpecialFormats(ColumnReal, RowReal.Key, OldValue, freezedMode, false);


            DatabaseReal.Cell.Set(Database.Column.SysRowChanger, Row, Database.UserName, freezedMode);
            DatabaseReal.Cell.Set(Database.Column.SysRowChangeDate, Row, DateTime.Now.ToString(), freezedMode);



            if (CellLinked == null)
            {
                Develop.DebugPrint(enFehlerArt.Warnung, "Keine verlinkte Zelle");
                return;
            }




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


        public static List<string> ValuesReadable(ColumnItem column, RowItem row, enShortenStyle style)
        {
            if (CellCollection.IsNullOrEmpty(column, row)) { return null; }

            //return ValuesReadable(Column, Row, style);

            return column.Database.Cell[column, row].ValuesReadable(style);
        }



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


            if (CellLinked != null)
            {
                CellLinked.ValueChanged -= CellLinked_ValueChanged;
                CellLinked.KeyChanged -= CellLinked_KeyChanged;
            }

            CellLinked = LinkedDatabase.Cell[_value];

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
            if (GetString() == null) { return new List<string>(); }


            var ret = new List<string>();

            if (!CellLinked.ColumnReal.MultiLine)
            {
                ret.Add(ValueReadable(CellLinked.ColumnReal, CellLinked.GetString(), Style));
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

            ColumnReal = column;
            RowReal = row;
           // DatabaseReal = row.Database;
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
            return CellCollection.KeyOfCell(ColumnReal, RowReal);
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


            //Invalidate_TmpColumnContentWidth();
            //Database.Cell.OnCellValueChanged(new CellEventArgs(this, ThisRow));
            //ThisRow.DoAutomatic(true, false);

        }


        private void CellLinked_KeyChanged(object sender, CellKeyChangedEventArgs e)
        {
            if (e.KeyNew != _value) { Develop.DebugPrint(enFehlerArt.Fehler, "Key Inkonstostentz"); }
            SetReal(e.KeyNew, false);
        }


        internal void OnValueChanged()
        {
            ValueChanged?.Invoke(this, System.EventArgs.Empty);
            DatabaseReal.Cell.OnCellValueChanged(new CellEventArgs(this)); // TODO: Über Events korrekt lösen!!!!
        }

        private void _ColumnReal_KeyChanged(object sender, KeyChangedEventArgs e)
        {
            OnKeyChanged(CellCollection.KeyOfCell(e.KeyOld, _RowReal.Key));
        }
        private void _RowReal_KeyChanged(object sender, KeyChangedEventArgs e)
        {
            OnKeyChanged(CellCollection.KeyOfCell(_ColumnReal.Key, e.KeyOld));
        }

        private void OnKeyChanged(string oldkey)
        {
            KeyChanged?.Invoke(this, new CellKeyChangedEventArgs(oldkey, CellKeyReal()));
        }

        internal void SaveToByteList(List<byte> List)
        {

            //internal void SaveToByteList(List<byte> List, KeyValuePair<string, CellItem> vCell)
            //{

            if (string.IsNullOrEmpty(GetStringReal())) { return; }


            if (!ColumnReal.SaveContent) { return; }

            var s = GetStringReal();
            var tx = enDatabaseDataType.ce_Value_withSizeData;

            if (ColumnReal.Format.NeedUTF8())
            {
                s = modConverter.StringtoUTF8(s);
                tx = enDatabaseDataType.ce_UTF8Value_withSizeData;
            }

            List.Add((byte)enRoutinen.CellFormat_OLD);
            List.Add((byte)tx);
            Database.SaveToByteList(List, s.Length, 3);
            Database.SaveToByteList(List, ColumnReal.Key, 3);
            Database.SaveToByteList(List, RowReal.Key, 3);
            List.AddRange(s.ToByte());
            Database.SaveToByteList(List, Size.Width, 2);
            Database.SaveToByteList(List, Size.Height, 2);

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



    }
}
