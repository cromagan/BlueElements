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

using static BlueBasics.Extensions;

#nullable enable

namespace BlueScript.Structures;

public class CurrentPosition {

    #region Constructors

    public CurrentPosition() : this("Main", 0) { }

    public CurrentPosition(string subname, int position) {
        Subname = subname;
        Position = position;
    }

    public CurrentPosition(CurrentPosition? cp) {
        Subname = cp?.Subname ?? "Main";
        Position = cp?.Position ?? 0;
        Protocol = cp?.Protocol ?? string.Empty;
        Chain = cp?.Chain ?? string.Empty;
    }

    public CurrentPosition(CurrentPosition? cp, string failedreason) : this(cp) {
        Protocol = failedreason + "\r\n" + Protocol;
    }

    #endregion

    #region Properties

    public string Chain { get; } = string.Empty;

    public int Position { get; } = -1;

    public string Protocol { get; } = string.Empty;

    public int Stufe => Chain.CountChar('\\', null);
    public string Subname { get; } = string.Empty;

    #endregion

    #region Methods

    public int Line(string normalizedScriptText) => normalizedScriptText.CountChar('¶', Position) + 1;

    #endregion
}