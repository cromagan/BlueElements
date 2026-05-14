# ListBox Performance Investigation

## Status: GELÖST - Phase 4 implementiert

## Symptom
ListBox mit 20 Items braucht ~125-135ms pro Mausbewegung (DoMouseMovement -> Refresh -> DrawControl).
TableView macht mehr Zeichenoperationen und ist schneller.

## Bisherige Erkenntnisse

### Was bereits gefixt wurde (Phase 1)
1. **TrimByWidth nutzte `Font.MeasureString` (kein Cache)** -> Jeder Aufruf erstellte `Graphics.FromHwnd(IntPtr.Zero)`
   - Fix: `BlueFont.TrimByWidth` Instanzmethode nutzt jetzt `MeasureString` mit `_stringSizeCache`
2. **Items cachten keinen getrimmten Text** -> Bei jedem Draw wurde TrimByWidth+Translate aufgerufen
   - Fix: `TextListItem` cached jetzt übersetzten+getrimmten Text pro (Text, Width)
3. **Timing-Bug** -> `back`-Werte waren negativ wegen falscher Formel

### Phase 2 Fixes
1. **TextListItem - Direct Draw Path** statt Draw_FormatedText durch 3 Overloads
2. **Draw_Back: FillRectangle statt FillPath für Rectangle**
3. **Draw_Border: DrawRectangle statt DrawPath für Solid1Px Rectangle**
4. **Stopwatch-Overhead entfernt**

## Phase 4: Individuelle Drawing-Routinen + erweitertes Caching (aktuell)

### Architektur-Änderung

Jeder `BackgroundStyle` und jeder `BorderStyle` hat jetzt eine **eigene Routine**.
Die Dispatcher-Methoden `Draw_Back` und `Draw_Border` berechnen nur noch das Rechteck und delegieren per Switch an die jeweilige Routine.

**Jede Routine entscheidet selbst:**
- `Contour.Rectangle` → direktes `FillRectangle`/`DrawRectangle` (kein Path, kein Overhead)
- Anderer Contour → Path aus `SkinCache.GetContour()` + `FillPath`/`DrawPath`

### Neue SkinCache-Caches

#### 4. Dotted-Pen-Cache
- **Key:** `int colorArgb`
- **Value:** `Pen` mit `DashStyle.Dot`
- Wird von `DrawBorder_Solid1PxFocusDot` und `DrawBorder_FocusDot` genutzt
- **Vorher:** Bei jedem Aufruf `new Pen(color) { DashStyle = DashStyle.Dot }` mit `using` — erstellt und disposed pro Draw

#### 5. Border-Gradient-Cache
- **Key:** `BorderGradientKey` (readonly record struct: C1, C2, H)
- **Value:** `LinearGradientBrush` mit `GammaCorrection = true`
- Wird von `DrawBorder_Solid1PxDualColor` genutzt
- **Vorher:** Bei jedem Aufruf `new LinearGradientBrush(...)` mit `using` — erstellt und disposed pro Draw

### Individuelle Background-Routinen (9 Stück)

| Routine | Parameter |
|---------|-----------|
| `DrawBack_Solid` | gr, contour, lr, backColor1 |
| `DrawBack_GradientVertical` | gr, contour, lr, backColor1, backColor2 |
| `DrawBack_GradientVertical3` | gr, contour, lr, backColor1, backColor2, backColor3, gradientMidpoint |
| `DrawBack_GradientHorizontal` | gr, contour, lr, backColor1, backColor2 |
| `DrawBack_GradientHorizontal3` | gr, contour, lr, backColor1, backColor2, backColor3, gradientMidpoint |
| `DrawBack_GradientDiagonal` | gr, contour, lr, backColor1, backColor2, backColor3, gradientMidpoint |
| `DrawBack_Glossy` | gr, contour, lr, backColor1, backColor2 |
| `DrawBack_GlossyPressed` | gr, contour, lr, backColor1, backColor2 |
| `DrawBack_GradientVerticalHighlight` | gr, contour, lr, backColor1, backColor2, gradientMidpoint |

### Individuelle Border-Routinen (6 Stück)

| Routine | Parameter |
|---------|-----------|
| `DrawBorder_Solid1Px` | gr, contour, lr, borderColor1 |
| `DrawBorder_Solid1PxDualColor` | gr, contour, lr, borderColor1, borderColor2 |
| `DrawBorder_Solid1PxFocusDot` | gr, contour, lr, borderColor1, borderColor2 |
| `DrawBorder_FocusDot` | gr, contour, lr, borderColor2 |
| `DrawBorder_Solid3Px` | gr, contour, lr, borderColor1 |
| `DrawBorder_Solid21Px` | gr, contour, lr, borderColor1 |

### Performance-Verbesserungen gegenüber Phase 3

1. **Kein Path für Rectangle-Contour** — `Solid1PxDualColor`, `Solid21Px` und der `default`-Case holten vorher immer einen Path (auch bei Rectangle). Jetzt wird direkt `DrawRectangle`/`FillRectangle` verwendet
2. **Dotted Pens gecacht** — `Solid1PxFocusDot` und `FocusDot` erstellen keine neuen Pens mehr pro Aufruf
3. **Border-Gradient-Brush gecacht** — `Solid1PxDualColor` erstellt keinen neuen `LinearGradientBrush` mehr pro Aufruf
4. **Kein unnötiger Path-Lookup** — Routinen die keinen Path brauchen (z.B. `FocusDot` mit Rectangle) fragen keinen Cache an
5. **Innere Pfade bei Rectangle** — `FocusDot` und `Solid1PxFocusDot` mit Rectangle-Contour verwenden `DrawRectangle` statt TranslateTransform + Path
6. **Entfernt:** `FillShape`/`DrawShape` Helper-Methoden — Logik ist direkt in den Routinen inline (weniger Methodenaufrufe, JIT kann besser optimieren)

### Cleanup-Architektur (aktualisiert)

| Cache | Ort | Trigger |
|-------|-----|---------|
| Kontur-Pfade | `SkinCache._contourCache` | `SkinCache.TrimCaches()` / `ClearAll()` |
| Gradient-Brushes | `SkinCache._gradientCache` | `SkinCache.TrimCaches()` / `ClearAll()` |
| Dotted-Pens | `SkinCache._dottedPenCache` | `SkinCache.TrimCaches()` / `ClearAll()` |
| Border-Gradient-Brushes | `SkinCache._borderGradientCache` | `SkinCache.TrimCaches()` / `ClearAll()` |
| Solid-Brushes | `BlueFont._brushCache` | `BlueFont.TrimAllCaches()` |
| Pens | `BlueFont._penCache` | `BlueFont.TrimAllCaches()` |
| Font-Instanzen | `BlueFont._blueFontCache` | `BlueFont.TrimAllCaches()` |
| String-Messungen | `BlueFont._stringSizeCache` | `BlueFont.TrimAllCaches()` |
| Übersetzungen | `LanguageTool._translationCache` | `LanguageTool.ClearTranslationCache()` |
| Zeilen-Pens | `Skin.PenLinieDünn/Kräftig/Dick` | `Skin.LoadSkin()` |

### Geändnte Dateien

### Phase 4
- `BlueControls/Classes/SkinCache.cs` — Dotted-Pen-Cache + Border-Gradient-Cache + TrimCaches erweitert
- `BlueControls/Classes/Skin.cs` — 15 individuelle Drawing-Routinen, Dispatcher vereinfacht, FillShape/DrawShape entfernt

### Neue Klasse: `SkinCache`
Zentraler Cache für alle Skin-Ressourcen. Pattern orientiert sich an `BlueFont` (ConcurrentDictionary + GetOrAdd).

#### 1. Kontur-Cache
- **Key:** `(Contour type, int Width, int Height)`
- **Value:** `GraphicsPath` gebaut bei (0, 0, W, H)
- Zeichnet mit `Graphics.TranslateTransform(x, y)` — Kontur bleibt immer bei (0,0)
- `FillShape` / `DrawShape` Helper wählen automatisch FillRectangle/DrawRectangle vs FillPath/DrawPath (in Phase 4 durch individuelle Routinen ersetzt)

#### 2. Gradient-Brush-Cache
- **Key:** `GradientKey` (readonly record struct: Style, C1, C2, C3, W, H, Midpoint)
- **Value:** `LinearGradientBrush` gebaut bei (0, 0, W, H)
- Alle Gradient-Typen werden gecacht: Vertical, Vertical3, Horizontal, Horizontal3, Diagonal, Glossy, GlossyPressed, GradientVerticalHighlight
- Abgeleitete Farben (z.B. `Color.FromArgb(180, BackColor2)` bei Glossy) werden vor dem Cache-Lookup berechnet

#### 3. DeleteBackBrush
- Statischer Singleton statt `new SolidBrush(Color.FromArgb(220, 255, 255, 255))` bei jedem Draw_FormatedText-Aufruf
- Nutzt `BlueFont.GetBrush()` (welches selbst cacht)

### Skin.cs Änderungen

#### Draw_Back
- **Vorher:** Bei jedem Aufruf `new SolidBrush()` / `new LinearGradientBrush()` + `Contour()` (erstellt `new GraphicsPath()`) — GDI+ Leak
- **Jetzt:** `BlueFont.GetBrush()` / `SkinCache.GetGradient()` + `SkinCache.GetContour()` — alles gecacht
- `TranslateTransform` verschiebt den Zeichenbereich, sodass Konturen/Brushes bei (0,0) gebaut werden
- `FillShape()` Helper reduziert Code-Duplizierung

#### Draw_Border
- **Vorher:** `new Pen()` bei jedem Aufruf — nicht disposed
- **Jetzt:** `BlueFont.GetPen()` (gecacht) für alle Standard-Border-Styles
- Nur noch `Solid1PxFocusDot` und `FocusDot` erstellen lokale Pens (wegen `DashStyle.Dot`) — **in Phase 4 durch SkinCache.GetDottedPen() gelöst**
- `Solid1PxDualColor` GradientBrush wird mit `using` korrekt disposed — **in Phase 4 durch SkinCache.GetBorderGradientBrush() gelöst**
- Innere Konturen (FocusDot) werden ebenfalls über `SkinCache.GetContour()` gecacht

#### Draw_FormatedText
- Überladungen von 4 auf 4 beibehalten (API-kompatibel), aber vereinfacht:
  - Overload 1 (Design+States) inlined jetzt DesignOf+QuickImage.Get statt Umweg über Overload 3
  - Overload 3 bleibt für bestehende Aufrufer
  - Nutzt `SkinCache.DeleteBackBrush` statt `new SolidBrush`
  - Alignment-Berechnung vereinfacht (`totalW` statt doppelter Berechnung)
  - Early return wenn `txt` und `qi` beide null/leer
- Entfernt: XML-Doc-Kommentare mit redundanten Parametern, alte auskommentierte `Draw_Border_DuoColor`-Methode, alte `Contour()`-Methode

#### Draw_Back_Transparent
- **Vorher:** 4× `new SolidBrush(control.Parent.BackColor)` — nicht disposed
- **Jetzt:** `BlueFont.GetBrush(control.Parent.BackColor)` — gecacht

#### LoadSkin
- Ruft `SkinCache.ClearAll()` auf, um alte Ressourcen freizugeben
- PenLinie*-Pens nutzen jetzt `BlueFont.GetPen()` statt `new Pen()`

### LanguageTool.cs Änderungen

- **Vorher:** Single-Entry-Cache (`_german`/`_english`) — nur bei 2x gleichem String hintereinander effektiv
- **Jetzt:** `ConcurrentDictionary<string, string>` mit vollem Dictionary-Cache
- `DoTranslateCore()` extrahiert — reine Übersetzungslogik ohne Caching
- Cache wird vor dem Table-Lookup geprüft, `string.Format` wird danach angewendet
- `ClearTranslationCache()` für manuelle Invalidierung (z.B. wenn Übersetzungstabelle sich ändert)
- Entfernt: `_german`/`_english` Single-Entry-Felder

### TextListItem.cs Änderungen

- Stopwatch + Debug.WriteLine entfernt (~20-50µs Overhead pro Item)
- Cache-Logik beibehalten (Translate+Trim pro Instanz)

### Allgemein.cs Änderungen

- `SkinCache.TrimCaches()` wird bei Speicherdruck zusammen mit `BlueFont.TrimAllCaches()` aufgerufen

### Cleanup-Architektur

| Cache | Ort | Trigger |
|-------|-----|---------|
| Kontur-Pfade | `SkinCache._contourCache` | `SkinCache.TrimCaches()` / `ClearAll()` |
| Gradient-Brushes | `SkinCache._gradientCache` | `SkinCache.TrimCaches()` / `ClearAll()` |
| Solid-Brushes | `BlueFont._brushCache` | `BlueFont.TrimAllCaches()` |
| Pens | `BlueFont._penCache` | `BlueFont.TrimAllCaches()` |
| Font-Instanzen | `BlueFont._blueFontCache` | `BlueFont.TrimAllCaches()` |
| String-Messungen | `BlueFont._stringSizeCache` | `BlueFont.TrimAllCaches()` |
| Übersetzungen | `LanguageTool._translationCache` | `LanguageTool.ClearTranslationCache()` |
| Zeilen-Pens | `Skin.PenLinieDünn/Kräftig/Dick` | `Skin.LoadSkin()` |

Speicherdruck-Handler (`Allgemein.CheckMemory`) ruft alle auf.

### Ressourcen-Dispose

| Phase 2 (vorher) | Phase 3 (jetzt) |
|---|---|
| `new SolidBrush()` × 5/Aufruf — **niemals disposed** | `BlueFont.GetBrush()` — gecacht, disposed bei Trim |
| `new Pen()` × 12/Aufruf — **niemals disposed** | `BlueFont.GetPen()` — gecacht, disposed bei Trim |
| `new LinearGradientBrush()` × 10/Aufruf — **niemals disposed** | `SkinCache.GetGradient()` — gecacht, disposed bei Trim/Clear |
| `new GraphicsPath()` × mehrere/Aufruf — **niemals disposed** | `SkinCache.GetContour()` — gecacht, disposed bei Trim/Clear |

## Geändnete Dateien

### Phase 1
- `BlueControls/Classes/ItemCollectionList/TextListItem.cs`
- `BlueControls/Classes/Skin.cs`

### Phase 2
- `BlueControls/Classes/ItemCollectionList/TextListItem.cs`
- `BlueControls/Classes/Skin.cs`

### Phase 3
- `BlueControls/Classes/SkinCache.cs` — **Neu**: Zentraler Skin-Ressourcen-Cache
- `BlueControls/Classes/Skin.cs` — Draw_Back/Draw_Border/Draw_FormatedText mit Cache + TranslateTransform
- `BlueTable/Classes/LanguageTool.cs` — Dictionary-basierter Übersetzungs-Cache
- `BlueControls/Classes/ItemCollectionList/TextListItem.cs` — Stopwatch entfernt
- `BlueControls/Classes/Allgemein.cs` — SkinCache.TrimCaches() bei Speicherdruck

## Nächste Schritte (falls später noch nötig)
1. **Bitmap-Cache für gerenderten Text** - DrawImage statt DrawString für statischen Text
2. **Nur geänderte Items neu zeichnen** (ListBox-Paint-Logik)
