namespace BlueControls.Enums
{
    public enum enMarkState
    {
        None = 0,

        /// <summary>
        /// Bei Rechtschreibfehlern
        /// </summary>
        Ringelchen = 1,
        /// <summary>
        /// Felder im Creativepad
        /// </summary>
        Field= 2,
        /// <summary>
        /// Verknüpfungen, der eigene Name
        /// </summary>
        MyOwn = 4,
        /// <summary>
        /// Verknüpfungen, ein erkannter Link
        /// </summary>
        Other = 8
    }
}