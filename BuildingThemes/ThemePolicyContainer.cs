using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework;
using UnityEngine;
using ColossalFramework.UI;

namespace BuildingThemes
{
    class ThemePolicyContainer : ToolsModifierControl
    {
        private UIButton m_Button;
        private UICheckBox m_Check;
        private void Start()
        {
            this.m_Button = base.Find<UIButton>("PolicyButton");
            this.m_Check = base.Find<UICheckBox>("Checkbox");
        }
        private void Update()
        {
            if (base.component.isVisible)
            {
                lock (m_Check) {
                    ushort districtId = (ushort)ToolsModifierControl.policiesPanel.targetDistrict;
                    var theme = (Configuration.Theme)m_Check.objectUserData;
                    if (Singleton<BuildingThemesManager>.instance.GetDistrictThemes(districtId, true).Contains(theme) != this.m_Check.isChecked) 
                    {
                        this.m_Check.isChecked = !this.m_Check.isChecked;
                        UnityEngine.Debug.LogFormat("Building Themes: ThemePolicyContainer. Chacnging theme {0} checkbox for district {1} to state: {2}",
                            theme.name, districtId, this.m_Check.isChecked);
                    }
                    this.m_Button.state = ((!this.m_Check.isEnabled) ? UIButton.ButtonState.Disabled : ((!this.m_Check.isChecked) ? UIButton.ButtonState.Normal : UIButton.ButtonState.Focused));
                 }
            
            }
        }
    }
}