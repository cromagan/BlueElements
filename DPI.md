# DPI-Skalierung komplett deaktiviert

## Warum es nun klappt

Windows skaliert Fenster und Controls automatisch, wenn:
1. Die App sich als "DPI-aware" deklariert (`dpiAware=true` im Manifest oder `HighDpiMode.DpiUnaware` fehlt)
2. Das `AutoScaleMode` der Form nicht auf `None` steht
3. `Skin.Scale` einen Wert != 1.0 zurückgibt und dadurch Schriftgrößen künstlich vergrößert/verkleinert werden

Alle drei Probleme wurden behoben. Die App verhält sich jetzt auf jedem Bildschirm identisch – gleiche Pixel, gleiche Schriftgrößen, gleiche Abstände.

## Was geändert wurde

### 1. app.manifest (`BlueControls/app.manifest`)

Vorher:
```xml
<dpiAware xmlns="http://schemas.microsoft.com/SMI/2005/WindowsSettings">true</dpiAware>
<dpiAwareness xmlns="http://schemas.microsoft.com/SMI/2016/WindowsSettings">PerMonitorV2</dpiAwareness>
```

Nachher:
```xml
<dpiAware xmlns="http://schemas.microsoft.com/SMI/2005/WindowsSettings">false</dpiAware>
<dpiAwareness xmlns="http://schemas.microsoft.com/SMI/2016/WindowsSettings">DpiUnaware</dpiAwareness>
```

**Wichtig:** `dpiAware=true` und `PerMonitorV2` überschreiben `Application.SetHighDpiMode(DpiUnaware)` im Code. Das Manifest gewinnt. Deshalb muss das Manifest zwingend auf `false`/`DpiUnaware` stehen, sonst bringt der Code-Aufruf nichts.

### 2. Program.cs – SetHighDpiMode

- **BeCreative/Program.cs:** Hatte bereits `DpiUnaware` – jetzt wirksam, da Manifest passt.
- **BluePaint/Program.cs:** `Application.SetHighDpiMode(HighDpiMode.DpiUnaware)` hinzugefügt.

Der Aufruf muss als allererstes vor allen anderen `Application.*`-Aufrufen stehen.

### 3. Skin.Scale

`Skin.Scale` war dynamisch und berechnete den DPI-Faktor des Hauptmonitors. Das wurde in `BlueFont` verwendet, um Schriftgrößen zu korrigieren (z.B. `Size / Skin.Scale`). Das ist jetzt konstant `1.0f` – keine DPI-Korrektur mehr.

### 4. Form.cs – AutoScaleMode.None

Im Konstruktor wird jetzt `AutoScaleMode = AutoScaleMode.None` gesetzt. Das verhindert, dass Windows bei Font-Änderungen oder DPI-Wechsel Controls automatisch skaliert.

### 5. Entfernte Totcode-Methoden

`PerformAutoScale()` und `Scale()` (ohne Parameter) wurden aus `GenericControl.cs` und `Form.cs` entfernt:
- `PerformAutoScale()` war `new` (kein Override) – wird durch `AutoScaleMode.None` ohnehin No-Op.
- `Scale()` hatte die falsche Signatur und wurde vom Framework nie aufgerufen.

## Was weiterhin besteht (unverändert)

Diese Mechanismen waren schon korrekt implementiert und bleiben bestehen:

| Mechanismus | Ort | Wirkung |
|---|---|---|
| `ScaleChildren => false` | GenericControl, Form | Kind-Controls werden nicht skaliert |
| `GetScaledBounds` → identity | GenericControl, Form | Bounds werden nicht verändert |
| `ScaleControl` → Faktor (1,1) | GenericControl, Form | Skalierungsfaktor immer 1 |
| `AutoSize => false` | GenericControl, Form | Kein AutoSize |

## Für neue Einstiegspunkte

Jede neue `Program.cs` muss enthalten:
```csharp
Application.SetHighDpiMode(HighDpiMode.DpiUnaware);
```

Jedes neue Projekt-Manifest muss enthalten:
```xml
<windowsSettings>
  <dpiAware xmlns="http://schemas.microsoft.com/SMI/2005/WindowsSettings">false</dpiAware>
  <dpiAwareness xmlns="http://schemas.microsoft.com/SMI/2016/WindowsSettings">DpiUnaware</dpiAwareness>
</windowsSettings>
```

Jede neue Form muss von `BlueControls.Forms.Form` erben (nicht von `System.Windows.Forms.Form`), damit `AutoScaleMode.None` automatisch gesetzt wird.
