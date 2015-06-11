using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

namespace BuildingThemes
{

    class DetoursHolder
    {

        //we'll use this variable to pass position to GetRandomBuildingInfo method. Or we can just pass District
        private static Vector3 position;

        public static RedirectCallsState zoneBlockSimulationStepState;
        public static RedirectCallsState privateBuidingAiSimulationStepState;
        public static RedirectCallsState privateBuidingAiGetUpgradeInfoState;


        //this is detoured version of BuildingManger#GetRandomBuildingInfo method. Note, that it's an instance method. It's better because this way all registers will be expected to have the same values
        //as in original methods
        public BuildingInfo GetRandomBuildingInfo(ref Randomizer r, ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level, int width, int length, BuildingInfo.ZoningMode zoningMode)
        {
            UnityEngine.Debug.LogFormat("Building Themes: Detoured GetRandomBuildingInfo. position: {0}", position);

            //this part is the same as in original method
            //I love this hack :) This is how I get this reference - and save it to a variable
            var buildingManager = (BuildingManager)Convert.ChangeType(this, typeof(BuildingManager));
            var refreshAreaBuidlings = typeof(BuildingManager).GetMethod("RefreshAreaBuildings", BindingFlags.NonPublic | BindingFlags.Instance);
            refreshAreaBuidlings.Invoke(buildingManager, new object[] { });
            var areaBuildings = (FastList<ushort>[])GetInstanceField(typeof(BuildingManager), buildingManager,
                "m_areaBuildings");
            var getAreaIndex = typeof(BuildingManager).GetMethod("GetAreaIndex", BindingFlags.NonPublic | BindingFlags.Static);

            FastList<ushort> fastList = areaBuildings[(int)getAreaIndex.Invoke(null, new object[] { service, subService, level, width, length, zoningMode })];
            if (fastList == null)
                return (BuildingInfo)null;
            if (fastList.m_size == 0)
                return (BuildingInfo)null;
            ushort districtIdx = Singleton<DistrictManager>.instance.GetDistrict(position);
            UnityEngine.Debug.LogFormat("Building Themes: Detoured GetRandomBuildingInfo. districtIdx: {0}", districtIdx);
            //District district = Singleton<DistrictManager>.instance.m_districts.m_buffer[districtIdx];
            //TODO(earalov): here fastList variable should be filtered. All buildings that  don't belong to the district should be removed from this list.
            int index = r.Int32((uint)fastList.m_size);
            return PrefabCollection<BuildingInfo>.GetPrefab((uint)fastList.m_buffer[index]);
        }

        public void ZoneBlockSimulationStep(ushort blockID)
        {
            var zoneBlock = (ZoneBlock)Convert.ChangeType(this, typeof(ZoneBlock));
            UnityEngine.Debug.LogFormat("Building Themes: Detoured ZoneBlock.SimulationStep was called. blockID: {0}, position: {1}", blockID, zoneBlock.m_position);
            position = zoneBlock.m_position;
            var methodInfo = typeof(ZoneBlock).GetMethod("SimulationStep", BindingFlags.Public | BindingFlags.Instance);
            RedirectionHelper.RevertRedirect(
                methodInfo,
                zoneBlockSimulationStepState
                );
            methodInfo.Invoke(zoneBlock, new object[] { blockID });
            RedirectionHelper.RedirectCalls(
                methodInfo,
                typeof(DetoursHolder).GetMethod("ZoneBlockSimulationStep", BindingFlags.Public | BindingFlags.Instance)
                );

        }

        public BuildingInfo PrivateBuildingAiGetUpgradeInfo(ushort buildingID, ref Building data)
        {
            UnityEngine.Debug.LogFormat("Building Themes: Detoured PrivateBuildingAI.GetUpgradeInfo was called. buildingID: {0}, position: {1}", buildingID, data.m_position);
            var privateBuildingAi = (PrivateBuildingAI)Convert.ChangeType(this, typeof(PrivateBuildingAI));
            position = data.m_position;
            var methodInfo = typeof(PrivateBuildingAI).GetMethod("GetUpgradeInfo", BindingFlags.Public | BindingFlags.Instance);
            RedirectionHelper.RevertRedirect(
                methodInfo,
                privateBuidingAiGetUpgradeInfoState
                );
            var returnValue = (BuildingInfo)methodInfo.Invoke(privateBuildingAi, new object[] { buildingID, data });
            RedirectionHelper.RedirectCalls(
                methodInfo,
                typeof(DetoursHolder).GetMethod("PrivateBuildingAiGetUpgradeInfo", BindingFlags.Public | BindingFlags.Instance)
                );
            return returnValue;
        }

        public void PrivateBuildingAiSimulationStep(ushort buildingID, ref Building buildingData,
            ref Building.Frame frameData)
        {
            UnityEngine.Debug.LogFormat("Building Themes: Detoured PrivateBuildingAI.SimulationStep. buildingID: {0}, position: {1}", buildingID, buildingData.m_position);
            var privateBuildingAi = (PrivateBuildingAI)Convert.ChangeType(this, typeof(PrivateBuildingAI));
            position = buildingData.m_position;
            var methodInfo = typeof(PrivateBuildingAI).GetMethod("SimulationStep", BindingFlags.Public | BindingFlags.Instance);
            RedirectionHelper.RevertRedirect(
                methodInfo,
                privateBuidingAiSimulationStepState
                );
            methodInfo.Invoke(privateBuildingAi, new object[] { buildingID, buildingData, frameData });
            RedirectionHelper.RedirectCalls(
                methodInfo,
                typeof(DetoursHolder).GetMethod("PrivateBuildingAiSimulationStep", BindingFlags.Public | BindingFlags.Instance)
                );
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
