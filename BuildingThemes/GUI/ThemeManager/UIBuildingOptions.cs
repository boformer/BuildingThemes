using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace BuildingThemes.GUI
{
    public class UIBuildingOptions : UIPanel
    {
        private UILabel m_noOption;

        private UICheckBox m_include;
        private UITextField m_spawnRate;

        private UITextField m_baseName;
        private UITextField m_upgradeName;

        private BuildingItem m_item;
        private BuildingItem m_upgradeBuilding;
        private BuildingItem m_baseBuilding;

        public override void Start()
        {
            base.Start();

            isVisible = true;
            canFocus = true;
            isInteractive = true;
            backgroundSprite = "UnlockingPanel";
            padding = new RectOffset(5, 5, 5, 0);

            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            autoLayoutPadding.top = 5;

            SetupControls();
        }

        private void SetupControls()
        {
            if (m_noOption != null) return;

            // No option available
            m_noOption = AddUIComponent<UILabel>();
            m_noOption.textScale = 0.9f;
            m_noOption.text = "No option available";

            // Include
            m_include = UIUtils.CreateCheckBox(this);
            m_include.text = "Include";
            m_include.isVisible = false;

            m_include.eventCheckChanged += (c, state) =>
            {
                UIThemeManager.instance.ChangeBuildingStatus(m_item, state);
                Show(m_item);
            };

            // Spawn rate
            UIPanel spawnRatePanel = AddUIComponent<UIPanel>();
            spawnRatePanel.height = 25;
            spawnRatePanel.isVisible = false;

            UILabel spawnRateLabel = spawnRatePanel.AddUIComponent<UILabel>();
            spawnRateLabel.textScale = 0.9f;
            spawnRateLabel.text = "Spawn rate:";
            spawnRateLabel.relativePosition = new Vector3(0, 5);

            m_spawnRate = UIUtils.CreateTextField(spawnRatePanel);
            m_spawnRate.size = new Vector2(60, 25);
            m_spawnRate.padding = new RectOffset(6, 6, 6, 0);
            m_spawnRate.numericalOnly = true;
            m_spawnRate.tooltip = "The higher the number, the more the building is likely to spawn.\nDefault value is 10. Maximum value is 100.";
            m_spawnRate.relativePosition = new Vector3(width - 70, 0);

            // Upgrade Name
            UIPanel upgradeNamePanel = AddUIComponent<UIPanel>();
            upgradeNamePanel.height = 25;
            upgradeNamePanel.isVisible = false;

            UILabel upgradeNameLabel = upgradeNamePanel.AddUIComponent<UILabel>();
            upgradeNameLabel.textScale = 0.9f;
            upgradeNameLabel.text = "Upgrade:";
            upgradeNameLabel.relativePosition = new Vector3(0, 5);

            m_upgradeName = UIUtils.CreateTextField(upgradeNamePanel);
            m_upgradeName.size = new Vector2(width - 10, 25);
            m_upgradeName.padding = new RectOffset(6, 6, 6, 0);
            m_upgradeName.tooltip = "Name of the building to spawn when upgraded.\nLeave empty for random spawn.";
            m_upgradeName.relativePosition = new Vector3(0, 25);

            m_upgradeName.eventMouseEnter += (c, p) => UIThemeManager.instance.buildingPreview.Show(m_upgradeBuilding);
            m_upgradeName.eventMouseLeave += (c, p) => UIThemeManager.instance.buildingPreview.Show(m_item);

            // Base Name
            UIPanel baseNamePanel = AddUIComponent<UIPanel>();
            baseNamePanel.height = 25;
            baseNamePanel.isVisible = false;

            UILabel baseNameLabel = baseNamePanel.AddUIComponent<UILabel>();
            baseNameLabel.textScale = 0.9f;
            baseNameLabel.text = "Base:";
            baseNameLabel.relativePosition = new Vector3(0, 5);

            m_baseName = UIUtils.CreateTextField(baseNamePanel);
            m_baseName.size = new Vector2(width - 10, 25);
            m_baseName.padding = new RectOffset(6, 6, 6, 0);
            m_baseName.isEnabled = false;
            m_baseName.tooltip = "Name of the original building.";
            m_baseName.relativePosition = new Vector3(0, 25);

            m_baseName.eventMouseEnter += (c, p) => UIThemeManager.instance.buildingPreview.Show(m_baseBuilding);
            m_baseName.eventMouseLeave += (c, p) => UIThemeManager.instance.buildingPreview.Show(m_item);
        }

        public void Show(BuildingItem item)
        {
            m_item = item;

            m_noOption.isVisible = false;
            m_include.isVisible = false;
            m_spawnRate.parent.isVisible = false;
            m_upgradeName.parent.isVisible = false;
            m_baseName.parent.isVisible = false;

            if (m_item == null)
            {
                m_noOption.isVisible = true;
                return;
            }

            m_include.isVisible = true;
            m_include.isChecked = m_item.included;

            if (m_item.included == false) return;

            m_spawnRate.parent.isVisible = true;
            m_spawnRate.text = m_item.building.spawnRate.ToString();
            
            m_upgradeName.parent.isVisible = true;
            m_upgradeName.text = "";
            m_upgradeBuilding = null;

            if (m_item.building.upgradeName != null)
            {
                m_upgradeBuilding = UIThemeManager.instance.GetBuildingItem(m_item.building.upgradeName);
                if (m_upgradeBuilding != null) m_upgradeName.text = m_upgradeBuilding.displayName;
            }

            if (m_item.building.baseName != null)
            {
                m_baseBuilding = UIThemeManager.instance.GetBuildingItem(m_item.building.baseName);
                if (m_baseBuilding != null) m_baseName.text = m_baseBuilding.displayName;
                m_baseName.parent.isVisible = true;
            }
        }
    }
}
