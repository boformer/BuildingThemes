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

        private static Dictionary<ulong, ushort> seedTable = new Dictionary<ulong, ushort>();
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
                InitTable();

                deployed = true;

                Debug.Log("Building Themes: BuildingManager Methods detoured!");
            }
        }

        private static void InitTable()
        {
            seedTable.Clear();
            for (ushort _seed = 0; _seed <= 65534; ++_seed)
            {
                var seed = (ulong)(6364136223846793005L * (long)_seed + 1442695040888963407L);
                seedTable.Add(seed, _seed);
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
                seedTable.Clear();

                deployed = false;

                Debug.Log("Building Themes: BuildingManager Methods restored!");
            }
        }


        // Detour

        public BuildingInfo GetRandomBuildingInfo(ref Randomizer r, ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level, int width, int length, BuildingInfo.ZoningMode zoningMode)
        {

            var isRandomizerSimulatorManagers = r.seed == Singleton<SimulationManager>.instance.m_randomizer.seed; //I do it here in case if randomizer methodos mell be called later
            var randimizerSeed = r.seed; //if they are called seed will change

            if (BuildingThemesMod.isDebug)
            {
                UnityEngine.Debug.LogFormat("Building Themes: Detoured GetRandomBuildingInfo was called. seed: {0} (singleton seed: {1}). service: {2}, subService: {3}," +
                    "level: {4}, width: {5}, length: {6}, zoningMode: {7}, current thread: {8}\nStack trace: {9}", r.seed, Singleton<SimulationManager>.instance.m_randomizer.seed,
                    service, subService, level, width, length, zoningMode,
                                            Thread.CurrentThread.ManagedThreadId, System.Environment.StackTrace);
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

            if (isRandomizerSimulatorManagers)
            {
                if (BuildingThemesMod.isDebug)
                {
                    UnityEngine.Debug.LogFormat(
                        "Building Themes: Getting position from static variable. position: {0}, current thread: {1}",
                        position, Thread.CurrentThread.ManagedThreadId);
                }
                FilterList(position, ref fastList);
            }
            else
            {
                if (BuildingThemesMod.isDebug)
                {
                    UnityEngine.Debug.LogFormat(
                        "Building Themes: Getting position from seed {0}... current thread: {1}", randimizerSeed,
                        Thread.CurrentThread.ManagedThreadId);
                }
                var buildingId = seedTable[randimizerSeed];
                var building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingId];
                var buildingPosition = building.m_position;
                if (BuildingThemesMod.isDebug)
                {
                    UnityEngine.Debug.LogFormat(
                        "Building Themes: Getting position from seed {0}. building: {1}, buildingId: {2}, position: {3}, threadId: {4}",
                        randimizerSeed, building.Info.name, buildingId, buildingPosition,
                        Thread.CurrentThread.ManagedThreadId);
                }
                FilterList(buildingPosition, ref fastList);
            }
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

            var newList = new FastList<ushort>();
            for (var i = 0; i < list.m_size; i++)
            {
                var name = PrefabCollection<BuildingInfo>.GetPrefab(list.m_buffer[i]).name;
                if (FilteringStrategy.DoesBuildingBelongToDistrict(name, districtIdx))
                {
                    newList.Add(list.m_buffer[i]);
                }
            }
            list = newList;
        }
    }
}
