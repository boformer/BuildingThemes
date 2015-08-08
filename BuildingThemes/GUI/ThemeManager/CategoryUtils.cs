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
        Industrial,
        Farming,
        Forestry,
        Oil,
        Ore,
        Office
    }

    public class CategoryIcons
    {

        public static readonly string[] atlases = { "Thumbnails", "Thumbnails", "Thumbnails", "Thumbnails", "Thumbnails",
                                                       "Ingame", "Ingame", "Ingame", "Ingame", "Thumbnails" };

        public static readonly string[] spriteNames = { "ZoningResidentialLow", "ZoningResidentialHigh", "ZoningCommercialLow", "ZoningCommercialHigh", "ZoningIndustrial",
                                                      "IconPolicyFarming", "IconPolicyForest", "IconPolicyOil", "IconPolicyOre", "ZoningOffice" };

        public static readonly string[] tooltips = {
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
    }
}
