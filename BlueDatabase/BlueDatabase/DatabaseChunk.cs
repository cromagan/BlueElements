// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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
using BlueBasics;
using BlueDatabase.Enums;
using static BlueBasics.Extensions;
using static BlueBasics.Generic;
using static BlueBasics.IO;

namespace BlueDatabase;

[EditorBrowsable(EditorBrowsableState.Never)]
public class DatabaseChunk {

    public long DataLenght => _bytes.Count;

    #region Fields

    public readonly string Filename = string.Empty;
    private readonly List<byte> _bytes = new List<byte>();

    #endregion

    #region Methods

    public void BytesAdd(byte bytes) {
        _bytes.Add(bytes);
    }

    public void BytesAddRange(byte[] bytes) {
        _bytes.AddRange(bytes);
    }

    public bool Save(string filename) {
        try {
            Develop.SetUserDidSomething();
            var datacompressed = _bytes.ToArray().ZipIt();
            if (datacompressed is not { }) { return false; }

            Develop.SetUserDidSomething();

            using FileStream x = new(filename, FileMode.Create, FileAccess.Write, FileShare.None);
            x.Write(datacompressed, 0, datacompressed.Length);
            x.Flush();
            x.Close();
            Develop.SetUserDidSomething();
        } catch { return false; }

        return true;
    }

    public void SaveToByteList(ColumnItem column, RowItem row) {
        if (column.Database is not { IsDisposed: false } db) { return; }

        var cellContent = db.Cell.GetStringCore(column, row);
        if (string.IsNullOrEmpty(cellContent)) { return; }

        BytesAdd((byte)Routinen.CellFormatUTF8_V402);

        var rowKeyByte = row.KeyName.UTF8_ToByte();
        SaveToByteList(rowKeyByte.Length, 1);
        BytesAddRange(rowKeyByte);

        var cellContentByte = cellContent.UTF8_ToByte();
        SaveToByteList(cellContentByte.Length, 2);
        BytesAddRange(cellContentByte);
    }

    public void SaveToByteList(DatabaseDataType databaseDataType, string content, string columnname) {
        BytesAdd((byte)Routinen.ColumnUTF8_V401);
        BytesAdd((byte)databaseDataType);

        var n = columnname.UTF8_ToByte();
        SaveToByteList(n.Length, 1);
        BytesAddRange(n);

        var b = content.UTF8_ToByte();
        SaveToByteList(b.Length, 3);
        BytesAddRange(b);
    }

    public void SaveToByteList(ColumnCollection c) {
        //Database.SaveToByteList(List, enDatabaseDataType.LastColumnKey, _LastColumnKey.ToString(false));
        foreach (var columnitem in c) {
            if (columnitem != null && !string.IsNullOrEmpty(columnitem.KeyName)) {
                SaveToByteList(columnitem);
            }
        }
    }

    public void SaveToByteList(DatabaseDataType databaseDataType, string content) {
        var b = content.UTF8_ToByte();
        BytesAdd((byte)Routinen.DatenAllgemeinUTF8);
        BytesAdd((byte)databaseDataType);
        SaveToByteList(b.Length, 3);
        BytesAddRange(b);
    }

    public void SaveToByteList(long numberToAdd, int byteCount) {
        do {
            byteCount--;
            var te = (long)Math.Pow(255, byteCount);
            // ReSharper disable once PossibleLossOfFraction
            var mu = (byte)Math.Truncate((decimal)(numberToAdd / te));

            BytesAdd(mu);
            numberToAdd %= te;
        } while (byteCount > 0);
    }

    public void SaveToByteList(ColumnItem c) {
        if (c.Database is not { IsDisposed: false } db) { return; }

        var name = c.KeyName;

        SaveToByteList(DatabaseDataType.ColumnName, c.KeyName, name);
        SaveToByteList(DatabaseDataType.ColumnCaption, c.Caption, name);
        SaveToByteList(DatabaseDataType.ColumnFunction, ((int)c.Function).ToString(), name);
        SaveToByteList(DatabaseDataType.DefaultRenderer, c.DefaultRenderer, name);
        SaveToByteList(DatabaseDataType.RendererSettings, c.RendererSettings, name);
        SaveToByteList(DatabaseDataType.CaptionGroup1, c.CaptionGroup1, name);
        SaveToByteList(DatabaseDataType.CaptionGroup2, c.CaptionGroup2, name);
        SaveToByteList(DatabaseDataType.CaptionGroup3, c.CaptionGroup3, name);
        SaveToByteList(DatabaseDataType.MultiLine, c.MultiLine.ToPlusMinus(), name);
        //SaveToByteList(l, DatabaseDataType.CellInitValue, c.CellInitValue, name);
        SaveToByteList(DatabaseDataType.SortAndRemoveDoubleAfterEdit, c.AfterEditQuickSortRemoveDouble.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.DoUcaseAfterEdit, c.AfterEditDoUCase.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.AutoCorrectAfterEdit, c.AfterEditAutoCorrect.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.RoundAfterEdit, c.RoundAfterEdit.ToString(), name);
        SaveToByteList(DatabaseDataType.MaxCellLenght, c.MaxCellLenght.ToString(), name);
        SaveToByteList(DatabaseDataType.FixedColumnWidth, c.FixedColumnWidth.ToString(), name);
        SaveToByteList(DatabaseDataType.AutoRemoveCharAfterEdit, c.AutoRemove, name);
        //SaveToByteList(l, DatabaseDataType.SaveContent, c.SaveContent.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.FilterOptions, ((int)c.FilterOptions).ToString(), name);
        SaveToByteList(DatabaseDataType.AutoFilterJoker, c.AutoFilterJoker, name);
        SaveToByteList(DatabaseDataType.IgnoreAtRowFilter, c.IgnoreAtRowFilter.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.EditableWithTextInput, c.TextBearbeitungErlaubt.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.SpellCheckingEnabled, c.SpellCheckingEnabled.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.ShowUndo, c.ShowUndo.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.TextFormatingAllowed, c.FormatierungErlaubt.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.ForeColor, c.ForeColor.ToArgb().ToString(), name);
        SaveToByteList(DatabaseDataType.BackColor, c.BackColor.ToArgb().ToString(), name);
        SaveToByteList(DatabaseDataType.LineStyleLeft, ((int)c.LineLeft).ToString(), name);
        SaveToByteList(DatabaseDataType.LineStyleRight, ((int)c.LineRight).ToString(), name);
        SaveToByteList(DatabaseDataType.EditableWithDropdown, c.DropdownBearbeitungErlaubt.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.DropDownItems, c.DropDownItems.JoinWithCr(), name);
        SaveToByteList(DatabaseDataType.LinkedCellFilter, c.LinkedCellFilter.JoinWithCr(), name);
        //SaveToByteList(l, DatabaseDataType.OpticalTextReplace, c.OpticalReplace.JoinWithCr(), name);
        SaveToByteList(DatabaseDataType.AutoReplaceAfterEdit, c.AfterEditAutoReplace.JoinWithCr(), name);
        SaveToByteList(DatabaseDataType.RegexCheck, c.Regex, name);
        SaveToByteList(DatabaseDataType.DropdownDeselectAllAllowed, c.DropdownAllesAbwählenErlaubt.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.ShowValuesOfOtherCellsInDropdown, c.DropdownWerteAndererZellenAnzeigen.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.ColumnQuickInfo, c.QuickInfo, name);
        SaveToByteList(DatabaseDataType.ColumnAdminInfo, c.AdminInfo, name);
        SaveToByteList(DatabaseDataType.ColumnSystemInfo, c.SystemInfo, name);
        //SaveToByteList(l, DatabaseDataType.ColumnContentWidth, c.ContentWidth.ToString(), name);
        SaveToByteList(DatabaseDataType.CaptionBitmapCode, c.CaptionBitmapCode, name);
        SaveToByteList(DatabaseDataType.AllowedChars, c.AllowedChars, name);
        SaveToByteList(DatabaseDataType.MaxTextLenght, c.MaxTextLenght.ToString(), name);
        SaveToByteList(DatabaseDataType.PermissionGroupsChangeCell, c.PermissionGroupsChangeCell.JoinWithCr(), name);
        SaveToByteList(DatabaseDataType.ColumnTags, c.Tags.JoinWithCr(), name);
        SaveToByteList(DatabaseDataType.EditAllowedDespiteLock, c.EditAllowedDespiteLock.ToPlusMinus(), name);
        SaveToByteList(DatabaseDataType.LinkedDatabase, c.LinkedDatabaseTableName, name);
        //SaveToByteList(l, DatabaseDataType.ConstantHeightOfImageCode, c.ConstantHeightOfImageCode, name);
        //SaveToByteList(l, DatabaseDataType.BehaviorOfImageAndText, ((int)c.BehaviorOfImageAndText).ToString(), name);
        SaveToByteList(DatabaseDataType.DoOpticalTranslation, ((int)c.DoOpticalTranslation).ToString(), name);
        SaveToByteList(DatabaseDataType.AdditionalFormatCheck, ((int)c.AdditionalFormatCheck).ToString(), name);
        SaveToByteList(DatabaseDataType.ScriptType, ((int)c.ScriptType).ToString(), name);
        //SaveToByteList(l, DatabaseDataType.KeyColumnKey, column.KeyColumnKey.ToString(false), key);
        SaveToByteList(DatabaseDataType.ColumnNameOfLinkedDatabase, c.LinkedCell_ColumnNameOfLinkedDatabase, name);
        //SaveToByteList(l, DatabaseDataType.MakeSuggestionFromSameKeyColumn, column.VorschlagsColumn.ToString(false), key);
        SaveToByteList(DatabaseDataType.ColumnAlign, ((int)c.Align).ToString(), name);
        SaveToByteList(DatabaseDataType.SortType, ((int)c.SortType).ToString(), name);
        //SaveToByteList(l, DatabaseDataType.ColumnTimeCode, column.TimeCode, key);

        if (c.Function != ColumnFunction.Virtuelle_Spalte) {
            foreach (var thisR in db.Row) {
                SaveToByteList(c, thisR);
            }
        }
    }

    public string WriteTempFileToDisk() {
        if (_bytes.Count < 50) { return string.Empty; }

        var tmpFileName = TempFile(Filename.FilePath() + Filename.FileNameWithoutSuffix() + ".tmp-" + UserName.ToUpperInvariant());

        if (Save(tmpFileName)) {
            return tmpFileName;
        }

        return string.Empty;
    }

    #endregion
}