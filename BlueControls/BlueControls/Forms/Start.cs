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

using BlueBasics;
using BlueBasics.Enums;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using System;
using System.Reflection;
using static BlueBasics.Converter;

#nullable enable

namespace BlueControls.Forms {

    public partial class Start : FormWithStatusBar, IUniqueWindow {

        #region Constructors

        public Start() : base() {
            InitializeComponent();

            //var types = Generic.GetTypesOfType<IIsStandalone>();

            var methods = Generic.GetMethodsWithAttribute<StandaloneInfo>();

            foreach (var thisType in methods) {
                var name = thisType.Name;
                QuickImage i = QuickImage.Get(ImageCode.Fragezeichen);
                string kat = "Sonstiges";
                int sort = 200;

                var attr = thisType.GetCustomAttribute<StandaloneInfo>();
                if (attr != null) {
                    name = attr.Name;
                    i = attr.Image;
                    kat = attr.Kategorie;
                    sort = attr.Sort;
                }

                if (Forms[kat] is { } cap) {
                    var tmp = Math.Min(IntParse(cap.UserDefCompareKey) / 10, sort);
                    cap.UserDefCompareKey = tmp.ToStringInt10() + "0";
                } else {
                    var pk = new TextListItem(kat, kat, null, true, true, sort.ToStringInt10() + "0");
                    Forms.ItemAdd(pk);
                }

                var p = new BitmapListItem(i, string.Empty, name) {
                    Padding = 5,
                    Tag = thisType,
                    UserDefCompareKey = sort.ToStringInt10() + "1" + name
                };

                //var p = new TextListItem(name, string.Empty, QuickImage.Get(i, 24), false, true, sort.ToStringInt10() + "1" + name) {
                //    Tag = thisType
                //};
                Forms.ItemAdd(p);
            }
        }

        #endregion

        #region Properties

        public object? Object { get; set; }

        #endregion

        #region Methods

        private void Forms_ItemClicked(object sender, EventArgs.AbstractListItemEventArgs e) {
            if (e.Item.Tag is MethodInfo methodInfo) {
                var result = methodInfo.Invoke(null, null);
                if (result is Form form) {
                    FormManager.RegisterForm(form);
                    form.Show();
                    Close();
                    form.BringToFront();
                }
            }

            //if (e.Item.Tag is not Type t) { return; }

            //var instance = (IIsStandalone)Activator.CreateInstance(t);

            //if (instance is System.Windows.Forms.Form frm) {
            //    FormManager.RegisterForm(frm);
            //    frm.Show();
            //    Close();
            //    frm.BringToFront();
            //}
        }

        #endregion
    }
}