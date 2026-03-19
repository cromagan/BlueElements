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

namespace BlueTable.Enums;

/// <summary>
/// Definiert die bekannten System-Spaltennamen.
/// </summary>
public static class SystemColumnName {
    public const string RowState = "SYS_ROWSTATE";
    public const string DateChanged = "SYS_DATECHANGED";
    public const string Changer = "SYS_CHANGER";
    public const string DateCreated = "SYS_DATECREATED";
    public const string Creator = "SYS_CREATOR";
    public const string Correct = "SYS_CORRECT";
    public const string Locked = "SYS_LOCKED";
    public const string RowKey = "SYS_ROWKEY";

    // Veraltete Spalten (TODO: Entfernen)
    public const string RowColor_Obsolete = "SYS_ROWCOLOR";
    public const string Chapter_Obsolete = "SYS_CHAPTER";

    // Alte Alternativnamen
    public const string ChangeDate_Alt = "SYS_CHANGEDATE";
    public const string CreateDate_Alt = "SYS_CREATEDATE";
}
