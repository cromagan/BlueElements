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

using System;
using System.Collections.Generic;

namespace BlueDatabase;

/// <summary>
/// Print Views werden nicht immer benötigt. Deswegen werden sie als String gespeichert. Der richtige Typ wäre ItemCollectionPad.
/// Noch dazu ist ItemCollectionPad in BlueConrolls verankert, das nur die Sichtbarmachung einen Sinn macht.
/// Und diese Sichtbarmachung braucht braucht Controls für die Bearbeitung.
/// </summary>
public class LayoutCollection : List<string>, ICloneable {
    //public string ToString(
    //     return this.JoinWithCr();
    //    )

    //public void Check() {
    //    for (var z = 0; z < Count; z++) {
    //        if (!this[z].StartsWith("{ID=#")) {
    //            this[z] = "{ID=#Converted" + z + ", " + this[z].Substring(1);
    //        }
    //    }
    //}

    #region Methods

    public object Clone() {
        var l = new LayoutCollection();
        l.AddRange(this);
        return l;
    }

    public int LayoutCaptionToIndex(string layoutcaption) {
        for (var z = 0; z < Count; z++) {
            if (this[z].Contains("Caption=" + layoutcaption + ",")) { return z; }
            if (this[z].Contains("Caption=\"" + layoutcaption + "\",")) { return z; }
        }
        return -1;
    }

    #endregion

    // Info:
    // ExportDialog.AddLayoutsOff wandelt Layouts In Items um
    //public int LayoutIdToIndex(string exportFormularId) {
    //    for (var z = 0; z < Count; z++) {
    //        if (this[z].Contains("ID=" + exportFormularId + ",")) { return z; }
    //        if (this[z].Contains("ID=\"" + exportFormularId + "\",")) { return z; }
    //    }
    //    return -1;
    //}
}