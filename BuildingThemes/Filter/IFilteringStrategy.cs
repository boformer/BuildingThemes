namespace BuildingThemes.Filter
{
    public interface IFilteringStrategy
    {
        bool DoesBuildingBelongToDistrict(string name, uint districtIdx);
    }
}