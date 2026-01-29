// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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
using System.Runtime.InteropServices;

namespace BlueBasics.Classes.BitmapExt_ImageFilters;

internal class ImageFilter_Blur : ImageFilter {

    #region Properties

    public override string KeyName => "Blur";

    #endregion

    #region Methods

    public override void ProcessFilter(BitmapData bitmapData, byte[] bits, float factor, int bias) {
        // Größe des Blur-Kernels basierend auf dem Faktor
        var kernelSize = (int)factor;

        // Temporärer Buffer für das Ergebnis (um Original nicht während Berechnung zu überschreiben)
        var tempBits = new byte[bits.Length];
        Array.Copy(bits, tempBits, bits.Length);

        // Schleife über alle Pixel im Array
        for (var x = 0; x < bitmapData.Width; x++) {
            for (var y = 0; y < bitmapData.Height; y++) {
                // Initialisierung der Summen für die Farbkanäle
                int sumR = 0, sumG = 0, sumB = 0;
                var count = 0;

                // Schleife über den Kernel um den aktuellen Pixel herum
                for (var dx = -kernelSize; dx <= kernelSize; dx++) {
                    for (var dy = -kernelSize; dy <= kernelSize; dy++) {
                        // Berechnung der Koordinaten des aktuellen Pixels im Originalbild
                        var newX = x + dx;
                        var newY = y + dy;

                        // Überprüfen, ob der Pixel im Bereich des Bildes liegt
                        if (newX >= 0 && newX < bitmapData.Width && newY >= 0 && newY < bitmapData.Height) {
                            // Berechnung des Index im Byte-Array
                            var pixelIndex = newY * bitmapData.Stride + newX * 4;

                            // Addieren der Farbwerte zum Summenkanal
                            sumR += bits[pixelIndex + 2];
                            sumG += bits[pixelIndex + 1];
                            sumB += bits[pixelIndex];
                            count++;
                        }
                    }
                }

                // Berechnung des Durchschnitts für jeden Farbkanal
                var avgR = sumR / count;
                var avgG = sumG / count;
                var avgB = sumB / count;

                // Berechnung des Index für den aktuellen Pixel
                var currentPixelIndex = y * bitmapData.Stride + x * 4;

                // Setzen des neuen Farbwerts im temporären Buffer
                tempBits[currentPixelIndex + 2] = (byte)avgR;
                tempBits[currentPixelIndex + 1] = (byte)avgG;
                tempBits[currentPixelIndex] = (byte)avgB;
            }
        }

        // Kopiere das Ergebnis zurück ins Original-Array
        Array.Copy(tempBits, bits, bits.Length);
        Marshal.Copy(bits, 0, bitmapData.Scan0, bits.Length);
    }


    #endregion
}