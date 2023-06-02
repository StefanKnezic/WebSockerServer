using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Threading;
using WebSockerServer;
using WebSockerServer.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<WeatherForecast>();
builder.Services.AddSingleton<WebSocketServerConnectionManager>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseWebSockets();

app.UseWebSocketServer();

app.Run(async context =>
{
    Console.WriteLine("hello from run");
    await context.Response.WriteAsync("hello nekom drugom tamo klijentu ");
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();



app.Run();


// void WriteRequestParam(HttpContext context)
//{
//    Console.WriteLine("Request method: " + context.Request.Method);
//    Console.WriteLine("Request protocol: " + context.Request.Protocol);

//    if (context.Request.Headers != null)
//    {
//        foreach (var header in context.Request.Headers)
//        {
            
//            Console.WriteLine("--> Header key:" + header.Key + " Header value: " + header.Value);
//        }
//    }
//}

/*async Task ReceiveMessage(WebSocket socket, Action<WebSocketReceiveResult,byte[]> handleMessage)
{
    var buffer = new byte[1024 * 4];   
    
    while (socket.State == WebSocketState.Open)
    {
        var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer),cancellationToken: CancellationToken.None);

         handleMessage(result,buffer);
    }
}*/