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

using System.Collections.Generic;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;

namespace BlueDatabase.AdditionalScriptMethods;

// ReSharper disable once UnusedType.Global
public class Method_UniqueRowId : Method_Database {

    #region Properties

    public override List<List<string>> Args => [];
    public override string Command => "uniquerowid";
    public override string Description => "Gibt eine systemweit einzigartige Zeilen-ID aller geladenen Datenbanken aus.";
     public override int LastArgMinCount => -1;
    public override bool GetCodeBlockAfter => false;
    public override MethodType MethodType => MethodType.ChangeAnyDatabaseOrRow | MethodType.NeedLongTime;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "UniqueRowID()";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, CanDoFeedback infos, ScriptProperties scp) {
        var attvar = SplitAttributeToVars(varCol, infos.AttributText, Args, LastArgMinCount, infos.Data, scp);
        return !string.IsNullOrEmpty(attvar.ErrorMessage)
            ? DoItFeedback.AttributFehler(infos.Data, this, attvar)
            : new DoItFeedback(RowCollection.UniqueKeyValue());
    }

    #endregion
}