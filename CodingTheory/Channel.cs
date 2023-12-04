namespace CodingTheory;

public sealed class Channel
{
    private readonly Random _random;

    // Atsitiktinių skaičių generatorių iškart inicizializuojam. Galima nurodyti ir konkretų seed, jei norima rezultatų pakartojamumo.
    public Channel()
    {
        _random = new Random(Guid.NewGuid().GetHashCode());
    }

    // Kanalu perduodamas žodis, nurodoma kokia turi būt klaidos tikimybė.
    // Grąžinamas žodis, kuris gali būti su klaidomis bei klaidų pozicijos.
    public byte[] Transmit(byte[] word, double errorProbability, out int[] errorPositions)
    {
        if (errorProbability is 0)
        {
            errorPositions = [];
            return word;
        }

        var errorPositionsList = new List<int>();
        var transmittedWord = new byte[word.Length];

        for (var i = 0; i < word.Length; i++)
        {
            var randomValue = _random.NextDouble();

            if (randomValue < errorProbability)
            {
                transmittedWord[i] = (byte)(1 - word[i]);
                errorPositionsList.Add(i);
            }
            else
                transmittedWord[i] = word[i];
        }

        errorPositions = [.. errorPositionsList];
        return transmittedWord;
    }
}
