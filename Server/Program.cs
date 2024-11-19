using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public class Program
    {
        static List<TcpClient> clients = new List<TcpClient>();
        static Dictionary<TcpClient, string> clientUsernames = new Dictionary<TcpClient, string>();

        public static void Main(string[] args)
        {
            Server();
        }

        static void Server()
        {
            //configura a porta e o ip da rede (que nesse caso vai ser dentro do roteador de internet de um celular) 
            IPAddress ipAddress = IPAddress.Parse("192.168.0.123");
            Console.WriteLine($"Host: {ipAddress}");
            int port = 8080;
            Console.WriteLine($"Porta: {port}");

            TcpListener listener = new TcpListener(ipAddress, port);

            try
            {
                listener.Start();
                Console.WriteLine("Servidor Online. Aguardando conexões... em 192.168.0.123:8080");

                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();

                    //define o ip do cliente e associa ao username
                    NetworkStream stream = client.GetStream();
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string username = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    Console.WriteLine($"Nova Conexão: {((IPEndPoint)client.Client.RemoteEndPoint!).Address}:{((IPEndPoint)client.Client.RemoteEndPoint).Port} ({username}) conectado.");

                    clients.Add(client);
                    clientUsernames[client] = username;

                    Thread clientThread = new Thread(() => ClientRequest(client, username));
                    clientThread.Start();

                    BroadCast($"{username} entrou no chat!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no servidor: {ex.Message}");
            }
            finally
            {
                listener.Stop();
            }
        }

        static void ClientRequest(TcpClient client, string username)
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
                    Console.WriteLine($"Mensagem recebida de {username}: {message}");

                    if (message.StartsWith("@"))
                    {
                        Unicast(client, message, username);
                    }
                    else
                    {
                        BroadCast($"{username}: {message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao lidar com o cliente: {ex.Message}");
            }
            finally
            {
                clients.Remove(client);
                clientUsernames.Remove(client);
                client.Close();

                BroadCast($"{username} saiu do chat.");
            }
        }

        static void Unicast(TcpClient sender, string message, string senderUsername)
        {
            try
            {
                string[] parts = message.Split(' ', 2);
                string targetUsername = parts[0].Substring(1);
                string unicastMessage = parts[1];

                foreach (var client in clients)
                {
                    if (clientUsernames[client] == targetUsername)
                    {
                        NetworkStream stream = client.GetStream();
                        byte[] data = Encoding.UTF8.GetBytes($"{senderUsername} (privado): {unicastMessage}");
                        stream.Write(data, 0, data.Length);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar mensagem unicast: {ex.Message}");
            }
        }

        static void BroadCast(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);

            foreach (var client in clients)
            {
                try
                {
                    NetworkStream stream = client.GetStream();
                    stream.Write(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao enviar mensagem broadcast: {ex.Message}");
                }
            }
        }
    }
}