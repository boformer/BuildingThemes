namespace BuildingThemes
{
    public interface IFilteringStrategy
    {
        bool DoesBuildingBelongToDistrict(string name, uint districtIdx);
    }
}