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

namespace BlueControls.Enums;

public enum CheckBehavior {

    /// <summary>
    /// Alles Einträge, die angezeigt werden, zählen als ausgewählt.
    /// </summary>
    AllSelected = 0,

    /// <summary>
    /// Erlaubt das Auswählen eines oder keines Eintrages. Sozusagen Cancel erlaubt.
    /// </summary>
    SingleSelection = 1,

    /// <summary>
    /// Erlaubt das Auswählen einer beliebigen Anzahl an Einträgen. Sozusagen Cancel erlaubt.
    /// </summary>
    MultiSelection = 2,

    ///// <summary>
    ///// Es muss genau ein Eintrag gewählt sein. Sozusagen -kein- Cancel erlaubt.
    ///// </summary>
    //AlwaysSingleSelection = 3,

    /// <summary>
    /// Es können keine Einträge gewählt werden. Es können nur Werte angeklickt werden.
    /// </summary>
    NoSelection = 4
}