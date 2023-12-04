using System.Text;

namespace CodingTheory;

// Pagalbinių funckijų klasė.
public static class Utils
{
    // Priima srautą simbolių. Įprastai tai bus failas.
    // Grąžina poziciją, nuo kurios BMP faile prasideda pikselių masyvas.
    public static uint GetPixelOffset(BinaryReader binaryReader)
    {
        // Pixelių masyvo offset'o aprašyma aprašytas 10-13 baituose.
        binaryReader.BaseStream.Seek(10, SeekOrigin.Begin);
        var pixelOffset = binaryReader.ReadUInt32();
        binaryReader.BaseStream.Seek(0, SeekOrigin.Begin);

        return pixelOffset;
    }

    // Sudaugina pirmą matricą su antra ir grąžina matricą.
    // Daugyba šiuo atveju apibrėžiama kaip Kroneckerio sandauga.
    // Daugiau: https://en.wikipedia.org/wiki/Kronecker_product.
    public static int[][] KroneckerProduct(this int[][] firstMatrix, int[][] secondMatrix)
    {
        var firstMatrixRows = firstMatrix.Length;
        var firstMatrixColumns = firstMatrix[0].Length;
        var secondMatrixRows = secondMatrix.Length;
        var secondMatrixColumns = secondMatrix[0].Length;

        var resultMatrix = new int[firstMatrixRows * secondMatrixRows][];
        for (var rowIndex = 0; rowIndex < firstMatrixRows * secondMatrixRows; rowIndex++)
            resultMatrix[rowIndex] = new int[firstMatrixColumns * secondMatrixColumns];

        for (var firstMatrixRowIndex = 0; firstMatrixRowIndex < firstMatrixRows; firstMatrixRowIndex++)
            for (var firstMatrixColumnIndex = 0; firstMatrixColumnIndex < firstMatrixColumns; firstMatrixColumnIndex++)
                for (var secondMatrixRowIndex = 0; secondMatrixRowIndex < secondMatrixRows; secondMatrixRowIndex++)
                    for (var secondMatrixColumnIndex = 0; secondMatrixColumnIndex < secondMatrixColumns; secondMatrixColumnIndex++)
                    {
                        var resultMatrixRowIndex = firstMatrixRowIndex * secondMatrixRows + secondMatrixRowIndex;
                        var resultMatrixColumnIndex = firstMatrixColumnIndex * secondMatrixColumns + secondMatrixColumnIndex;

                        resultMatrix[resultMatrixRowIndex][resultMatrixColumnIndex] =
                            firstMatrix[firstMatrixRowIndex][firstMatrixColumnIndex] *
                            secondMatrix[secondMatrixRowIndex][secondMatrixColumnIndex];
                    }

        return resultMatrix;
    }

    // Sukuria nurodyto dydžio vienetinę matricą.
    public static int[][] IdentityMatrix(int size)
    {
        var matrix = new int[size][];

        for (var rowIndex = 0; rowIndex < size; rowIndex++)
            matrix[rowIndex] = new int[size];

        for (int i = 0; i < matrix.Length; ++i)
            for (int j = 0; j < matrix[0].Length; ++j)
                matrix[i][j] = i == j ? 1 : 0;

        return matrix;
    }

    // Gautą matricą pavaizduoja gražiau. Buvo reikalinga tik testavimo tikslams.
    // Palikta, nes galbūt prireiks atsiskaitymo metu parodyti matricas.
    public static string ToMatrixString<T>(this T[][] matrix, string delimiter = "  ")
    {
        var s = new StringBuilder();

        for (var i = 0; i < matrix.GetLength(0); i++)
        {
            for (var j = 0; j < matrix[i].GetLength(0); j++)
            {
                s.Append(matrix[i][j]).Append(delimiter);
            }

            s.AppendLine();
        }

        return s.ToString();
    }
}
