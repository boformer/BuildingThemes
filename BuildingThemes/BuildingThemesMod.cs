using ICities;
using System;
using System.Collections.Generic;
using ColossalFramework;
using ColossalFramework.UI;
using ColossalFramework.Plugins;
using System.Text;
using UnityEngine;
using System.Reflection;

namespace BuildingThemes
{
    public class BuildingThemesMod : LoadingExtensionBase, IUserMod
    {
        public string Name
        {
            get { return "Building Themes"; }
        }

        public string Description
        {
            get { return "Create building themes and apply them to map themes, cities and districts."; }
        }

        private const string configPath = "BuildingThemes.xml";

        private UIButton tab;

        // district id --> list of enabled themes
        private Dictionary<byte, List<string>> districtThemes;

        // [district id][area index] --> prefab fastlist
        private FastList<ushort>[,] m_district_areaBuildings;

        // theme name --> prefab fastlists
        private Dictionary<string, FastList<ushort>[]> themeBuildings;

        public override void OnLevelLoaded(LoadMode mode) 
        {
            // Is it an actual game ?
            if (mode != LoadMode.LoadGame && mode != LoadMode.NewGame) return;

            // TODO: load and save the district "policy" data. This is just a placeholder
            districtThemes = new Dictionary<byte, List<string>>();
            for (byte i = 0; i < 128; i++) 
            {
                districtThemes.Add(i, new List<string>(new string[] { "European", "International" }));
            }

            // load the xml configuration
            Configuration config = Configuration.Deserialize(configPath);

            if (config == null)
            {
                config = Configuration.GenerateDefaultConfig();
                Configuration.Serialize(configPath, config);
            }

            // generate the list of buildings per theme, ordered by area index
            Dictionary<string, FastList<ushort>[]> themeBuildings = GenerateThemeBuildingLists(config);

            // 128 districts, 2720 possible area indexes
            m_district_areaBuildings = new FastList<ushort>[128, 2720];
            //TODO compile the fastlists for every district.

            // Hook into policies GUI
            ToolsModifierControl.policiesPanel.component.eventVisibilityChanged += OnPoliciesPanelVisibilityChanged;

            ReplaceBuildingManager();
        }

        public override void OnLevelUnloading()
        {
            // Remove the custom policy tab
            RemoveThemesTab();

            //TODO(earalov): revert detoured methods
        }

        private string GetCurrentEnvironment()
        {
            return Singleton<SimulationManager>.instance.m_metaData.m_environment;
        }

        private void ReplaceBuildingManager()
        {
            //TODO(earalov): save redirected state
            RedirectionHelper.RedirectCalls(
                typeof (BuildingManager).GetMethod("GetRandomBuildingInfo", BindingFlags.Instance | BindingFlags.Public),
                typeof (DetoursHolder).GetMethod("GetRandomBuildingInfo", BindingFlags.Instance | BindingFlags.Public)
                );
            Debug.Log("Building Themes: Building Manager successfully replaced.");

        }

        private Dictionary<string, FastList<ushort>[]> GenerateThemeBuildingLists(Configuration config) 
        {
            Dictionary<string, FastList<ushort>[]> lists = new Dictionary<string, FastList<ushort>[]>(config.themes.Count);

            foreach (Configuration.Theme theme in config.themes)
            {
                FastList<ushort>[] list = new FastList<ushort>[2720];
                lists.Add(theme.name, list);

                foreach (Configuration.Building building in theme.buildings)
                {
                    for (int i = 0; i < PrefabCollection<BuildingInfo>.PrefabCount(); i++)
                    {
                        BuildingInfo prefab = PrefabCollection<BuildingInfo>.GetPrefab((uint)i);
                        if (prefab != null && prefab.name == building.name && prefab.m_class.m_service != ItemClass.Service.None && prefab.m_placementStyle == ItemClass.Placement.Automatic && prefab.m_class.m_service <= ItemClass.Service.Office)
                        {
                            if (prefab.m_cellWidth < 1 || prefab.m_cellWidth > 4)
                            {
                                string text = PrefabCollection<BuildingInfo>.PrefabName((uint)i);
                                CODebugBase<LogChannel>.Error(LogChannel.Core, string.Concat(new object[]
						        {
							        "Invalid width (",
							        text,
							        "): ",
							        prefab.m_cellWidth
						        }));
                            }
                            else if (prefab.m_cellLength < 1 || prefab.m_cellLength > 4)
                            {
                                string text2 = PrefabCollection<BuildingInfo>.PrefabName((uint)i);
                                CODebugBase<LogChannel>.Error(LogChannel.Core, string.Concat(new object[]
						        {
							        "Invalid length (",
							        text2,
							        "): ",
							        prefab.m_cellLength
						        }));
                            }
                            else
                            {
                                int areaIndex = BuildingThemesMod.GetAreaIndex(prefab.m_class.m_service, prefab.m_class.m_subService, prefab.m_class.m_level, prefab.m_cellWidth, prefab.m_cellLength, prefab.m_zoningMode);

                                if (list[areaIndex] == null) list[areaIndex] = new FastList<ushort>();

                                list[areaIndex].Add((ushort)i);
                            }
                        }
                    }
                }
            }
            return lists;
        }

    	public static int GetAreaIndex(ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level, int width, int length, BuildingInfo.ZoningMode zoningMode)
	    {
		    int num;
		    if (subService != ItemClass.SubService.None)
		    {
			    num = 8 + subService - ItemClass.SubService.ResidentialLow;
		    }
		    else
		    {
			    num = service - ItemClass.Service.Residential;
		    }
		    num = (int)(num * 5 + level);
		    if (zoningMode == BuildingInfo.ZoningMode.CornerRight)
		    {
			    num = num * 4 + length - 1;
			    num = num * 4 + width - 1;
			    num = num * 2 + 1;
		    }
		    else
		    {
			    num = num * 4 + width - 1;
			    num = num * 4 + length - 1;
			    num = (int)(num * 2 + zoningMode);
		    }
		    return num;
	    }


        // GUI stuff

        private void OnPoliciesPanelVisibilityChanged(UIComponent component, bool visible)
        {
            // It is necessary to remove the custom tab when the panel is closed 
            // because the game logic is coupled to the GUI
            if (visible)
            {
                AddThemesTab();
            }
            else
            {
                RemoveThemesTab();
            }
        }

        private void AddThemesTab()
        {
            UITabstrip tabstrip = ToolsModifierControl.policiesPanel.Find("Tabstrip") as UITabstrip;
            tab = tabstrip.AddTab("Themes");
            tab.stringUserData = "CityPlanning";

            // recalculate the width of the tabs
            for (int i = 0; i < tabstrip.tabCount; i++)
            {
                tabstrip.tabs[i].width = tabstrip.width / ((float)tabstrip.tabCount - 1);
            }

            // TODO this is hacky. better store it in a field
            GameObject go = GameObject.Find("Tab 5 - Themes");
            if (go == null)
            {
                return;
            }

            // remove the default stuff if something is in there
            foreach (Transform child in go.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            UIPanel container = go.GetComponent<UIPanel>();

            container.autoLayout = true;
            container.autoLayoutDirection = LayoutDirection.Vertical;
            container.autoLayoutPadding.top = 5;

            // add some sample buttons

            AddThemePolicyButton(container, "Chicago 1890");
            AddThemePolicyButton(container, "New York 1940");
            AddThemePolicyButton(container, "Houston 1990");
            AddThemePolicyButton(container, "Euro-Contemporary");
            AddThemePolicyButton(container, "My first custom theme");
        }

        private void RemoveThemesTab() 
        {
            // TODO this is hacky. better store it in a field
            GameObject go = GameObject.Find("Tab 5 - Themes");
            if (go == null)
            {
                return;
            }
            GameObject.Destroy(go);

            UITabstrip tabstrip = ToolsModifierControl.policiesPanel.Find("Tabstrip") as UITabstrip;
            tabstrip.RemoveUIComponent(tab);
            GameObject.Destroy(tab.gameObject);
        }

        private void AddThemePolicyButton(UIPanel container, string name) 
        {
            
            UIPanel policyPanel = container.AddUIComponent<UIPanel>();
            policyPanel.name = name;
            policyPanel.backgroundSprite = "GenericPanel";
            policyPanel.size = new Vector2(364f, 44f);
            policyPanel.objectUserData = ToolsModifierControl.policiesPanel;
            policyPanel.stringUserData = "None";

            UIButton policyButton = policyPanel.AddUIComponent<UIButton>();
            policyButton.name = "PolicyButton";
            policyButton.text = name;
            policyButton.size = new Vector2(324f, 40f);
            policyButton.focusedBgSprite = "PolicyBarBackActive";
            policyButton.normalBgSprite = "PolicyBarBack";
            policyButton.relativePosition = new Vector3(2f, 2f, 0f);
            policyButton.textPadding.left = 50;
            policyButton.textColor = new Color32(0,0,0,255);
            policyButton.disabledTextColor = new Color32(0, 0, 0, 255);
            policyButton.hoveredTextColor = new Color32(0, 0, 0, 255);
            policyButton.pressedTextColor = new Color32(0, 0, 0, 255);
            policyButton.focusedTextColor = new Color32(0, 0, 0, 255);
            policyButton.disabledColor = new Color32(124, 124, 124, 255);
            policyButton.dropShadowColor = new Color32(103, 103, 103, 255);
            policyButton.dropShadowOffset = new Vector2(1f, 1f);
            policyButton.textHorizontalAlignment = UIHorizontalAlignment.Left;
            policyButton.useDropShadow = false;
            policyButton.textScale = 0.875f;

            UICheckBox policyCheckBox = policyButton.AddUIComponent<UICheckBox>();
            policyCheckBox.name = "Checkbox";
            policyCheckBox.size = new Vector2(363f, 44f);
            policyCheckBox.relativePosition = new Vector3(0f, -2f, 0f);
            policyCheckBox.clipChildren = true;

            UISprite sprite = policyCheckBox.AddUIComponent<UISprite>();
            sprite.name = "Unchecked";
            sprite.spriteName = "ToggleBase";
            sprite.size = new Vector2(16f, 16f);
            sprite.relativePosition = new Vector3(336.6984f,14,0f);

            policyCheckBox.checkedBoxObject = sprite.AddUIComponent<UISprite>();
            policyCheckBox.checkedBoxObject.name = "Checked";
            ((UISprite)policyCheckBox.checkedBoxObject).spriteName = "ToggleBaseFocused";
            policyCheckBox.checkedBoxObject.size = new Vector2(16f, 16f);
            policyCheckBox.checkedBoxObject.relativePosition = Vector3.zero;

            // TODO link the checkbox and the focus of the button (like PolicyContainer component does)
        }

    }

}
