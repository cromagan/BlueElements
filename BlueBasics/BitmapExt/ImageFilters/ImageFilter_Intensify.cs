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

using System.Drawing;

namespace BlueBasics;

internal class ImageFilter_Intensify : ImageFilter {

    #region Properties

    public override string KeyName => "Intensify";

    #endregion

    #region Methods

    public override void ProcessFilter(int width, int height, ref int[] bits, float factor, int bias) {
        // Schleife über alle Pixel im Array
        for (var x = 0; x < width; x++) {
            for (var y = 0; y < height; y++) {
                // Extrahieren der ARGB-Komponenten aus dem Integer-Wert
                var argb = bits[(y * width) + x];
                var a = (argb >> 24) & 0xff; // Alpha-Komponente
                var r = (argb >> 16) & 0xff; // Rot-Komponente
                var g = (argb >> 8) & 0xff; // Grün-Komponente
                var b = argb & 0xff; // Blau-Komponente

                // Überprüfen, ob der Pixel intensiviert werden muss
                if (a > 0.5 && GetBrightness(r, g, b) < 0.9) {
                    // Setzen des Pixels auf Schwarz
                    bits[(y * width) + x] = Color.Black.ToArgb();
                }
            }
        }
    }

    // Hilfsmethode zur Berechnung der Helligkeit eines Pixels
    private float GetBrightness(int r, int g, int b) => ((0.3f * r) + (0.59f * g) + (0.11f * b)) / 255.0f;

    #endregion
}