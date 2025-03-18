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

using System.Collections.Generic;
using System.IO.Ports;
using BlueScript.Methods;

namespace BlueScript.Structures;

public class ScriptProperties {

    #region Constructors

    public ScriptProperties(string scriptname, List<Method> allowedMethods, bool produktivphase, List<string> scriptAttributes, object? additionalInfo, string chain, string mainInfo) {
        ScriptName = scriptname;
        AllowedMethods = allowedMethods;
        ProduktivPhase = produktivphase;
        ScriptAttributes = scriptAttributes;
        AdditionalInfo = additionalInfo;
        Stufe = 0;
        Chain = chain;
        MainInfo = mainInfo;
    }

    public ScriptProperties(ScriptProperties scriptProperties, List<Method> allowedMethods, int stufe, string chain) : this(scriptProperties.ScriptName, allowedMethods, scriptProperties.ProduktivPhase, scriptProperties.ScriptAttributes, scriptProperties.AdditionalInfo, chain, scriptProperties.MainInfo) {
        Stufe = stufe;
    }

    #endregion

    #region Properties

    public object? AdditionalInfo { get; }
    public List<Method> AllowedMethods { get; }
    public string Chain { get; } = string.Empty;

    public string MainInfo { get; } = string.Empty;
    public bool ProduktivPhase { get; }

    /// <summary>
    /// Diese Attriute muss das nachfolgende Script mindestens erfüllen
    /// </summary>
    public List<string> ScriptAttributes { get; }

    public string ScriptName { get; }
    public int Stufe { get; }

    #endregion
}