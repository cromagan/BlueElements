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

internal class ImageFilter_AllePixelZuSchwarz : ImageFilter {

    #region Properties

    public override string KeyName => "AllePixelZuSchwarz";

    #endregion

    #region Methods

    public override void ProcessFilter(int width, int height, ref int[] bits, float factor, int bias) {
        // Schleife über alle Pixel im Array
        for (var i = 0; i < bits.Length; i++) {
            // Extrahieren der ARGB-Komponenten aus dem Integer-Wert
            var argb = bits[i];
            var a = (argb >> 24) & 0xff; // Alpha-Komponente
            var r = (argb >> 16) & 0xff; // Rot-Komponente
            var g = (argb >> 8) & 0xff;  // Grün-Komponente
            var b = argb & 0xff;         // Blau-Komponente

            // Konvertieren der ARGB-Komponenten in ein Color-Objekt
            var ca = Color.FromArgb(a, r, g, b);

            // Überprüfen, ob die Farbe nahe Weiß ist
            if (!ca.IsNearWhite(factor)) {
                // Wenn nicht, setze die Farbe auf Schwarz
                bits[i] = Color.Black.ToArgb();
            }
        }
    }

    #endregion
}