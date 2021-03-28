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

namespace BlueBasics.Interfaces
{
    public static class IntegerArrayExtension
    {
        public static string[] ToStringArray(this int[] Ar)
        {
            var sar = new string[Ar.GetUpperBound(0) + 1];

            for (var z = 0; z <= Ar.GetUpperBound(0); z++)
            {
                sar[z] = Ar[z].ToString().Trim();
            }

            return sar;
        }
    }
}