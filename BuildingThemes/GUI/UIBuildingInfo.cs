using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace BuildingThemes.GUI
{
    public class UIBuildingInfo : UIPanel
    {
        private UILabel m_noInfo;
        private UILabel m_category;
        private UILabel m_level;
        private UILabel m_size;

        /*private UICheckBox m_included;

        private UILabel m_spawnRateLabel;
        private UITextField m_spawnRate;

        private UILabel m_baseName;

        private UILabel m_upgradeNameLabel;
        private UITextField m_upgradeName;*/

        private BuildingItem m_item;

        private static readonly string[] _categoryNames = {
            "Low density residential",
            "High density residential",
            "Low density commercial",
            "High density commercial",
            "Generic Industry",
            "Farming Industry",
            "Forest Industry",
            "Oil Industry",
            "Ore Industry",
            "Office" };

        public override void Start()
        {
            base.Start();

            isVisible = true;
            canFocus = true;
            isInteractive = true;
            backgroundSprite = "UnlockingPanel";
            padding = new RectOffset(10, 10, 5, 10);

            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            autoLayoutPadding.top = 5;

            SetupControls();
        }

        private void SetupControls()
        {
            if (m_noInfo != null) return;

            // No information
            m_noInfo = AddUIComponent<UILabel>();
            m_noInfo.padding = new RectOffset(0, 0, 5, 0);
            m_noInfo.textScale = 0.9f;
            m_noInfo.text = "No information";

            // Category
            m_category = AddUIComponent<UILabel>();
            m_category.padding = new RectOffset(0, 0, 5, 0);
            m_category.textScale = 0.9f;
            m_category.text = "Category";
            m_category.isVisible = false;

            // Level
            m_level = AddUIComponent<UILabel>();
            m_level.padding = new RectOffset(0, 0, 5, 0);
            m_level.textScale = 0.9f;
            m_level.text = "Level";
            m_level.isVisible = false;

            // Size
            m_size = AddUIComponent<UILabel>();
            m_size.padding = new RectOffset(0, 0, 5, 0);
            m_size.textScale = 0.9f;
            m_size.text = "Size";
            m_size.isVisible = false;
        }

        public void Show(BuildingItem item)
        {
            m_item = item;

            if(item == null)
            {
                m_noInfo.isVisible = true;
                m_category.isVisible = false;
                m_level.isVisible = false;
                m_size.isVisible = false;
            }
            else
            {
                m_noInfo.isVisible = false;

                Category category = m_item.category;
                if(category != Category.None)
                {
                    m_category.text = _categoryNames[(int)category];
                    m_category.isVisible = true;
                }
                else
                {
                    m_category.isVisible = false;
                }

                if (!item.level.IsNullOrWhiteSpace())
                {
                    m_level.text = item.level.Replace("L", "Level ");
                    m_level.isVisible = true;
                }
                else
                {
                    m_level.isVisible = false;
                }

                if (!item.size.IsNullOrWhiteSpace())
                {
                    m_size.text = item.size.Replace("L", "Level ");
                    m_size.isVisible = true;
                }
                else
                {
                    m_size.isVisible = false;
                }
            }

        }
    }
}
