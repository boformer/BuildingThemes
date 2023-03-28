using System;
using System.IO;
using ColossalFramework.IO;
using ICities;

namespace BuildingThemes.Data.DistrictStylesPlusImport
{
    public static class DSPSerializer 
    {
        
        // Current data version
        private const uint DataVersion = 0;

        // Unique data ID
        private const string DataId = "DistrictStylesPlusMod";

        // flattened data to save
        private static TransientDistrictStyleContainer[] _transientDistrictStyles;

        public static void LoadData(ISerializableData serializableDataManager)
        {
            try
            {
                // read byte data from save game
                var byteData = serializableDataManager.LoadData(DataId);
                
                // check if anything to read
                if (byteData != null && byteData.Length > 0)
                {
                    using (MemoryStream stream = new MemoryStream(byteData))
                    {
                        _transientDistrictStyles =
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
        }

        internal static TransientDistrictStyleContainer[] GetSavedData()
        {
            return _transientDistrictStyles;
        }

    }
}