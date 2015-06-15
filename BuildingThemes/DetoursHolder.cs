using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Reflection;
using System.Threading;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

namespace BuildingThemes
{

    class DetoursHolder
    {

        private static Dictionary<ulong, ushort> seedTable = new Dictionary<ulong, ushort>();

        public static void InitTable()
        {

            for (ushort _seed = 0; _seed <= 65534; ++_seed)
            {
                var seed = (ulong)(6364136223846793005L * (long)_seed + 1442695040888963407L);
                seedTable.Add(seed, _seed);
            }
        }

        //we'll use this variable to pass position to GetRandomBuildingInfo method. Or we can just pass District
        public static Vector3 position;

        public static RedirectCallsState zoneBlockSimulationStepState;
        public static MethodInfo zoneBlockSimulationStep;
        public static IntPtr zoneBlockSimulationStepPtr;
        public static IntPtr zoneBlockSimulationStepDetourPtr;
        
        /*
        private static MethodInfo refreshAreaBuidlings;
        private static MethodInfo getAreaIndex;
        */

        public static RedirectCallsState resourceManagerAddResourceState;
        public static MethodInfo resourceManagerAddResource;
        public static IntPtr resourceManagerAddResourcePtr;
        public static IntPtr resourceManagerAddResourceDetourPtr;

        //this is detoured version of BuildingManger#GetRandomBuildingInfo method. Note, that it's an instance method. It's better because this way all registers will be expected to have the same values
        //as in original methods
        public BuildingInfo GetRandomBuildingInfo(ref Randomizer r, ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level, int width, int length, BuildingInfo.ZoningMode zoningMode)
        {

            var isRandomizerSimulatorManagers = r.seed == Singleton<SimulationManager>.instance.m_randomizer.seed; //I do it here in case if randomizer methodos mell be called later
            var randomizerSeed = r.seed; //if they are called seed will change
            if (BuildingThemesMod.isDebug)
            {
                UnityEngine.Debug.LogFormat("Building Themes: Detoured GetRandomBuildingInfo was called. seed: {0} (singleton seed: {1}). service: {2}, subService: {3}," +
                    "level: {4}, width: {5}, length: {6}, zoningMode: {7}, current thread: {8}\nStack trace: {9}", r.seed, Singleton<SimulationManager>.instance.m_randomizer.seed,
                    service, subService, level, width, length, zoningMode,
                                            Thread.CurrentThread.ManagedThreadId, System.Environment.StackTrace);
            }

            //this part is the same as in original method
            /*
            var buildingManager = Singleton<BuildingManager>.instance;
            if (refreshAreaBuidlings == null)
            {
                refreshAreaBuidlings = typeof(BuildingManager).GetMethod("RefreshAreaBuildings", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            refreshAreaBuidlings.Invoke(buildingManager, new object[] { });
            */

            // get the position of the [zone block] or [existing building]
            Vector3 thePosition;
            
            if (isRandomizerSimulatorManagers)
            {
                if (BuildingThemesMod.isDebug)
                {
                    UnityEngine.Debug.LogFormat(
                        "Building Themes: Getting position from static variable. position: {0}, current thread: {1}",
                        position, Thread.CurrentThread.ManagedThreadId);
                }
                thePosition = position;
            }
            else
            {
                var buildingId = seedTable[randomizerSeed];
                var building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingId];
                thePosition = building.m_position;
                if (BuildingThemesMod.isDebug)
                {
                    UnityEngine.Debug.LogFormat(
                        "Building Themes: Getting position from seed {0}. building: {1}, buildingId: {2}, position: {3}, threadId: {4}",
                        randomizerSeed, building.Info.name, buildingId, thePosition,
                        Thread.CurrentThread.ManagedThreadId);
                }
            }

            // get the district id
            ushort districtIdx = Singleton<DistrictManager>.instance.GetDistrict(position);

            //get the area index (based on width, length, level, service, etc.)
            int areaIndex = BuildingThemesMod.GetAreaIndex(service, subService, level, width, length, zoningMode);

            // get the fastlist for the [districtIdx, areaIndex]
            FastList<ushort> fastList = BuildingThemesMod.m_district_areaBuildings[districtIdx, areaIndex];

            UnityEngine.Debug.LogFormat("Fastlist contains " + fastList.m_size + "elements (service: {0}, subService: {1}," +
                    "level: {2}, width: {3}, length: {4}, zoningMode: {5}) for district " + Singleton<DistrictManager>.instance.GetDistrictName(districtIdx),
                    service, subService, level, width, length, zoningMode);


            if (fastList == null || fastList.m_size == 0)
            {
                if (BuildingThemesMod.isDebug)
                {
                    UnityEngine.Debug.LogFormat(
                        "Building Themes: Filtered list is empty. Return null, current thread: {0}",
                        Thread.CurrentThread.ManagedThreadId);
                }
                return (BuildingInfo)null;
            }

            // select a random prefab from the list
            int index = r.Int32((uint)fastList.m_size);
            var buildingInfo = PrefabCollection<BuildingInfo>.GetPrefab((uint)fastList.m_buffer[index]);
            return buildingInfo;
        }

        /*
        private static void FilterList(Vector3 position, ref FastList<ushort> list)
        {
            ushort districtIdx = Singleton<DistrictManager>.instance.GetDistrict(position);
            //districtIdx==0 probably means 'outside of any district'
            if (BuildingThemesMod.isDebug)
            {
                UnityEngine.Debug.LogFormat(
                    "Building Themes: Detoured GetRandomBuildingInfo. districtIdx: {0};current thread: {1}",
                    districtIdx, Thread.CurrentThread.ManagedThreadId);
            }
            //District district = Singleton<DistrictManager>.instance.m_districts.m_buffer[districtIdx];
            //TODO(earalov): here fastList variable should be filtered. All buildings that  don't belong to the district should be removed from this list.

            //this is stub implementation
            FastList<ushort> newList = new FastList<ushort>();
            for (int i = 0; i < list.m_size; i++)
            {
                var name = PrefabCollection<BuildingInfo>.GetPrefab(list.m_buffer[i]).name;
                var isEuropean = (name.Contains("lock") || name.Contains("EU"));
                if (isEuropean && districtIdx == 0)
                {
                    continue;
                }
                if (!isEuropean && districtIdx != 0)
                {
                    continue;
                }
                newList.Add(list.m_buffer[i]);
            }
            list = newList;
        }
        */

        public void ZoneBlockSimulationStep(ushort blockID)
        {
            var zoneBlock = Singleton<ZoneManager>.instance.m_blocks.m_buffer[blockID];
            if (BuildingThemesMod.isDebug)
            {
                UnityEngine.Debug.LogFormat(
                    "Building Themes: Detoured ZoneBlock.SimulationStep was called. blockID: {0}, position: {1}. current thread: {2}",
                    blockID, zoneBlock.m_position, Thread.CurrentThread.ManagedThreadId);
            }
            position = zoneBlock.m_position;
            RedirectionHelper.RevertJumpTo(zoneBlockSimulationStepPtr, zoneBlockSimulationStepState);
            zoneBlockSimulationStep.Invoke(zoneBlock, new object[] { blockID });
            RedirectionHelper.PatchJumpTo(zoneBlockSimulationStepPtr, zoneBlockSimulationStepDetourPtr);

        }

        public int ImmaterialResourceManagerAddResource(ImmaterialResourceManager.Resource resource, int rate, Vector3 positionArg, float radius)
        {
            if (BuildingThemesMod.isDebug)
            {
                UnityEngine.Debug.LogFormat(
                    "Building Themes: Detoured ImmaterialResource.AddResource was called. position: {0}. current thread: {1}",
                    positionArg, Thread.CurrentThread.ManagedThreadId);
            }
            if (resource == ImmaterialResourceManager.Resource.Abandonment)
            {
                position = positionArg;
            }
            RedirectionHelper.RevertJumpTo(resourceManagerAddResourcePtr, resourceManagerAddResourceState);
            var result = Singleton<ImmaterialResourceManager>.instance.AddResource(resource, rate, positionArg, radius);
            RedirectionHelper.PatchJumpTo(resourceManagerAddResourcePtr, resourceManagerAddResourceDetourPtr);
            return result;
        }

        internal static object GetInstanceField(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }
    }
}
