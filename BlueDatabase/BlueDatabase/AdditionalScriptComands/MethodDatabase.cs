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

using BlueBasics;
using BlueDatabase;

namespace BlueScript {

    public abstract class MethodDatabase : Method {

        #region Methods

        protected ColumnItem Column(Script s, string name) => MyDatabase(s)?.Column.Exists(name);

        protected Database DatabaseOf(Script s, string name) {
            var f = s.Variablen.GetSystem("filename");
            if (f == null) { return null; }

            var newf = f.ValueString.FilePath() + name + ".mdb";

            return Database.GetByFilename(newf, true, false);
        }

        protected Database MyDatabase(Script s) {
            var f = s.Variablen.GetSystem("filename");
            return f == null ? null : Database.GetByFilename(f.ValueString, true, false);
        }

        #endregion
    }
}