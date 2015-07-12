using UnityEngine;
using ColossalFramework.UI;

namespace BuildingThemes.GUI
{
    public class UIBuildingFilter : UIPanel
    {
        public UICheckBox[] zoningToggles;
        public UIDropDown levelFilter;
        public UIDropDown sizeFilter;
        public UITextField nameFilter;

        public enum Zones
        {
            ResidentialLow = 0,
            ResidentialHigh,
            CommercialLow,
            CommercialHigh,
            Industrial,
            Farming,
            Forestry,
            Oil,
            Ore,
            Office
        }

        public enum Levels
        {
            All = 0,
            L1,
            L2,
            L3,
            L4,
            L5
        }

        public enum Sizes
        {
            All = 0,
            S1x1,
            S2x2,
            S2x3,
            S3x2,
            S3x3,
            S3x4,
            S4x3,
            S4x4
        }

        public override void Start()
        {
            base.Start();

            // Zoning
            zoningToggles = new UICheckBox[10];
            zoningToggles[(int)Zones.ResidentialLow]    = UIUtils.CreateIconToggle(this, "Thumbnails", "ZoningResidentialLow", "ZoningResidentialLowDisabled");
            zoningToggles[(int)Zones.ResidentialHigh]   = UIUtils.CreateIconToggle(this, "Thumbnails", "ZoningResidentialHigh", "ZoningResidentialHighDisabled");
            zoningToggles[(int)Zones.CommercialLow]     = UIUtils.CreateIconToggle(this, "Thumbnails", "ZoningCommercialLow", "ZoningCommercialLowDisabled");
            zoningToggles[(int)Zones.CommercialHigh]    = UIUtils.CreateIconToggle(this, "Thumbnails", "ZoningCommercialHigh", "ZoningCommercialHighDisabled");
            zoningToggles[(int)Zones.Industrial]        = UIUtils.CreateIconToggle(this, "Thumbnails", "ZoningIndustrial", "ZoningIndustrialDisabled");
            zoningToggles[(int)Zones.Farming]           = UIUtils.CreateIconToggle(this, "Ingame", "IconPolicyFarming", "IconPolicyFarmingDisabled");
            zoningToggles[(int)Zones.Forestry]          = UIUtils.CreateIconToggle(this, "Ingame", "IconPolicyForest", "IconPolicyForestDisabled");
            zoningToggles[(int)Zones.Oil]               = UIUtils.CreateIconToggle(this, "Ingame", "IconPolicyOil", "IconPolicyOilDisabled");
            zoningToggles[(int)Zones.Ore]               = UIUtils.CreateIconToggle(this, "Ingame", "IconPolicyOre", "IconPolicyOreDisabled");
            zoningToggles[(int)Zones.Office]            = UIUtils.CreateIconToggle(this, "Thumbnails", "ZoningOffice", "ZoningOfficeDisabled");

            for (int i = 0; i < 10; i++)
            {
                zoningToggles[i].relativePosition = new Vector3(40 * i, 0);
                zoningToggles[i].isChecked = true;
            }

            // Level
            UILabel levelLabel = AddUIComponent<UILabel>();
            levelLabel.textScale = 0.8f;
            levelLabel.padding = new RectOffset(0, 0, 8, 0);
            levelLabel.relativePosition = new Vector3(405, 5);
            levelLabel.text = "Level: ";

            levelFilter = UIUtils.CreateDropDown(this);
            levelFilter.width = 60;
            levelFilter.AddItem("All");
            levelFilter.AddItem("1");
            levelFilter.AddItem("2");
            levelFilter.AddItem("3");
            levelFilter.AddItem("4");
            levelFilter.AddItem("5");
            levelFilter.selectedIndex = 0;
            levelFilter.relativePosition = new Vector3(445, 5);

            // Size
            UILabel sizeLabel = AddUIComponent<UILabel>();
            sizeLabel.textScale = 0.8f;
            sizeLabel.padding = new RectOffset(0, 0, 8, 0);
            sizeLabel.relativePosition = new Vector3(515, 5);
            sizeLabel.text = "Size: ";

            sizeFilter = UIUtils.CreateDropDown(this);
            sizeFilter.width = 60;
            sizeFilter.AddItem("All");
            sizeFilter.AddItem("1x1");
            sizeFilter.AddItem("2x2");
            sizeFilter.AddItem("2x3");
            sizeFilter.AddItem("3x2");
            sizeFilter.AddItem("3x3");
            sizeFilter.AddItem("3x4");
            sizeFilter.AddItem("4x3");
            sizeFilter.AddItem("4x4");
            sizeFilter.selectedIndex = 0;
            sizeFilter.relativePosition = new Vector3(550, 5);

            // Name filter
            UILabel nameLabel = AddUIComponent<UILabel>();
            nameLabel.textScale = 0.8f;
            nameLabel.padding = new RectOffset(0, 0, 8, 0);
            nameLabel.relativePosition = new Vector3(620, 5);
            nameLabel.text = "Name: ";

            nameFilter = UIUtils.CreateTextField(this);
            nameFilter.width = width - 665;
            nameFilter.height = 30;
            nameFilter.padding = new RectOffset(6, 6, 6, 6);
            nameFilter.relativePosition = new Vector3(width - nameFilter.width, 5);
        }
    }
}
