﻿// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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

using BlueScript;
using Skript.Enums;
using System.Collections.Generic;

public struct strSplittedAttributesFeedback {

    #region Fields

    public List<Variable> Attributes;

    ///// <summary>
    ///// Die Position, wo der Fehler stattgefunfden hat ODER die Position wo weiter geparsesd werden muss
    ///// </summary>
    //public int ContinueOrErrorPosition;
    public string ErrorMessage;

    public enSkriptFehlerTyp FehlerTyp;

    #endregion

    #region Constructors

    public strSplittedAttributesFeedback(List<Variable> atts) {
        Attributes = atts;
        ErrorMessage = string.Empty;
        FehlerTyp = enSkriptFehlerTyp.ohne;
    }

    public strSplittedAttributesFeedback(enSkriptFehlerTyp type, string error) {
        Attributes = null;
        ErrorMessage = error;
        FehlerTyp = type;
    }

    #endregion

    ///// <summary>
    ///// TRUE, wenn der Befehl erkannt wurde, aber nicht ausgeführt werden kann.
    ///// </summary>
    //public bool MustAbort;
    ///// <summary>
    ///// Der Text, mit dem eingetiegen wird. Also der Behel mit dem StartString.
    ///// </summary>
    //public string ComandText;
    ///// <summary>
    ///// Der Text zwischen dem StartString und dem EndString
    ///// </summary>
    //public string AttributText;
    ///// <summary>
    ///// Falls ein Codeblock { } direkt nach dem Befehl beginnt, dessen Inhalt
    ///// </summary>
    //public string CodeBlockAfterText;
    //public int LineBreakInCodeBlock;
}