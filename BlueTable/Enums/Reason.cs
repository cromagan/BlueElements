// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

namespace BlueTable.Enums;

public enum Reason {
    SetCommand = 0,

    /// <summary>
    /// Es werden keine Werte invalidiert. Kein Undo geloggt. und auch keine Repairs oder Skripte ausgeführt.
    /// Allerdings wird der Freeze-State umgangen. Z.B. um eine Tabelle laden zu können
    /// Wird benutzt beim Laden einer Tabelle, beim Systemspalten befüllen, oder nichtspeicherbare Spalten zu befüllen
    /// </summary>
    NoUndo_NoInvalidate = 1,

    ///// <summary>
    ///// Wenn Daten von der Festplatte nachgeladen und nur verbucht werden sollen.
    ///// </summary>
    //UpdateChanges = 2,

    //AdditionalWorkAfterCommand = 3
}