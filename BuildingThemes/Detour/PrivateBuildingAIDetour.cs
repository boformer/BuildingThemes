using ColossalFramework;
using ColossalFramework.Math;
using System;
using System.Reflection;
using UnityEngine;

namespace BuildingThemes.Detour
{
    public class PrivateBuildingAIDetour<A> where A : PrivateBuildingAI
    {
        private static bool deployed = false;

        private static RedirectCallsState _GetUpgradeInfo_state;
        private static MethodInfo _GetUpgradeInfo_original;
        private static MethodInfo _GetUpgradeInfo_detour;

        public static void Deploy()
        {
            if (!deployed)
            {
                _GetUpgradeInfo_original = typeof(A).GetMethod("GetUpgradeInfo", BindingFlags.Instance | BindingFlags.Public);
                _GetUpgradeInfo_detour = typeof(PrivateBuildingAIDetour<A>).GetMethod("GetUpgradeInfo", BindingFlags.Instance | BindingFlags.Public);
                _GetUpgradeInfo_state = RedirectionHelper.RedirectCalls(_GetUpgradeInfo_original, _GetUpgradeInfo_detour);

                deployed = true;

                Debug.LogFormat("Building Themes: {0} Methods detoured!", typeof(A).Name);
            }
        }

        public static void Revert()
        {
            if (deployed)
            {
                RedirectionHelper.RevertRedirect(_GetUpgradeInfo_original, _GetUpgradeInfo_state);
                _GetUpgradeInfo_original = null;
                _GetUpgradeInfo_detour = null;
                
                deployed = false;

                Debug.LogFormat("Building Themes: {0} Methods restored!", typeof(A).Name);
            }
        }

        public virtual BuildingInfo GetUpgradeInfo(ushort buildingID, ref Building data)
        {
            BuildingManagerDetour.position = data.m_position;

            BuildingInfo info = data.Info;
            Randomizer randomizer = new Randomizer((int)buildingID);
            ItemClass.Level level = info.m_class.m_level + 1;
            return Singleton<BuildingManager>.instance.GetRandomBuildingInfo(ref randomizer, info.m_class.m_service,
                info.m_class.m_subService, level, data.Width, data.Length, info.m_zoningMode);
        }
    }
}
