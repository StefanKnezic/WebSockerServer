using System.Net.WebSockets;
using System.Text;
using System.Linq;

namespace WebSockerServer.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class WebSocketServer
    {
        private readonly RequestDelegate _next;

        private readonly WeatherForecast _message;

        private readonly WebSocketServerConnectionManager _manager;

        public WebSocketServer(RequestDelegate next, WeatherForecast message, WebSocketServerConnectionManager manager)
        {
            _next = next;
            _message = message;
            _manager = manager;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                Console.WriteLine("websocket connected");

                string ConnID = _manager.AddSocket(webSocket);
                await _message.SendConnIDAsync(webSocket, ConnID);

                await _message.ReceiveMessage(webSocket, async (result, buffer) =>
                {
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        Console.WriteLine("message received");
                        Console.WriteLine($"Message : {Encoding.UTF8.GetString(buffer,0,result.Count)}");
                       await _message.RouteJsonAsync(Encoding.UTF8.GetString(buffer, 0, result.Count));
                        return;

                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        string id = _manager.GetAllSockets().FirstOrDefault(s => s.Value == webSocket).Key;
                        

                        _manager.GetAllSockets().TryRemove(id, out WebSocket socket);
                        await socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);

                    }
                });
            }
            else
            {
                Console.WriteLine("cao iz use middleware");
                await _next(context);

                
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class WebSocketServerExtensions
    {
        public static IApplicationBuilder UseWebSocketServer(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<WebSocketServer>();
        }
    }



    

}


