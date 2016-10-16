using System.Reflection;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;
using BuildingThemes.GUI;
using System;

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

                Debugger.Log("Building Themes: PoliciesPanel Methods detoured!");
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

                Debugger.Log("Building Themes: PoliciesPanel Methods restored!");
            }
        }


        // Detours

        public void SetParentButton(UIButton button)
        {
            if (button == null) return;
            
            // We have to remove the custom tab before the original SetParentButton method is called
            // SetParentButton() is searching for a TutorialUITag component which our tab does not have
            GUI.ThemePolicyTab.RemoveThemesTab();

            // Call the original method
            RedirectionHelper.RevertRedirect(_PoliciesPanel_SetParentButton_original, _PoliciesPanel_SetParentButton_state);
            try
            {
                _PoliciesPanel_SetParentButton_original.Invoke(this, new object[] { button });
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                // ignore this error, seems harmless
            }
            RedirectionHelper.RedirectCalls(_PoliciesPanel_SetParentButton_original, _PoliciesPanel_SetParentButton_detour);

            
            // After the method call, add our custom tab again
            GUI.ThemePolicyTab.AddThemesTab();
        }

        private void RefreshPanel()
        {
            // We have to remove the custom tab before the original RefreshPanel method is called
            // RefreshPanel() checks for every policy button if the assigned policy is loaded
            // Our fake policy buttons are not related to a game policy
            GUI.ThemePolicyTab.RemoveThemesTab();

            // Call the original method
            RedirectionHelper.RevertRedirect(_PoliciesPanel_RefreshPanel_original, _PoliciesPanel_RefreshPanel_state);
            try
            {
                _PoliciesPanel_RefreshPanel_original.Invoke(this, new object[] { });
            }
            catch(Exception e)
            {
                Debugger.LogException(e);
            }
            RedirectionHelper.RedirectCalls(_PoliciesPanel_RefreshPanel_original, _PoliciesPanel_RefreshPanel_detour);

            // After the method call, add our custom tab again
            GUI.ThemePolicyTab.AddThemesTab();
        }
    }
}
