using System;
using System.IO;
using System.Text;

namespace ElegantSnake
{
    /// <summary>
    /// Proste efekty dźwiękowe — krótkie tony generowane w locie.
    /// Odtwarzane na Windows przez <c>System.Media.SoundPlayer</c>; na innych
    /// systemach po cichu pomijane. Pełne, wieloplatformowe audio wymagałoby
    /// dodatkowej biblioteki (np. NAudio albo OpenAL).
    /// </summary>
    public sealed class SoundManager
    {
        public void Eat() => Beep(880, 70);
        public void Bonus() => Beep(1245, 110);
        public void Slow() => Beep(420, 120);
        public void GameOver() => Beep(196, 260);

        private static void Beep(double frequency, int durationMs)
        {
            if (!OperatingSystem.IsWindows())
                return;

            try
            {
                using var stream = CreateTone(frequency, durationMs);
                var player = new System.Media.SoundPlayer(stream);
                player.Load();   // kopiuje dane synchronicznie...
                player.Play();   // ...a odtwarza asynchronicznie
            }
            catch
            {
                // Dźwięk to tylko dodatek — nie może wywrócić gry.
            }
        }

        /// <summary>Tworzy w pamięci krótki sygnał WAV (PCM 16-bit, mono).</summary>
        private static MemoryStream CreateTone(double frequency, int durationMs)
        {
            const int sampleRate = 44100;
            const short amplitude = 9000;
            int samples = sampleRate * durationMs / 1000;
            int dataSize = samples * 2;

            var stream = new MemoryStream(44 + dataSize);
            var w = new BinaryWriter(stream);

            w.Write(Encoding.ASCII.GetBytes("RIFF"));
            w.Write(36 + dataSize);
            w.Write(Encoding.ASCII.GetBytes("WAVE"));
            w.Write(Encoding.ASCII.GetBytes("fmt "));
            w.Write(16);                 // rozmiar bloku fmt
            w.Write((short)1);           // format PCM
            w.Write((short)1);           // liczba kanałów (mono)
            w.Write(sampleRate);
            w.Write(sampleRate * 2);     // bajty na sekundę
            w.Write((short)2);           // wyrównanie bloku
            w.Write((short)16);          // bity na próbkę
            w.Write(Encoding.ASCII.GetBytes("data"));
            w.Write(dataSize);

            for (int i = 0; i < samples; i++)
            {
                double fade = 1.0 - (double)i / samples;   // wyciszenie, by uniknąć trzasków
                double value = Math.Sin(2 * Math.PI * frequency * i / sampleRate) * amplitude * fade;
                w.Write((short)value);
            }

            w.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
