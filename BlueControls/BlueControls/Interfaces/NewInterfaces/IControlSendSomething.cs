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
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Forms;
using BlueControls.ItemCollection;
using BlueDatabase;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BlueControls.Interfaces;

public interface IControlSendSomething {

    #region Properties

    public ReadOnlyCollection<IControlSendSomething> Childs { get; }
    public DatabaseAbstract? OutputDatabase { get; }

    #endregion

    #region Methods

    public void ChildAdd(IControlSendSomething c);

    #endregion
}

public static class IControlSendSomethingExtension {
}

public class ControlSendSomething {
}