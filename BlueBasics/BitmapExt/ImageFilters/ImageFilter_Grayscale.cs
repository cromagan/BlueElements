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

internal class ImageFilter_Grayscale : ImageFilter {

    #region Properties

    public override string KeyName => "Grayscale";

    #endregion

    #region Methods

    public override void ProcessFilter(BitmapData bitmapData, ref byte[] bits, float factor, int bias) {
        // Schleife über alle Pixel im Bild
        unsafe {
            for (var i = 0; i < bits.Length; i += 4) {
                // Extrahieren der einzelnen Farbkomponenten aus dem Pixel
                var a = bits[i + 3];
                var r = bits[i + 2];
                var g = bits[i + 1];
                var b = bits[i];

                // Berechnung des Grauwerts mit der Luma-Formel
                var gray = (byte)((0.3 * r) + (0.59 * g) + (0.11 * b));

                // Setzen der Graustufenwerte für alle Farbkanäle
                bits[i] = gray;       // Blau
                bits[i + 1] = gray;   // Grün
                bits[i + 2] = gray;   // Rot
                // Alpha-Kanal bleibt unverändert (bits[i + 3])
            }
        }
    }

    #endregion
}