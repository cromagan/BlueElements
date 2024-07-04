﻿// Authors:
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

using BlueScript.Enums;
using System.Collections.Generic;

namespace BlueScript.Structures;

public class ScriptProperties {

    #region Constructors

    public ScriptProperties() : this(string.Empty, MethodType.Standard, false, [], null,0) { }

    public ScriptProperties(string scriptname, MethodType allowedMethods, bool produktivphase, List<string> scriptAttributes, object? additionalInfo, int stufe) {
        ScriptName = scriptname;
        AllowedMethods = allowedMethods;
        ProduktivPhase = produktivphase;
        ScriptAttributes = scriptAttributes;
        AdditionalInfo = additionalInfo;
        Stufe = stufe;
    }

    public ScriptProperties(ScriptProperties scriptProperties, MethodType allowedMethods, int stufe) : this(scriptProperties.ScriptName, allowedMethods, scriptProperties.ProduktivPhase, scriptProperties.ScriptAttributes, scriptProperties.AdditionalInfo, stufe) { }

    #endregion

    #region Properties

    public int Stufe { get; }

    public object? AdditionalInfo { get; }
    public bool ProduktivPhase { get; }

    /// <summary>
    /// Diese Attriute muss das nachfolgende Script mindestens erfüllen
    /// </summary>
    public List<string> ScriptAttributes { get; }

    public string ScriptName { get; }

    internal MethodType AllowedMethods { get; }

    #endregion
}