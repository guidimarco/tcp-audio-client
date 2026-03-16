namespace Tcp.AudioClient.Console.Core;

public enum ConnectionStatus
{
  Disconnected,
  Connecting,
  Connected,
  Error,
}

public interface IAudioClient
{
  bool IsConnected { get; }
  
  event Action<ConnectionStatus> OnStatusChanged;
  event Action<string> OnMessageReceived;

  Task ConnectAsync(string ipAddress, int port);
  Task SendMessageAsync(string message);
  void Disconnect();
}