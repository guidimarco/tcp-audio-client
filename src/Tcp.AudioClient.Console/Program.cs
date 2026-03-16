using Tcp.AudioClient.Console.Infrastructure;
using Tcp.AudioClient.Console.Services;
using Tcp.AudioClient.Console.Models;

var state = new AmplifierState();
var client = new TcpAudioClient();

/// Get the Configuration

Console.Write("Insert IP Address: [127.0.0.1]: ");
string inputIp = Console.ReadLine() ?? "";
string finalIp = string.IsNullOrWhiteSpace(inputIp) ? "127.0.0.1" : inputIp;

Console.Write("Insert Port: [1234]: ");
string inputPort = Console.ReadLine() ?? "";
int finalPort = int.TryParse(inputPort, out int p) ? p : 1234;

Console.Write("Retry Interval (sec): [4]: ");
string inputRetry = Console.ReadLine() ?? "";
int finalRetry = int.TryParse(inputRetry, out int r) ? r : 4;

Logger.Info($"Starting with IP: {finalIp}, Port: {finalPort}, Retry Interval: {finalRetry}s\n");

/// Start the Application

var manager = new AmplifierManager(client, state, finalIp, finalPort, finalRetry);

Logger.Success("🔊 Amplification Control Started!");
Logger.Info("--------------------------");
Logger.Info("⌨️ Available Commands:");
Logger.Info("- Press 'S' -> See the current state");
Logger.Info("- Press 'Q' -> Exit the application\n");
Logger.Info("📡 Waiting for connection and data...");
Logger.Info("--------------------------\n");

_ = manager.StartAsync();

while (true)
{
  var key = Console.ReadKey(true).Key;
  if (key == ConsoleKey.S) Logger.Info($"Amplifier state: {state}");
  else if (key == ConsoleKey.Q)
  {
    Logger.Info("Application stopped");
    manager.Stop();
    break;
  }
}