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

using System.Collections.Generic;

namespace BlueBasics;

internal class ImageFilter_Median : ImageFilter {

    #region Properties

    public override string KeyName => "Median";

    #endregion

    #region Methods

    public override void ProcessFilter(int width, int height, ref int[] bits, float factor, int bias) {
        // Größe des Median-Kernels basierend auf dem Faktor
        var kernelSize = (int)factor;

        // Kopie des Eingabe-Arrays, um Änderungen vorzunehmen
        var originalBits = (int[])bits.Clone();

        // Schleife über alle Pixel im Array
        for (var x = 0; x < width; x++) {
            for (var y = 0; y < height; y++) {
                // Liste zur Speicherung der Pixelwerte im Kernel
                var pixelValues = new List<int>();

                // Schleife über den Kernel um den aktuellen Pixel herum
                for (var dx = -kernelSize; dx <= kernelSize; dx++) {
                    for (var dy = -kernelSize; dy <= kernelSize; dy++) {
                        // Berechnung der Koordinaten des aktuellen Pixels im Originalbild
                        var newX = x + dx;
                        var newY = y + dy;

                        // Überprüfen, ob der Pixel im Bereich des Bildes liegt
                        if (newX >= 0 && newX < width && newY >= 0 && newY < height) {
                            // Extrahieren des Farbwerts des Pixels und Hinzufügen zur Liste
                            var argb = originalBits[(newY * width) + newX];
                            pixelValues.Add(argb);
                        }
                    }
                }

                // Sortieren der Pixelwerte
                pixelValues.Sort();

                // Berechnung des Medianwertes
                var medianIndex = pixelValues.Count / 2;
                var medianValue = pixelValues[medianIndex];

                // Setzen des Medianwerts für den aktuellen Pixel
                bits[(y * width) + x] = medianValue;
            }
        }
    }

    #endregion
}