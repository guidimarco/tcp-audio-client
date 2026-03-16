using System.Text;
using System.Net.Sockets;
using Tcp.AudioClient.Console.Core;

namespace Tcp.AudioClient.Console.Infrastructure;

public class TcpAudioClient : IAudioClient
{
  private TcpClient? _client;
  private NetworkStream? _stream;
  private volatile bool _isConnected;

  public bool IsConnected => _isConnected;

  public event Action<ConnectionStatus>? OnStatusChanged;
  public event Action<string>? OnMessageReceived;

  /// <summary>
  /// Listen for messages from the server asynchronously.
  /// </summary>
  /// <returns>The completed task.</returns>
  /// <exception cref="Exception">If an error occurs while listening for messages.</exception>
  private async Task ListenForMessagesAsync()
  {
    byte[] buffer = new byte[1024];
    var messageBuilder = new StringBuilder();

    try
    {
      while (_isConnected && _stream != null)
      {
        int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
        if (bytesRead == 0)
        {
          Logger.Info("Server disconnected");
          break;
        }

        string received = Encoding.ASCII.GetString(buffer, 0, bytesRead);
        messageBuilder.Append(received);

        string currentData = messageBuilder.ToString();
        int terminatorIndex;

        while ((terminatorIndex = currentData.IndexOf('\r')) != -1)
        {
          string cmd = currentData.Substring(0, terminatorIndex).Trim();
          if (!string.IsNullOrEmpty(cmd))
          {
            OnMessageReceived?.Invoke(cmd);
          }

          messageBuilder.Remove(0, terminatorIndex + 1);
          currentData = messageBuilder.ToString();
        }
      }
    }
    catch (Exception ex)
    {
      if (_isConnected) Logger.Error($"Read Error: {ex.Message}");
    }
    finally
    {
      Disconnect();
    }
  }

  /// <summary>
  /// Connect to the server asynchronously.
  /// </summary>
  /// <param name="ipAddress">The IP address of the server.</param>
  /// <param name="port">The port of the server.</param>
  /// <returns>The completed task.</returns>
  /// <exception cref="Exception">If an error occurs while connecting to the server.</exception>
  public async Task ConnectAsync(string ipAddress, int port)
  {
    try
    {
      _client = new TcpClient();
      OnStatusChanged?.Invoke(ConnectionStatus.Connecting);

      await _client.ConnectAsync(ipAddress, port);
      _stream = _client.GetStream();
      _isConnected = true;
      
      OnStatusChanged?.Invoke(ConnectionStatus.Connected);
      _ = Task.Run(() => ListenForMessagesAsync());
    }
    catch (Exception ex)
    {
      _isConnected = false;
      _stream?.Dispose();
      _stream = null;
      _client?.Dispose();
      _client = null;
      OnStatusChanged?.Invoke(ConnectionStatus.Error);
      Logger.Warning($"Connection error: {ex.Message}");
      throw;
    }
  }

  /// <summary>
  /// Send an ASCII message to the server asynchronously.
  /// </summary>
  /// <param name="message">The message to send.</param>
  /// <returns>The completed task.</returns>
  /// <exception cref="InvalidOperationException">If the client is not connected.</exception>
  /// <exception cref="Exception">If an error occurs while sending the message.</exception>
  public async Task SendMessageAsync(string message)
  {
    if (!_isConnected || _stream == null) throw new InvalidOperationException("Not connected");

    byte[] data = Encoding.ASCII.GetBytes(message);
    await _stream.WriteAsync(data, 0, data.Length);
    Logger.Info($"Message sent: {message.Trim()}");
  }

  public void Disconnect()
  {
    if (!_isConnected) return;
    
    _isConnected = false;
    _stream?.Dispose();
    _stream = null;
    _client?.Dispose();
    _client = null;
    OnStatusChanged?.Invoke(ConnectionStatus.Disconnected);
  }
}