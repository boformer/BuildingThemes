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

        private static Filter.IFilteringStrategy FilteringStrategy;


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

                FilteringStrategy = new Filter.DefaultFilteringStrategy();

                deployed = true;

                Debug.Log("Building Themes: BuildingManager Methods detoured!");
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

                FilteringStrategy = null;

                deployed = false;

                Debug.Log("Building Themes: BuildingManager Methods restored!");
            }
        }


        // Detour

        public BuildingInfo GetRandomBuildingInfo(ref Randomizer r, ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level, int width, int length, BuildingInfo.ZoningMode zoningMode)
        {
            if (BuildingThemesMod.isDebug)
            {
                UnityEngine.Debug.LogFormat("Building Themes: Detoured GetRandomBuildingInfo was called\nservice: {0}, subService: {1}," +
                        "level: {2}, width: {3}, length: {4}, zoningMode: {5}", service, subService, level, width, length, zoningMode);
            }

            //this part is the same as in original method
            var buildingManager = Singleton<BuildingManager>.instance;
            _RefreshAreaBuidlings.Invoke(buildingManager, new object[] { });
            var areaBuildings = (FastList<ushort>[])_m_areaBuildings.GetValue(buildingManager);
            FastList<ushort> fastList = areaBuildings[(int)_GetAreaIndex.Invoke(null, new object[] { service, subService, level, width, length, zoningMode })];
            if (fastList == null)
            {
                if (BuildingThemesMod.isDebug)
                {
                    UnityEngine.Debug.LogFormat("Building Themes: Fast list is null. Return null, current thread: {0}",
                        Thread.CurrentThread.ManagedThreadId);
                }
                return (BuildingInfo)null;
            }

            if (fastList.m_size == 0)
            {
                if (BuildingThemesMod.isDebug)
                {
                    UnityEngine.Debug.LogFormat(
                        "Building Themes: Fast list is empty. Return null, current thread: {0}",
                        Thread.CurrentThread.ManagedThreadId);
                }
                return (BuildingInfo)null;
            }

            FilterList(position, ref fastList);

            if (fastList.m_size == 0)
            {
                if (BuildingThemesMod.isDebug)
                {
                    UnityEngine.Debug.LogFormat(
                        "Building Themes: Filtered list is empty. Return null, current thread: {0}",
                        Thread.CurrentThread.ManagedThreadId);
                }
                return (BuildingInfo)null;
            }
            int index = r.Int32((uint)fastList.m_size);
            var buildingInfo = PrefabCollection<BuildingInfo>.GetPrefab((uint)fastList.m_buffer[index]);
            return buildingInfo;
        }

        private static FastList<ushort> filteredList = new FastList<ushort>();

        private static void FilterList(Vector3 position, ref FastList<ushort> list)
        {
            //districtIdx==0 probably means 'outside of any district'
            var districtIdx = Singleton<DistrictManager>.instance.GetDistrict(position);

            if (BuildingThemesMod.isDebug)
            {
                UnityEngine.Debug.LogFormat(
                    "Building Themes: Detoured GetRandomBuildingInfo. districtIdx: {0};current thread: {1}",
                    districtIdx, Thread.CurrentThread.ManagedThreadId);
            }

            filteredList.Clear();

            for (var i = 0; i < list.m_size; i++)
            {
                var name = PrefabCollection<BuildingInfo>.GetPrefab(list.m_buffer[i]).name;
                if (FilteringStrategy.DoesBuildingBelongToDistrict(name, districtIdx))
                {
                    filteredList.Add(list.m_buffer[i]);
                }
            }
            list = filteredList;
        }
    }
}
