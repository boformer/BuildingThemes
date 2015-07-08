using ColossalFramework;
using ColossalFramework.Math;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace BuildingThemes.Detour
{
    public class BuildingManagerDetour
    {
        private static bool deployed = false;

        private static RedirectCallsState _BuildingManager_GetRandomBuildingInfo_state;
        private static MethodInfo _BuildingManager_GetRandomBuildingInfo_original;
        private static MethodInfo _BuildingManager_GetRandomBuildingInfo_detour;

        private static MethodInfo _RefreshAreaBuidlings;
        private static MethodInfo _GetAreaIndex;

        private static FieldInfo _m_areaBuildings;

        // we'll use this variable to pass the building position to GetRandomBuildingInfo method
        public static Vector3 position;

        public static bool upgrade = false;
        public static ushort infoIndex;

        public static void Deploy()
        {
            if (!deployed)
            {
                _BuildingManager_GetRandomBuildingInfo_original = typeof(BuildingManager).GetMethod("GetRandomBuildingInfo", BindingFlags.Instance | BindingFlags.Public);
                _BuildingManager_GetRandomBuildingInfo_detour = typeof(BuildingManagerDetour).GetMethod("GetRandomBuildingInfo", BindingFlags.Instance | BindingFlags.Public);
                _BuildingManager_GetRandomBuildingInfo_state = RedirectionHelper.RedirectCalls(_BuildingManager_GetRandomBuildingInfo_original, _BuildingManager_GetRandomBuildingInfo_detour);

                _RefreshAreaBuidlings = typeof(BuildingManager).GetMethod("RefreshAreaBuildings", BindingFlags.NonPublic | BindingFlags.Instance);
                _GetAreaIndex = typeof(BuildingManager).GetMethod("GetAreaIndex", BindingFlags.NonPublic | BindingFlags.Static);
                _m_areaBuildings = typeof(BuildingManager).GetField("m_areaBuildings", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                deployed = true;

                Debugger.Log("Building Themes: BuildingManager Methods detoured!");
            }
        }

        public static void Revert()
        {
            if (deployed)
            {
                RedirectionHelper.RevertRedirect(_BuildingManager_GetRandomBuildingInfo_original, _BuildingManager_GetRandomBuildingInfo_state);
                _BuildingManager_GetRandomBuildingInfo_original = null;
                _BuildingManager_GetRandomBuildingInfo_detour = null;

                _RefreshAreaBuidlings = null;
                _GetAreaIndex = null;
                _m_areaBuildings = null;

                deployed = false;

                Debugger.Log("Building Themes: BuildingManager Methods restored!");
            }
        }


        // Detour

        public BuildingInfo GetRandomBuildingInfo(ref Randomizer r, ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level, int width, int length, BuildingInfo.ZoningMode zoningMode)
        {
            //this part is the same as in original method
            var buildingManager = Singleton<BuildingManager>.instance;

            var areaIndex = GetAreaIndex(service, subService, level, width, length, zoningMode);
            var districtId = (int)Singleton<DistrictManager>.instance.GetDistrict(position);

            var fastList = Singleton<BuildingThemesManager>.instance.GetAreaBuildings(districtId, areaIndex);

            if (fastList == null)
            {
                return (BuildingInfo)null;
            }

            if (fastList.m_size == 0)
            {
                return (BuildingInfo)null;
            }

            int index = r.Int32((uint)fastList.m_size);
            var buildingInfo = PrefabCollection<BuildingInfo>.GetPrefab((uint)fastList.m_buffer[index]);
            return buildingInfo;
        }

        private static int GetAreaIndex(ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level, int width, int length, BuildingInfo.ZoningMode zoningMode)
        {
            int num;
            if (subService != ItemClass.SubService.None)
            {
                num = 8 + subService - ItemClass.SubService.ResidentialLow;
            }
            else
            {
                num = service - ItemClass.Service.Residential;
            }
            num = (int)(num * 5 + level);
            if (zoningMode == BuildingInfo.ZoningMode.CornerRight)
            {
                num = num * 4 + length - 1;
                num = num * 4 + width - 1;
                num = num * 2 + 1;
            }
            else
            {
                num = num * 4 + width - 1;
                num = num * 4 + length - 1;
                num = (int)(num * 2 + zoningMode);
            }
            return num;
        }


    }
}
