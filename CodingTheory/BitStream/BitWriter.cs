namespace CodingTheory.BitStream;


// Klasė skirta manipuliavimui su bitais (ne baitais).
// Kadangi C# neturi bitų tipo, tai naudojami byte tipo masyvai.
// Ši klasė skirta srauto rašymui į srautą.
// Pavyzdžiui, jei įrašome '01100001', tai bus įrašomas toks baitas, kuris atitinka tokią bitų reprezentaciją. Šiuo atveju, tai galėtų būti simbolio 'a' ASCII reprezntacija.
public sealed class BitWriter : Stream
{
    public Stream BaseStream => _stream;

    private readonly Stream _stream;

    private readonly byte[] _writeBitsBuffer;
    private readonly byte[] _writeByteBuffer;
    private int _writeBitsBufferPosition = 0;

    public BitWriter(Stream stream, int bitBufferSize = 4096 * 8)
    {
        _stream = stream;

        _writeBitsBuffer = new byte[bitBufferSize];
        _writeByteBuffer = new byte[bitBufferSize / 8];
    }

    public override bool CanWrite => true;

    public override void Write(byte[] buffer, int offset, int count)
    {
        Span<byte> writeBitsBuffer;
        var bits = new Span<byte>(buffer, offset, count);

        while (bits.Length - (_writeBitsBuffer.Length - _writeBitsBufferPosition) > 0)
        {
            var remainingSpaceInWriteBuffer = _writeBitsBuffer.Length - _writeBitsBufferPosition;

            writeBitsBuffer = new Span<byte>(_writeBitsBuffer, _writeBitsBufferPosition, remainingSpaceInWriteBuffer);
            bits[..remainingSpaceInWriteBuffer].CopyTo(writeBitsBuffer);

            bits = bits[remainingSpaceInWriteBuffer..];
            _writeBitsBufferPosition += remainingSpaceInWriteBuffer;

            WriteToUnderlyingStream();
        }

        writeBitsBuffer = new Span<byte>(_writeBitsBuffer, _writeBitsBufferPosition, bits.Length);
        bits.CopyTo(writeBitsBuffer);

        _writeBitsBufferPosition += bits.Length;
    }

    private void WriteToUnderlyingStream()
    {
        if (_writeBitsBufferPosition % 8 != 0)
            throw new InvalidOperationException("Amount of bits written should be divisible by 8.");

        var writeBitsBuffer = new Span<byte>(_writeBitsBuffer, 0, _writeBitsBufferPosition);

        for (var i = 0; i < writeBitsBuffer.Length / 8; i++)
        {
            _writeByteBuffer[i] = (byte)
                (
                    writeBitsBuffer[i * 8 + 0] << 7 |
                    writeBitsBuffer[i * 8 + 1] << 6 |
                    writeBitsBuffer[i * 8 + 2] << 5 |
                    writeBitsBuffer[i * 8 + 3] << 4 |
                    writeBitsBuffer[i * 8 + 4] << 3 |
                    writeBitsBuffer[i * 8 + 5] << 2 |
                    writeBitsBuffer[i * 8 + 6] << 1 |
                    writeBitsBuffer[i * 8 + 7] << 0
                );
        }

        _stream.Write(_writeByteBuffer, 0, writeBitsBuffer.Length / 8);
        _writeBitsBufferPosition = 0;
    }

    public override void Flush()
    {
        WriteToUnderlyingStream();
        _stream.Flush();
    }

    public long SeekToBeginning()
    {
        Flush();
        return _stream.Seek(0, SeekOrigin.Begin);
    }

    protected override void Dispose(bool disposing)
    {
        Flush();
        if (disposing)
            _stream.Dispose();
    }

    public override bool CanRead => throw new NotImplementedException();

    public override bool CanSeek => throw new NotImplementedException();

    public override long Length => throw new NotImplementedException();

    public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotImplementedException();
    }

    public override void SetLength(long value)
    {
        throw new NotImplementedException();
    }
}