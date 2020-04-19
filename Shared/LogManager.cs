using System;
using System.Diagnostics;
using System.IO;

namespace RDRN_Shared
{
    public static class LogManager
    {
        internal static string LogDirectory;

        private static object errorLogLock = new object();

        public static void Init(string path)
        {
            LogDirectory = path;
        }

        private static void CreateLogDirectory()
        {
            if (!Directory.Exists(LogDirectory))
                Directory.CreateDirectory(LogDirectory);
        }

        public static void SimpleLog(string filename, string text)
        {
            if (string.IsNullOrEmpty(LogDirectory))
                throw new Exception("LogManager not Initialized!");

            CreateLogDirectory();
            try
            {
                Console.WriteLine("SimpleLog: " + text);
                lock (errorLogLock)
                    File.AppendAllText(LogDirectory + "\\" + filename + ".log", "[" + DateTime.Now.ToString("hh:mm:ss") + "] " + text + "\r\n");
            }
            catch { }
        }
        public static void CefLog(string text)
        {
            if (string.IsNullOrEmpty(LogDirectory))
                throw new Exception("LogManager not Initialized!");

            CreateLogDirectory();
            try
            {
                Console.WriteLine("CefLog: " + text);
                lock (errorLogLock)
                    File.AppendAllText(LogDirectory + "\\" + "CEF.log", "[" + DateTime.Now.ToString("hh:mm:ss") + "] " + text + "\r\n");
            }
            catch { }
        }

        public static void CefLog(Exception ex, string source)
        {
            if (string.IsNullOrEmpty(LogDirectory))
                throw new Exception("LogManager not Initialized!");

            CreateLogDirectory();
            lock (errorLogLock)
            {
                Console.WriteLine("CefLog: " + ex.ToString());
                File.AppendAllText(LogDirectory + "\\CEF.log", ">> EXCEPTION OCCURED AT " + DateTime.Now + " FROM " + source + "\r\n" + ex.ToString() + "\r\n\r\n");
            }
        }

        public static void DebugLog(int text) => DebugLog(text.ToString());
        public static void DebugLog(string text)
        {
            if (string.IsNullOrEmpty(LogDirectory))
                throw new Exception("LogManager not Initialized!");

            Console.WriteLine("DebugLog: " + text);
            CreateLogDirectory();
            lock (errorLogLock)
            {
                File.AppendAllText(LogDirectory + "\\Debug.log", "[" + DateTime.Now.ToString("hh:mm:ss") + "] " + text + Environment.NewLine);
            }
        }

        public static void RuntimeLog(string text)
        {
            if (string.IsNullOrEmpty(LogDirectory))
                throw new Exception("LogManager not Initialized!");

            try
            {
                Debug.WriteLine(text);
                CreateLogDirectory();
                lock (errorLogLock)
                {
                    File.AppendAllText(LogDirectory + "\\Runtime.log", "[" + DateTime.Now.ToString("hh:mm:ss") + "] " + text + "\r\n");
                }
            }
            catch (Exception) { }
        }

        public static void LogException(Exception ex, string source)
        {
            if (string.IsNullOrEmpty(LogDirectory))
                throw new Exception("LogManager not Initialized!");

            CreateLogDirectory();
            Console.WriteLine("LogException: " + ex.ToString());
            lock (errorLogLock)
            {
                File.AppendAllText(LogDirectory + "\\Error.log", ">> EXCEPTION OCCURED AT " + DateTime.Now + " FROM " + source + "\r\n" + ex.ToString() + "\r\n\r\n");
            }
        }

        public static void LogException(string error, string source)
        {
            if (string.IsNullOrEmpty(LogDirectory))
                throw new Exception("LogManager not Initialized!");

            CreateLogDirectory();
            Console.WriteLine("LogException: " + error + " " + source);
            lock (errorLogLock)
            {
                File.AppendAllText(LogDirectory + "\\Error.log", ">> EXCEPTION OCCURED AT " + DateTime.Now + " FROM " + source + "\r\n" + error + "\r\n\r\n");
            }
        }
    }
}