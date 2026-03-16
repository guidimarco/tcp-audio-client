using System.Globalization;
using Tcp.AudioClient.Console.Models;

namespace Tcp.AudioClient.Console.Infrastructure;

public class ResponseParser
{
  public static void Parse(string message, AmplifierState state)
  {
    if (string.IsNullOrWhiteSpace(message)) return;

    var cmd = message.Trim().ToUpper();

    if (cmd.StartsWith("MV"))
    {
      string value = cmd.Substring(2);
      if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double rawVolume))
      {
        double normalizedVolume = rawVolume / 10.0;
        int volume = (int) Math.Floor(normalizedVolume);
        volume = Math.Clamp(volume, 0, 100);
        state.Volume = volume;
        Logger.Success($"Volume Updated: {volume}");
      }
      else
      {
        Logger.Warning($"Failed to Parse Volume: {value}");
        throw new FormatException($"Failed to Parse Volume: {value}");
      }
    }
    else if (cmd.StartsWith("ZM"))
    {
      state.IsOn = cmd.EndsWith("ON");
      Logger.Success($"Power Updated: {state.IsOn}");
    }
    else
    {
      throw new InvalidOperationException($"Unknown Command: {cmd}");
    }
  }
}