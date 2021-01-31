namespace Pon
{
  public static class OpenFile
  {
    public static void Open(string path)
    {
      if (string.IsNullOrEmpty(path)) return;
#if UNITY_STANDALONE_WIN
      OpenWindows(path);
#elif UNITY_STANDALONE_OSX
        OpenMacOS(path);
#elif UNITY_STANDALONE_LINUX
        OpenLinux(path);
#endif
    }

    private static void OpenWindows(string path)
    {
      var cleanPath = System.IO.Path.GetFullPath(path);

      string args = $"/select,\"{cleanPath}\"";
      var info = new System.Diagnostics.ProcessStartInfo {FileName = "explorer", Arguments = args};
      System.Diagnostics.Process.Start(info);
    }

    private static void OpenMacOS(string path)
    {
      var cleanPath = System.IO.Path.GetFullPath(path);

      string args = $"-n -R \"{cleanPath}\"";
      var info = new System.Diagnostics.ProcessStartInfo {FileName = "open", Arguments = args};
      System.Diagnostics.Process.Start(info);
    }

    private static void OpenLinux(string path)
    {
      var cleanPath = System.IO.Path.GetFullPath(path);

      string args = $"\"{cleanPath}\"";
      var info = new System.Diagnostics.ProcessStartInfo {FileName = "xdg-open", Arguments = args};
      System.Diagnostics.Process.Start(info);
    }
  }
}