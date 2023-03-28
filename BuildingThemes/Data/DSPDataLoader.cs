using BuildingThemes.Data.DistrictStylesPlusImport;
using ICities;

namespace BuildingThemes.Data
{
    internal static class DSPDataLoader
    {
        public static DistrictsConfiguration TryImportDSPConfiguration(ISerializableData serializableDataManager)
        {
            Debugger.Log("Attempting to load DSP data...");
            DSPSerializer.LoadData(serializableDataManager);
            DSPTransientStyleManager.LoadDataFromSave();

            return null; //TODO
        }
    }
}