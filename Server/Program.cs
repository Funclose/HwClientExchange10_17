using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
     class Program
    {
        static void Main(string[] args)
        {
            CurrentServer server = new CurrentServer();
            server.Start();
        }
    }
    class CurrentServer
    {
        private TcpListener _listener;
        private bool _isRunning;

        public CurrentServer()
        {
            _listener = new TcpListener(IPAddress.Any, 8888);
            _isRunning = true;
        }

        private static readonly Dictionary<string, double> exchangeRates = new Dictionary<string, double>()
        {
            {"USD_EURO", 0.91 },
            {"EURO_USD", 1.10 },
            {"USD_UAH", 41.25 },
            {"UAH_USD", 0.025 },
            {"USD_GBP", 0.76 },
            {"GBP_USD", 1.32 }
        };

        public void Start()
        {
            _listener.Start();
            Console.WriteLine("server started!!!");

            while (_isRunning)
            {
                TcpClient client = _listener.AcceptTcpClient();
                Console.WriteLine("client connected");

                Thread clientTread = new Thread(HandleClient);
                clientTread.Start(client);
            }

        }
        private void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = 0;

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                string[] currencies = request.Split(' ');
                if(currencies.Length == 2)
                {
                    string currencyPair = $"{currencies[0]}_{currencies[1]}";
                    if (exchangeRates.ContainsKey(currencyPair))
                    {
                        double rate = exchangeRates[currencyPair];
                        string respose = rate.ToString();
                        byte[] responseByte = Encoding.UTF8.GetBytes(respose);
                        stream.Write(responseByte, 0, responseByte.Length);
                        Console.WriteLine($"server response {respose}");
                    }
                    else
                    {
                        string error = "error: Course not found";
                        byte[] errorbytes = Encoding.UTF8.GetBytes(error);
                        stream.Write(errorbytes, 0, errorbytes.Length);
                        Console.WriteLine("server Response: I don't understand");
                    }
                }
                else
                {
                    string error = "Error i don'y understand this message";
                    byte[] converByte = Encoding.UTF8.GetBytes(error);
                    stream.Write(converByte,0,converByte.Length);
                    Console.WriteLine("i don't understand this message.");
                }
            }
            Console.WriteLine("client disconnected");
            client.Close();
        }
        public void Stop()
        {
            _listener.Stop();
        }
    }
}
