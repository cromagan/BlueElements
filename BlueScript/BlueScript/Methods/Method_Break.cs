﻿// Authors:
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

using System.Collections.Generic;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueScript.Methods;

internal class Method_Break : Method {

    #region Properties

    public override List<List<string>> Args => new();
    public override string Description => "Beendet eine Schleife oder Subroutine sofort.\r\nKann auch nur innerhalb von diesen verwendet werden.";

    public override bool EndlessArgs => false;

    public override string EndSequence => ";";

    public override bool GetCodeBlockAfter => false;

    public override string Returns => string.Empty;
    public override string StartSequence => "";

    public override string Syntax => "Break;";

    #endregion

    #region Methods

    public override List<string> Comand(List<Variable>? currentvariables) => new() { "break" };

    public override DoItFeedback DoIt(Script s, CanDoFeedback infos) {
        if (s.Sub < 1) { return new DoItFeedback(infos, "Break nur innerhalb einer Schleife oder Subroutine erlaubt."); }

        if (s.BreakFired) { return new DoItFeedback(infos, "Break doppelt ausgelöst."); }
        s.BreakFired = true;
        return DoItFeedback.Null(infos);
    }

    #endregion
}