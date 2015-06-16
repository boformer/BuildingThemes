using ColossalFramework;

namespace BuildingThemes
{
    public class DefaultFilteringStrategy : IFilteringStrategy
    {
        public bool DoesBuildingBelongToDistrict(string name, int districtIdx)
        {
            return Singleton<BuildingThemesManager>.instance.GetAvailableBuidlings(districtIdx).Contains(name);
        }
    }
}