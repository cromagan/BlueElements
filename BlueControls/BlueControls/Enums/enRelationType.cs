namespace BlueBasics.Enums
{
    public enum enRelationType
    {
        None = 0,

        // Reihenfolge wichtig für Berechnung.
        // Zuerst die mächtigsten, die die am einfachsten erfüllt werden können.


        PositionZueinander = 10,
        YPositionZueinander = 11,
        WaagerechtSenkrecht = 20,

        AbstandZueinander = 30,




        Dummy = 1000
    }
}