using System;

namespace BlueDatabase.Enums {
    [Flags]
    public enum enFilterType {

        // Filterarten allgemein -------------------------------------------------------
        KeinFilter = 0,


        Istgleich = 1,
        //  ZUR INFO:  Ungleich = 2 
        Instr = 4,

        //  Tendiert_Zu = 8

        //  EigenschaftTendenz = 8,


        Berechne = 16,
        BeginntMit = 32,


        // Filter - Bits -----------------------------------------------------------------
        GroßKleinEgal = 128,
        ODER = 256,
        UND = 512,
        MultiRowIgnorieren = 1024,
        // FormatStandardisieren = 2048


        // Std-Filter - Kombinationen ----------------------------------------------------
        Istgleich_GroßKleinEgal = Istgleich | GroßKleinEgal,
        IstGleich_ODER = Istgleich | ODER,
        IstGleich_UND = Istgleich | UND,
        Istgleich_ODER_GroßKleinEgal = Istgleich_GroßKleinEgal | ODER,
        Istgleich_UND_GroßKleinEgal = Istgleich_GroßKleinEgal | UND,


        Istgleich_GroßKleinEgal_MultiRowIgnorieren = Istgleich | GroßKleinEgal | MultiRowIgnorieren,
        Istgleich_MultiRowIgnorieren = Istgleich | MultiRowIgnorieren,

        Ungleich_MultiRowIgnorieren = 2 | MultiRowIgnorieren,
        Ungleich_MultiRowIgnorieren_UND_GroßKleinEgal = Ungleich_MultiRowIgnorieren | UND | GroßKleinEgal,
        Ungleich_MultiRowIgnorieren_GroßKleinEgal = Ungleich_MultiRowIgnorieren | GroßKleinEgal,


        Instr_GroßKleinEgal = Instr | GroßKleinEgal,
        Instr_ODER_GroßKleinEgal = Instr | GroßKleinEgal | ODER,
        Instr_UND_GroßKleinEgal = Instr | GroßKleinEgal | UND
    }
}