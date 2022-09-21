// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Controls;
using BlueControls.EventArgs;
using System;
using System.Collections.Generic;
using System.Drawing;
using static BlueBasics.Converter;

namespace BlueControls.ItemCollection;

public abstract class RectanglePadItemWithVersion : RectanglePadItem {

    #region Constructors

    protected RectanglePadItemWithVersion(string internalname) : base(internalname) { }

    #endregion

    #region Properties

    public int Version { get; set; } = 0;

    #endregion

    #region Methods

    public string DefaultItemToControlName() {
        return Internal + "-" + Version;
    }

    public override bool ParseThis(string tag, string value) {
        if (base.ParseThis(tag, value)) { return true; }
        switch (tag) {
            case "version":
                Version = IntParse(value);
                return true;
        }
        return false;
    }

    public void RaiseVersion() {
        if (Version == int.MaxValue) { Version = 0; }
        Version++;
    }

    public override string ToString() {
        var t = base.ToString();
        t = t.Substring(0, t.Length - 1) + ", ";
        t = t + "Version=" + Version + ", ";
        return t.Trim(", ") + "}";
    }

    #endregion
}