# Zrzuty ekranu

Ten katalog jest pusty celowo.

Gra używa Avalonii, czyli renderuje przez GPU. Środowisko, w którym
przygotowywano to repozytorium, nie ma karty graficznej ani zainstalowanego
.NET SDK (`builds.dotnet.microsoft.com` jest niedostępne przez proxy), więc
aplikacji nie dało się w nim ani zbudować, ani uruchomić.

Wstawienie tu wizualizacji zamiast prawdziwego zrzutu byłoby gorsze niż brak
obrazka: czytelnik nie miałby jak odróżnić jednego od drugiego.

## Jak je zrobić

Na dowolnym komputerze z pulpitem i .NET SDK 8:

```bash
dotnet run
```

Następnie zwykłym narzędziem systemowym (Win+Shift+S, `gnome-screenshot`,
Cmd+Shift+4) zapisz tutaj:

| Plik | Widok |
|---|---|
| `menu.png` | ekran startowy z wyborem poziomu |
| `gameplay-normal.png` | rozgrywka na poziomie normalnym |
| `gameplay-hard.png` | poziom trudny — widoczne przeszkody |
| `game-over.png` | ekran końca gry z wynikiem |
| `scoreboard.png` | tabela najlepszych wyników |

Potem podlinkuj je w README.
