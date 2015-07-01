using ColossalFramework;
using ICities;

namespace BuildingThemes
{
    // This extension saves the position of buildings which are attempting to level up.
    public class LevelUpExtension : LevelUpExtensionBase
    {
        public override ResidentialLevelUp OnCalculateResidentialLevelUp(ResidentialLevelUp levelUp,
            int averageEducation, int landValue, ushort buildingID, Service service, SubService subService,
            Level currentLevel)
        {
            Building building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID];
            Detour.BuildingManagerDetour.position = building.m_position;

            return levelUp;
        }

        public override OfficeLevelUp OnCalculateOfficeLevelUp(OfficeLevelUp levelUp, int averageEducation,
            int serviceScore, ushort buildingID, Service service, SubService subService, Level currentLevel)
        {
            Building building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID];
            Detour.BuildingManagerDetour.position = building.m_position;

            return levelUp;
        }

        public override CommercialLevelUp OnCalculateCommercialLevelUp(CommercialLevelUp levelUp, int averageWealth,
            int landValue, ushort buildingID, Service service, SubService subService, Level currentLevel)
        {
            Building building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID];
            Detour.BuildingManagerDetour.position = building.m_position;

            return levelUp;
        }

        public override IndustrialLevelUp OnCalculateIndustrialLevelUp(IndustrialLevelUp levelUp, int averageEducation,
            int serviceScore, ushort buildingID, Service service, SubService subService, Level currentLevel)
        {
            Building building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID];
            Detour.BuildingManagerDetour.position = building.m_position;

            return levelUp;
        }
    }
}
