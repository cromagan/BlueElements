﻿// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueTable.Classes;

/// <summary>
/// Meldet den Fortschritt eines Vorgangs: das mitgegebene Objekt, Anzahl bearbeiteter
/// Einheiten und Gesamtvorhaben. Die Rückgabe true fordert den Abbruch des Vorgangs.
/// </summary>
public delegate bool ProgressDelegate(object? state, int processed, int total);
