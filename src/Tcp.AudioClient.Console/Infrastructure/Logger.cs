using System;
using System.Runtime.CompilerServices;

namespace Tcp.AudioClient.Console.Infrastructure;

public class Logger
{
  private static void Log(string message, string level, System.ConsoleColor color, string source = "Unknown")
  {
    var oldColor = System.Console.ForegroundColor;
    System.Console.ForegroundColor = color;
    string fileName = Path.GetFileNameWithoutExtension(source);
    System.Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{fileName}] [{level}] {message}");
    System.Console.ForegroundColor = oldColor;
  }

  public static void Info(string message, [CallerFilePath] string source = "Unknown")
  {
    Log(message, "INFO", System.ConsoleColor.White, source);
  }

  public static void Success(string message, [CallerFilePath] string source = "Unknown")
  {
    Log(message, "OK", System.ConsoleColor.Green, source);
  }

  public static void Warning(string message, [CallerFilePath] string source = "Unknown")
  {
    Log(message, "WARNING", System.ConsoleColor.Yellow, source);
  }

  public static void Error(string message, [CallerFilePath] string source = "Unknown")
  {
    Log(message, "ERROR", System.ConsoleColor.Red, source);
  }
}