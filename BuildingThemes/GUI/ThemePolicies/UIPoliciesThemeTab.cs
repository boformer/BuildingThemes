using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BuildingThemes.GUI
{
    public class UIPoliciesThemeTab
    {
        // Themes tab GUI Helpers

        private static UIButton tab;
        private static UIPanel container;
        private static UIPanel controls;
        private static FastList<UIPanel> themePolicyButtons = new FastList<UIPanel>();

        public static void AddThemesTab()
        {
            if (container != null)
            {
                return;
            }

            UITabstrip tabstrip = ToolsModifierControl.policiesPanel.Find("Tabstrip") as UITabstrip;

            // Add a custom tab
            tab = tabstrip.AddTab("Themes");
            tab.stringUserData = "Themes";
            tab.textScale = 0.875f;

            // recalculate the width of the tabs
            for (int i = 0; i < tabstrip.tabCount; i++)
            {
                tabstrip.tabs[i].width = tabstrip.width / ((float)tabstrip.tabCount - 1);
            }

            // The container for the policies was created by the game when we added the tab
            var pageIndex = tabstrip.tabPages.childCount - 1;
            container = (UIPanel)tabstrip.tabPages.components[pageIndex];

            container.autoLayout = true;
            container.autoLayoutDirection = LayoutDirection.Vertical;
            container.autoLayoutPadding.top = 5;

            // Only make the container visible if our tab was selected when the panel was closed last time
            container.isVisible = tabstrip.selectedIndex == pageIndex;

            // Add the theme buttons
            themePolicyButtons.Clear();
            RefreshThemesContainer();

            // The panel holding the controls
            controls = container.AddUIComponent<UIPanel>();

            controls.width = container.width;
            controls.autoLayout = true;
            controls.autoLayoutDirection = LayoutDirection.Vertical;
            controls.autoLayoutPadding.top = 5;

            // Add a checkbox to toggle "Blacklist Mode"
            UICheckBox blacklistModeCheckBox = CreateCheckBox(controls);
            blacklistModeCheckBox.name = "Blacklist Mode Checkbox";
            blacklistModeCheckBox.gameObject.AddComponent<BlacklistModeCheckboxContainer>();
            blacklistModeCheckBox.text = "Allow buildings which are not in any theme";
            blacklistModeCheckBox.isChecked = false;

            blacklistModeCheckBox.eventCheckChanged += delegate(UIComponent component, bool isChecked)
            {
                lock (component)
                {
                    var districtId1 = ToolsModifierControl.policiesPanel.targetDistrict;

                    Singleton<BuildingThemesManager>.instance.ToggleBlacklistMode(districtId1, isChecked);
                }

            };

            // Add a checkbox to "Enable Theme Management for this district"
            UICheckBox enableThemeManagementCheckBox = CreateCheckBox(controls);
            enableThemeManagementCheckBox.name = "Theme Management Checkbox";
            enableThemeManagementCheckBox.gameObject.AddComponent<ThemeManagementCheckboxContainer>();
            enableThemeManagementCheckBox.text = "Enable Theme Management for this district";
            enableThemeManagementCheckBox.isChecked = false;

            enableThemeManagementCheckBox.eventCheckChanged += delegate(UIComponent component, bool isChecked)
            {
                lock (component)
                {
                    var districtId1 = ToolsModifierControl.policiesPanel.targetDistrict;

                    Singleton<BuildingThemesManager>.instance.ToggleThemeManagement(districtId1, isChecked);
                }

            };

            // Add a button to show the Building Theme Manager
            UIButton showThemeManager = GUI.UIUtils.CreateButton(controls);
            showThemeManager.width = controls.width;
            showThemeManager.text = "Theme Manager";

            showThemeManager.eventClick += (c, p) => GUI.UIThemeManager.instance.Toggle();
        }

        // This method has to be called when the theme list was modified!
        public static void RefreshThemesContainer()
        {
            if (container == null)
            {
                return;
            }

            // remove the existing PolicyButtons
            for (int i = 0; i < themePolicyButtons.m_size; i++)
            {
                GameObject.Destroy(themePolicyButtons[i].gameObject);
            }
            themePolicyButtons.Clear();

            // Add the theme buttons
            foreach (Configuration.Theme theme in Singleton<BuildingThemesManager>.instance.GetAllThemes())
            {
                AddThemePolicyButton(container, theme);
            }

            if (controls != null) controls.BringToFront();
        }

        public static void RemoveThemesTab()
        {
            UITabstrip tabstrip = ToolsModifierControl.policiesPanel.Find("Tabstrip") as UITabstrip;

            if (tab != null)
            {
                tabstrip.RemoveUIComponent(tab);
                GameObject.Destroy(tab.gameObject);
                tab = null;
            }

            if (container != null)
            {
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

            themePolicyButtons.Add(policyPanel);

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

            // This helper component updates the checkbox state every game tick
            policyButton.gameObject.AddComponent<ThemePolicyContainer>();

            UICheckBox policyCheckBox = policyButton.AddUIComponent<UICheckBox>();
            policyCheckBox.name = "Checkbox";
            policyCheckBox.size = new Vector2(363f, 44f);
            policyCheckBox.relativePosition = new Vector3(0f, -2f, 0f);
            policyCheckBox.clipChildren = true;
            policyCheckBox.objectUserData = theme;

            // Check if theme is enabled in selected district and set the checkbox
            var districtId = ToolsModifierControl.policiesPanel.targetDistrict;
            var districtThemes = Singleton<BuildingThemesManager>.instance.GetDistrictThemes(districtId, true);
            policyCheckBox.isChecked = districtThemes.Contains(theme);

            // Connect the checkbox with our theme manager
            policyCheckBox.eventCheckChanged += delegate(UIComponent component, bool isChecked)
            {
                lock (component)
                {
                    var districtId1 = ToolsModifierControl.policiesPanel.targetDistrict;

                    if (isChecked)
                    {
                        Singleton<BuildingThemesManager>.instance.EnableTheme(districtId1, theme);
                    }
                    else
                    {
                        Singleton<BuildingThemesManager>.instance.DisableTheme(districtId1, theme);
                    }
                }

            };

            // Checkbox-related UI components
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
        }

        public static UICheckBox CreateCheckBox(UIComponent parent)
        {
            UICheckBox checkBox = (UICheckBox)parent.AddUIComponent<UICheckBox>();

            checkBox.width = 364f;
            checkBox.height = 20f;
            checkBox.clipChildren = true;

            UISprite sprite = checkBox.AddUIComponent<UISprite>();
            sprite.spriteName = "ToggleBase";
            sprite.size = new Vector2(16f, 16f);
            sprite.relativePosition = Vector3.zero;

            checkBox.checkedBoxObject = sprite.AddUIComponent<UISprite>();
            ((UISprite)checkBox.checkedBoxObject).spriteName = "ToggleBaseFocused";
            checkBox.checkedBoxObject.size = new Vector2(16f, 16f);
            checkBox.checkedBoxObject.relativePosition = Vector3.zero;

            checkBox.label = checkBox.AddUIComponent<UILabel>();
            checkBox.label.text = " ";
            checkBox.label.textScale = 0.9f;
            checkBox.label.relativePosition = new Vector3(22f, 2f);

            return checkBox;
        }
    }
}

