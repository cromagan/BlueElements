// Authors:
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

using BlueControls.ItemCollectionPad.Abstract;
using System.Collections.Generic;

namespace BlueControls.ItemCollectionPad.Temporär;

internal class PhysicPadItem : AbstractPhysicPadItem {

    #region Constructors

    public PhysicPadItem() : base(string.Empty) { }

    #endregion

    #region Properties

    public static string ClassId => "Physics-Object";
    public override string Description => string.Empty;
    public List<Kraft> Kraft { get; } = [];
    public override string MyClassId => ClassId;
    protected override int SaveOrder => 999;

    #endregion

    #region Methods

    public override void InitialPosition(int x, int y, int width, int height) { }

    #endregion

    //protected override AbstractPadItem? TryCreate(string id, string name) {
    //    if (id.Equals(ClassId, StringComparison.OrdinalIgnoreCase)) {
    //        return new PhysicPadItem(name);
    //    }
    //    return null;
    //}
}