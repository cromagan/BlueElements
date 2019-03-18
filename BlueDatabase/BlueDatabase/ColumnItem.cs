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
using System.Drawing;
using System.Drawing.Imaging;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using static BlueBasics.FileOperations;
using System.Text.RegularExpressions;

namespace BlueDatabase
{
    public sealed class ColumnItem : IObjectWithDialog, IReadableText, ICompareKey, ICheckable
    {
        #region  Variablen-Deklarationen 

        public readonly Database Database;


        private string _Name;
        private bool _MultiLine;
        private string _Caption;
        private Bitmap _CaptionBitmap;
        private string _QuickInfo;
        private string _Ueberschrift1;
        private string _Ueberschrift2;
        private string _Ueberschrift3;
        private enDataFormat _Format;
        private enEditTypeFormula _EditType;
        private Color _ForeColor;
        private Color _BackColor;
        private enColumnLineStyle _LineLeft;
        private enColumnLineStyle _LineRight;

        private string _AutoFilterJoker;
        private string _Identifier;

        public readonly ListExt<string> DropDownItems = new ListExt<string>();
        public readonly ListExt<string> Tags = new ListExt<string>();
        public readonly ListExt<string> PermissionGroups_ChangeCell = new ListExt<string>();
        public readonly ListExt<string> Replacer = new ListExt<string>();

        private string _AllowedChars;
        private string _AdminInfo;
        private bool _AutofilterErlaubt;
        private bool _AutofilterTextFilterErlaubt;
        private bool _AutoFilterErweitertErlaubt;
        private bool _IgnoreAtRowFilter;
        private bool _CompactView;
        private bool _ShowMultiLineInOneLine;
        private bool _DropdownBearbeitungErlaubt;
        private bool _DropdownAllesAbw�hlenErlaubt;
        private bool _TextBearbeitungErlaubt;
        private bool _DropdownWerteAndererZellenAnzeigen;

        private bool _EditTrotzSperreErlaubt;
        private bool _SpellCheckingEnabled;
        private string _Suffix;
        private bool _ShowUndo;


        private string _LinkedKeyKennung;
        private string _LinkedDatabaseFile;
        private enImageNotFound _BildCode_ImageNotFound;
        private int _BildCode_ConstantHeight;

        private string _ImagePrefix;
        private string _ImageSuffix;

        private string _BestFile_StandardSuffix;
        private string _BestFile_StandardFolder;

        private bool _AfterEdit_QuickSortRemoveDouble;
        private int _AfterEdit_Runden;
        private bool _AfterEdit_DoUCase;
        private bool _AfterEdit_AutoCorrect;

        private string _CellInitValue;

        public SizeF TMP_CaptionText_Size = new SizeF(-1, -1);
        internal Database _TMP_LinkedDatabase;
        public int? TMP_ColumnContentWidth = null;

        internal List<string> _UcaseNamesSortedByLenght = null;

        public const string AllowedCharsInternalName = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.[]()";

        #endregion


        #region  Event-Deklarationen + Delegaten 
        public event EventHandler Changed;
        #endregion


        #region  Construktor + Initialize 


        private void Initialize()
        {
            _Name = string.Empty;
            _Caption = string.Empty;
            _CaptionBitmap = null;

            _Format = enDataFormat.Bit;
            _LineLeft = enColumnLineStyle.D�nn;
            _LineRight = enColumnLineStyle.Ohne;
            _MultiLine = false;
            _QuickInfo = string.Empty;
            _Ueberschrift1 = string.Empty;
            _Ueberschrift2 = string.Empty;
            _Ueberschrift3 = string.Empty;
            _ForeColor = Color.Black;
            _BackColor = Color.White;
            _CellInitValue = string.Empty;


            _EditType = enEditTypeFormula.Textfeld;
            _Identifier = string.Empty;



            _AllowedChars = string.Empty;
            _AdminInfo = string.Empty;
            _AutofilterErlaubt = true;
            _AutofilterTextFilterErlaubt = true;
            _AutoFilterErweitertErlaubt = true;
            _IgnoreAtRowFilter = false;
            _DropdownBearbeitungErlaubt = false;
            _DropdownAllesAbw�hlenErlaubt = false;
            _TextBearbeitungErlaubt = false;
            _DropdownWerteAndererZellenAnzeigen = false;
            _AfterEdit_QuickSortRemoveDouble = false;
            _AfterEdit_Runden = -1;
            _AfterEdit_AutoCorrect = false;
            _AfterEdit_DoUCase = false;
            _AutoFilterJoker = string.Empty;

            _SpellCheckingEnabled = false;

            _CompactView = true;
            _ShowUndo = true;
            _ShowMultiLineInOneLine = false;
            _EditTrotzSperreErlaubt = false;

            _Suffix = string.Empty;

            _LinkedKeyKennung = string.Empty;
            _LinkedDatabaseFile = string.Empty;
            _BildCode_ImageNotFound = enImageNotFound.ShowErrorPic;
            _BildCode_ConstantHeight = 0;
            _ImagePrefix = string.Empty;
            _ImageSuffix = string.Empty;
            _BestFile_StandardSuffix = string.Empty;
            _BestFile_StandardFolder = string.Empty;
            _UcaseNamesSortedByLenght = null;

            Invalidate_TmpVariables();
        }



        public ColumnItem(ColumnItem Source, bool addtodatabase) : this(Source.Database, -1, Source._Name, addtodatabase)
        {
            Caption = Source.Caption;
            CaptionBitmap = Source.CaptionBitmap;

            Format = Source.Format;
            LineLeft = Source.LineLeft;
            LineRight = Source.LineRight;
            MultiLine = Source.MultiLine;
            Quickinfo = Source.Quickinfo;
            ForeColor = Source.ForeColor;
            BackColor = Source.BackColor;

            EditTrotzSperreErlaubt = Source.EditTrotzSperreErlaubt;


            EditType = Source.EditType;
            Identifier = Source.Identifier;

            PermissionGroups_ChangeCell.Clear();
            PermissionGroups_ChangeCell.AddRange(Source.PermissionGroups_ChangeCell);

            Tags.Clear();
            Tags.AddRange(Source.Tags);


            AllowedChars = Source.AllowedChars;
            AdminInfo = Source.AdminInfo;
            AutoFilterErlaubt = Source.AutoFilterErlaubt;
            AutofilterTextFilterErlaubt = Source.AutofilterTextFilterErlaubt;
            AutoFilterErweitertErlaubt = Source.AutoFilterErweitertErlaubt;
            IgnoreAtRowFilter = Source.IgnoreAtRowFilter;
            DropdownBearbeitungErlaubt = Source.DropdownBearbeitungErlaubt;
            DropdownAllesAbw�hlenErlaubt = Source.DropdownAllesAbw�hlenErlaubt;
            TextBearbeitungErlaubt = Source.TextBearbeitungErlaubt;
            SpellCheckingEnabled = Source.SpellCheckingEnabled;
            DropdownWerteAndererZellenAnzeigen = Source.DropdownWerteAndererZellenAnzeigen;
            AfterEdit_QuickSortRemoveDouble = Source.AfterEdit_QuickSortRemoveDouble;

            AfterEdit_Runden = Source.AfterEdit_Runden;
            AfterEdit_DoUCase = Source.AfterEdit_DoUCase;
            AfterEdit_AutoCorrect = Source.AfterEdit_AutoCorrect;
            CellInitValue = Source.CellInitValue;
            AutoFilterJoker = Source.AutoFilterJoker;

            DropDownItems.Clear();
            DropDownItems.AddRange(Source.DropDownItems);

            Replacer.Clear();
            Replacer.AddRange(Source.Replacer);


            CompactView = Source.CompactView;
            ShowUndo = Source.ShowUndo;
            ShowMultiLineInOneLine = Source.ShowMultiLineInOneLine;

            Ueberschrift1 = Source.Ueberschrift1;
            Ueberschrift2 = Source.Ueberschrift2;
            Ueberschrift3 = Source.Ueberschrift3;

            Suffix = Source.Suffix;

            LinkedKeyKennung = Source.LinkedKeyKennung;
            LinkedDatabaseFile = Source.LinkedDatabaseFile;
            BildCode_ImageNotFound = Source.BildCode_ImageNotFound;
            BildCode_ConstantHeight = Source.BildCode_ConstantHeight;
            BestFile_StandardSuffix = Source.BestFile_StandardSuffix;
            BestFile_StandardFolder = Source.BestFile_StandardFolder;

            ImagePrefix = Source.ImagePrefix;
            ImageSuffix = Source.ImageSuffix;

        }



        public ColumnItem(Database database, bool addtodatabase) : this(database, -1, string.Empty, addtodatabase) { }

        public ColumnItem(Database database, string columninternalname, bool addtodatabase) : this(database, -1, columninternalname, addtodatabase) { }


        public ColumnItem(Database database, string columninternalname, string caption, string suffix, enDataFormat format, bool addtodatabase) : this(database, -1, columninternalname, addtodatabase)
        {
            Caption = caption;
            Suffix = suffix;
            Format = format;

            MultiLine = false;
            TextBearbeitungErlaubt = true;
        }

        public ColumnItem(Database database, int columnkey, string columninternalname, bool addtodatabase)
        {


            if (!addtodatabase)
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Neue Spalte MUSS zur Datenbank hinzugef�gt werden.");
                // Grund: Undos m�ssen geloggt werden
            }

            Database = database;



            if (columnkey == -1)
            {
                this.Key = Database.Column.NextColumnKey();
            }
            else
            {
                this.Key = columnkey;
            }


            DropDownItems.ListOrItemChanged += DropDownItems_ListOrItemChanged;
            Replacer.ListOrItemChanged += Replacer_ListOrItemChanged;
            PermissionGroups_ChangeCell.ListOrItemChanged += PermissionGroups_ChangeCell_ListOrItemChanged;

            Tags.ListOrItemChanged += Tags_ListOrItemChanged;

            Initialize();
            Name = Database.Column.Freename(columninternalname);


            if (addtodatabase) { database.Column.Add(this); }

        }


        #endregion




        #region  Properties 



        public int Key { get; }

        public string Caption
        {
            get
            {
                return _Caption;
            }
            set
            {
                value = value.Replace("<br>", "\r", RegexOptions.IgnoreCase);
                if (_Caption == value) { return; }
                Database.AddPending(enDatabaseDataType.co_Caption, this, _Caption, value, true);
                Invalidate_TmpVariables();
                OnChanged();
            }
        }



        public string AutoFilterJoker
        {
            get
            {
                return _AutoFilterJoker;
            }
            set
            {
                if (_AutoFilterJoker == value) { return; }
                Database.AddPending(enDatabaseDataType.co_AutoFilterJoker, this, _AutoFilterJoker, value, true);
                OnChanged();
            }
        }


        public string Name
        {
            get
            {
                return _Name.ToUpper();
            }
            set
            {
                value = value.ToUpper();
                if (value == _Name.ToUpper()) { return; }
                if (Database.Column[value] != null) { return; }
                if (string.IsNullOrEmpty(value)) { return; }

                Database.AddPending(enDatabaseDataType.co_Name, this, _Name, value, false);

                var old = _Name;
                _Name = value;
                Database.Column_NameChanged(old, this);
                OnChanged();
            }
        }

        public string Identifier
        {
            get
            {
                return _Identifier;
            }
            set
            {
                if (_Identifier == value) { return; }
                Database.AddPending(enDatabaseDataType.co_Identifier, this, _Identifier, value, true);
                OnChanged();
            }
        }

        public enEditTypeFormula EditType
        {
            get
            {
                return _EditType;
            }
            set
            {
                if (_EditType == value) { return; }
                Database.AddPending(enDatabaseDataType.co_EditType, this, ((int)_EditType).ToString(), ((int)value).ToString(), true);
                OnChanged();
            }
        }

        public bool MultiLine
        {
            get
            {
                return _MultiLine && _Format.MultilinePossible();
            }
            set
            {
                if (!_Format.MultilinePossible()) { value = false; }

                if (_MultiLine == value) { return; }
                Database.AddPending(enDatabaseDataType.co_MultiLine, this, _MultiLine.ToPlusMinus(), value.ToPlusMinus(), true);
                Invalidate_ColumAndContent();
                OnChanged();

            }
        }

        public string QickInfoText(string AdditionalText)
        {
            var T = string.Empty;
            if (!string.IsNullOrEmpty(_QuickInfo)) { T = T + _QuickInfo; }
            if (Database.IsAdministrator() && !string.IsNullOrEmpty(_AdminInfo)) { T = T + "<br><br><b><u>Administrator-Info:</b></u><br>" + _AdminInfo; }


            T = T.Trim();
            T = T.Trim("<br>");
            T = T.Trim();


            if (!string.IsNullOrEmpty(T) && !string.IsNullOrEmpty(AdditionalText))
            {
                T = "<b><u>" + AdditionalText + "</b></u><br><br>" + T;
            }

            return T;
        }

        public string Quickinfo
        {
            get
            {
                return _QuickInfo;
            }
            set
            {
                if (_QuickInfo == value) { return; }
                Database.AddPending(enDatabaseDataType.co_QuickInfo, this, _QuickInfo, value, true);
                OnChanged();
            }
        }

        public string Ueberschrift1
        {
            get
            {
                return _Ueberschrift1;
            }
            set
            {
                if (_Ueberschrift1 == value) { return; }
                Database.AddPending(enDatabaseDataType.co_Ueberschrift1, this, _Ueberschrift1, value, true);
                OnChanged();
            }
        }


        public string Ueberschrift2
        {
            get
            {
                return _Ueberschrift2;
            }
            set
            {
                if (_Ueberschrift2 == value) { return; }
                Database.AddPending(enDatabaseDataType.co_Ueberschrift2, this, _Ueberschrift2, value, true);
                OnChanged();
            }
        }

        public string Ueberschrift3
        {
            get
            {
                return _Ueberschrift3;
            }
            set
            {
                if (_Ueberschrift3 == value) { return; }
                Database.AddPending(enDatabaseDataType.co_Ueberschrift3, this, _Ueberschrift3, value, true);
                OnChanged();
            }
        }



        public Bitmap CaptionBitmap
        {
            get
            {
                return _CaptionBitmap;
            }
            set
            {
                if (modConverter.BitmapToString(_CaptionBitmap, ImageFormat.Png) == modConverter.BitmapToString(value, ImageFormat.Png)) { return; }
                Database.AddPending(enDatabaseDataType.co_CaptionBitmap, this, modConverter.BitmapToString(_CaptionBitmap, ImageFormat.Png), modConverter.BitmapToString(value, ImageFormat.Png), false);


                if (value == null)
                {
                    _CaptionBitmap = null;
                }
                else
                {
                    _CaptionBitmap = (Bitmap)value.Clone();
                }

                Invalidate_TmpVariables();
                OnChanged();
            }
        }


        public string AdminInfo
        {
            get
            {
                return _AdminInfo;
            }
            set
            {
                if (_AdminInfo == value) { return; }
                Database.AddPending(enDatabaseDataType.co_AdminInfo, this, _AdminInfo, value, true);
                OnChanged();
            }
        }

        public List<string> GetUcaseNamesSortedByLenght()
        {

            if (_UcaseNamesSortedByLenght != null) { return _UcaseNamesSortedByLenght; }
            var tmp = Contents(null);


            for (var Z = 0; Z < tmp.Count; Z++)
            {
                tmp[Z] = tmp[Z].Length.Nummer(10) + tmp[Z].ToUpper();
            }


            tmp.Sort();

            for (var Z = 0; Z < tmp.Count; Z++)
            {
                tmp[Z] = tmp[Z].Substring(10);
            }

            _UcaseNamesSortedByLenght = tmp;
            return _UcaseNamesSortedByLenght;

        }

        /// <summary>
        /// Was in Textfeldern oder Datenbankzeilen f�r ein Suffix angezeigt werden soll. Beispiel: mm
        /// </summary>
        public string Suffix
        {
            get
            {
                return _Suffix;
            }
            set
            {
                if (_Suffix == value) { return; }
                Database.AddPending(enDatabaseDataType.co_Suffix, this, _Suffix, value, true);
                OnChanged();
            }
        }


        /// <summary>
        /// Bei dem Datenformat "BildCode" wird dieser String bei der Bildanzeige hinzugef�gt
        /// </summary>
        public string ImagePrefix
        {
            get
            {
                return _ImagePrefix;
            }
            set
            {
                if (_ImagePrefix == value) { return; }
                Database.AddPending(enDatabaseDataType.co_ImagePrefix, this, _ImagePrefix, value, true);
                OnChanged();
            }
        }
        /// <summary>
        /// Bei dem Datenformat "BildCode" wird dieser String bei der Bildanzeige hinzugef�gt
        /// </summary>
        public string ImageSuffix
        {
            get
            {
                return _ImageSuffix;
            }
            set
            {
                if (_ImagePrefix == value) { return; }
                Database.AddPending(enDatabaseDataType.co_ImageSuffix, this, _ImageSuffix, value, true);
                OnChanged();
            }
        }



        public string LinkedDatabaseFile
        {
            get
            {
                return _LinkedDatabaseFile;
            }
            set
            {
                if (_LinkedDatabaseFile == value) { return; }
                Database.AddPending(enDatabaseDataType.co_LinkedDatabase, this, _LinkedDatabaseFile, value, true);
                Invalidate_TmpVariables();
                OnChanged();
            }
        }


        public string LinkedKeyKennung
        {
            get
            {
                return _LinkedKeyKennung;
            }
            set
            {
                if (_LinkedKeyKennung == value) { return; }
                Database.AddPending(enDatabaseDataType.co_LinkKeyKennung, this, _LinkedKeyKennung, value, true);
                OnChanged();
            }
        }

        public string BestFile_StandardSuffix
        {
            get
            {
                return _BestFile_StandardSuffix;
            }
            set
            {
                if (_BestFile_StandardSuffix == value) { return; }
                Database.AddPending(enDatabaseDataType.co_BestFile_StandardSuffix, this, _BestFile_StandardSuffix, value, true);
                OnChanged();
            }
        }

        public string BestFile_StandardFolder
        {
            get
            {
                return _BestFile_StandardFolder;
            }
            set
            {
                if (_BestFile_StandardFolder == value) { return; }
                Database.AddPending(enDatabaseDataType.co_BestFile_StandardFolder, this, _BestFile_StandardFolder, value, true);
                OnChanged();
            }
        }

        public int BildCode_ConstantHeight
        {
            get
            {
                return _BildCode_ConstantHeight;
            }
            set
            {
                if (_BildCode_ConstantHeight == value) { return; }
                Database.AddPending(enDatabaseDataType.co_BildCode_ConstantHeight, this, _BildCode_ConstantHeight.ToString(), value.ToString(), true);
                OnChanged();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Nummer">Muss 1, 2 oder 3 sein</param>
        /// <returns></returns>
        public string Ueberschrift(int Nummer)
        {
            switch (Nummer)
            {
                case 0: return _Ueberschrift1;
                case 1: return _Ueberschrift2;
                case 2: return _Ueberschrift3;
                default:
                    Develop.DebugPrint(enFehlerArt.Warnung, "Nummer " + Nummer + " nicht erlaubt.");
                    return string.Empty;
            }
        }

        public enImageNotFound BildCode_ImageNotFound
        {
            get
            {
                return _BildCode_ImageNotFound;
            }
            set
            {
                if (_BildCode_ImageNotFound == value) { return; }
                Database.AddPending(enDatabaseDataType.co_BildCode_ImageNotFound, this, ((int)_BildCode_ImageNotFound).ToString(), ((int)value).ToString(), true);
                OnChanged();
            }
        }


        public bool AutoFilterErlaubt
        {
            get
            {
                if (!_Format.Autofilter_m�glich()) { return false; }
                return _AutofilterErlaubt;
            }
            set
            {
                if (_AutofilterErlaubt == value) { return; }
                Database.AddPending(enDatabaseDataType.co_AutoFilterErlaubt, this, _AutofilterErlaubt.ToPlusMinus(), value.ToPlusMinus(), true);
                Invalidate_TmpVariables();
                OnChanged();
            }
        }

        public bool AutoFilterErweitertErlaubt
        {
            get
            {
                if (!AutoFilterErlaubt) { return false; }
                return _AutoFilterErweitertErlaubt;
            }
            set
            {
                if (_AutoFilterErweitertErlaubt == value) { return; }
                Database.AddPending(enDatabaseDataType.co_AutoFilterErweitertErlaubt, this, _AutoFilterErweitertErlaubt.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public bool AutofilterTextFilterErlaubt
        {
            get
            {
                if (!AutoFilterErlaubt) { return false; }
                if (!_Format.TextboxEditPossible()) { return false; }
                return _AutofilterTextFilterErlaubt;
            }
            set
            {
                if (_AutofilterTextFilterErlaubt == value) { return; }
                Database.AddPending(enDatabaseDataType.co_AutoFilterTextFilterErlaubt, this, _AutofilterTextFilterErlaubt.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }


        public bool IgnoreAtRowFilter
        {
            get
            {
                if (!_Format.Autofilter_m�glich()) { return true; }
                return _IgnoreAtRowFilter;
            }
            set
            {
                if (_IgnoreAtRowFilter == value) { return; }
                Database.AddPending(enDatabaseDataType.co_BeiZeilenfilterIgnorieren, this, _IgnoreAtRowFilter.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public bool TextBearbeitungErlaubt
        {
            get
            {
                return _TextBearbeitungErlaubt;
            }
            set
            {
                if (_TextBearbeitungErlaubt == value) { return; }
                Database.AddPending(enDatabaseDataType.co_TextBearbeitungErlaubt, this, _TextBearbeitungErlaubt.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public bool SpellCheckingEnabled
        {
            get
            {
                return _SpellCheckingEnabled;
            }
            set
            {
                if (_SpellCheckingEnabled == value) { return; }
                Database.AddPending(enDatabaseDataType.co_SpellCheckingEnabled, this, _SpellCheckingEnabled.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }


        public bool DropdownBearbeitungErlaubt
        {
            get
            {
                return _DropdownBearbeitungErlaubt;
            }
            set
            {
                if (_DropdownBearbeitungErlaubt == value) { return; }
                Database.AddPending(enDatabaseDataType.co_DropdownBearbeitungErlaubt, this, _DropdownBearbeitungErlaubt.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public bool DropdownAllesAbw�hlenErlaubt
        {
            get
            {
                return _DropdownAllesAbw�hlenErlaubt;
            }
            set
            {
                if (_DropdownAllesAbw�hlenErlaubt == value) { return; }
                Database.AddPending(enDatabaseDataType.co_DropdownAllesAbw�hlenErlaubt, this, _DropdownAllesAbw�hlenErlaubt.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public bool DropdownWerteAndererZellenAnzeigen
        {
            get
            {
                return _DropdownWerteAndererZellenAnzeigen;
            }
            set
            {
                if (_DropdownWerteAndererZellenAnzeigen == value) { return; }
                Database.AddPending(enDatabaseDataType.co_DropdownWerteAndererZellenAnzeigen, this, _DropdownWerteAndererZellenAnzeigen.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public bool AfterEdit_QuickSortRemoveDouble
        {
            get
            {
                if (!_MultiLine) { return false; }
                return _AfterEdit_QuickSortRemoveDouble;
            }
            set
            {
                if (_AfterEdit_QuickSortRemoveDouble == value) { return; }
                Database.AddPending(enDatabaseDataType.co_AfterEdit_QuickSortAndRemoveDouble, this, _AfterEdit_QuickSortRemoveDouble.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public bool AfterEdit_DoUCase
        {
            get
            {
                return _AfterEdit_DoUCase;
            }
            set
            {
                if (_AfterEdit_DoUCase == value) { return; }
                Database.AddPending(enDatabaseDataType.co_AfterEdit_DoUcase, this, _AfterEdit_DoUCase.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }


        public bool AfterEdit_AutoCorrect
        {
            get
            {
                return _AfterEdit_AutoCorrect;
            }
            set
            {
                if (_AfterEdit_AutoCorrect == value) { return; }
                Database.AddPending(enDatabaseDataType.co_AfterEdit_AutoCorrect, this, _AfterEdit_AutoCorrect.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }

        public string CellInitValue
        {
            get
            {
                return _CellInitValue;
            }
            set
            {
                if (_CellInitValue == value) { return; }
                Database.AddPending(enDatabaseDataType.co_CellInitValue, this, _CellInitValue, value, true);
                OnChanged();
            }
        }


        public int AfterEdit_Runden
        {
            get
            {
                return _AfterEdit_Runden;
            }
            set
            {
                if (_AfterEdit_Runden == value) { return; }
                Database.AddPending(enDatabaseDataType.co_AfterEdit_Runden, this, _AfterEdit_Runden.ToString(), value.ToString(), true);
                OnChanged();
            }
        }


        public bool CompactView
        {
            get
            {
                return _CompactView;
            }
            set
            {
                if (_CompactView == value) { return; }
                Database.AddPending(enDatabaseDataType.co_CompactView, this, _CompactView.ToPlusMinus(), value.ToPlusMinus(), true);
                Invalidate_ColumAndContent();
                OnChanged();
            }
        }

        public bool ShowUndo
        {
            get
            {
                return _ShowUndo;
            }
            set
            {
                if (_ShowUndo == value) { return; }
                Database.AddPending(enDatabaseDataType.co_ShowUndo, this, _ShowUndo.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }


        public bool ShowMultiLineInOneLine
        {
            get
            {
                if (!_MultiLine) { return false; }
                return _ShowMultiLineInOneLine;
            }
            set
            {
                if (_ShowMultiLineInOneLine == value) { return; }
                Database.AddPending(enDatabaseDataType.co_ShowMultiLineInOneLine, this, _ShowMultiLineInOneLine.ToPlusMinus(), value.ToPlusMinus(), true);
                Invalidate_ColumAndContent();
                OnChanged();
            }
        }


        public string AllowedChars
        {
            get
            {
                return _AllowedChars;
            }
            set
            {
                if (_AllowedChars == value) { return; }
                Database.AddPending(enDatabaseDataType.co_AllowedChars, this, _AllowedChars, value, true);
                OnChanged();
            }
        }


        public enDataFormat Format
        {
            get
            {
                return _Format;
            }
            set
            {
                if (_Format == value) { return; }
                Database.AddPending(enDatabaseDataType.co_Format, this, Convert.ToInt32(_Format).ToString(), Convert.ToInt32(value).ToString(), true);
                Invalidate_ColumAndContent();
                OnChanged();
            }
        }

        public Color ForeColor
        {
            get
            {
                return _ForeColor;
            }
            set
            {
                if (_ForeColor.ToArgb() == value.ToArgb()) { return; }
                Database.AddPending(enDatabaseDataType.co_ForeColor, this, _ForeColor.ToArgb().ToString(), value.ToArgb().ToString(), true);
                OnChanged();
            }
        }


        public Color BackColor
        {
            get
            {
                return _BackColor;
            }
            set
            {
                if (_BackColor.ToArgb() == value.ToArgb()) { return; }
                Database.AddPending(enDatabaseDataType.co_BackColor, this, _BackColor.ToArgb().ToString(), value.ToArgb().ToString(), true);
                OnChanged();
            }
        }


        public bool EditTrotzSperreErlaubt
        {
            get
            {
                return _EditTrotzSperreErlaubt;
            }
            set
            {
                if (_EditTrotzSperreErlaubt == value) { return; }
                Database.AddPending(enDatabaseDataType.co_EditTrotzSperreErlaubt, this, _EditTrotzSperreErlaubt.ToPlusMinus(), value.ToPlusMinus(), true);
                OnChanged();
            }
        }


        public enColumnLineStyle LineLeft
        {
            get
            {
                return _LineLeft;
            }
            set
            {
                if (_LineLeft == value) { return; }
                Database.AddPending(enDatabaseDataType.co_LineLeft, this, Convert.ToInt32(_LineLeft).ToString(), Convert.ToInt32(value).ToString(), true);
                OnChanged();
            }
        }

        public enColumnLineStyle LineRight
        {
            get
            {
                return _LineRight;
            }
            set
            {
                if (_LineRight == value) { return; }
                Database.AddPending(enDatabaseDataType.co_LinieRight, this, Convert.ToInt32(_LineRight).ToString(), Convert.ToInt32(value).ToString(), true);
                OnChanged();
            }
        }

        public bool IsParsing
        {
            get
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Kann nur �ber die Datenbank geparsed werden.");
                return false;
            }
        }




        #endregion


        private Database TMP_LinkedDatabase
        {
            set
            {
                if (value == _TMP_LinkedDatabase) { return; }

                Invalidate_TmpVariables();

                _TMP_LinkedDatabase = value;

                if (_TMP_LinkedDatabase != null)
                {
                    _TMP_LinkedDatabase.RowKeyChanged += _TMP_LinkedDatabase_RowKeyChanged;
                    _TMP_LinkedDatabase.ColumnKeyChanged += _TMP_LinkedDatabase_ColumnKeyChanged;
                    _TMP_LinkedDatabase.ConnectedControlsStopAllWorking += _TMP_LinkedDatabase_ConnectedControlsStopAllWorking;
                    _TMP_LinkedDatabase.Disposed += _TMP_LinkedDatabase_Disposed;
                    _TMP_LinkedDatabase.Cell.CellValueChanged += _TMP_LinkedDatabase_Cell_CellValueChanged;
                }

            }
        }

        private void _TMP_LinkedDatabase_ColumnKeyChanged(object sender, KeyChangedEventArgs e)
        {



            if (_Format != enDataFormat.Columns_f�r_LinkedCellDropdown)
            {
                var os = e.KeyOld.ToString();
                var ns = e.KeyNew.ToString();
                foreach (var ThisRow in Database.Row)
                {
                    if (Database.Cell.GetStringBehindLinkedValue(this, ThisRow) == os)
                    {
                        Database.Cell.SetValueBehindLinkedValue(this, ThisRow, ns, false);
                    }
                }
            }

            if (_Format != enDataFormat.LinkedCell)
            {
                var os = e.KeyOld.ToString() + "|";
                var ns = e.KeyNew.ToString() + "|";
                foreach (var ThisRow in Database.Row)
                {
                    var val = Database.Cell.GetStringBehindLinkedValue(this, ThisRow);
                    if (val.StartsWith(os))
                    {
                        Database.Cell.SetValueBehindLinkedValue(this, ThisRow, val.Replace(os, ns), false);
                    }
                }
            }
        }

        private void _TMP_LinkedDatabase_RowKeyChanged(object sender, KeyChangedEventArgs e)
        {
            if (_Format != enDataFormat.LinkedCell)
            {
                var os = "|" + e.KeyOld.ToString();
                var ns = "|" + e.KeyNew.ToString();
                foreach (var ThisRow in Database.Row)
                {
                    var val = Database.Cell.GetStringBehindLinkedValue(this, ThisRow);
                    if (val.EndsWith(os))
                    {
                        Database.Cell.SetValueBehindLinkedValue(this, ThisRow, val.Replace(os, ns), false);
                    }
                }
            }
        }

        private void _TMP_LinkedDatabase_Cell_CellValueChanged(object sender, CellEventArgs e)
        {

            var tKey = CellCollection.KeyOfCell(e.Column, e.Row);

            foreach (var ThisRow in Database.Row)
            {
                if (Database.Cell.GetStringBehindLinkedValue(this, ThisRow) == tKey)
                {
                    CellCollection.Invalidate_CellContentSize(this, ThisRow);
                    Invalidate_TmpColumnContentWidth();
                    Database.Cell.OnCellValueChanged(new CellEventArgs(this, ThisRow));
                    ThisRow.DoAutomatic(false, true);
                }
            }
        }

        private void _TMP_LinkedDatabase_Disposed(object sender, System.EventArgs e)
        {
            Invalidate_TmpVariables();
        }

        private void _TMP_LinkedDatabase_ConnectedControlsStopAllWorking(object sender, DatabaseStoppedEventArgs e)
        {
            Database.OnConnectedControlsStopAllWorking(e);
        }

        public Database LinkedDatabase()
        {
            if (_TMP_LinkedDatabase != null) { return _TMP_LinkedDatabase; }
            if (string.IsNullOrEmpty(_LinkedDatabaseFile)) { return null; }

            var el = new DatabaseSettingsEventHandler(this, Database.Filename.FilePath() + _LinkedDatabaseFile, Database.ReadOnly, Database._PasswordSub, Database._GenerateLayout, Database._RenameColumnInLayout);
            Database.OnLoadingLinkedDatabase(el);

            TMP_LinkedDatabase = Database.Load(el.Filenname, el.ReadOnly, el.PasswordSub, el.GenenerateLayout, el.RenameColumnInLayout);
            if (_TMP_LinkedDatabase != null) { _TMP_LinkedDatabase.UserGroup = Database.UserGroup; }
            return _TMP_LinkedDatabase;
        }


        public void OnChanged()
        {
            Changed?.Invoke(this, System.EventArgs.Empty);
        }
        internal string ParsableColumnKey()
        {
            return ColumnCollection.ParsableColumnKey(this);
        }




        public List<string> Contents(FilterCollection Filter)
        {
            var list = new List<string>();

            foreach (var ThisRowItem in Database.Row)
            {
                if (ThisRowItem != null)
                {
                    if (ThisRowItem.MatchesTo(Filter))
                    {
                        if (_MultiLine)
                        {
                            list.AddRange(ThisRowItem.CellGetList(this));
                        }
                        else
                        {
                            if (ThisRowItem.CellGetString(this).Length > 0)
                            {
                                list.Add(ThisRowItem.CellGetString(this));
                            }
                        }
                    }
                }
            }

            return list.SortedDistinctList();
        }


        public void DeleteContents(FilterCollection Filter)
        {

            foreach (var ThisRowItem in Database.Row)
            {
                if (ThisRowItem != null && ThisRowItem.MatchesTo(Filter)) { ThisRowItem.CellSet(this, ""); }
            }
        }




        public bool IsFirst()
        {
            return Convert.ToBoolean(Database.Column[0] == this);
        }


        public ColumnItem Previous()
        {

            var ColumnCount = Index();

            do
            {
                ColumnCount -= 1;
                if (ColumnCount < 0) { return null; }
                if (Database.Column[ColumnCount] != null) { return Database.Column[ColumnCount]; }
            } while (true);
        }

        public ColumnItem Next()
        {


            var ColumnCount = Index();

            do
            {
                ColumnCount += 1;
                if (ColumnCount >= Database.Column.Count) { return null; }
                if (Database.Column[ColumnCount] != null) { return Database.Column[ColumnCount]; }

            } while (true);
        }


        internal string Load(enDatabaseDataType Art, string Wert)
        {
            switch (Art)
            {

                case enDatabaseDataType.co_Name:
                    _Name = Wert;
                    Invalidate_TmpVariables();
                    break;
                case enDatabaseDataType.co_Caption:
                    _Caption = Wert;
                    break;
                case enDatabaseDataType.co_Format:
                    _Format = (enDataFormat)int.Parse(Wert);

                    if (_Format == (enDataFormat)65) { _Format = enDataFormat.RelationText; } //TODO: Bei zeiten l�schen (10.12.2018) 

                    break;
                case enDatabaseDataType.co_ForeColor:
                    _ForeColor = Color.FromArgb(int.Parse(Wert));
                    break;
                case enDatabaseDataType.co_BackColor:
                    _BackColor = Color.FromArgb(int.Parse(Wert));
                    break;
                case enDatabaseDataType.co_LineLeft:
                    _LineLeft = (enColumnLineStyle)Convert.ToInt32(Wert);
                    break;
                case enDatabaseDataType.co_LinieRight:
                    _LineRight = (enColumnLineStyle)Convert.ToInt32(Wert);
                    break;
                case enDatabaseDataType.co_QuickInfo:
                    _QuickInfo = Wert;
                    break;
                case enDatabaseDataType.co_Ueberschrift1:
                    _Ueberschrift1 = Wert;
                    break;
                case enDatabaseDataType.co_Ueberschrift2:
                    _Ueberschrift2 = Wert;
                    break;
                case enDatabaseDataType.co_Ueberschrift3:
                    _Ueberschrift3 = Wert;
                    break;
                case enDatabaseDataType.co_CaptionBitmap:
                    _CaptionBitmap = modConverter.StringToBitmap(Wert);
                    break;
                case enDatabaseDataType.co_Identifier:
                    _Identifier = Wert;
                    StandardWerteNachKennungx();
                    Database.Column.GetSystems();
                    break;
                case enDatabaseDataType.co_EditType:
                    _EditType = (enEditTypeFormula)Convert.ToInt32(Wert);
                    break;
                case enDatabaseDataType.co_MultiLine:
                    _MultiLine = Wert.FromPlusMinus();
                    break;
                case enDatabaseDataType.co_DropDownItems:
                    DropDownItems.SplitByCR_QuickSortAndRemoveDouble(Wert);
                    break;
                case enDatabaseDataType.co_Replacer:
                    Replacer.SplitByCR(Wert);
                    break;
                case enDatabaseDataType.co_Tags:
                    Tags.SplitByCR_QuickSortAndRemoveDouble(Wert);
                    break;
                //case enDatabaseDataType.co_TagsInternal_ALT:
                //    break;
                case enDatabaseDataType.co_AutoFilterJoker:
                    _AutoFilterJoker = Wert;
                    break;
                case enDatabaseDataType.co_PermissionGroups_ChangeCell:
                    PermissionGroups_ChangeCell.SplitByCR_QuickSortAndRemoveDouble(Wert);
                    break;
                case enDatabaseDataType.co_AllowedChars:
                    _AllowedChars = Wert;
                    break;
                case enDatabaseDataType.co_AutoFilterErlaubt:
                    _AutofilterErlaubt = Wert.FromPlusMinus();
                    break;
                case enDatabaseDataType.co_AutoFilterTextFilterErlaubt:
                    _AutofilterTextFilterErlaubt = Wert.FromPlusMinus();
                    break;
                case enDatabaseDataType.co_AutoFilterErweitertErlaubt:
                    _AutoFilterErweitertErlaubt = Wert.FromPlusMinus();
                    break;
                case enDatabaseDataType.co_BeiZeilenfilterIgnorieren:
                    _IgnoreAtRowFilter = Wert.FromPlusMinus();
                    break;
                case enDatabaseDataType.co_CompactView:
                    _CompactView = Wert.FromPlusMinus();
                    break;
                case enDatabaseDataType.co_ShowUndo:
                    _ShowUndo = Wert.FromPlusMinus();
                    break;
                case enDatabaseDataType.co_ShowMultiLineInOneLine:
                    _ShowMultiLineInOneLine = Wert.FromPlusMinus();
                    break;
                case enDatabaseDataType.co_TextBearbeitungErlaubt:
                    _TextBearbeitungErlaubt = Wert.FromPlusMinus();
                    break;
                case enDatabaseDataType.co_DropdownBearbeitungErlaubt:
                    _DropdownBearbeitungErlaubt = Wert.FromPlusMinus();
                    break;
                case enDatabaseDataType.co_SpellCheckingEnabled:
                    _SpellCheckingEnabled = Wert.FromPlusMinus();
                    break;
                case enDatabaseDataType.co_DropdownAllesAbw�hlenErlaubt:
                    _DropdownAllesAbw�hlenErlaubt = Wert.FromPlusMinus();
                    break;
                case enDatabaseDataType.co_DropdownWerteAndererZellenAnzeigen:
                    _DropdownWerteAndererZellenAnzeigen = Wert.FromPlusMinus();
                    break;
                case enDatabaseDataType.co_AfterEdit_QuickSortAndRemoveDouble:
                    _AfterEdit_QuickSortRemoveDouble = Wert.FromPlusMinus();
                    break;
                case enDatabaseDataType.co_AfterEdit_Runden:
                    _AfterEdit_Runden = int.Parse(Wert);
                    break;
                case enDatabaseDataType.co_AfterEdit_DoUcase:
                    _AfterEdit_DoUCase = Wert.FromPlusMinus();
                    break;
                case enDatabaseDataType.co_AfterEdit_AutoCorrect:
                    _AfterEdit_AutoCorrect = Wert.FromPlusMinus();
                    break;
                case enDatabaseDataType.co_AdminInfo:
                    _AdminInfo = Wert;
                    break;
                case enDatabaseDataType.co_Suffix:
                    _Suffix = Wert;
                    break;
                case enDatabaseDataType.co_LinkedDatabase:
                    _LinkedDatabaseFile = Wert;
                    break;
                case enDatabaseDataType.co_LinkKeyKennung:
                    _LinkedKeyKennung = Wert;
                    break;
                case enDatabaseDataType.co_BestFile_StandardSuffix:
                    _BestFile_StandardSuffix = Wert;
                    break;
                case enDatabaseDataType.co_BestFile_StandardFolder:
                    _BestFile_StandardFolder = Wert;
                    break;
                case enDatabaseDataType.co_BildCode_ConstantHeight:
                    _BildCode_ConstantHeight = int.Parse(Wert);
                    break;
                case enDatabaseDataType.co_ImagePrefix:
                    _ImagePrefix = Wert;
                    break;
                case enDatabaseDataType.co_ImageSuffix:
                    _ImageSuffix = Wert;
                    break;

                //case (enDatabaseDataType)172:
                //    // TODO: FontSclae, L�schen

                //    break;
                case enDatabaseDataType.co_BildCode_ImageNotFound:
                    if (Wert == "-")
                    {
                        _BildCode_ImageNotFound = enImageNotFound.NoImage;
                    }
                    else
                    {
                        _BildCode_ImageNotFound = (enImageNotFound)int.Parse(Wert);
                    }
                    break;

                case enDatabaseDataType.co_EditTrotzSperreErlaubt:
                    _EditTrotzSperreErlaubt = Wert.FromPlusMinus();
                    break;
                case enDatabaseDataType.co_CellInitValue:
                    _CellInitValue = Wert;

                    break;
                default:
                    if (Art.ToString() == Convert.ToInt32(Art).ToString())
                    {
                        Develop.DebugPrint(enFehlerArt.Info, "Laden von Datentyp '" + Art + "' nicht definiert.<br>Wert: " + Wert + "<br>Datei: " + Database.Filename);
                    }
                    else
                    {
                        return "Interner Fehler: F�r den Datentyp  '" + Art + "'  wurde keine Laderegel definiert.";
                    }

                    break;
            }


            return string.Empty;
        }

        private void StandardWerteNachKennungx()
        {
            if (string.IsNullOrEmpty(_Identifier))
            {
                return;
            }

            _LineLeft = enColumnLineStyle.D�nn;
            _LineRight = enColumnLineStyle.Ohne;
            _ForeColor = Color.FromArgb(0, 0, 0);


            _CaptionBitmap = null;

            switch (_Identifier)
            {
                case "System: Creator":
                    _Name = "SYS_Creator";
                    _Caption = "Ersteller";
                    _DropdownBearbeitungErlaubt = true;
                    _DropdownWerteAndererZellenAnzeigen = true;
                    _SpellCheckingEnabled = false;
                    _Format = enDataFormat.Text_Ohne_Kritische_Zeichen;
                    _ForeColor = Color.FromArgb(0, 0, 128);
                    _BackColor = Color.FromArgb(185, 186, 255);
                    break;

                case "System: Changer":
                    _Name = "SYS_Changer";
                    _Caption = "�nderer";
                    _Format = enDataFormat.Text_Ohne_Kritische_Zeichen;
                    _SpellCheckingEnabled = false;
                    _TextBearbeitungErlaubt = false;
                    _DropdownBearbeitungErlaubt = false;
                    _ShowUndo = false;
                    PermissionGroups_ChangeCell.Clear();
                    _ForeColor = Color.FromArgb(0, 128, 0);
                    _BackColor = Color.FromArgb(185, 255, 185);
                    break;

                case "System: Chapter":
                    _Name = "SYS_Chapter";
                    if (string.IsNullOrEmpty(_Caption)) { _Caption = "Kapitel"; }
                    //_SpellCheckingEnabled = false;
                    _Format = enDataFormat.Text_Ohne_Kritische_Zeichen;
                    if (_MultiLine) { _ShowMultiLineInOneLine = true; }
                    _ForeColor = Color.FromArgb(0, 0, 0);
                    _BackColor = Color.FromArgb(255, 255, 150);
                    _LineLeft = enColumnLineStyle.Dick;
                    break;

                case "System: Date Created":
                    _Name = "SYS_CreateDate";
                    _Caption = "Erstell-Datum";
                    _SpellCheckingEnabled = false;
                    _Format = enDataFormat.Datum_und_Uhrzeit;
                    _ForeColor = Color.FromArgb(0, 0, 128);
                    _BackColor = Color.FromArgb(185, 185, 255);
                    _LineLeft = enColumnLineStyle.Dick;
                    break;

                case "System: Date Changed":
                    _Name = "SYS_ChangeDate";
                    _Caption = "�nder-Datum";
                    _SpellCheckingEnabled = false;
                    _ShowUndo = false;
                    _Format = enDataFormat.Datum_und_Uhrzeit;
                    _TextBearbeitungErlaubt = false;
                    _SpellCheckingEnabled = false;
                    _DropdownBearbeitungErlaubt = false;
                    PermissionGroups_ChangeCell.Clear();
                    _ForeColor = Color.FromArgb(0, 128, 0);
                    _BackColor = Color.FromArgb(185, 255, 185);
                    _LineLeft = enColumnLineStyle.Dick;
                    break;

                case "System: Correct":
                    _Name = "SYS_Correct";
                    _Caption = "Fehlerfrei";
                    _SpellCheckingEnabled = false;
                    _Format = enDataFormat.Bit;
                    _ForeColor = Color.FromArgb(128, 0, 0);
                    _BackColor = Color.FromArgb(255, 185, 185);
                    _LineLeft = enColumnLineStyle.Dick;
                    break;

                case "System: Locked":
                    _Name = "SYS_Locked";
                    _Caption = "Abgeschlossen";
                    _SpellCheckingEnabled = false;
                    _Format = enDataFormat.Bit;
                    _QuickInfo = "Eine abgeschlossene Zeile kann<br>nicht mehr bearbeitet werden.";

                    if (_TextBearbeitungErlaubt || _DropdownBearbeitungErlaubt)
                    {
                        _TextBearbeitungErlaubt = false;
                        _DropdownBearbeitungErlaubt = true;
                        _EditTrotzSperreErlaubt = true;
                    }
                    _ForeColor = Color.FromArgb(128, 0, 0);
                    _BackColor = Color.FromArgb(255, 185, 185);
                    break;

                case "System: State":
                    _Name = "SYS_RowState";
                    _Caption = "veraltet und kann gel�scht werden: Zeilenstand";
                    _Identifier = "";
                    break;

                case "System: ID":
                    _Name = "SYS_ID";
                    _Caption = "veraltet und kann gel�scht werden: Zeilen-ID";
                    _Identifier = "";
                    break;

                case "System: Last Used Layout":
                    _Name = "SYS_Layout";
                    _Caption = "veraltet und kann gel�scht werden:  Letztes Layout";
                    _Identifier = "";
                    break;

                default:
                    Develop.DebugPrint("Unbekannte Kennung: " + _Identifier);
                    break;
            }
        }

        public double? Summe(FilterCollection Filter)
        {
            double summ = 0;

            foreach (var thisrow in Database.Row)
            {
                if (thisrow != null && thisrow.MatchesTo(Filter))
                {
                    if (!thisrow.CellIsNullOrEmpty(this))
                    {
                        if (!thisrow.CellGetString(this).IsDouble()) { return null; }
                        summ += thisrow.CellGetDouble(this);
                    }
                }
            }
            return summ;
        }

        public double? Summe(List<RowItem> sort)
        {
            double summ = 0;

            foreach (var thisrow in sort)
            {
                if (thisrow != null)
                {
                    if (!thisrow.CellIsNullOrEmpty(this))
                    {
                        if (!thisrow.CellGetString(this).IsDouble()) { return null; }
                        summ += thisrow.CellGetDouble(this);
                    }
                }
            }
            return summ;
        }


        public bool ExportableTextformatForLayout()
        {
            return _Format.ExportableForLayout();
        }

        /// <summary>
        /// Der Invalidate, der am meisten invalidiert: Alle tempor�ren Variablen und auch jede Zell-Gr��e der Spalte.
        /// </summary>
        public void Invalidate_ColumAndContent()
        {
            TMP_CaptionText_Size = new SizeF(-1, -1);

            Invalidate_TmpColumnContentWidth();
            Invalidate_TmpVariables();

            foreach (var ThisRow in Database.Row)
            {
                if (ThisRow != null) { CellCollection.Invalidate_CellContentSize(this, ThisRow); }
            }
        }

        /// <summary>
        /// Wenn sich ein Zelleninhalt ver�ndert hat, muss die Spalte neu berechnet werden.
        /// </summary>
        internal void Invalidate_TmpColumnContentWidth()
        {
            TMP_ColumnContentWidth = null;
        }

        internal void Invalidate_TmpVariables()
        {
            TMP_CaptionText_Size = new SizeF(-1, -1);


            if (_TMP_LinkedDatabase != null)
            {
                _TMP_LinkedDatabase.RowKeyChanged -= _TMP_LinkedDatabase_RowKeyChanged;
                _TMP_LinkedDatabase.ColumnKeyChanged -= _TMP_LinkedDatabase_ColumnKeyChanged;
                _TMP_LinkedDatabase.ConnectedControlsStopAllWorking -= _TMP_LinkedDatabase_ConnectedControlsStopAllWorking;
                _TMP_LinkedDatabase.Disposed -= _TMP_LinkedDatabase_Disposed;
                _TMP_LinkedDatabase.Cell.CellValueChanged -= _TMP_LinkedDatabase_Cell_CellValueChanged;
                _TMP_LinkedDatabase = null;
            }

            TMP_ColumnContentWidth = null;
        }

        internal void SaveToByteList(ref List<byte> l)
        {
            Database.SaveToByteList(l, enDatabaseDataType.co_Name, _Name, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_Caption, _Caption, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_Format, Convert.ToInt32(_Format).ToString(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_Ueberschrift1, _Ueberschrift1, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_Ueberschrift2, _Ueberschrift2, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_Ueberschrift3, _Ueberschrift3, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_MultiLine, _MultiLine.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_CellInitValue, _CellInitValue, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_AfterEdit_QuickSortAndRemoveDouble, _AfterEdit_QuickSortRemoveDouble.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_AfterEdit_DoUcase, _AfterEdit_DoUCase.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_AfterEdit_AutoCorrect, _AfterEdit_AutoCorrect.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_AfterEdit_Runden, _AfterEdit_Runden.ToString(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_AutoFilterErlaubt, _AutofilterErlaubt.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_AutoFilterTextFilterErlaubt, _AutofilterTextFilterErlaubt.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_AutoFilterErweitertErlaubt, _AutoFilterErweitertErlaubt.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_AutoFilterJoker, _AutoFilterJoker, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_BeiZeilenfilterIgnorieren, _IgnoreAtRowFilter.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_TextBearbeitungErlaubt, _TextBearbeitungErlaubt.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_SpellCheckingEnabled, _SpellCheckingEnabled.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_CompactView, _CompactView.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_ShowMultiLineInOneLine, _ShowMultiLineInOneLine.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_ShowUndo, _ShowUndo.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_ForeColor, _ForeColor.ToArgb().ToString(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_BackColor, _BackColor.ToArgb().ToString(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_LineLeft, Convert.ToInt32(_LineLeft).ToString(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_LinieRight, Convert.ToInt32(_LineRight).ToString(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_DropdownBearbeitungErlaubt, _DropdownBearbeitungErlaubt.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_DropDownItems, DropDownItems.JoinWithCr(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_Replacer, Replacer.JoinWithCr(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_DropdownAllesAbw�hlenErlaubt, _DropdownAllesAbw�hlenErlaubt.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_DropdownWerteAndererZellenAnzeigen, _DropdownWerteAndererZellenAnzeigen.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_QuickInfo, _QuickInfo, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_AdminInfo, _AdminInfo, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_CaptionBitmap, modConverter.BitmapToString(_CaptionBitmap, ImageFormat.Png), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_AllowedChars, _AllowedChars, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_PermissionGroups_ChangeCell, PermissionGroups_ChangeCell.JoinWithCr(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_EditType, Convert.ToInt32(_EditType).ToString(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_Tags, Tags.JoinWithCr(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_EditTrotzSperreErlaubt, _EditTrotzSperreErlaubt.ToPlusMinus(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_Suffix, _Suffix, Key);

            Database.SaveToByteList(l, enDatabaseDataType.co_LinkedDatabase, _LinkedDatabaseFile, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_LinkKeyKennung, _LinkedKeyKennung, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_BestFile_StandardFolder, _BestFile_StandardFolder, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_BestFile_StandardSuffix, _BestFile_StandardSuffix, Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_BildCode_ConstantHeight, _BildCode_ConstantHeight.ToString(), Key);
            Database.SaveToByteList(l, enDatabaseDataType.co_BildCode_ImageNotFound, ((int)_BildCode_ImageNotFound).ToString(), Key);
            //Database.SaveToByteList(l, enDatabaseDataType.co_ImagePrefix, _ImagePrefix, Key);
            //Database.SaveToByteList(l, enDatabaseDataType.co_ImageSuffix, _ImageSuffix, Key);

            //Kennung UNBEDINGT zum Schluss, damit die Standard-Werte gesetzt werden k�nnen
            Database.SaveToByteList(l, enDatabaseDataType.co_Identifier, _Identifier, Key);
        }






        internal void CheckFormulaEditType()
        {

            if (UserEditDialogTypeInFormula(_EditType)) { return; }// Alles OK!

            for (var z = 0; z <= 999; z++)
            {
                var w = (enEditTypeFormula)z;
                if (w.ToString() != z.ToString())
                {
                    if (UserEditDialogTypeInFormula(w))
                    {
                        _EditType = w;
                        return;
                    }
                }
            }

            _EditType = enEditTypeFormula.None;
        }


        public QuickImage SymbolForReadableText()
        {
            if (this == Database.Column.SysRowChanger) { return QuickImage.Get(enImageCode.Person); }
            if (this == Database.Column.SysRowCreator) { return QuickImage.Get(enImageCode.Person); }


            switch (_Format)
            {
                case enDataFormat.Link_To_Filesystem: return QuickImage.Get(enImageCode.Datei, 16);
                //case enDataFormat.Relation: return QuickImage.Get(enImageCode.Herz, 16);
                case enDataFormat.RelationText: return QuickImage.Get(enImageCode.Herz, 16);
                case enDataFormat.Datum_und_Uhrzeit: return QuickImage.Get(enImageCode.Uhr, 16);
                case enDataFormat.Bit: return QuickImage.Get(enImageCode.H�kchen, 16);
                case enDataFormat.Farbcode: return QuickImage.Get(enImageCode.Pinsel, 16);
                case enDataFormat.Ganzzahl: return QuickImage.Get(enImageCode.Ganzzahl, 16);
                case enDataFormat.Gleitkommazahl: return QuickImage.Get(enImageCode.Gleitkommazahl, 16);
                case enDataFormat.Email: return QuickImage.Get(enImageCode.Brief, 16);
                case enDataFormat.InternetAdresse: return QuickImage.Get(enImageCode.Globus, 16);
                case enDataFormat.Bin�rdaten: return QuickImage.Get(enImageCode.Bin�rdaten, 16);
                case enDataFormat.Bin�rdaten_Bild: return QuickImage.Get(enImageCode.Bild, 16);
                case enDataFormat.BildCode: return QuickImage.Get(enImageCode.Smiley, 16);
                case enDataFormat.Telefonnummer: return QuickImage.Get(enImageCode.Telefon, 16);
                case enDataFormat.LinkedCell: return QuickImage.Get(enImageCode.Fernglas, 16);
                case enDataFormat.Columns_f�r_LinkedCellDropdown: return QuickImage.Get(enImageCode.Fernglas, 16, "FF0000", "");
                case enDataFormat.Values_f�r_LinkedCellDropdown: return QuickImage.Get(enImageCode.Fernglas, 16, "00FF00", "");
            }


            if (_Format.TextboxEditPossible())
            {
                if (_MultiLine) { return QuickImage.Get(enImageCode.Textfeld, 16, "FF0000", ""); }
                return QuickImage.Get(enImageCode.Textfeld);
            }

            return QuickImage.Get("Pfeil_Unten_Scrollbar|14");

        }

        public string ReadableText()
        {
            foreach (var ThisColumnItem in Database.Column)
            {
                if (ThisColumnItem != null)
                {
                    if (ThisColumnItem != this && ThisColumnItem.Caption.ToUpper() == _Caption.ToUpper())
                    {
                        return _Caption.Replace("\r", "; ") + " (" + _Name + ")";
                    }
                }
            }
            return _Caption.Replace("\r", "; ");
        }

        public string CompareKey()
        {
            string tmp = null;

            if (string.IsNullOrEmpty(_Caption))
            {
                tmp = _Name + Constants.FirstSortChar + _Name;
            }
            else
            {
                tmp = _Caption + Constants.FirstSortChar + _Name;
            }


            tmp = tmp.Trim(' ');
            tmp = tmp.TrimStart('-');
            tmp = tmp.Trim(' ');

            return tmp;
        }


        public bool UserEditDialogTypeInFormula(enEditTypeFormula EditType_To_Check)
        {

            switch (_Format)
            {

                case enDataFormat.Text:
                case enDataFormat.Text_Ohne_Kritische_Zeichen:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.Telefonnummer:
                case enDataFormat.Email:
                case enDataFormat.Ganzzahl:
                case enDataFormat.Gleitkommazahl:
                case enDataFormat.InternetAdresse:
                case enDataFormat.Datum_und_Uhrzeit:
                case enDataFormat.BildCode:
                case enDataFormat.LinkedCell:
                case enDataFormat.RelationText:
                    if (_TextBearbeitungErlaubt && EditType_To_Check == enEditTypeFormula.Textfeld) { return true; } // Textfeld immer erlauben auch wenn beide Bearbeitungen nicht erlaubt sind, um die Anzeieg zu gew�hrleisten.
                    if (_MultiLine && EditType_To_Check == enEditTypeFormula.Textfeld_mit_Auswahlknopf) { return false; }
                    if (_DropdownBearbeitungErlaubt && EditType_To_Check == enEditTypeFormula.Textfeld_mit_Auswahlknopf) { return true; }
                    if (_DropdownBearbeitungErlaubt && EditType_To_Check == enEditTypeFormula.Listbox_1_Zeile) { return true; }
                    if (_MultiLine && _DropdownBearbeitungErlaubt && EditType_To_Check == enEditTypeFormula.Listbox_3_Zeilen) { return true; }
                    if (_MultiLine && _DropdownBearbeitungErlaubt && EditType_To_Check == enEditTypeFormula.Listbox_6_Zeilen) { return true; }

                    if (EditType_To_Check == enEditTypeFormula.nur_als_Text_anzeigen) { return true; }

                    return false;

                case enDataFormat.Columns_f�r_LinkedCellDropdown:
                case enDataFormat.Values_f�r_LinkedCellDropdown:
                    if (EditType_To_Check == enEditTypeFormula.Textfeld_mit_Auswahlknopf) { return true; }
                    return false;

                case enDataFormat.Bit:
                    if (_MultiLine) { return false; }
                    if (EditType_To_Check == enEditTypeFormula.Ja_Nein_Knopf)
                    {
                        if (_DropdownWerteAndererZellenAnzeigen) { return false; }
                        if (DropDownItems.Count > 0) { return false; }
                        return true;
                    }
                    if (EditType_To_Check == enEditTypeFormula.Textfeld_mit_Auswahlknopf) { return true; }
                    return false;

                case enDataFormat.Link_To_Filesystem:
                    if (_MultiLine)
                    {
                        //if (EditType_To_Check == enEditType.Listbox) { return true; }
                        if (EditType_To_Check == enEditTypeFormula.Gallery) { return true; }
                    }
                    else
                    {
                        if (EditType_To_Check == enEditTypeFormula.EasyPic) { return true; }
                    }
                    return false;


                case enDataFormat.Bin�rdaten:
                    return false;

                //case enDataFormat.Relation:
                //    switch (EditType_To_Check)
                //    {
                //        case enEditTypeFormula.Listbox_1_Zeile:
                //        case enEditTypeFormula.Listbox_3_Zeilen:
                //        case enEditTypeFormula.Listbox_6_Zeilen:
                //            return true;
                //        default:
                //            return false;
                //    }

                case enDataFormat.Farbcode:
                    if (EditType_To_Check == enEditTypeFormula.Farb_Auswahl_Dialog) { return true; }
                    return false;

                case enDataFormat.Bin�rdaten_Bild:
                    return false;


                case enDataFormat.Schrift:
                    if (EditType_To_Check == enEditTypeFormula.Font_AuswahlDialog) { return true; }
                    return false;

                default:

                    Develop.DebugPrint(_Format);
                    return false;

            }

        }




        public int Index()
        {
            return Database.Column.IndexOf(this);
        }

        public string ErrorReason()
        {
            enEditTypeTable TMP_EditDialog = 0;

            if (Key < 0) { return "Interner Fehler: ID nicht definiert"; }

            if (string.IsNullOrEmpty(_Name)) { return "Spaltenname nicht definiert."; }


            // Diese Routine ist nicht ganz so streng und erlaubgt auch �' und so.
            // Beim Editor eingeben wird das allerdings unterbunden.
            if (!Name.ContainsOnlyChars(AllowedCharsInternalName)) { return "Spaltenname enth�lt ung�ltige Zeichen."; }


            foreach (var ThisColumn in Database.Column)
            {
                if (ThisColumn != this && ThisColumn != null)
                {
                    if (_Name.ToUpper() == ThisColumn.Name.ToUpper()) { return "Spalten-Name bereits vorhanden."; }
                }
            }

            if (string.IsNullOrEmpty(_Caption)) { return "Spalten Beschriftung fehlt."; }


            if (((int)_Format).ToString() == _Format.ToString()) { return "Format fehlerhaft."; }


            TMP_EditDialog = UserEditDialogTypeInTable(_Format, false, true, _MultiLine);


            if (_MultiLine)
            {
                if (!_Format.MultilinePossible()) { return "Format unterst�tzt keine mehrzeiligen Texte."; }
                if (_AfterEdit_Runden != -1) { return "Runden nur bei einzeiligen Texten m�glich"; }
            }
            else
            {
                //if (_Format == enDataFormat.Relation) { return "Bei Beziehungen muss mehrzeilig ausgew�hlt werden."; }
                if (_Format == enDataFormat.RelationText) { return "Bei Beziehungen muss mehrzeilig ausgew�hlt werden."; }
                if (_ShowMultiLineInOneLine) { return "Wenn mehrzeilige Texte einzeilig dargestellt werden sollen, muss mehrzeilig angew�hlt sein."; }
                if (_AfterEdit_QuickSortRemoveDouble) { return "Sortierung kann nur bei mehrzeiligen Feldern erfolgen."; }
            }

            if (_SpellCheckingEnabled && !_Format.SpellCheckingPossible()) { return "Rechtschreibpr�fung bei diesem Format nicht m�glich."; }



            if (_EditTrotzSperreErlaubt && !_TextBearbeitungErlaubt && !_DropdownBearbeitungErlaubt) { return "Wenn die Zeilensperre ignoriert werden soll, muss eine Bearbeitungsmethode definiert sein."; }


            if (_TextBearbeitungErlaubt)
            {
                if (TMP_EditDialog == enEditTypeTable.Dropdown_Single) { return "Format unterst�tzt nur Dropdown-Men�."; }
                if (TMP_EditDialog == enEditTypeTable.None) { return "Format unterst�tzt keine Standard-Bearbeitung."; }

            }
            else
            {
                if (!string.IsNullOrEmpty(_AllowedChars)) { return "'Erlaubte Zeichen' nur bei Texteingabe n�tig."; }
            }

            if (_DropdownBearbeitungErlaubt)
            {
                //if (TMP_EditDialog == enEditTypeTable.RelationEditor_InTable) { return "Format unterst�tzt nur die Standard-Bearbeitung."; }
                if (_SpellCheckingEnabled) { return "Entweder Dropdownmen� oder Rechtschreibpr�fung."; }
                if (TMP_EditDialog == enEditTypeTable.None) { return "Format unterst�tzt keine Auswahlmen�-Bearbeitung."; }
            }


            if (!_DropdownBearbeitungErlaubt && !_TextBearbeitungErlaubt)
            {
                if (PermissionGroups_ChangeCell.Count > 0) { return "Bearbeitungsberechtigungen entfernen, wenn keine Bearbeitung erlaubt ist."; }
            }


            foreach (var thisS in PermissionGroups_ChangeCell)
            {
                if (thisS.Contains("|")) { return "Unerlaubtes Zeichen bei den Gruppen, die eine Zelle bearbeiten d�rfen."; }
                if (thisS.ToUpper() == "#ADMINISTRATOR") { return "'#Administrator' bei den Bearbeitern entfernen."; }
            }

            if (_DropdownBearbeitungErlaubt || TMP_EditDialog == enEditTypeTable.Dropdown_Single)
            {

                if (_Format != enDataFormat.Bit && _Format != enDataFormat.Columns_f�r_LinkedCellDropdown && _Format != enDataFormat.Values_f�r_LinkedCellDropdown)
                {
                    if (!_DropdownWerteAndererZellenAnzeigen && DropDownItems.Count == 0) { return "Keine Dropdown-Items vorhanden bzw. Alles hinzuf�gen nicht angew�hlt."; }
                }

            }
            else
            {

                if (_DropdownWerteAndererZellenAnzeigen) { return "Dropdownmenu nicht ausgew�hlt, 'alles hinzuf�gen' pr�fen."; }
                if (_DropdownAllesAbw�hlenErlaubt) { return "Dropdownmenu nicht ausgew�hlt, 'alles abw�hlen' pr�fen."; }
                if (DropDownItems.Count > 0) { return "Dropdownmenu nicht ausgew�hlt, Dropdown-Items vorhanden."; }

            }


            if (_DropdownWerteAndererZellenAnzeigen && !_Format.DropdownItemsOfOtherCellsAllowed()) { return "'Dropdownmenu alles hinzuf�gen' bei diesem Format nicht erlaubt."; }
            if (_DropdownAllesAbw�hlenErlaubt && !_Format.DropdownUnselectAllAllowed()) { return "'Dropdownmenu alles abw�hlen' bei diesem Format nicht erlaubt."; }
            if (DropDownItems.Count > 0 && !_Format.DropdownItemsAllowed()) { return "Manuelle 'Dropdow-Items' bei diesem Format nicht erlaubt."; }







            if (_Format == enDataFormat.Link_To_Filesystem)
            {
                if (_MultiLine && !_AfterEdit_QuickSortRemoveDouble) { return "Format muss sortiert werden."; }
            }


            if (_Format == enDataFormat.Text_mit_Formatierung)
            {
                if (_AfterEdit_QuickSortRemoveDouble) { return "Format darf nicht sortiert werden."; }
            }


            if (!string.IsNullOrEmpty(_Suffix))
            {
                if (!_Format.IsZahl()) { return "Format unterst�tzt keine Einheit."; }
                if (_MultiLine) { return "Einheiten und Mehrzeilig darf nicht kombiniert werden."; }
            }


            if (_AfterEdit_Runden > 6) { return "Beim Runden maximal 6 Nachkommastellen m�glich"; }


            if (!_AutofilterErlaubt)
            {
                if (!string.IsNullOrEmpty(_AutoFilterJoker)) { return "Wenn kein Autofilter erlaubt ist, immer anzuzeigende Werte entfernen"; }
            }



            if (_Format.NeedTargetDatabase())
            {
                if (LinkedDatabase() == null) { return "Verkn�pfte Datenbank fehlt oder existiert nicht."; }
                if (LinkedDatabase() == Database) { return "Zirkelbezug mit verkn�pfter Datenbank."; }
            }

            if (string.IsNullOrEmpty(_LinkedKeyKennung) && _Format.NeedLinkedKeyKennung()) { return "Spaltenkennung f�r verlinkte Datenbanken fehlt."; }


            if (Replacer.Count > 0)
            {
                if (_Format != enDataFormat.Text &&
                    _Format != enDataFormat.Text_Ohne_Kritische_Zeichen &&
                    _Format != enDataFormat.Columns_f�r_LinkedCellDropdown &&
                    _Format != enDataFormat.BildCode) { return "Format unterst�tzt keine Ersetzungen."; }
            }

            return string.Empty;
        }



        public bool IsOk()
        {
            return string.IsNullOrEmpty(ErrorReason());
        }




        private void Tags_ListOrItemChanged(object sender, System.EventArgs e)
        {
            Database.AddPending(enDatabaseDataType.co_Tags, Key, Tags.JoinWithCr(), false);
        }


        private void DropDownItems_ListOrItemChanged(object sender, System.EventArgs e)
        {
            Database.AddPending(enDatabaseDataType.co_DropDownItems, Key, DropDownItems.JoinWithCr(), false);
        }

        private void Replacer_ListOrItemChanged(object sender, System.EventArgs e)
        {
            Database.AddPending(enDatabaseDataType.co_Replacer, Key, Replacer.JoinWithCr(), false);
        }

        private void PermissionGroups_ChangeCell_ListOrItemChanged(object sender, System.EventArgs e)
        {
            Database.AddPending(enDatabaseDataType.co_PermissionGroups_ChangeCell, Key, PermissionGroups_ChangeCell.JoinWithCr(), false);
        }

        public string FreeFileName(string Wunschname, string Suffix)
        {


            if (string.IsNullOrEmpty(Wunschname))
            {
                Wunschname = "Data" + Key + "-" + DateTime.Now;
                Wunschname = Wunschname.Replace(":", "-").Replace(".", "-").Trim('-').Replace(" ", "_");
            }
            Wunschname = Wunschname.RemoveChars(Constants.Char_DateiSonderZeichen);

            var NeuerName = BestFile(Wunschname); // N�chstbesten Namen holen

            NeuerName = TempFile(NeuerName.FilePath(), NeuerName.FileNameWithoutSuffix(), Suffix); // TempFile k�mmert sich um den Index hinten drann

            return NeuerName;
        }


        public string AutoCorrect(string Value)
        {
            if (_AfterEdit_DoUCase)
            {
                Value = Value.ToUpper();
            }

            if (_AfterEdit_QuickSortRemoveDouble)
            {
                var l = new List<string>(Value.SplitByCR()).SortedDistinctList();
                Value = l.JoinWithCr();
            }

            if (_AfterEdit_AutoCorrect)
            {
                Value = KleineFehlerCorrect(Value);
            }

            if (_AfterEdit_Runden > -1 && double.TryParse(Value, out var erg))
            {
                erg = Math.Round(erg, _AfterEdit_Runden);
                Value = erg.ToString();
            }


            return Value;
        }


        private string KleineFehlerCorrect(string TXT)
        {
            if (string.IsNullOrEmpty(TXT)) { return string.Empty; }

            string oTXT = null;

            const char h4 = (char)1004; // H4 = Normaler Text, nach links rutschen
            const char h3 = (char)1003; // �berschrift
            const char h2 = (char)1002; // �berschrift
            const char h1 = (char)1001; // �berschrift
            const char h7 = (char)1007; // bold

            if (_Format == enDataFormat.Text_mit_Formatierung) { TXT = TXT.HTMLSpecialToNormalChar(); }

            do
            {
                oTXT = TXT;
                if (oTXT.ToLower().Contains(".at")) { break; }
                if (oTXT.ToLower().Contains(".de")) { break; }
                if (oTXT.ToLower().Contains(".com")) { break; }
                if (oTXT.ToLower().Contains("http")) { break; }
                if (oTXT.ToLower().Contains("ftp")) { break; }
                if (oTXT.ToLower().Contains(".xml")) { break; }
                if (oTXT.ToLower().Contains(".doc")) { break; }
                if (oTXT.IsFormat(enDataFormat.Datum_und_Uhrzeit)) { break; }

                TXT = TXT.Replace("\r\n", "\r");

                // 1/2 l Milch
                // 3-5 Stunden
                // 180�C


                // Nach Zahlen KEINE leerzeichen einf�gen. Es gibgt so viele dinge.... 90er Schichtsalat



                TXT = TXT.Insert(" ", ",", "1234567890, \r");
                TXT = TXT.Insert(" ", "!", " !?)\r");
                TXT = TXT.Insert(" ", "?", " !?)\r");
                TXT = TXT.Insert(" ", ".", " 1234567890.!?/)\r");
                TXT = TXT.Insert(" ", ")", " .;!?\r");
                TXT = TXT.Insert(" ", ";", " 1234567890\r");
                TXT = TXT.Insert(" ", ":", "1234567890 \\/\r"); // auch 3:50 Uhr


                // H4= Normaler Text
                TXT = TXT.Replace(" " + h4, h4 + " "); // H4 = Normaler Text, nach links rutschen
                TXT = TXT.Replace("\r" + h4, h4 + "\r");

                // Dei restlichen Hs'
                TXT = TXT.Replace(h3 + " ", " " + h3); // �berschrift, nach Rechts
                TXT = TXT.Replace(h2 + " ", " " + h2); // �berschrift, nach Rechts
                TXT = TXT.Replace(h1 + " ", " " + h1); // �berschrift, nach Rechts
                TXT = TXT.Replace(h7 + " ", " " + h7); // Bold, nach Rechts


                TXT = TXT.Replace(h3 + "\r", "\r" + h3); // �berschrift, nach Rechts
                TXT = TXT.Replace(h2 + "\r", "\r" + h2); // �berschrift, nach Rechts
                TXT = TXT.Replace(h1 + "\r", "\r" + h1); // �berschrift, nach Rechts
                TXT = TXT.Replace(h7 + "\r", "\r" + h7); // Bold, nach Rechts

                TXT = TXT.Replace(h7 + h4.ToString(), h4.ToString());
                TXT = TXT.Replace(h3 + h4.ToString(), h4.ToString());
                TXT = TXT.Replace(h2 + h4.ToString(), h4.ToString());
                TXT = TXT.Replace(h1 + h4.ToString(), h4.ToString());
                TXT = TXT.Replace(h4 + h4.ToString(), h4.ToString());


                TXT = TXT.Replace(" �", "�");
                TXT = TXT.Replace(" .", ".");
                TXT = TXT.Replace(" ,", ",");
                TXT = TXT.Replace(" :", ":");
                TXT = TXT.Replace(" ?", "?");
                TXT = TXT.Replace(" !", "!");
                TXT = TXT.Replace(" )", ")");
                TXT = TXT.Replace("( ", "(");

                TXT = TXT.Replace("/ ", "/");
                TXT = TXT.Replace(" /", "/");


                TXT = TXT.Replace("\r ", "\r");
                TXT = TXT.Replace(" \r", "\r");

                TXT = TXT.Replace("     ", " "); // Wenn das hier nicht da ist, passieren wirklich fehler...
                TXT = TXT.Replace("    ", " ");
                TXT = TXT.Replace("   ", " "); // Wenn das hier nicht da ist, passieren wirklich fehler...
                TXT = TXT.Replace("  ", " ");


                TXT = TXT.Trim(' ');
                TXT = TXT.Trim("\r");
                TXT = TXT.TrimEnd("\t");

            } while (oTXT != TXT);

            if (Format == enDataFormat.Text_mit_Formatierung)
            {
                TXT = TXT.CreateHtmlCodes();
                TXT = TXT.Replace("<br>", "\r");
            }

            return TXT;
        }


        public string NewPureBestFile(string FileNameWithoutPath)
        {
            do
            {
                Database.Reload();
                var n = TempFile(BestFile(FileNameWithoutPath + "_" + DateTime.Now.ToString().ReduceToChars(Constants.Char_Numerals))).FileNameWithoutSuffix();
                if (Database.Row[new FilterItem(this, enFilterType.Istgleich_Gro�KleinEgal, n)] == null) { return n; }

            } while (true);


        }

        public string BestFile(string FileNameWithoutPath)
        {

            if (_Format != enDataFormat.Link_To_Filesystem)
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Nur bei Link_To_Filesystem erlaubt!");
            }

            FileNameWithoutPath = FileNameWithoutPath.RemoveChars(Constants.Char_DateiSonderZeichen);
            if (string.IsNullOrEmpty(FileNameWithoutPath)) { return string.Empty; }

            if (FileNameWithoutPath.Contains("\r"))
            {
                Develop.DebugPrint_NichtImplementiert();
            }


            if (string.IsNullOrEmpty(FileNameWithoutPath.FileSuffix()))
            {
                FileNameWithoutPath = (FileNameWithoutPath + "." + _BestFile_StandardSuffix).TrimEnd(".");
            }


            var Fold = _BestFile_StandardFolder.Trim("\\");
            if (string.IsNullOrEmpty(Fold)) { Fold = "Files"; }


            if (Fold.Substring(1, 1) != ":" && Fold.Substring(0, 1) != "\\")
            {
                Fold = Database.Filename.FilePath() + Fold;
            }

            return Fold.TrimEnd("\\") + "\\" + FileNameWithoutPath;


        }


        public bool AutoFilter_m�glich()
        {
            if (!AutoFilterErlaubt) { return false; }
            return Format.Autofilter_m�glich();
        }


        public List<string> Autofilter_ItemList(FilterCollection vFilter)
        {

            if (vFilter == null || vFilter.Count() < 0) { return Contents(null); }

            var tfilter = new FilterCollection(Database);

            foreach (var ThisFilter in vFilter)
            {
                if (ThisFilter != null && this != ThisFilter.Column) { tfilter.Add(ThisFilter); }
            }

            return Contents(tfilter);

        }


        public static string ForHTMLExport(ColumnItem Column, string Einstiegstext)
        {
            switch (Column.Format)
            {

                case enDataFormat.Text:
                case enDataFormat.Text_Ohne_Kritische_Zeichen:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.Datum_und_Uhrzeit:
                case enDataFormat.Ganzzahl:
                case enDataFormat.Telefonnummer:
                case enDataFormat.Email:
                case enDataFormat.InternetAdresse:
                case enDataFormat.BildCode:
                case enDataFormat.Link_To_Filesystem:
                case enDataFormat.Values_f�r_LinkedCellDropdown:
                case enDataFormat.RelationText:
                    // hier nix.
                    break;


                case enDataFormat.Bit:
                    if (Einstiegstext == true.ToPlusMinus())
                    {
                        return "Ja";
                    }
                    else if (Einstiegstext == false.ToPlusMinus())
                    {
                        return "Nein";
                    }
                    else if (Einstiegstext == "o" || Einstiegstext == "O")
                    {
                        return "Neutral";
                    }
                    else if (Einstiegstext == "?")
                    {
                        return "Unbekannt";
                    }
                    else
                    {
                        return Einstiegstext;
                    }


                case enDataFormat.Gleitkommazahl:
                    Einstiegstext = Einstiegstext.Replace(".", ",");
                    break;

                case enDataFormat.Bin�rdaten_Bild:
                case enDataFormat.Bin�rdaten:
                case enDataFormat.Farbcode:
                    Einstiegstext = "?";
                    break;


                case enDataFormat.LinkedCell:
                    Develop.DebugPrint("Fremdzelle d�rfte hier nicht ankommen");
                    break;


                //case enDataFormat.Relation:
                //    var tmp = new clsRelation(Column, null, Einstiegstext);
                //    return tmp.ReadableText();


                case enDataFormat.Columns_f�r_LinkedCellDropdown:
                    return Draw_FormatedText_TextOf(Einstiegstext, null, Column, enShortenStyle.Unreplaced);

                default:
                    Develop.DebugPrint(Column.Format);
                    return "???";
            }




            Einstiegstext = Einstiegstext.Replace("\r\n", "<br>");
            Einstiegstext = Einstiegstext.Replace("\r", "<br>");
            Einstiegstext = Einstiegstext.Replace("\n", "<br>");
            Einstiegstext = Einstiegstext.Trim();
            Einstiegstext = Einstiegstext.Trim("<br>");
            Einstiegstext = Einstiegstext.Trim();
            Einstiegstext = Einstiegstext.Trim("<br>");

            return Einstiegstext;

        }

        public static string Draw_FormatedText_TextOf(string Txt, QuickImage ImageCode, ColumnItem Column, enShortenStyle Style)
        {
            switch (Column.Format)
            {
                case enDataFormat.Text:
                case enDataFormat.Text_mit_Formatierung:
                case enDataFormat.Text_Ohne_Kritische_Zeichen:
                case enDataFormat.Datum_und_Uhrzeit:
                case enDataFormat.Bin�rdaten:
                case enDataFormat.Link_To_Filesystem:
                case enDataFormat.Telefonnummer:
                case enDataFormat.Email:
                case enDataFormat.InternetAdresse:
                case enDataFormat.Gleitkommazahl:
                case enDataFormat.Ganzzahl:
                case enDataFormat.Values_f�r_LinkedCellDropdown:
                case enDataFormat.RelationText:
                    if (Txt == null || string.IsNullOrEmpty(Txt)) { return string.Empty; }
                    Txt = ColumnReplace(Txt, Column, Style);
                    return Txt.Replace("\r\n", " ");


                case enDataFormat.BildCode:
                    if (Column.CompactView) { return string.Empty; }
                    Txt = ColumnReplace(Txt, Column, Style);
                    return Txt; //modFormat.ForHTMLExportx(Nothing, Format, Txt)


                case enDataFormat.Bit:
                    if (Column.CompactView) { return string.Empty; }
                    return ForHTMLExport(Column, Txt);


                //case enDataFormat.Relation:
                //    if (!string.IsNullOrEmpty(Txt))
                //    {
                //        if (ImageCode != null) { return Txt; }

                //        if (!Txt.Contains("{") && !Txt.Contains("|")) { return Txt; }

                //        var x = new clsRelation(Column, null, Txt);

                //        if (Column.CompactView) { return x.Sec; }
                //        return x.ReadableText();
                //    }
                //    return string.Empty;


                case enDataFormat.Farbcode:
                    if (Column.CompactView) { return string.Empty; }
                    if (!string.IsNullOrEmpty(Txt) && Txt.IsFormat(enDataFormat.Farbcode))
                    {
                        var col = Color.FromArgb(int.Parse(Txt));
                        return col.ColorName();
                    }
                    return Txt;

                case enDataFormat.Bin�rdaten_Bild:
                    return string.Empty;


                case enDataFormat.Schrift:
                    Develop.DebugPrint_NichtImplementiert();
                    //if (string.IsNullOrEmpty(Txt) || Txt.Substring(0, 1) != "{") { return Txt; }

                    //if (Column.CompactView) { return string.Empty; }
                    //return BlueFont.Get(Txt).ReadableText();
                    return string.Empty;

                case enDataFormat.LinkedCell:
                    // Bei LinkedCell kommt direkt der Text der verlinkten Zelle an
                    if (Txt == null || string.IsNullOrEmpty(Txt)) { return string.Empty; }
                    Txt = ColumnReplace(Txt, Column, Style);
                    return Txt.Replace("\r\n", " ");


                case enDataFormat.Columns_f�r_LinkedCellDropdown:
                    // Hier kommt die Spalten-ID  an
                    if (string.IsNullOrEmpty(Txt)) { return string.Empty; }
                    if (!int.TryParse(Txt, out var ColKey)) { return "Columkey kann nicht geparsed werden"; }
                    var LinkedDatabase = Column.LinkedDatabase();
                    if (LinkedDatabase == null) { return "Datenbankverkn�pfung fehlt"; }
                    var C = LinkedDatabase.Column.SearchByKey(ColKey);
                    if (C == null) { return "Columnkey nicht gefunden"; }
                    Txt = ColumnReplace(C.ReadableText(), Column, Style);
                    return Txt;

                default:
                    Develop.DebugPrint(Column.Format);
                    return Txt;
            }


        }


        public static string ColumnReplace(string newTXT, ColumnItem Column, enShortenStyle Style)
        {
            if (Style == enShortenStyle.Unreplaced || Column.Replacer.Count == 0) { return newTXT; }

            var OT = newTXT;

            foreach (var ThisString in Column.Replacer)
            {
                var x = ThisString.SplitBy("|");
                if (x.Length == 2) { newTXT = newTXT.Replace(x[0], x[1]); }
                if (x.Length == 1 && !ThisString.StartsWith("|")) { newTXT = newTXT.Replace(x[0], string.Empty); }
            }

            if (Style == enShortenStyle.Replaced || OT == newTXT) { return newTXT; }
            return OT + " (" + newTXT + ")";
        }

        public static enEditTypeTable UserEditDialogTypeInTable(ColumnItem vColumn, bool DoDropDown)
        {
            return UserEditDialogTypeInTable(vColumn.Format, DoDropDown, vColumn.TextBearbeitungErlaubt, vColumn.MultiLine);
        }

        public static enEditTypeTable UserEditDialogTypeInTable(enDataFormat Format, bool DoDropDown, bool KeybordInputAllowed, bool isMultiline)
        {
            if (!DoDropDown && !KeybordInputAllowed) { return enEditTypeTable.None; }

            switch (Format)
            {
                case enDataFormat.Bin�rdaten:
                    return enEditTypeTable.None;

                case enDataFormat.Bit:
                case enDataFormat.Columns_f�r_LinkedCellDropdown:
                case enDataFormat.Values_f�r_LinkedCellDropdown:
                    return enEditTypeTable.Dropdown_Single;

                case enDataFormat.Bin�rdaten_Bild:
                    return enEditTypeTable.Image_Auswahl_Dialog;

                case enDataFormat.Link_To_Filesystem:
                    return enEditTypeTable.FileHandling_InDateiSystem;

                case enDataFormat.Farbcode:
                    if (DoDropDown) { return enEditTypeTable.Dropdown_Single; }
                    return enEditTypeTable.Farb_Auswahl_Dialog;

                case enDataFormat.Schrift:
                    if (DoDropDown) { return enEditTypeTable.Dropdown_Single; }
                    return enEditTypeTable.Font_AuswahlDialog;

                default:
                    if (Format.TextboxEditPossible())
                    {
                        if (!DoDropDown) { return enEditTypeTable.Textfeld; }

                        if (isMultiline) { return enEditTypeTable.Dropdown_Single; }
                        if (KeybordInputAllowed) { return enEditTypeTable.Textfeld_mit_Auswahlknopf; }
                        return enEditTypeTable.Dropdown_Single;
                    }

                    Develop.DebugPrint(Format);
                    return enEditTypeTable.None;

            }

        }

        public void Parse(string ToParse)
        {
            Develop.DebugPrint(enFehlerArt.Fehler, "Kann nur �ber die Datenbank geparsed werden.");
        }

        public object Clone()
        {
            if (!IsOk())
            {
                Develop.DebugPrint(enFehlerArt.Fehler, "Quell-Spalte fehlerhaft:\r\nQuelle: " + Name + "\r\nFehler: " + ErrorReason());
            }

            return new ColumnItem(this, false);
        }
    }
}