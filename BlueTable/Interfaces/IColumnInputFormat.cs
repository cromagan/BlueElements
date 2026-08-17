// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Collections.ObjectModel;

namespace BlueTable.Interfaces;

public interface IColumnInputFormat : IInputFormat {

    #region Properties

    bool AfterEditDoUCase { get; set; }
    bool AfterEditQuickSortRemoveDouble { get; set; }
    int AfterEditRound { get; set; }
    AlignmentHorizontal Align { get; set; }
    string DefaultRenderer { get; set; }
    TranslationType DoOpticalTranslation { get; set; }
    ReadOnlyCollection<string> DropDownItems { get; set; }
    bool EditableWithTextInput { get; set; }

    string ControlStrategy { get; set; }

    string ControlStrategyParameter { get; set; }

    string RendererSettings { get; set; }
    ScriptType ScriptType { get; set; }
    bool ShowValuesOfOtherCellsInDropdown { get; set; }
    SortierTyp SortType { get; set; }

    #endregion
}

public static class ColumnInputFormatExtensions {

    #region Methods

    public static void GetStyleFrom(this IColumnInputFormat? t, IColumnInputFormat? source) {
        if (source is null || t is null) { return; }

        ((IInputFormat)t).GetStyleFrom(source);

        t.Align = source.Align;
        t.DoOpticalTranslation = source.DoOpticalTranslation;
        t.ControlStrategy = source.ControlStrategy;
        t.ControlStrategyParameter = source.ControlStrategyParameter;
        t.DropDownItems = source.DropDownItems;
        t.ShowValuesOfOtherCellsInDropdown = source.ShowValuesOfOtherCellsInDropdown;
        t.ScriptType = source.ScriptType;
        t.SortType = source.SortType;
        t.EditableWithTextInput = source.EditableWithTextInput;
        t.DefaultRenderer = source.DefaultRenderer;
        t.RendererSettings = source.RendererSettings;
        t.AfterEditDoUCase = source.AfterEditDoUCase;
        t.AfterEditQuickSortRemoveDouble = source.AfterEditQuickSortRemoveDouble;
        t.AfterEditRound = source.AfterEditRound;
    }

    #endregion
}