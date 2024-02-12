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

using System;

namespace BlueBasics;

internal class ImageFilter_Contrast : ImageFilter {

    #region Properties

    public override string KeyName => "Contrast";

    #endregion

    #region Methods

    public override void ProcessFilter(int width, int height, ref int[] bits, float factor, int bias) {
        factor = (100.0f + factor) / 100.0f;
        factor *= factor;

        for (var i = 0; i < bits.Length; i++) {
            var argb = bits[i];

            // Extrahieren der einzelnen Farbkomponenten aus dem Pixel
            var a = (argb >> 24) & 0xff;
            var r = (argb >> 16) & 0xff;
            var g = (argb >> 8) & 0xff;
            var b = argb & 0xff;

            // Anpassen des Kontrasts für jede Farbkomponente und Begrenzen der Farbwerte
            r = (int)Math.Max(0, Math.Min(255, ((((r / 255f) - 0.5f) * factor) + 0.5f) * 255.0f));
            g = (int)Math.Max(0, Math.Min(255, ((((g / 255f) - 0.5f) * factor) + 0.5f) * 255.0f));
            b = (int)Math.Max(0, Math.Min(255, ((((b / 255f) - 0.5f) * factor) + 0.5f) * 255.0f));

            // Kombinieren der Komponenten zurück in ein Pixel und direkt im Array aktualisieren
            bits[i] = (a << 24) | (r << 16) | (g << 8) | b;
        }
    }

    #endregion
}