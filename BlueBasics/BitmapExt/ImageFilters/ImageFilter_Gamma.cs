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
using System.Drawing.Imaging;

namespace BlueBasics;

internal class ImageFilter_Gamma : ImageFilter {

    #region Properties

    public override string KeyName => "Gamma";

    #endregion

    #region Methods

    public override void ProcessFilter(BitmapData bitmapData, byte[] bits, float factor, int bias) {
        // Stellen Sie sicher, dass der Gamma-Wert gültig ist
        factor = Math.Max(factor, 0.001f);

        // Erstellen Sie eine Lookup-Tabelle für die Gamma-Korrektur
        var gammaArray = new byte[256];
        for (var i = 0; i < 256; ++i) {
            gammaArray[i] = (byte)Math.Min(255, (int)((255.0 * Math.Pow(i / 255.0, 1.0 / factor)) + 0.5));
        }

        // Schleife über alle Pixel im Bild
        for (var i = 0; i < bits.Length; i += 4) {
            // Extrahieren der einzelnen Farbkomponenten aus dem Pixel
            var r = bits[i + 2];
            var g = bits[i + 1];
            var b = bits[i];

            // Gamma-Korrektur für jede Farbkomponente
            bits[i + 2] = gammaArray[r]; // Rot-Komponente
            bits[i + 1] = gammaArray[g]; // Grün-Komponente
            bits[i] = gammaArray[b];      // Blau-Komponente
            // Alpha-Kanal bleibt unverändert (bits[i + 3])
        }
    }

    #endregion
}