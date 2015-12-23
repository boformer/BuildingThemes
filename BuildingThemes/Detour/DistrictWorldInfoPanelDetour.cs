using System.Reflection;
using ColossalFramework.UI;
using UnityEngine;

namespace BuildingThemes.Detour
{
    public class DistrictWorldInfoPanelDetour : DistrictWorldInfoPanel
    {

        private static bool deployed = false;

        private static RedirectCallsState _state;
        private static MethodInfo _original;
        private static MethodInfo _detour;

        public static void Deploy()
        {
            if (!deployed)
            {
                _original = typeof(DistrictWorldInfoPanel).GetMethod("OnPoliciesClick", BindingFlags.Instance | BindingFlags.Public);
                _detour = typeof(DistrictWorldInfoPanelDetour).GetMethod("OnPoliciesClick", BindingFlags.Instance | BindingFlags.Public);
                _state = RedirectionHelper.RedirectCalls(_original, _detour);

                deployed = true;

                Debugger.Log("Building Themes: DistrictWorldInfoPanel Methods detoured!");
            }
        }

        public static void Revert()
        {
            if (deployed)
            {
                RedirectionHelper.RevertRedirect(_original, _state);
                _original = null;
                _detour = null;

                deployed = false;

                Debugger.Log("Building Themes: DistrictWorldInfoPanel Methods restored!");
            }
        }

        public new void OnPoliciesClick()
        {
            if ((Object)ToolsModifierControl.GetTool<DistrictTool>() != (Object)ToolsModifierControl.GetCurrentTool<DistrictTool>())
                ToolsModifierControl.keepThisWorldInfoPanel = true;
            ToolsModifierControl.mainToolbar.ShowPoliciesPanel(this.m_InstanceID.District);
            UIView.Find<UIPanel>("PoliciesPanel").Find<UITabstrip>("Tabstrip").selectedIndex = 0;
        }
    }
}