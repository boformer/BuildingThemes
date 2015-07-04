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

        private static bool justStarted = true;
        private static List<string> log = new List<string>(); 
        
        public static bool Enabled { get; set; }

        public static void Reset()
        {
            lock (log) 
            { 
                log.Clear();
            }

            if (Enabled && justStarted)
            {
                File.Delete(logFilePath);
                justStarted = false;
            }
        }

        public static void Save() 
        {
            if (Enabled)
            {
                if (justStarted)
                {
                    File.Delete(logFilePath);
                    justStarted = false;
                }
                
                lock (log)
                {
                    using (StreamWriter w = File.AppendText(logFilePath))
                    {
                        foreach (var message in log)
                        {
                            w.WriteLine(message);
                        }
                    }
                    log.Clear();
                }
            }
        }

        public static void Log(string message)
        {
            if (Enabled)
            {
                lock (log)
                {
                    log.Add(message);
                }
                Debug.Log(message);
            }
        }

        public static void LogFormat(string format, params object[] args) 
        {
            Log(String.Format(format, args));
        }

        public static void LogException(Exception e)
        {
            Log("ERROR: " + e.Message);
            Log(e.StackTrace);
        }

        public static void AppendModList()
        {
            if (Enabled)
            {
                Log("Enabled Plugins:");
                
                foreach (var pluginInfo in Singleton<PluginManager>.instance.GetPluginsInfo())
                {
                    if (pluginInfo.isEnabled)
                    {
                        IUserMod mod = (IUserMod)pluginInfo.userModInstance;

                        LogFormat("# {0}", mod.Name);
                    }
                }
            }
        }

        internal static void AppendThemeList()
        {
            if (Debugger.Enabled)
            {
                Debugger.Log("Loaded Themes:");
                foreach (var theme in Singleton<BuildingThemesManager>.instance.GetAllThemes())
                {
                    LogFormat("# {0}", theme.name);
                }
            }
        }
    }
}
