// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System;

namespace BlueBasics.Interfaces;

//In.NET gibt es zwei Arten von Ressourcen: verwaltete und nicht verwaltete.Hier sind einige Beispiele für beide:

//Verwaltete Ressourcen: Verwaltete Ressourcen sind solche, die vom .NET Garbage Collector (GC) verwaltet und automatisch freigegeben werden. Sie müssen nicht manuell bereinigt werden, aber es kann sinnvoll sein, sie explizit freizugeben, wenn sie nicht mehr benötigt werden, um den Speicher früher freizugeben.

//Beispiele für verwaltete Ressourcen:
//Instanzen von Klassen
//Strings
//Arrays
//Delegates
//Objekte, die von System.Collections.Generic wie List<T>, Dictionary<TKey, TValue>, etc.
//LINQ-to-Objects-Abfragen
//Tasks und andere asynchrone Primitiven
//Nicht verwaltete Ressourcen: Nicht verwaltete Ressourcen sind solche, die außerhalb der.NET-Umgebung existieren und nicht vom Garbage Collector verwaltet werden.Sie müssen explizit freigegeben werden, um Speicherlecks und andere Probleme zu vermeiden.

//Beispiele für nicht verwaltete Ressourcen:
//Dateihandles (z.B.geöffnete Dateien)
//Netzwerkverbindungen (z.B.Sockets)
//Datenbankverbindungen
//Handles zu Betriebssystemressourcen (z.B.Fensterhandles in einer Windows-Anwendung)
//Direkter Speicherzugriff(z.B.über Marshal.AllocHGlobal oder Marshal.AllocCoTaskMem)
//COM-Objekte
//Grafikressourcen(z.B.GDI+ Objekte, DirectX Ressourcen)
//Unmanaged Code, der über P/Invoke oder COM Interop aufgerufen wird
//
//Um nicht verwaltete Ressourcen in .NET zu verwalten, implementieren Klassen oft das IDisposable-Interface und verwenden das Dispose-Muster, um sicherzustellen, dass diese Ressourcen korrekt freigegeben werden, wenn das Objekt nicht mehr benötigt wird.

public interface IDisposableExtended : IDisposable {

    #region Properties

    //public bool Disposing { get; }
    bool IsDisposed { get; }

    #endregion
}