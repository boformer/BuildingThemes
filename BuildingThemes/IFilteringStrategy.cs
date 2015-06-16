namespace BuildingThemes
{
    public interface IFilteringStrategy
    {
        bool DoesBuildingBelongToDistrict(string name, int districtIdx);
    }
}