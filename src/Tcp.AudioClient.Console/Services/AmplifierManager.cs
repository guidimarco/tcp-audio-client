using Tcp.AudioClient.Console.Core;
using Tcp.AudioClient.Console.Infrastructure;
using Tcp.AudioClient.Console.Models;

namespace Tcp.AudioClient.Console.Services;

public class AmplifierManager
{
  private readonly IAudioClient _audioClient;
  private readonly AmplifierState _amplifierState;
  private ConnectionStatus? _status = null;
  private readonly string _ipAddress;
  private readonly int _port;
  private readonly int _interval;
  private bool _isUserExiting = false;
  private bool _isFirstConnection = true;
  private CancellationTokenSource? _cts;

  public AmplifierManager(IAudioClient audioClient, AmplifierState amplifierState, string ipAddress, int port, int interval)
  {
    _audioClient = audioClient;
    _amplifierState = amplifierState;
    _ipAddress = ipAddress;
    _port = port;
    _interval = interval;

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
    _cts = new CancellationTokenSource();
    Logger.Info("Starting Daemon...");
    await ConnectionLoop(_cts.Token);
  }

  private async Task ConnectionLoop(CancellationToken ct)
  {
    while (!_isUserExiting && !ct.IsCancellationRequested)
    {
      try
      {
        await _audioClient.ConnectAsync(_ipAddress, _port);

        if (_isFirstConnection)
        {
          await _audioClient.SendMessageAsync("INIT\r");
          _isFirstConnection = false;
          Logger.Info("Session Started");
        }

        while (!_isUserExiting && _audioClient.IsConnected)
        {
          await Task.Delay(1000, ct);
        }
      }
      catch (OperationCanceledException) {}
      catch (Exception)
      {
        Logger.Info($"Retrying connection in {_interval} seconds...");
        await Task.Delay(_interval * 1000, ct);
      }
    }
    Logger.Info("Daemon stopped");
  }

  /// <summary>
  /// Handle the incoming message.
  /// </summary>
  /// <param name="message">The message to handle.</param>
  private void HandleIncomingMessage(string message)
  {
    try
    {
      ResponseParser.Parse(message, _amplifierState);
    }
    catch (Exception ex)
    {
      Logger.Warning($"Failed to parse message: {ex.Message} on message: {message}");
    }
  }

  /// <summary>
  /// Handle the status changed.
  /// </summary>
  /// <param name="status">The status to handle.</param>
  private void HandleStatusChanged(ConnectionStatus status)
  {
    if (status == _status) return;

    _status = status;
    string message = $"New State: {status}";
    switch (status)
    {
      case ConnectionStatus.Connecting:
        Logger.Info(message);
        break;
      case ConnectionStatus.Connected:
        Logger.Success(message);
        break;
      case ConnectionStatus.Disconnected:
        Logger.Warning(message);
        break;
      case ConnectionStatus.Error:
        Logger.Error(message);
        break;
      default:
        Logger.Info(message);
        break;
    }
  }

  /// <summary>
  /// User requested to exit the application.
  /// </summary>
  public void Stop()
  {
    _isUserExiting = true;
    _cts?.Cancel();
    _audioClient.Disconnect();
  }
}