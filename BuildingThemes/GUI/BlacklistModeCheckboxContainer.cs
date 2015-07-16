using ColossalFramework;
using ColossalFramework.UI;

namespace BuildingThemes.GUI
{
    // This helper component updates the "Blacklist Mode" checkbox state in the policy panel every game tick.
    public class BlacklistModeCheckboxContainer : ToolsModifierControl
    {
        private UICheckBox m_Check;

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
                    var districtId = ToolsModifierControl.policiesPanel.targetDistrict;

                    bool blacklistMode = BuildingThemesManager.instance.IsBlacklistModeEnabled(districtId);

                    if (blacklistMode != this.m_Check.isChecked)
                    {
                        this.m_Check.isChecked = blacklistMode;
                    }
                    bool managed = BuildingThemesManager.instance.IsThemeManagementEnabled(districtId);

                    this.m_Check.opacity = (!managed) ? 0.5f : 1f;
                    this.m_Check.isEnabled = managed;
                }
            }
        }
    }
}