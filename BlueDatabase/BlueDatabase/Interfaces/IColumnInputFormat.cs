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

using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using System.Collections.ObjectModel;

namespace BlueDatabase.Interfaces;

public interface IColumnInputFormat : IInputFormat {

    #region Properties

    public bool AfterEditQuickSortRemoveDouble { get; set; }
    public AlignmentHorizontal Align { get; set; }
    public BildTextVerhalten BehaviorOfImageAndText { get; set; }
    public TranslationType DoOpticalTranslation { get; set; }
    public bool DropdownAllesAbwählenErlaubt { get; set; }
    public bool DropdownBearbeitungErlaubt { get; set; }
    public ReadOnlyCollection<string> DropDownItems { get; set; }
    public bool DropdownWerteAndererZellenAnzeigen { get; set; }
    public ColumnFunction Function { get; set; }
    public ScriptType ScriptType { get; set; }
    public SortierTyp SortType { get; set; }
    public bool TextBearbeitungErlaubt { get; set; }

    #endregion
}

public static class ColumnInputFormatExtensions {

    #region Methods

    public static void GetStyleFrom(this IColumnInputFormat? t, IColumnInputFormat? source) {
        if (source == null || t == null) { return; }

        ((IInputFormat)t).GetStyleFrom(source);

        t.AfterEditQuickSortRemoveDouble = source.AfterEditQuickSortRemoveDouble;
        t.Align = source.Align;
        t.BehaviorOfImageAndText = source.BehaviorOfImageAndText;
        t.DoOpticalTranslation = source.DoOpticalTranslation;
        t.DropdownAllesAbwählenErlaubt = source.DropdownAllesAbwählenErlaubt;
        t.DropdownBearbeitungErlaubt = source.DropdownBearbeitungErlaubt;
        t.DropDownItems = source.DropDownItems;
        t.DropdownWerteAndererZellenAnzeigen = source.DropdownWerteAndererZellenAnzeigen;
        t.Function = source.Function;
        t.ScriptType = source.ScriptType;
        t.SortType = source.SortType;
        t.TextBearbeitungErlaubt = source.TextBearbeitungErlaubt;
        t.MaxTextLenght = source.MaxTextLenght;
    }

    #endregion
}