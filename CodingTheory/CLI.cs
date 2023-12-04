using CodingTheory.BitStream;
using System.Globalization;

namespace CodingTheory;

public sealed class Cli
{
    private Channel _channel = null!;
    private Encoder _encoder = null!;
    private Decoder _decoder = null!;

    private int _m;
    private double _errorProbability;

    // Paleidžiamas ciklas, kuris vykdomas tol, kol vartotojas neišjungs programos.
    public Task StartAsync(CancellationToken token = default)
    {
        // Paprašoma parametrų m ir klaidos tikimybės.
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        (_m, _errorProbability) = GetParameters();

        _channel = new Channel();
        _encoder = new Encoder(_m);
        _decoder = new Decoder(_m);

        while (!token.IsCancellationRequested)
        {
            Console.Clear();
            WriteHeader();
            WriteScenarios();

            while (true)
            {
                var key = Console.ReadKey(intercept: true);

                // Tikrinama, kurį scenarijų vartotojas pasirinko.

                if (key.KeyChar is '1')
                {
                    RunFirstScenario();
                    break;
                }
                else if (key.KeyChar is '2')
                {
                    RunSecondScenario();
                    break;
                }
                else if (key.KeyChar is '3')
                {
                    RunThirdScenario();
                    token.ThrowIfCancellationRequested();
                    break;
                }
                else if (key.KeyChar is '4')
                {
                    (_m, _errorProbability) = GetParameters();
                    _encoder = new Encoder(_m);
                    _decoder = new Decoder(_m);
                    break;
                }
                else
                {
                    WriteLine("Pasirinktas neegzistuojantis scenarijus.", ConsoleColor.Red);
                }
            }
        }

        return Task.CompletedTask;
    }

    // Antro scenarijaus vykdymas.
    private void RunSecondScenario()
    {
        var repeatScenario = true;
        while (repeatScenario)
        {
            Console.Clear();
            WriteHeader();

            WriteLine("Įveskite tekstinio failo pavadinimą, kuriame yra jūsų tekstas:");
            var textFilePath = Console.ReadLine();

            while (!File.Exists(textFilePath))
            {
                WriteLine($"Failas nerastas.\nVeskite failo pavadinimą iš naujo:", ConsoleColor.Red);
                textFilePath = Console.ReadLine();
            }

            const string fileNameWithCoding = "su-kodavimu.txt";
            const string fileNameWithoutCoding = "be-kodavimo.txt";

            // Būtent čia yra pagrindinė logika. Žr. į funkcijos implemtenaciją ir komentarus joje.
            SendDataThroughChannel(textFilePath, fileNameWithCoding, fileNameWithoutCoding, isBmp: false);

            WriteLine($"Failas siųstas kanalu (naudojant kodavimą) išsaugotas į '{fileNameWithCoding}'.", ConsoleColor.Green);
            WriteLine($"Failas siųstas kanalu (nenaudojant kodavimo) išsaugotas į '{fileNameWithoutCoding}'.", ConsoleColor.Green);

            // Kartojamas scenarijus, jei vartotojas to nori.
            repeatScenario = false;
            RunYesNoPrompt("Ar kartoti scenarijų?", () =>
            {
                repeatScenario = true;
            });
        }
    }

    // Trečio scenarijaus vykdymas.
    private void RunThirdScenario()
    {
        var repeatScenario = true;
        while (repeatScenario)
        {
            Console.Clear();
            WriteHeader();

            WriteLine("Įveskite paveiksliuko failo pavadinimą:");
            var imageFilePath = Console.ReadLine();

            while (!File.Exists(imageFilePath))
            {
                WriteLine($"Failas nerastas.\nVeskite failo pavadinimą iš naujo:", ConsoleColor.Red);
                imageFilePath = Console.ReadLine();
            }

            const string fileNameWithCoding = "su-kodavimu.bmp";
            const string fileNameWithoutCoding = "be-kodavimo.bmp";

            // Būtent čia yra pagrindinė logika. Žr. į funkcijos implemtenaciją ir komentarus joje.
            SendDataThroughChannel(imageFilePath, fileNameWithCoding, fileNameWithoutCoding, isBmp: true);

            WriteLine($"Failas siųstas kanalu (naudojant kodavimą) išsaugotas į '{fileNameWithCoding}'.", ConsoleColor.Green);
            WriteLine($"Failas siųstas kanalu (nenaudojant kodavimo) išsaugotas į '{fileNameWithoutCoding}'.", ConsoleColor.Green);

            // Kartojamas scenarijus, jei vartotojas to nori.
            repeatScenario = false;
            RunYesNoPrompt("Ar kartoti scenarijų?", () =>
            {
                repeatScenario = true;
            });
        }
    }

    // Funkcija priima failo pavadinimą (tiksliau file path), naujų failų pavadinimus (failų be kodavimo taikymo ir su), bei ar perduodama siunčiama bmp nuotrauka.
    // Toliau vykdomas perdavimas kanalu.
    private void SendDataThroughChannel(string filePath, string fileNameWithCoding, string fileNameWithoutCoding, bool isBmp)
    {
        using var fileReader = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var binaryReader = new BinaryReader(fileReader);
        using var bitReader = new BitReader(fileReader);

        using var fileWriter = new FileStream(fileNameWithCoding, FileMode.Create, FileAccess.Write);
        using var bitWriter = new BitWriter(fileWriter);

        using var fileWriterWithoutCoding = new FileStream(fileNameWithoutCoding, FileMode.Create, FileAccess.Write);
        using var bitWriterWithoutCoding = new BitWriter(fileWriterWithoutCoding);

        // BMP antrašę laikome tarnybine informacija, jos nekraipome.
        // Tarnybinė informacija - tai visa informacija iki pirmo pikselio pozicijos pradžios.
        // Ją iškart perrašome tiek į failą, kuriam netaikysim kodavimo, tiek į tą, kuriam taikysim.
        if (isBmp)
        {
            var pixelOffset = Utils.GetPixelOffset(binaryReader);

            var headerBytes = new byte[pixelOffset];
            fileReader.ReadExactly(headerBytes);
            fileWriter.Write(headerBytes);
            fileWriterWithoutCoding.Write(headerBytes);
        }

        var word = new byte[_m + 1];
        int readBitsCount;

        // Skaitome _m + 1 ilgio žodžius iki kol nuskaitome mažiau nei _m + 1 ilgio bitų.
        while ((readBitsCount = bitReader.ReadAtLeast(word, word.Length, throwOnEndOfStream: false)) >= word.Length)
        {
            // Užkoduojam, siunčiam kanalu, gautą žodį iš kanalo dekoduojam, ir išsaugom į failą.
            var encodedWord = _encoder.Encode(word);
            var receivedWord = _channel.Transmit(encodedWord, _errorProbability, out var _);
            var decodedWord = _decoder.Decode(receivedWord);
            bitWriter.Write(decodedWord);

            // Žodžio nekoduojam, o siunčiam į kanalą. Iš kanalo gautą žodį išsaugom į kitą failą.
            var receivedWordWithoutCodingApplied = _channel.Transmit(word, _errorProbability, out var _);
            bitWriterWithoutCoding.Write(receivedWordWithoutCodingApplied);
        }

        // Susitvarkome su paskutniu žodžiu, kuris gali būt neužpildytas.
        if (readBitsCount > 0)
        {
            for (var i = readBitsCount; i < word.Length; ++i)
                word[i] = 0;

            // Užkoduojam, siunčiam kanalu, gautą žodį iš kanalo dekoduojam, ir išsaugom į failą.
            var encodedWord = _encoder.Encode(word);
            var receivedWord = _channel.Transmit(encodedWord, _errorProbability, out var _);

            // Paimam tik 'readBitsCount' bitų, nes būtent toks originalus buvo žodžio ilgis, tiesiog reikėjo jį užpildyti kažkokiomis reikšmėmis prieš tai.
            var decodedWord = _decoder.Decode(receivedWord)[..readBitsCount];
            bitWriter.Write(decodedWord);

            // Žodžio nekoduojam, o siunčiam į kanalą. Iš kanalo gautą žodį išsaugom į kitą failą
            // Paimam tik 'readBitsCount' bitų, nes būtent toks originalus buvo žodžio ilgis, tiesiog reikėjo jį užpildyti kažkokiomis reikšmėmis prieš tai.
            var receivedWordWithoutCodingApplied = _channel.Transmit(word, _errorProbability, out var _)[..readBitsCount];
            bitWriterWithoutCoding.Write(receivedWordWithoutCodingApplied);
        }
    }

    // Vykdomas 1 scenarijus.
    private void RunFirstScenario()
    {
        var repeatScenario = true;
        while (repeatScenario)
        {
            Console.Clear();
            WriteHeader();
            WriteLine("Pasirinktas 1 scenarijus.", ConsoleColor.Blue);

            var word = GetInputVector(length: _m + 1);

            var encodedWord = _encoder.Encode(word);
            WriteLine("Užkoduotas vektorius:");
            WriteLine(encodedWord);

            var receivedWord = _channel.Transmit(encodedWord, _errorProbability, out var errorPositions);
            WriteReceivedWord(receivedWord, errorPositions);
            RunYesNoPrompt("Ar norite pakeisti šį vektorių?", () =>
            {
                var newWord = GetInputVector(length: receivedWord.Length);
                Array.Copy(newWord, receivedWord, receivedWord.Length);
            });

            var decodedWord = _decoder.Decode(receivedWord);
            WriteLine("Dekoduotas vektorius:");
            WriteLine(decodedWord);
            WriteLine("");

            repeatScenario = false;
            RunYesNoPrompt("Ar kartoti scenarijų?", () =>
            {
                repeatScenario = true;
            });
        }
    }

    // Funkcija skirta grąžiau parodyti iš kanalo gautą žodį su klaidomis.
    private static void WriteReceivedWord(byte[] receivedWord, int[] errorPositions)
    {
        WriteLine("Gautas vektorius iš kanalo:");

        for (var i = 0; i < receivedWord.Length; i += 1)
        {
            if (errorPositions.Contains(i))
                Write(receivedWord[i], ConsoleColor.Red);
            else
                Write(receivedWord[i]);
        }
        WriteLine("");

        WriteLine($"Klaidų skaičius: {errorPositions.Length}. Klaidų pozicijos (pradendant 0-tąja):");
        WriteLine(errorPositions);
    }

    // Gaunamas įvesties vektorius iš konsolės. Prašoma vartotojo įvesti tol, kol bus vektorius korektiškas.
    private static byte[] GetInputVector(int length)
    {
        Console.WriteLine($"Įveskite {length} ilgio vektorių:");

        var vectorAsString = Console.ReadLine();

        while (
            vectorAsString is null ||
            vectorAsString.Length != length ||
            vectorAsString.Any(letter => letter is not '0' && letter is not '1'))
        {
            WriteLine($"Vektorius turi būti ilgio {length} ir sudarytas iš simbolių {{0, 1}}.", ConsoleColor.Red);
            vectorAsString = Console.ReadLine();
        }

        var vector = new byte[length];
        for (var i = 0; i < vector.Length; i++)
            vector[i] = (byte)(vectorAsString[i] - '0');

        return vector;
    }

    // Gaunami korektiški m ir klaidos tikimybė iš varotojo.
    private static (int m, double errorProbability) GetParameters()
    {
        Console.Clear();

        WriteLine("Įveskite parametrą 'm':");
        var mAsString = Console.ReadLine();

        int m;
        while (!int.TryParse(mAsString, CultureInfo.InvariantCulture, out m) || m <= 0)
        {
            WriteLine("Įvestas parametras 'm' turi būti teigiamas natūralus skaičius.\nVeskite iš naujo:", ConsoleColor.Red);
            mAsString = Console.ReadLine();
        }

        WriteLine("Įveskite kanalo klaidos tikimybę 'p':");
        var errorProbabilityAsString = Console.ReadLine()?.Replace(',', '.');

        double errorProbability;
        while (!double.TryParse(errorProbabilityAsString, CultureInfo.InvariantCulture, out errorProbability) ||
                errorProbability < 0 || errorProbability > 1)
        {
            WriteLine("Įvestas parametras 'p' turi būti realusis skaičius intervale [0; 1].\nVeskite iš naujo:", ConsoleColor.Red);
            errorProbabilityAsString = Console.ReadLine()?.Replace(',', '.');
        }

        return (m, errorProbability);
    }

    // Pagalbinė funkcija gauti 'taip/ne' atsakymui norimam klausimui (prompt) ir veiksmo atlikimui (callback), jei atsakymas yra 'taip'.
    private static void RunYesNoPrompt(string prompt, Action callback)
    {
        WriteLine($"{prompt} Jei taip - paspauskite 't'. Jei ne - bet kokį kitą simbolį.", ConsoleColor.Blue);
        var key = Console.ReadKey(intercept: true);

        if (key.KeyChar is 't')
            callback();
    }

    // Pagalbinė parametrų išspausdinimui.
    private void WriteHeader()
    {
        WriteLine($"m = {_m}, p = {_errorProbability}\n", ConsoleColor.Blue);
    }

    // Pagalbinė scenarijų išspausdinimui.
    private static void WriteScenarios()
    {
        WriteLine(
            $"1 - Užrašyti programos nurodyto ilgio vektorių\n" +
            $"2 - Nurodyti tekstą (tekstinis failas)\n" +
            $"3 - Nurodyti paveiksliuką (bmp failas)\n" +
            $"4 - Pakeisti parametrus");
    }

    // Pagalbinė funkcija string tipo spausdinimui.
    private static void WriteLine(string text, ConsoleColor color = ConsoleColor.Gray)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = ConsoleColor.Gray;
    }
    
    // Pagalbinė funkcija byte[] (kai laikome baitų masyvą bitų masyvu) tipo spausdinimui.
    private static void WriteLine(byte[] bits, ConsoleColor color = ConsoleColor.Gray)
    {
        Console.ForegroundColor = color;

        foreach (var bit in bits)
            Console.Write(bit);

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Gray;
    }

    // Pagalbinė funkcija int[] tipo spausdinimui.
    private static void WriteLine(int[] ints, ConsoleColor color = ConsoleColor.Gray)
    {
        Console.ForegroundColor = color;

        foreach (var number in ints)
            Console.Write($"{number} ");

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Gray;
    }

    // Pagalbinė funkcija byte (kai laikom baitą bitu) tipo spausdinimui.
    private static void Write(byte bit, ConsoleColor color = ConsoleColor.Gray)
    {
        Console.ForegroundColor = color;
        Console.Write(bit);
        Console.ForegroundColor = ConsoleColor.Gray;
    }
}

