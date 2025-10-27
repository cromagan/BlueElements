// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

namespace BlueTable.Enums;

public static class TableDataTypeExtension {

    #region Methods

    public static bool IsCellValue(this TableDataType type) => (int)type is >= 200 and <= 206;

    public static bool IsColumnTag(this TableDataType type) => (int)type is >= 100 and <= 199;

    public static bool IsCommand(this TableDataType type) => (int)type is >= 220 and <= 230;

    //public static bool IsTableTag(this TableDataType type) => (int)type is < 100 or >= 249;

    public static bool IsHeaderType(this TableDataType type) => type is TableDataType.Version or
                                                                           TableDataType.Werbung or
                                                                           TableDataType.LastEditTimeUTC or
                                                                           TableDataType.LastEditUser or
                                                                           TableDataType.LastEditApp or
                                                                           TableDataType.LastEditMachineName or
                                                                           TableDataType.LastEditID;

    public static bool IsObsolete(this TableDataType type) => (int)type is 0 or 2 or 3 or 16 or 22 or 33 or 34 or 35 or 52 or 53 or 54 or 56 or 58 or 59 or 60 or 61 or 62 or 64 or 65 or 67 or 70 or 73 or 79 or 81 or 103 or 134 or 151 or 153 or 155 or 178 or 185 or 191 or 249;

    public static bool IsUnimportant(this TableDataType type) => type.IsObsolete() ||
                                                                  type is TableDataType.TemporaryTableMasterTimeUTC or
                                                                       TableDataType.TemporaryTableMasterUser or
                                                                       TableDataType.TemporaryTableMasterMachine or
                                                                       TableDataType.TemporaryTableMasterApp or
                                                                       TableDataType.TemporaryTableMasterId or
                                                                       TableDataType.Werbung or
                                                                       TableDataType.LastEditApp or
                                                                       TableDataType.LastEditID or
                                                                       TableDataType.LastEditMachineName or
                                                                       TableDataType.LastEditTimeUTC or
                                                                       TableDataType.LastEditUser or
                                                                       TableDataType.Undo or
                                                                       TableDataType.UndoInOne or
                                                                       TableDataType.EOF or
                                                                       TableDataType.Command_NewStart or
                                                                       TableDataType.SystemValue;

    #endregion

    //public static bool Nameless(this TableDataType type) => type.ToString() == ((int)type).ToString();
}

public enum TableDataType : byte {
    // Enum.TryParse gibt 0 zurück, wenn der Wert nicht erkannt wird

    //Error = 0,
    Version = 1,

    //CryptionState = 2,
    //CryptionTest = 3,
    //InBearbeitung = 3,
    TemporaryTableMasterTimeUTC = 4,

    TemporaryTableMasterUser = 5,
    TemporaryTableMasterMachine = 6,    // Zugriffanzahl = 6
    TemporaryTableMasterApp = 7, //TotalSperrbit = 7
    TemporaryTableMasterId = 8, //TotalSperrer = 8

    //TotalSperrDatum = 9
    Creator = 10,

    CreateDateUTC = 11,

    //Vorschaubild = 12
    Caption = 13,

    //Beschreibung = 14
    //Schlagwörter = 15
    //Formatkennung = 16,

    //FormatVersion = 17
    Tags = 18,

    // ErlaubteBenutzer = 19
    // RowUBound = 20
    GlobalShowPass = 21,

    //FileEncryptionKey = 22,
    //DateiEndung = 23
    //VorgängerDatei = 24
    //UniqueColumns = 25
    SortDefinition = 26,

    Werbung = 27,

    //ColumnUBound = 28
    //LastFileSystem = 29,
    //GlobalInfo = 30,
    PermissionGroupsNewRow = 31,

    ColumnArrangement = 32,

    //Views = 33,
    //LastRowKey = 34,
    //Rules = 35,
    //VorgängerDateien = 37
    TableAdminGroups = 38,

    //TabelleUser = 39
    //BackUpDelete = 40
    //MDBInDays = 41
    //CSVInDays = 42
    //HTMLInDays = 48
    //BackUpDir = 49
    //AdminPass = 50
    //CriticalChangePass = 51
    //Skin = 52,
    //Ansicht = 53,
    //Layouts = 54,

    //BinaryData = 55,
    //AutoExport = 56,
    //BinaryCount = 57,
    //ReloadDelaySecond = 58,
    //LastColumnKey = 59,
    //BinaryDataInOne = 60,
    //JoinTyp = 61,
    //VerwaisteDaten = 62,
    EventScript = 63,

    //GlobalScale = 64,

    //FilterImagePfad = 65,
    RowQuickInfo = 66,

    //RulesScript = 67,
    AdditionalFilesPath = 68,

    StandardFormulaFile = 69,

    //FirstColumn = 70,
    TableVariables = 71,

    EventScriptVersion = 72,
    //NeedsScriptFix = 73,

    /// <summary>
    /// Datum/Uhrzeit, bis zu dem die Undos engelesen und fest verankert wurden.
    /// </summary>
    LastSaveMainFileUtcDate = 74,

    LastEditUser = 75,
    LastEditApp = 76,
    LastEditMachineName = 77,
    LastEditTimeUTC = 78,

    //EventScriptEdited = 79,
    LastEditID = 80,

    //RowColorRules = 81,

    ColumnName = 100,

    ColumnCaption = 101,
    MultiLine = 102,

    //ColumnFunction = 103,
    ForeColor = 104,

    BackColor = 105,
    AdditionalFormatCheck = 106,
    ScriptType = 107,
    LinkedCellFilter = 108, // co_ShowPass = 108

    SaveContent = 109, //ColumnContentWidth = 109, // co_ChangePass = 109
    LineStyleLeft = 110,

    LineStyleRight = 111,

    IsKeyColumn = 112, //ColumnTimeCode = 112, // co_Ausrichtung = 112
    ColumnQuickInfo = 113,

    MaxTextLength = 114, // co_FesteBreite = 114
    MaxCellLength = 115,  // co_Bitlänge = 115
    FixedColumnWidth = 116, // co_FilterOption = 116
    DropDownItems = 117,

    IsFirst = 119, // co_SteuerelementName = 119
    Relationship_to_First = 120, // co_BezugsSpalte = 120
    DefaultRenderer = 121,

    RendererSettings = 122,     // co_ReplaceAnsicht = 122

    RelationType = 123, // co_CellUBound = 123
    Value_for_Chunk = 124,// co_GetFilesAufruf = 124
    PermissionGroupsChangeCell = 125,

    ColumnTags = 126,

    //co_ReserveBits = 127
    //co_FehlerWennLeer = 128
    //co_FehlerWenn = 129
    //co_FehlerWennText = 130
    //co_FehlerWennSpalteBefüllt = 131
    //co_FehlerWennSpalteBefüllt_Spalte = 132
    //co_FehlerWennFalschesFormat = 133
    //co_FehlerBeiUnerlaubtenZeichen = 136
    //  CellInitValue = 134,

    AllowedChars = 135,
    ColumnAdminInfo = 137,
    ColumnSystemInfo = 138,

    //co_AutoFilterErlaubt = 138,
    //co_AutoFilterTextFilterErlaubt_alt = 139,
    //co_AutoFilterErweitertErlaubt_alt = 140,
    EditableWithTextInput = 141,

    EditableWithDropdown = 142,
    DropdownDeselectAllAllowed = 143,
    ShowValuesOfOtherCellsInDropdown = 144,
    SortAndRemoveDoubleAfterEdit = 145,
    IgnoreAtRowFilter = 146,

    //co_EditType = 147,
    //co_SpaltenGröße = 148
    //co_ÜberschriftAnordnung = 149
    //co_ÜberschriftErsatz = 150
    //ColumnIdentify = 151,
    //co_CompactView_alt = 152,
    //ShowMultiLineInOneLine = 153,

    EditAllowedDespiteLock = 154,

    //ShowUndo = 155,
    SpellCheckingEnabled = 156,

    //co_TagsInternal = 157,
    //co_QuickInfoImage = 158
    //co_CaptionBitmapCode = 159,
    //Suffix = 160,

    DoUcaseAfterEdit = 161,
    AutoCorrectAfterEdit = 162,
    AutoFilterJoker = 163,
    AfterEditRound = 164,

    //co_AutoFilterJokerModus = 165
    //co_ID = 166
    LinkedTableTableName = 166,

    //co_LinkKeyKennung = 167,
    //co_BestFile_StandardSuffix = 168,
    //co_BestFile_StandardFolder = 169,
    //ConstantHeightOfImageCode = 170,

    //BehaviorOfImageAndText = 171,

    //co_FontScale = 172,
    //OpticalTextReplace = 173,

    CaptionGroup1 = 174,
    CaptionGroup2 = 175,
    CaptionGroup3 = 176,
    //Prefix = 177,

    //KeyColumnKey = 178,
    //co_LinkedCell_RowKeyIsInColumn = 179,
    ColumnNameOfLinkedTable = 180,

    //co_LinkedCell_ColumnValueFoundIn = 181,
    //co_LinkedCell_ColumnValueAdd = 182,
    //co_LinkedCell_Behaviour = 183,
    //co_DropDownKey = 184,
    //MakeSuggestionFromSameKeyColumn = 185,
    ColumnAlign = 186,

    RegexCheck = 187,
    SortType = 188,

    //co_ZellenZusammenfassen = 189,
    AfterEditAutoRemoveChar = 190,

    //SaveContent = 191,
    //co_AutoFilter_Dauerfilter = 192,
    //co_Intelligenter_Multifilter = 193,
    //ColumnKey = 194,   //    co_DauerFilterPos = 194,

    AutoReplaceAfterEdit = 195,
    FilterOptions = 196,
    CaptionBitmapCode = 197,
    DoOpticalTranslation = 198,
    TextFormatingAllowed = 199,

    //Value_withoutSizeData = 200,

    //Value_withSizeData = 201,
    UTF8Value_withoutSizeData = 202,

    // UTF8Value_withSizeData = 203,
    //Dummy_ce_ValueWithoutSizeUncrypted = 204,

    SystemValue = 205,

    Command_RemoveRow = 220,
    Command_AddRow = 221,

    //dummyCommand_AddUndo = 222,
    Command_RemoveColumn = 223,

    //AddColumnKeyInfo = 224,
    //AddColumnNameInfo = 225,
    //Command_AddColumn = 226,
    //Command_AddColumnByKey = 227,
    Command_AddColumnByName = 228,

    Command_NewStart = 229,

    //UndoCount = 249,
    //PendingsInOne = 250,
    UndoInOne = 251,

    //StatisticInOne = 252
    //Statistic = 253
    Undo = 254,

    EOF = 255
}