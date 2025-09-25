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

using BlueBasics.Enums;
using BlueBasics.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BlueBasics;

public readonly struct FileOperationResult {

    #region Fields

    /// <summary>
    /// Signalisiert, dass die Operation wiederholt werden soll
    /// </summary>
    public static readonly FileOperationResult DoRetry = new(null, true);

    /// <summary>
    /// Erstellt ein erfolgreiches Ergebnis mit 'null' als Wert
    /// </summary>
    public static readonly FileOperationResult SuccessNull = new(null, false);

    /// <summary>
    /// Erfolgreiche Operation mit Rückgabewert 'false'
    /// </summary>
    public static readonly FileOperationResult ValueFalse = new(false, false);

    /// <summary>
    /// Erfolgreiche Operation mit Rückgabewert 'true'
    /// </summary>
    public static readonly FileOperationResult ValueTrue = new(true, false);

    public readonly bool Retry;
    public readonly object? ReturnValue;

    #endregion

    #region Constructors

    public FileOperationResult(object? returnValue, bool retry) {
        ReturnValue = returnValue;
        Retry = retry;
    }

    public FileOperationResult(object? returnValue) {
        ReturnValue = returnValue;
        Retry = false;
    }

    #endregion
}