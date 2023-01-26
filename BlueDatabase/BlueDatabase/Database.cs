// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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
using BlueDatabase.Enums;
using BlueDatabase.EventArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using static BlueBasics.Converter;
using BlueBasics.MultiUserFile;
using static BlueBasics.Generic;
using static BlueBasics.IO;
using static BlueDatabase.DatabaseAbstract;
using System.Configuration;

namespace BlueDatabase;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class Database : DatabaseAbstract {

    #region Fields

    public ListExt<WorkItem>? Works;

    #endregion

    #region Constructors

    public Database(ConnectionInfo ci, bool readOnly, NeedPassword? needPassword) : this(ci.AdditionalData, readOnly, false, ci.TableName, needPassword) { }

    public Database(Stream stream, string tablename) : this(stream, string.Empty, true, false, tablename, null) { }

    public Database(bool readOnly, string tablename) : this(null, string.Empty, readOnly, true, tablename, null) { }

    public Database(string filename, bool readOnly, bool create, string tablename, NeedPassword? needPassword) : this(null, filename, readOnly, create, tablename, needPassword) { }

    private Database(Stream? stream, string filename, bool readOnly, bool create, string tablename, NeedPassword? needPassword) : base(tablename, readOnly) {
        //Develop.StartService();

        Works = new ListExt<WorkItem>();

        Initialize();

        if (!string.IsNullOrEmpty(filename)) {
            //DropConstructorMessage?.Invoke(this, new MessageEventArgs(enFehlerArt.Info, "Lade Datenbank aus Dateisystem: \r\n" + filename.FileNameWithoutSuffix()));
            LoadFromFile(filename, create, needPassword);
        } else if (stream != null) {
            LoadFromStream(stream);
        }
    }

    #endregion

    #region Properties

    public static string DatabaseId => typeof(Database).Name;

    public override ConnectionInfo ConnectionData => new(TableName, this, DatabaseId, Filename);
    public string Filename { get; set; } = string.Empty;

    #endregion

    #region Methods

    public static void Parse(byte[] bLoaded, ref int pointer, out DatabaseDataType type, out long colKey, out long rowKey, out string value, out string colName) {
        colName = string.Empty;
        colKey = -1;
        rowKey = -1;

        switch ((Routinen)bLoaded[pointer]) {
            case Routinen.CellFormat: {
                    type = (DatabaseDataType)bLoaded[pointer + 1];
                    var les = NummerCode3(bLoaded, pointer + 2);
                    colKey = NummerCode3(bLoaded, pointer + 5);
                    rowKey = NummerCode3(bLoaded, pointer + 8);
                    var b = new byte[les];
                    Buffer.BlockCopy(bLoaded, pointer + 11, b, 0, les);
                    value = b.ToStringWin1252();
                    //var width = NummerCode2(bLoaded, pointer + 11 + les);
                    //var height = NummerCode2(bLoaded, pointer + 11 + les + 2);
                    pointer += 11 + les + 4;
                    break;
                }
            case Routinen.CellFormatUTF8: {
                    type = (DatabaseDataType)bLoaded[pointer + 1];
                    var les = NummerCode3(bLoaded, pointer + 2);
                    colKey = NummerCode3(bLoaded, pointer + 5);
                    rowKey = NummerCode3(bLoaded, pointer + 8);
                    var b = new byte[les];
                    Buffer.BlockCopy(bLoaded, pointer + 11, b, 0, les);
                    value = b.ToStringUtf8();
                    //var width = NummerCode2(bLoaded, pointer + 11 + les);
                    // var height = NummerCode2(bLoaded, pointer + 11 + les + 2);
                    pointer += 11 + les + 4;
                    break;
                }
            case Routinen.CellFormatUTF8_V400: {
                    type = (DatabaseDataType)bLoaded[pointer + 1];
                    var les = NummerCode3(bLoaded, pointer + 2);
                    colKey = NummerCode7(bLoaded, pointer + 5);
                    rowKey = NummerCode7(bLoaded, pointer + 12);
                    var b = new byte[les];
                    Buffer.BlockCopy(bLoaded, pointer + 19, b, 0, les);
                    value = b.ToStringUtf8();
                    //var  width = NummerCode2(bLoaded, pointer + 19 + les);
                    //var  height = NummerCode2(bLoaded, pointer + 19 + les + 2);
                    pointer += 19 + les + 4;
                    break;
                }
            case Routinen.CellFormatUTF8_V401: {
                    type = (DatabaseDataType)bLoaded[pointer + 1];
                    var les = NummerCode3(bLoaded, pointer + 2);
                    rowKey = NummerCode7(bLoaded, pointer + 5);
                    var b = new byte[les];
                    Buffer.BlockCopy(bLoaded, pointer + 12, b, 0, les);
                    value = b.ToStringUtf8();
                    pointer += 12 + les;
                    colKey = -1;
                    break;
                }

            case Routinen.DatenAllgemein: {
                    type = (DatabaseDataType)bLoaded[pointer + 1];
                    var les = NummerCode3(bLoaded, pointer + 2);
                    colKey = -1;
                    rowKey = -1;
                    var b = new byte[les];
                    Buffer.BlockCopy(bLoaded, pointer + 5, b, 0, les);
                    value = b.ToStringWin1252();
                    //width = 0;
                    //height = 0;
                    pointer += 5 + les;
                    break;
                }
            case Routinen.DatenAllgemeinUTF8: {
                    type = (DatabaseDataType)bLoaded[pointer + 1];
                    var les = NummerCode3(bLoaded, pointer + 2);
                    colKey = -1;
                    rowKey = -1;
                    var b = new byte[les];
                    Buffer.BlockCopy(bLoaded, pointer + 5, b, 0, les);
                    value = b.ToStringUtf8();
                    //width = 0;
                    //height = 0;
                    pointer += 5 + les;
                    break;
                }
            case Routinen.Column: {
                    type = (DatabaseDataType)bLoaded[pointer + 1];
                    var les = NummerCode3(bLoaded, pointer + 2);
                    colKey = NummerCode3(bLoaded, pointer + 5);
                    rowKey = NummerCode3(bLoaded, pointer + 8);
                    var b = new byte[les];
                    Buffer.BlockCopy(bLoaded, pointer + 11, b, 0, les);
                    value = b.ToStringWin1252();
                    //width = 0;
                    //height = 0;
                    pointer += 11 + les;
                    break;
                }
            case Routinen.ColumnUTF8: {
                    type = (DatabaseDataType)bLoaded[pointer + 1];
                    var les = NummerCode3(bLoaded, pointer + 2);
                    colKey = NummerCode3(bLoaded, pointer + 5);
                    rowKey = NummerCode3(bLoaded, pointer + 8);
                    var b = new byte[les];
                    Buffer.BlockCopy(bLoaded, pointer + 11, b, 0, les);
                    value = b.ToStringUtf8();
                    //width = 0;
                    //height = 0;
                    pointer += 11 + les;
                    break;
                }
            case Routinen.ColumnUTF8_V400: {
                    type = (DatabaseDataType)bLoaded[pointer + 1];
                    var les = NummerCode3(bLoaded, pointer + 2);
                    colKey = NummerCode7(bLoaded, pointer + 5);
                    rowKey = NummerCode7(bLoaded, pointer + 12);
                    var b = new byte[les];
                    Buffer.BlockCopy(bLoaded, pointer + 19, b, 0, les);
                    value = b.ToStringUtf8();
                    //width = 0;
                    //height = 0;
                    pointer += 19 + les;
                    break;
                }
            case Routinen.ColumnUTF8_V401: {
                    type = (DatabaseDataType)bLoaded[pointer + 1];

                    var cles = NummerCode1(bLoaded, pointer + 2);
                    var cb = new byte[cles];
                    Buffer.BlockCopy(bLoaded, pointer + 3, cb, 0, cles);
                    colName = cb.ToStringUtf8();

                    var les = NummerCode3(bLoaded, pointer + 3 + cles);
                    var b = new byte[les];
                    Buffer.BlockCopy(bLoaded, pointer + 6+ cles, b, 0, les);
                    value = b.ToStringUtf8();

                    pointer += 6 + les + cles;
                    break;
                }

            default: {
                    type = (DatabaseDataType)0;
                    value = string.Empty;
                    //width = 0;
                    //height = 0;
                    Develop.DebugPrint(FehlerArt.Fehler, "Laderoutine nicht definiert: " + bLoaded[pointer]);
                    break;
                }
        }
    }

    public static void Parse(byte[] data, DatabaseAbstract db, ListExt<WorkItem>? works, NeedPassword? needPassword) {
        db.Column.ThrowEvents = false;
        db.Row.ThrowEvents = false;
        var pointer = 0;

        ColumnItem? column = null;
        RowItem? row = null;

        //long colKey = -1;
        //long rowKey = -1;
        //string columname = string.Empty;

        var columnUsed = new List<ColumnItem>();

        works?.Clear();

        do {
            if (pointer >= data.Length) { break; }

            Parse(data, ref pointer, out var art, out var colKey, out var rowKey, out var inhalt, out var columname);
            //Console.WriteLine(art);
            if (!art.IsObsolete()) {

                #region Zeile suchen oder erstellen

                if (rowKey > -1) {
                    row = db.Row.SearchByKey(rowKey);
                    if (row == null) {
                        db.Row.SetValueInternal(DatabaseDataType.Comand_AddRow, rowKey, true);
                        row = db.Row.SearchByKey(rowKey);
                    }
                    if (row == null) {
                        Develop.DebugPrint(FehlerArt.Fehler, "Zeile hinzufügen Fehler");
                        db.SetReadOnly();
                        return;
                    }
                    row.IsInCache = DateTime.UtcNow;
                }

                #endregion

                #region Spalte suchen oder erstellen

                if (colKey > -1 && string.IsNullOrEmpty(columname)) {
                    column = db.Column.SearchByKey(colKey);
                    if (column == null) {
                        if (art != DatabaseDataType.ColumnName) { Develop.DebugPrint(art + " an erster Stelle!"); }
                        db.Column.SetValueInternal(DatabaseDataType.Comand_AddColumnByKey, colKey, true, string.Empty);
                        column = db.Column.SearchByKey(colKey);
                    }
                    if (column == null) {
                        Develop.DebugPrint(FehlerArt.Fehler, "Spalte hinzufügen Fehler");
                        db.SetReadOnly();
                        return;
                    }
                    column.IsInCache = DateTime.UtcNow;
                    columnUsed.Add(column);
                }

                if (colKey < 0 && !string.IsNullOrEmpty(columname)) {
                    column = db.Column.Exists(columname);
                    if (column == null) {
                        if (art != DatabaseDataType.ColumnName) { Develop.DebugPrint(art + " an erster Stelle!"); }
                        db.Column.SetValueInternal(DatabaseDataType.Comand_AddColumnByName, db.Column.NextColumnKey(), true, columname);
                        column = db.Column.Exists(columname);
                    }
                    if (column == null) {
                        Develop.DebugPrint(FehlerArt.Fehler, "Spalte hinzufügen Fehler");
                        db.SetReadOnly();
                        return;
                    }
                    column.IsInCache = DateTime.UtcNow;
                    columnUsed.Add(column);
                }

                #endregion

                #region Bei verschlüsselten Datenbanken das Passwort abfragen

                if (art == DatabaseDataType.GlobalShowPass && !string.IsNullOrEmpty(inhalt)) {
                    var pwd = string.Empty;

                    if (needPassword != null) { pwd = needPassword(); }
                    if (pwd != inhalt) {
                        db.SetReadOnly();
                        break;
                    }
                }

                #endregion

                var fehler = db.SetValueInternal(art, inhalt, column?.Name, row?.Key, true);

                if (art == DatabaseDataType.EOF) { break; }

                if (!string.IsNullOrEmpty(fehler)) {
                    db.SetReadOnly();
                    Develop.DebugPrint("Schwerer Datenbankfehler:<br>Version: " + DatabaseVersion + "<br>Datei: " + db.TableName + "<br>Meldung: " + fehler);
                }
            }
        } while (true);

        #region unbenutzte (gelöschte) Spalten entfernen

        var l = new List<ColumnItem>();
        foreach (var thisColumn in db.Column) {
            l.Add(thisColumn);
        }

        foreach (var thisColumn in l) {
            if (!columnUsed.Contains(thisColumn)) {
                db.SetValueInternal(DatabaseDataType.Comand_RemoveColumn, thisColumn.Key.ToString(), thisColumn.Name, null, true);
            }
        }

        #endregion

        db.Row.RemoveNullOrEmpty();
        db.Cell.RemoveOrphans();
        //Works?.AddRange(oldPendings);
        //oldPendings?.Clear();
        //ExecutePending();
        db.Column.ThrowEvents = true;
        db.Row.ThrowEvents = true;

        //if (db != null && db.Column.Count > 0 && string.IsNullOrEmpty(db.FirstColumn)) {
        //    db.FirstColumn = Col
        //}

        if (IntParse(db.LoadedVersion.Replace(".", "")) > IntParse(DatabaseVersion.Replace(".", ""))) { db.SetReadOnly(); }
    }

    public static void SaveToByteList(ColumnItem c, ref List<byte> l) {
        var name = c.Name;

        SaveToByteList(l, DatabaseDataType.ColumnName, c.Name, name);
        SaveToByteList(l, DatabaseDataType.ColumnCaption, c.Caption, name);
        SaveToByteList(l, DatabaseDataType.ColumnFormat, ((int)c.Format).ToString(), name);
        SaveToByteList(l, DatabaseDataType.CaptionGroup1, c.CaptionGroup1, name);
        SaveToByteList(l, DatabaseDataType.CaptionGroup2, c.CaptionGroup2, name);
        SaveToByteList(l, DatabaseDataType.CaptionGroup3, c.CaptionGroup3, name);
        SaveToByteList(l, DatabaseDataType.MultiLine, c.MultiLine.ToPlusMinus(), name);
        SaveToByteList(l, DatabaseDataType.CellInitValue, c.CellInitValue, name);
        SaveToByteList(l, DatabaseDataType.SortAndRemoveDoubleAfterEdit, c.AfterEditQuickSortRemoveDouble.ToPlusMinus(), name);
        SaveToByteList(l, DatabaseDataType.DoUcaseAfterEdit, c.AfterEditDoUCase.ToPlusMinus(), name);
        SaveToByteList(l, DatabaseDataType.AutoCorrectAfterEdit, c.AfterEditAutoCorrect.ToPlusMinus(), name);
        SaveToByteList(l, DatabaseDataType.RoundAfterEdit, c.AfterEditRunden.ToString(), name);
        SaveToByteList(l, DatabaseDataType.AutoRemoveCharAfterEdit, c.AutoRemove, name);
        SaveToByteList(l, DatabaseDataType.SaveContent, c.SaveContent.ToPlusMinus(), name);
        SaveToByteList(l, DatabaseDataType.FilterOptions, ((int)c.FilterOptions).ToString(), name);
        SaveToByteList(l, DatabaseDataType.AutoFilterJoker, c.AutoFilterJoker, name);
        SaveToByteList(l, DatabaseDataType.IgnoreAtRowFilter, c.IgnoreAtRowFilter.ToPlusMinus(), name);
        SaveToByteList(l, DatabaseDataType.EditableWithTextInput, c.TextBearbeitungErlaubt.ToPlusMinus(), name);
        SaveToByteList(l, DatabaseDataType.SpellCheckingEnabled, c.SpellCheckingEnabled.ToPlusMinus(), name);
        SaveToByteList(l, DatabaseDataType.ShowMultiLineInOneLine, c.ShowMultiLineInOneLine.ToPlusMinus(), name);
        SaveToByteList(l, DatabaseDataType.ShowUndo, c.ShowUndo.ToPlusMinus(), name);
        SaveToByteList(l, DatabaseDataType.TextFormatingAllowed, c.FormatierungErlaubt.ToPlusMinus(), name);
        SaveToByteList(l, DatabaseDataType.ForeColor, c.ForeColor.ToArgb().ToString(), name);
        SaveToByteList(l, DatabaseDataType.BackColor, c.BackColor.ToArgb().ToString(), name);
        SaveToByteList(l, DatabaseDataType.LineStyleLeft, ((int)c.LineLeft).ToString(), name);
        SaveToByteList(l, DatabaseDataType.LineStyleRight, ((int)c.LineRight).ToString(), name);
        SaveToByteList(l, DatabaseDataType.EditableWithDropdown, c.DropdownBearbeitungErlaubt.ToPlusMinus(), name);
        SaveToByteList(l, DatabaseDataType.DropDownItems, c.DropDownItems.JoinWithCr(), name);
        SaveToByteList(l, DatabaseDataType.LinkedCellFilter, c.LinkedCellFilter.JoinWithCr(), name);
        SaveToByteList(l, DatabaseDataType.OpticalTextReplace, c.OpticalReplace.JoinWithCr(), name);
        SaveToByteList(l, DatabaseDataType.AutoReplaceAfterEdit, c.AfterEditAutoReplace.JoinWithCr(), name);
        SaveToByteList(l, DatabaseDataType.RegexCheck, c.Regex, name);
        SaveToByteList(l, DatabaseDataType.DropdownDeselectAllAllowed, c.DropdownAllesAbwählenErlaubt.ToPlusMinus(), name);
        SaveToByteList(l, DatabaseDataType.ShowValuesOfOtherCellsInDropdown, c.DropdownWerteAndererZellenAnzeigen.ToPlusMinus(), name);
        SaveToByteList(l, DatabaseDataType.ColumnQuickInfo, c.Quickinfo, name);
        SaveToByteList(l, DatabaseDataType.ColumnAdminInfo, c.AdminInfo, name);
        SaveToByteList(l, DatabaseDataType.ColumnContentWidth, c.ContentWidth.ToString(), name);
        SaveToByteList(l, DatabaseDataType.CaptionBitmapCode, c.CaptionBitmapCode, name);
        SaveToByteList(l, DatabaseDataType.AllowedChars, c.AllowedChars, name);
        SaveToByteList(l, DatabaseDataType.MaxTextLenght, c.MaxTextLenght.ToString(), name);
        SaveToByteList(l, DatabaseDataType.PermissionGroupsChangeCell, c.PermissionGroupsChangeCell.JoinWithCr(), name);
        SaveToByteList(l, DatabaseDataType.ColumnTags, c.Tags.JoinWithCr(), name);
        SaveToByteList(l, DatabaseDataType.EditAllowedDespiteLock, c.EditAllowedDespiteLock.ToPlusMinus(), name);
        SaveToByteList(l, DatabaseDataType.Suffix, c.Suffix, name);
        SaveToByteList(l, DatabaseDataType.LinkedDatabase, c.LinkedDatabaseFile, name);
        SaveToByteList(l, DatabaseDataType.ConstantHeightOfImageCode, c.ConstantHeightOfImageCode, name);
        SaveToByteList(l, DatabaseDataType.BehaviorOfImageAndText, ((int)c.BehaviorOfImageAndText).ToString(), name);
        SaveToByteList(l, DatabaseDataType.DoOpticalTranslation, ((int)c.DoOpticalTranslation).ToString(), name);
        SaveToByteList(l, DatabaseDataType.AdditionalFormatCheck, ((int)c.AdditionalFormatCheck).ToString(), name);
        SaveToByteList(l, DatabaseDataType.ScriptType, ((int)c.ScriptType).ToString(), name);
        SaveToByteList(l, DatabaseDataType.Prefix, c.Prefix, name);
        //SaveToByteList(l, DatabaseDataType.KeyColumnKey, c.KeyColumnKey.ToString(), key);
        SaveToByteList(l, DatabaseDataType.ColumnNameOfLinkedDatabase, c.LinkedCell_ColumnNameOfLinkedDatabase, name);
        //SaveToByteList(l, DatabaseDataType.MakeSuggestionFromSameKeyColumn, c.VorschlagsColumn.ToString(), key);
        SaveToByteList(l, DatabaseDataType.ColumnAlign, ((int)c.Align).ToString(), name);
        SaveToByteList(l, DatabaseDataType.SortType, ((int)c.SortType).ToString(), name);
        //SaveToByteList(l, DatabaseDataType.ColumnTimeCode, c.TimeCode, key);

        foreach (var thisR in c.Database.Row) {
            SaveToByteList(l, c, thisR);
        }
    }

    public static List<byte> ToListOfByte(DatabaseAbstract db, ListExt<WorkItem>? works) {
        try {
            List<byte> l = new();
            // Wichtig, Reihenfolge und Länge NIE verändern!
            SaveToByteList(l, DatabaseDataType.Formatkennung, "BlueDatabase");
            SaveToByteList(l, DatabaseDataType.Version, DatabaseVersion);
            SaveToByteList(l, DatabaseDataType.Werbung, "                                                                    BlueDataBase - (c) by Christian Peter                                                                                        "); // Die Werbung dient als Dummy-Platzhalter, falls doch mal was vergessen wurde...
            // Passwörter ziemlich am Anfang speicher, dass ja keinen Weiteren Daten geladen werden können
            //if (string.IsNullOrEmpty(GlobalShowPass)) {
            //    SaveToByteList(l, DatabaseDataType.CryptionState, false.ToPlusMinus());
            //} else {
            //    SaveToByteList(l, DatabaseDataType.CryptionState, true.ToPlusMinus());
            //    SaveToByteList(l, DatabaseDataType.CryptionTest, "OK");
            //}
            SaveToByteList(l, DatabaseDataType.GlobalShowPass, db.GlobalShowPass);
            //SaveToByteList(l, DatabaseDataType.FileEncryptionKey, _fileEncryptionKey);
            SaveToByteList(l, DatabaseDataType.Creator, db.Creator);
            SaveToByteList(l, DatabaseDataType.CreateDateUTC, db.CreateDate);
            SaveToByteList(l, DatabaseDataType.Caption, db.Caption);
            //SaveToByteList(l, enDatabaseDataType.JoinTyp, ((int)_JoinTyp).ToString());
            //SaveToByteList(l, DatabaseDataType.VerwaisteDaten, ((int)_verwaisteDaten).ToString());
            SaveToByteList(l, DatabaseDataType.Tags, db.Tags.JoinWithCr());
            SaveToByteList(l, DatabaseDataType.PermissionGroupsNewRow, db.PermissionGroupsNewRow.JoinWithCr());
            SaveToByteList(l, DatabaseDataType.DatabaseAdminGroups, db.DatenbankAdmin.JoinWithCr());
            SaveToByteList(l, DatabaseDataType.GlobalScale, db.GlobalScale.ToString(Constants.Format_Float1));
            //SaveToByteList(l, DatabaseDataType.Ansicht, ((int)_ansicht).ToString());
            //SaveToByteList(l, DatabaseDataType.ReloadDelaySecond, ReloadDelaySecond.ToString());
            //SaveToByteList(l, enDatabaseDataType.ImportScript, _ImportScript);
            SaveToByteList(l, DatabaseDataType.RulesScript, db.RulesScript);
            //SaveToByteList(l, enDatabaseDataType.BinaryDataInOne, Bins.ToString(true));
            //SaveToByteList(l, enDatabaseDataType.FilterImagePfad, _filterImagePfad);
            SaveToByteList(l, DatabaseDataType.AdditionalFilesPath, db.AdditionalFilesPfad);
            //SaveToByteList(l, DatabaseDataType.FirstColumn, db.FirstColumn);
            SaveToByteList(l, DatabaseDataType.RowQuickInfo, db.ZeilenQuickInfo);
            SaveToByteList(l, DatabaseDataType.StandardFormulaFile, db.StandardFormulaFile);
            SaveToByteList(l, db.Column);
            //Row.SaveToByteList(l);
            //SaveToByteList(l, db.Cell, db);
            if (db.SortDefinition == null) {
                // Ganz neue Datenbank
                SaveToByteList(l, DatabaseDataType.SortDefinition, string.Empty);
            } else {
                SaveToByteList(l, DatabaseDataType.SortDefinition, db.SortDefinition.ToString());
            }
            //SaveToByteList(l, enDatabaseDataType.Rules_ALT, Rules.ToString(true));
            SaveToByteList(l, DatabaseDataType.ColumnArrangement, db.ColumnArrangements.ToString());
            SaveToByteList(l, DatabaseDataType.Layouts, db.Layouts.JoinWithCr());
            SaveToByteList(l, DatabaseDataType.AutoExport, db.Export.ToString(true));
            // Beim Erstellen des Undo-Speichers die Works nicht verändern, da auch bei einem nicht
            // erfolgreichen Speichervorgang der Datenbank-String erstellt wird.
            // Status des Work-Items ist egal, da es beim LADEN automatisch auf 'Undo' gesetzt wird.
            List<string> works2 = new();
            if (works != null) {
                foreach (var thisWorkItem in works) {
                    if (thisWorkItem != null) {
                        if (thisWorkItem.Comand != DatabaseDataType.Value_withoutSizeData) {
                            works2.Add(thisWorkItem.ToString());
                        } else {
                            if (thisWorkItem.LogsUndo(db)) {
                                works2.Add(thisWorkItem.ToString());
                            }
                        }
                    }
                }
            }

            SaveToByteList(l, DatabaseDataType.UndoCount, db.UndoCount.ToString());
            if (works2.Count > db.UndoCount) { works2.RemoveRange(0, works2.Count - db.UndoCount); }
            SaveToByteList(l, DatabaseDataType.UndoInOne, works2.JoinWithCr((int)(16581375 * 0.9)));
            SaveToByteList(l, DatabaseDataType.EOF, "END");
            return l;
        } catch {
            return ToListOfByte(db, works);
        }
    }

    public static string UndoText(ColumnItem? column, RowItem? row, ListExt<WorkItem>? works) {
        if (works == null || works.Count == 0) { return string.Empty; }
        var cellKey = CellCollection.KeyOfCell(column, row);
        var t = "";
        for (var z = works.Count - 1; z >= 0; z--) {
            if (works[z] != null && works[z].CellKey == cellKey) {
                t = t + works[z].UndoTextTableMouseOver() + "<br>";
            }
        }
        t = t.Trim("<br>");
        t = t.Trim("<hr>");
        t = t.Trim("<br>");
        t = t.Trim("<hr>");
        return t;
    }

    public override string AdditionalFilesPfadWhole() {
        var x = base.AdditionalFilesPfadWhole();
        if (!string.IsNullOrEmpty(x)) { return x; }

        if (!string.IsNullOrEmpty(Filename)) {
            var t = (Filename.FilePath() + "AdditionalFiles\\").CheckPath();
            if (DirectoryExists(t)) {
                _additionalFilesPfadtmp = t;
                return t;
            }
        }
        _additionalFilesPfadtmp = string.Empty;
        return string.Empty;
    }

    public override List<ConnectionInfo>? AllAvailableTables(List<DatabaseAbstract>? allreadychecked, List<string>? ignorePath) {
        if (string.IsNullOrWhiteSpace(Filename)) { return null; } // Stream-Datenbank

        if (allreadychecked != null) {
            foreach (var thisa in allreadychecked) {
                if (thisa is Database db) {
                    if (string.Equals(db.Filename.FilePath(), Filename.FilePath())) { return null; }
                }
                if (thisa is DatabaseMultiUser dbm) {
                    if (string.Equals(dbm.Filename.FilePath(), Filename.FilePath())) { return null; }
                }
            }
        }

        if (ignorePath != null) {
            foreach (var thisPf in ignorePath) {
                if (Filename.FilePath().StartsWith(thisPf, StringComparison.OrdinalIgnoreCase)) { return null; }
            }
        }

        var nn = Directory.GetFiles(Filename.FilePath(), "*.mdb", SearchOption.AllDirectories);
        var gb = new List<ConnectionInfo>();
        foreach (var thisn in nn) {
            gb.Add(ConnectionDataOfOtherTable(thisn.FileNameWithoutSuffix(), false));
        }
        return gb;
    }

    public override ConnectionInfo? ConnectionDataOfOtherTable(string tableName, bool checkExists) {
        if (string.IsNullOrEmpty(Filename)) { return null; }

        var f = Filename.FilePath() + tableName.FileNameWithoutSuffix() + ".mdb";

        if (checkExists && !File.Exists(f)) { return null; }

        return new ConnectionInfo(SQLBackAbstract.MakeValidTableName(tableName.FileNameWithoutSuffix()), null, DatabaseId, f);
    }

    public void LoadFromFile(string fileNameToLoad, bool createWhenNotExisting, NeedPassword? needPassword) {
        if (string.Equals(fileNameToLoad, Filename, StringComparison.OrdinalIgnoreCase)) { return; }
        if (!string.IsNullOrEmpty(Filename)) { Develop.DebugPrint(FehlerArt.Fehler, "Geladene Dateien können nicht als neue Dateien geladen werden."); }
        if (string.IsNullOrEmpty(fileNameToLoad)) { Develop.DebugPrint(FehlerArt.Fehler, "Dateiname nicht angegeben!"); }
        //fileNameToLoad = modConverter.SerialNr2Path(fileNameToLoad);
        if (!createWhenNotExisting && !CanWriteInDirectory(fileNameToLoad.FilePath())) { SetReadOnly(); }
        if (!IsFileAllowedToLoad(fileNameToLoad)) { return; }
        if (!FileExists(fileNameToLoad)) {
            if (createWhenNotExisting) {
                if (ReadOnly) {
                    Develop.DebugPrint(FehlerArt.Fehler, "Readonly kann keine Datei erzeugen<br>" + fileNameToLoad);
                    return;
                }
                SaveAsAndChangeTo(fileNameToLoad);
            } else {
                Develop.DebugPrint(FehlerArt.Warnung, "Datei existiert nicht: " + fileNameToLoad);  // Readonly deutet auf Backup hin, in einem anderne Verzeichnis (Linked)
                SetReadOnly();
                return;
            }
        }
        Filename = fileNameToLoad;
        //ReCreateWatcher();
        // Wenn ein Dateiname auf Nix gesezt wird, z.B: bei Bitmap import
        if (string.IsNullOrEmpty(Filename)) { return; }

        //LoadingEventArgs ec = new(_initialLoadDone);
        OnLoading();

        var bLoaded = LoadBytesFromDisk(BlueBasics.Enums.ErrorReason.Load);
        if (bLoaded == null) { return; }

        Database.Parse(bLoaded, this, Works, needPassword);

        RepairAfterParse();
        OnLoaded();
        CreateWatcher();
    }

    public void LoadFromStream(Stream stream) {
        OnLoading();
        byte[] bLoaded;
        using (BinaryReader r = new(stream)) {
            bLoaded = r.ReadBytes((int)stream.Length);
            r.Close();
        }

        //if (bLoaded.Length > 4 && BitConverter.ToInt32(bLoaded, 0) == 67324752) {
        //    // Gezipte Daten-Kennung gefunden
        //    bLoaded = MultiUserFile.UnzipIt(bLoaded);
        //}

        Database.Parse(bLoaded, this, Works, null);

        RepairAfterParse();
        OnLoaded();
        CreateWatcher();
    }

    public override void RefreshColumnsData(List<ColumnItem?>? columns) {
        if (columns == null || columns.Count == 0) { return; }

        foreach (var thiscol in columns) {
            if (thiscol != null) { thiscol.IsInCache = DateTime.UtcNow; }
        }
    }

    public override bool RefreshRowData(List<RowItem> row, bool refreshAlways) {
        if (row == null || row.Count == 0) { return false; }

        foreach (var thisrow in row) {
            thisrow.IsInCache = DateTime.UtcNow;
        }

        return false;
    }

    public override bool Save() {
        if (ReadOnly) { return false; }
        if (string.IsNullOrEmpty(Filename)) { return false; }

        if (!HasPendingChanges) { return false; }

        var tmpFileName = WriteTempFileToDisk();

        if (string.IsNullOrEmpty(tmpFileName)) { return false; }

        if (FileExists(Backupdateiname())) {
            if (!DeleteFile(Backupdateiname(), false)) { return false; }
        }

        // Haupt-Datei wird zum Backup umbenannt
        if (!MoveFile(Filename, Backupdateiname(), false)) { return false; }

        // --- TmpFile wird zum Haupt ---
        MoveFile(tmpFileName, Filename, true);

        HasPendingChanges = false;
        return true;
    }

    public void SaveAsAndChangeTo(string newFileName) {
        if (string.Equals(newFileName, Filename, StringComparison.OrdinalIgnoreCase)) { Develop.DebugPrint(FehlerArt.Fehler, "Dateiname unterscheiden sich nicht!"); }
        Save(); // Original-Datei speichern, die ist ja dann weg.
        // Jetzt kann es aber immer noch sein, das PendingChanges da sind.
        // Wenn kein Dateiname angegeben ist oder bei Readonly wird die Datei nicht gespeichert und die Pendings bleiben erhalten!

        Filename = newFileName;

        var l = ToListOfByte(this, Works);
        using FileStream x = new(newFileName, FileMode.Create, FileAccess.Write, FileShare.None);
        x.Write(l.ToArray(), 0, l.ToArray().Length);
        x.Flush();
        x.Close();
    }

    public override string UndoText(ColumnItem? column, RowItem? row) => UndoText(column, row, Works);

    internal static void SaveToByteList(List<byte> list, ColumnItem c, RowItem r) {
        var cv = r.CellGetString(c);

        if (string.IsNullOrEmpty(cv)) { return; }
        //db.Cell.DataOfCellKey(cell.Key, out var tColumn, out var tRow);
        if (!c.SaveContent) { return; }
        var b = cv.UTF8_ToByte();
        list.Add((byte)Routinen.CellFormatUTF8_V401);
        list.Add((byte)DatabaseDataType.Value_withoutSizeData);
        SaveToByteList(list, b.Length, 3);
        SaveToByteList(list, r.Key, 7);
        list.AddRange(b);
    }

    //public override void WaitEditable() => _muf.WaitEditable();
    internal static void SaveToByteList(List<byte> list, DatabaseDataType databaseDataType, string content, string columnname) {
        list.Add((byte)Routinen.ColumnUTF8_V401);
        list.Add((byte)databaseDataType);

        var n = columnname.UTF8_ToByte();
        SaveToByteList(list, n.Length, 1);
        list.AddRange(n);

        var b = content.UTF8_ToByte();
        SaveToByteList(list, b.Length, 3);
        list.AddRange(b);
    }

    internal static void SaveToByteList(List<byte> list, ColumnCollection c) {
        //Database.SaveToByteList(List, enDatabaseDataType.LastColumnKey, _LastColumnKey.ToString());
        foreach (var columnitem in c) {
            if (columnitem != null && !string.IsNullOrEmpty(columnitem.Name)) {
                SaveToByteList(columnitem, ref list);
            }
        }
    }

    internal static void SaveToByteList(List<byte> list, DatabaseDataType databaseDataType, string content) {
        var b = content.UTF8_ToByte();
        list.Add((byte)Routinen.DatenAllgemeinUTF8);
        list.Add((byte)databaseDataType);
        SaveToByteList(list, b.Length, 3);
        list.AddRange(b);
    }

    internal override string SetValueInternal(DatabaseDataType type, string value, string? columnname, long? rowkey, bool isLoading) {
        if (IsDisposed) { return "Datenbank verworfen!"; }

        var r = base.SetValueInternal(type, value, columnname, rowkey, isLoading);

        if (type == DatabaseDataType.UndoInOne) {
            Works.Clear();
            var uio = value.SplitAndCutByCr();
            for (var z = 0; z <= uio.GetUpperBound(0); z++) {
                WorkItem tmpWork = new(uio[z]);
                Works.Add(tmpWork);
            }
        }

        if (!isLoading && !ReadOnly) {
            HasPendingChanges = true;
        }

        return r;
    }

    protected override void AddUndo(string tableName, DatabaseDataType comand, string? columnName, long? rowKey, string previousValue, string changedTo, string userName, string comment) {
        Works.Add(new WorkItem(comand, columnName, rowKey, previousValue, changedTo, userName));
    }

    protected override void Dispose(bool disposing) {
        //_muf.Dispose();
        Works.Dispose();
        base.Dispose(disposing);
    }

    protected override void Initialize() {
        base.Initialize();
        Works.Clear();
    }

    private static int NummerCode1(byte[] b, int pointer) => b[pointer];

    private static int NummerCode2(byte[] b, int pointer) => (b[pointer] * 255) + b[pointer + 1];

    //protected override string SpecialErrorReason(ErrorReason mode) => _muf.ErrorReason(mode);
    private static int NummerCode3(byte[] b, int pointer) => (b[pointer] * 65025) + (b[pointer + 1] * 255) + b[pointer + 2];

    private static long NummerCode7(byte[] b, int pointer) {
        long nu = 0;
        for (var n = 0; n < 7; n++) {
            nu += b[pointer + n] * (long)Math.Pow(255, 6 - n);
        }
        return nu;
    }

    private static void SaveToByteList(List<byte> list, long numberToAdd, int byteCount) {
        //var tmp = numberToAdd;
        //var nn = byteCount;

        do {
            byteCount--;
            var te = (long)Math.Pow(255, byteCount);
            var mu = (byte)Math.Truncate((decimal)(numberToAdd / te));

            list.Add(mu);
            numberToAdd %= te;
        } while (byteCount > 0);

        //if (nn == 3) {
        //    if (NummerCode3(list.ToArray(), list.Count - nn) != tmp) { Debugger.Break(); }
        //}

        //if (nn == 7) {
        //    if (NummerCode7(list.ToArray(), list.Count - nn) != tmp) { Debugger.Break(); }
        //}
    }

    private string Backupdateiname() => string.IsNullOrEmpty(Filename) ? string.Empty : Filename.FilePath() + Filename.FileNameWithoutSuffix() + ".bak";

    /// <summary>
    /// Diese Routine lädt die Datei von der Festplatte. Zur Not wartet sie bis zu 5 Minuten.
    /// Hier wird auch nochmal geprüft, ob ein Laden überhaupt möglich ist.
    /// Es kann auch NULL zurück gegeben werden, wenn es ein Reload ist und die Daten inzwischen aktuell sind.
    /// </summary>
    /// <param name="checkmode"></param>
    /// <returns></returns>
    private byte[]? LoadBytesFromDisk(ErrorReason checkmode) {
        var startTime = DateTime.UtcNow;
        byte[] bLoaded;
        while (true) {
            try {
                var f = ErrorReason(checkmode);
                if (string.IsNullOrEmpty(f)) {
                    //var tmpLastSaveCode1 = GetFileInfo(Filename, true);
                    bLoaded = File.ReadAllBytes(Filename);
                    //tmpLastSaveCode2 = GetFileInfo(Filename, true);
                    //if (tmpLastSaveCode1 == tmpLastSaveCode2) { break; }
                    //f = "Datei wurde während des Ladens verändert.";
                    break;
                }

                if (DateTime.UtcNow.Subtract(startTime).TotalSeconds > 20) {
                    Develop.DebugPrint(FehlerArt.Info, f + "\r\n" + Filename);
                }

                Pause(0.5, false);
            } catch (Exception ex) {
                // Home Office kann lange blokieren....
                if (DateTime.UtcNow.Subtract(startTime).TotalSeconds > 300) {
                    Develop.DebugPrint(FehlerArt.Fehler, "Die Datei<br>" + Filename + "<br>konnte trotz mehrerer Versuche nicht geladen werden.<br><br>Die Fehlermeldung lautet:<br>" + ex.Message);
                    return null;
                }
            }
        }

        if (bLoaded.Length > 4 && BitConverter.ToInt32(bLoaded, 0) == 67324752) {
            // Gezipte Daten-Kennung gefunden
            bLoaded = MultiUserFile.UnzipIt(bLoaded);
        }
        return bLoaded;
    }

    private string WriteTempFileToDisk() {
        var f = ErrorReason(BlueBasics.Enums.ErrorReason.Save);
        if (!string.IsNullOrEmpty(f)) { return string.Empty; }

        var dataUncompressed = ToListOfByte(this, Works).ToArray();
        var tmpFileName = TempFile(Filename.FilePath() + Filename.FileNameWithoutSuffix() + ".tmp-" + UserName().ToUpper());

        using FileStream x = new(tmpFileName, FileMode.Create, FileAccess.Write, FileShare.None);
        x.Write(dataUncompressed, 0, dataUncompressed.Length);
        x.Flush();
        x.Close();

        return tmpFileName;
    }

    #endregion

    //private void ChangeWorkItems(ItemState oldState, ItemState newState) {
    //    foreach (var thisWork in Works) {
    //        if (thisWork != null) {
    //            if (thisWork.State == oldState) { thisWork.State = newState; }
    //        }
    //    }
    //}

    //private void ExecutePending() {
    //    if (!_muf.IsLoading) { Develop.DebugPrint(FehlerArt.Fehler, "Nur während des Parsens möglich"); }

    //    var e2 = new MultiUserFileHasPendingChangesEventArgs();
    //    HasPendingChanges(null, e2);

    //    if (!e2.HasPendingChanges) { return; }
    //    // Erst die Neuen Zeilen / Spalten alle neutralisieren
    //    //var dummy = -1000;
    //    //foreach (var ThisPending in Works) {
    //    //    if (ThisPending.State == enItemState.Pending) {
    //    //        //if (ThisPending.Comand == enDatabaseDataType.dummyComand_AddRow) {
    //    //        //    dummy--;
    //    //        //    ChangeRowKeyInPending(ThisPending.RowKey, dummy);
    //    //        //}
    //    //        //if (ThisPending.Comand == enDatabaseDataType.AddColumnKeyInfo) {
    //    //        //    dummy--;
    //    //        //    ChangeColumnKeyInPending(ThisPending.ColKey, dummy);
    //    //        //}
    //    //    }
    //    //}
    //    //// Dann den neuen Zeilen / Spalten Tatsächlich eine neue ID zuweisen
    //    //foreach (var ThisPending in Works) {
    //    //    if (ThisPending.State == enItemState.Pending) {
    //    //        switch (ThisPending.Comand) {
    //    //            //case enDatabaseDataType.dummyComand_AddRow when _JoinTyp == enJoinTyp.Intelligent_zusammenfassen: {
    //    //            //        var Value = SearchKeyValueInPendingsOf(ThisPending.RowKey);
    //    //            //        var fRow = Row[Value];
    //    //            //        if (!string.IsNullOrEmpty(Value) && fRow != null) {
    //    //            //            ChangeRowKeyInPending(ThisPending.RowKey, fRow.Key);
    //    //            //        } else {
    //    //            //            ChangeRowKeyInPending(ThisPending.RowKey, Row.NextRowKey());
    //    //            //        }
    //    //            //        break;
    //    //            //    }
    //    //            //case enDatabaseDataType.dummyComand_AddRow:
    //    //            //    ChangeRowKeyInPending(ThisPending.RowKey, Row.NextRowKey());
    //    //            //    break;

    //    //            //case enDatabaseDataType.AddColumnKeyInfo:
    //    //            //    ChangeColumnKeyInPending(ThisPending.ColKey, Column.NextColumnKey);
    //    //            //    break;
    //    //        }
    //    //    }
    //    //}
    //    // Und nun alles ausführen!
    //    foreach (var thisPending in Works.Where(thisPending => thisPending.State == ItemState.Pending)) {
    //        if (thisPending.Comand == DatabaseDataType.ColumnName) {
    //            thisPending.ChangedTo = Column.Freename(thisPending.ChangedTo);
    //        }
    //        ExecutePending(thisPending);
    //    }
    //}

    //private void ExecutePending(WorkItem thisPendingItem) {
    //    if (thisPendingItem.State == ItemState.Pending) {
    //        //RowItem? row = null;
    //        //if (thisPendingItem.RowKey > -1) {
    //        //    row = Row.SearchByKey(thisPendingItem.RowKey);
    //        //    if (row == null) {
    //        //        if (thisPendingItem.Comand != DatabaseDataType.Comand_AddRow && thisPendingItem.User != UserName) {
    //        //            Develop.DebugPrint("Pending verworfen, Zeile gelöscht.<br>" + ConnectionData.TableName + "<br>" + thisPendingItem.ToString());
    //        //            return;
    //        //        }
    //        //    }
    //        //}
    //        //ColumnItem? col = null;
    //        //if (thisPendingItem.ColKey > -1) {
    //        //    col = Column.SearchByKey(thisPendingItem.ColKey);
    //        //    if (col == null) {
    //        //        //if (thisPendingItem.Comand != DatabaseDataType.AddColumnKeyInfo && thisPendingItem.User != UserName) {
    //        //        Develop.DebugPrint("Pending verworfen, Spalte gelöscht.<br>" + ConnectionData.TableName + "<br>" + thisPendingItem.ToString());
    //        //        return;
    //        //        //}
    //        //    }
    //        //}
    //        SetValueInternal(thisPendingItem.Comand, thisPendingItem.ChangedTo, thisPendingItem.ColKey, thisPendingItem.RowKey, 0, 0, true);
    //    }
    //}
}