using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace MuseumClient.Services
{
    // Обычный ByteArrayContent не даёт узнать, сколько байт уже реально
    // ушло на сервер. Эта обёртка пишет в поток запроса кусками и репортит
    // прогресс (0..100) через IProgress<double> — используется для полосы
    // загрузки при отправке больших файлов (видео).
    public class ProgressableByteArrayContent : ByteArrayContent
    {
        private readonly byte[] _content;
        private readonly int _bufferSize;
        private readonly IProgress<double>? _progress;

        public ProgressableByteArrayContent(
            byte[] content,
            IProgress<double>? progress = null,
            int bufferSize = 81920)
            : base(content)
        {
            _content = content;
            _progress = progress;
            _bufferSize = bufferSize;
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            var buffer = new byte[_bufferSize];
            long uploaded = 0;

            using var source = new MemoryStream(_content);

            int bytesRead;
            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await stream.WriteAsync(buffer, 0, bytesRead);

                uploaded += bytesRead;

                if (_content.Length > 0)
                {
                    _progress?.Report((double)uploaded / _content.Length * 100);
                }
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            length = _content.Length;
            return true;
        }
    }
}