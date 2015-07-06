using ColossalFramework;
using ColossalFramework.Plugins;
using ICities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BuildingThemes
{
    public static class Debugger
    {
        private const string logFilePath = "BuildingThemes_log.txt";

        private static bool initialized = false;
        private static Logger logger = null;

        private static bool _enabled = false;
        public static bool Enabled 
        { 
            get 
            {
                return _enabled;
            } 
            set 
            {
                if (value != _enabled) 
                {
                    if (_enabled = value)
                    {
                        Initialize();
                    }
                    else
                    {
                        Deinitialize();
                    }
                }
            } 
        }

        public static void Initialize()
        {
            Deinitialize();
            
            if (Enabled) 
            {
                logger = new Logger(logFilePath);
                initialized = true;
            }
        }

        public static void Deinitialize()
        {
            if (initialized)
            {
                if (logger != null)
                {
                    logger.Dispose();
                    logger = null;
                }
                initialized = false;
            }
        }

        public static void Log(string message)
        {
            if (initialized && Enabled)
            {
                logger.Log(message);
                Debug.Log(message);
            }
        }

        public static void LogFormat(string format, params object[] args) 
        {
            Log(String.Format(format, args));
        }

        public static void LogException(Exception e)
        {
            Log("ERROR: " + e.Message + "\n" + e.StackTrace);
        }

        public static void AppendModList()
        {
            if (initialized && Enabled)
            {
                string message = "Enabled Plugins:\n";
                
                foreach (var pluginInfo in Singleton<PluginManager>.instance.GetPluginsInfo())
                {
                    if (pluginInfo.isEnabled)
                    {
                        IUserMod mod = (IUserMod)pluginInfo.userModInstance;

                        message += String.Format("# {0}\n", mod.Name);
                    }
                }

                logger.Log(message);
            }
        }

        public static void AppendThemeList()
        {
            if (initialized && Enabled)
            {
                string message = "Loaded Themes:";

                foreach (var theme in Singleton<BuildingThemesManager>.instance.GetAllThemes())
                {
                    message += String.Format("# {0}\n", theme.name);
                }

                logger.Log(message);
            }
        }

        public sealed class Logger : IDisposable
        {
            private delegate void WriteMessage(string message);
            private readonly object Locker = new object();
            private readonly StreamWriter Writer;
            private bool Disposed;

            public Logger(string logFileName)
            {
                Writer = new StreamWriter(logFileName, true);
            }

            ~Logger()
            {
                Dispose(false);
            }

            public void Log(string message)
            {
                WriteMessage action = this.MessageWriter;
                action.BeginInvoke(message, MessageWriteComplete, action);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void MessageWriteComplete(IAsyncResult iar)
            {
                ((WriteMessage)iar.AsyncState).EndInvoke(iar);
            }

            private void Dispose(bool disposing)
            {
                lock (Locker)
                {
                    if (Disposed)
                    {
                        return;
                    }

                    if (disposing)
                    {
                        if (Writer != null)
                        {
                            Writer.Dispose();
                        }
                    }

                    Disposed = true;
                }
            }

            private void MessageWriter(string message)
            {
                lock (Locker)
                {
                    if (!Disposed && (Writer != null))
                    {
                        Writer.WriteLine(message);
                    }
                }
            }
        }
    }
}
