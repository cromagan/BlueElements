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

using System;

namespace BlueBasics.Enums
{
    [Flags]
    public enum enDirection : byte
    {
        Nichts = 0,
        Oben = 1,
        Unten = 2,
        Links = 4,
        Rechts = 8,

        Oben_Links = Oben | Links,
        Oben_Rechts = Oben | Rechts,
        Unten_Links = Unten | Links,
        Unten_Rechts = Unten | Rechts
    }
}