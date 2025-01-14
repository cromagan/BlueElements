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

internal class ImageFilter_Intensify : ImageFilter {

    #region Properties

    public override string KeyName => "Intensify";

    #endregion

    #region Methods

    public override void ProcessFilter(BitmapData bitmapData, byte[] bits, float factor, int bias) {
        // Schleife über alle Pixel im Bild
        for (var y = 0; y < bitmapData.Height; y++) {
            for (var x = 0; x < bitmapData.Width; x++) {
                // Berechnen des Index für den aktuellen Pixel im Byte-Array
                var index = (y * bitmapData.Stride) + (x * 4);

                // Extrahieren der einzelnen Farbkomponenten aus dem Pixel
                var a = bits[index + 3];
                var r = bits[index + 2];
                var g = bits[index + 1];
                var b = bits[index];

                // Überprüfen, ob der Pixel intensiviert werden muss
                if (a > 127 && GetBrightness(r, g, b) < 0.9) { // a > 127 entspricht etwa einem Alphawert von 0.5
                    // Setzen des Pixels auf Schwarz
                    bits[index] = 0;        // Blau
                    bits[index + 1] = 0;    // Grün
                    bits[index + 2] = 0;    // Rot
                }
            }
        }
    }

    // Hilfsmethode zur Berechnung der Helligkeit eines Pixels
    private float GetBrightness(int r, int g, int b) => ((0.3f * r) + (0.59f * g) + (0.11f * b)) / 255.0f;

    #endregion
}