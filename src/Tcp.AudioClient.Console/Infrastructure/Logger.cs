using System;

namespace Tcp.AudioClient.Console.Infrastructure;

public class Logger
{
  private static void Log(string message, string level, System.ConsoleColor color)
  {
    var oldColor = System.Console.ForegroundColor;
    System.Console.ForegroundColor = color;
    System.Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{level}] {message}");
    System.Console.ForegroundColor = oldColor;
  }

  public static void Info(string message)
  {
    Log(message, "INFO", System.ConsoleColor.White);
  }

  public static void Success(string message)
  {
    Log(message, "OK", System.ConsoleColor.Green);
  }

  public static void Error(string message)
  {
    Log(message, "ERROR", System.ConsoleColor.Red);
  }
}