# Elegant Snake

Klasyczny Snake w C# na **[Avalonia UI](https://avaloniaui.net/)** — wieloplatformowym
frameworku .NET. Gra działa na **Windows, Linux i macOS**, ma dopracowaną oprawę
graficzną (gradienty, zaokrąglenia, poświaty), a kod jest podzielony na wiele plików
według odpowiedzialności.

## Sterowanie

| Klawisz | Akcja |
|---|---|
| Strzałki / `WASD` | ruch węża |
| `P` lub `Esc` | pauza / wznowienie |
| `R` | restart |
| `Enter` / `Spacja` | start gry oraz nowa gra po przegranej |

## Struktura projektu

| Plik | Odpowiedzialność |
|---|---|
| `Program.cs` | punkt wejścia, konfiguracja Avalonii |
| `App.axaml` / `App.axaml.cs` | aplikacja Avalonia, motyw, okno główne |
| `MainWindow.axaml` / `.axaml.cs` | okno hostujące planszę gry |
| `GameControl.cs` | kontrolka gry: czasomierze, sterowanie, stany ekranów |
| `SnakeGame.cs` | czysta logika gry (wąż, jedzenie, kolizje, wynik) |
| `GameRenderer.cs` | całe rysowanie przez `DrawingContext` Avalonii |
| `BoardGeometry.cs` | przeliczenia siatka ↔ piksele |
| `HighScoreStore.cs` | zapis i odczyt najlepszego wyniku |
| `GameConfig.cs` | centralne ustawienia (rozmiary, prędkość, punkty) |
| `Direction.cs` | enum kierunku ruchu |
| `GamePhase.cs` | enum stanu gry (Ready / Playing / Paused / GameOver) |
| `Snake.csproj` | plik projektu .NET (pakiety Avalonia) |

Logika gry (`SnakeGame`, `HighScoreStore`, `GameConfig`…) jest oddzielona od
warstwy rysowania i interfejsu — łatwo ją testować i rozwijać niezależnie.

## Funkcje

- 🎬 **Ekran startowy** — gra czeka na start zamiast ruszać od razu.
- ⏸️ **Pauza** (`P` / `Esc`) z czytelną nakładką.
- 🏆 **Trwały najlepszy wynik** — rekord zapisuje się i przeżywa zamknięcie gry.
- 🖥️ **Wieloplatformowość** — ten sam kod działa na Windows, Linux i macOS.

## Budowanie i uruchamianie

Wymagany [.NET SDK 8.0](https://dotnet.microsoft.com/download) (dowolny system).

```bash
dotnet restore
dotnet run
```

Budowa wersji wydania:

```bash
dotnet build -c Release
```
