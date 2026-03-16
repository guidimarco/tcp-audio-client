using Tcp.AudioClient.Console.Infrastructure;

Logger.Info("Application ready!.");

var client = new TcpAudioClient();

client.OnMessageReceived += (message) => Logger.Success($"Message received: {message}");
client.OnStatusChanged += (status) => Logger.Info($"Status changed: {status}");

await client.ConnectAsync("127.0.0.1", 1234);

await client.SendMessageAsync("INIT\r");

Console.WriteLine("Listening for messages...");

await Task.Delay(-1);