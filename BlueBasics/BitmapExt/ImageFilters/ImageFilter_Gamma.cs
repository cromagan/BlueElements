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

internal class ImageFilter_Gamma : ImageFilter {

    #region Properties

    public override string KeyName => "Gamma";

    #endregion

    #region Methods

    public override void ProcessFilter(int width, int height, ref int[] bits, float factor, int bias) {
        // Stellen Sie sicher, dass der Gamma-Wert gültig ist
        factor = Math.Max(factor, 0.001f);

        // Erstellen Sie eine Lookup-Tabelle für die Gamma-Korrektur
        var gammaArray = new int[256];
        for (var i = 0; i < 256; ++i) {
            gammaArray[i] = Math.Min(255, (int)((255.0 * Math.Pow(i / 255.0, 1.0 / factor)) + 0.5));
        }

        // Anpassen des Gammas für jedes Pixel im Bits-Array
        for (var i = 0; i < bits.Length; i++) {
            var argb = bits[i];
            var a = (argb >> 24) & 0xff; // Alpha-Komponente
            var r = (argb >> 16) & 0xff; // Rot-Komponente
            var g = (argb >> 8) & 0xff;  // Grün-Komponente
            var b = argb & 0xff;         // Blau-Komponente

            // Gamma-Korrektur für jede Farbkomponente
            r = gammaArray[r];
            g = gammaArray[g];
            b = gammaArray[b];

            // Kombinieren der Komponenten zurück in ein Pixel und direkt im Array aktualisieren
            bits[i] = (a << 24) | (r << 16) | (g << 8) | b;
        }
    }

    #endregion
}