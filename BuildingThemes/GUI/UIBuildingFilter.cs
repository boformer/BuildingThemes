using UnityEngine;
using ColossalFramework.UI;

using System.Reflection;

namespace BuildingThemes.GUI
{
    public class UIBuildingFilter : UIPanel
    {
        public UICheckBox[] zoningToggles;
        public UIButton allZones;
        public UIButton noZones;
        public UIDropDown levelFilter;
        public UIDropDown sizeFilterX;
        public UIDropDown sizeFilterY;
        public UITextField nameFilter;

        public enum Zone
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

        public bool IsZoneSelected(Zone zone)
        {
            return zoningToggles[(int)zone].isChecked;
        }

        public bool IsAllZoneSelected()
        {
            return zoningToggles[(int)Zone.ResidentialLow].isChecked &&
                zoningToggles[(int)Zone.ResidentialHigh].isChecked &&
                zoningToggles[(int)Zone.CommercialLow].isChecked &&
                zoningToggles[(int)Zone.CommercialHigh].isChecked &&
                zoningToggles[(int)Zone.Industrial].isChecked &&
                zoningToggles[(int)Zone.Farming].isChecked &&
                zoningToggles[(int)Zone.Forestry].isChecked &&
                zoningToggles[(int)Zone.Oil].isChecked &&
                zoningToggles[(int)Zone.Ore].isChecked &&
                zoningToggles[(int)Zone.Office].isChecked;
        }

        public ItemClass.Level buildingLevel
        {
            get { return (ItemClass.Level)(levelFilter.selectedIndex - 1); }
        }

        public Vector2 buildingSize
        {
            get
            {
                if (sizeFilterX.selectedIndex == 0) return Vector2.zero;
                return new Vector2(sizeFilterX.selectedIndex, sizeFilterY.selectedIndex + 1);
            }
        }

        public string buildingName
        {
            get { return nameFilter.text.Trim(); }
        }

        public event PropertyChangedEventHandler<int> eventFilteringChanged;

        public override void Start()
        {
            base.Start();

            // Zoning
            zoningToggles = new UICheckBox[10];
            zoningToggles[(int)Zone.ResidentialLow] = UIUtils.CreateIconToggle(this, "Thumbnails", "ZoningResidentialLow", "ZoningResidentialLowDisabled");
            zoningToggles[(int)Zone.ResidentialHigh] = UIUtils.CreateIconToggle(this, "Thumbnails", "ZoningResidentialHigh", "ZoningResidentialHighDisabled");
            zoningToggles[(int)Zone.CommercialLow] = UIUtils.CreateIconToggle(this, "Thumbnails", "ZoningCommercialLow", "ZoningCommercialLowDisabled");
            zoningToggles[(int)Zone.CommercialHigh] = UIUtils.CreateIconToggle(this, "Thumbnails", "ZoningCommercialHigh", "ZoningCommercialHighDisabled");
            zoningToggles[(int)Zone.Industrial] = UIUtils.CreateIconToggle(this, "Thumbnails", "ZoningIndustrial", "ZoningIndustrialDisabled");
            zoningToggles[(int)Zone.Farming] = UIUtils.CreateIconToggle(this, "Ingame", "IconPolicyFarming", "IconPolicyFarmingDisabled");
            zoningToggles[(int)Zone.Forestry] = UIUtils.CreateIconToggle(this, "Ingame", "IconPolicyForest", "IconPolicyForestDisabled");
            zoningToggles[(int)Zone.Oil] = UIUtils.CreateIconToggle(this, "Ingame", "IconPolicyOil", "IconPolicyOilDisabled");
            zoningToggles[(int)Zone.Ore] = UIUtils.CreateIconToggle(this, "Ingame", "IconPolicyOre", "IconPolicyOreDisabled");
            zoningToggles[(int)Zone.Office] = UIUtils.CreateIconToggle(this, "Thumbnails", "ZoningOffice", "ZoningOfficeDisabled");

            for (int i = 0; i < 10; i++)
            {
                zoningToggles[i].relativePosition = new Vector3(40 * i, 35);
                zoningToggles[i].isChecked = true;
                zoningToggles[i].readOnly = true;
                zoningToggles[i].checkedBoxObject.isInteractive = false; // Don't eat my double click event please

                zoningToggles[i].eventClick += (c, p) =>
                {
                    ((UICheckBox)c).isChecked = !((UICheckBox)c).isChecked;
                    eventFilteringChanged(this, 0);
                };

                zoningToggles[i].eventDoubleClick += (c, p) =>
                {
                    for (int j = 0; j < 10; j++)
                        zoningToggles[j].isChecked = false;
                    ((UICheckBox)c).isChecked = true;

                    eventFilteringChanged(this, 0);
                };
            }

            allZones = UIUtils.CreateButton(this);
            allZones.width = 55;
            allZones.text = "All";
            allZones.relativePosition = new Vector3(400, 40);

            allZones.eventClick += (c, p) =>
            {
                for (int i = 0; i < 10; i++)
                {
                    zoningToggles[i].isChecked = true;
                }
                eventFilteringChanged(this, 0);
            };

            noZones = UIUtils.CreateButton(this);
            noZones.width = 55;
            noZones.text = "None";
            noZones.relativePosition = new Vector3(460, 40);

            noZones.eventClick += (c, p) =>
            {
                for (int i = 0; i < 10; i++)
                {
                    zoningToggles[i].isChecked = false;
                }
                eventFilteringChanged(this, 0);
            };

            // Level
            UILabel levelLabel = AddUIComponent<UILabel>();
            levelLabel.textScale = 0.8f;
            levelLabel.padding = new RectOffset(0, 0, 8, 0);
            levelLabel.text = "Level: ";
            levelLabel.relativePosition = new Vector3(0, 0);

            levelFilter = UIUtils.CreateDropDown(this);
            levelFilter.width = 55;
            levelFilter.AddItem("All");
            levelFilter.AddItem("1");
            levelFilter.AddItem("2");
            levelFilter.AddItem("3");
            levelFilter.AddItem("4");
            levelFilter.AddItem("5");
            levelFilter.selectedIndex = 0;
            levelFilter.relativePosition = new Vector3(40, 0);

            levelFilter.eventSelectedIndexChanged += (c, i) => eventFilteringChanged(this, 1);

            // Size
            UILabel sizeLabel = AddUIComponent<UILabel>();
            sizeLabel.textScale = 0.8f;
            sizeLabel.padding = new RectOffset(0, 0, 8, 0);
            sizeLabel.text = "Size: ";
            sizeLabel.relativePosition = new Vector3(105, 0);

            sizeFilterX = UIUtils.CreateDropDown(this);
            sizeFilterX.width = 55;
            sizeFilterX.AddItem("All");
            sizeFilterX.AddItem("1");
            sizeFilterX.AddItem("2");
            sizeFilterX.AddItem("3");
            sizeFilterX.AddItem("4");
            sizeFilterX.selectedIndex = 0;
            sizeFilterX.relativePosition = new Vector3(140, 0);

            UILabel XLabel = AddUIComponent<UILabel>();
            XLabel.textScale = 0.8f;
            XLabel.padding = new RectOffset(0, 0, 8, 0);
            XLabel.text = "X";
            XLabel.isVisible = false;
            XLabel.relativePosition = new Vector3(190, 5);

            sizeFilterY = UIUtils.CreateDropDown(this);
            sizeFilterY.width = 45;
            sizeFilterY.AddItem("1");
            sizeFilterY.AddItem("2");
            sizeFilterY.AddItem("3");
            sizeFilterY.AddItem("4");
            sizeFilterY.selectedIndex = 0;
            sizeFilterY.isVisible = false;
            sizeFilterY.relativePosition = new Vector3(205, 0);

            sizeFilterX.eventSelectedIndexChanged += (c, i) =>
            {
                if (i == 0)
                {
                    sizeFilterX.width = 55;
                    XLabel.isVisible = false;
                    sizeFilterY.isVisible = false;
                }
                else
                {
                    sizeFilterX.width = 45;
                    XLabel.isVisible = true;
                    sizeFilterY.isVisible = true;
                }

                eventFilteringChanged(this, 2);
            };

            sizeFilterY.eventSelectedIndexChanged += (c, i) => eventFilteringChanged(this, 2);

            // Name filter
            UILabel nameLabel = AddUIComponent<UILabel>();
            nameLabel.textScale = 0.8f;
            nameLabel.padding = new RectOffset(0, 0, 8, 0);
            nameLabel.relativePosition = new Vector3(width - 250, 0);
            nameLabel.text = "Name: ";

            nameFilter = UIUtils.CreateTextField(this);
            nameFilter.width = 200;
            nameFilter.height = 30;
            nameFilter.padding = new RectOffset(6, 6, 6, 6);
            nameFilter.relativePosition = new Vector3(width - nameFilter.width, 0);

            nameFilter.eventTextChanged += (c, s) => eventFilteringChanged(this, 3);
        }
    }
}
