using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace BuildingThemes
{
    public class PoliciesPanelDetour
    {

        public static RedirectCallsState setParentButtonState;
        public static MethodInfo setParentButton;
        public static MethodInfo setParentButtonDetour;

        public static RedirectCallsState refreshPanelState;
        public static MethodInfo refreshPanel;
        public static MethodInfo refreshPanelDetour;
        public void SetParentButton(UIButton button)
        {
            //UnityEngine.Debug.LogFormat("PoliciesPanel.SetParentButton called");

            RemoveThemesTab();

            RedirectionHelper.RevertRedirect(setParentButton, setParentButtonState);
            setParentButton.Invoke(this, new object[] { button });
            RedirectionHelper.RedirectCalls(setParentButton, setParentButtonDetour);

            AddThemesTab();

            //UnityEngine.Debug.LogFormat("PoliciesPanel.SetParentButton done");
        }

        private void RefreshPanel()
        {
            //UnityEngine.Debug.LogFormat("PoliciesPanel.RefreshPanel called");

            RemoveThemesTab();

            RedirectionHelper.RevertRedirect(refreshPanel, refreshPanelState);
            refreshPanel.Invoke(this, new object[] {});
            RedirectionHelper.RedirectCalls(refreshPanel, refreshPanelDetour);

            AddThemesTab();

            //UnityEngine.Debug.LogFormat("PoliciesPanel.RefreshPanel done");
        }

        private static UIButton tab;
        private static UIPanel container;

        public static void AddThemesTab()
        {
            if (container != null)
            {
                return;
            }
            
            //UnityEngine.Debug.LogFormat("Adding Tab");
            
            UITabstrip tabstrip = ToolsModifierControl.policiesPanel.Find("Tabstrip") as UITabstrip;
            tab = tabstrip.AddTab("Themes");
            tab.stringUserData = "Themes";

            // recalculate the width of the tabs
            for (int i = 0; i < tabstrip.tabCount; i++)
            {
                tabstrip.tabs[i].width = tabstrip.width / ((float)tabstrip.tabCount - 1);
            }

            //UnityEngine.Debug.LogFormat("tabcount:" + tabstrip.tabCount);

            container = (UIPanel) tabstrip.tabPages.components[tabstrip.tabPages.childCount - 1];

            container.autoLayout = true;
            container.autoLayoutDirection = LayoutDirection.Vertical;
            container.autoLayoutPadding.top = 5;

            container.isVisible = tabstrip.selectedIndex == 4;

            RefreshThemesContainer();
        }

        public static void RefreshThemesContainer()
        {
            if(container == null) 
            {
                return;
            }

            // remove the existing stuff if something is in there
            foreach (Transform child in container.gameObject.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            
            foreach (Configuration.Theme theme in Singleton<BuildingThemesManager>.instance.GetAllThemes())
            {
                AddThemePolicyButton(container, theme);
            }
        }

        public static void RemoveThemesTab()
        {
            //UnityEngine.Debug.LogFormat("Removing...");

            UITabstrip tabstrip = ToolsModifierControl.policiesPanel.Find("Tabstrip") as UITabstrip;
            
            if (tab != null) 
            {
                //UnityEngine.Debug.LogFormat("... tab");
                
                tabstrip.RemoveUIComponent(tab);
                GameObject.Destroy(tab.gameObject);
                tab = null;
            }

            if (container != null) 
            {
                //UnityEngine.Debug.LogFormat("... container");

                tabstrip.tabPages.RemoveUIComponent(container);
                GameObject.Destroy(container.gameObject);
                container = null;

            }
        }

        private static void AddThemePolicyButton(UIPanel container, Configuration.Theme theme)
        {

            UIPanel policyPanel = container.AddUIComponent<UIPanel>();
            policyPanel.name = theme.name;
            policyPanel.backgroundSprite = "GenericPanel";
            policyPanel.size = new Vector2(364f, 44f);
            policyPanel.objectUserData = ToolsModifierControl.policiesPanel;

            UIButton policyButton = policyPanel.AddUIComponent<UIButton>();
            policyButton.name = "PolicyButton";
            policyButton.text = theme.name;
            policyButton.size = new Vector2(324f, 40f);
            policyButton.focusedBgSprite = "PolicyBarBackActive";
            policyButton.normalBgSprite = "PolicyBarBack";
            policyButton.relativePosition = new Vector3(2f, 2f, 0f);
            policyButton.textPadding.left = 50;
            policyButton.textColor = new Color32(0, 0, 0, 255);
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
            policyButton.gameObject.AddComponent<ThemePolicyContainer>();

            UICheckBox policyCheckBox = policyButton.AddUIComponent<UICheckBox>();
            policyCheckBox.name = "Checkbox";
            policyCheckBox.size = new Vector2(363f, 44f);
            policyCheckBox.relativePosition = new Vector3(0f, -2f, 0f);
            policyCheckBox.clipChildren = true;
            policyCheckBox.objectUserData = theme;

            ushort districtId1 = (ushort)ToolsModifierControl.policiesPanel.targetDistrict;

            var districtThemes = Singleton<BuildingThemesManager>.instance.GetDistrictThemes(districtId1, true);
            policyCheckBox.isChecked = districtThemes.Contains(theme);


            policyCheckBox.eventCheckChanged += delegate(UIComponent component, bool enabled)
            {
                lock (component)
                {
                    uint districtId = (uint)ToolsModifierControl.policiesPanel.targetDistrict;
                    if (enabled)
                    {
                        Singleton<BuildingThemesManager>.instance.EnableTheme(districtId, theme, true);
                        if (BuildingThemesMod.isDebug)
                        {
                            Debug.Log("enabled theme " + theme.name + " in district " + districtId);
                        }
                    }
                    else
                    {
                        Singleton<BuildingThemesManager>.instance.DisableTheme(districtId, theme.name, true);
                        if (BuildingThemesMod.isDebug)
                        {
                            Debug.Log("disabled theme " + theme.name + " in district " + districtId);
                        }
                    }
                }

            };


            UISprite sprite = policyCheckBox.AddUIComponent<UISprite>();
            sprite.name = "Unchecked";
            sprite.spriteName = "ToggleBase";
            sprite.size = new Vector2(16f, 16f);
            sprite.relativePosition = new Vector3(336.6984f, 14, 0f);

            policyCheckBox.checkedBoxObject = sprite.AddUIComponent<UISprite>();
            policyCheckBox.checkedBoxObject.name = "Checked";
            ((UISprite)policyCheckBox.checkedBoxObject).spriteName = "ToggleBaseFocused";
            policyCheckBox.checkedBoxObject.size = new Vector2(16f, 16f);
            policyCheckBox.checkedBoxObject.relativePosition = Vector3.zero;

            // TODO link the checkbox and the focus of the button (like PolicyContainer component does)
        }
    }
}
