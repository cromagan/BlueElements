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

namespace BlueTable.Enums;

/// <summary>
/// In welchem Datenformat die Daten gelesen werden.
/// </summary>
public enum Routinen : byte {
    //[Obsolete("Wird zukünftig entfernt werden", false)]
    //Column = 0,

    //[Obsolete("Wird zukünftig entfernt werden", false)]
    //DatenAllgemein = 1,

    //[Obsolete("Wird zukünftig entfernt werden", false)]
    //CellFormat = 2,

    DatenAllgemeinUTF8 = 3,

    //[Obsolete("Wird zukünftig entfernt werden", false)]
    //CellFormatUTF8 = 4,

    //[Obsolete("Wird zukünftig entfernt werden", false)]
    //ColumnUTF8 = 5,

    //[Obsolete("Wird zukünftig entfernt werden", false)]
    //CellFormatUTF8_V400 = 6,

    //ColumnUTF8_V400 = 7,

    CellFormatUTF8_V401 = 8,

    ColumnUTF8_V401 = 9,

    [Obsolete("Wird zukünftig entfernt werden", false)]
    CellFormatUTF8_V402 = 10, // Ohne ColumnName, mit RowKey

    CellFormatUTF8_V403 = 11 // Mit ColumnName und RowKey
}