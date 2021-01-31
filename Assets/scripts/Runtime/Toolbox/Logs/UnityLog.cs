using UnityEngine;

namespace Pon
{
  /// <summary>
  /// Unity implementation of Log.
  /// </summary>
  public static class UnityLog
  {
    #region Members

#if UNITY_STANDALONE || UNITY_EDITOR
    public static bool isEnabled = true;
#else
    public static bool isEnabled = false;
#endif

    #endregion

    #region Constructors

    static UnityLog()
    {
      if (isEnabled == false) return;

      Log.LogDebug += (m, s) => Debug.Log(Prepare("DEBUG", m, DebugColor), CastToUnityObject(s));
      Log.LogInfo += (m, s) => Debug.Log(Prepare("INFO", m, InfoColor), CastToUnityObject(s));
      Log.LogWarning += (m, s) => Debug.LogWarning(Prepare("WARN", m, WarningColor), CastToUnityObject(s));
      Log.LogError += (m, s) => Debug.LogError(Prepare("ERROR", m, ErrorColor), CastToUnityObject(s));
      Log.LogException += Debug.LogException;
    }

    public static void Init()
    {
    }

    #endregion

    #region Methods

    private static string Prepare(string type, string message, string color)
    {
      message = Color(message, color);
      message = Format(type, message);

      return message;
    }

    private static string Format(string type, string message)
    {
      return string.Format("{0}|{1}", type, message);
    }

    private static string Color(string message, string color)
    {
#if UNITY_EDITOR
      return string.Format("<color='{0}'>{1}</color>", color, message);
#else
      return message;
#endif
    }

    private static Object CastToUnityObject(object o)
    {
      try
      {
        return (Object) o;
      }
      catch (System.Exception e)
      {
        Log.Error("Cast to `UnityEngine.Object` in `Log` failed!");
        Log.Exception(e);
        return null;
      }
    }

    #endregion

    #region Properties

    public static string DebugColor
    {
      get { return "teal"; }
    }

    public static string InfoColor
    {
      get { return (Application.HasProLicense() ? "gray" : "black"); }
    }

    public static string WarningColor
    {
      get { return "orange"; }
    }

    public static string ErrorColor
    {
      get { return "red"; }
    }

    #endregion
  }
}