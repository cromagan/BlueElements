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

internal class ImageFilter_Contrast : ImageFilter {

    #region Properties

    public override string KeyName => "Contrast";

    #endregion

    #region Methods

    public override void ProcessFilter(BitmapData bitmapData, byte[] bits, float factor, int bias) {
        factor = (100.0f + factor) / 100.0f;
        factor *= factor;

        // Schleife über alle Pixel im Bild
        for (var i = 0; i < bits.Length; i += 4) {
            // Extrahieren der einzelnen Farbkomponenten aus dem Pixel
            var a = bits[i + 3];
            var r = bits[i + 2];
            var g = bits[i + 1];
            var b = bits[i];

            // Anpassen des Kontrasts für jede Farbkomponente und Begrenzen der Farbwerte
            r = (byte)Math.Max(0, Math.Min(255, ((((r / 255f) - 0.5f) * factor) + 0.5f) * 255.0f));
            g = (byte)Math.Max(0, Math.Min(255, ((((g / 255f) - 0.5f) * factor) + 0.5f) * 255.0f));
            b = (byte)Math.Max(0, Math.Min(255, ((((b / 255f) - 0.5f) * factor) + 0.5f) * 255.0f));

            // Aktualisieren der Farbkomponenten im Array
            bits[i + 2] = r;
            bits[i + 1] = g;
            bits[i] = b;
            // Alpha-Kanal bleibt unverändert (bits[i + 3])
        }
    }

    #endregion
}