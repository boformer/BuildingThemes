using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BuildingThemes.GUI
{
    public enum Origin
    {
        All,
        Default,
        Custom,
        Cloned
    }

    public enum Status
    {
        All,
        Included,
        Excluded
    }

    public enum Category
    {
        None = -1,
        ResidentialLow = 0,
        ResidentialHigh,
        CommercialLow,
        CommercialHigh,
        CommercialLeisure,
        CommercialTourism,
        Industrial,
        Farming,
        Forestry,
        Oil,
        Ore,
        Office
    }

    public class CategoryIcons
    {

        public static readonly string[] atlases = { "Thumbnails", "Thumbnails", "Thumbnails", "Thumbnails", "Ingame", "Ingame", "Thumbnails",
                                                       "Ingame", "Ingame", "Ingame", "Ingame", "Thumbnails" };

        public static readonly string[] spriteNames = { "ZoningResidentialLow", "ZoningResidentialHigh", "ZoningCommercialLow", "ZoningCommercialHigh",
                                                        "IconPolicyLeisure", "IconPolicyTourist",
                                                        "ZoningIndustrial", "IconPolicyFarming", "IconPolicyForest", "IconPolicyOil", "IconPolicyOre",
                                                        "ZoningOffice" };

        public static readonly string[] tooltips = {
            "Low density residential",
            "High density residential",
            "Low density commercial",
            "High density commercial",
            "Leisure commercial",
            "Tourism commercial",
            "Generic Industry",
            "Farming Industry",
            "Forest Industry",
            "Oil Industry",
            "Ore Industry",
            "Office" };
    }
}
