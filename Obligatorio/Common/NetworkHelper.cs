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

        public void Send(byte[] data)
        {
            int size = data.Length;
            int offset = 0;
            while (offset < size)
            {
                int bytesToSend = size - offset;
                _stream.Write(data, offset, bytesToSend);
                offset += bytesToSend;
            }
        }

        public byte[] Receive(int dataLength)
        {
            byte[] buffer = new byte[dataLength];
            int offset = 0;
            int size = dataLength;
            while (offset < size)
            {
                int bytesReceived = _stream.Read(buffer, offset, size - offset);
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