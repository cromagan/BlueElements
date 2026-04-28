// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BlueBasics.ClassesStatic;

namespace BlueBasics.Classes;

public class AssemblyAwareCache<T> : IEnumerable<T> {

    #region Fields

    private int _assemblyCount;
    private List<T>? _items;
    private bool _loading;

    #endregion

    #region Constructors

    public AssemblyAwareCache() {
        Generic.LoadAllAssemblies();
    }

    #endregion

    #region Properties

    private List<T> Items {
        get {
            if (_loading) { return []; }

            var currentCount = AppDomain.CurrentDomain.GetAssemblies().Length;
            if (_items != null && _assemblyCount == currentCount) { return _items; }

            _loading = true;
            _items = BuildCache();
            _assemblyCount = AppDomain.CurrentDomain.GetAssemblies().Length;
            _loading = false;
            return _items;
        }
    }

    #endregion

    #region Methods

    public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private List<T> BuildCache() {
        var result = new List<T>();
        var targetType = typeof(T);

        foreach (var thist in AssemblyAwareCache.AllTypes) {
            if (!targetType.IsAssignableFrom(thist)) { continue; }
            try {
                if (thist.GetConstructor(Type.EmptyTypes) == null) { continue; }
                var instance = Activator.CreateInstance(thist);
                if (instance is T t) {
                    result.Add(t);
                }
            } catch {
                Develop.AbortAppIfStackOverflow();
            }
        }

        if (typeof(IComparable).IsAssignableFrom(typeof(T))) {
            result.Sort(Comparer<T>.Default);
        }

        return result;
    }

    #endregion
}