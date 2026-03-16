namespace Tcp.AudioClient.Console.Core;

public interface IAudioClient
{
  bool IsConnected { get; }
  
  event Action<string> OnStatusChanged;
  event Action<string> OnMessageReceived;

  Task ConnectAsync(string ipAddress, int port);
  Task SendMessageAsync(string message);
  void Disconnect();
}