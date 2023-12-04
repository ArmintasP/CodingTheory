namespace CodingTheory.BitStream;

// Klasė skirta manipuliavimui su bitais (ne baitais).
// Kadangi C# neturi bitų tipo, tai naudojami byte tipo masyvai.
// Ši klasė skirta srauto skaitymui iš srauto.
// Pavyzdžiui, jei iš sraute yra baitas/simbolis 'a', tai nuskaitoma bus [0,1 ,1 , 0, 0, 0, 0, 1], t. y. jo binarinė reprentacija.
public sealed class BitReader : Stream
{
    private readonly Stream _stream;

    private int _readBitBufferPosition = 0;
    private int _readBitBufferFilledLength = 0;
    private readonly byte[] _readBitBuffer;
    private readonly byte[] _readByteBuffer;

    public BitReader(Stream stream, int bitBufferSize = 4096 * 8)
    {
        _stream = stream;

        _readBitBuffer = new byte[bitBufferSize];
        _readByteBuffer = new byte[bitBufferSize / 8];
    }

    public override long Length => _stream.Length * 8;

    public override bool CanRead => true;

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (_readBitBufferPosition >= _readBitBufferFilledLength)
            ReadToBitBuffer();

        var indexOfLastReadBit = _readBitBufferPosition + count > _readBitBufferFilledLength
            ? _readBitBufferFilledLength
            : _readBitBufferPosition + count;

        var numberOfReadBits = indexOfLastReadBit - _readBitBufferPosition;
        Buffer.BlockCopy(_readBitBuffer, _readBitBufferPosition, buffer, offset, numberOfReadBits);

        _readBitBufferPosition = indexOfLastReadBit;

        return numberOfReadBits;
    }

    private void ReadToBitBuffer()
    {
        var bytesReadCount = _stream.Read(_readByteBuffer, 0, _readByteBuffer.Length);

        _readBitBufferPosition = 0;
        _readBitBufferFilledLength = 0;

        foreach (var b in _readByteBuffer[0..bytesReadCount])
        {
            _readBitBuffer[_readBitBufferFilledLength + 0] = (byte)((b & 0b10000000) >> 7);
            _readBitBuffer[_readBitBufferFilledLength + 1] = (byte)((b & 0b01000000) >> 6);
            _readBitBuffer[_readBitBufferFilledLength + 2] = (byte)((b & 0b00100000) >> 5);
            _readBitBuffer[_readBitBufferFilledLength + 3] = (byte)((b & 0b00010000) >> 4);
            _readBitBuffer[_readBitBufferFilledLength + 4] = (byte)((b & 0b00001000) >> 3);
            _readBitBuffer[_readBitBufferFilledLength + 5] = (byte)((b & 0b00000100) >> 2);
            _readBitBuffer[_readBitBufferFilledLength + 6] = (byte)((b & 0b00000010) >> 1);
            _readBitBuffer[_readBitBufferFilledLength + 7] = (byte)((b & 0b00000001) >> 0);
            _readBitBufferFilledLength += 8;
        }
    }

    public long SeekToBeginning()
    {
        _readBitBufferPosition = 0;
        _readBitBufferFilledLength = 0;
        return _stream.Seek(0, SeekOrigin.Begin);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _stream.Dispose();
    }

    public override bool CanSeek => throw new NotImplementedException();

    public override bool CanWrite => throw new NotImplementedException();

    public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public override void Flush()
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

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }
}
