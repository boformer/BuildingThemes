using BuildingThemes.Redirection;
using ColossalFramework.UI;
using UnityEngine;

namespace BuildingThemes.Detour
{
    [TargetType(typeof(DistrictWorldInfoPanel))]
    public class DistrictWorldInfoPanelDetour : DistrictWorldInfoPanel
    {
        [RedirectMethod]
        public new void OnPoliciesClick()
        {
            if ((Object)ToolsModifierControl.GetTool<DistrictTool>() != (Object)ToolsModifierControl.GetCurrentTool<DistrictTool>())
                ToolsModifierControl.keepThisWorldInfoPanel = true;
            ToolsModifierControl.mainToolbar.ShowPoliciesPanel(this.m_InstanceID.District);

            //begin mod
            UIView.Find<UIPanel>("PoliciesPanel").Find<UITabstrip>("Tabstrip").selectedIndex = 0;
            //end mod
        }
    }
}