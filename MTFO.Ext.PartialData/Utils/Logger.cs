using BepInEx.Logging;

namespace MTFO.Ext.PartialData.Utils
{
    internal static class Logger
    {
        public static ManualLogSource LogInstance;
        public static bool UsingLog = false;

        public static void Log(string format, params object[] args) => Log(string.Format(format, args));

        public static void Log(string str)
        {
            if (UsingLog)
                LogInstance?.Log(LogLevel.Message, str);
        }

        public static void Warning(string format, params object[] args) => Warning(string.Format(format, args));

        public static void Warning(string str)
        {
            LogInstance?.Log(LogLevel.Warning, str);
        }

        public static void Error(string format, params object[] args) => Error(string.Format(format, args));

        public static void Error(string str)
        {
            LogInstance?.Log(LogLevel.Error, str);
        }

        public static void Debug(string format, params object[] args) => Debug(string.Format(format, args));

        public static void Debug(string str)
        {
            LogInstance?.Log(LogLevel.Debug, str);
        }
    }
}