using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BuildingThemes.GUI
{
    public class ThemePolicyTab
    {
        // Themes tab GUI Helpers

        private static UIButton tab;
        private static UIPanel container;
        private static UIPanel controls;
        private static UIFastList themePolicyButtons;

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

            // Theme buttons
            themePolicyButtons = UIFastList.Create<UIThemePolicyItem>(container);
            themePolicyButtons.width = 364f;
            themePolicyButtons.rowHeight = 49f;
            themePolicyButtons.autoHideScrollbar = true;

            // The panel holding the controls
            controls = container.AddUIComponent<UIPanel>();

            controls.width = container.width;
            controls.height = 100f;
            controls.autoLayout = true;
            controls.autoLayoutDirection = LayoutDirection.Vertical;
            controls.autoLayoutPadding.top = 5;

            // Add a checkbox to toggle "Blacklist Mode"
            UICheckBox blacklistModeCheckBox = CreateCheckBox(controls);
            blacklistModeCheckBox.name = "Blacklist Mode Checkbox";
            blacklistModeCheckBox.gameObject.AddComponent<BlacklistModeCheckboxContainer>();
            blacklistModeCheckBox.text = "Only allow buildings not in selected themes";
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

            RefreshThemesContainer();
        }

        // This method has to be called when the theme list was modified!
        public static void RefreshThemesContainer()
        {
            if (container == null)
            {
                return;
            }

            themePolicyButtons.rowsData.m_buffer = BuildingThemesManager.instance.GetAllThemes().ToArray();
            themePolicyButtons.rowsData.m_size = themePolicyButtons.rowsData.m_buffer.Length;
            Array.Sort(themePolicyButtons.rowsData.m_buffer as Configuration.Theme[], ThemeCompare);

            controls.autoSize = true;
            themePolicyButtons.height = Mathf.Min(themePolicyButtons.rowsData.m_size * themePolicyButtons.rowHeight, container.height - controls.height - 5);
            themePolicyButtons.Refresh();

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

        private static int ThemeCompare(Configuration.Theme a, Configuration.Theme b)
        {
            // Sort by name
            return a.name.CompareTo(b.name);
        }
    }
}

