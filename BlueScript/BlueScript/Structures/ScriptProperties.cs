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

using BlueScript.Enums;

namespace BlueScript.Structures;

public class ScriptProperties {

    #region Constructors

    public ScriptProperties(MethodType allowedMethods, bool changeValues, string scriptAttributes) {
        AllowedMethods = allowedMethods;
        ChangeValues = changeValues;
        ScriptAttributes = scriptAttributes;
    }

    public ScriptProperties(ScriptProperties scriptProperties, MethodType allowedMethods) {
        AllowedMethods = allowedMethods;
        ChangeValues = scriptProperties.ChangeValues;
        ScriptAttributes = scriptProperties.ScriptAttributes;
    }

    #endregion

    #region Properties

    public bool ChangeValues { get; }
    public string ScriptAttributes { get; }
    internal MethodType AllowedMethods { get; }

    #endregion
}