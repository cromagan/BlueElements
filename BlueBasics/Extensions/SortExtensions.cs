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

using System;
using System.Collections.Generic;
using System.Linq;

namespace BlueBasics;

public static partial class Extensions {

    #region Methods

    public static List<string> SortedDistinctList(this IEnumerable<string>? arr) {
        if (arr == null || !arr.Any()) { return new(); }

        var arr2 = arr.Distinct().ToList();
        if (arr2.Contains(string.Empty)) { _ = arr2.Remove(string.Empty); }
        arr2.Sort();
        return arr2;
    }

    public static List<T> SortedDistinctList<T>(this IEnumerable<T> arr) where T : IComparable {
        if (arr == null || !arr.Any()) { return new List<T>(); }

        var arr2 = arr.Distinct().ToList();
        //if (arr2.Contains(null)) { _ = arr2.Remove(null); }
        arr2.Sort();
        return arr2;
    }

    #endregion
}