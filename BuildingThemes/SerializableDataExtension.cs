using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using System.Xml.Serialization;
using ColossalFramework;
using ICities;
using UnityEngine;

namespace BuildingThemes
{
    // This extension handles the loading and saving of district theme data (which themes are assigned to a district).
    public class SerializableDataExtension : ISerializableDataExtension
    {
        public static ISerializableData SerializableData;
        
        public static string XMLSaveDataId = "BuildingThemes-SaveData";

        // support for legacy data
        public static string LegacyDataId = "BuildingThemes";

        public void OnCreated(ISerializableData serializableData)
        {
            SerializableData = serializableData;
        }

        public void OnReleased()
        {
        }

        public void OnLoadData()
        {
            try 
            {
                byte[] saveData = SerializableData.LoadData(XMLSaveDataId);

                if (saveData != null)
                {
                    if (Debugger.Enabled)
                    {
                        Debugger.Log("Building Themes: Loading Save Data...");
                    }
                    
                    DistrictsConfiguration configuration = null;
                    
                    var xmlSerializer = new XmlSerializer(typeof(DistrictsConfiguration));
                    using (var memoryStream = new MemoryStream(saveData))
                    {
                        configuration = xmlSerializer.Deserialize(new MemoryStream(saveData)) as DistrictsConfiguration;
                    }

                    ApplyConfiguration(configuration);
                }
                else
                {
                    // search for legacy save data
                    byte[] legacyData = SerializableData.LoadData(LegacyDataId);

                    if (legacyData != null)
                    {
                        if (Debugger.Enabled)
                        {
                            Debugger.Log("Building Themes: Loading Legacy Save Data...");
                        }

                        var UniqueId = 0u;

                        for (var i = 0; i < legacyData.Length - 3; i++)
                        {
                            UniqueId = BitConverter.ToUInt32(legacyData, i);
                        }

                        Debug.Log(UniqueId);

                        var filepath = Path.Combine(Application.dataPath, String.Format("buildingThemesSave_{0}.xml", UniqueId));

                        Debug.Log(filepath);

                        if (!File.Exists(filepath))
                        {
                            if (Debugger.Enabled)
                            {
                                Debugger.Log(filepath + " not found!");
                            }
                            return;
                        }

                        DistrictsConfiguration configuration;

                        var serializer = new XmlSerializer(typeof(DistrictsConfiguration));
                        try
                        {
                            using (var reader = new StreamReader(filepath))
                            {
                                configuration = (DistrictsConfiguration)serializer.Deserialize(reader);
                            }
                        }
                        catch
                        {
                            configuration = null;
                        }

                        ApplyConfiguration(configuration);
                    }
                    else
                    { 
                        if (Debugger.Enabled)
                        {
                            Debugger.Log("No legacy save data found!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debugger.LogError("Building Themes: Error loading theme data");
                Debugger.LogException(ex);
            }
        }

        public void OnSaveData()
        {
            if (Debugger.Enabled)
            {
                Debugger.Log("Building Themes: Saving Data...");
            }
            
            try
            {
                var configuration = new DistrictsConfiguration();

                var themesManager = Singleton<BuildingThemesManager>.instance;
                for (byte i = 0; i < 128; i++)
                {
                    if (!themesManager.IsThemeManagementEnabled(i)) continue;

                    var themes = themesManager.GetDistrictThemes(i, false);
                    if (themes == null)
                    {
                        continue; ;
                    }
                    var themesNames = new string[themes.Count];
                    var j = 0;
                    foreach (var theme in themes)
                    {
                        themesNames[j] = theme.name;
                        j++;
                    }
                    configuration.Districts.Add(new DistrictsConfiguration.District()
                    {
                        id = i,
                        blacklistMode = themesManager.IsBlacklistModeEnabled(i),
                        themes = themesNames
                    });
                    if (Debugger.Enabled)
                    {
                        Debugger.LogFormat("Building Themes: Saving: {0} themes enabled for district {1}", themes.Count, i);
                    }
                }

                byte[] configurationData;

                var xmlSerializer = new XmlSerializer(typeof(DistrictsConfiguration));
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                using (var memoryStream = new MemoryStream())
                {
                    xmlSerializer.Serialize(memoryStream, configuration, ns);
                    configurationData = memoryStream.ToArray();
                }
                SerializableData.SaveData(XMLSaveDataId, configurationData);

                // output for debugging
                /*
                using (System.IO.StreamWriter streamWriter = new System.IO.StreamWriter("BuildingThemesData.xml"))
                {
                    xmlSerializer.Serialize(streamWriter, configuration, ns);
                }
                */

                if (Debugger.Enabled)
                {
                    Debugger.LogFormat("Building Themes: Serialization done.");
                    Debugger.AppendThemeList();
                }
            }

            catch (Exception ex)
            {
                Debugger.LogError("Building Themes: Error saving theme data");
                Debugger.LogException(ex);
            }
        }

        private static void ApplyConfiguration(DistrictsConfiguration configuration) 
        {
            var buildingThemesManager = BuildingThemesManager.instance;
            buildingThemesManager.ImportThemes();

            foreach (var district in configuration.Districts)
            {
                //skip districts which do not exist
                if (DistrictManager.instance.m_districts.m_buffer[district.id].m_flags == District.Flags.None) continue;

                var themes = new HashSet<Configuration.Theme>();

                foreach (var themeName in district.themes)
                {
                    var theme = buildingThemesManager.GetThemeByName(themeName);
                    if (theme == null)
                    {
                        Debugger.LogFormat("Theme {0} that was enabled in district {1} could not be found!", themeName, district.id);
                        continue;
                    }
                    themes.Add(theme);
                }

                if (Debugger.Enabled)
                {
                    Debugger.LogFormat("Building Themes: Loading: {0} themes enabled for district {1}", themes.Count, district.id);
                }

                buildingThemesManager.setThemeInfo(district.id, themes, district.blacklistMode);
            }
        }
    }

    public class DistrictsConfiguration
    {

        public class District
        {
            public byte id;
            public bool blacklistMode = false;
            public string[] themes;
        }

        public List<District> Districts = new List<District>();
    }
}
