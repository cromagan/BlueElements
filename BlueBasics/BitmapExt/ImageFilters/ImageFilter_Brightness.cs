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

internal class ImageFilter_Brightness : ImageFilter {

    #region Properties

    public override string KeyName => "Brightness";

    #endregion

    #region Methods

    public override void ProcessFilter( BitmapData bitmapData, byte[] bits, float factor, int bias) {
        factor = Math.Max(factor, 0.001f); // Stellen Sie sicher, dass die Helligkeit nicht 0 ist

        // Schleife über alle Pixel im Bild
        unsafe {
            for (var y = 0; y < bitmapData.Height; y++) {
                for (var x = 0; x < bitmapData.Width; x++) {
                    // Berechnung des Index für den aktuellen Pixel im Byte-Array
                    var index = (y * bitmapData.Stride) + (x * 4); // 4 Bytes pro Pixel (RGBA)

                    // Anpassen der Helligkeit für jede Farbkomponente (RGB)
                    bits[index] = (byte)Math.Max(0, Math.Min(255, bits[index] * factor)); // Blau
                    bits[index + 1] = (byte)Math.Max(0, Math.Min(255, bits[index + 1] * factor)); // Grün
                    bits[index + 2] = (byte)Math.Max(0, Math.Min(255, bits[index + 2] * factor)); // Rot
                    // Alpha-Kanal bleibt unverändert (bits[index + 3])
                }
            }
        }
    }

    #endregion
}