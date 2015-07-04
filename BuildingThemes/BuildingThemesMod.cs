using ICities;
using ColossalFramework;
using UnityEngine;

namespace BuildingThemes
{
    public class BuildingThemesMod : LoadingExtensionBase, IUserMod
    {
        public static bool isDebug = false;

        public string Name
        {
            get { return "Building Themes"; }
        }
        public string Description
        {
            get { return "Create building themes and apply them to cities and districts."; }
        }

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);

            Debug.Log("Building Themes: Initializing Mod...");

            Singleton<BuildingThemesManager>.instance.Reset();
            Singleton<BuildingThemesManager>.instance.searchBuildingThemeMods();

            Detour.BuildingManagerDetour.Deploy();
            Detour.ZoneBlockDetour.Deploy();
            Detour.ImmaterialResourceManagerDetour.Deploy();
            Detour.PrivateBuildingAIDetour<ResidentialBuildingAI>.Deploy();
            Detour.PrivateBuildingAIDetour<CommercialBuildingAI>.Deploy();
            Detour.PrivateBuildingAIDetour<IndustrialBuildingAI>.Deploy();
            Detour.PrivateBuildingAIDetour<OfficeBuildingAI>.Deploy();
            Detour.PoliciesPanelDetour.Deploy();

            Debug.Log("Building Themes: Mod successfully intialized.");
        }

        public override void OnReleased()
        {
            base.OnReleased();

            UnityEngine.Debug.Log("Building Themes: Reverting detoured methods...");

            Singleton<BuildingThemesManager>.instance.Reset();

            Detour.BuildingManagerDetour.Revert();
            Detour.ZoneBlockDetour.Revert();
            Detour.ImmaterialResourceManagerDetour.Revert();
            Detour.PrivateBuildingAIDetour<ResidentialBuildingAI>.Revert();
            Detour.PrivateBuildingAIDetour<CommercialBuildingAI>.Revert();
            Detour.PrivateBuildingAIDetour<IndustrialBuildingAI>.Revert();
            Detour.PrivateBuildingAIDetour<OfficeBuildingAI>.Revert();
            Detour.PoliciesPanelDetour.Revert();

            UnityEngine.Debug.Log("Building Themes: Done!");
        }
    }
}
