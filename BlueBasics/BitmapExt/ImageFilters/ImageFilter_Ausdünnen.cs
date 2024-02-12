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
using System.Drawing;
using System.Drawing.Imaging;

namespace BlueBasics;

internal class ImageFilter_Ausdünnen : ImageFilter {

    #region Properties

    public override string KeyName => "Ausdünnen";

    #endregion

    #region Methods

    public override void ProcessFilter(BitmapData bitmapData, ref byte[] bits, float factor, int bias) {
        // Überprüfen der Eingabeparameter
        if (bits == null || bits.Length == 0 || bitmapData.Width <= 0 || bitmapData.Height <= 0 || factor <= 0) {
            throw new ArgumentException("Ungültige Eingabeparameter.");
        }

        var BlackIndex = -1;

        // Schleife über alle Zeilen im Bild
        for (var y = 0; y < bitmapData.Height; y++) {
            var darkPixelCountInRow = 0; // Zähler für dunkle Pixel in einer Zeile

            // Schleife über alle Pixel in der aktuellen Zeile
            for (var x = 0; x < bitmapData.Width; x++) {
                var index = (y * bitmapData.Width + x) * 4; // Index des aktuellen Pixels im bits-Array

                // Überprüfen, ob der aktuelle Pixel dunkel ist
                if (IsDarkPixel(bits[index], bits[index + 1], bits[index + 2])) {

                    if (darkPixelCountInRow == 0) { BlackIndex = index; }
                    darkPixelCountInRow++; // Inkrementieren des Zählers für dunkle Pixel in der Zeile

                } else {

                    // Überprüfen, ob die Anzahl der dunklen Pixel in der Zeile den Faktor erreicht hat
                    if (darkPixelCountInRow > 1) {
                        // Ersetzen des linksten dunklen Pixels durch Weiß
                        bits[BlackIndex] = 255;         // Blau-Komponente
                        bits[BlackIndex + 1] = 255;     // Grün-Komponente
                        bits[BlackIndex + 2] = 255;     // Rot-Komponente
                    }
                    darkPixelCountInRow = 0; // Zurücksetzen des Zählers, wenn ein heller Pixel gefunden wird
                }
            }
        }

        // Schleife über alle Spalten im Bild
        for (var x = 0; x < bitmapData.Width; x++) {
            var darkPixelCountInColumn = 0; // Zähler für dunkle Pixel in einer Spalte

            // Schleife über alle Pixel in der aktuellen Spalte
            for (var y = 0; y < bitmapData.Height; y++) {
                var index = (y * bitmapData.Width + x) * 4; // Index des aktuellen Pixels im bits-Array

                // Überprüfen, ob der aktuelle Pixel dunkel ist
                if (IsDarkPixel(bits[index], bits[index + 1], bits[index + 2])) {

                    if (darkPixelCountInColumn == 0) { BlackIndex = index;}
                    darkPixelCountInColumn++; // Inkrementieren des Zählers für dunkle Pixel in der Spalte


                } else {
                    // Überprüfen, ob die Anzahl der dunklen Pixel in der Spalte den Faktor erreicht hat
                    if (darkPixelCountInColumn > 1) {
                        // Ersetzen des obersten dunklen Pixels durch Weiß
                        bits[BlackIndex] = 255;         // Blau-Komponente
                        bits[BlackIndex + 1] = 255;     // Grün-Komponente
                        bits[BlackIndex + 2] = 255;     // Rot-Komponente
                    }
                    darkPixelCountInColumn = 0; // Zurücksetzen des Zählers, wenn ein heller Pixel gefunden wird
                }
            }
        }
    }

    // Hilfsmethode zur Überprüfung, ob ein Pixel dunkel ist
    private bool IsDarkPixel(byte b, byte g, byte r) {
        // Hier können Sie die Kriterien anpassen, um festzustellen, ob ein Pixel dunkel ist
        // Zum Beispiel könnten Sie die Helligkeit des Pixels überprüfen
        // Eine einfache Möglichkeit ist die Überprüfung, ob alle Kanäle unter einem bestimmten Schwellenwert liegen
        var threshold = 50; // Schwellenwert für die Dunkelheit

        return (r < threshold && g < threshold && b < threshold);
    }

    #endregion

    // Hilfsmethode zur Überprüfung, ob ein Pixel nahe Weiß ist
}