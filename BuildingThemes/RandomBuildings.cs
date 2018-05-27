using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

namespace BuildingThemes
{
    public static class RandomBuildings
    {

        // called before a new building spawns on empty land (ZoneBlock.SimulationStep)
        public static BuildingInfo GetRandomBuildingInfo_Spawn(Vector3 position, ref Randomizer r, ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level, int width, int length, BuildingInfo.ZoningMode zoningMode, int style)
        {
            if (Debugger.Enabled)
            {
                Debugger.Log("GetRandomBuildingInfo_Spawn called!");
            }

            var areaIndex = BuildingThemesManager.GetAreaIndex(service, subService, level, width, length, zoningMode);

            var districtId = Singleton<DistrictManager>.instance.GetDistrict(position);
            FastList<ushort> fastList = Singleton<BuildingThemesManager>.instance.GetAreaBuildings(districtId, areaIndex);

            if (fastList == null || fastList.m_size == 0)
            {
                return (BuildingInfo)null;
            }

            // select a random prefab from the list
            int index = r.Int32((uint)fastList.m_size);
            return PrefabCollection<BuildingInfo>.GetPrefab((uint)fastList.m_buffer[index]);
        }

        // Called every frame on building upgrade
        public static BuildingInfo GetRandomBuildingInfo_Upgrade(Vector3 position, ushort prefabIndex, ref Randomizer r, ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level, int width, int length, BuildingInfo.ZoningMode zoningMode, int style)
        {
            // This method is very fragile, no logging here!

            var districtId = Singleton<DistrictManager>.instance.GetDistrict(position);

            // See if there is a special upgraded building
            var buildingInfo = BuildingThemesManager.instance.GetUpgradeBuildingInfo(prefabIndex, districtId);
            if (buildingInfo != null)
            {
                return buildingInfo;
            }

            var areaIndex = BuildingThemesManager.GetAreaIndex(service, subService, level, width, length, zoningMode);

            // list of possible prefabs
            var fastList = Singleton<BuildingThemesManager>.instance.GetAreaBuildings(districtId, areaIndex);

            if (fastList == null || fastList.m_size == 0)
            {
                return (BuildingInfo)null;
            }

            // select a random prefab from the list
            int index = r.Int32((uint)fastList.m_size);
            return PrefabCollection<BuildingInfo>.GetPrefab((uint)fastList.m_buffer[index]);
        }
    }
}