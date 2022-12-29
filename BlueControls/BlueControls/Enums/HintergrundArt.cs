// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

namespace BlueControls.Enums;

public enum HintergrundArt {
    Unbekannt = -1,
    Ohne = 0,
    Solide = 2,
    Verlauf_Vertical_2 = 20,

    [Obsolete("Wird zukünftig entfernt werden", false)]
    Verlauf_Vertical_3 = 21,

    [Obsolete("Wird zukünftig entfernt werden", false)]
    Verlauf_Horizontal_2 = 30,

    [Obsolete("Wird zukünftig entfernt werden", false)]
    Verlauf_Horizontal_3 = 31,

    [Obsolete("Wird zukünftig entfernt werden", false)]
    Verlauf_Diagonal_3 = 41,

    [Obsolete("Wird zukünftig entfernt werden", false)]
    Glossy = 50,

    [Obsolete("Wird zukünftig entfernt werden", false)]
    Verlauf_Vertikal_Glanzpunkt = 51,

    [Obsolete("Wird zukünftig entfernt werden", false)]
    GlossyPressed = 52
}