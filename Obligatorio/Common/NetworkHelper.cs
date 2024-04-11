using System.Net.Sockets;

namespace Common
{
    public class NetworkHelper
    {
        private Socket _socket;
        public NetworkHelper(Socket socket)
        {
            this._socket = socket;
        }

        public void Send(byte[] data)
        {
            int size = data.Length;
            int offset = 0;
            while (offset < size)
            {
                int bytesEnviado = _socket.Send(data, offset, size - offset, SocketFlags.None);
                offset += bytesEnviado;
            }

        }

        public byte[] Receive(int dataLength)
        {
            byte[] buffer = new byte[dataLength];

            int offset = 0;
            int size = dataLength;
            while (offset < size)
            {
                int bytesRecibidos = _socket.Receive(buffer, offset, size - offset, SocketFlags.None);
                if (bytesRecibidos == 0)
                {
                    throw new Exception();
                }
                offset += bytesRecibidos;
            }
            return buffer;
        }
    }
}