#region BlueElements - a collection of useful tools, database and controls
// Authors:
// Christian Peter
//
// Copyright (c) 2020 Christian Peter
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
#endregion

namespace BlueBasics.Enums {
    public enum BlurType {
        Mean3x3,
        Mean5x5,
        Mean7x7,
        Mean9x9,

        MotionBlur5x5,
        MotionBlur7x7,
        MotionBlur9x9,

        MotionBlur7x7At45Degrees,
        MotionBlur7x7At135Degrees,

        MotionBlur9x9At45Degrees,
        MotionBlur9x9At135Degrees,

        MotionBlur5x5At45Degrees,
        MotionBlur5x5At135Degrees,

        GaussianBlur3x3,
        GaussianBlur5x5,

        Median3x3,
        Median5x5,
        Median7x7,
        Median9x9,
        Median11x11

    }
}
