// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System;

namespace BlueBasics.Interfaces;

public interface IDisposableExtendedWithEvent : IDisposableExtended {

    #region Events

    /// <summary>
    /// Wird ausgelöst, wenn Dispose aufgerufenn wird - IsDisposed ist nmoch false!
    /// </summary>
    event EventHandler? DisposingEvent;

    #endregion
}