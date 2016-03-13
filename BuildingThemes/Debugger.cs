using ColossalFramework;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
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
        private static bool initialized = false;

        private static bool _enabled = false;

        public static bool xmlCorrupt = false;


        private static bool loaded = false;
        private static string exceptions = "";

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
                initialized = true;
            }
        }

        public static void Deinitialize()
        {
            if (initialized)
            {
                initialized = false;
                exceptions = "";
                loaded = false;
            }
        }

        public static void Log(string message)
        {
            if (initialized && Enabled)
            {
                Debug.Log(message);
            }
        }

        public static void LogFormat(string format, params object[] args) 
        {
            Log(String.Format(format, args));
        }

        public static void LogError(string error)
        {
            Debug.LogError(error);
        }

        public static void LogException(Exception e)
        {
            string message = "ERROR: " + e.Message + "\n" + e.StackTrace  + "\n";
            
            Debug.LogException(e);

            exceptions += message; 

            if (loaded) ShowExceptions();
        }

        public static void OnLevelLoaded()
        {
            loaded = true;

            Debugger.AppendModList();
            Debugger.AppendThemeList();
            ShowExceptions();
        }

        public static void OnLevelUnloading()
        {
            loaded = false;
        }

        private static void ShowExceptions()
        {
            string text = null;

            if (xmlCorrupt)
            {
                text = "BuildingThemes is unable to read the BuildingThemes.xml file " +
                       "that stores your settings and themes!\n\n" +
                       "To fix it, delete the corrupted file and restart the game:\n" +
                       "{Steam folder}\\steamapps\\common\\\nCities_Skylines\\BuildingThemes.xml\n\n" +
                       "(Your self-made themes and other settings will be lost. Thank your PC for that)";
            }

            else if (exceptions != "")
            {
                text = "Please report this error on the Building Themes workshop page:\n" + exceptions;
            }

            if (text != null)
            {
                UIView.library?.ShowModal<ExceptionPanel>("ExceptionPanel")?.SetMessage("Building Themes Error",text,true);

                exceptions = "";
            }
        }

        public static void AppendModList()
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

            Debug.Log(message);
        }

        public static void AppendThemeList()
        {
            try
            {
                string message = "Loaded Themes:\n";

                foreach (var theme in Singleton<BuildingThemesManager>.instance.GetAllThemes())
                {
                    message += String.Format("# {0}\n", theme.name);
                }

                Debug.Log(message);
            }
            catch
            {
            }
        }
    }
}
