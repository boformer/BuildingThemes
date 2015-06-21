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
    public class SerializableDataExtension : ISerializableDataExtension
    {
        public static string DataId = "BuildingThemes"; //TODO(earalov): maybe support versioning?
        public static UInt32 UniqueId;

        public static ISerializableData SerializableData;

        private static Timer _timer;

        public void OnCreated(ISerializableData serializableData)
        {
            UniqueId = 0u;
            SerializableData = serializableData;
        }

        public void OnReleased()
        {
        }

        public static void GenerateUniqueId()
        {
            UniqueId = (uint)UnityEngine.Random.Range(1000000f, 2000000f);

            while (File.Exists(BuildSaveFilePath()))
            {
                UniqueId = (uint)UnityEngine.Random.Range(1000000f, 2000000f);
            }
        }

        private static string BuildSaveFilePath()
        {
            return Path.Combine(Application.dataPath, String.Format("buildingThemesSave_{0}.xml", UniqueId));
        }

        public void OnLoadData()
        {
            byte[] data = SerializableData.LoadData(DataId);

            if (data == null)
            {
                GenerateUniqueId();
            }
            else
            {
                _timer = new Timer(2000);
                // Hook up the Elapsed event for the timer. 
                _timer.Elapsed += OnLoadDataTimed;
                _timer.Enabled = true;
            }
        }

        private static void OnLoadDataTimed(System.Object source, ElapsedEventArgs e)
        {
            byte[] data = SerializableData.LoadData(DataId);

            UniqueId = 0u;

            for (var i = 0; i < data.Length - 3; i++)
            {
                UniqueId = BitConverter.ToUInt32(data, i);
            }

            var filepath = BuildSaveFilePath();
            _timer.Enabled = false;

            if (!File.Exists(filepath))
            {

                return;
            }

            var configuration = DistrictsConfiguration.Deserialize(filepath);
            var buildingThemesManager = Singleton<BuildingThemesManager>.instance;
            foreach (var district in configuration.Districts)
            {
                foreach (var themeName in district.themes)
                {
                    var theme = buildingThemesManager.GetThemeNyName(themeName);
                    if (theme == null)
                    {
                        continue;
                    }
                    buildingThemesManager.EnableTheme(district.id, theme, false, false);
                }
                buildingThemesManager.MergeDistrictThemes(district.id);
            }
        }

        public void OnSaveData()
        {

            var data = new FastList<byte>();

            GenerateUniqueId();

            var uniqueIdBytes = BitConverter.GetBytes(UniqueId);
            foreach (var uniqueIdByte in uniqueIdBytes)
            {
                data.Add(uniqueIdByte);
            }

            var dataToSave = data.ToArray();
            SerializableData.SaveData(DataId, dataToSave);

            var filepath = BuildSaveFilePath();

            var configuration = new DistrictsConfiguration();

            var themesManager = Singleton<BuildingThemesManager>.instance;
            for (uint i = 0; i < 128; i++)
            {
                var themes = themesManager.GetDistrictThemes(i, false, false);
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
                    themes = themesNames
                });

            }

            DistrictsConfiguration.Serialize(filepath, configuration);
        }
    }

    public class DistrictsConfiguration
    {

        public class District
        {
            public uint id;
            public string[] themes;
        }

        public List<District> Districts = new List<District>();

        public void OnPreSerialize()
        {
        }

        public void OnPostDeserialize()
        {
        }

        public static void Serialize(string filename, DistrictsConfiguration config)
        {
            var serializer = new XmlSerializer(typeof(DistrictsConfiguration));

            using (var writer = new StreamWriter(filename))
            {
                config.OnPreSerialize();
                serializer.Serialize(writer, config);
            }
        }

        public static DistrictsConfiguration Deserialize(string filename)
        {
            var serializer = new XmlSerializer(typeof(DistrictsConfiguration));

            try
            {
                using (var reader = new StreamReader(filename))
                {
                    var config = (DistrictsConfiguration)serializer.Deserialize(reader);
                    config.OnPostDeserialize();
                    return config;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
