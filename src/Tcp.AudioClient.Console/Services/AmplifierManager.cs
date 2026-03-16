using Tcp.AudioClient.Console.Core;
using Tcp.AudioClient.Console.Infrastructure;
using Tcp.AudioClient.Console.Models;

namespace Tcp.AudioClient.Console.Services;

public class AmplifierManager
{
  private readonly IAudioClient _audioClient;
  private readonly AmplifierState _amplifierState;
  private readonly string _ipAddress;
  private readonly int _port;
  private bool _isUserExiting = false;

  public AmplifierManager(IAudioClient audioClient, AmplifierState amplifierState, string ipAddress, int port)
  {
    _audioClient = audioClient;
    _amplifierState = amplifierState;
    _ipAddress = ipAddress;
    _port = port;

    _audioClient.OnMessageReceived += HandleIncomingMessage;
    _audioClient.OnStatusChanged += HandleStatusChanged;
  }

  /// <summary>
  /// Start the amplifier manager daemon.
  /// </summary>
  /// <returns>The completed task.</returns>
  public async Task StartAsync()
  {
    _isUserExiting = false;
    Logger.Info("Starting amplifier manager daemon...");
    await ConnectionLoop();
  }

  private async Task ConnectionLoop()
  {
    while (!_isUserExiting)
    {
      try
      {
        await _audioClient.ConnectAsync(_ipAddress, _port);
        await _audioClient.SendMessageAsync("INIT\r");

        while (!_isUserExiting && _audioClient.IsConnected)
        {
          await Task.Delay(1000);
        }
      }
      catch (Exception ex)
      {
        Logger.Error($"Error connecting to the amplifier: {ex.Message}");
        Logger.Info("Retrying connection in 2 seconds...");
        await Task.Delay(2000);
      }
    }
    Logger.Info("Amplifier manager daemon stopped");
  }

  /// <summary>
  /// Handle the incoming message.
  /// </summary>
  /// <param name="message">The message to handle.</param>
  private void HandleIncomingMessage(string message)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(message)) return;
      Logger.Success($"Message received: {message}");
    }
    catch (Exception ex)
    {
      Logger.Error($"Failed to parse message: {ex.Message} on message: {message}");
    }
  }

  /// <summary>
  /// Handle the status changed.
  /// </summary>
  /// <param name="status">The status to handle.</param>
  private void HandleStatusChanged(string status)
  {
    if (status.Equals("Connected", StringComparison.OrdinalIgnoreCase))
    {
      Logger.Success("Connected to the amplifier");
    }
    else if (status.Equals("Disconnected", StringComparison.OrdinalIgnoreCase))
    {
      Logger.Error("Disconnected from the amplifier");
    }
  }

  /// <summary>
  /// Set the volume of the amplifier.
  /// </summary>
  /// <param name="volume">The volume to set.</param>
  public async Task SetVolumeAsync(int volume)
  {
    try
    {
    if (volume < 0) volume = 0;
    if (volume > 100) volume = 100;

      _amplifierState.Volume = volume;
      await _audioClient.SendMessageAsync($"MV{volume:D3}\r");
    }
    catch (Exception ex)
    {
      Logger.Error($"Failed to send volume command: {ex.Message}");
    }
  }

  /// <summary>
  /// User requested to exit the application.
  /// </summary>
  public void Stop()
  {
    _isUserExiting = true;
    _audioClient.Disconnect();
  }
}