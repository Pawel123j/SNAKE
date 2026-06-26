# Elegant Snake

Klasyczny Snake w C# na **[Avalonia UI](https://avaloniaui.net/)** — wieloplatformowym
frameworku .NET. Gra działa na **Windows, Linux i macOS**, ma dopracowaną oprawę
graficzną (gradienty, zaokrąglenia, poświaty), poziomy trudności, przeszkody,
różne rodzaje jedzenia, dźwięki i tabelę najlepszych wyników. Kod jest podzielony
na wiele plików według odpowiedzialności.

## Sterowanie

| Klawisz | Akcja |
|---|---|
| Strzałki / `WASD` | ruch węża |
| `1` / `2` / `3` | wybór poziomu na ekranie startowym (Łatwy / Normalny / Trudny) |
| `P` lub `Esc` | pauza / wznowienie |
| `R` | restart |
| `Enter` / `Spacja` | start gry oraz nowa gra po przegranej |

## Funkcje

- 🎬 **Ekran startowy** z wyborem poziomu trudności i tabelą wyników.
- 🎚️ **Poziomy trudności:**
  - *Łatwy* — wolniej, **owijanie ścian** (wyjście jedną stroną = wejście drugą),
  - *Normalny* — klasyczne zasady,
  - *Trudny* — szybciej i z **przeszkodami** na planszy.
- 🍎 **Rodzaje jedzenia:**
  - zwykłe (+10, lekkie przyspieszenie),
  - 🟡 złoty bonus (+30),
  - 🔵 spowalniacz (+5 i zwolnienie tempa).
- ⏸️ **Pauza** (`P` / `Esc`) z czytelną nakładką.
- 🔊 **Dźwięki** (zjedzenie, bonus, koniec gry) — na Windows; na innych systemach po cichu pomijane.
- 🏆 **Tabela top 5** wyników, zapisywana między uruchomieniami.
- 🧪 **Testy jednostkowe** logiki gry (xUnit).
- 🖥️ **Wieloplatformowość** — ten sam kod działa na Windows, Linux i macOS.

## Struktura projektu

| Plik | Odpowiedzialność |
|---|---|
| `Program.cs` | punkt wejścia, konfiguracja Avalonii |
| `App.axaml` / `App.axaml.cs` | aplikacja Avalonia, motyw, okno główne |
| `MainWindow.axaml` / `.axaml.cs` | okno hostujące planszę gry |
| `GameControl.cs` | kontrolka gry: czasomierze, sterowanie, dźwięki, stany ekranów |
| `SnakeGame.cs` | czysta logika gry (wąż, jedzenie, przeszkody, kolizje, wynik) |
| `GameRenderer.cs` | całe rysowanie przez `DrawingContext` Avalonii |
| `BoardGeometry.cs` | przeliczenia siatka ↔ piksele |
| `Difficulty.cs` | poziomy trudności i ich parametry |
| `FoodType.cs` | rodzaje jedzenia |
| `Scoreboard.cs` | tabela najlepszych wyników (zapis/odczyt) |
| `SoundManager.cs` | generowane efekty dźwiękowe |
| `GameConfig.cs` | wymiary planszy |
| `Direction.cs` / `GamePhase.cs` | enumy kierunku i stanu gry |
| `Snake.csproj` | plik projektu .NET (pakiety Avalonia) |
| `tests/SnakeGame.Tests/` | testy jednostkowe logiki gry (xUnit) |

Logika gry jest oddzielona od warstwy rysowania i interfejsu — łatwo ją testować
i rozwijać niezależnie.

## Budowanie i uruchamianie

Wymagany [.NET SDK 8.0](https://dotnet.microsoft.com/download) (dowolny system).

```bash
dotnet run            # uruchom grę
dotnet test tests/SnakeGame.Tests   # uruchom testy logiki
```

Budowa wersji wydania:

```bash
dotnet build -c Release
```
