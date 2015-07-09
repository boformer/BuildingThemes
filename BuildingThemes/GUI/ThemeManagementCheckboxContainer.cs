using ColossalFramework;
using ColossalFramework.UI;

namespace BuildingThemes.GUI
{
    public class ThemeManagementCheckboxContainer : ToolsModifierControl
    {
        private UICheckBox m_Check;

        private const string TEXT_CITY = "Enable Theme Management for this city";
        private const string TEXT_DISTRICT = "Enable Theme Management for this district";

        private void Start()
        {
            this.m_Check = base.GetComponent<UICheckBox>();
        }

        private void Update()
        {
            if (base.component.isVisible)
            {
                lock (m_Check)
                {
                    ushort districtId = (ushort)ToolsModifierControl.policiesPanel.targetDistrict;
                    var theme = (Configuration.Theme)m_Check.objectUserData;

                    bool managed = BuildingThemesManager.instance.IsThemeManagementEnabled(districtId);

                    m_Check.text = districtId == 0 ? TEXT_CITY : TEXT_DISTRICT;

                    if (managed != this.m_Check.isChecked)
                    {
                        this.m_Check.isChecked = managed;
                    }
                }
            }
        }
    }
}