using System;
using System.IO;
using System.Xml.Serialization;
using ICities;
using UnityEngine;

namespace BuildingThemes.Data
{
    internal static class LegacyDataLoader
    {
        private const string LegacyDataId = "BuildingThemes";        
        public static DistrictsConfiguration TryImportLegacyConfiguration(ISerializableData serializableDataManager)
        {
            var legacyData = serializableDataManager.LoadData(LegacyDataId);
            if (legacyData == null)
            {
                return null;
            }
            if (Debugger.Enabled)
            {
                Debugger.Log("Building Themes: Loading Legacy Save Data...");
            }

            var uniqueId = 0u;

            for (var i = 0; i < legacyData.Length - 3; i++)
            {
                uniqueId = BitConverter.ToUInt32(legacyData, i);
            }

            Debug.Log(uniqueId);

            var filepath = Path.Combine(Application.dataPath, $"buildingThemesSave_{uniqueId}.xml");

            Debug.Log(filepath);

            if (!File.Exists(filepath))
            {
                if (Debugger.Enabled)
                {
                    Debugger.Log(filepath + " not found!");
                }

                return null;
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

            return configuration;
        }
    }
}