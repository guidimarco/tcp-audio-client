namespace Tcp.AudioClient.Console.Models;

public class AmplifierState
{
  public bool IsOn { get; set; }
  public int Volume { get; set; }

  public override string ToString()
  {
    string status = IsOn ? "ON" : "OFF";
    return $"[AmplifierState] Stato: {status}, Volume: {Volume}";
  }
}