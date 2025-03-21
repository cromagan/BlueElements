﻿// Authors:
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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using System;
using System.Collections.ObjectModel;

namespace BlueDatabase;

public class ColumnFormatHolder : FormatHolder, IColumnInputFormat {

    #region Fields

    public static readonly ColumnFormatHolder BildCode = new(FormatHolder.Text) {
        Function = ColumnFunction.Normal,
        Align = AlignmentHorizontal.Links,
        SortType = SortierTyp.Original_String,
        DoOpticalTranslation = TranslationType.Original_Anzeigen,
        AfterEditQuickSortRemoveDouble = false,
        ScriptType = ScriptType.String,
        DropdownDeselectAllAllowed = true,
        EditableWithDropdown = true,
        EditableWithTextInput = true,
        DropDownItems = new(Array.Empty<string>()),
        ShowValuesOfOtherCellsInDropdown = true,
        DefaultRenderer = "ImageAndText",
        RendererSettings = "{ClassId=\"ImageAndText\", Style=\"Windows 11\", ShowPic=+, ShowText=-, ImageWidth=16, ImageHeight=16}"
    };

    public static new readonly ColumnFormatHolder Bit = new(FormatHolder.Bit) {
        Function = ColumnFunction.Normal,
        Align = AlignmentHorizontal.Zentriert,
        SortType = SortierTyp.Original_String,
        DoOpticalTranslation = TranslationType.Original_Anzeigen,
        AfterEditQuickSortRemoveDouble = false,
        ScriptType = ScriptType.Bool,
        DropdownDeselectAllAllowed = false,
        EditableWithDropdown = true,
        EditableWithTextInput = false,
        DropDownItems = new(["+", "-"]),
        ShowValuesOfOtherCellsInDropdown = true,
        DefaultRenderer = "ImageAndText",
        RendererSettings = "{ClassId=\"ImageAndText\", Style=\"Windows 11\", ShowPic=+, ShowText=-, ImageReplace=+[G]Häkchen|o[G]Kreis2|-[G]Kreuz, ImageWidth=16, ImageHeight=16}"
    };

    public static new readonly ColumnFormatHolder Date = new(FormatHolder.Date) {
        Function = ColumnFunction.Normal,
        Align = AlignmentHorizontal.Links,
        SortType = SortierTyp.Datum_Uhrzeit,
        DoOpticalTranslation = TranslationType.Datum,
        AfterEditQuickSortRemoveDouble = false,
        ScriptType = ScriptType.String,
        DropdownDeselectAllAllowed = false,
        EditableWithDropdown = false,
        EditableWithTextInput = true,
        DropDownItems = new(Array.Empty<string>()),
        ShowValuesOfOtherCellsInDropdown = false,
        DefaultRenderer = "TextOneLine",
        RendererSettings = "{ClassId=\"TextOneLine\", Style=\"Windows 11\"}"
    };

    public static new readonly ColumnFormatHolder DateTime = new(FormatHolder.DateTime) {
        Function = ColumnFunction.Normal,
        Align = AlignmentHorizontal.Links,
        SortType = SortierTyp.Datum_Uhrzeit,
        DoOpticalTranslation = TranslationType.Datum,
        AfterEditQuickSortRemoveDouble = false,
        ScriptType = ScriptType.String,
        DropdownDeselectAllAllowed = false,
        EditableWithDropdown = false,
        EditableWithTextInput = true,
        DropDownItems = new(Array.Empty<string>()),
        ShowValuesOfOtherCellsInDropdown = false,
        DefaultRenderer = "TextOneLine",
        RendererSettings = "{ClassId=\"TextOneLine\", Style=\"Windows 11\"}"
    };

    public static new readonly ColumnFormatHolder Email = new(FormatHolder.Email) {
        Function = ColumnFunction.Normal,
        Align = AlignmentHorizontal.Links,
        SortType = SortierTyp.Original_String,
        DoOpticalTranslation = TranslationType.Original_Anzeigen,
        AfterEditQuickSortRemoveDouble = false,
        ScriptType = ScriptType.String,
        DropdownDeselectAllAllowed = false,
        EditableWithDropdown = false,
        EditableWithTextInput = true,
        DropDownItems = new(Array.Empty<string>()),
        ShowValuesOfOtherCellsInDropdown = false,
        DefaultRenderer = "TextOneLine",
        RendererSettings = "{ClassId=\"TextOneLine\", Style=\"Windows 11\"}"
    };

    public static new readonly ColumnFormatHolder Float = new(FormatHolder.Float) {
        Function = ColumnFunction.Normal,
        Align = AlignmentHorizontal.Rechts,
        SortType = SortierTyp.ZahlenwertFloat,
        DoOpticalTranslation = TranslationType.Zahl,
        AfterEditQuickSortRemoveDouble = false,
        ScriptType = ScriptType.Numeral,
        DropdownDeselectAllAllowed = false,
        EditableWithDropdown = false,
        EditableWithTextInput = true,
        DropDownItems = new(Array.Empty<string>()),
        ShowValuesOfOtherCellsInDropdown = false,
        DefaultRenderer = "TextOneLine",
        RendererSettings = "{ClassId=\"TextOneLine\", Style=\"Windows 11\"}"
    };

    // ReSharper disable once UnusedMember.Global
    public static new readonly ColumnFormatHolder FloatPositive = new(FormatHolder.FloatPositive) {
        Function = ColumnFunction.Normal,
        Align = AlignmentHorizontal.Rechts,
        SortType = SortierTyp.ZahlenwertFloat,
        DoOpticalTranslation = TranslationType.Zahl,
        AfterEditQuickSortRemoveDouble = false,
        ScriptType = ScriptType.Numeral,
        DropdownDeselectAllAllowed = false,
        EditableWithDropdown = false,
        EditableWithTextInput = true,
        DropDownItems = new(Array.Empty<string>()),
        ShowValuesOfOtherCellsInDropdown = false,
        DefaultRenderer = "TextOneLine",
        RendererSettings = "{ClassId=\"TextOneLine\", Style=\"Windows 11\"}"
    };

    public static new readonly ColumnFormatHolder Long = new(FormatHolder.Long) {
        Function = ColumnFunction.Normal,
        Align = AlignmentHorizontal.Rechts,
        SortType = SortierTyp.ZahlenwertInt,
        DoOpticalTranslation = TranslationType.Zahl,
        AfterEditQuickSortRemoveDouble = false,
        ScriptType = ScriptType.Numeral,
        DropdownDeselectAllAllowed = false,
        EditableWithDropdown = false,
        EditableWithTextInput = true,
        DropDownItems = new(Array.Empty<string>()),
        ShowValuesOfOtherCellsInDropdown = false,
        DefaultRenderer = "TextOneLine",
        RendererSettings = "{ClassId=\"TextOneLine\", Style=\"Windows 11\"}"
    };

    public static new readonly ColumnFormatHolder LongPositive = new(FormatHolder.LongPositive) {
        Function = ColumnFunction.Normal,
        Align = AlignmentHorizontal.Rechts,
        SortType = SortierTyp.ZahlenwertInt,
        DoOpticalTranslation = TranslationType.Zahl,
        AfterEditQuickSortRemoveDouble = false,
        ScriptType = ScriptType.Numeral,
        DropdownDeselectAllAllowed = false,
        EditableWithDropdown = false,
        EditableWithTextInput = true,
        DropDownItems = new(Array.Empty<string>()),
        ShowValuesOfOtherCellsInDropdown = false,
        DefaultRenderer = "TextOneLine",
        RendererSettings = "{ClassId=\"TextOneLine\", Style=\"Windows 11\"}"
    };

    public static new readonly ColumnFormatHolder PhoneNumber = new(FormatHolder.PhoneNumber) {
        Function = ColumnFunction.Normal,
        Align = AlignmentHorizontal.Links,
        SortType = SortierTyp.Original_String,
        DoOpticalTranslation = TranslationType.Original_Anzeigen,
        AfterEditQuickSortRemoveDouble = false,
        ScriptType = ScriptType.String,
        DropdownDeselectAllAllowed = false,
        EditableWithDropdown = false,
        EditableWithTextInput = true,
        DropDownItems = new(Array.Empty<string>()),
        ShowValuesOfOtherCellsInDropdown = false,
        DefaultRenderer = "TextOneLine",
        RendererSettings = "{ClassId=\"TextOneLine\", Style=\"Windows 11\"}"
    };

    public static new readonly ColumnFormatHolder SystemName = new(FormatHolder.SystemName) {
        Function = ColumnFunction.Normal,
        Align = AlignmentHorizontal.Links,
        SortType = SortierTyp.Original_String,
        DoOpticalTranslation = TranslationType.Original_Anzeigen,
        AfterEditQuickSortRemoveDouble = false,
        ScriptType = ScriptType.String,
        DropdownDeselectAllAllowed = false,
        EditableWithDropdown = false,
        EditableWithTextInput = true,
        DropDownItems = new(Array.Empty<string>()),
        ShowValuesOfOtherCellsInDropdown = false,
        DefaultRenderer = "TextOneLine",
        RendererSettings = "{ClassId=\"TextOneLine\", Style=\"Windows 11\"}"
    };

    public static new readonly ColumnFormatHolder Text = new(FormatHolder.Text) {
        Function = ColumnFunction.Normal,
        Align = AlignmentHorizontal.Links,
        SortType = SortierTyp.Sprachneutral_String,
        DoOpticalTranslation = TranslationType.Original_Anzeigen,
        AfterEditQuickSortRemoveDouble = false,
        ScriptType = ScriptType.String,
        DropdownDeselectAllAllowed = false,
        EditableWithDropdown = false,
        EditableWithTextInput = true,
        DropDownItems = new(Array.Empty<string>()),
        ShowValuesOfOtherCellsInDropdown = false,
        DefaultRenderer = "TextOneLine",
        RendererSettings = "{ClassId=\"TextOneLine\", Style=\"Windows 11\"}"
    };

    // ReSharper disable once UnusedMember.Global
    public static new readonly ColumnFormatHolder TextMitFormatierung = new(FormatHolder.TextMitFormatierung) {
        Function = ColumnFunction.Normal,
        Align = AlignmentHorizontal.Links,
        SortType = SortierTyp.Sprachneutral_String,
        DoOpticalTranslation = TranslationType.Original_Anzeigen,
        AfterEditQuickSortRemoveDouble = false,
        ScriptType = ScriptType.String,
        DropdownDeselectAllAllowed = false,
        EditableWithDropdown = false,
        EditableWithTextInput = true,
        DropDownItems = new(Array.Empty<string>()),
        ShowValuesOfOtherCellsInDropdown = false,
        DefaultRenderer = "ImageAndText",
        RendererSettings = "{ClassId=\"ImageAndText\", Style=\"Windows 11\", ShowPic=-, ShowText=+}"
    };

    public static readonly ColumnFormatHolder TextOptions = new(FormatHolder.Text) {
        Function = ColumnFunction.Normal,
        Align = AlignmentHorizontal.Links,
        SortType = SortierTyp.Sprachneutral_String,
        DoOpticalTranslation = TranslationType.Übersetzen,
        AfterEditQuickSortRemoveDouble = true,
        ScriptType = ScriptType.String,
        DropdownDeselectAllAllowed = true,
        EditableWithDropdown = true,
        EditableWithTextInput = true,
        DropDownItems = new(Array.Empty<string>()),
        ShowValuesOfOtherCellsInDropdown = true,
        MultiLine = true // Verhalten von Setformat überschreiben
    };

    public static new readonly ColumnFormatHolder Url = new(FormatHolder.Url) {
        Function = ColumnFunction.Normal,
        Align = AlignmentHorizontal.Links,
        SortType = SortierTyp.Original_String,
        DoOpticalTranslation = TranslationType.Original_Anzeigen,
        AfterEditQuickSortRemoveDouble = false,
        ScriptType = ScriptType.String,
        DropdownDeselectAllAllowed = false,
        EditableWithDropdown = false,
        EditableWithTextInput = true,
        DropDownItems = new(Array.Empty<string>()),
        ShowValuesOfOtherCellsInDropdown = false,
        DefaultRenderer = "TextOneLine",
        RendererSettings = "{ClassId=\"TextOneLine\", Style=\"Windows 11\"}"
    };

    #endregion

    #region Constructors

    private ColumnFormatHolder(FormatHolder vorlage) : base(vorlage.Name) {
        AllFormats.Add(this);
        this.GetStyleFrom(vorlage);
    }

    #endregion

    #region Properties

    public bool AfterEditQuickSortRemoveDouble { get; set; }
    public AlignmentHorizontal Align { get; set; }
    public string DefaultRenderer { get; set; }
    public TranslationType DoOpticalTranslation { get; set; }
    public bool DropdownDeselectAllAllowed { get; set; }
    public ReadOnlyCollection<string> DropDownItems { get; set; } = new(Array.Empty<string>());
    public bool EditableWithDropdown { get; set; }
    public bool EditableWithTextInput { get; set; }
    public ColumnFunction Function { get; set; }
    public string RendererSettings { get; set; }
    public ScriptType ScriptType { get; set; }
    public bool ShowValuesOfOtherCellsInDropdown { get; set; }
    public SortierTyp SortType { get; set; }

    #endregion
}