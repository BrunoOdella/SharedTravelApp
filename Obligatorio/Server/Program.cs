using System.Net.Sockets;
using System.Net;
using System.Text;
using Common;
using Common.Interfaces;

namespace Server
{
    internal class Program
    {
        static readonly ISettingsManager SettingsMgr = new SettingsManager();
        static void Main(string[] args)
        {
            var localEndPoint = new IPEndPoint(
                                     IPAddress.Parse(SettingsMgr.ReadSetting(ServerConfig.ServerIpConfigKey)),
                                     int.Parse(SettingsMgr.ReadSetting(ServerConfig.SeverPortConfigKey))
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
