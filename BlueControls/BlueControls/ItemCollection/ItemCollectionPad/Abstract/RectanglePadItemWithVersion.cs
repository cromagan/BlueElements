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

using BlueBasics;
using System.Collections.Generic;
using static BlueBasics.Converter;

namespace BlueControls.ItemCollection;

public abstract class RectanglePadItemWithVersion : RectanglePadItem {

    #region Constructors

    protected RectanglePadItemWithVersion(string internalname) : base(internalname) { }

    #endregion

    #region Properties

    public int Version { get; set; }

    #endregion

    #region Methods

    public string DefaultItemToControlName() => KeyName + "-" + Version;

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
        var result = new List<string>();
        result.ParseableAdd("Version", Version);
        return result.Parseable(base.ToString());
    }

    #endregion
}