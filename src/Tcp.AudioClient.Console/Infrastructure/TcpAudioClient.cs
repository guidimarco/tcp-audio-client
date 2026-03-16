using System.Text;
using System.Net.Sockets;
using Tcp.AudioClient.Console.Core;

namespace Tcp.AudioClient.Console.Infrastructure;

public class TcpAudioClient : IAudioClient
{
  private TcpClient? _client;
  private NetworkStream? _stream;
  private bool _isConnected;

  public event Action<string>? OnStatusChanged;
  public event Action<string>? OnMessageReceived;

  /// <summary>
  /// Listen for messages from the server asynchronously.
  /// </summary>
  /// <returns>The completed task.</returns>
  /// <exception cref="Exception">If an error occurs while listening for messages.</exception>
  private async Task ListenForMessagesAsync()
  {
    byte[] buffer = new byte[1024];
    var messageBuilder = new System.Text.StringBuilder();

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

        string received = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);

        foreach (char c in received)
        {
          if (c == '\r')
          {
            string message = messageBuilder.ToString();
            OnMessageReceived?.Invoke(message);
            messageBuilder.Clear();
          }
          else
          {
            messageBuilder.Append(c);
          }
        }
      }
    }
    catch (Exception ex)
    {
      Logger.Error($"Error listening for messages: {ex.Message}");
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
    while (!_isConnected)
    {
      try
      {
        _client = new TcpClient();
        // ^ ^ ^ TcpClient empty to avoid sync connection
        await _client.ConnectAsync(ipAddress, port);
        _stream = _client.GetStream();
        _isConnected = true;
        OnStatusChanged?.Invoke("Connected");
        Logger.Success("Client connected");
        _ = Task.Run(() => ListenForMessagesAsync());
      }
      catch (Exception ex)
      {
        Logger.Error($"Error connecting to server: {ex.Message}");
        _client?.Dispose();
        await Task.Delay(2000);
      }
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
    if (!_isConnected || _stream == null)
    {
      throw new InvalidOperationException("Client not connected");
    }

    try {
      byte[] data = Encoding.ASCII.GetBytes(message);
      await _stream.WriteAsync(data, 0, data.Length);
      Logger.Info($"Message sent: {message}");
    }
    catch (Exception ex)
    {
      Logger.Error($"Error sending message: {ex.Message}");
      Disconnect();
      throw;
    }
  }

  public void Disconnect()
  {
    _isConnected = false;
    _stream?.Close();
    _client?.Close();
    OnStatusChanged?.Invoke("Disconnected");
    Logger.Info("Client disconnected");
  }
}