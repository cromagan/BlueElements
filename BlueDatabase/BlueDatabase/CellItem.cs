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
using System.Drawing;

namespace BlueDatabase
{


    /// <summary>
    /// Diese Klasse enthält nur das Aussehen und gibt keinerlei Events ab. Deswegen INTERNAL!
    /// </summary>
    public class CellItem
    {
        string _value = string.Empty;


        #region Konstruktor

        public CellItem(string value, int width, int height)
        {
            _value = value;
            if (width > 0) { Size = new Size(width, height); }
        }
        #endregion


        #region Properties

        public Size Size { get; set; }


        //public Color BackColor { get; set; }
        //public Color FontColor { get; set; }

        //public bool Editable { get; set; }

        //public byte Symbol { get; set; }

        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (_value == value) { return; }

                _value = value;
                InvalidateSize();

            }
        }

        #endregion

        internal void InvalidateSize()
        {
            Size = Size.Empty;
        }


        public static string CleanFormat(string value, ColumnItem column, RowItem row)
        {
            if (string.IsNullOrEmpty(value)) { return string.Empty; }
            if (column == null) { return string.Empty; }
            if (row == null) { return string.Empty; }

            switch (column.Format)
            {
                case enDataFormat.Text:
                case enDataFormat.InternetAdresse:
                case enDataFormat.RelationText:
                    return value;

                case enDataFormat.Text_Ohne_Kritische_Zeichen:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.BildCode:
                    return value.ReduceToChars(column.Format.AllowedChars());


                case enDataFormat.Email:
                    return value.ReduceToChars(Constants.Char_Buchstaben + Constants.Char_Buchstaben.ToUpper() + Constants.Char_Numerals + "@_-.");

                case enDataFormat.Telefonnummer:
                    if (value.ReduceToChars("+/().- " + Constants.Char_Numerals) != value) { return string.Empty; }
                    return value.ReduceToChars(Constants.Char_Numerals + "+");


                case enDataFormat.Datum_und_Uhrzeit:
                    if (value.Length == 6) { value = value + "1800"; }
                    if (!value.IsFormat(column.Format)) { return string.Empty; }
                    if (value.Length != 10) { return string.Empty; }// Nicht definiert!
                    return modAllgemein.DateTimeParse(value).ToString("yyyy-MM-dd HH:mm:ss");

                case enDataFormat.Link_To_Filesystem:
                    return value;


                case enDataFormat.Ganzzahl:
                    if (value.ReduceToChars("- " + Constants.Char_Numerals) != value) { return string.Empty; }
                    return value.ReduceToChars("-" + Constants.Char_Numerals).TrimStart('0');

                case enDataFormat.Gleitkommazahl:
                    if (value.ReduceToChars(Constants.Char_Numerals + "-,. ") != value) { return string.Empty; }
                    value = value.ReduceToChars(Constants.Char_Numerals + "-,.").Replace(".", ","); if (value == ",") { return string.Empty; }
                    return value;

                case enDataFormat.Bit:
                    return string.Empty;

                case enDataFormat.Columns_für_LinkedCellDropdown:
                    if (int.TryParse(value, out var ColKey))
                    {
                        var C = column.LinkedDatabase().Column.SearchByKey(ColKey);
                        if (C != null) { return C.ReadableText(); }
                    }


                    //column.Database.Cell.LinkedCellData(column, row, out var ContentHolderCellColumn, out _);
                    //if (ContentHolderCellColumn != null) { return column.ReadableText(); }
                    return value;

                default:
                    Develop.DebugPrint(column.Format);
                    return value;
            }

        }






    }
}
