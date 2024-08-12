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

using System.Drawing.Imaging;

namespace BlueBasics;

internal class ImageFilter_Invert : ImageFilter {

    #region Properties

    public override string KeyName => "Invert";

    #endregion

    #region Methods

    public override void ProcessFilter(BitmapData bitmapData, byte[] bits, float factor, int bias) {
        // Schleife über alle Pixel im Bild
        for (var i = 0; i < bits.Length; i += 4) {
            // Extrahieren der einzelnen Farbkomponenten aus dem Pixel
            var r = bits[i + 2];
            var g = bits[i + 1];
            var b = bits[i];

            // Invertieren der Farbwerte für jeden Farbkanal
            r = (byte)(255 - r);
            g = (byte)(255 - g);
            b = (byte)(255 - b);

            // Setzen der invertierten Farbwerte für den aktuellen Pixel
            bits[i + 2] = r; // Rot-Komponente
            bits[i + 1] = g; // Grün-Komponente
            bits[i] = b;     // Blau-Komponente
            // Alpha-Kanal bleibt unverändert (bits[i + 3])
        }
    }

    #endregion
}