using System;
using System.IO;
using System.Xml.Serialization;
using BuildingThemes.Data.DistrictStylesPlusImport;
using ColossalFramework;
using ICities;

namespace BuildingThemes.Data
{
    // This extension handles the loading and saving of district theme data (which themes are assigned to a district).
    public class SerializableDataExtension : SerializableDataExtensionBase
    {
        private const string XMLSaveDataId = "BuildingThemes-SaveData";

        // support for legacy data

        public override void OnLoadData()
        {
            base.OnLoadData();
            try 
            {
                DistrictsConfiguration configuration;
                var saveData = serializableDataManager.LoadData(XMLSaveDataId);

                if (saveData != null)
                {
                    if (Debugger.Enabled)
                    {
                        Debugger.Log("Building Themes: Loading Save Data...");
                    }
                    

                    
                    var xmlSerializer = new XmlSerializer(typeof(DistrictsConfiguration));
                    using (var memoryStream = new MemoryStream(saveData))
                    {
                        configuration = xmlSerializer.Deserialize(memoryStream) as DistrictsConfiguration;
                    }

                    ConfigurationHelper.ApplyConfiguration(configuration);
                }
                else
                {
                    // search for legacy save data
                    if ((configuration = LegacyDataLoader.TryImportLegacyConfiguration(serializableDataManager)) != null)
                    {
                        ConfigurationHelper.ApplyConfiguration(configuration);
                    }
                    else
                    { 
                        if (Debugger.Enabled)
                        {
                            Debugger.Log("No legacy save data found!");
                        }

                        //search for District Styles Plus save data
                        if ((configuration = DSPDataLoader.TryImportDSPConfiguration(serializableDataManager)) != null)
                        {
                            ConfigurationHelper.ApplyConfiguration(configuration);
                        }
                        else
                        {
                            if (Debugger.Enabled)
                            {
                                Debugger.Log("No DSP save data found!");
                            }
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

        public override void OnSaveData()
        {
            base.OnSaveData();
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
                var namespaces = new XmlSerializerNamespaces();
                namespaces.Add("", "");
                using (var memoryStream = new MemoryStream())
                {
                    xmlSerializer.Serialize(memoryStream, configuration, namespaces);
                    configurationData = memoryStream.ToArray();
                }
                serializableDataManager.SaveData(XMLSaveDataId, configurationData);

                // output for debugging
                /*
                using (System.IO.StreamWriter streamWriter = new System.IO.StreamWriter("BuildingThemesData.xml"))
                {
                    xmlSerializer.Serialize(streamWriter, configuration, ns);
                }
                */

                if (!Debugger.Enabled)
                {
                    return;
                }
                Debugger.LogFormat("Building Themes: Serialization done.");
                Debugger.AppendThemeList();
            }

            catch (Exception ex)
            {
                Debugger.LogError("Building Themes: Error saving theme data");
                Debugger.LogException(ex);
            }
        }
    }
}
