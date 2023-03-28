using System.Collections.Generic;
using System.Linq;
using ColossalFramework;
using ColossalFramework.IO;

namespace BuildingThemes.Data.DistrictStylesPlusImport
{
    /// <summary>
    /// Serialisation / DeSerialisation of district styles selections to save game file.
    /// </summary>
    public class TransientDistrictStyleContainer : IDataContainer
    {

        public HashSet<string> StyleFullNames;
        
        public void Serialize(DataSerializer s)
        {
            UnityEngine.Debug.Log("Write DistrictStylesPlus data.");
            s.WriteUniqueStringArray(StyleFullNames.ToArray());
        }

        public void Deserialize(DataSerializer s)
        {
            UnityEngine.Debug.Log("Load DistrictStylesPlus data.");
            StyleFullNames = new HashSet<string>(s.ReadUniqueStringArray());
        }

        public void AfterDeserialize(DataSerializer s)
        {
            UnityEngine.Debug.Log("Validate DistrictStylesPlus data.");

            if (!DistrictManager.exists)
            {
                UnityEngine.Debug.LogError("Load from save game problem. District Manager does not exist.");
                return;
            }
            
            UnityEngine.Debug.Log($"Check if all styles exists: {string.Join(", ", StyleFullNames.ToArray())}");

            if (StyleFullNames.Count <= 0) return; // no styles mentioned, nothing to validate
            
            var validatedStylesNames = Singleton<DistrictManager>.instance.m_Styles
                .Where(style => StyleFullNames.Contains(style.FullName))
                .Select(style => style.FullName)
                .ToArray();
                
            StyleFullNames = new HashSet<string>(validatedStylesNames);
        }
    }
}