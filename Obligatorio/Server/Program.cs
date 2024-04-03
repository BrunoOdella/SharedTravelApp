using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IPEndPoint localEndPoint = new IPEndPoint(
                IPAddress.Parse("127.0.0.1"), 5000
            );

            Socket soc = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp
            );

            soc.Bind(localEndPoint);
            soc.Listen(10);
            Console.WriteLine("Esperando clientes ...");

            int clients = 1;

            while (true)
            {
                Socket client = soc.Accept();
                int n = clients;
                new Thread(() => manageClient(client, n)).Start();
                clients++;

            }

        }
        public static void manageClient(Socket client, int actualClient)
        {
            Console.WriteLine($"El cliente {actualClient} se conecto");

            while (true)
            {
                byte[] buffer = new byte[8];
                int receivedBytes = client.Receive(buffer);

                if (receivedBytes == 0)
                {
                    Console.WriteLine("El cliente se desconecto");
                    break;
                }
                else
                {
                    string message = Encoding.UTF8.GetString(buffer);
                    Console.WriteLine($"El cliente {actualClient} : {message}");
                }

            }

            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }
    }
}
