// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Classes;

namespace BlueControls.Interfaces;

/// <summary>
/// Vertrag für Objekte (Forms wie <see cref="Forms.FloatingForm" /> bzw. künftig
/// auch Controls), die von der <see cref="Animator" />-Engine animiert werden.
/// Der Animations-Thread ruft pro Frame <see cref="Animate" /> auf und wendet das
/// gelieferte <see cref="AnimationFrame" /> via Win32 an. Sobald
/// <see cref="AnimationFrame.Finished" /> true ist, wird
/// <see cref="OnAnimationFinished" /> aufgerufen.
/// Alle Member werden aus dem Animations-Thread aufgerufen und müssen thread-safe
/// sein — insbesondere dürfen keine WinForms-Properties anderer Controls gelesen
/// werden.
/// </summary>
public interface IAnimatable : IDisposableExtended {

    #region Properties

    bool Disposing { get; }

    /// <summary>
    /// Win32-Window-Handle des zu animierenden Objekts. Muss gültig sein,
    /// bevor die Animation gestartet wird.
    /// </summary>
    IntPtr Handle { get; }

    bool IsHandleCreated { get; }

    #endregion

    #region Methods

    /// <summary>
    /// Berechnet das Frame der Animation aus der seit Start der Animation
    /// verstrichenen Zeit. Wird auf dem Animations-Thread aufgerufen und muss
    /// thread-safe sein — keine WinForms-Properties anderer Controls lesen,
    /// stattdessen die Helper-Methoden von <see cref="Animator" /> nutzen
    /// (z.B. <see cref="Animator.GetWindowY" />,
    /// <see cref="Animator.IsHwndVisible" />). Wenn
    /// <see cref="AnimationFrame.Finished" /> true ist, beendet die Engine die
    /// Animation und ruft <see cref="OnAnimationFinished" /> auf.
    /// </summary>
    AnimationFrame Animate(TimeSpan elapsed);

    void Close();

    /// <summary>
    /// Wird aus dem Animations-Thread aufgerufen, sobald die Animation beendet
    /// ist (<see cref="AnimationFrame.Finished" /> war true). UI-Aufrufe darin
    /// müssen selbst via BeginInvoke gemarshalled werden. Typische
    /// Implementierung schließt bzw. versteckt das Objekt.
    /// </summary>
    void OnAnimationFinished() {
        if (!IsHandleCreated || Disposing || IsDisposed) { return; }
        try {
            ((ISynchronizeInvoke)this).BeginInvoke(new Action(Close), null);
        } catch (Exception ex) {
            Develop.DebugPrint("OnFinished konnte nicht gemarshalled werden", ex);
        }
    }

    public void StartAnimation() {
        if (!IsHandleCreated) {
            Develop.DebugPrint("StartAnimation vor CreateHandle aufgerufen");
            return;
        }
        Animator.Start(this);
    }

    public void StopAnimation() {
        if (IsHandleCreated) {
            Animator.Stop(Handle);
        }
    }

    #endregion
}