using ICities;
using BuildingThemes.GUI;
using ColossalFramework.UI;
using UnityEngine;

namespace BuildingThemes
{
    public class BuildingThemesMod : IUserMod
    {
        // we'll use this variable to pass the building position to GetRandomBuildingInfo method. It's here to make possible 81 Tiles compatibility
        public static Vector3 position;
        public static readonly string EIGHTY_ONE_MOD = "81 Tiles (Fixed for C:S 1.2+)";

        public string Name { get { return "Building Themes"; } }

        public string Description
        {
            get
            {
                var adPanel = GameObject.Find("WorkshopAdPanel");
                var chirper = GameObject.Find("Chirper");
                var moo = GameObject.Find("MooMemorial");
                if (moo == null && chirper != null && adPanel != null)
                {
                    var chirperSprite = chirper.GetComponent<UISprite>();
                    if (chirperSprite != null)
                    {
                        chirperSprite.isVisible = false;
                        var label = chirperSprite.parent.AddUIComponent<UILabel>();
                        label.name = "MooMemorial";
                        label.textColor = new Color32(128, 128, 128, 255);
                        label.bottomColor = new Color32(52, 112, 140, 255); //new Color32(163, 226, 254, 255);
                        label.useGradient = true;
                        label.dropShadowColor = new Color32(0, 0, 0, 255);
                        label.dropShadowOffset = new Vector2(0f, -1.33f);
                        label.useDropShadow = true;
                        label.text = "Dedicated to TotalyMoo";
                        label.tooltip = "The greatest community manager of all times!";
                        label.isTooltipLocalized = false;
                        label.CenterToParent();
                        label.position = new Vector2(label.position.x, chirperSprite.position.y);
                    }
                }
                return "Create building themes and apply them to cities and districts.";
            }
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            UIHelperBase group = helper.AddGroup("Building Themes");
            group.AddCheckbox("Unlock Policies Panel From Start", PolicyPanelEnabler.Unlock, delegate(bool c) { PolicyPanelEnabler.Unlock = c; });
            group.AddCheckbox("Enable Prefab Cloning (experimental, not stable!)", BuildingVariationManager.Enabled, delegate(bool c) { BuildingVariationManager.Enabled = c; });
            group.AddGroup("Warning: When you disable this option, spawned clones will disappear!");

            group.AddCheckbox("Warning message when selecting an invalid theme", UIThemePolicyItem.showWarning, delegate(bool c) { UIThemePolicyItem.showWarning = c; });
            group.AddCheckbox("Debug Mode", Debugger.Enabled, delegate(bool c) { Debugger.Enabled = c; });

        }
    }
}
