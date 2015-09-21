using ColossalFramework;
using ColossalFramework.Math;
using System.Reflection;

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

                Debugger.LogFormat("Building Themes: {0} Methods detoured!", typeof(A).Name);
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

                Debugger.LogFormat("Building Themes: {0} Methods restored!", typeof(A).Name);
            }
        }

        public virtual BuildingInfo GetUpgradeInfo(ushort buildingID, ref Building data)
        {
            // This method is very fragile, no logging here!

            BuildingInfo info = data.Info;

            Randomizer randomizer = new Randomizer((int)buildingID);
            for (int i = 0; i <= (int)info.m_class.m_level; i++)
            {
                randomizer.Int32(1000u);
            }

            ItemClass.Level level = info.m_class.m_level + 1;
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte district = instance.GetDistrict(data.m_position);
            ushort style = instance.m_districts.m_buffer[(int)district].m_Style;

            /*
            return Singleton<BuildingManager>.instance.GetRandomBuildingInfo(ref randomizer, info.m_class.m_service,
                info.m_class.m_subService, level, data.Width, data.Length, info.m_zoningMode);
            */

            return BuildingManagerDetour.GetRandomBuildingInfo_Upgrade(data.m_position, data.m_infoIndex,
                ref randomizer, info.m_class.m_service, info.m_class.m_subService, level, data.Width, data.Length, info.m_zoningMode, style);
        }
    }
}
