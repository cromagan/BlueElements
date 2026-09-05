// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Interfaces;

/// <summary>
/// Vertrag für Objekte (Forms wie FloatingForm bzw. künftig
/// auch Controls), die von der Animator-Engine animiert werden.
/// Der Animations-Thread ruft pro Frame Animate auf und wendet das
/// gelieferte AnimationFrame via Win32 an. Sobald
/// AnimationFrame.Finished true ist, wird
/// OnAnimationFinished aufgerufen.
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
    /// stattdessen die Helper-Methoden von Animator nutzen
    /// (z.B. Animator.GetWindowY,
    /// Animator.IsHwndVisible). Wenn
    /// AnimationFrame.Finished true ist, beendet die Engine die
    /// Animation und ruft OnAnimationFinished auf.
    /// </summary>
    AnimationFrame Animate(TimeSpan elapsed);

    void Close();

    /// <summary>
    /// Wird aus dem Animations-Thread aufgerufen, sobald die Animation beendet
    /// ist (AnimationFrame.Finished war true). UI-Aufrufe darin
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