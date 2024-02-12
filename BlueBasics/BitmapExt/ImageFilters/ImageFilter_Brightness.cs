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

internal class ImageFilter_Brightness : ImageFilter {

    #region Properties

    public override string KeyName => "Brightness";

    #endregion

    #region Methods

    public override void ProcessFilter(int width, int height, ref int[] bits, float factor, int bias) {
        factor = Math.Max(factor, 0.001f); // Stellen Sie sicher, dass die Helligkeit nicht 0 ist

        for (var i = 0; i < bits.Length; i++) {
            var argb = bits[i];

            // Extrahieren der einzelnen Farbkomponenten aus dem Pixel
            var a = (argb >> 24) & 0xff;
            var r = (argb >> 16) & 0xff;
            var g = (argb >> 8) & 0xff;
            var b = argb & 0xff;

            // Anpassen der Helligkeit für jede Farbkomponente
            r = (int)Math.Min(255, r * factor);
            g = (int)Math.Min(255, g * factor);
            b = (int)Math.Min(255, b * factor);

            // Kombinieren der Komponenten zurück in ein Pixel
            bits[i] = (a << 24) | (r << 16) | (g << 8) | b;
        }
    }

    #endregion
}