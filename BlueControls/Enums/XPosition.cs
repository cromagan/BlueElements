// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System;

namespace BlueControls.Enums;

[Flags]
public enum XPosition {
    frei = 0,
    Ganze_Breite = 101,
    Linke_Hälfte = 201,
    Rechte_Hälfte = 202,
    Spalten_3_links = 301,
    Spalten_3_Mitte = 302,
    Spalten_3_rechts = 303,
    Spalten_4_links = 401,
    Spalten_4_Position_2 = 402,
    Spalten_4_Position_3 = 403,
    Spalten_4_Rechts = 404,
    Spalten_5_links = 501,
    Spalten_5_Position_2 = 502,
    Spalten_5_Position_3 = 503,
    Spalten_5_Position_4 = 504,
    Spalten_5_Rechts = 505,
    Spalten_6_links = 601,
    Spalten_6_Position_2 = 602,
    Spalten_6_Position_3 = 603,
    Spalten_6_Position_4 = 604,
    Spalten_6_Position_5 = 605,
    Spalten_6_Rechts = 606,
}