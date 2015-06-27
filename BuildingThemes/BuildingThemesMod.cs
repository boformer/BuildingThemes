using ICities;
using System;
using System.Collections.Generic;
using ColossalFramework;
using ColossalFramework.UI;
using ColossalFramework.Plugins;
using System.Text;
using UnityEngine;
using System.Reflection;
using System.Resources;
using System.Threading;
using ColossalFramework.Math;

namespace BuildingThemes
{


    public class LevelUpExtension : LevelUpExtensionBase
    {

        public override ResidentialLevelUp OnCalculateResidentialLevelUp(ResidentialLevelUp levelUp,
            int averageEducation, int landValue, ushort buildingID, Service service, SubService subService,
            Level currentLevel)
        {
            BuildingManager buildingManager = Singleton<BuildingManager>.instance;
            Building building = buildingManager.m_buildings.m_buffer[buildingID];
            if (BuildingThemesMod.isDebug)
            {
                UnityEngine.Debug.LogFormat("Building Themes: OnCalculateResidentialLevelUp. buildingID: {0}, target level: {1}, position: {2}. current thread: {3}", buildingID, levelUp.targetLevel, building.m_position, Thread.CurrentThread.ManagedThreadId);
            }
            DetoursHolder.position = building.m_position;
            return levelUp;
        }

        public override OfficeLevelUp OnCalculateOfficeLevelUp(OfficeLevelUp levelUp, int averageEducation,
            int serviceScore, ushort buildingID, Service service, SubService subService, Level currentLevel)
        {
            BuildingManager buildingManager = Singleton<BuildingManager>.instance;
            Building building = buildingManager.m_buildings.m_buffer[buildingID];
            if (BuildingThemesMod.isDebug)
            {

                UnityEngine.Debug.LogFormat(
                    "Building Themes: OnCalculateOfficeLevelUp. buildingID: {0}, target level: {1}, position: {2}. current thread: {3}",
                    buildingID, levelUp.targetLevel, building.m_position, Thread.CurrentThread.ManagedThreadId);
            }
            DetoursHolder.position = building.m_position;
            return levelUp;
        }

        public override CommercialLevelUp OnCalculateCommercialLevelUp(CommercialLevelUp levelUp, int averageWealth,
            int landValue, ushort buildingID, Service service, SubService subService, Level currentLevel)
        {
           BuildingManager buildingManager = Singleton<BuildingManager>.instance;
            Building building = buildingManager.m_buildings.m_buffer[buildingID];
            if (BuildingThemesMod.isDebug)
            {

                UnityEngine.Debug.LogFormat(
                    "Building Themes: OnCalculateCommercialLevelUp. buildingID: {0}, target level: {1}, position: {2}. current thread: {3}",
                    buildingID, levelUp.targetLevel, building.m_position, Thread.CurrentThread.ManagedThreadId);
            }
            DetoursHolder.position = building.m_position;
            return levelUp;
        }

        public override IndustrialLevelUp OnCalculateIndustrialLevelUp(IndustrialLevelUp levelUp, int averageEducation,
            int serviceScore, ushort buildingID, Service service, SubService subService, Level currentLevel)
        {
            BuildingManager buildingManager = Singleton<BuildingManager>.instance;
            Building building = buildingManager.m_buildings.m_buffer[buildingID];
            if (BuildingThemesMod.isDebug)
            {

                UnityEngine.Debug.LogFormat(
                    "Building Themes: OnCalculateIndustrialLevelUp. buildingID: {0}, target level: {1}, position: {2}. current thread: {3}",
                    buildingID, levelUp.targetLevel, building.m_position, Thread.CurrentThread.ManagedThreadId);
            }
            DetoursHolder.position = building.m_position;
            return levelUp;
        }
    }



    public class BuildingThemesMod : LoadingExtensionBase, IUserMod
    {

        public static bool isDebug = false;

        public string Name
        {
            get
            {
                return "Building Themes";
            }
        }

        public string Description
        {
            get { return "Create building themes and apply them to cities and districts."; }
        }

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
            if (isDebug)
            {
                Debug.Log("Building Themes: Initializing Building Themes Mod...");
            }
            Singleton<BuildingThemesManager>.instance.Reset();
            Singleton<BuildingThemesManager>.instance.searchBuildingThemeMods();

            DetoursHolder.InitTable();
            DetoursHolder.FilteringStrategy = new DefaultFilteringStrategy();//new StubFilteringStrategy();
            //TODO(earalov): save redirected state

            DetoursHolder.getRandomBuildingInfo = typeof(BuildingManager).GetMethod("GetRandomBuildingInfo", BindingFlags.Instance | BindingFlags.Public);
            DetoursHolder.getRandomBuildingInfoState = RedirectionHelper.RedirectCalls(
                DetoursHolder.getRandomBuildingInfo,
                typeof(DetoursHolder).GetMethod("GetRandomBuildingInfo", BindingFlags.Instance | BindingFlags.Public)
                );

            DetoursHolder.zoneBlockSimulationStep = typeof(ZoneBlock).GetMethod("SimulationStep", BindingFlags.Public | BindingFlags.Instance);
            DetoursHolder.zoneBlockSimulationStepPtr = DetoursHolder.zoneBlockSimulationStep.MethodHandle.GetFunctionPointer();
            DetoursHolder.zoneBlockSimulationStepDetourPtr = typeof(DetoursHolder).GetMethod("ZoneBlockSimulationStep", BindingFlags.Public | BindingFlags.Instance).MethodHandle.GetFunctionPointer();
            DetoursHolder.zoneBlockSimulationStepState = RedirectionHelper.PatchJumpTo(
                DetoursHolder.zoneBlockSimulationStepPtr,
                DetoursHolder.zoneBlockSimulationStepDetourPtr
                );
            DetoursHolder.resourceManagerAddResource = typeof(ImmaterialResourceManager).GetMethod("AddResource", new[] { typeof(ImmaterialResourceManager.Resource), typeof(int), typeof(Vector3), typeof(float) });
            DetoursHolder.resourceManagerAddResourcePtr = DetoursHolder.resourceManagerAddResource.MethodHandle.GetFunctionPointer();
            DetoursHolder.resourceManagerAddResourceDetourPtr = typeof(DetoursHolder).GetMethod("ImmaterialResourceManagerAddResource").MethodHandle.GetFunctionPointer();
            DetoursHolder.resourceManagerAddResourceState = RedirectionHelper.PatchJumpTo(
                DetoursHolder.resourceManagerAddResourcePtr,
                DetoursHolder.resourceManagerAddResourceDetourPtr
                );

            // neccessary because of how the game logic is bound to the GUI

            PoliciesPanelDetour.refreshPanel = typeof(PoliciesPanel).GetMethod("RefreshPanel", BindingFlags.Instance | BindingFlags.NonPublic);
            PoliciesPanelDetour.refreshPanelDetour = typeof(PoliciesPanelDetour).GetMethod("RefreshPanel", BindingFlags.Instance | BindingFlags.NonPublic);
            PoliciesPanelDetour.refreshPanelState = RedirectionHelper.RedirectCalls(PoliciesPanelDetour.refreshPanel, PoliciesPanelDetour.refreshPanelDetour);

            PoliciesPanelDetour.setParentButton = typeof(PoliciesPanel).GetMethod("SetParentButton", BindingFlags.Instance | BindingFlags.Public);
            PoliciesPanelDetour.setParentButtonDetour = typeof(PoliciesPanelDetour).GetMethod("SetParentButton", BindingFlags.Instance | BindingFlags.Public);
            PoliciesPanelDetour.setParentButtonState = RedirectionHelper.RedirectCalls(PoliciesPanelDetour.setParentButton, PoliciesPanelDetour.setParentButtonDetour);

            if (isDebug)
            {
                Debug.Log("Building Themes: Building Themes Mod successfully intialized.");
            }
        }

        public override void OnLevelLoaded(LoadMode mode) 
        {
            // Is it an actual game ?
            //if (mode != LoadMode.LoadGame && mode != LoadMode.NewGame) return;

            // TODO load data (serialized for policies, xml for themes)

            // Hook into policies GUI
            //ToolsModifierControl.policiesPanel.component.eventVisibilityChanged += OnPoliciesPanelVisibilityChanged;
        }

        public override void OnLevelUnloading()
        {
            Singleton<BuildingThemesManager>.instance.Reset();
        }

        public override void OnReleased() 
        {
            UnityEngine.Debug.Log("Building Themes: Reverting detoured methods...");

            if (DetoursHolder.getRandomBuildingInfo != null) {
                UnityEngine.Debug.Log("Building Themes: Reverting simulation methods");

                RedirectionHelper.RevertRedirect(DetoursHolder.getRandomBuildingInfo, DetoursHolder.getRandomBuildingInfoState);
                DetoursHolder.getRandomBuildingInfo = null;

                RedirectionHelper.RevertJumpTo(DetoursHolder.zoneBlockSimulationStepPtr, DetoursHolder.zoneBlockSimulationStepState);

                RedirectionHelper.RevertJumpTo(DetoursHolder.resourceManagerAddResourcePtr, DetoursHolder.resourceManagerAddResourceState);
                
            }
            if (PoliciesPanelDetour.refreshPanel != null) 
            {
                UnityEngine.Debug.Log("Building Themes: Reverting GUI methods");
                
                // GUI
                RedirectionHelper.RevertRedirect(PoliciesPanelDetour.refreshPanel, PoliciesPanelDetour.refreshPanelState);
                PoliciesPanelDetour.refreshPanel = null;

                RedirectionHelper.RevertRedirect(PoliciesPanelDetour.setParentButton, PoliciesPanelDetour.setParentButtonState);
                PoliciesPanelDetour.setParentButton = null;

            }
            UnityEngine.Debug.Log("Building Themes: Done!");
        }

        private string GetCurrentEnvironment()
        {
            return Singleton<SimulationManager>.instance.m_metaData.m_environment;
        }

        // GUI stuff

    }
}
