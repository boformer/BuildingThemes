using System.Reflection;
using ColossalFramework;
using UnityEngine;

namespace BuildingThemes.Detour
{
    public class DistrictManagerDetour : DistrictManager
    {

        private static bool deployed = false;

        private static RedirectCallsState _DistrictManager_ReleaseDistrictImplementation_state;
        private static MethodInfo _DistrictManager_ReleaseDistrictImplementation_original;
        private static MethodInfo _DistrictManager_ReleaseDistrictImplementation_detour;

        public static void Deploy()
        {
            if (!deployed)
            {
                _DistrictManager_ReleaseDistrictImplementation_original = typeof(DistrictManager).GetMethod("ReleaseDistrictImplementation", BindingFlags.Instance | BindingFlags.NonPublic);
                _DistrictManager_ReleaseDistrictImplementation_detour = typeof(DistrictManagerDetour).GetMethod("ReleaseDistrictImplementation", BindingFlags.Instance | BindingFlags.NonPublic);
                _DistrictManager_ReleaseDistrictImplementation_state = RedirectionHelper.RedirectCalls(_DistrictManager_ReleaseDistrictImplementation_original, _DistrictManager_ReleaseDistrictImplementation_detour);

                deployed = true;

                Debugger.Log("Building Themes: DistrictManager Methods detoured!");
            }
        }

        public static void Revert()
        {
            if (deployed)
            {
                RedirectionHelper.RevertRedirect(_DistrictManager_ReleaseDistrictImplementation_original, _DistrictManager_ReleaseDistrictImplementation_state);
                _DistrictManager_ReleaseDistrictImplementation_original = null;
                _DistrictManager_ReleaseDistrictImplementation_detour = null;

                deployed = false;

                Debugger.Log("Building Themes: DistrictManager Methods restored!");
            }
        }

        // Detour

        // The original ReleaseDistrictImplementation method.
        // It's same as vanilla implementation with only difference that out BuildingThemesManager#ToggleThemeManagement() method gets called

        private void ReleaseDistrictImplementation(byte district, ref District data)
        {
            BuildingThemesManager.instance.ToggleThemeManagement(district, false);
            if (data.m_flags == District.Flags.None)
                return;
            Singleton<InstanceManager>.instance.ReleaseInstance(new InstanceID()
            {
                District = district
            });
            data.m_flags = District.Flags.None;
            data.m_totalAlpha = 0U;
            this.m_districts.ReleaseItem(district);
            this.m_districtCount = (int)this.m_districts.ItemCount() - 1;
        } 
    }
}