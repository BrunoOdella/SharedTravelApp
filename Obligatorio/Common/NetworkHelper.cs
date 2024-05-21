using System.Net.Sockets;

namespace Common
{
    public class NetworkHelper
    {
        private TcpClient _client;
        private NetworkStream _stream;

        public NetworkHelper(TcpClient client)
        {
            _client = client;
            _stream = client.GetStream();
        }

        public async Task SendAsync(byte[] data, CancellationToken token)
        {
            await _stream.WriteAsync(data, 0, data.Length, token);
        }

        public async Task<byte[]> ReceiveAsync(int dataLength, CancellationToken token)
        {
            byte[] buffer = new byte[dataLength];
            int offset = 0;
            int size = dataLength;
            while (offset < size)
            {
                int bytesReceived = await _stream.ReadAsync(buffer, offset, size - offset, token);
                if (bytesReceived == 0)
                {
                    throw new Exception("Connection closed by remote host.");
                }
                offset += bytesReceived;
            }
            return buffer;
        }
    }
}