namespace CodingTheory;

// Galima ištaisyti tik [(d - 1) / 2] klaidas, o d = 2^(m-1)
public class Decoder
{
    // Bazinė matrica H, apibrėžta literatūroje [HLL91, §3.8–3.9, p. 89–95]
    private static readonly int[][] BaseMatrix =
    {
        [1, 1],
        [1, -1]
    };

    // Visų H matricų, kurios gali būti gautos iš bazinės matricos H, sąrašas.
    private readonly List<int[][]> _matrices;
    
    private readonly int _m;

    // Kuriant dekoderį, sugeneruojam visas H_1, H_2, ..., H_m matricas.
    public Decoder(int m)
    {
        _m = m;
        _matrices = new List<int[][]>(capacity: m);
        InitializeMatrixSeries(m);
    }

    // Dekoduojam 'word'. 'Word' iteratyviai dauginamas iš visų H_1, H_2, ..., H_m matricų.
    // Grąžinamas dekoduotas žodžis.
    // Naudojamas 3.9.4 algoirtmas iš [HLL91, §3.8–3.9, p. 89–95].
    public byte[] Decode(byte[] word)
    {
        var modifiedWord = new int[word.Length];
        for (var i = 0; i < word.Length; i++)
            modifiedWord[i] = word[i] is 0 ? -1 : 1;

        for (var i = 0; i < _matrices.Count; i++)
            modifiedWord = MultiplyVectorByMatrix(modifiedWord, _matrices[i]);

        var largestComponent = int.MinValue;
        var positionOfLargestComponent = -1;

        for (var i = 0; i < modifiedWord.Length; i++)
        {
            var abosuluteValue = Math.Abs(modifiedWord[i]);

            if (abosuluteValue > largestComponent)
            {
                largestComponent = abosuluteValue;
                positionOfLargestComponent = i;
            }
        }

        var decodedPartAsBinaryNumberString = Convert.ToString(positionOfLargestComponent, 2).PadLeft(_m, '0');

        var decodedWord = new byte[_m + 1];
        decodedWord[0] = modifiedWord[positionOfLargestComponent] > 0 ? (byte)1 : (byte)0;

        for (var i = 1; i < _m + 1; i++)
            decodedWord[i] = (byte)(decodedPartAsBinaryNumberString[_m - i] - '0');

        return decodedWord;
    }

    // Paduotas vektorius dauginamas iš paduotos matricos.
    // Rezultatas - naujas vektorius.
    private int[] MultiplyVectorByMatrix(int[] vector, int[][] matrix)
    {
        var result = new int[matrix[0].Length];

        for (var i = 0; i < matrix[0].Length; i++)
            for (var j = 0; j < vector.Length; j++)
                result[i] += vector[j] * matrix[j][i];

        return result;
    }

    // Sugeneruojamas nurodytas skaičius H matricų.
    // Pvz.: Jei m = 3, sugeneruojamos H_1, H_2, H_3 matricos.
    // Čia, H matricos - vėl gi, iš [HLL91, §3.8–3.9, p. 89–95] literatūros.
    private void InitializeMatrixSeries(int matrixCount)
    {
        for (var i = 1; i <= matrixCount; ++i)
        {
            var firstIdentityMatrix = Utils.IdentityMatrix(size: (int)Math.Pow(2, matrixCount - i));
            var lastIdentityMatrix = Utils.IdentityMatrix(size: (int)Math.Pow(2, i - 1));

            var matrixResult = firstIdentityMatrix.KroneckerProduct(BaseMatrix).KroneckerProduct(lastIdentityMatrix);
            _matrices.Add(matrixResult);
        }
    }
}
