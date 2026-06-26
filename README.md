# Elegant Snake

Klasyczny Snake w C# / Windows Forms z dopracowaną, nowoczesną oprawą graficzną.
Kod został podzielony na wiele plików według odpowiedzialności, co ułatwia
rozwijanie i utrzymanie projektu.

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
| `Program.cs` | punkt wejścia aplikacji |
| `SnakeForm.cs` | okno gry: czasomierze, sterowanie, stany ekranów |
| `SnakeGame.cs` | czysta logika gry (wąż, jedzenie, kolizje, wynik) |
| `GameRenderer.cs` | całe rysowanie (tło, HUD, plansza, wąż, nakładki) |
| `BoardGeometry.cs` | przeliczenia siatka ↔ piksele |
| `GraphicsUtils.cs` | pomocnicze funkcje rysujące (zaokrąglone rogi itp.) |
| `HighScoreStore.cs` | zapis i odczyt najlepszego wyniku |
| `GameConfig.cs` | centralne ustawienia (rozmiary, prędkość, punkty) |
| `Direction.cs` | enum kierunku ruchu |
| `GamePhase.cs` | enum stanu gry (Ready / Playing / Paused / GameOver) |
| `Snake.csproj` | plik projektu .NET |

## Co nowego

- **Ekran startowy** — gra czeka na start zamiast ruszać od razu.
- **Pauza** (`P` / `Esc`) z czytelną nakładką.
- **Trwały najlepszy wynik** — rekord jest zapisywany i przeżywa zamknięcie gry.
- **Podział na moduły** — logika gry jest oddzielona od rysowania i UI.

## Budowanie i uruchamianie

Wymagany [.NET SDK 8.0](https://dotnet.microsoft.com/download) na systemie Windows
(projekt korzysta z Windows Forms).

```bash
dotnet run
```

lub zbuduj plik wykonywalny:

```bash
dotnet build -c Release
```
