// Authors:
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

using System.Drawing.Imaging;

namespace BlueBasics;

internal class ImageFilter_AllePixelZuSchwarz : ImageFilter {

    #region Properties

    public override string KeyName => "AllePixelZuSchwarz";

    #endregion

    #region Methods

    public override void ProcessFilter(BitmapData bitmapData, byte[] bits, float factor, int bias) {
        // Berechnung der Schwellenwerte für die Farben
        var threshold = (int)(255 * factor);

        // Schleife über alle Pixel im Bild
        for (var i = 0; i < bits.Length; i += 4) {
            // Extrahieren der ARGB-Komponenten aus dem Integer-Wert
            //var a = bits[i + 3]; // Alpha-Komponente
            var r = bits[i + 2]; // Rot-Komponente
            var g = bits[i + 1]; // Grün-Komponente
            var b = bits[i];     // Blau-Komponente

            // Berechnen der Helligkeit des Pixels
            var brightness = (0.3f * r) + (0.59f * g) + (0.11f * b);

            // Überprüfen, ob die Helligkeit nahe dem Weißwert liegt
            if (brightness < threshold) {
                // Wenn nicht, setzen Sie die Farbe auf Schwarz
                bits[i] = 0;       // Blau
                bits[i + 1] = 0;   // Grün
                bits[i + 2] = 0;   // Rot
            }
        }
    }

    #endregion
}