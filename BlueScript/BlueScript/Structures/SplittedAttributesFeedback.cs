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

using BlueScript.Enums;
using BlueScript.Variables;
using System;
using System.Collections.Generic;

namespace BlueScript.Structures;

public readonly struct SplittedAttributesFeedback {

    #region Constructors

    public SplittedAttributesFeedback(VariableCollection atts) {
        Attributes = atts;
        ErrorMessage = string.Empty;
        FehlerTyp = ScriptIssueType.ohne;
    }

    public SplittedAttributesFeedback(ScriptIssueType type, string error) {
        Attributes = new VariableCollection();
        ErrorMessage = error;
        FehlerTyp = type;
    }

    #endregion

    #region Properties

    public VariableCollection Attributes { get; }

    public string ErrorMessage { get; }

    public ScriptIssueType FehlerTyp { get; }

    #endregion

    #region Methods

    public string ReadableText(int varno) {
        if (varno < 0 || varno >= Attributes.Count) { return string.Empty; }

        if (Attributes[varno] is Variable vs) { return vs.ReadableText; }
        return string.Empty;
    }

    public bool ReadOnly(int varno) {
        if (varno < 0 || varno >= Attributes.Count) { return true; }

        if (Attributes[varno] is Variable vs) { return vs.ReadOnly; }
        return true;
    }

    public bool ValueBool(int varno) {
        if (varno < 0 || varno >= Attributes.Count) { return false; }

        if (Attributes[varno] is VariableBool vs) { return vs.ValueBool; }
        return false;
    }

    public DateTime ValueDate(int varno) {
        if (varno < 0 || varno >= Attributes.Count) { return default; }

        if (Attributes[varno] is VariableDateTime vs) { return vs.ValueDate; }
        return default;
    }

    public int ValueInt(int varno) {
        if (varno < 0 || varno >= Attributes.Count) { return 0; }

        if (Attributes[varno] is VariableFloat vs) { return vs.ValueInt; }
        return 0;
    }

    public List<string> ValueListString(int varno) {
        if (varno < 0 || varno >= Attributes.Count) { return new(); }

        if (Attributes[varno] is VariableListString vs) { return vs.ValueList; }
        return new();
    }

    public double ValueNum(int varno) {
        if (varno < 0 || varno >= Attributes.Count) { return 0; }

        if (Attributes[varno] is VariableFloat vs) { return vs.ValueNum; }
        return 0;
    }

    public string ValueString(int varno) {
        if (varno < 0 || varno >= Attributes.Count) { return string.Empty; }

        if (Attributes[varno] is VariableString vs) { return vs.ValueString; }
        return string.Empty;
    }

    #endregion
}