using Godot;
using System;
using System.IO;
using Environment = System.Environment;

namespace OpenARPG
{
    public static class Logger
    {
        public static Action<string> Log { get; private set; } = (message) => { GD.Print(message); };
        public static Action<string> Warning { get; private set; } = (message) => { GD.PushWarning(message); };
        public static Action<string> Error { get; private set; } = (message) => { GD.PushError(message); };

        public static string TimeStamp()
            => DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'");

        public static Action Initialize { get; private set; } = () => 
        {
            Initialize = () => { Error("Logger: Initialize: Logger already initialized"); };

            if (OS.HasFeature("release"))
            {
                Log = (_) => {};
                Warning = (_) => {};
                Error = (_) => {};

                /*string path = $"{ProjectSettings.GlobalizePath("user://")}/Log.txt";
                File.Delete(path);
                Log = (message) => { try { File.AppendAllText(path, $"{TimeStamp()} {message}{Environment.NewLine}"); } catch { } };
                Warning = (message) => { try { File.AppendAllText(path, $"{TimeStamp()} [WARNING] {message}{Environment.NewLine}"); } catch { } };
                Error = (message) => { try { File.AppendAllText(path, $"{TimeStamp()} [ERROR] {message}{Environment.NewLine}"); } catch { } };*/
            }

            Log("Logger: Initialize: Logger initialized");
        };
    }
}