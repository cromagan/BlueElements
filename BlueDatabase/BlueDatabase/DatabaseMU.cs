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

#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.MultiUserFile;
using BlueDatabase.Enums;
using static BlueBasics.Converter;
using static BlueBasics.Generic;
using static BlueBasics.IO;

namespace BlueDatabase;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
internal class DatabaseMU : Database {

    #region Constructors

    public DatabaseMU(string filename, bool readOnly, string freezedReason, bool create, NeedPassword? needPassword) : base(filename, readOnly, freezedReason, create, needPassword) { }

    public DatabaseMU(ConnectionInfo ci, bool readOnly, NeedPassword? needPassword) : base(ci, readOnly, needPassword) { }

    #endregion

    #region Properties

    public new static string DatabaseId => nameof(DatabaseMU);

    #endregion

    #region Methods

    public new static DatabaseAbstract? CanProvide(ConnectionInfo ci, bool readOnly, NeedPassword? needPassword) {
        if (!DatabaseId.Equals(ci.DatabaseId, StringComparison.OrdinalIgnoreCase)) { return null; }

        if (string.IsNullOrEmpty(ci.AdditionalData)) { return null; }

        if (!FileExists(ci.AdditionalData)) { return null; }

        return new DatabaseMU(ci, readOnly, needPassword);
    }

    protected override IEnumerable<DatabaseAbstract> LoadedDatabasesWithSameServer() {
        var oo = new List<DatabaseMU>();

        if (string.IsNullOrEmpty(Filename)) { return oo; }

        var filepath = Filename.FilePath();

        foreach (var thisDb in AllFiles) {
            if (thisDb is DatabaseMU dbmu) {
                if (dbmu.Filename.FilePath().Equals(filepath, StringComparison.OrdinalIgnoreCase)) {
                    oo.Add(dbmu);
                }
            }
        }

        return oo;
    }

    #endregion
}