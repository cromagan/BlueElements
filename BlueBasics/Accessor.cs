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
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BlueBasics {
    // https://stackoverflow.com/questions/32901771/multiple-enum-descriptions
    // https://stackoverflow.com/questions/1402803/passing-properties-by-reference-in-c-sharp

    public class Accessor<T> {

        #region Fields

        public readonly bool CanRead = false;
        public readonly bool CanWrite = false;
        public readonly string Name = "[unbekannt]";
        public readonly string QuickInfo = string.Empty;
        public readonly string TypeFullname = string.Empty;
        private Func<T> Getter;
        private Action<T> Setter;

        #endregion

        #region Constructors

        public Accessor(Expression<Func<T>> expr) {
            var memberExpression = (MemberExpression)expr.Body;
            var instanceExpression = memberExpression.Expression;
            var parameter = Expression.Parameter(typeof(T));

            IEnumerable<Attribute> ca = null;

            if (memberExpression.Member is PropertyInfo propertyInfo) {
                Setter = Expression.Lambda<Action<T>>(Expression.Call(instanceExpression, propertyInfo.GetSetMethod(), parameter), parameter).Compile();
                Getter = Expression.Lambda<Func<T>>(Expression.Call(instanceExpression, propertyInfo.GetGetMethod())).Compile();
                CanWrite = propertyInfo.CanWrite;
                CanRead = propertyInfo.CanRead;
                Name = propertyInfo.Name;
                TypeFullname = propertyInfo.PropertyType.FullName;
                ca = propertyInfo.GetCustomAttributes();
            } else if (memberExpression.Member is FieldInfo fieldInfo) {
                Setter = Expression.Lambda<Action<T>>(Expression.Assign(memberExpression, parameter), parameter).Compile();
                Getter = Expression.Lambda<Func<T>>(Expression.Field(instanceExpression, fieldInfo)).Compile();
                CanWrite = !fieldInfo.IsInitOnly;
                CanRead = true;
                Name = fieldInfo.Name;
                TypeFullname = fieldInfo.FieldType.FullName;
                ca = fieldInfo.GetCustomAttributes();
            }

            if (ca != null) {
                foreach (var thisas in ca) {
                    if (thisas is DescriptionAttribute da) {
                        QuickInfo = da.Description;
                    }
                }
            }
        }

        #endregion

        #region Methods

        public T Get() => Getter();

        public void Set(T value) => Setter(value);

        #endregion
    }
}