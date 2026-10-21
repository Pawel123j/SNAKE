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

## Weryfikacja środowiska — 2026-10-17

`dotnet test tests/SnakeGame.Tests/SnakeGame.Tests.csproj --configuration Release` zalicza 31 testów logiki; build Release działa. Testy obejmują reguły gry, ale nie potwierdzają sterowania, renderowania ani dźwięku w działającym oknie.

W środowisku bez wyświetlacza/Xvfb nie wykonano GUI ani walidacji paczki na Windows/macOS. Repozytorium ma CI i workflow wydania; nie uruchomiono publikacji. Przed dystrybucją sprawdź menu, pauzę, restart, kolizje i zapis wyników na docelowym systemie. Wyników użytkownika nie należy usuwać przy aktualizacji; rollback polega na przywróceniu poprzedniej binarki z zachowaniem danych.

## Walidacja etapu 2 — 2026-10-18

Renderer używa teraz jawnie dołączonego fontu Inter. Na badanym Linuksie wymuszenie Segoe UI, a także domyślny fallback, dawało działającą planszę z niewidocznym tekstem.

Po `dotnet build -c Release` uruchom w izolowanym ekranie X11: `xvfb-run -a python scripts/smoke_gui.py --output gui-artifacts` (Python + Pillow, Xvfb, xdotool). Test uruchamia własny proces, wysyła start/pauzę/wznowienie/restart, zapisuje cztery zrzuty i sprawdza wyrenderowany tekst. Nie steruje już otwartym oknem. CI ma ten krok na Linuksie. Lokalnie: 31 testów logiki i ten test GUI PASS. Nie potwierdzono Windows, dźwięku ani pełnego przebiegu punktacji w wydanej paczce.

## Etap 3 — 2026-10-21

33 testy PASS. Rekordy są zapisywane przez plik tymczasowy, flush i atomową
zamianę. Błąd zapisu pozostawia poprzedni kompletny plik. Naprawiono też wybór
katalogu danych: nieistniejący XDG_CONFIG_HOME nie powoduje już zapisu względem
katalogu uruchomienia gry.

Audyt wykazał GHSA-xrw6-gwf8-vvr9 (high) w Tmds.DBus.Protocol 0.20.0.
Przypięto zgodną gałąź poprawki 0.21.3; audyt NuGet aplikacji i testów na
2026-10-21 nie wykazuje znanych podatności. Restore blokuje high/critical.

Linux X11: menu 1/2/3, start, pauza, wznowienie i restart PASS. Pełny test
`scripts/scored_gui.py` steruje rzeczywistym oknem na podstawie pikseli planszy,
zbiera jedzenie, kończy grę, sprawdza zapis i uruchamia nowy proces. OCR nagłówka
potwierdził odczyt rekordu (ostatni przebieg: 5 punktów). Nie zmienia stanu gry
ani nie podstawia pliku z rekordem. Uruchom po buildzie na izolowanym DISPLAY:
`python scripts/scored_gui.py --output NOWY_KATALOG` (Pillow, xdotool, tesseract).

Windows x64 self-contained/single-file został zbudowany po poprawkach.
**Nie uruchomiono go na Windows; dźwięk Windows pozostaje niezweryfikowany.**
Dlatego projekt nadal PARTIAL. Linux celowo nie odtwarza audio.
