// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading;

namespace BlueBasics.Classes;
// https://stackoverflow.com/questions/32901771/multiple-enum-descriptions
// https://stackoverflow.com/questions/1402803/passing-properties-by-reference-in-c-sharp

public class Accessor<T> : IDisposableExtended, IHasQuickInfo {

    #region Fields

    private readonly AccessorMemberEntry<T> _entry;
    private readonly object? _target;
    private volatile int _isDisposedFlag;

    #endregion

    #region Constructors

    /// <summary>
    /// Erzeugt einen Accessor aus einem Lambda-Ausdruck, der auf eine
    /// Eigenschaft oder ein Feld zeigt. Aktuell werden nur einfache Ausdrücke
    /// der Form <c>() => Property</c> unterstützt (Ziel = this).
    /// </summary>
    public Accessor(Expression<Func<T>>? expr) {
        var memberExpression = (MemberExpression?)expr?.Body;

        // Zielobjekt aus dem Instance-Ausdruck lösen. Bei () => this.Property
        // ist die Instance eine ConstantExpression, die das Objekt direkt enthält.
        _target = ResolveTarget(memberExpression?.Expression);

        var member = memberExpression?.Member;
        _entry = member is null ? AccessorMemberEntry<T>.Unknown : AccessorMemberEntry<T>.Create(member);

        // Auf Änderungen am Zielobjekt lauschen.
        if (_target is INotifyPropertyChanged inpc) {
            inpc.PropertyChanged += OnTargetPropertyChanged;
        }
    }

    #endregion

    #region Events

    public event EventHandler? Disposed;

    public event EventHandler? ValueChanged;

    #endregion

    #region Properties

    public bool CanRead => _entry.CanRead;
    public bool CanWrite => _entry.CanWrite;
    public bool IsDisposed => _isDisposedFlag == 1;
    public string Name => _entry.Name;
    public string QuickInfo => _entry.QuickInfo;

    #endregion

    #region Methods

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public T? Get() {
        if (_entry.TryGet(_target, out var value)) { return value; }
        Develop.DebugPrint("Getter ist null!");
        return default;
    }

    public void Set(T value) {
        if (!_entry.TrySet(_target, value)) {
            Develop.DebugPrint("Setter ist null!");
        }
    }

    protected virtual void Dispose(bool disposing) {
        if (Interlocked.CompareExchange(ref _isDisposedFlag, 1, 0) != 0) { return; }

        if (disposing) {
            OnDisposed();

            if (_target is INotifyPropertyChanged inpc) {
                inpc.PropertyChanged -= OnTargetPropertyChanged;
            }

            ValueChanged = null;
            Disposed = null;
        }
    }

    /// <summary>
    /// Löst das Zielobjekt aus dem Instance-Ausdruck auf. Unterstützt werden
    /// nur einfache Property-Ausdrücke (Konstante als Instance, also this).
    /// </summary>
    private static object? ResolveTarget(Expression? instanceExpression) {
        if (instanceExpression is ConstantExpression ce) { return ce.Value; }
        if (instanceExpression is null) { return null; }
        Develop.DebugPrint("Nur einfache Property-Ausdrücke werden unterstützt.");
        return null;
    }

    private void OnDisposed() => Disposed?.Invoke(this, System.EventArgs.Empty);

    private void OnTargetPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == Name || string.IsNullOrEmpty(e.PropertyName)) {
            OnValueChanged();
        }
    }

    private void OnValueChanged() => ValueChanged?.Invoke(this, System.EventArgs.Empty);

    #endregion
}