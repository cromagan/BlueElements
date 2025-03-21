﻿// Authors:
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

using BlueScript.Variables;

namespace BlueControls.Interfaces;

/// <summary>
/// Wird verwendet, wenn das PadItem mit Variablen umgehen kann und sich dadurch die Anzeige ändert.
/// </summary>
public interface ICanHaveVariables {

    #region Methods

    bool ReplaceVariable(Variable variable);

    bool ResetVariables();

    #endregion
}

public static class CanHaveVariables {

    #region Methods

    public static void ParseVariables(this ICanHaveVariables obj, VariableCollection? variables) {
        _ = obj.ResetVariables();
        if (variables == null) { return; }

        foreach (var thisV in variables) {
            _ = obj.ReplaceVariable(thisV);
        }
    }

    #endregion
}