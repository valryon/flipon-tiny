using System;

namespace Pon
{
  /// <summary>
  /// Custom independent logger. Must be wired to do something.
  /// </summary>
  public static class Log
  {
    public static event Action<string, object> LogDebug;
    public static event Action<string, object> LogInfo;
    public static event Action<string, object> LogWarning;
    public static event Action<string, object> LogError;
    public static event Action<Exception> LogException;

    /// <summary>
    /// Print Debug message.
    /// </summary>
    public static void Debug(object message, object context = null)
    {
      LogDebug.Raise(message.ToString(), context);
    }

    /// <summary>
    /// Print Info message.
    /// </summary>
    public static void Info(object message, object context = null)
    {
      LogInfo.Raise(message.ToString(), context);
    }

    /// <summary>
    /// Print Warning message.
    /// </summary>
    public static void Warning(object message, object context = null)
    {
      LogWarning.Raise(message.ToString(), context);
    }

    /// <summary>
    /// Print Error message.
    /// </summary>
    public static void Error(object message, object context = null)
    {
      LogError.Raise(message.ToString(), context);
    }

    /// <summary>
    /// Print Exception.
    /// </summary>
    public static void Exception(Exception e)
    {
      LogException.Raise(e);
    }
  }
}