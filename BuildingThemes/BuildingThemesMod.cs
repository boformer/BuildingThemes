using System;
using ICities;
using BuildingThemes.GUI;
using CitiesHarmony.API;
using ColossalFramework.UI;
using Epic.OnlineServices.Platform;
using UnityEngine;

namespace BuildingThemes
{
    public class BuildingThemesMod : IUserMod
    {
        public static bool xmlCorrupt = false;
        
        
        // we'll use this variable to pass the building position to GetRandomBuildingInfo method. It's here to make possible 81 Tiles compatibility
        public static Vector3 position;
        public static readonly string EIGHTY_ONE_MOD = "81 Tiles (Fixed for C:S 1.2+)";
        public static readonly string EIGHTY_ONE_2_ASSEMBLY = "EightyOne2";

        public string Name => "Building Themes";

        public string Description => "Create building themes and apply them to cities and districts.";

        public void OnSettingsUI(UIHelperBase helper)
        {
            UIHelperBase group = helper.AddGroup("Building Themes");
            try
            {
                group.AddCheckbox("Unlock Policies Panel From Start", PolicyPanelEnabler.Unlock,
                    delegate (bool c) { PolicyPanelEnabler.Unlock = c; });
                group.AddCheckbox("Enable Prefab Cloning (experimental, not stable!)", BuildingVariationManager.Enabled,
                    delegate (bool c) { BuildingVariationManager.Enabled = c; });
                group.AddGroup("Warning: When you disable this option, spawned clones will disappear!");

                group.AddCheckbox("Warning message when selecting an invalid theme", UIThemePolicyItem.showWarning,
                    delegate (bool c) { UIThemePolicyItem.showWarning = c; });
                group.AddCheckbox("Generate Debug Output", Debugger.Enabled, delegate (bool c) { Debugger.Enabled = c; });
            }
            catch
            {
                group.AddGroup("BuildingThemes is unable to read the BuildingThemes.xml file\n" +
                               "that stores your settings and themes!\n" +
                               "To fix it, delete this file and restart the game:\n" +
                               "{Steam folder}\\steamapps\\common\\Cities_Skylines\\BuildingThemes.xml");
            }

        }

        public void OnEnabled() {
            HarmonyHelper.EnsureHarmonyInstalled();
        }
    }
}
