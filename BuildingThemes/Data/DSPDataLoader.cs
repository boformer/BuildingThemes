using System;
using System.IO;
using BuildingThemes.Data.DistrictStylesPlusImport;
using ColossalFramework.IO;
using ICities;

namespace BuildingThemes.Data
{
    internal static class DSPDataLoader
    {
                
        private const string DataId = "DistrictStylesPlusMod";
        
        public static DistrictsConfiguration TryImportDSPConfiguration(ISerializableData serializableDataManager)
        {

            var data = LoadData(serializableDataManager);
            Debugger.Log("Building Themes: Attempting to load DSP data...");
            
            for (var i = 0; i < data.Length; i++)
            {
                var districtId = (byte) i;

                if (DistrictManager.instance.m_districts.m_buffer[i].m_flags == District.Flags.None)
                {
                    continue;
                }

                var transientDistrictStyle = data[i];
                if (transientDistrictStyle != null && transientDistrictStyle.StyleFullNames.Count > 0) {
                    DSPTransientStyleManager.SetSelectedStylesForDistrict(districtId, transientDistrictStyle.StyleFullNames);
                }
            }
            
            Debugger.Log("Building Themes: DSP data loaded!");

            return null; //TODO
        }

        private static TransientDistrictStyleContainer[] LoadData(ISerializableData serializableDataManager)
        {
            TransientDistrictStyleContainer[] transientDistrictStyles = null;
            try
            {
                // read byte data from save game
                var byteData = serializableDataManager.LoadData(DataId);
                
                // check if anything to read
                if (byteData != null && byteData.Length > 0)
                {
                    using (var stream = new MemoryStream(byteData))
                    {
                        transientDistrictStyles =
                            DataSerializer.DeserializeArray<TransientDistrictStyleContainer>(stream,
                                DataSerializer.Mode.Memory);
                    }
                }
                else
                {
                    UnityEngine.Debug.Log("DSP does not have anything to read from save game.");
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("DistrictStylePlus data failed to be loaded.");
                UnityEngine.Debug.LogException(e);
            }

            return transientDistrictStyles;
        }
    }
}