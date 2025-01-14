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

#nullable enable

using System;
using System.Collections.Generic;
using System.Drawing;
using BlueScript.Enums;
using BlueScript.Variables;
using static BlueBasics.Converter;

namespace BlueScript.Structures;

public readonly struct SplittedAttributesFeedback {

    #region Constructors

    public SplittedAttributesFeedback(VariableCollection atts) {
        Attributes = atts;
        ErrorMessage = string.Empty;
        FehlerTyp = ScriptIssueType.ohne;
    }

    public SplittedAttributesFeedback(ScriptIssueType type, string error) {
        Attributes = [];
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

    public string MyClassId(int varno) {
        if (varno < 0 || varno >= Attributes.Count) { return string.Empty; }

        return Attributes[varno] is { } vs ? vs.MyClassId : string.Empty;
    }

    public string Name(int varno) {
        if (varno < 0 || varno >= Attributes.Count) { return string.Empty; }

        return Attributes[varno] is { } vs ? vs.KeyName : string.Empty;
    }

    public string ReadableText(int varno) {
        if (varno < 0 || varno >= Attributes.Count) { return string.Empty; }

        return Attributes[varno] is { } vs ? vs.ReadableText : string.Empty;
    }

    public bool ReadOnly(int varno) {
        if (varno < 0 || varno >= Attributes.Count) { return true; }

        return Attributes[varno] is not { ReadOnly: false };
    }

    public Bitmap? ValueBitmapGet(int varno) {
        if (varno < 0 || varno >= Attributes.Count) { return null; }

        return Attributes[varno] is VariableBitmap vs ? vs.ValueBitmap : null;
    }

    public bool ValueBoolGet(int varno) {
        if (varno < 0 || varno >= Attributes.Count) { return false; }

        return Attributes[varno] is VariableBool { ValueBool: true };
    }

    public DateTime? ValueDateGet(int varno) {
        if (varno < 0 || varno >= Attributes.Count) { return default; }

        if (Attributes[varno] is VariableString vs) {
            if (DateTimeTryParse(vs.ValueString, out var d)) { return d; }
        }
        return null;
    }

    public int ValueIntGet(int varno) {
        if (varno < 0 || varno >= Attributes.Count) { return 0; }

        return Attributes[varno] is VariableFloat vs ? vs.ValueInt : 0;
    }

    public List<string> ValueListStringGet(int varno) {
        if (varno < 0 || varno >= Attributes.Count) { return []; }

        return Attributes[varno] is VariableListString vs ? vs.ValueList : ([]);
    }

    public DoItFeedback? ValueListStringSet(int varno, List<string> value, LogData ld) {
        if (varno < 0 || varno >= Attributes.Count) { return DoItFeedback.WertKonnteNichtGesetztWerden(ld, varno); }

        if (Attributes[varno] is VariableListString vs) {
            vs.ValueList = value;
            return null;
        }
        return DoItFeedback.WertKonnteNichtGesetztWerden(ld, varno);
    }

    public double ValueNumGet(int varno) {
        if (varno < 0 || varno >= Attributes.Count) { return 0; }

        return Attributes[varno] is VariableFloat vs ? vs.ValueNum : 0;
    }

    public string ValueStringGet(int varno) {
        if (varno < 0 || varno >= Attributes.Count) { return string.Empty; }

        return Attributes[varno] is VariableString vs ? vs.ValueString : string.Empty;
    }

    internal DoItFeedback? ValueBoolSet(int varno, bool value, LogData ld) {
        if (varno < 0 || varno >= Attributes.Count) { return DoItFeedback.WertKonnteNichtGesetztWerden(ld, varno); }

        if (Attributes[varno] is VariableBool vs) {
            vs.ValueBool = value;
            return null;
        }
        return DoItFeedback.WertKonnteNichtGesetztWerden(ld, varno);
    }

    internal DoItFeedback? ValueNumSet(int varno, double value, LogData ld) {
        if (varno < 0 || varno >= Attributes.Count) { return DoItFeedback.WertKonnteNichtGesetztWerden(ld, varno); }

        if (Attributes[varno] is VariableFloat vs) {
            vs.ValueNum = value;
            return null;
        }
        return DoItFeedback.WertKonnteNichtGesetztWerden(ld, varno);
    }

    internal DoItFeedback? ValueStringSet(int varno, string value, LogData ld) {
        if (varno < 0 || varno >= Attributes.Count) { return DoItFeedback.WertKonnteNichtGesetztWerden(ld, varno); }

        if (Attributes[varno] is VariableString vs) {
            vs.ValueString = value;
            return null;
        }
        return DoItFeedback.WertKonnteNichtGesetztWerden(ld, varno);
    }

    #endregion
}