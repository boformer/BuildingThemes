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
        public static IFilteringStrategy FilteringStrategy;

        public static void InitTable()
        {
            seedTable.Clear();
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
        
        private static MethodInfo refreshAreaBuidlings;
        private static MethodInfo getAreaIndex;

        public static RedirectCallsState resourceManagerAddResourceState;
        public static MethodInfo resourceManagerAddResource;
        public static IntPtr resourceManagerAddResourcePtr;
        public static IntPtr resourceManagerAddResourceDetourPtr;

        //this is detoured version of BuildingManger#GetRandomBuildingInfo method. Note, that it's an instance method. It's better because this way all registers will be expected to have the same values
        //as in original methods
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
            if (refreshAreaBuidlings == null)
            {
                refreshAreaBuidlings = typeof(BuildingManager).GetMethod("RefreshAreaBuildings", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            refreshAreaBuidlings.Invoke(buildingManager, new object[] { });
            var areaBuildings = (FastList<ushort>[])GetInstanceField(typeof(BuildingManager), buildingManager, "m_areaBuildings");
            if (getAreaIndex == null)
            {
                getAreaIndex = typeof(BuildingManager).GetMethod("GetAreaIndex", BindingFlags.NonPublic | BindingFlags.Static);
            }
            FastList<ushort> fastList = areaBuildings[(int)getAreaIndex.Invoke(null, new object[] { service, subService, level, width, length, zoningMode })];
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

        private object addRessourceLock = new object();

        public int ImmaterialResourceManagerAddResource(ImmaterialResourceManager.Resource resource, int rate, Vector3 positionArg, float radius)
        {
            lock (addRessourceLock)
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
