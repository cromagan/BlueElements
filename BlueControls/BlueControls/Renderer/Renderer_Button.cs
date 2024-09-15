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

using System.Collections.Generic;
using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueDatabase;
using BlueDatabase.Enums;

namespace BlueControls.CellRenderer;

// ReSharper disable once UnusedMember.Global
public class Renderer_Button : Renderer_Abstract {

    #region Fields

    private bool _bild_anzeigen;

    private bool _checkstatus_anzeigen;

    private bool _text_anzeigen;

    #endregion

    //public Renderer_Button() : base() { }

    #region Properties

    public static string ClassId => "Button";

    public bool Bild_anzeigen {
        get => _bild_anzeigen;
        set {
            if (_bild_anzeigen == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _bild_anzeigen = value;
            OnPropertyChanged();
        }
    }

    public bool CheckStatus_anzeigen {
        get => _checkstatus_anzeigen;
        set {
            if (_checkstatus_anzeigen == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _checkstatus_anzeigen = value;
            OnPropertyChanged();
        }
    }

    public override string Description => "Text wird immer einzeilig dargestellt.";

    public override string MyClassId => ClassId;

    public bool Text_anzeigen {
        get => _text_anzeigen;
        set {
            if (_text_anzeigen == value) { return; }
            if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
            _text_anzeigen = value;
            OnPropertyChanged();
        }
    }

    #endregion

    //public string Präfix {
    //    get => _präfix;
    //    set {
    //        if (_präfix == value) { return; }
    //        if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
    //        _präfix = value;
    //        OnPropertyChanged();
    //    }
    //}

    //public string Suffix {
    //    get => _suffix;
    //    set {
    //        if (_suffix == value) { return; }
    //        if (ReadOnly) { Develop.DebugPrint_ReadOnly(); return; }
    //        _suffix = value;
    //        OnPropertyChanged();
    //    }
    //}

    #region Methods

    public override void Draw(Graphics gr, string content, Rectangle drawarea, Design design, States state, TranslationType translate, Alignment align, float scale) {
        if (string.IsNullOrEmpty(content)) { return; }
        var font = Skin.DesignOf(design, state).BFont?.Scale(scale);
        if (font == null) { return; }

        var s = state;

        if (_checkstatus_anzeigen) {
            var t = (content + ";").SplitBy(";");
            if (_bild_anzeigen && t[1].FromPlusMinus()) {
                s |= States.Checked;
            } else if (!_bild_anzeigen && t[0].FromPlusMinus()) {
                s |= States.Checked;
            }
        }

        var replacedText = ValueReadable(content, ShortenStyle.Replaced, translate);
        var q = QImage(content);

        drawarea.Inflate(-Skin.PaddingSmal, -Skin.PaddingSmal);

        Button.DrawButton(null, gr, Design.Button_CheckBox, s, q, Alignment.Horizontal_Vertical_Center, false, null, replacedText, drawarea, true);

        //Skin.Draw_FormatedText(gr, replacedText, null, align, drawarea, font, false);
    }

    public override List<GenericControl> GetProperties(int widthOfControl) {
        List<GenericControl> result =
        [   new FlexiControlForProperty<bool>(() => Bild_anzeigen),
            new FlexiControlForProperty<bool>(() => CheckStatus_anzeigen),
                new FlexiControlForProperty<bool>(() => Text_anzeigen)
        ];
        return result;
    }

    public override bool ParseThis(string key, string value) {
        switch (key.ToLower()) {
            case "showpic":
                _bild_anzeigen = value.FromPlusMinus();
                return true;

            case "showtext":
                _text_anzeigen = value.FromPlusMinus();
                return true;

            case "showcheckstate":
                _checkstatus_anzeigen = value.FromPlusMinus();
                return true;
        }
        return true; // Immer true. So kann gefahrlos hin und her geschaltet werden und evtl. Werte aus anderen Renderen benutzt werden.
    }

    public override string ReadableText() => "Als Knopf anzeigen";

    public override QuickImage SymbolForReadableText() => QuickImage.Get(ImageCode.Textfeld2);

    public override string ToParseableString() {
        List<string> result = [];

        result.ParseableAdd("ShowPic", _bild_anzeigen);
        result.ParseableAdd("ShowText", _text_anzeigen);
        result.ParseableAdd("ShowCheckState", _checkstatus_anzeigen);
        return result.Parseable(base.ToParseableString());
    }

    protected override Size CalculateContentSize(string content, Design design, States state, TranslationType translate) {
        var font = Skin.DesignOf(design, state).BFont?.Font();

        if (font == null) { return new Size(16, 32); }
        var replacedText = ValueReadable(content, ShortenStyle.Replaced, translate);

        return font.FormatedText_NeededSize(replacedText, QImage(content), 32);
    }

    /// <summary>
    /// Gibt eine einzelne Zeile richtig ersetzt mit Prä- und Suffix zurück.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="style"></param>
    /// <param name="translate"></param>
    /// <returns></returns>
    protected override string CalculateValueReadable(string content, ShortenStyle style, TranslationType translate) {
        if (!_text_anzeigen) { return string.Empty; }

        content = content.Replace("\r\n", "; ");
        content = content.Replace("\r", "; ");

        var t = (content + ";;").SplitBy(";");

        string r;

        if (_bild_anzeigen && _checkstatus_anzeigen) {
            r = t[2];
        } else if (_bild_anzeigen && _checkstatus_anzeigen) {
            r = t[1];
        } else {
            r = t[0];
        }

        return LanguageTool.PrepaireText(r, style, string.Empty, string.Empty, translate, null);
    }

    private QuickImage? QImage(string content) {
        var t = content.SplitBy(";");

        if (_bild_anzeigen) {
            return QuickImage.Get(t[0]);
        }

        return null;
    }

    #endregion
}