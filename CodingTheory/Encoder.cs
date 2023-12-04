namespace CodingTheory;

public sealed class Encoder
{
    // Generuojanti matrica.
    public readonly byte[][] _matrix;
    private readonly int _m;

    // Sukurdami encoder'į, sukuriam ir kodą generuojančią matricą.
    public Encoder(int m)
    {
        _m = m;
        _matrix = ConstructGeneratorMatrix(m);
    }

    // Duotą žodį užkoduoja ir jį grąžina.
    public byte[] Encode(byte[] word)
    {
        if (word.Length != _m + 1)
            throw new ArgumentException("Word length must be equal to m + 1.");

        var encodedWord = new byte[_matrix[0].Length];

        for (var i = 0; i < _matrix[0].Length; i++)
            for (var j = 0; j < word.Length; j++)
                encodedWord[i] ^= (byte)(word[j] * _matrix[j][i]);

        return encodedWord;
    }

    // Pasinaudoju 3.2.8 teiginiu.
    // Tegu m >= 2. Matrica, kurios eilutės yra funkcijas x1, x2, . . . , xm ir 1 atitinkantys vektoriai
    // ~x1, ~x2, . . . , ~xm,~1, yra kodo RM(1, m) generuojanti matrica.
    // Atvejis, kai m = 1, yra trivialus.
    // Generuojama kodą generuojanti matrica.
    private static byte[][] ConstructGeneratorMatrix(int m)
    {
        if (m is 1)
            return
            [
                [0, 1],
                [1, 1]
            ];

        var dimension = 1 + m;
        var length = (int)Math.Pow(2, m);

        var matrix = new byte[dimension][];
        for (var rowIndex = 0; rowIndex < dimension; rowIndex++)
            matrix[rowIndex] = new byte[length];

        // Pirma eilutė - vektorius vien iš vienetų.
        matrix[0] = Enumerable.Repeat((byte)1, length).ToArray();

        for (var columnIndex = 0; columnIndex < length; columnIndex++)
        {
            var permutation = GetPermutation(m, columnIndex);

            for (var rowIndex = 1; rowIndex < dimension; rowIndex++)
                matrix[rowIndex][columnIndex] = permutation[dimension - rowIndex - 1];
        }

        return matrix;
    }

    // Gaunama index-oji length ilgio permutacija (su pasikartojimais).
    // Pavyzdžiui visos galimos 2 ilgio binarinės permutacijos: 00, 01, 10, 11,
    // Laikysime, kad, pavyzdžiui, 2 ilgio permutacija, kurios index = 0 bus 00, index = 1, 01 ir t. t.
    private static byte[] GetPermutation(int length, int index)
    {
        var permutationAsString = Convert.ToString(index, 2).PadLeft(length, '0');

        var permutation = new byte[length];

        for (var digitIndex = 0; digitIndex < permutationAsString.Length; digitIndex++)
            permutation[digitIndex] = permutationAsString[digitIndex] is '1' ? (byte)1 : (byte)0;

        return permutation;
    }

}
