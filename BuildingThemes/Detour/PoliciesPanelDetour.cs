using System.Reflection;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;
using BuildingThemes.GUI;

namespace BuildingThemes.Detour
{
    // This detour hooks into the Policy Panel to display the 'Themes' tab
    public class PoliciesPanelDetour
    {
        private static bool deployed = false;

        private static RedirectCallsState _PoliciesPanel_RefreshPanel_state;
        private static MethodInfo _PoliciesPanel_RefreshPanel_original;
        private static MethodInfo _PoliciesPanel_RefreshPanel_detour;

        private static RedirectCallsState _PoliciesPanel_SetParentButton_state;
        private static MethodInfo _PoliciesPanel_SetParentButton_original;
        private static MethodInfo _PoliciesPanel_SetParentButton_detour;

        public static void Deploy()
        {
            if (!deployed)
            {
                _PoliciesPanel_RefreshPanel_original = typeof(PoliciesPanel).GetMethod("RefreshPanel", BindingFlags.Instance | BindingFlags.NonPublic);
                _PoliciesPanel_RefreshPanel_detour = typeof(PoliciesPanelDetour).GetMethod("RefreshPanel", BindingFlags.Instance | BindingFlags.NonPublic);
                _PoliciesPanel_RefreshPanel_state = RedirectionHelper.RedirectCalls(_PoliciesPanel_RefreshPanel_original, _PoliciesPanel_RefreshPanel_detour);

                _PoliciesPanel_SetParentButton_original = typeof(PoliciesPanel).GetMethod("SetParentButton", BindingFlags.Instance | BindingFlags.Public);
                _PoliciesPanel_SetParentButton_detour = typeof(PoliciesPanelDetour).GetMethod("SetParentButton", BindingFlags.Instance | BindingFlags.Public);
                _PoliciesPanel_SetParentButton_state = RedirectionHelper.RedirectCalls(_PoliciesPanel_SetParentButton_original, _PoliciesPanel_SetParentButton_detour);

                deployed = true;

                Debug.Log("Building Themes: PoliciesPanel Methods detoured!");
            }
        }

        public static void Revert()
        {
            if (deployed)
            {
                RedirectionHelper.RevertRedirect(_PoliciesPanel_RefreshPanel_original, _PoliciesPanel_RefreshPanel_state);
                _PoliciesPanel_RefreshPanel_original = null;
                _PoliciesPanel_RefreshPanel_detour = null;

                RedirectionHelper.RevertRedirect(_PoliciesPanel_SetParentButton_original, _PoliciesPanel_SetParentButton_state);
                _PoliciesPanel_SetParentButton_original = null;
                _PoliciesPanel_SetParentButton_detour = null;

                deployed = false;

                Debug.Log("Building Themes: PoliciesPanel Methods restored!");
            }
        }


        // Detours

        public void SetParentButton(UIButton button)
        {
            // We have to remove the custom tab before the original SetParentButton method is called
            // SetParentButton() is searching for a TutorialUITag component which our tab does not have
            RemoveThemesTab();

            // Call the original method
            RedirectionHelper.RevertRedirect(_PoliciesPanel_SetParentButton_original, _PoliciesPanel_SetParentButton_state);
            _PoliciesPanel_SetParentButton_original.Invoke(this, new object[] { button });
            RedirectionHelper.RedirectCalls(_PoliciesPanel_SetParentButton_original, _PoliciesPanel_SetParentButton_detour);

            // After the method call, add our custom tab again
            AddThemesTab();
        }

        private void RefreshPanel()
        {
            // We have to remove the custom tab before the original RefreshPanel method is called
            // RefreshPanel() checks for every policy button if the assigned policy is loaded
            // Our fake policy buttons are not related to a game policy
            RemoveThemesTab();

            // Call the original method
            RedirectionHelper.RevertRedirect(_PoliciesPanel_RefreshPanel_original, _PoliciesPanel_RefreshPanel_state);
            _PoliciesPanel_RefreshPanel_original.Invoke(this, new object[] { });
            RedirectionHelper.RedirectCalls(_PoliciesPanel_RefreshPanel_original, _PoliciesPanel_RefreshPanel_detour);

            // After the method call, add our custom tab again
            AddThemesTab();
        }


        // Themes tab GUI Helpers

        private static UIButton tab;
        private static UIPanel container;

        private static void AddThemesTab()
        {
            if (container != null)
            {
                return;
            }

            // Add a custom tab
            UITabstrip tabstrip = ToolsModifierControl.policiesPanel.Find("Tabstrip") as UITabstrip;
            tab = tabstrip.AddTab("Themes");
            tab.stringUserData = "Themes";

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
            RefreshThemesContainer();
        }

        // This method has to be called when the theme list was modified!
        public static void RefreshThemesContainer()
        {
            if (container == null)
            {
                return;
            }

            // remove the existing stuff if something is in there
            foreach (Transform child in container.gameObject.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            // Add the theme buttons
            foreach (Configuration.Theme theme in Singleton<BuildingThemesManager>.instance.GetAllThemes())
            {
                AddThemePolicyButton(container, theme);
            }
        }

        private static void RemoveThemesTab()
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
            var districtId = (ushort)ToolsModifierControl.policiesPanel.targetDistrict;
            var districtThemes = Singleton<BuildingThemesManager>.instance.GetDistrictThemes(districtId, true);
            policyCheckBox.isChecked = districtThemes.Contains(theme);

            // Connect the checkbox with our theme manager
            policyCheckBox.eventCheckChanged += delegate(UIComponent component, bool isChecked)
            {
                lock (component)
                {
                    var districtId1 = (uint)ToolsModifierControl.policiesPanel.targetDistrict;

                    if (isChecked)
                    {
                        Singleton<BuildingThemesManager>.instance.EnableTheme(districtId1, theme, true);
                    }
                    else
                    {
                        Singleton<BuildingThemesManager>.instance.DisableTheme(districtId1, theme.name, true);
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
    }
}
