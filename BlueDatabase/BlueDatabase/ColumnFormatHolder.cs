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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueDatabase.Enums;
using BlueDatabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BlueDatabase;

public class ColumnFormatHolder : FormatHolder, IColumnInputFormat {

    #region Fields

    public static readonly ColumnFormatHolder BildCode = new(FormatHolder.Text) {
        Format = DataFormat.Text,
        Align = AlignmentHorizontal.Links,
        SortType = SortierTyp.Original_String,
        DoOpticalTranslation = TranslationType.Original_Anzeigen,
        AfterEditQuickSortRemoveDouble = false,
        BehaviorOfImageAndText = BildTextVerhalten.Bild_oder_Text,
        ScriptType = ScriptType.String,
        DropdownAllesAbwählenErlaubt = true,
        DropdownBearbeitungErlaubt = true,
        TextBearbeitungErlaubt = true,
        DropDownItems = new(Array.Empty<string>()),
        DropdownWerteAndererZellenAnzeigen = true
    };

    public new static readonly ColumnFormatHolder Bit = new(FormatHolder.Bit) {
        Format = DataFormat.Text,
        Align = AlignmentHorizontal.Zentriert,
        SortType = SortierTyp.Original_String,
        DoOpticalTranslation = TranslationType.Original_Anzeigen,
        AfterEditQuickSortRemoveDouble = false,
        BehaviorOfImageAndText = BildTextVerhalten.Interpretiere_Bool,
        ScriptType = ScriptType.Bool,
        DropdownAllesAbwählenErlaubt = false,
        DropdownBearbeitungErlaubt = true,
        TextBearbeitungErlaubt = false,
        DropDownItems = new(new List<string> { "+", "-" }),
        DropdownWerteAndererZellenAnzeigen = true
    };

    public new static readonly ColumnFormatHolder Date = new(FormatHolder.Date) {
        Format = DataFormat.Text,
        Align = AlignmentHorizontal.Links,
        SortType = SortierTyp.Datum_Uhrzeit,
        DoOpticalTranslation = TranslationType.Datum,
        AfterEditQuickSortRemoveDouble = false,
        BehaviorOfImageAndText = BildTextVerhalten.Nur_Text,
        ScriptType = ScriptType.String,
        DropdownAllesAbwählenErlaubt = false,
        DropdownBearbeitungErlaubt = false,
        TextBearbeitungErlaubt = true,
        DropDownItems = new(Array.Empty<string>()),
        DropdownWerteAndererZellenAnzeigen = false
    };

    public new static readonly ColumnFormatHolder DateTime = new(FormatHolder.DateTime) {
        Format = DataFormat.Text,
        Align = AlignmentHorizontal.Links,
        SortType = SortierTyp.Datum_Uhrzeit,
        DoOpticalTranslation = TranslationType.Datum,
        AfterEditQuickSortRemoveDouble = false,
        BehaviorOfImageAndText = BildTextVerhalten.Nur_Text,
        ScriptType = ScriptType.String,
        DropdownAllesAbwählenErlaubt = false,
        DropdownBearbeitungErlaubt = false,
        TextBearbeitungErlaubt = true,
        DropDownItems = new(Array.Empty<string>()),
        DropdownWerteAndererZellenAnzeigen = false
    };

    public new static readonly ColumnFormatHolder Email = new(FormatHolder.Email) {
        Format = DataFormat.Text,
        Align = AlignmentHorizontal.Links,
        SortType = SortierTyp.Original_String,
        DoOpticalTranslation = TranslationType.Original_Anzeigen,
        AfterEditQuickSortRemoveDouble = false,
        BehaviorOfImageAndText = BildTextVerhalten.Nur_Text,
        ScriptType = ScriptType.String,
        DropdownAllesAbwählenErlaubt = false,
        DropdownBearbeitungErlaubt = false,
        TextBearbeitungErlaubt = true,
        DropDownItems = new(Array.Empty<string>()),
        DropdownWerteAndererZellenAnzeigen = false
    };

    public new static readonly ColumnFormatHolder Float = new(FormatHolder.Float) {
        Format = DataFormat.Text,
        Align = AlignmentHorizontal.Rechts,
        SortType = SortierTyp.ZahlenwertFloat,
        DoOpticalTranslation = TranslationType.Zahl,
        AfterEditQuickSortRemoveDouble = false,
        BehaviorOfImageAndText = BildTextVerhalten.Nur_Text,
        ScriptType = ScriptType.Numeral,
        DropdownAllesAbwählenErlaubt = false,
        DropdownBearbeitungErlaubt = false,
        TextBearbeitungErlaubt = true,
        DropDownItems = new(Array.Empty<string>()),
        DropdownWerteAndererZellenAnzeigen = false
    };

    // ReSharper disable once UnusedMember.Global
    public new static readonly ColumnFormatHolder FloatPositive = new(FormatHolder.FloatPositive) {
        Format = DataFormat.Text,
        Align = AlignmentHorizontal.Rechts,
        SortType = SortierTyp.ZahlenwertFloat,
        DoOpticalTranslation = TranslationType.Zahl,
        AfterEditQuickSortRemoveDouble = false,
        BehaviorOfImageAndText = BildTextVerhalten.Nur_Text,
        ScriptType = ScriptType.Numeral,
        DropdownAllesAbwählenErlaubt = false,
        DropdownBearbeitungErlaubt = false,
        TextBearbeitungErlaubt = true,
        DropDownItems = new(Array.Empty<string>()),
        DropdownWerteAndererZellenAnzeigen = false
    };

    public new static readonly ColumnFormatHolder Integer = new(FormatHolder.Integer) {
        Format = DataFormat.Text,
        Align = AlignmentHorizontal.Rechts,
        SortType = SortierTyp.ZahlenwertInt,
        DoOpticalTranslation = TranslationType.Zahl,
        AfterEditQuickSortRemoveDouble = false,
        BehaviorOfImageAndText = BildTextVerhalten.Nur_Text,
        ScriptType = ScriptType.Numeral,
        DropdownAllesAbwählenErlaubt = false,
        DropdownBearbeitungErlaubt = false,
        TextBearbeitungErlaubt = true,
        DropDownItems = new(Array.Empty<string>()),
        DropdownWerteAndererZellenAnzeigen = false
    };

    public new static readonly ColumnFormatHolder IntegerPositive = new(FormatHolder.IntegerPositive) {
        Format = DataFormat.Text,
        Align = AlignmentHorizontal.Rechts,
        SortType = SortierTyp.ZahlenwertInt,
        DoOpticalTranslation = TranslationType.Zahl,
        AfterEditQuickSortRemoveDouble = false,
        BehaviorOfImageAndText = BildTextVerhalten.Nur_Text,
        ScriptType = ScriptType.Numeral,
        DropdownAllesAbwählenErlaubt = false,
        DropdownBearbeitungErlaubt = false,
        TextBearbeitungErlaubt = true,
        DropDownItems = new(Array.Empty<string>()),
        DropdownWerteAndererZellenAnzeigen = false
    };

    public new static readonly ColumnFormatHolder PhoneNumber = new(FormatHolder.PhoneNumber) {
        Format = DataFormat.Text,
        Align = AlignmentHorizontal.Links,
        SortType = SortierTyp.Original_String,
        DoOpticalTranslation = TranslationType.Original_Anzeigen,
        AfterEditQuickSortRemoveDouble = false,
        BehaviorOfImageAndText = BildTextVerhalten.Nur_Text,
        ScriptType = ScriptType.String,
        DropdownAllesAbwählenErlaubt = false,
        DropdownBearbeitungErlaubt = false,
        TextBearbeitungErlaubt = true,
        DropDownItems = new(Array.Empty<string>()),
        DropdownWerteAndererZellenAnzeigen = false
    };

    public new static readonly ColumnFormatHolder SystemName = new(FormatHolder.SystemName) {
        Format = DataFormat.Text,
        Align = AlignmentHorizontal.Links,
        SortType = SortierTyp.Original_String,
        DoOpticalTranslation = TranslationType.Original_Anzeigen,
        AfterEditQuickSortRemoveDouble = false,
        BehaviorOfImageAndText = BildTextVerhalten.Nur_Text,
        ScriptType = ScriptType.String,
        DropdownAllesAbwählenErlaubt = false,
        DropdownBearbeitungErlaubt = false,
        TextBearbeitungErlaubt = true,
        DropDownItems = new(Array.Empty<string>()),
        DropdownWerteAndererZellenAnzeigen = false
    };

    public new static readonly ColumnFormatHolder Text = new(FormatHolder.Text) {
        Format = DataFormat.Text,
        Align = AlignmentHorizontal.Links,
        SortType = SortierTyp.Sprachneutral_String,
        DoOpticalTranslation = TranslationType.Original_Anzeigen,
        AfterEditQuickSortRemoveDouble = false,
        BehaviorOfImageAndText = BildTextVerhalten.Nur_Text,
        ScriptType = ScriptType.String,
        DropdownAllesAbwählenErlaubt = false,
        DropdownBearbeitungErlaubt = false,
        TextBearbeitungErlaubt = true,
        DropDownItems = new(Array.Empty<string>()),
        DropdownWerteAndererZellenAnzeigen = false
    };

    // ReSharper disable once UnusedMember.Global
    public new static readonly ColumnFormatHolder TextMitFormatierung = new(FormatHolder.TextMitFormatierung) {
        Format = DataFormat.Text,
        Align = AlignmentHorizontal.Links,
        SortType = SortierTyp.Sprachneutral_String,
        DoOpticalTranslation = TranslationType.Original_Anzeigen,
        AfterEditQuickSortRemoveDouble = false,
        BehaviorOfImageAndText = BildTextVerhalten.Nur_Text,
        ScriptType = ScriptType.String,
        DropdownAllesAbwählenErlaubt = false,
        DropdownBearbeitungErlaubt = false,
        TextBearbeitungErlaubt = true,
        DropDownItems = new(Array.Empty<string>()),
        DropdownWerteAndererZellenAnzeigen = false
    };

    public static readonly ColumnFormatHolder TextOptions = new(FormatHolder.Text) {
        Format = DataFormat.Text,
        Align = AlignmentHorizontal.Links,
        SortType = SortierTyp.Sprachneutral_String,
        DoOpticalTranslation = TranslationType.Übersetzen,
        AfterEditQuickSortRemoveDouble = true,
        BehaviorOfImageAndText = BildTextVerhalten.Nur_Text,
        ScriptType = ScriptType.String,
        DropdownAllesAbwählenErlaubt = true,
        DropdownBearbeitungErlaubt = true,
        TextBearbeitungErlaubt = true,
        DropDownItems = new(Array.Empty<string>()),
        DropdownWerteAndererZellenAnzeigen = true,
        MultiLine = true // Verhalten von Setformat überschreiben
    };

    public new static readonly ColumnFormatHolder Url = new(FormatHolder.Url) {
        Format = DataFormat.Text,
        Align = AlignmentHorizontal.Links,
        SortType = SortierTyp.Original_String,
        DoOpticalTranslation = TranslationType.Original_Anzeigen,
        AfterEditQuickSortRemoveDouble = false,
        BehaviorOfImageAndText = BildTextVerhalten.Nur_Text,
        ScriptType = ScriptType.String,
        DropdownAllesAbwählenErlaubt = false,
        DropdownBearbeitungErlaubt = false,
        TextBearbeitungErlaubt = true,
        DropDownItems = new(Array.Empty<string>()),
        DropdownWerteAndererZellenAnzeigen = false
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
    public BildTextVerhalten BehaviorOfImageAndText { get; set; }
    public TranslationType DoOpticalTranslation { get; set; }
    public bool DropdownAllesAbwählenErlaubt { get; set; }
    public bool DropdownBearbeitungErlaubt { get; set; }
    public ReadOnlyCollection<string> DropDownItems { get; set; } = new(Array.Empty<string>());
    public bool DropdownWerteAndererZellenAnzeigen { get; set; }
    public DataFormat Format { get; set; }
    public ScriptType ScriptType { get; set; }
    public SortierTyp SortType { get; set; }
    public bool TextBearbeitungErlaubt { get; set; }

    #endregion
}