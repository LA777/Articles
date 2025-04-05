using System.IO.Pipes;
using System.Text;

namespace NamedPipesMultiThreading;

sealed class StringPipe(PipeStream pipeStream)
{
    private readonly PipeStream _pipeStream = pipeStream ?? throw new ArgumentNullException(nameof(pipeStream));
    private readonly UnicodeEncoding _streamEncoding = new();

    public string ReadString()
    {
        var byte1 = _pipeStream.ReadByte();
        var byte2 = _pipeStream.ReadByte();
        var bufferLength = (short)(byte1 | (byte2 << 8));
        var inputBuffer = new byte[bufferLength];
        _pipeStream.ReadExactly(inputBuffer, 0, bufferLength);

        return _streamEncoding.GetString(inputBuffer);
    }

    public void WriteString(string outputString)
    {
        var outputBuffer = _streamEncoding.GetBytes(outputString);
        var bufferLength = outputBuffer.Length;
        if (bufferLength > ushort.MaxValue)
        {
            throw new NotSupportedException("Text is too long.");
        }
        _pipeStream.WriteByte((byte)(bufferLength & 0xFF));
        _pipeStream.WriteByte((byte)((bufferLength >> 8) & 0xFF));
        _pipeStream.Write(outputBuffer, 0, bufferLength);
        _pipeStream.Flush();
    }
}
