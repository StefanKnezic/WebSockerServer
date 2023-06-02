using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Serialization;
using WebSockerServer.Middleware;

namespace WebSockerServer
{
    public  class WeatherForecast
    {
        private readonly WebSocketServerConnectionManager _manager;
        public WeatherForecast(WebSocketServerConnectionManager manager)
        {
            _manager = manager;
        }
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string? Summary { get; set; }

       public async Task ReceiveMessage(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer), cancellationToken: CancellationToken.None);

                handleMessage(result, buffer);
            }
        }

        public async Task SendConnIDAsync(WebSocket socket, string ConnID)
        {
            var buffer = Encoding.UTF8.GetBytes("ConID: " + ConnID);

            await socket.SendAsync(buffer,WebSocketMessageType.Text,true,CancellationToken.None);
        }

        public async Task RouteJsonAsync(string message)
        {
            var routeOb = JsonConvert.DeserializeObject<dynamic>(message);

            if(Guid.TryParse(routeOb.To.ToString(),out Guid guidoutput))
            {
                Console.WriteLine("targeted");
                var sock = _manager.GetAllSockets().FirstOrDefault(s => s.Key == routeOb.To.ToString());

                if (sock.Value != null)
                {
                    if(sock.Value.State == WebSocketState.Open)
                    {
                        await sock.Value.SendAsync(Encoding.UTF8.GetBytes(routeOb.Message.ToString()), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
                else
                {
                    Console.WriteLine("invalid recipient");
                }
            }
           

            else
            {
                Console.WriteLine("Broadcast");
                foreach (var sock in _manager.GetAllSockets())
                {
                    if(sock.Value.State == WebSocketState.Open)
                    {
                        await sock.Value.SendAsync(Encoding.UTF8.GetBytes(routeOb.Message.ToString()),WebSocketMessageType.Text,true,CancellationToken.None);
                    }
                }
            }
        }

    }
}