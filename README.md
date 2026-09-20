# Elegant Snake

[![CI](https://github.com/Pawel123j/SNAKE/actions/workflows/ci.yml/badge.svg)](https://github.com/Pawel123j/SNAKE/actions/workflows/ci.yml)

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
dotnet run                                                   # uruchom grę
dotnet test tests/SnakeGame.Tests/SnakeGame.Tests.csproj     # 31 testów logiki
dotnet build Snake.csproj -c Release                         # wersja wydania
```

> Repozytorium nie ma pliku rozwiązania, a projekt testowy leży w podkatalogu,
> więc polecenia wskazują projekty wprost. `dotnet restore` w korzeniu
> obsłużyłby tylko samą aplikację.

### Gotowy plik do pobrania

Każde wydanie zawiera samodzielne binarki dla Windowsa (x64) i Linuksa (x64) —
`self-contained`, więc **nie trzeba mieć zainstalowanego .NET**. Do wydania
dołączony jest `SHA256SUMS.txt`.

Wydanie powstaje z tagu:

```bash
git tag v1.0.0 && git push origin v1.0.0
```

## Testy i CI

31 testów czystej logiki, bez uruchamiania interfejsu:

| Plik | Co sprawdza |
|---|---|
| `SnakeGameTests` (9) | ruch, zakaz zawrócenia, wzrost, punktacja zwykła i bonusowa, ściana, owijanie, obecność przeszkód |
| `SnakeGameCollisionTests` (14) | kolizja z własnym ciałem, ściany w pionie, owijanie w obu osiach, niezmienniki przeszkód, punktacja spowalniacza i sumowanie wyniku, reset, obsługa kierunku |
| `ScoreboardTests` (8) | sortowanie, limit pięciu wpisów, odrzucanie wyników ≤ 0, zapis i odczyt, odporność na uszkodzony plik |

CI buduje i uruchamia testy na **Ubuntu i Windowsie**, z `fail-fast: false` —
w projekcie przeniesionym z WinForms na Avalonię właśnie po to, żeby działał
wieloplatformowo, najważniejszą informacją z przebiegu jest to, czy problem
dotyczy obu systemów, czy jednego.

### Dźwięk działa tylko na Windowsie

To jest znane ograniczenie, nie błąd. `SoundManager` generuje krótkie tony
w pamięci i odtwarza je przez `System.Media.SoundPlayer`, który istnieje
wyłącznie na Windowsie. Na Linuksie i macOS dźwięki są **po cichu pomijane** —
gra działa normalnie, tylko bez efektów.

Pełne, wieloplatformowe audio wymagałoby dodatkowej biblioteki (NAudio,
OpenAL albo Avalonia z natywnym backendem). Nie zostało dodane świadomie:
byłaby to nowa zależność w projekcie, który poza tym nie ma żadnej poza
Avalonią, dla funkcji ozdobnej.

## Zrzuty ekranu

Katalog `docs/screenshots/` jest pusty — patrz
[docs/screenshots/README.md](docs/screenshots/README.md).

## Licencja

MIT — patrz [LICENSE](LICENSE).
