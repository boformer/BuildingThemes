using System;
using BuildingThemes.Detour;
using BuildingThemes.GUI;
using BuildingThemes.Redirection;
using ColossalFramework;
using ICities;

namespace BuildingThemes
{
    public class LoadingExtension : LoadingExtensionBase
    {
        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);

            Debugger.Initialize();

            Debugger.Log("ON_CREATED");
            Debugger.Log("Building Themes: Initializing Mod...");

            try
            {

                PolicyPanelEnabler.Register();
                BuildingThemesManager.instance.Reset();
                BuildingVariationManager.instance.Reset();

                UpdateConfig();

                try
                {
                    Redirector<BuildingManagerDetour>.Deploy();
                    Debugger.Log("Building Themes: BuildingManager Methods detoured!");
                }
                catch (Exception e)
                {
                    Debugger.LogException(e);
                }
                try
                {
                    Redirector<DistrictManagerDetour>.Deploy();
                    Debugger.Log("Building Themes: DistrictManager Methods detoured!");
                }
                catch (Exception e)
                {
                    Debugger.LogException(e);
                }
                try
                {
                    Redirector<ZoneBlockDetour>.Deploy();
                    Debugger.Log("Building Themes: ZoneBlock Methods detoured!");
                    ZoneBlockDetour.SetUp();
                }
                catch (Exception e)
                {
                    Debugger.LogException(e);
                }
                try
                {
                    Detour.ImmaterialResourceManagerDetour.Deploy();
                }
                catch (Exception e)
                {
                    Debugger.LogException(e);
                }
                try
                {
                    Detour.PrivateBuildingAIDetour<ResidentialBuildingAI>.Deploy();
                }
                catch (Exception e)
                {
                    Debugger.LogException(e);
                }
                try
                {
                    Detour.PrivateBuildingAIDetour<CommercialBuildingAI>.Deploy();
                }
                catch (Exception e)
                {
                    Debugger.LogException(e);
                }
                try
                {
                    Detour.PrivateBuildingAIDetour<IndustrialBuildingAI>.Deploy();
                }
                catch (Exception e)
                {
                    Debugger.LogException(e);
                }
                try
                {
                    Detour.PrivateBuildingAIDetour<OfficeBuildingAI>.Deploy();
                }
                catch (Exception e)
                {
                    Debugger.LogException(e);
                }
                try
                {
                    Detour.PoliciesPanelDetour.Deploy();
                }
                catch (Exception e)
                {
                    Debugger.LogException(e);
                }
                try
                {
                    Redirector<DistrictWorldInfoPanelDetour>.Deploy();
                    Debugger.Log("Building Themes: DistrictWorldInfoPanel Methods detoured!");
                }
                catch (Exception e)
                {
                    Debugger.LogException(e);
                }

                Debugger.Log("Building Themes: Mod successfully intialized.");
            }
            catch (Exception e)
            {
                Debugger.LogException(e);
            }
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            Debugger.Log("ON_LEVEL_LOADED");
            Debugger.OnLevelLoaded();

            try
            {

                // Don't load if it's not a game
                if (mode != LoadMode.LoadGame && mode != LoadMode.NewGame) return;

                BuildingThemesManager.instance.ImportThemes();

                PolicyPanelEnabler.UnlockPolicyToolbarButton();
                UIThemeManager.Initialize();
                UIStyleButtonReplacer.ReplaceStyleButton();
            }
            catch (Exception e)
            {
                Debugger.LogException(e);
            }
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            Debugger.Log("ON_LEVEL_UNLOADING");
            Debugger.OnLevelUnloading();

            BuildingThemesManager.instance.Reset();
            UIThemeManager.Destroy();
        }

        public override void OnReleased()
        {
            base.OnReleased();
            Debugger.Log("ON_RELEASED");

            BuildingThemesManager.instance.Reset();
            BuildingVariationManager.instance.Reset();
            PolicyPanelEnabler.Unregister();

            Debugger.Log("Building Themes: Reverting detoured methods...");
            try
            {
                Detour.BuildingInfoDetour.Revert();
                Redirector<BuildingManagerDetour>.Revert();
                Debugger.Log("Building Themes: BuildingManager Methods restored!");
                Redirector<DistrictManagerDetour>.Revert();
                Debugger.Log("Building Themes: DistrictManager Methods restored!");
                Redirector<ZoneBlockDetour>.Revert();
                Debugger.Log("Building Themes: ZoneBlock Methods restored!");
                Detour.ImmaterialResourceManagerDetour.Revert();
                Detour.PrivateBuildingAIDetour<ResidentialBuildingAI>.Revert();
                Detour.PrivateBuildingAIDetour<CommercialBuildingAI>.Revert();
                Detour.PrivateBuildingAIDetour<IndustrialBuildingAI>.Revert();
                Detour.PrivateBuildingAIDetour<OfficeBuildingAI>.Revert();
                Detour.PoliciesPanelDetour.Revert();
                Redirector<DistrictWorldInfoPanelDetour>.Revert();
                Debugger.Log("Building Themes: DistrictWorldInfoPanel Methods restored!");
            }
            catch (Exception e)
            {
                Debugger.LogException(e);
            }

            Debugger.Log("Building Themes: Done!");

            Debugger.Deinitialize();
        }

        private void UpdateConfig()
        {
            // If config version is 0, disable the cloning feature if it is not used in one of the themes
            if (BuildingVariationManager.Enabled)
            {
                bool cloneFeatureUsed = false;

                if (BuildingThemesManager.instance.Configuration.version == 0)
                {
                    foreach (var theme in BuildingThemesManager.instance.Configuration.themes)
                    {
                        foreach (var building in theme.buildings)
                        {
                            if (building.baseName != null)
                            {
                                cloneFeatureUsed = true;
                                break;
                            }
                        }

                        if (cloneFeatureUsed) break;
                    }
                }
                else cloneFeatureUsed = true;

                if (cloneFeatureUsed)
                {
                    try { Detour.BuildingInfoDetour.Deploy(); }
                    catch (Exception e) { Debugger.LogException(e); }
                }
                else
                {
                    BuildingVariationManager.Enabled = false;
                }
            }
            BuildingThemesManager.instance.Configuration.version = 1;
            BuildingThemesManager.instance.SaveConfig();
        }
    }
}