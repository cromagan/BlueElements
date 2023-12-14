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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace BlueBasics;
// https://stackoverflow.com/questions/32901771/multiple-enum-descriptions
// https://stackoverflow.com/questions/1402803/passing-properties-by-reference-in-c-sharp

public class Accessor<T> {

    #region Fields

    private readonly Func<T>? _getter;
    private readonly Action<T>? _setter;

    #endregion

    #region Constructors

    public Accessor(Expression<Func<T>>? expr) {
        var memberExpression = (MemberExpression?)expr?.Body;
        var instanceExpression = memberExpression?.Expression;
        var parameter = Expression.Parameter(typeof(T));

        IEnumerable<Attribute>? ca = null;

        if (memberExpression?.Member is PropertyInfo propertyInfo) {
            var setm = propertyInfo.GetSetMethod();
            if (setm != null) {
                _setter = Expression.Lambda<Action<T>>(Expression.Call(instanceExpression, setm, parameter), parameter).Compile();
            }

            var getm = propertyInfo.GetGetMethod();
            if (getm != null) {
                _getter = Expression.Lambda<Func<T>>(Expression.Call(instanceExpression, getm)).Compile();
            }
            CanWrite = propertyInfo.CanWrite;
            CanRead = propertyInfo.CanRead;
            Name = propertyInfo.Name;
            //TypeFullname = propertyInfo.PropertyType.FullName;
            ca = propertyInfo.GetCustomAttributes();
        } else if (memberExpression?.Member is FieldInfo fieldInfo) {
            _setter = Expression.Lambda<Action<T>>(Expression.Assign(memberExpression, parameter), parameter).Compile();
            _getter = Expression.Lambda<Func<T>>(Expression.Field(instanceExpression, fieldInfo)).Compile();
            CanWrite = !fieldInfo.IsInitOnly;
            CanRead = true;
            Name = fieldInfo.Name;
            //TypeFullname = fieldInfo.FieldType.FullName;
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

    #region Properties

    public bool CanRead { get; }
    public bool CanWrite { get; }
    public string Name { get; } = "[unbekannt]";
    public string QuickInfo { get; } = string.Empty;

    #endregion

    #region Methods

    public T? Get() {
        if (_getter != null) { return _getter(); }
        Develop.DebugPrint("Getter ist null!");
        return default;
    }

    public void Set(T value) {
        if (_setter != null) {
            _setter(value);
        } else {
            Develop.DebugPrint("Setter ist null!");
        }
    }

    #endregion
}