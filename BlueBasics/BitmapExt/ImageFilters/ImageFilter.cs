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

#nullable enable

using System;
using System.Drawing;
using System.Threading.Tasks;
using BlueBasics.Interfaces;

namespace BlueBasics;

public abstract class ImageFilter : IHasKeyName {

    #region Properties

    public abstract string KeyName { get; }

    #endregion

    #region Methods

    public static void ApplyMatrix(int width, int height, ref int[] bits, double[,] matrix) => ApplyMatrix(width, height, ref bits, matrix, 1, 0);

    public static void ApplyMatrix(int width, int height, ref int[] bits, double[,] matrix, double factor, int bias) {
        // Überprüfen, ob das Bits-Array initialisiert ist
        if (bits.Length != width * height) {
            return;
        }

        // Erstellen einer temporären Kopie des Bits-Arrays
        var tempBits = (int[])bits.Clone();

        var matrixHeight = matrix.GetLength(0);
        var matrixWidth = matrix.GetLength(1);
        var matrixOffsetY = matrixHeight / 2;
        var matrixOffsetX = matrixWidth / 2;

        // Durch das Bits-Array iterieren und die Matrix anwenden
        Parallel.For(matrixOffsetY, height - matrixOffsetY, y => {
            for (var x = matrixOffsetX; x < width - matrixOffsetX; x++) {
                double r = 0, g = 0, b = 0;

                for (var ky = 0; ky < matrixHeight; ky++) {
                    for (var kx = 0; kx < matrixWidth; kx++) {
                        var offsetX = x + kx - matrixOffsetX;
                        var offsetY = y + ky - matrixOffsetY;

                        if (offsetX >= 0 && offsetX < width && offsetY >= 0 && offsetY < height) {
                            var index = (offsetY * width) + offsetX;
                            var color = Color.FromArgb(tempBits[index]);

                            r += color.R * matrix[ky, kx];
                            g += color.G * matrix[ky, kx];
                            b += color.B * matrix[ky, kx];
                        }
                    }
                }

                var resultIndex = (y * width) + x;
                var red = (byte)Math.Min(Math.Max((int)((r / factor) + bias), 0), 255);
                var green = (byte)Math.Min(Math.Max((int)((g / factor) + bias), 0), 255);
                var blue = (byte)Math.Min(Math.Max((int)((b / factor) + bias), 0), 255);

                // Sperren ist nicht notwendig, da jeder Thread auf unterschiedliche Pixel schreibt
                tempBits[resultIndex] = Color.FromArgb(red, green, blue).ToArgb();
            }
        });

        // Aktualisieren des ursprünglichen Bits-Arrays
        bits = tempBits;
    }

    public abstract void ProcessFilter(int width, int height, ref int[] bits, float factor, int bias);

    #endregion
}