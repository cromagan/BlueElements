// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System;

namespace BlueTable.Enums;

[Flags]
public enum FilterType {

    // Filterarten allgemein -------------------------------------------------------
    Istgleich = 1 << 0,

    //  ZUR INFO:  Ungleich = 1 << 1
    Instr = 1 << 2,

    AlwaysFalse = 1 << 3,
    Between = 1 << 4,
    BeginntMit = 1 << 5,

    RowKey = 1 << 6,

    // Filter - Bits -----------------------------------------------------------------
    GroßKleinEgal = 1 << 7,

    ODER = 1 << 8,
    UND = 1 << 9,
    MultiRowIgnorieren = 1 << 10,

    // FormatStandardisieren = 2048
    // Std-Filter - Kombinationen ----------------------------------------------------
    Istgleich_GroßKleinEgal = Istgleich | GroßKleinEgal,

    IstGleich_ODER = Istgleich | ODER,
    IstGleich_UND = Istgleich | UND,
    Istgleich_ODER_GroßKleinEgal = Istgleich_GroßKleinEgal | ODER,
    Istgleich_UND_GroßKleinEgal = Istgleich_GroßKleinEgal | UND,
    Istgleich_GroßKleinEgal_MultiRowIgnorieren = Istgleich | GroßKleinEgal | MultiRowIgnorieren,
    Istgleich_MultiRowIgnorieren = Istgleich | MultiRowIgnorieren,
    Ungleich_MultiRowIgnorieren = (1 << 1) | MultiRowIgnorieren,
    Ungleich_MultiRowIgnorieren_UND_GroßKleinEgal = Ungleich_MultiRowIgnorieren | UND | GroßKleinEgal,
    Ungleich_MultiRowIgnorieren_GroßKleinEgal = Ungleich_MultiRowIgnorieren | GroßKleinEgal,
    Instr_GroßKleinEgal = Instr | GroßKleinEgal,
    BeginntMit_GroßKleinEgal = BeginntMit | GroßKleinEgal,

    //Instr_ODER_GroßKleinEgal = Instr | GroßKleinEgal | ODER,
    Instr_UND_GroßKleinEgal = Instr | GroßKleinEgal | UND
}