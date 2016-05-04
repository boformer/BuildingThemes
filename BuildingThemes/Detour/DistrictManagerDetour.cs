using BuildingThemes.Redirection;
using ColossalFramework;

namespace BuildingThemes.Detour
{
    [TargetType(typeof(DistrictManager))]
    public class DistrictManagerDetour : DistrictManager
    {
        [RedirectMethod]
        private void ReleaseDistrictImplementation(byte district, ref District data)
        {
            if (data.m_flags == District.Flags.None)
                return;
            //begin mod
            BuildingThemesManager.instance.ToggleThemeManagement(district, false);
            //end mod
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