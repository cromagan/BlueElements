// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

namespace BlueControls.Enums {

    public enum enHintergrundArt {
        Unbekannt = -1,
        Ohne = 0,
        Solide = 2,
        Verlauf_Vertical_2 = 20,

        [Obsolete]
        Verlauf_Vertical_3 = 21,

        [Obsolete]
        Verlauf_Horizontal_2 = 30,

        [Obsolete]
        Verlauf_Horizontal_3 = 31,

        [Obsolete]
        Verlauf_Diagonal_3 = 41,

        [Obsolete]
        Glossy = 50,

        [Obsolete]
        Verlauf_Vertikal_Glanzpunkt = 51,

        [Obsolete]
        GlossyPressed = 52
    }
}