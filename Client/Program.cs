using System.Net.Sockets;
using System.Text;

namespace Client
{
    public class Program
    {
        public static void Main()
        {
            Client();
        }

        static void Client()
        {
            Console.Write("Digite seu nome de usuário: ");
            string username = Console.ReadLine()!;

            string host = "192.168.0.123";
            int port = 8080;

            try
            {
                // Conecta ao servidor
                TcpClient client = new TcpClient(host, port);
                Console.WriteLine($"{username} foi Conectado ao servidor.");
                Console.WriteLine("Digite 'Sair' para desconectar do chat!");

                NetworkStream stream = client.GetStream();
                byte[] data = Encoding.UTF8.GetBytes(username);
                stream.Write(data, 0, data.Length);

                Thread receiveThread = new Thread(() => Messages(client));
                receiveThread.Start();

                while (true)
                {
                    var message = Console.ReadLine();
                    if (message.ToLower() == "sair")
                    {
                        break;
                    }
                    data = Encoding.UTF8.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                }

                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no cliente: {ex.Message}");
            }
        }

        static void Messages(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];

                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao receber mensagens: {ex.Message}");
            }
        }
    }
}