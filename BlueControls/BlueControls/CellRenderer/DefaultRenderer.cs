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

using System.Drawing;
using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueDatabase;
using BlueDatabase.Enums;

namespace BlueControls.CellRenderer;

public class DefaultRenderer : AbstractCellRenderer {

    #region Properties

    public override string KeyName => "Default";
    public override string QuickInfo => "Standard Anzeige für Zellen";

    #endregion

    #region Methods

    public override void Draw(Graphics gr, string content, Rectangle drawarea, Design design,
                                States state,
                                ColumnItem? column,
                                float scale) {
        if (column == null) { return; }
        if (string.IsNullOrEmpty(content)) { return; }
        var font = Skin.DesignOf(design, state).BFont;
        if (font == null) { return; }

        var pix16 = Table.GetPix(16, font, scale);

        if (!ShowMultiLine(content, column.MultiLine)) {
            DrawOneLine(gr, content, column, drawarea, 0, false, font, pix16, column.BehaviorOfImageAndText, state, scale);
        } else {
            var mei = content.SplitAndCutByCrAndBr();

            switch (column.BehaviorOfImageAndText) {
                case BildTextVerhalten.Nur_erste_Zeile_darstellen:
                    if (mei.Length > 1) {
                        DrawOneLine(gr, mei[0] + "...", column, drawarea, 0, false, font, pix16, BildTextVerhalten.Nur_Text, state, scale);
                    } else if (mei.Length == 1) {
                        DrawOneLine(gr, mei[0], column, drawarea, 0, false, font, pix16, BildTextVerhalten.Nur_Text, state, scale);
                    }
                    break;

                case BildTextVerhalten.Mehrzeilig_einzeilig_darsellen:
                    DrawOneLine(gr, mei.JoinWith("; "), column, drawarea, 0, false, font, pix16, BildTextVerhalten.Nur_Text, state, scale);
                    break;

                default: {
                        var y = 0;
                        for (var z = 0; z <= mei.GetUpperBound(0); z++) {
                            DrawOneLine(gr, mei[z], column, drawarea, y, z != mei.GetUpperBound(0), font, pix16, column.BehaviorOfImageAndText, state, scale);
                            y += CellItem.ContentSize(column.KeyName, mei[z], font, pix16 - 1, column.BehaviorOfImageAndText, column.Prefix, column.Suffix, column.DoOpticalTranslation, column.OpticalReplace, scale, column.ConstantHeightOfImageCode).Height;
                        }

                        break;
                    }
            }
        }
    }

    public override string ReadableText() => "Standard";

    public override QuickImage? SymbolForReadableText() => null;

    private static void DrawOneLine(Graphics gr, string drawString, ColumnItem column, Rectangle drawarea, int txtYPix, bool changeToDot, BlueFont font, int pix16, BildTextVerhalten bildTextverhalten, States state, float scale) {
        Rectangle r = new(drawarea.Left, drawarea.Top + txtYPix, drawarea.Width, pix16);

        if (r.Bottom + pix16 > drawarea.Bottom) {
            if (r.Bottom > drawarea.Bottom) { return; }
            if (changeToDot) { drawString = "..."; }// Die letzte Zeile noch ganz hinschreiben
        }

        var (text, qi) = CellItem.GetDrawingData(column.KeyName, drawString, bildTextverhalten, column.Prefix, column.Suffix, column.DoOpticalTranslation, column.OpticalReplace, scale, column.ConstantHeightOfImageCode);
        var tmpImageCode = qi;
        if (tmpImageCode != null) { tmpImageCode = QuickImage.Get(tmpImageCode, Skin.AdditionalState(state)); }

        Skin.Draw_FormatedText(gr, text, tmpImageCode, (Alignment)column.Align, r, null, false, font, false);
    }

    private bool ShowMultiLine(string txt, bool ml) {
        if (!ml) { return false; }

        if (txt.Contains("\r")) { return true; }
        return txt.Contains("<br>");
    }

    #endregion
}