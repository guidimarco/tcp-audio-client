using Tcp.AudioClient.Console.Infrastructure;
using Tcp.AudioClient.Console.Services;
using Tcp.AudioClient.Console.Models;

var state = new AmplifierState();
var client = new TcpAudioClient();
var manager = new AmplifierManager(client, state, "127.0.0.1", 1234);

Logger.Info("Avvio del Manager...");
_ = manager.StartAsync();

Console.WriteLine("Digita 'V' per settare il volume, 'Q' per uscire...");

while (true)
{
  var key = Console.ReadKey(true).Key;
  if (key == ConsoleKey.V) await manager.SetVolumeAsync(60);
  else if (key == ConsoleKey.Q)
  {
    manager.Stop();
    break;
  }
}