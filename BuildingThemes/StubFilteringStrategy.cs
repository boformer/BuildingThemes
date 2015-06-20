namespace BuildingThemes
{
    public class StubFilteringStrategy : IFilteringStrategy
    {
        public bool DoesBuildingBelongToDistrict(string name, uint districtIdx)
        {

            var isEuropean = (name.Contains("lock") || name.Contains("EU"));
            if (isEuropean && districtIdx == 0)
            {
                return false;
            }
            if (!isEuropean && districtIdx != 0)
            {
                return false;
            }
            return true;
        }
    }
}