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

using BlueScript.Structures;
using System.Collections.Generic;

namespace BlueScript.Methods;

internal class Method_End : Method {

    #region Properties

    public override List<List<string>> Args => new();

    public override string Description => "Beendet das Skript ohne Fehler.";

    public override bool EndlessArgs => false;

    public override string EndSequence => ";";

    public override bool GetCodeBlockAfter => false;

    public override string Returns => string.Empty;

    public override string StartSequence => "";

    //public Method_var(Script parent) : base(parent) { }
    public override string Syntax => "End;";

    #endregion

    #region Methods

    public override List<string> Comand(Script? s) => new() { "end" };

    public override DoItFeedback DoIt(CanDoFeedback infos, Script s, int line) {
        s.EndScript = true;
        return DoItFeedback.Null(line);
    }

    #endregion
}