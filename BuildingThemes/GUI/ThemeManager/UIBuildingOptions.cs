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
        private UIFastList m_dropDownList;

        private static UIBuildingOptions _instance;

        public static UIBuildingOptions instance
        {
            get { return _instance; }
        }

        public override void Start()
        {
            base.Start();

            _instance = this;

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
            upgradeNamePanel.height = 50;
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

            m_upgradeName.eventMouseEnter += (c, p) =>
            {
                if (!m_upgradeName.hasFocus && m_upgradeBuilding != null)
                    UIThemeManager.instance.buildingPreview.Show(m_upgradeBuilding);
            };

            m_upgradeName.eventMouseLeave += (c, p) =>
            {
                UIThemeManager.instance.buildingPreview.Show(m_item);
            };

            m_upgradeName.eventEnterFocus += (c, p) =>
            {
                if (!m_upgradeName.text.IsNullOrWhiteSpace())
                    ShowDropDown();
            };

            m_upgradeName.eventTextChanged += (c, name) =>
            {
                if (m_upgradeName.hasFocus && !name.IsNullOrWhiteSpace())
                    ShowDropDown();
            };

            m_upgradeName.eventTextSubmitted += (c, name) =>
            {
                if (m_dropDownList == null || !m_dropDownList.isVisible)
                    UIThemeManager.instance.ChangeUpgradeBuilding(null);
                else
                    HideDropDown();

                Show(m_item);
            };

            // Base Name
            UIPanel baseNamePanel = AddUIComponent<UIPanel>();
            baseNamePanel.height = 50;
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

        public override void Update()
        {
            base.Update();

            if (m_dropDownList == null || !m_dropDownList.isVisible) return;

            if (!m_upgradeName.hasFocus)
                HideDropDown();

            if (Input.GetKeyUp(KeyCode.DownArrow))
            {
                m_dropDownList.selectedIndex = Mathf.Clamp(m_dropDownList.selectedIndex + 1, 0, m_dropDownList.rowsData.m_size);

                float max = m_dropDownList.listPosition + m_dropDownList.height / 30;

                if(m_dropDownList.selectedIndex >= max)
                {
                    m_dropDownList.DisplayAt(m_dropDownList.listPosition + 1f);
                }
            }
            if (Input.GetKeyUp(KeyCode.UpArrow))
            {
                m_dropDownList.selectedIndex = Mathf.Clamp(m_dropDownList.selectedIndex - 1, 0, m_dropDownList.rowsData.m_size);

                float min = m_dropDownList.listPosition;

                if (m_dropDownList.selectedIndex < min)
                {
                    m_dropDownList.DisplayAt(m_dropDownList.listPosition - 1f);
                }
            }
            if (Input.GetKeyUp(KeyCode.Return))
            {
                UIThemeManager.instance.ChangeUpgradeBuilding(m_dropDownList.selectedItem as BuildingItem);
                HideDropDown();
                Show(m_item);
            }
            if (Input.GetKeyUp(KeyCode.Backspace) || Input.GetKeyUp(KeyCode.Delete))
            {
                if (m_upgradeName.text.IsNullOrWhiteSpace()) HideDropDown();
            }
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

            m_upgradeName.parent.isVisible = m_item.level < m_item.maxLevel;
            m_upgradeName.text = "";
            m_upgradeBuilding = null;

            if (m_item.building.upgradeName != null && m_item.level < m_item.maxLevel)
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

        public void ShowDropDown()
        {
            Category category = m_item.category;
            if (category == Category.None && m_item.building.baseName != null)
            {
                BuildingItem item = UIThemeManager.instance.GetBuildingItem(m_item.building.baseName);
                if (item != null) category = item.category;
            }

            FastList<object> list = UIThemeManager.instance.GetBuildingsFiltered(category, m_item.level + 1, m_upgradeName.text);

            if (m_dropDownList == null)
            {
                m_dropDownList = UIFastList.Create<UIDropDownItem>(GetRootContainer());
                m_dropDownList.width = m_upgradeName.width;
                m_dropDownList.rowHeight = 30;
                m_dropDownList.autoHideScrollbar = true;
                m_dropDownList.canSelect = true;
                m_dropDownList.selectOnMouseEnter = true;
                m_dropDownList.canFocus = true;
                m_dropDownList.backgroundSprite = "GenericPanelLight";
                m_dropDownList.backgroundColor = new Color32(45, 52, 61, 255);
                m_dropDownList.absolutePosition = m_upgradeName.absolutePosition + new Vector3(0, m_upgradeName.height);
            }

            m_dropDownList.height = Mathf.Min(list.m_size * 30, 150);
            m_dropDownList.rowsData = list;
            m_dropDownList.isVisible = list.m_size > 0;
            if (m_dropDownList.isVisible)
                m_dropDownList.selectedIndex = 0;
            else
                m_dropDownList.selectedIndex = -1;
        }

        public void HideDropDown()
        {
            if (m_dropDownList != null)
            {
                m_dropDownList.isVisible = false;
                m_dropDownList.selectedIndex = -1;
            }
        }
    }

    public class UIDropDownItem: UIPanel, IUIFastListRow
    {
        private UILabel m_name;
        private UILabel m_size;

        private BuildingItem m_building;

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            if (m_name == null) return;

            m_size.relativePosition = new Vector3(width - 35f, 5);
        }

        private void SetupControls()
        {
            if (m_name != null) return;

            isVisible = true;
            isInteractive = true;
            width = parent.width;
            height = 30;

            m_name = AddUIComponent<UILabel>();
            m_name.relativePosition = new Vector3(5, 5);
            m_name.textColor = new Color32(170, 170, 170, 255);

            m_size = AddUIComponent<UILabel>();
            m_size.width = 30;
            m_size.textAlignment = UIHorizontalAlignment.Center;
            m_size.textColor = new Color32(170, 170, 170, 255);
        }

        protected override void OnMouseDown(UIMouseEventParameter p)
        {
            p.Use();
            UIThemeManager.instance.ChangeUpgradeBuilding(m_building);

            base.OnMouseDown(p);
        }

        protected override void OnMouseEnter(UIMouseEventParameter p)
        {
            base.OnMouseEnter(p);
            UIThemeManager.instance.buildingPreview.Show(m_building);
        }


        protected override void OnMouseLeave(UIMouseEventParameter p)
        {
            base.OnMouseLeave(p);
            UIThemeManager.instance.buildingPreview.Show(UIThemeManager.instance.selectedBuilding);
        }

        #region IUIFastListRow implementation
        public void Display(object data, bool isRowOdd)
        {
            SetupControls();

            m_building = data as BuildingItem;
            m_name.text = m_building.displayName;

            UIUtils.TruncateLabel(m_name, width - 40);
            m_size.text = m_building.size;

            backgroundSprite = null;
        }

        public void Select(bool isRowOdd)
        {
            backgroundSprite = "ListItemHighlight";
            color = new Color32(255, 255, 255, 255);
        }

        public void Deselect(bool isRowOdd)
        {
            backgroundSprite = null;
        }
        #endregion
    }

}
