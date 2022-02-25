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

using System;

namespace BlueDatabase.Enums {

    public enum enDatabaseDataType : byte {

        // Fehler = 0
        Version = 1,

        CryptionState = 2,
        CryptionTest = 3,

        //InBearbeitung = 3,
        //CheckOutTime = 4,
        //CheckOutUser = 5,
        // Zugriffanzahl = 6
        // TotalSperrbit = 7
        // TotalSperrer = 8
        // TotalSperrDatum = 9
        Creator = 10,

        CreateDate = 11,

        // Vorschaubild = 12
        Caption = 13,

        // Beschreibung = 14
        // Schlagwörter = 15
        Formatkennung = 16,

        // FormatVersion = 17
        Tags = 18,

        // ErlaubteBenutzer = 19
        // RowUBoundx = 20
        GlobalShowPass = 21,

        FileEncryptionKey = 22,

        // DateiEndung = 23
        // VorgängerDateix = 24
        // UniqueColumns = 25
        SortDefinition = 26,

        Werbung = 27,

        //  ColumnUBoundx = 28
        LastFileSystem = 29,

        //GlobalInfo = 30,
        PermissionGroups_NewRow = 31,

        ColumnArrangement = 32,
        Views = 33,

        //[Obsolete]
        //LastRowKey = 34,

        //[Obsolete]
        //Rules_ALT = 35,

        //    VorgängerDateien_ALT = 37
        DatenbankAdmin = 38,

        // DatenbankUser = 39
        // BackUpDelete_ALT = 40
        // MDBInDays_ALT = 41
        //  CSVInDays_ALT = 42
        //   HTMLInDays_ALT = 48
        //  BackUpDir_ALT = 49
        // AdminPass = 50
        // CriticalChangePass = 51
        // Skin = 52,
        Ansicht = 53,

        Layouts = 54,

        //BinaryData = 55,
        AutoExport = 56,

        //BinaryCount = 57,
        ReloadDelaySecond = 58,

        //[Obsolete]
        //LastColumnKey = 59,

        //[Obsolete]
        //BinaryDataInOne = 60,

        //[Obsolete]
        //JoinTyp = 61,

        VerwaisteDaten = 62,

        //ImportScript = 63,
        GlobalScale = 64,

        FilterImagePfad = 65,
        ZeilenQuickInfo = 66,
        RulesScript = 67,
        AdditionaFilesPfad = 68,
        Info_ColumDataSart = 100,
        Info_ColumnDataEnd = 199,
        co_Name = 100,
        co_Caption = 101,
        co_MultiLine = 102,
        co_Format = 103,
        co_ForeColor = 104,
        co_BackColor = 105,

        co_AdditionalCheck = 106,

        co_ScriptType = 107,

        co_LinkedCellFilter = 108, // co_ShowPass = 108

        // co_ChangePass = 109
        co_LineLeft = 110,

        co_LinieRight = 111,

        // co_Ausrichtung = 112
        co_QuickInfo = 113,

        // co_FesteBreite = 114
        // co_Bitlänge = 115
        // co_FilterOption = 116
        co_DropDownItems = 117,

        // co_SteuerelementName = 119
        // co_BezugsSpalte = 120
        // FREI = 121
        // co_ReplaceAnsicht = 122
        // co_CellUBoundx = 123
        // co_GetFilesAufruf = 124
        co_PermissionGroups_ChangeCell = 125,

        co_Tags = 126,

        // co_ReserveBits = 127
        // co_FehlerWennLeer = 128
        // co_FehlerWenn = 129
        // co_FehlerWennText = 130
        // co_FehlerWennSpalteBefüllt = 131
        // co_FehlerWennSpalteBefüllt_Spalte = 132
        // co_FehlerWennFalschesFormat = 133
        // co_FehlerBeiUnerlaubtenZeichen = 136
        co_CellInitValue = 134,

        co_AllowedChars = 135,
        co_AdminInfo = 137,
        co_AutoFilterErlaubt_alt = 138,
        co_AutoFilterTextFilterErlaubt_alt = 139,
        co_AutoFilterErweitertErlaubt_alt = 140,
        co_TextBearbeitungErlaubt = 141,
        co_DropdownBearbeitungErlaubt = 142,
        co_DropdownAllesAbwählenErlaubt = 143,
        co_DropdownWerteAndererZellenAnzeigen = 144,
        co_AfterEdit_QuickSortAndRemoveDouble = 145,
        co_BeiZeilenfilterIgnorieren = 146,
        co_EditType = 147,

        // co_SpaltenGröße = 148
        // co_ÜberschriftAnordnung = 149
        // co_ÜberschriftErsatz = 150
        co_Identifier = 151,

        co_CompactView_alt = 152,
        co_ShowMultiLineInOneLine = 153,
        co_EditTrotzSperreErlaubt = 154,
        co_ShowUndo = 155,
        co_SpellCheckingEnabled = 156,

        //co_TagsInternal_ALT = 157, // TODO: Entfernen
        //    co_QuickInfoImage = 158

        //co_CaptionBitmap = 159,

        co_Suffix = 160,
        co_AfterEdit_DoUcase = 161,
        co_AfterEdit_AutoCorrect = 162,
        co_AutoFilterJoker = 163,
        co_AfterEdit_Runden = 164,

        //co_AutoFilterJokerModus = 165
        //co_ID = 166
        co_LinkedDatabase = 166,

        [Obsolete]
        co_LinkKeyKennung = 167,

        co_BestFile_StandardSuffix = 168,
        co_BestFile_StandardFolder = 169,
        co_BildCode_ConstantHeight = 170,
        co_BildTextVerhalten = 171,

        //co_FontScale = 172,
        co_OpticalReplace = 173,

        co_Ueberschrift1 = 174,
        co_Ueberschrift2 = 175,
        co_Ueberschrift3 = 176,
        co_Prefix = 177,
        co_KeyColumnKey = 178,
        co_LinkedCell_RowKeyIsInColumn = 179,
        co_LinkedCell_ColumnKeyOfLinkedDatabase = 180,

        //[Obsolete]
        //co_LinkedCell_ColumnValueFoundIn = 181,

        //[Obsolete]
        //co_LinkedCell_ColumnValueAdd = 182,

        //co_LinkedCell_Behaviour = 183,
        co_DropDownKey = 184,

        co_VorschlagColumn = 185,
        co_Align = 186,
        co_Regex = 187,
        co_SortType = 188,

        //co_ZellenZusammenfassen = 189,
        co_AutoRemove = 190,

        co_SaveContent = 191,

        //co_AutoFilter_Dauerfilter = 192,
        //co_Intelligenter_Multifilter = 193,
        co_DauerFilterPos = 194,

        co_AfterEdit_AutoReplace = 195,
        co_FilterOptions = 196,
        co_CaptionBitmapTXT = 197,
        co_Translate = 198,
        co_FormatierungErlaubt = 199,

        ce_Value_withoutSizeData = 200,
        ce_Value_withSizeData = 201,

        [Obsolete]
        ce_UTF8Value_withoutSizeData = 202,

        [Obsolete]
        ce_UTF8Value_withSizeData = 203,

        //Dummy_ce_ValueWithoutSizeUncrypted = 204,
        dummyComand_RemoveRow = 220,

        dummyComand_AddRow = 221,

        //	dummyComand_AddUndo = 222,
        dummyComand_RemoveColumn = 223,

        AddColumn = 224,
        UndoCount = 249,

        //PendingsInOne = 250,
        UndoInOne = 251,

        //   StatisticInOneOld = 252
        //StatisticOld = 253
        //UndoOld = 254
        EOF = 255
    }
}