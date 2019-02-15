using System;

namespace BlueBasics.Interfaces
{
    public interface IChangedFeedback
    {

        /// <summary>
        /// Wird ausgelöst, wenn sich ein Objet verändert.
        /// Wird nicht bei der Neuerstellung des Objektes oder beim Parsen ausgelöst.
        /// </summary>
        event EventHandler Changed;

        void OnChanged();


    }
}