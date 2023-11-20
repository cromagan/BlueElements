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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.MultiUserFile;
using BlueDatabase.Enums;
using static BlueBasics.Converter;
using static BlueBasics.Generic;
using static BlueBasics.IO;

namespace BlueDatabase;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public class Database : DatabaseAbstract {

    #region Fields

    private string _canWriteError = string.Empty;
    private DateTime _canWriteNextCheckUtc = DateTime.UtcNow.AddSeconds(-30);
    private string _editNormalyError = string.Empty;
    private DateTime _editNormalyNextCheckUtc = DateTime.UtcNow.AddSeconds(-30);
    private bool _isInSave;

    #endregion

    #region Constructors

    public Database(ConnectionInfo ci, bool readOnly, NeedPassword? needPassword) : this(ci.AdditionalData, readOnly, ci.MustBeFreezed, false, needPassword) { }

    public Database(Stream stream, string tablename) : this(stream, string.Empty, true, "Stream-Datenbank", false, tablename, null) { }

    public Database(bool readOnly, string freezedReason, string tablename) : this(null, string.Empty, readOnly, freezedReason, true, tablename, null) { }

    public Database(string filename, bool readOnly, string freezedReason, bool create, NeedPassword? needPassword) : this(null, filename, readOnly, freezedReason, create, filename.FileNameWithoutSuffix(), needPassword) { }

    private Database(Stream? stream, string filename, bool readOnly, string freezedReason, bool create, string tablename, NeedPassword? needPassword) : base(tablename, readOnly, freezedReason) {
        TableName = MakeValidTableName(tablename);

        if (!IsValidTableName(TableName, false)) {
            Develop.DebugPrint(FehlerArt.Fehler, "Tabellenname ungültig: " + tablename);
        }

        Initialize();

        if (!string.IsNullOrEmpty(filename)) {
            LoadFromFile(filename, create, needPassword);
        } else if (stream != null) {
            LoadFromStream(stream);
        }

        TryToSetMeTemporaryMaster();
    }

    #endregion

    #region Properties

    public static string DatabaseId => nameof(Database);

    public override ConnectionInfo ConnectionData => new(TableName, this, DatabaseId, Filename, FreezedReason);

    public string Filename { get; protected set; } = string.Empty;

    protected override bool DoCellChanges => true;

    #endregion

    #region Methods

    public static DatabaseAbstract? CanProvide(ConnectionInfo ci, bool readOnly, NeedPassword? needPassword) {
        if (!DatabaseId.Equals(ci.DatabaseId, StringComparison.OrdinalIgnoreCase)) { return null; }

        if (string.IsNullOrEmpty(ci.AdditionalData)) { return null; }
        if (ci.AdditionalData.FileSuffix().ToUpper() is not "BDB" or "MDB") { return null; }
        if (!FileExists(ci.AdditionalData)) { return null; }
        return new Database(ci, readOnly, needPassword);
    }

    public static void Parse(byte[] data, DatabaseAbstract db, NeedPassword? needPassword) {
        var pointer = 0;

        ColumnItem? column = null;
        RowItem? row = null;
        var columnUsed = new List<ColumnItem>();

        db.Undo.Clear();

        do {
            if (pointer >= data.Length) { break; }

            var (i, type, value, columname, rowKey) = Parse(data, pointer);
            pointer = i;

            if (!type.IsObsolete()) {

                #region Zeile suchen oder erstellen

                if (!string.IsNullOrEmpty(rowKey)) {
                    row = db.Row.SearchByKey(rowKey);
                    if (row == null || row.IsDisposed) {
                        _ = db.Row.ExecuteCommand(DatabaseDataType.Command_AddRow, rowKey, null, Reason.LoadReload);
                        row = db.Row.SearchByKey(rowKey);
                    }
                    if (row == null || row.IsDisposed) {
                        Develop.DebugPrint(FehlerArt.Fehler, "Zeile hinzufügen Fehler");
                        db.Freeze("Zeile hinzufügen Fehler");
                        return;
                    }
                    row.IsInCache = DateTime.UtcNow;
                }

                #endregion

                #region Spalte suchen oder erstellen

                //if (colKey > -1 && string.IsNullOrEmpty(columname)) {
                //    column = db.Column.SearchByKey(colKey);
                //    if (Column  ==null || Column .IsDisposed) {
                //        if (art != DatabaseDataType.ColumnName) { Develop.DebugPrint(art + " an erster Stelle!"); }
                //        _ = db.Column.SetValueInternal(DatabaseDataType.Command_AddColumnByKey, true, string.Empty);
                //        column = db.Column.SearchByKey(colKey);
                //    }
                //    if (Column  ==null || Column .IsDisposed) {
                //        Develop.DebugPrint(FehlerArt.Fehler, "Spalte hinzufügen Fehler");
                //        db.SetReadOnly();
                //        return;
                //    }
                //    column.IsInCache = DateTime.UtcNow;
                //    columnUsed.Add(column);
                //}

                if (!string.IsNullOrEmpty(columname)) {
                    column = db.Column.Exists(columname);
                    if (column == null || column.IsDisposed) {
                        if (type != DatabaseDataType.ColumnName) { Develop.DebugPrint(type + " an erster Stelle!"); }
                        _ = db.Column.ExecuteCommand(DatabaseDataType.Command_AddColumnByName, Reason.LoadReload, columname);
                        column = db.Column.Exists(columname);
                    }
                    if (column == null || column.IsDisposed) {
                        Develop.DebugPrint(FehlerArt.Fehler, "Spalte hinzufügen Fehler");
                        db.Freeze("Spalte hinzufügen Fehler");
                        return;
                    }
                    column.IsInCache = DateTime.UtcNow;
                    columnUsed.Add(column);
                }

                #endregion

                #region Bei verschlüsselten Datenbanken das Passwort abfragen

                if (type == DatabaseDataType.GlobalShowPass && !string.IsNullOrEmpty(value)) {
                    var pwd = string.Empty;

                    if (needPassword != null) { pwd = needPassword(); }
                    if (pwd != value) {
                        db.Freeze("Passwort falsch");
                        break;
                    }
                }

                #endregion

                var fehler = db.SetValueInternal(type, value, column, row, Reason.LoadReload, UserName, DateTime.UtcNow, "Parsen");

                if (type == DatabaseDataType.EOF) { break; }

                if (!string.IsNullOrEmpty(fehler)) {
                    db.Freeze("Datenbank-Ladefehler");
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
                _ = db.SetValueInternal(DatabaseDataType.Command_RemoveColumn, thisColumn.KeyName, thisColumn, null, Reason.LoadReload, UserName, DateTime.UtcNow, "Parsen");
            }
        }

        #endregion

        db.Row.RemoveNullOrEmpty();
        db.Cell.RemoveOrphans();
        //Works?.AddRange(oldPendings);
        //oldPendings?.Clear();
        //ExecutePending();

        //if (db != null && db.Column.Count > 0 && string.IsNullOrEmpty(db.FirstColumn)) {
        //    db.FirstColumn = Col
        //}

        if (IntParse(db.LoadedVersion.Replace(".", string.Empty)) > IntParse(DatabaseVersion.Replace(".", string.Empty))) { db.Freeze("Datenbankversions-Konflikt"); }
    }

    // Dateibasierte Systeme haben immer den Undo-Speicher
    public static void SaveToByteList(ColumnItem c, ref List<byte> l) {
        if (c.Database is not DatabaseAbstract db) { return; }

        var name = c.KeyName;

        SaveToByteList(l, DatabaseDataType.ColumnName, c.KeyName, name);
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
        SaveToByteList(l, DatabaseDataType.RoundAfterEdit, c.RoundAfterEdit.ToString(), name);
        SaveToByteList(l, DatabaseDataType.MaxCellLenght, c.MaxCellLenght.ToString(), name);
        SaveToByteList(l, DatabaseDataType.FixedColumnWidth, c.FixedColumnWidth.ToString(), name);
        SaveToByteList(l, DatabaseDataType.AutoRemoveCharAfterEdit, c.AutoRemove, name);
        //SaveToByteList(l, DatabaseDataType.SaveContent, c.SaveContent.ToPlusMinus(), name);
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
        SaveToByteList(l, DatabaseDataType.LinkedDatabase, c.LinkedDatabaseTableName, name);
        SaveToByteList(l, DatabaseDataType.ConstantHeightOfImageCode, c.ConstantHeightOfImageCode, name);
        SaveToByteList(l, DatabaseDataType.BehaviorOfImageAndText, ((int)c.BehaviorOfImageAndText).ToString(), name);
        SaveToByteList(l, DatabaseDataType.DoOpticalTranslation, ((int)c.DoOpticalTranslation).ToString(), name);
        SaveToByteList(l, DatabaseDataType.AdditionalFormatCheck, ((int)c.AdditionalFormatCheck).ToString(), name);
        SaveToByteList(l, DatabaseDataType.ScriptType, ((int)c.ScriptType).ToString(), name);
        SaveToByteList(l, DatabaseDataType.Prefix, c.Prefix, name);
        //SaveToByteList(l, DatabaseDataType.KeyColumnKey, column.KeyColumnKey.ToString(false), key);
        SaveToByteList(l, DatabaseDataType.ColumnNameOfLinkedDatabase, c.LinkedCell_ColumnNameOfLinkedDatabase, name);
        //SaveToByteList(l, DatabaseDataType.MakeSuggestionFromSameKeyColumn, column.VorschlagsColumn.ToString(false), key);
        SaveToByteList(l, DatabaseDataType.ColumnAlign, ((int)c.Align).ToString(), name);
        SaveToByteList(l, DatabaseDataType.SortType, ((int)c.SortType).ToString(), name);
        //SaveToByteList(l, DatabaseDataType.ColumnTimeCode, column.TimeCode, key);

        foreach (var thisR in db.Row) {
            SaveToByteList(l, c, thisR);
        }
    }

    public static bool SaveToFile(DatabaseAbstract db, int minLen, string filn) {
        var bytes = ToListOfByte(db, minLen);

        if (bytes == null) {
            return false;
        }

        try {
            using FileStream x = new(filn, FileMode.Create, FileAccess.Write, FileShare.None);
            x.Write(bytes.ToArray(), 0, bytes.ToArray().Length);
            x.Flush();
            x.Close();
        } catch { return false; }

        return true;
    }

    public static List<byte>? ToListOfByte(DatabaseAbstract db, int minLen) {
        try {
            var x = db.LastChange;
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
            SaveToByteList(l, DatabaseDataType.FileStateUTCDate, db.FileStateUTCDate);
            SaveToByteList(l, DatabaseDataType.Caption, db.Caption);

            SaveToByteList(l, DatabaseDataType.TemporaryDatabaseMasterUser, db.TemporaryDatabaseMasterUser);
            SaveToByteList(l, DatabaseDataType.TemporaryDatabaseMasterTimeUTC, db.TemporaryDatabaseMasterTimeUtc);

            //SaveToByteList(l, enDatabaseDataType.JoinTyp, ((int)_JoinTyp).ToString(false));
            //SaveToByteList(l, DatabaseDataType.VerwaisteDaten, ((int)_verwaisteDaten).ToString(false));
            SaveToByteList(l, DatabaseDataType.Tags, db.Tags.JoinWithCr());
            SaveToByteList(l, DatabaseDataType.PermissionGroupsNewRow, db.PermissionGroupsNewRow.JoinWithCr());
            SaveToByteList(l, DatabaseDataType.DatabaseAdminGroups, db.DatenbankAdmin.JoinWithCr());
            SaveToByteList(l, DatabaseDataType.GlobalScale, db.GlobalScale.ToString(Constants.Format_Float1));
            //SaveToByteList(l, DatabaseDataType.Ansicht, ((int)_ansicht).ToString(false));
            //SaveToByteList(l, DatabaseDataType.ReloadDelaySecond, ReloadDelaySecond.ToString(false));
            //SaveToByteList(l, DatabaseDataType.RulesScript, db.RulesScript);
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
            SaveToByteList(l, DatabaseDataType.ColumnArrangement, db.ColumnArrangements.ToString(false));
            //SaveToByteList(l, DatabaseDataType.Layouts, db.Layouts.JoinWithCr());
            //SaveToByteList(l, DatabaseDataType.AutoExport, db.Export.ToString(true));

            SaveToByteList(l, DatabaseDataType.EventScript, db.EventScript.ToString(true));
            SaveToByteList(l, DatabaseDataType.EventScriptVersion, db.EventScriptVersion.ToString());
            SaveToByteList(l, DatabaseDataType.EventScriptErrorMessage, db.EventScriptErrorMessage);
            //SaveToByteList(l, DatabaseDataType.Events, db.Events.ToString(true));
            SaveToByteList(l, DatabaseDataType.DatabaseVariables, db.Variables.ToList().ToString(true));

            if (x != db.LastChange) { return null; } // Works haben sich evtl. geändert

            // Beim Erstellen des Undo-Speichers die Undos nicht verändern, da auch bei einem nicht
            // erfolgreichen Speichervorgang der Datenbank-String erstellt wird.
            List<string> works2 = new();
            foreach (var thisWorkItem in db.Undo) {
                if (thisWorkItem != null) {
                    if (thisWorkItem.Command != DatabaseDataType.Value_withoutSizeData) {
                        works2.Add(thisWorkItem.ToString());
                    } else {
                        if (thisWorkItem.LogsUndo(db)) {
                            works2.Add(thisWorkItem.ToString());
                        }
                    }
                }
            }

            var undoCount = 5000;
            //SaveToByteList(l, DatabaseDataType.UndoCount, db.UndoCount.ToString());
            if (works2.Count > undoCount) { works2.RemoveRange(0, works2.Count - undoCount); }
            SaveToByteList(l, DatabaseDataType.UndoInOne, works2.JoinWithCr((int)(16581375 * 0.95)));
            SaveToByteList(l, DatabaseDataType.EOF, "END");

            if (l.Count < minLen) {
                //Develop.DebugPrint(FehlerArt.Fehler, "ToString Fehler!");
                return null;
            }

            if (x != db.LastChange) { return null; } // Stand stimmt nicht mehr

            return l;
        } catch {
            Develop.CheckStackForOverflow();
            return ToListOfByte(db, minLen);
        }
    }

    public override string AdditionalFilesPfadWhole() {
        var x = base.AdditionalFilesPfadWhole();
        if (!string.IsNullOrEmpty(x)) { return x; }

        if (!string.IsNullOrEmpty(Filename)) {
            var t = (Filename.FilePath() + "AdditionalFiles\\").CheckPath();
            if (DirectoryExists(t)) {
                AdditionalFilesPfadtmp = t;
                return t;
            }
        }
        AdditionalFilesPfadtmp = string.Empty;
        return string.Empty;
    }

    public override List<ConnectionInfo>? AllAvailableTables(List<DatabaseAbstract>? allreadychecked, string mustBeFreezed) {
        if (string.IsNullOrWhiteSpace(Filename)) { return null; } // Stream-Datenbank

        if (allreadychecked != null) {
            foreach (var thisa in allreadychecked) {
                if (thisa is Database db) {
                    if (string.Equals(db.Filename.FilePath(), Filename.FilePath())) { return null; }
                }
            }
        }

        //if (ignorePath != null) {
        //    foreach (var thisPf in ignorePath) {
        //        if (Filename.FilePath().StartsWith(thisPf, StringComparison.OrdinalIgnoreCase)) { return null; }
        //    }
        //}

        var nn = Directory.GetFiles(Filename.FilePath(), "*.bdb", SearchOption.AllDirectories);
        var gb = new List<ConnectionInfo>();
        foreach (var thisn in nn) {
            var t = ConnectionDataOfOtherTable(thisn.FileNameWithoutSuffix(), false, mustBeFreezed);
            if (t != null) { gb.Add(t); }
        }
        return gb;
    }

    public override ConnectionInfo? ConnectionDataOfOtherTable(string tableName, bool checkExists, string mustBeeFreezed) {
        if (string.IsNullOrEmpty(Filename)) { return null; }

        var f = Filename.FilePath() + tableName.FileNameWithoutSuffix() + ".bdb";

        if (checkExists && !File.Exists(f)) { return null; }

        return new ConnectionInfo(MakeValidTableName(tableName.FileNameWithoutSuffix()), null, DatabaseId, f, FreezedReason);
    }

    public override string EditableErrorReason(EditableErrorReasonType mode) {
        var m = base.EditableErrorReason(mode);
        if (!string.IsNullOrEmpty(m)) { return m; }

        //----------Load, vereinfachte Prüfung ------------------------------------------------------------------------
        if (mode.HasFlag(EditableErrorReasonType.Load) || mode.HasFlag(EditableErrorReasonType.LoadForCheckingOnly)) {
            if (string.IsNullOrEmpty(Filename)) { return "Kein Dateiname angegeben."; }
        }

        //----------Alle Edits und Save ------------------------------------------------------------------------
        //  Generelle Prüfung, die eigentlich immer benötigt wird. Mehr allgemeine Fehler, wo sich nicht so schnell ändern
        //  und eine Prüfung, die nicht auf die Sekunde genau wichtig ist.
        if (CheckForLastError(ref _editNormalyNextCheckUtc, ref _editNormalyError)) { return _editNormalyError; }
        if (!string.IsNullOrEmpty(Filename)) {
            if (!CanWriteInDirectory(Filename.FilePath())) {
                _editNormalyError = "Sie haben im Verzeichnis der Datei keine Schreibrechte.";
                return _editNormalyError;
            }
        }

        //---------- Save ------------------------------------------------------------------------------------------
        if (mode.HasFlag(EditableErrorReasonType.Save)) {
            if (DateTime.UtcNow.Subtract(LastChange).TotalSeconds < 1) { return "Kürzlich vorgenommene Änderung muss verarbeitet werden."; }
            if (DateTime.UtcNow.Subtract(Develop.LastUserActionUtc).TotalSeconds < 6) { return "Aktuell werden vom Benutzer Daten bearbeitet."; } // Evtl. Massenänderung. Da hat ein Reload fatale auswirkungen. SAP braucht manchmal 6 sekunden für ein zca4
            if (string.IsNullOrEmpty(Filename)) { return string.Empty; } // EXIT -------------------
            if (!FileExists(Filename)) { return string.Empty; } // EXIT -------------------
            if (CheckForLastError(ref _canWriteNextCheckUtc, ref _canWriteError) && !string.IsNullOrEmpty(_canWriteError)) {
                return _canWriteError;
            }

            try {
                FileInfo f2 = new(Filename);
                if (DateTime.UtcNow.Subtract(f2.LastWriteTimeUtc).TotalSeconds < 5) {
                    _canWriteError = "Anderer Speichervorgang noch nicht abgeschlossen.";
                    return _canWriteError;
                }
            } catch {
                _canWriteError = "Dateizugriffsfehler.";
                return _canWriteError;
            }
            if (!CanWrite(Filename, 0.5)) {
                _canWriteError = "Windows blockiert die Datei.";
                return _canWriteError;
            }
        }
        return string.Empty;

        // Gibt true zurück, wenn die letzte Prüfung noch gülig ist
        static bool CheckForLastError(ref DateTime nextCheckUtc, ref string lastMessage) {
            if (DateTime.UtcNow.Subtract(nextCheckUtc).TotalSeconds < 0) { return true; }
            lastMessage = string.Empty;
            nextCheckUtc = DateTime.UtcNow.AddSeconds(5);
            return false;
        }
    }

    public override List<UndoItem>? GetLastChanges(IEnumerable<DatabaseAbstract> db, DateTime fromUTC, DateTime toUTC) => new();

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
                Freeze("Datei existiert nicht");
                return;
            }
        }
        Filename = fileNameToLoad;
        //ReCreateWatcher();
        // Wenn ein Dateiname auf Nix gesezt wird, z.B: bei Bitmap import
        if (string.IsNullOrEmpty(Filename)) { return; }

        //LoadingEventArgs ec = new(_initialLoadDone);
        OnLoading();

        var bLoaded = LoadBytesFromDisk(EditableErrorReasonType.Load);
        if (bLoaded == null) { return; }

        Parse(bLoaded, this, needPassword);

        RepairAfterParse();
        OnLoaded();
        CreateWatcher();
        _ = ExecuteScript(ScriptEventTypes.loaded, string.Empty, true, null, null);
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

        Parse(bLoaded, this, null);

        RepairAfterParse();
        OnLoaded();
        CreateWatcher();
        _ = ExecuteScript(ScriptEventTypes.loaded, string.Empty, true, null, null);
    }

    public override string NextRowKey() {
        var tmp = 0;
        string key;

        do {
            key = GetUniqueKey(tmp, "row");
            tmp++;
        } while (Row.SearchByKey(key) != null);
        return key;
    }

    public override void RefreshColumnsData(List<ColumnItem> columns) {
        if (columns.Count == 0) { return; }

        foreach (var thiscol in columns) {
            if (thiscol != null) {
                thiscol.IsInCache = DateTime.UtcNow;

                if (thiscol.LinkedDatabase is DatabaseAbstract dbl &&
                    dbl.Column.Exists(thiscol.LinkedCell_ColumnNameOfLinkedDatabase) is ColumnItem col) {
                    dbl.RefreshColumnsData(col);
                }
            }
        }
    }

    public override (bool didreload, string errormessage) RefreshRowData(IEnumerable<RowItem> row, bool refreshAlways) {
        if (row == null || !row.Any()) { return (false, string.Empty); }

        foreach (var thisrow in row) {
            thisrow.IsInCache = DateTime.UtcNow;
        }
        //var x = Row.DoLinkedDatabase(row);
        return (false, string.Empty);
    }

    public override void RepairAfterParse() {
        // Nicht IsInCache setzen, weil ansonsten DatabaseMU nicht mehr funktioniert

        base.RepairAfterParse();

        if (!string.IsNullOrEmpty(Filename)) {
            if (!string.Equals(TableName, MakeValidTableName(Filename.FileNameWithoutSuffix()), StringComparison.OrdinalIgnoreCase)) {
                Develop.DebugPrint(FehlerArt.Warnung, "Tabellenname stimmt nicht: " + Filename);
            }
        }
    }

    public override bool Save() {
        if (_isInSave) { return false; }

        if (!HasPendingChanges) { return false; }

        _isInSave = true;
        var v = SaveInternal();
        _isInSave = false;
        OnInvalidateView();
        return v;
    }

    public void SaveAsAndChangeTo(string newFileName) {
        if (string.Equals(newFileName, Filename, StringComparison.OrdinalIgnoreCase)) { Develop.DebugPrint(FehlerArt.Fehler, "Dateiname unterscheiden sich nicht!"); }
        _ = Save(); // Original-Datei speichern, die ist ja dann weg.
        // Jetzt kann es aber immer noch sein, das PendingChanges da sind.
        // Wenn kein Dateiname angegeben ist oder bei Readonly wird die Datei nicht gespeichert und die Pendings bleiben erhalten!

        Filename = newFileName;

        var l = ToListOfByte(this, 100);

        if (l == null) { return; }

        using FileStream x = new(newFileName, FileMode.Create, FileAccess.Write, FileShare.None);
        x.Write(l.ToArray(), 0, l.ToArray().Length);
        x.Flush();
        x.Close();
        OnInvalidateView();
    }

    internal static void SaveToByteList(List<byte> list, ColumnItem column, RowItem row) {
        if (column.Database is not DatabaseAbstract db || db.IsDisposed) { return; }

        var cellContent = db.Cell.GetStringCore(column, row);
        if (string.IsNullOrEmpty(cellContent)) { return; }

        list.Add((byte)Routinen.CellFormatUTF8_V402);

        var rowKeyByte = row.KeyName.UTF8_ToByte();
        SaveToByteList(list, rowKeyByte.Length, 1);
        list.AddRange(rowKeyByte);

        var cellContentByte = cellContent.UTF8_ToByte();
        SaveToByteList(list, cellContentByte.Length, 2);
        list.AddRange(cellContentByte);
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
        //Database.SaveToByteList(List, enDatabaseDataType.LastColumnKey, _LastColumnKey.ToString(false));
        foreach (var columnitem in c) {
            if (columnitem != null && !string.IsNullOrEmpty(columnitem.KeyName)) {
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

    internal override string SetValueInternal(DatabaseDataType type, string value, ColumnItem? column, RowItem? row, Reason reason, string user, DateTime datetimeutc, string comment) {
        if (IsDisposed) { return "Datenbank verworfen!"; }

        var r = base.SetValueInternal(type, value, column, row, reason, user, datetimeutc, comment);

        if (type == DatabaseDataType.UndoInOne) {
            Undo.Clear();
            var uio = value.SplitAndCutByCr();
            for (var z = 0; z <= uio.GetUpperBound(0); z++) {
                UndoItem tmpWork = new(uio[z]);
                Undo.Add(tmpWork);
            }
        }

        if (reason != Reason.LoadReload) {
            HasPendingChanges = true;
        }

        return r;
    }

    protected override void DoWorkAfterLastChanges() { }

    protected override IEnumerable<DatabaseAbstract> LoadedDatabasesWithSameServer() => new List<DatabaseAbstract>();

    protected bool SaveInternal() {
        var m = EditableErrorReason(EditableErrorReasonType.Save);
        if (!string.IsNullOrEmpty(m)) { return false; }

        if (string.IsNullOrEmpty(Filename)) { return false; }

        var tmpFileName = WriteTempFileToDisk();

        if (string.IsNullOrEmpty(tmpFileName)) { return false; }

        if (FileExists(Backupdateiname())) {
            if (!DeleteFile(Backupdateiname(), false)) { return false; }
        }

        // Haupt-Datei wird zum Backup umbenannt
        if (!MoveFile(Filename, Backupdateiname(), false)) { return false; }

        if (FileExists(Filename)) {
            // Paralleler Prozess hat gespeichert?!?
            _ = DeleteFile(tmpFileName, false);
            return false;
        }

        // --- TmpFile wird zum Haupt ---
        _ = MoveFile(tmpFileName, Filename, true);

        HasPendingChanges = false;
        return true;
    }

    private static int NummerCode1(IReadOnlyList<byte> b, int pointer) => b[pointer];

    private static int NummerCode2(IReadOnlyList<byte> b, int pointer) => (b[pointer] * 255) + b[pointer + 1];

    private static int NummerCode3(IReadOnlyList<byte> b, int pointer) => (b[pointer] * 65025) + (b[pointer + 1] * 255) + b[pointer + 2];

    private static long NummerCode7(IReadOnlyList<byte> b, int pointer) {
        long nu = 0;
        for (var n = 0; n < 7; n++) {
            nu += b[pointer + n] * (long)Math.Pow(255, 6 - n);
        }
        return nu;
    }

    private static (int pointer, DatabaseDataType type, string value, string colName, string rowKey) Parse(byte[] bLoaded, int pointer) {
        string colName = string.Empty;
        string rowKey = string.Empty;
        string value;
        DatabaseDataType type;

        switch ((Routinen)bLoaded[pointer]) {
            //case Routinen.CellFormat: {
            //        type = (DatabaseDataType)bLoaded[pointer + 1];
            //        var les = NummerCode3(bLoaded, pointer + 2);
            //        //colKey = NummerCode3(bLoaded, pointer + 5);
            //        rowKey = NummerCode3(bLoaded, pointer + 8);
            //        var cellContentByte = new byte[les];
            //        Buffer.BlockCopy(bLoaded, pointer + 11, cellContentByte, 0, les);
            //        value = cellContentByte.ToStringWin1252();
            //        //var width = NummerCode2(bLoaded, pointer + 11 + les);
            //        //var height = NummerCode2(bLoaded, pointer + 11 + les + 2);
            //        pointer += 11 + les + 4;
            //        break;
            //    }
            //case Routinen.CellFormatUTF8: {
            //        type = (DatabaseDataType)bLoaded[pointer + 1];
            //        var les = NummerCode3(bLoaded, pointer + 2);
            //        //colKey = NummerCode3(bLoaded, pointer + 5);
            //        rowKey = NummerCode3(bLoaded, pointer + 8);
            //        var cellContentByte = new byte[les];
            //        Buffer.BlockCopy(bLoaded, pointer + 11, cellContentByte, 0, les);
            //        value = cellContentByte.ToStringUtf8();
            //        //var width = NummerCode2(bLoaded, pointer + 11 + les);
            //        // var height = NummerCode2(bLoaded, pointer + 11 + les + 2);
            //        pointer += 11 + les + 4;
            //        break;
            //    }
            //case Routinen.CellFormatUTF8_V400: {
            //        type = (DatabaseDataType)bLoaded[pointer + 1];
            //        var les = NummerCode3(bLoaded, pointer + 2);
            //        //colKey = NummerCode7(bLoaded, pointer + 5);
            //        rowKey = NummerCode7(bLoaded, pointer + 12);
            //        var cellContentByte = new byte[les];
            //        Buffer.BlockCopy(bLoaded, pointer + 19, cellContentByte, 0, les);
            //        value = cellContentByte.ToStringUtf8();
            //        //var  width = NummerCode2(bLoaded, pointer + 19 + les);
            //        //var  height = NummerCode2(bLoaded, pointer + 19 + les + 2);
            //        pointer += 19 + les + 4;
            //        break;
            //    }
            case Routinen.CellFormatUTF8_V401: {
                    type = (DatabaseDataType)bLoaded[pointer + 1];
                    var les = NummerCode3(bLoaded, pointer + 2);
                    rowKey = NummerCode7(bLoaded, pointer + 5).ToString();
                    var b = new byte[les];
                    Buffer.BlockCopy(bLoaded, pointer + 12, b, 0, les);
                    value = b.ToStringUtf8();
                    pointer += 12 + les;
                    //colKey = -1;
                    break;
                }

            //case Routinen.DatenAllgemein: {
            //        type = (DatabaseDataType)bLoaded[pointer + 1];
            //        var les = NummerCode3(bLoaded, pointer + 2);
            //        //colKey = -1;
            //        rowKey = -1;
            //        var cellContentByte = new byte[les];
            //        Buffer.BlockCopy(bLoaded, pointer + 5, cellContentByte, 0, les);
            //        value = cellContentByte.ToStringWin1252();
            //        //width = 0;
            //        //height = 0;
            //        pointer += 5 + les;
            //        break;
            //    }
            case Routinen.DatenAllgemeinUTF8: {
                    type = (DatabaseDataType)bLoaded[pointer + 1];
                    var les = NummerCode3(bLoaded, pointer + 2);
                    rowKey = string.Empty;
                    var b = new byte[les];
                    Buffer.BlockCopy(bLoaded, pointer + 5, b, 0, les);
                    value = b.ToStringUtf8();
                    //width = 0;
                    //height = 0;
                    pointer += 5 + les;
                    break;
                }
            //case Routinen.Column: {
            //        type = (DatabaseDataType)bLoaded[pointer + 1];
            //        var les = NummerCode3(bLoaded, pointer + 2);
            //        //colKey = NummerCode3(bLoaded, pointer + 5);
            //        rowKey = NummerCode3(bLoaded, pointer + 8);
            //        var cellContentByte = new byte[les];
            //        Buffer.BlockCopy(bLoaded, pointer + 11, cellContentByte, 0, les);
            //        value = cellContentByte.ToStringWin1252();
            //        //width = 0;
            //        //height = 0;
            //        pointer += 11 + les;
            //        break;
            //    }
            //case Routinen.ColumnUTF8: {
            //        type = (DatabaseDataType)bLoaded[pointer + 1];
            //        var les = NummerCode3(bLoaded, pointer + 2);
            //        //colKey = NummerCode3(bLoaded, pointer + 5);
            //        rowKey = NummerCode3(bLoaded, pointer + 8);
            //        var cellContentByte = new byte[les];
            //        Buffer.BlockCopy(bLoaded, pointer + 11, cellContentByte, 0, les);
            //        value = cellContentByte.ToStringUtf8();
            //        //width = 0;
            //        //height = 0;
            //        pointer += 11 + les;
            //        break;
            //    }
            //case Routinen.ColumnUTF8_V400: {
            //        type = (DatabaseDataType)bLoaded[pointer + 1];
            //        var les = NummerCode3(bLoaded, pointer + 2);
            //        //colKey = NummerCode7(bLoaded, pointer + 5);
            //        rowKey = NummerCode7(bLoaded, pointer + 12);
            //        var cellContentByte = new byte[les];
            //        Buffer.BlockCopy(bLoaded, pointer + 19, cellContentByte, 0, les);
            //        value = cellContentByte.ToStringUtf8();
            //        //width = 0;
            //        //height = 0;
            //        pointer += 19 + les;
            //        break;
            //    }
            case Routinen.ColumnUTF8_V401: {
                    type = (DatabaseDataType)bLoaded[pointer + 1];

                    var cles = NummerCode1(bLoaded, pointer + 2);
                    var cb = new byte[cles];
                    Buffer.BlockCopy(bLoaded, pointer + 3, cb, 0, cles);
                    colName = cb.ToStringUtf8();

                    var les = NummerCode3(bLoaded, pointer + 3 + cles);
                    var b = new byte[les];
                    Buffer.BlockCopy(bLoaded, pointer + 6 + cles, b, 0, les);
                    value = b.ToStringUtf8();

                    pointer += 6 + les + cles;
                    break;
                }

            case Routinen.CellFormatUTF8_V402: {
                    type = DatabaseDataType.UTF8Value_withoutSizeData;

                    var lenghtRowKey = NummerCode1(bLoaded, pointer + 1);
                    var rowKeyByte = new byte[lenghtRowKey];
                    Buffer.BlockCopy(bLoaded, pointer + 2, rowKeyByte, 0, lenghtRowKey);
                    rowKey = rowKeyByte.ToStringUtf8();

                    var lenghtValue = NummerCode2(bLoaded, pointer + 2 + lenghtRowKey);
                    var valueByte = new byte[lenghtValue];
                    Buffer.BlockCopy(bLoaded, pointer + 2 + lenghtRowKey + 2, valueByte, 0, lenghtValue);
                    value = valueByte.ToStringUtf8();

                    pointer += 2 + lenghtRowKey + 2 + lenghtValue;

                    break;
                }

            default: {
                    type = 0;
                    value = string.Empty;
                    //width = 0;
                    //height = 0;
                    Develop.DebugPrint(FehlerArt.Fehler, $"Laderoutine nicht definiert: {bLoaded[pointer]}");
                    break;
                }
        }

        return (pointer, type, value, colName, rowKey);
    }

    private static void SaveToByteList(ICollection<byte> list, long numberToAdd, int byteCount) {
        do {
            byteCount--;
            var te = (long)Math.Pow(255, byteCount);
            // ReSharper disable once PossibleLossOfFraction
            var mu = (byte)Math.Truncate((decimal)(numberToAdd / te));

            list.Add(mu);
            numberToAdd %= te;
        } while (byteCount > 0);
    }

    private string Backupdateiname() => string.IsNullOrEmpty(Filename) ? string.Empty : Filename.FilePath() + Filename.FileNameWithoutSuffix() + ".bak";

    /// <summary>
    /// Diese Routine lädt die Datei von der Festplatte. Zur Not wartet sie bis zu 5 Minuten.
    /// Hier wird auch nochmal geprüft, ob ein Laden überhaupt möglich ist.
    /// Es kann auch NULL zurück gegeben werden, wenn es ein Reload ist und die Daten inzwischen aktuell sind.
    /// </summary>
    /// <param name="checkmode"></param>
    /// <returns></returns>
    private byte[]? LoadBytesFromDisk(EditableErrorReasonType checkmode) {
        var startTime = DateTime.UtcNow;
        byte[] bLoaded;
        while (true) {
            try {
                var f = EditableErrorReason(checkmode);
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
        var f = EditableErrorReason(EditableErrorReasonType.Save);
        if (!string.IsNullOrEmpty(f)) { return string.Empty; }

        var dataUncompressed = ToListOfByte(this, 1200)?.ToArray();

        if (dataUncompressed == null) { return string.Empty; }

        var tmpFileName = TempFile(Filename.FilePath() + Filename.FileNameWithoutSuffix() + ".tmp-" + UserName.ToUpper());

        using FileStream x = new(tmpFileName, FileMode.Create, FileAccess.Write, FileShare.None);
        x.Write(dataUncompressed, 0, dataUncompressed.Length);
        x.Flush();
        x.Close();

        return tmpFileName;
    }

    #endregion
}