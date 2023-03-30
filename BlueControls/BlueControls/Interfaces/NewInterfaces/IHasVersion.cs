// Authors:
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

using BlueBasics.Interfaces;
using BlueDatabase;
using BlueDatabase.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace BlueControls.Interfaces;

public interface IHasVersion {

    #region Properties

    public int Version { get; set; }

    #endregion
}

public static class IHasVersionExtensions {

    #region Methods

    public static void RaiseVersion(this IHasVersion item) {
        if (item.Version == int.MaxValue) { item.Version = 0; }
        item.Version++;
    }

    #endregion
}