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

namespace BlueDatabase.EventArgs;

public class ProgressbarEventArgs : System.EventArgs {

    #region Constructors

    public ProgressbarEventArgs(string name, int current, int count, bool beginns, bool ends) {
        Name = name;
        Current = current;
        Count = count;
        Beginns = beginns;
        Ends = ends;
    }

    #endregion

    #region Properties

    public bool Beginns { get; }
    public int Count { get; }
    public int Current { get; }
    public bool Ends { get; }
    public string Name { get; }

    #endregion
}