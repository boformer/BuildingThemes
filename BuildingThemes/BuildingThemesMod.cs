using System;
using ICities;
using ColossalFramework;
using ColossalFramework.UI;
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
            Detour.PoliciesPanelDetour.Revert();

            UnityEngine.Debug.Log("Building Themes: Done!");
        }

        public GameObject gameObject;

        public override void OnLevelLoaded(LoadMode mode)
        {
            try
            {
                // Is it an actual game ?
                if (mode != LoadMode.LoadGame && mode != LoadMode.NewGame) return;

                // Creating our own gameObect, helps finding the UI in ModTools
                gameObject = new GameObject("BuildingThemes");
                gameObject.transform.parent = UIView.GetAView().transform;
                gameObject.AddComponent<GUI.UIThemeManager>();
            }
            catch (Exception e)
            {
                // Catching any exception to not block the loading process of other mods
                Debug.Log("Building Themes: An error has happened during the UI creation.");
                Debug.LogException(e);
            }
        }

        public override void OnLevelUnloading()
        {
            try
            {
                if(gameObject != null)
                GameObject.Destroy(gameObject);
            }
            catch (Exception e)
            {
                // Catching any exception to not block the unloading process of other mods
                Debug.Log("Building Themes: An error has happened during the UI destruction.");
                Debug.LogException(e);
            }
        }
    }
}
