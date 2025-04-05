using System;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public sealed class StringPipe
    {
        private readonly PipeStream _pipeStream;
        private readonly UnicodeEncoding _streamEncoding = new UnicodeEncoding();

        public StringPipe(PipeStream pipeStream)
        {
            _pipeStream = pipeStream ?? throw new ArgumentNullException(nameof(pipeStream));
        }

        public async Task<string> ReadStringAsync()
        {
            var byte1 = _pipeStream.ReadByte();
            var byte2 = _pipeStream.ReadByte();
            var bufferLength = (short)(byte1 | (byte2 << 8));
            var inputBuffer = new byte[bufferLength];
            await _pipeStream.ReadAsync(inputBuffer, 0, bufferLength);

            return _streamEncoding.GetString(inputBuffer);
        }

        public async Task WriteStringAsync(string outputString)
        {
            if (string.IsNullOrEmpty(outputString))
            {
                return;
            }

            var outputBuffer = _streamEncoding.GetBytes(outputString);
            var bufferLength = outputBuffer.Length;
            if (bufferLength > ushort.MaxValue)
            {
                throw new NotSupportedException("Text is too long.");
            }
            _pipeStream.WriteByte((byte)(bufferLength & 0xFF));
            _pipeStream.WriteByte((byte)((bufferLength >> 8) & 0xFF));
            await _pipeStream.WriteAsync(outputBuffer, 0, bufferLength);
            await _pipeStream.FlushAsync();
        }
    }
}
