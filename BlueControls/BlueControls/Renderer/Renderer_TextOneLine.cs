﻿// Authors:
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

using System.Collections.Generic;
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueDatabase;
using BlueDatabase.Enums;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.CellRenderer;

// ReSharper disable once UnusedMember.Global
public class Renderer_TextOneLine : Renderer_Abstract {

    #region Fields

    private string _präfix = string.Empty;

    private string _suffix = string.Empty;

    #endregion

    #region Properties

    public static string ClassId => "TextOneLine";

    public override string Description => "Text wird immer einzeilig dargestellt.";

    public string Präfix {
        get => _präfix;
        set {
            if (_präfix == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _präfix = value;
            OnPropertyChanged();
        }
    }

    public string Suffix {
        get => _suffix;
        set {
            if (_suffix == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _suffix = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Methods

    public override void Draw(Graphics gr, string content, Rectangle scaleddrawarea, TranslationType translate, Alignment align, float scale) {
        if (string.IsNullOrEmpty(content)) { return; }
        //var font = Skin.GetBlueFont(SheetStyle, PadStyles.Standard, States.Standard).Scale(SheetStyleScale);
        var replacedText = ValueReadable(content, ShortenStyle.Replaced, translate);

        Skin.Draw_FormatedText(gr, replacedText, null, align, scaleddrawarea, this.GetFont(scale), false);
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<AbstractListItem> cbxEinheit =
        [
            ItemOf("µm", ImageCode.Lineal),
            ItemOf("mm", ImageCode.Lineal),
            ItemOf("cm", ImageCode.Lineal),
            ItemOf("dm", ImageCode.Lineal),
            ItemOf("m", ImageCode.Lineal),
            ItemOf("km", ImageCode.Lineal),
            ItemOf("mm²", ImageCode.GrößeÄndern),
            ItemOf("m²", ImageCode.GrößeÄndern),
            ItemOf("µg", ImageCode.Gewicht),
            ItemOf("mg", ImageCode.Gewicht),
            ItemOf("g", ImageCode.Gewicht),
            ItemOf("kg", ImageCode.Gewicht),
            ItemOf("t", ImageCode.Gewicht),
            ItemOf("h", ImageCode.Uhr),
            ItemOf("min", ImageCode.Uhr),
            ItemOf("St.", ImageCode.Eins)
        ];

        List<GenericControl> result =
        [   new FlexiControlForProperty<string>(() => Präfix),
            new FlexiControlForProperty<string>(() => Suffix,cbxEinheit, true)
        ];
        return result;
    }

    public override List<string> ParseableItems() {
        List<string> result = [.. base.ParseableItems()];

        result.ParseableAdd("Prefix", _präfix);

        result.ParseableAdd("Suffix", _suffix);

        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key.ToLower()) {
            case "prefix":
                _präfix = value.FromNonCritical();
                return true;

            case "suffix":
                _suffix = value.FromNonCritical();
                return true;
        }
        return true; // Immer true. So kann gefahrlos hin und her geschaltet werden und evtl. Werte aus anderen Renderen benutzt werden.
    }

    public override string ReadableText() => "Einzeiliger Text";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Textfeld2);

    /// <summary>
    /// Status des Bildes (Disabled) wird geändert. Diese Routine sollte nicht innerhalb der Table Klasse aufgerufen werden.
    /// Sie dient nur dazu, das Aussehen eines Textes wie eine Zelle zu imitieren.
    /// </summary>
    ///
    protected override Size CalculateContentSize(string content, TranslationType translate) {
        var replacedText = ValueReadable(content, ShortenStyle.Replaced, translate);
        return this.GetFont().FormatedText_NeededSize(replacedText, null, 16);
    }

    /// <summary>
    /// Gibt eine einzelne Zeile richtig ersetzt mit Prä- und Suffix zurück.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="style"></param>
    /// <param name="translate"></param>
    /// <returns></returns>
    protected override string CalculateValueReadable(string content, ShortenStyle style, TranslationType translate) {
        //if (_bildTextverhalten == BildTextVerhalten.Nur_Bild && style != ShortenStyle.HTML) { return string.Empty; }

        content = LanguageTool.PrepaireText(content, style, _präfix, _suffix, translate, null);

        content = content.Replace("\r\n", "; ");
        content = content.Replace("\r", "; ");

        return content;
    }

    #endregion
}