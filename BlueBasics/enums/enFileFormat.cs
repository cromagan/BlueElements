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
    public enum enFileFormat {
        Unknown = -1,

        WordKind = 1,

        Textdocument = 3,
        ExcelKind = 4,
        PowerPointKind = 5,
        EMail = 6,
        Pdf = 7,
        HTML = 8,
        Image = 9,
        CompressedArchive = 10,
        Movie = 11,
        Executable = 12,
        HelpFile = 13,
        Database = 14,
        XMLFile = 15,
        Visitenkarte = 16,

        Sound = 17,
        Icon = 18,

        ProgrammingCode = 19,
        Link = 20
    }
}