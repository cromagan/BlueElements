// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueBasics.Interfaces;

/// <summary>
/// Zentrale Verwaltung der FragmentChanged-Bubbling-Verdrahtung zwischen einem
/// Container (parent) und seinen Fragment-Kindern (child). Jede Kind-Änderung wird
/// dem Parent als FragmentChanged-Ereignis gereicht — mit eigenem Crumb (Navigations-Typ
/// und je Ereignis frisch gelesenem Key des Kindes) oder ohne zusätzlichen Crumb
/// (Durchleitung, wenn das Kind seinen Pfad bereits komplett mitliefert).
/// Ist das Parent IDisposableExtended, werden seine Kinder beim Dispose automatisch
/// abgemeldet; bereits verworfene Parents werden gar nicht erst verdrahtet.
/// </summary>
public static class FragmentEventExtension {

    #region Fields

    private static readonly object Locker = new();

    private static readonly Dictionary<IFragmentStoreable, ParentRegistration> Registered = [];

    #endregion

    #region Methods

    /// <summary>
    /// Verdrahtet ein Kind mit Crumb: Jede Kind-Änderung reicht der Container mit
    /// dem eigenen Pfad-Segment (Navigations-Typ, Key des Kindes) eine Ebene höher.
    /// </summary>
    public static void RegisterEvents(this IFragmentStoreable parent, Enum crumbType, IFragmentStoreable? child) {
        if (parent is null || child is null) { return; }
        if (parent is IDisposableExtended { IsDisposed: true }) { return; }
        if (child is IDisposableExtended { IsDisposed: true }) { return; }

        lock (Locker) {
            var entry = Entry(parent);

            if (entry.Has(child, crumbType)) { return; }

            var registration = new FragmentRegistration(parent, crumbType, child);
            registration.Attach();
            entry.Items.Add(registration);
        }
    }

    /// <summary>
    /// Verdrahtet ein Kind ohne eigenen Crumb: Das Ereignis des Kindes wird
    /// unverändert durchgereicht (der Pfad des Kindes ist bereits komplett).
    /// </summary>
    public static void RegisterEvents(this IFragmentStoreable parent, IFragmentStoreable? child) {
        if (parent is null || child is null) { return; }
        if (parent is IDisposableExtended { IsDisposed: true }) { return; }
        if (child is IDisposableExtended { IsDisposed: true }) { return; }

        lock (Locker) {
            var entry = Entry(parent);

            if (entry.Has(child, null)) { return; }

            var registration = new FragmentRegistration(parent, null, child);
            registration.Attach();
            entry.Items.Add(registration);
        }
    }

    /// <summary>
    /// Verdrahtet alle Kinder der Liste mit demselben Crumb-Typ.
    /// </summary>
    public static void RegisterEvents(this IFragmentStoreable parent, Enum crumbType, IReadOnlyList<IFragmentStoreable>? children) {
        if (children is null) { return; }

        for (var i = 0; i < children.Count; i++) {
            RegisterEvents(parent, crumbType, children[i]);
        }
    }

    /// <summary>
    /// Löst die Verdrahtung eines Kindes; bei null werden alle Registrierungen
    /// des Crumb-Typs gelöst (z. B. vor dem Neuaufbau eines Einzel-Kindes).
    /// </summary>
    public static void UnregisterEvents(this IFragmentStoreable parent, Enum crumbType, IFragmentStoreable? child) {
        if (parent is null) { return; }

        lock (Locker) {
            if (!Registered.TryGetValue(parent, out var entry)) { return; }

            if (child is null) {
                entry.RemoveAll(crumbType);
            } else {
                entry.Remove(child, crumbType);
            }

            CheckEmpty(parent, entry);
        }
    }

    /// <summary>
    /// Löst die Verdrahtung eines Kindes ohne Crumb.
    /// </summary>
    public static void UnregisterEvents(this IFragmentStoreable parent, IFragmentStoreable? child) {
        if (parent is null || child is null) { return; }

        lock (Locker) {
            if (!Registered.TryGetValue(parent, out var entry)) { return; }

            entry.Remove(child, null);
            CheckEmpty(parent, entry);
        }
    }

    /// <summary>
    /// Löst die Verdrahtung aller Kinder der Liste mit demselben Crumb-Typ.
    /// </summary>
    public static void UnregisterEvents(this IFragmentStoreable parent, Enum crumbType, IReadOnlyList<IFragmentStoreable>? children) {
        if (children is null) { return; }

        for (var i = 0; i < children.Count; i++) {
            UnregisterEvents(parent, crumbType, children[i]);
        }
    }

    /// <summary>
    /// Löst die Verdrahtung aller Kinder des Crumb-Typs und verdrahtet die Liste neu —
    /// entfernte Kinder fallen damit automatisch heraus, neue werden ergänzt.
    /// </summary>
    public static void ReregisterEvents(this IFragmentStoreable parent, Enum crumbType, IReadOnlyList<IFragmentStoreable>? children) {
        if (parent is null) { return; }

        UnregisterEvents(parent, crumbType, (IFragmentStoreable?)null);
        RegisterEvents(parent, crumbType, children);
    }

    /// <summary>
    /// Löst die Verdrahtung eines Kindes und verdrahtet es neu (Idempotenz-Sicherung).
    /// </summary>
    public static void ReregisterEvents(this IFragmentStoreable parent, Enum crumbType, IFragmentStoreable? child) {
        UnregisterEvents(parent, crumbType, child);
        RegisterEvents(parent, crumbType, child);
    }

    /// <summary>
    /// Liefert den Registrierungs-Eintrag des Parents und hängt bei
    /// IDisposableExtended-Parents einmalig den Dispose-Automatismus an.
    /// Muss im Lock aufgerufen werden.
    /// </summary>
    private static ParentRegistration Entry(IFragmentStoreable parent) {
        if (!Registered.TryGetValue(parent, out var entry)) {
            entry = new ParentRegistration();
            Registered.Add(parent, entry);
        }

        if (!entry.DisposeHooked && parent is IDisposableExtended disposable) {
            entry.DisposeHooked = true;
            disposable.Disposed += Parent_Disposed;
        }

        return entry;
    }

    /// <summary>
    /// Entfernt den Parent-Eintrag, sobald keine Registrierung mehr besteht,
    /// damit das statische Register den Container nicht am Einsammeln hindert.
    /// Muss im Lock aufgerufen werden.
    /// </summary>
    private static void CheckEmpty(IFragmentStoreable parent, ParentRegistration entry) {
        if (entry.Items.Count != 0) { return; }

        _ = Registered.Remove(parent);
    }

    /// <summary>
    /// Meldet beim Dispose des Parents alle Kinder automatisch ab und räumt das Register auf.
    /// </summary>
    private static void Parent_Disposed(object? sender, System.EventArgs e) {
        if (sender is not IFragmentStoreable parent) { return; }

        lock (Locker) {
            if (!Registered.Remove(parent, out var entry)) { return; }

            entry.RemoveAll();
        }
    }

    #endregion

    #region Nested Types

    /// <summary>
    /// Alle Registrierungen eines Containers plus Kennzeichen für den Dispose-Automatismus.
    /// </summary>
    private sealed class ParentRegistration {

        #region Fields

        public bool DisposeHooked;
        public readonly List<FragmentRegistration> Items = [];

        #endregion

        #region Methods

        /// <summary>
        /// True, wenn das Kind bereits mit diesem Crumb verdrahtet ist.
        /// </summary>
        public bool Has(IFragmentStoreable child, Enum? crumbType) {
            foreach (var registration in Items) {
                if (registration.IsFor(child, crumbType)) { return true; }
            }

            return false;
        }

        /// <summary>
        /// Löst alle Registrierungen des Crumb-Typs (null = Durchleitungen).
        /// </summary>
        public void RemoveAll(Enum? crumbType) {
            for (var i = Items.Count - 1; i >= 0; i--) {
                if (!Items[i].IsForCrumb(crumbType)) { continue; }

                Items[i].Detach();
                Items.RemoveAt(i);
            }
        }

        /// <summary>
        /// Löst alle Registrierungen.
        /// </summary>
        public void RemoveAll() {
            foreach (var registration in Items) {
                registration.Detach();
            }

            Items.Clear();
        }

        /// <summary>
        /// Löst die Registrierung des Kindes mit dem Crumb-Typ.
        /// </summary>
        public void Remove(IFragmentStoreable child, Enum? crumbType) {
            for (var i = Items.Count - 1; i >= 0; i--) {
                if (!Items[i].IsFor(child, crumbType)) { continue; }

                Items[i].Detach();
                Items.RemoveAt(i);
                return;
            }
        }

        #endregion
    }

    /// <summary>
    /// Eine einzelne Verdrahtung: hält Container, Crumb-Typ und Kind und reicht
    /// Kind-Änderungen als Methode (kein Lambda) an den Container weiter.
    /// </summary>
    private sealed class FragmentRegistration(IFragmentStoreable parent, Enum? crumbType, IFragmentStoreable child) {

        #region Properties

        private IFragmentStoreable Child { get; } = child;

        private Enum? CrumbType { get; } = crumbType;

        #endregion

        #region Methods

        /// <summary>
        /// True, wenn die Registrierung zu Kind und Crumb-Typ passt.
        /// </summary>
        public bool IsFor(IFragmentStoreable child, Enum? crumbType) =>
            ReferenceEquals(Child, child) && IsForCrumb(crumbType);

        /// <summary>
        /// True, wenn der Crumb-Typ passt (null = Durchleitung).
        /// </summary>
        public bool IsForCrumb(Enum? crumbType) => CrumbType?.Equals(crumbType) ?? crumbType is null;

        /// <summary>
        /// Verdrahtet das Kind.
        /// </summary>
        public void Attach() => Child.FragmentChanged += Child_FragmentChanged;

        /// <summary>
        /// Löst die Verdrahtung des Kindes.
        /// </summary>
        public void Detach() => Child.FragmentChanged -= Child_FragmentChanged;

        /// <summary>
        /// Reicht die Kind-Änderung dem Container weiter: mit frisch gelesenem
        /// Crumb-Key (umbenennungssicher) oder unverändert bei Durchleitung.
        /// </summary>
        private void Child_FragmentChanged(object? sender, FragmentChangedEventArgs e) {
            if (parent is IDisposableExtended { IsDisposed: true }) { return; }

            if (CrumbType is null) {
                // Durchleitung: Der Pfad des Kindes ist bereits komplett.
                parent.OnFragmentChanged(e);
                return;
            }

            var crumbKey = sender is IHasKeyName named ? named.KeyName : string.Empty;
            parent.OnFragmentChanged(new FragmentChangedEventArgs(parent, FragmentChangedEventArgs.Crumbs(CrumbType, crumbKey, e.RelativePath), e.TypeSelector, e.ContentType, e.Content) { ListContent = e.ListContent });
        }

        #endregion
    }

    #endregion
}
