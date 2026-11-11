# Zrzuty ekranu

Obrazy powstały z uruchomionej aplikacji na Linuksie pod X11/Xvfb. `small-window.png`
jest zrzutem tego samego okna po zmianie rozmiaru na 400×450; pozostałe pokazują
rzeczywistą rozgrywkę, zakończenie gry i wynik odczytany po ponownym uruchomieniu.

Wygeneruj ponownie test GUI po Release buildzie:

```bash
xvfb-run -a python scripts/smoke_gui.py --output /tmp/snake-gui-smoke
```

Skrypt naciska klawisze w oknie gry i sprawdza widoczność tekstu oraz mały widok.
Pełny przebieg zdobywania punktów i ponownego odczytu rekordu uruchamia
`scripts/scored_gui.py`; wymaga Pillow, `xdotool`, Tesseract i X11.

Zrzuty nie potwierdzają działania dźwięku ani wersji Windows/macOS.
