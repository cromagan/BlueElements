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

using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using System.Collections.ObjectModel;

namespace BlueDatabase.Interfaces;

public interface IColumnInputFormat : IInputFormat {

    #region Properties

    public bool AfterEditQuickSortRemoveDouble { get; set; }
    public AlignmentHorizontal Align { get; set; }
    public string DefaultRenderer { get; set; }
    public TranslationType DoOpticalTranslation { get; set; }
    public bool DropdownDeselectAllAllowed { get; set; }
    public ReadOnlyCollection<string> DropDownItems { get; set; }
    public bool EditableWithDropdown { get; set; }
    public bool EditableWithTextInput { get; set; }
    public string RendererSettings { get; set; }
    public ScriptType ScriptType { get; set; }
    public bool ShowValuesOfOtherCellsInDropdown { get; set; }
    public SortierTyp SortType { get; set; }

    #endregion
}

public static class ColumnInputFormatExtensions {

    #region Methods

    public static void GetStyleFrom(this IColumnInputFormat? t, IColumnInputFormat? source) {
        if (source == null || t == null) { return; }

        ((IInputFormat)t).GetStyleFrom(source);

        t.AfterEditQuickSortRemoveDouble = source.AfterEditQuickSortRemoveDouble;
        t.Align = source.Align;
        t.DoOpticalTranslation = source.DoOpticalTranslation;
        t.DropdownDeselectAllAllowed = source.DropdownDeselectAllAllowed;
        t.EditableWithDropdown = source.EditableWithDropdown;
        t.DropDownItems = source.DropDownItems;
        t.ShowValuesOfOtherCellsInDropdown = source.ShowValuesOfOtherCellsInDropdown;
        t.ScriptType = source.ScriptType;
        t.SortType = source.SortType;
        t.EditableWithTextInput = source.EditableWithTextInput;
        t.MaxTextLenght = source.MaxTextLenght;
        t.DefaultRenderer = source.DefaultRenderer;
        t.RendererSettings = source.RendererSettings;
    }

    #endregion
}