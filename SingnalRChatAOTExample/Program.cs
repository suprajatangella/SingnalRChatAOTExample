using Microsoft.AspNetCore.SignalR;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddSignalR();
builder.Services
    .Configure<JsonHubProtocolOptions>(o =>
{
    o.PayloadSerializerOptions
    .TypeInfoResolverChain
    .Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();
app.UseStaticFiles();

app.MapHub<ChatHub>("/chatHub");
app.MapGet("/", () => Results.Content("""
<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <title>SignalR Chat</title>
    <link rel="stylesheet" href="/styles.css" />
    <script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/7.0.5/signalr.min.js"></script>
    
</head>
<body>
    <div class="chat-container">
        <h2>SignalR Chat</h2>
        <input id="userInput" placeholder="Enter your name" />
        <input id="messageInput" placeholder="Type a message" />
        <button onclick="sendMessage()">Send</button>
        <ul id="messages"></ul>
    </div>

    <script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/8.0.7/signalr.min.js"></script>
    <script>
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/chatHub")
            .build();

        connection.on("ReceiveMessage", (user, message) => {
            const li = document.createElement("li");
            li.textContent = `${user}: ${message}`;
            document.getElementById("messages").appendChild(li);
        });

        async function sendMessage() {
            const user = document.getElementById("userInput").value;
            const message = document.getElementById("messageInput").value;
            await connection.invoke("SendMessage", user, message);
        }

        connection.start().catch(err => console.error(err));
    </script>
</body>
</html>
""", "text/html"));

app.Run();

[JsonSerializable(typeof(string))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }

public class ChatHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}
