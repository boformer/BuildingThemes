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
        ResidentialEco, // gc
        CommercialLow,
        CommercialHigh,
        CommercialLeisure,
        CommercialTourism,
        CommercialEco, // gc
        Industrial,
        Farming,
        Forestry,
        Oil,
        Ore,
        Office,
        OfficeHightech, // gc
        ResidentialWallToWall, //pp
        CommercialWallToWall, //pp
        OfficeWallToWall, //pp
        OfficeFinancial //fd
    }

    public class CategoryIcons
    {

        public static readonly string[] atlases = {
            "Thumbnails",
            "Thumbnails",
            "Thumbnails", // gc
            "Thumbnails",
            "Thumbnails",
            "Thumbnails",
            "Thumbnails",
            "Thumbnails", // gc
            "Thumbnails",
            "Ingame",
            "Ingame",
            "Ingame",
            "Ingame",
            "Thumbnails",
            "Thumbnails", //gc
            "Thumbnails",
            "Thumbnails",
            "Thumbnails",
            "Thumbnails"
        };

        public static readonly string[] spriteNames = {
            "ZoningResidentialLow",
            "ZoningResidentialHigh",
            "DistrictSpecializationSelfsufficient",
            "ZoningCommercialLow",
            "ZoningCommercialHigh",
            "DistrictSpecializationLeisure",
            "DistrictSpecializationTourist",
            "DistrictSpecializationOrganic",
            "ZoningIndustrial",
            "IconPolicyFarming",
            "IconPolicyForest",
            "IconPolicyOil",
            "IconPolicyOre",
            "ZoningOffice",
            "DistrictSpecializationHightech",
            "DistrictSpecializationResidentialWallToWall",
            "DistrictSpecializationCommercialWallToWall",
            "DistrictSpecializationOfficeWallToWall",
            "DistrictSpecializationFinancial"
        };

        public static readonly string[] tooltips = {
            "Low density residential",
            "High density residential",
            "Self-sufficient residential",
            "Low density commercial",
            "High density commercial",
            "Leisure commercial",
            "Tourism commercial",
            "Organic & local produce commercial",
            "Generic Industry",
            "Farming Industry",
            "Forest Industry",
            "Oil Industry",
            "Ore Industry",
            "Office",
            "IT office",
            "Wall-to-Wall residential",
            "Wall-to-Wall commercial",
            "Wall-to-Wall office",
            "Financial Office"
        };
    }
}
