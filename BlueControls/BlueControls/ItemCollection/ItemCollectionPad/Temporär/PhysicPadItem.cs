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

using System;
using System.Collections.Generic;

namespace BlueControls.ItemCollection {

    internal class PhysicPadItem : AbstractPhysicPadItem {

        #region Fields

        public readonly List<Kraft> Kraft = new();

        #endregion

        #region Constructors

        public PhysicPadItem(string internalname) : base(internalname) { }

        public PhysicPadItem() : base(string.Empty) { }

        #endregion

        #region Properties

        protected override int SaveOrder => 999;

        #endregion

        #region Methods

        public override void InitialPosition(int x, int y, int width, int height) { }

        protected override string ClassId() => "Physics-Object";

        protected override BasicPadItem? TryCreate(string id, string name) {
            if (id.Equals(ClassId(), StringComparison.OrdinalIgnoreCase)) {
                return new PhysicPadItem(name);
            }
            return null;
        }

        #endregion
    }
}