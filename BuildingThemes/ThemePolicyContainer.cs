using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                ushort districtId = (ushort)ToolsModifierControl.policiesPanel.targetDistrict;
                if (BuildingThemesMod.districtThemes[districtId].Contains((Configuration.Theme)m_Check.objectUserData) != this.m_Check.isChecked) 
                {
                    this.m_Check.isChecked = !this.m_Check.isChecked;
                }
            
                this.m_Button.state = ((!this.m_Check.isEnabled) ? UIButton.ButtonState.Disabled : ((!this.m_Check.isChecked) ? UIButton.ButtonState.Normal : UIButton.ButtonState.Focused));
		    }
	    }
    }
}
